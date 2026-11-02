namespace GiftOfTheGivers.Helpers;

public static class CertificateNumberGenerator
{
    // Format: GOTG-yyyyMMdd-XXXXXX, where the date is the UTC issue date
    public static string Create(DateTime issuedDate)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"GOTG-{issuedDate:yyyyMMdd}-{suffix}";
    }
}
