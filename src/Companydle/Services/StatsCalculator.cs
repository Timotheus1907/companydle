using Companydle.Models;

namespace Companydle.Services;

/// <summary>
/// Daily-only statistics. Archive games deliberately never call this class.
/// </summary>
public static class StatsCalculator
{
    public static double WinPercent(GameStats stats) =>
        stats.Played == 0 ? 0 : stats.Wins * 100d / stats.Played;

    /// <summary>
    /// Records one completed daily puzzle. Calling this twice for the same date
    /// is a no-op, which makes recovery from an interrupted save safe.
    /// </summary>
    public static void RecordDailyCompletion(
        GameStats stats,
        DateOnly date,
        bool won,
        int guessCount)
    {
        ArgumentNullException.ThrowIfNull(stats);

        if (stats.LastCompletedDate is { } last && date <= last)
        {
            return;
        }

        stats.Played++;
        if (won)
        {
            stats.Wins++;
        }

        var distribution = stats.Distribution;
        if (distribution.Length != GameRules.MaxGuesses + 1)
        {
            Array.Resize(ref distribution, GameRules.MaxGuesses + 1);
            stats.Distribution = distribution;
        }

        var bucket = won
            ? Math.Clamp(guessCount, 1, GameRules.MaxGuesses) - 1
            : GameRules.MaxGuesses;
        distribution[bucket]++;

        if (stats.LastCompletedDate is { } previous && previous.AddDays(1) == date)
        {
            stats.CurrentStreak = won ? stats.CurrentStreak + 1 : 0;
        }
        else
        {
            stats.CurrentStreak = won ? 1 : 0;
        }

        stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak);
        stats.LastCompletedDate = date;
    }
}
