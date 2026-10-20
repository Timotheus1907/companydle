using Companydle.Services;

namespace Companydle.Tests;

public class NumberFormatterTests
{
    [TestCase(0, "$0")]
    [TestCase(999, "$999")]
    [TestCase(1_000, "$1k")]
    [TestCase(164_000, "$164k")]
    [TestCase(999_999, "$1M")]
    [TestCase(1_234_000, "$1.23M")]
    [TestCase(182_000_000_000, "$182B")]
    [TestCase(1_234_000_000_000, "$1.23T")]
    [TestCase(-8_200_000_000, "-$8.2B")]
    public void FormatUsd_UsesCompactUnits(long value, string expected)
    {
        Assert.That(NumberFormatter.FormatUsd(value), Is.EqualTo(expected));
    }

    [TestCase(544, "544")]
    [TestCase(319_000, "319k")]
    [TestCase(999_999, "1M")]
    [TestCase(1_234_000, "1.23M")]
    [TestCase(-1_500, "-1.5k")]
    public void FormatEmployees_UsesCompactUnits(int value, string expected)
    {
        Assert.That(NumberFormatter.FormatEmployees(value), Is.EqualTo(expected));
    }

    [TestCase(null, "-")]
    [TestCase(0, "0.0%")]
    [TestCase(21, "21.0%")]
    [TestCase(-2.5, "-2.5%")]
    public void FormatTaxRate_UsesOneDecimalPlace(decimal? value, string expected)
    {
        Assert.That(NumberFormatter.FormatTaxRate(value), Is.EqualTo(expected));
    }

    [TestCase(0, "0%")]
    [TestCase(66.666, "66.7%")]
    [TestCase(100, "100%")]
    public void FormatPercent_RoundsToOneDecimal(double value, string expected)
    {
        Assert.That(NumberFormatter.FormatPercent(value), Is.EqualTo(expected));
    }
}
