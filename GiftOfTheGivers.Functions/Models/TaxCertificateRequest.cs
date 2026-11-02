namespace GiftOfTheGivers.Functions.Models;

public class TaxCertificateRequest
{
    public string? DonorName { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? DonationType { get; set; }
    public bool Anonymous { get; set; }
}
