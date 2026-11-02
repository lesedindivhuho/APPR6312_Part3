# Gift of the Givers

<!-- Replace with the markdown from Pipelines > GiftoftheGivers > ... > Status badge once the pipeline has been created -->
[![Build Status](https://dev.azure.com/GiftOfTheGiversTeam/Gift%20of%20the%20Givers%20Relief%20Management%20System/_apis/build/status/GiftoftheGivers?branchName=master)](https://dev.azure.com/GiftOfTheGiversTeam/Gift%20of%20the%20Givers%20Relief%20Management%20System/_build)

ASP.NET Core MVC web app (`Part1`) with an Azure Functions app (`GiftOfTheGivers.Functions`) in the same solution (`Part1.sln`).

## Projects

| Project | Description |
| --- | --- |
| `Part1` | MVC web app (donations, volunteers, employee dashboard) |
| `GiftOfTheGivers.Functions` | Azure Functions (.NET 8 isolated worker, Functions v4) |
| `GiftOfTheGivers.Helpers` | Shared helper library, published as a NuGet package to Azure Artifacts |
| `GiftOfTheGivers.Tests` | xUnit tests for the helpers and the `GenerateTaxCertificate` function |

## GiftOfTheGivers.Helpers package

The web app and the Functions app both use the helpers through a `PackageReference`, not a project reference:

- `DonationValidator` - the donation rules (amount, currency, donation type, donor name) used by the function and by `DonorController.DonateMoney`.
- `CertificateNumberGenerator` - certificate numbers in the format `GOTG-yyyyMMdd-XXXXXX`.
- `AmountFormatter` - amounts for display, for example `ZAR 300.00` on the tax certificate page.

Feed: `GiftOfTheGiversHelpers` (project-scoped Azure Artifacts feed, with nuget.org as an upstream source). `nuget.config` adds the feed and maps `GiftOfTheGivers.*` packages to it, so everything else still comes from nuget.org.

Versioning follows semantic versioning. The version is set in `GiftOfTheGivers.Helpers.csproj` and each version is published once:

| Version | Change |
| --- | --- |
| 1.0.0 | `DonationValidator` and `CertificateNumberGenerator` moved out of the function |
| 1.1.0 | Added `AmountFormatter` (new feature, nothing removed, so a minor version) |

To publish a new version, bump `<Version>`, then run `dotnet pack -c Release` and `dotnet nuget push` to the feed, and update the `PackageReference` in `Part1` and `GiftOfTheGivers.Functions`. Restoring from the feed needs Azure DevOps credentials, for example Visual Studio signed in to the org, or the Azure Artifacts Credential Provider with `dotnet restore --interactive`.

## Tests and CI pipeline

Run the tests with `dotnet test`. `azure-pipelines.yml` runs on every push to `master`:

1. **Build** - restore (with `NuGetAuthenticate` for the feed), build and run the tests, then publish the test results and code coverage.
2. **Package** - publish the web app and Functions app and pack the Helpers library as build artifacts.
3. **SmokeTest** - call the deployed `GenerateTaxCertificate` function.

The shared restore steps live in `pipelines/templates/restore-steps.yml`. The smoke test needs a secret pipeline variable named `FunctionKey` (Edit > Variables > Keep this value secret). It is never stored in the repo.

## Azure Functions

| Function | Trigger | Purpose |
| --- | --- | --- |
| `GenerateTaxCertificate` | HTTP GET / POST | Validates a money donation and issues a tax certificate number (`GOTG-yyyyMMdd-XXXXXX`) |
| `LogProjectUpdate` | HTTP POST | Saves an employee project update to Azure Table Storage (`ProjectUpdates` table) |

The web app calls these functions through a named `HttpClient` (see `Part1/Services/FunctionsClient.cs`):

- When a donor submits the money donation form, `DonorController.DonateMoney` calls `GenerateTaxCertificate` and shows the returned certificate number on the Tax Certificate page.
- When an employee posts a project update on the dashboard, `EmployeeController.PostUpdate` sends it to `LogProjectUpdate`.

If the Functions app is not running (or storage is down), the website still works. The donor gets a temporary `TX-...` reference with a notice, and the project update is still posted on the dashboard.

The base URL is set in `Part1/appsettings.json`:

```json
"AzureFunctions": {
  "BaseUrl": "http://localhost:7071/api",
  "FunctionKey": ""
}
```

`FunctionKey` is only needed once the functions are deployed to Azure (keys are not checked when running locally).

## Running locally

Requirements: .NET 8 SDK/runtime, [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (`npm install -g azure-functions-core-tools@4`) and, for `LogProjectUpdate` only, [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (`npm install -g azurite`).

1. Create the local settings file for the Functions app (it is git-ignored):

   ```powershell
   Copy-Item GiftOfTheGivers.Functions\local.settings.example.json GiftOfTheGivers.Functions\local.settings.json
   ```

2. (Optional) Start Azurite for table storage:

   ```powershell
   azurite --silent --location $env:TEMP\azurite
   ```

3. Start the Functions app (listens on http://localhost:7071):

   ```powershell
   cd GiftOfTheGivers.Functions
   func start
   ```

   Core Tools 4.15+ prints a note suggesting `dotnet run` for isolated projects. `func start` was used for testing and works fine with this project.

4. In another terminal start the web app:

   ```powershell
   dotnet run --project Part1 --launch-profile http
   ```

   Then open http://localhost:5171/Donor/Donate, submit a money donation and check the certificate number.

In Visual Studio you can also set both projects as startup projects (Solution > Configure Startup Projects > Multiple startup projects).

## Sample requests (Postman / browser)

### GenerateTaxCertificate

Browser (GET):

```
http://localhost:7071/api/GenerateTaxCertificate?donorName=Thabo%20Mokoena&amount=500&currency=ZAR&donationType=once-off
http://localhost:7071/api/GenerateTaxCertificate?amount=250&currency=USD&donationType=recurring&anonymous=true
http://localhost:7071/api/GenerateTaxCertificate?donorName=Thabo&amount=-50&currency=GBP&donationType=weekly
```

Postman (POST `http://localhost:7071/api/GenerateTaxCertificate`, Body > raw > JSON):

```json
{
  "donorName": "Thabo Mokoena",
  "amount": 1500.50,
  "currency": "ZAR",
  "donationType": "recurring",
  "anonymous": false
}
```

Successful response (`200 OK`):

```json
{
  "certificateNumber": "GOTG-20260924-751360",
  "donorName": "Thabo Mokoena",
  "amount": 1500.50,
  "currency": "ZAR",
  "donationType": "recurring",
  "issuedDate": "2026-09-24T22:04:44.0553443Z"
}
```

Invalid input returns `400 Bad Request` with the reasons, for example:

```json
{
  "message": "Invalid certificate request.",
  "errors": [
    "Amount must be greater than zero.",
    "Currency 'GBP' is not supported. Use ZAR, USD or EUR.",
    "Donation type 'weekly' is not supported. Use once-off or recurring."
  ]
}
```

Validation rules: amount must be between 0.01 and 1,000,000; currency must be ZAR, USD or EUR; donation type must be `once-off` or `recurring`; donor name is required unless `anonymous` is `true`.

### LogProjectUpdate

POST `http://localhost:7071/api/LogProjectUpdate` (Body > raw > JSON):

```json
{
  "title": "Flood relief in KZN",
  "message": "Food parcels delivered to 200 families in Durban.",
  "postedBy": "Employee"
}
```

Returns `201 Created` when saved, `400 Bad Request` if the title or message is missing, and `503 Service Unavailable` if table storage (Azurite) is not running.
