namespace GiftOfTheGivers.Functions.Models;

public class TaxCertificateResponse
{
    public string CertificateNumber { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string DonationType { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
}
