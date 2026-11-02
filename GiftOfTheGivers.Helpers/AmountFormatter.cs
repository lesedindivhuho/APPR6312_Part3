using System.Globalization;

namespace GiftOfTheGivers.Helpers;

public static class AmountFormatter
{
    // For example 300 and "zar" give "ZAR 300.00"
    public static string Format(decimal amount, string? currency)
    {
        var formatted = amount.ToString("N2", CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(currency))
        {
            return formatted;
        }

        return $"{currency.Trim().ToUpperInvariant()} {formatted}";
    }
}
