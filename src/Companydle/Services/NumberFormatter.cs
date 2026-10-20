using System.Globalization;

namespace Companydle.Services;

/// <summary>
/// Compact, locale-independent display formatting for the game tiles and share
/// text. Three significant figures keep the values readable without implying
/// more precision than the data has.
/// </summary>
public static class NumberFormatter
{
    private static readonly (decimal Divisor, string Suffix)[] Units =
    [
        (1m, string.Empty),
        (1_000m, "k"),
        (1_000_000m, "M"),
        (1_000_000_000m, "B"),
        (1_000_000_000_000m, "T"),
    ];

    public static string FormatUsd(decimal value)
    {
        if (value == 0)
        {
            return "$0";
        }

        var negative = value < 0;
        return $"{(negative ? "-" : string.Empty)}${FormatCompact(Math.Abs(value))}";
    }

    public static string FormatCount(decimal value)
    {
        if (value == 0)
        {
            return "0";
        }

        var negative = value < 0;
        return $"{(negative ? "-" : string.Empty)}{FormatCompact(Math.Abs(value))}";
    }

    public static string FormatEmployees(int value) => FormatCount(value);

    public static string FormatTaxRate(decimal? value) =>
        value is null ? "-" : $"{value.Value.ToString("0.0", CultureInfo.InvariantCulture)}%";

    public static string FormatPercent(double value) =>
        $"{Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.#", CultureInfo.InvariantCulture)}%";

    private static string FormatCompact(decimal abs)
    {
        var unitIndex = 0;
        for (var index = 1; index < Units.Length; index++)
        {
            if (abs >= Units[index].Divisor)
            {
                unitIndex = index;
            }
        }

        while (true)
        {
            var scaled = abs / Units[unitIndex].Divisor;
            var (format, decimals) = scaled switch
            {
                >= 100m => ("0", 0),
                >= 10m => ("0.#", 1),
                _ => ("0.##", 2),
            };
            var rounded = decimal.Round(scaled, decimals, MidpointRounding.AwayFromZero);

            if (rounded >= 1000m && unitIndex < Units.Length - 1)
            {
                unitIndex++;
                continue;
            }

            return $"{rounded.ToString(format, CultureInfo.InvariantCulture)}{Units[unitIndex].Suffix}";
        }
    }
}
