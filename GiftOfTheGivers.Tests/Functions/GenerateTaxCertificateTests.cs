using System.Text;
using System.Text.RegularExpressions;
using GiftOfTheGivers.Functions.Functions;
using GiftOfTheGivers.Functions.Models;
using GiftOfTheGivers.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Tests.Functions;

public class GenerateTaxCertificateTests
{
    private readonly ListLogger<GenerateTaxCertificate> _logger = new();

    [Fact]
    public async Task Get_ValidDonation_ReturnsCertificate()
    {
        var request = CreateGet("?amount=500&currency=ZAR&donorName=Test%20Donor&donationType=once-off");

        var result = await new GenerateTaxCertificate(_logger).Run(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var certificate = Assert.IsType<TaxCertificateResponse>(ok.Value);
        Assert.Matches(new Regex($"^GOTG-{certificate.IssuedDate:yyyyMMdd}-[0-9A-F]{{6}}$"), certificate.CertificateNumber);
        Assert.Equal(DateTimeKind.Utc, certificate.IssuedDate.Kind);
        Assert.Equal("Test Donor", certificate.DonorName);
        Assert.Equal(500m, certificate.Amount);
        Assert.Equal("ZAR", certificate.Currency);
        Assert.Equal("once-off", certificate.DonationType);
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("ZAR 500.00"));
    }

    [Fact]
    public async Task Get_NegativeAmount_ReturnsBadRequestAndLogsWarning()
    {
        var request = CreateGet("?amount=-5&currency=ZAR&donorName=Test%20Donor&donationType=once-off");

        var result = await new GenerateTaxCertificate(_logger).Run(request);

        Assert.Equal(new[] { "Amount must be greater than zero." }, GetErrors(result));
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("rejected"));
    }

    [Fact]
    public async Task Get_AmountNotANumber_ReturnsBadRequest()
    {
        var request = CreateGet("?amount=abc&currency=ZAR&donorName=Test&donationType=once-off");

        var result = await new GenerateTaxCertificate(_logger).Run(request);

        Assert.Equal(new[] { "Amount 'abc' is not a valid number." }, GetErrors(result));
    }

    [Fact]
    public async Task Post_AnonymousRecurringDonation_ReturnsAnonymousCertificate()
    {
        var request = CreatePost("""{ "amount": 250, "currency": "usd", "donationType": "Recurring", "anonymous": true }""");

        var result = await new GenerateTaxCertificate(_logger).Run(request);

        var certificate = Assert.IsType<TaxCertificateResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal("Anonymous", certificate.DonorName);
        Assert.Equal("USD", certificate.Currency);
        Assert.Equal("recurring", certificate.DonationType);
    }

    [Fact]
    public async Task Post_EmptyBody_ReturnsBadRequest()
    {
        var request = CreatePost("");

        var result = await new GenerateTaxCertificate(_logger).Run(request);

        var error = Assert.Single(GetErrors(result));
        Assert.StartsWith("Request body is empty.", error);
    }

    private static HttpRequest CreateGet(string queryString)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.QueryString = new QueryString(queryString);
        return context.Request;
    }

    private static HttpRequest CreatePost(string json)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return context.Request;
    }

    // The function returns an anonymous { message, errors } object for bad requests
    private static List<string> GetErrors(IActionResult result)
    {
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = badRequest.Value!.GetType().GetProperty("errors")!.GetValue(badRequest.Value);
        return Assert.IsType<List<string>>(errors);
    }
}
