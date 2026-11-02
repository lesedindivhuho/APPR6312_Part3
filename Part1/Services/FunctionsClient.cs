using System.Text.Json;
using Part1.Models;

namespace Part1.Services
{
    // Calls the GiftOfTheGivers.Functions app. Every call fails soft so the website keeps working
    // when the Functions host (or its storage) is not running.
    public class FunctionsClient
    {
        public const string HttpClientName = "AzureFunctions";

        private readonly HttpClient _http;
        private readonly ILogger<FunctionsClient> _logger;

        public FunctionsClient(IHttpClientFactory httpClientFactory, ILogger<FunctionsClient> logger)
        {
            _http = httpClientFactory.CreateClient(HttpClientName);
            _logger = logger;
        }

        public async Task<CertificateResult> GenerateTaxCertificateAsync(Donation donation)
        {
            var payload = new
            {
                donorName = donation.DonorName,
                amount = donation.DonationAmount,
                currency = donation.Currency,
                donationType = donation.IsRecurring ? "recurring" : "once-off",
                anonymous = string.IsNullOrWhiteSpace(donation.DonorName)
            };

            try
            {
                var response = await _http.PostAsJsonAsync("GenerateTaxCertificate", payload);

                if (response.IsSuccessStatusCode)
                {
                    var certificate = await response.Content.ReadFromJsonAsync<TaxCertificateDto>();
                    if (!string.IsNullOrWhiteSpace(certificate?.CertificateNumber))
                    {
                        _logger.LogInformation("Received certificate {CertificateNumber} for donation {DonationId}.",
                            certificate.CertificateNumber, donation.Id);
                        return CertificateResult.Ok(certificate.CertificateNumber);
                    }
                }

                var error = await ReadErrorAsync(response);
                _logger.LogWarning("Certificate function returned {StatusCode}: {Error}", (int)response.StatusCode, error);
                return CertificateResult.Failed(error ?? "The certificate service could not issue a certificate.");
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException)
            {
                _logger.LogError(ex, "Could not reach the certificate function.");
                return CertificateResult.Failed("The certificate service is unavailable right now.");
            }
        }

        public async Task<bool> LogProjectUpdateAsync(ProjectUpdate update)
        {
            var payload = new
            {
                title = update.Title,
                message = update.Message,
                postedBy = update.PostedBy
            };

            try
            {
                var response = await _http.PostAsJsonAsync("LogProjectUpdate", payload);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Project update function returned {StatusCode}: {Error}",
                    (int)response.StatusCode, await ReadErrorAsync(response));
                return false;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogError(ex, "Could not reach the project update function.");
                return false;
            }
        }

        private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var error = await response.Content.ReadFromJsonAsync<FunctionErrorDto>();
                if (error?.Errors != null && error.Errors.Count > 0)
                {
                    return string.Join(" ", error.Errors);
                }
                return error?.Message;
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                return null;
            }
        }

        private class TaxCertificateDto
        {
            public string? CertificateNumber { get; set; }
        }

        private class FunctionErrorDto
        {
            public string? Message { get; set; }
            public List<string>? Errors { get; set; }
        }
    }

    public class CertificateResult
    {
        public bool Success { get; private set; }
        public string? CertificateNumber { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static CertificateResult Ok(string certificateNumber) =>
            new CertificateResult { Success = true, CertificateNumber = certificateNumber };

        public static CertificateResult Failed(string message) =>
            new CertificateResult { Success = false, ErrorMessage = message };
    }
}
