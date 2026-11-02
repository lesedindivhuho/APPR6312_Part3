using System.Globalization;
using GiftOfTheGivers.Helpers;

namespace GiftOfTheGivers.Tests.Helpers;

public class AmountFormatterTests
{
    [Theory]
    [InlineData(300, "ZAR", "ZAR 300.00")]
    [InlineData(1250.5, "usd", "USD 1,250.50")]
    [InlineData(0.5, " eur ", "EUR 0.50")]
    public void Format_AddsCurrencyAndTwoDecimals(double amount, string currency, string expected)
    {
        Assert.Equal(expected, AmountFormatter.Format((decimal)amount, currency));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Format_WithoutCurrency_ReturnsAmountOnly(string? currency)
    {
        Assert.Equal("300.00", AmountFormatter.Format(300m, currency));
    }

    [Fact]
    public void Format_IgnoresTheServerCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            // en-ZA uses a comma for decimals, the certificate should not change with it
            CultureInfo.CurrentCulture = new CultureInfo("en-ZA");

            Assert.Equal("ZAR 1,250.50", AmountFormatter.Format(1250.5m, "ZAR"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
