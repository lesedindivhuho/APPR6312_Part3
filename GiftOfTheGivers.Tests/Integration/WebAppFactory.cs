using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Part1.Services;

namespace GiftOfTheGivers.Tests.Integration;

// Starts the real MVC app in memory, with the Azure Function call routed to InProcessFunctionHandler
public class WebAppFactory : WebApplicationFactory<Program>
{
    public InProcessFunctionHandler FunctionHandler { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddHttpClient(FunctionsClient.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => FunctionHandler);
        });
    }

    public HttpClient CreateNoRedirectClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
