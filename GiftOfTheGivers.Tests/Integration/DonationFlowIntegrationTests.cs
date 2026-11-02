using System.Net;
using Part1.Controllers;

namespace GiftOfTheGivers.Tests.Integration;

[Trait("Category", "Integration")]
public class DonationFlowIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    public DonationFlowIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;
    }

    // A.2: donation flow  form > Azure Function > stored donation
    [Fact]
    public async Task DonationForm_CallsFunctionAndStoresDonationWithCertificate()
    {
        var client = _factory.CreateClient();
        var donor = $"Integration Donor {Guid.NewGuid():N}";
        var callsBefore = _factory.FunctionHandler.Calls;

        var response = await client.PostAsync("/Donor/DonateMoney", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["DonationAmount"] = "300",
            ["Currency"] = "ZAR",
            ["Email"] = "donor@example.com",
            ["DonorName"] = donor
        }));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(_factory.FunctionHandler.Calls > callsBefore, "The Azure Function was not called.");
        Assert.Contains("ZAR 300.00", html);
        Assert.Matches(@"GOTG-\d{8}-[0-9A-F]{6}", html);   // number issued by the function, not the TX- fallback

        var saved = DonorController.allDonations.Single(d => d.DonorName == donor);
        Assert.Equal(300m, saved.DonationAmount);
        Assert.StartsWith("GOTG-", saved.CertificateNumber);
    }

    [Fact]
    public async Task AnonymousDonation_IsAcceptedAndShownAsAnonymous()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/Donor/DonateMoney", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["DonationAmount"] = "50",
            ["Currency"] = "USD",
            ["Email"] = "anon@example.com"
        }));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Anonymous", html);
        Assert.Contains("USD 50.00", html);
    }

    [Fact]
    public async Task InvalidDonation_IsRejectedAndNotStored()
    {
        var client = _factory.CreateClient();
        var storedBefore = DonorController.allDonations.Count;

        var response = await client.PostAsync("/Donor/DonateMoney", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["DonationAmount"] = "-10",
            ["Currency"] = "ZAR",
            ["Email"] = "bad@example.com",
            ["DonorName"] = "Bad Amount"
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);   // the form is shown again
        Assert.Equal(storedBefore, DonorController.allDonations.Count);
    }

    [Fact]
    public async Task EmployeeRegisterAndLogin_RedirectsToDashboard()
    {
        var client = _factory.CreateNoRedirectClient();
        var username = $"emp{Guid.NewGuid():N}";

        var register = await client.PostAsync("/Register/RegisterUser", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Fullname"] = "Test Employee",
            ["Username"] = username,
            ["Email"] = "emp@example.com",
            ["Password"] = "P@ssw0rd!",
            ["role"] = "Employee"
        }));
        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);

        var login = await client.PostAsync("/Register/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = username,
            ["Password"] = "P@ssw0rd!"
        }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Contains("/Employee/Dashboard", login.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Login_WithWrongPassword_StaysOnLoginPage()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/Register/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "nobody",
            ["Password"] = "wrong"
        }));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", html);
    }
}
