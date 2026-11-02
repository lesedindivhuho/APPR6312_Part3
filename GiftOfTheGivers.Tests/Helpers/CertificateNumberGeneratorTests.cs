using System.Text.RegularExpressions;
using GiftOfTheGivers.Helpers;

namespace GiftOfTheGivers.Tests.Helpers;

public class CertificateNumberGeneratorTests
{
    [Fact]
    public void Create_UsesPrefixIssueDateAndSixCharacterSuffix()
    {
        var issuedDate = new DateTime(2026, 9, 25, 10, 30, 0, DateTimeKind.Utc);

        var number = CertificateNumberGenerator.Create(issuedDate);

        Assert.Matches(new Regex("^GOTG-20260925-[0-9A-F]{6}$"), number);
    }

    [Fact]
    public void Create_SameDate_GivesDifferentNumbers()
    {
        var issuedDate = new DateTime(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);

        var numbers = Enumerable.Range(0, 20)
            .Select(_ => CertificateNumberGenerator.Create(issuedDate))
            .ToList();

        Assert.Equal(numbers.Count, numbers.Distinct().Count());
    }
}
