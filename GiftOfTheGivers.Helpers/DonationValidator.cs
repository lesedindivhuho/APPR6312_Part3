using System.Globalization;

namespace GiftOfTheGivers.Helpers;

public static class DonationValidator
{
    public const decimal MaxAmount = 1_000_000m;
    public const int MaxNameLength = 100;

    public static IReadOnlyList<string> SupportedCurrencies { get; } = new[] { "ZAR", "USD", "EUR" };
    public static IReadOnlyList<string> SupportedDonationTypes { get; } = new[] { "once-off", "recurring" };

    // Returns an empty list when the donation is valid
    public static List<string> Validate(decimal? amount, string? currency, string? donationType, string? donorName, bool anonymous)
    {
        var errors = new List<string>();

        if (amount == null)
        {
            errors.Add("Amount is required.");
        }
        else if (amount <= 0)
        {
            errors.Add("Amount must be greater than zero.");
        }
        else if (amount > MaxAmount)
        {
            errors.Add($"Amount cannot be more than {MaxAmount.ToString("N0", CultureInfo.InvariantCulture)}.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            errors.Add("Currency is required (ZAR, USD or EUR).");
        }
        else if (!SupportedCurrencies.Contains(currency.Trim().ToUpperInvariant()))
        {
            errors.Add($"Currency '{currency}' is not supported. Use ZAR, USD or EUR.");
        }

        if (string.IsNullOrWhiteSpace(donationType))
        {
            errors.Add("Donation type is required (once-off or recurring).");
        }
        else if (!SupportedDonationTypes.Contains(donationType.Trim().ToLowerInvariant()))
        {
            errors.Add($"Donation type '{donationType}' is not supported. Use once-off or recurring.");
        }

        if (!anonymous)
        {
            if (string.IsNullOrWhiteSpace(donorName))
            {
                errors.Add("Donor name is required unless the donation is anonymous.");
            }
            else if (donorName.Trim().Length > MaxNameLength)
            {
                errors.Add($"Donor name cannot be longer than {MaxNameLength} characters.");
            }
        }

        return errors;
    }
}
