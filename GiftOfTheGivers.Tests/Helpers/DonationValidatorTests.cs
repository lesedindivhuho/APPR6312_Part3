using GiftOfTheGivers.Helpers;

namespace GiftOfTheGivers.Tests.Helpers;

public class DonationValidatorTests
{
    [Fact]
    public void Validate_ValidNamedDonation_ReturnsNoErrors()
    {
        var errors = DonationValidator.Validate(500m, "ZAR", "once-off", "Thabo Mokoena", anonymous: false);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_AnonymousDonationWithoutName_ReturnsNoErrors()
    {
        var errors = DonationValidator.Validate(250m, "USD", "recurring", null, anonymous: true);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_AmountZeroOrNegative_ReturnsGreaterThanZeroError(double amount)
    {
        var errors = DonationValidator.Validate((decimal)amount, "ZAR", "once-off", "Thabo", anonymous: false);

        Assert.Equal(new[] { "Amount must be greater than zero." }, errors);
    }

    [Fact]
    public void Validate_MissingAmount_ReturnsRequiredError()
    {
        var errors = DonationValidator.Validate(null, "ZAR", "once-off", "Thabo", anonymous: false);

        Assert.Equal(new[] { "Amount is required." }, errors);
    }

    [Fact]
    public void Validate_AmountAtMaximum_IsAllowed()
    {
        var errors = DonationValidator.Validate(DonationValidator.MaxAmount, "ZAR", "once-off", "Thabo", anonymous: false);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_AmountAboveMaximum_ReturnsLimitError()
    {
        var errors = DonationValidator.Validate(1_000_000.01m, "ZAR", "once-off", "Thabo", anonymous: false);

        Assert.Equal(new[] { "Amount cannot be more than 1,000,000." }, errors);
    }

    [Theory]
    [InlineData("zar")]
    [InlineData(" USD ")]
    [InlineData("Eur")]
    public void Validate_SupportedCurrencyInAnyCase_IsAccepted(string currency)
    {
        var errors = DonationValidator.Validate(100m, currency, "once-off", "Thabo", anonymous: false);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_UnsupportedCurrency_ReturnsCurrencyError()
    {
        var errors = DonationValidator.Validate(100m, "GBP", "once-off", "Thabo", anonymous: false);

        Assert.Equal(new[] { "Currency 'GBP' is not supported. Use ZAR, USD or EUR." }, errors);
    }

    [Fact]
    public void Validate_MissingCurrency_ReturnsRequiredError()
    {
        var errors = DonationValidator.Validate(100m, " ", "once-off", "Thabo", anonymous: false);

        Assert.Equal(new[] { "Currency is required (ZAR, USD or EUR)." }, errors);
    }

    [Fact]
    public void Validate_UnsupportedDonationType_ReturnsDonationTypeError()
    {
        var errors = DonationValidator.Validate(100m, "ZAR", "weekly", "Thabo", anonymous: false);

        Assert.Equal(new[] { "Donation type 'weekly' is not supported. Use once-off or recurring." }, errors);
    }

    [Fact]
    public void Validate_NamedDonationWithoutName_ReturnsDonorNameError()
    {
        var errors = DonationValidator.Validate(100m, "ZAR", "once-off", "", anonymous: false);

        Assert.Equal(new[] { "Donor name is required unless the donation is anonymous." }, errors);
    }

    [Fact]
    public void Validate_DonorNameTooLong_ReturnsLengthError()
    {
        var longName = new string('a', DonationValidator.MaxNameLength + 1);

        var errors = DonationValidator.Validate(100m, "ZAR", "once-off", longName, anonymous: false);

        Assert.Equal(new[] { "Donor name cannot be longer than 100 characters." }, errors);
    }

    [Fact]
    public void Validate_SeveralProblems_ReturnsEveryErrorInOrder()
    {
        var errors = DonationValidator.Validate(-50m, "GBP", "weekly", null, anonymous: false);

        Assert.Equal(new[]
        {
            "Amount must be greater than zero.",
            "Currency 'GBP' is not supported. Use ZAR, USD or EUR.",
            "Donation type 'weekly' is not supported. Use once-off or recurring.",
            "Donor name is required unless the donation is anonymous."
        }, errors);
    }

    [Fact]
    public void SupportedValues_MatchTheDonationForm()
    {
        Assert.Equal(new[] { "ZAR", "USD", "EUR" }, DonationValidator.SupportedCurrencies);
        Assert.Equal(new[] { "once-off", "recurring" }, DonationValidator.SupportedDonationTypes);
    }
}
