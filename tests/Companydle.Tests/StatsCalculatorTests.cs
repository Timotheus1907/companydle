using Companydle.Models;
using Companydle.Services;

namespace Companydle.Tests;

public class StatsCalculatorTests
{
    [Test]
    public void RecordDailyCompletion_UpdatesCountsAndDistribution()
    {
        var stats = new GameStats();

        StatsCalculator.RecordDailyCompletion(stats, new DateOnly(2026, 10, 20), true, 3);

        Assert.Multiple(() =>
        {
            Assert.That(stats.Played, Is.EqualTo(1));
            Assert.That(stats.Wins, Is.EqualTo(1));
            Assert.That(stats.Distribution[2], Is.EqualTo(1));
            Assert.That(stats.CurrentStreak, Is.EqualTo(1));
            Assert.That(stats.MaxStreak, Is.EqualTo(1));
            Assert.That(stats.LastCompletedDate, Is.EqualTo(new DateOnly(2026, 10, 20)));
        });
    }

    [Test]
    public void RecordDailyCompletion_LossUsesFailBucketAndResetsStreak()
    {
        var stats = new GameStats
        {
            Played = 1,
            Wins = 1,
            CurrentStreak = 3,
            MaxStreak = 3,
            Distribution = [0, 0, 1, 0, 0, 0, 0, 0, 0],
            LastCompletedDate = new DateOnly(2026, 10, 19),
        };

        StatsCalculator.RecordDailyCompletion(stats, new DateOnly(2026, 10, 20), false, 8);

        Assert.Multiple(() =>
        {
            Assert.That(stats.Played, Is.EqualTo(2));
            Assert.That(stats.Wins, Is.EqualTo(1));
            Assert.That(stats.CurrentStreak, Is.EqualTo(0));
            Assert.That(stats.MaxStreak, Is.EqualTo(3));
            Assert.That(stats.Distribution[8], Is.EqualTo(1));
        });
    }

    [Test]
    public void RecordDailyCompletion_GapResetsStreakToZeroOrOne()
    {
        var stats = new GameStats
        {
            Played = 1,
            Wins = 1,
            CurrentStreak = 4,
            MaxStreak = 4,
            Distribution = [0, 0, 0, 0, 1, 0, 0, 0, 0],
            LastCompletedDate = new DateOnly(2026, 10, 18),
        };

        StatsCalculator.RecordDailyCompletion(stats, new DateOnly(2026, 10, 20), true, 2);

        Assert.Multiple(() =>
        {
            Assert.That(stats.CurrentStreak, Is.EqualTo(1));
            Assert.That(stats.MaxStreak, Is.EqualTo(4));
            Assert.That(stats.Distribution[1], Is.EqualTo(1));
        });
    }

    [Test]
    public void RecordDailyCompletion_ConsecutiveWinIncrementsStreak()
    {
        var stats = new GameStats
        {
            Played = 1,
            Wins = 1,
            CurrentStreak = 4,
            MaxStreak = 4,
            Distribution = [0, 0, 0, 0, 1, 0, 0, 0, 0],
            LastCompletedDate = new DateOnly(2026, 10, 19),
        };

        StatsCalculator.RecordDailyCompletion(stats, new DateOnly(2026, 10, 20), true, 1);

        Assert.That(stats.CurrentStreak, Is.EqualTo(5));
        Assert.That(stats.MaxStreak, Is.EqualTo(5));
    }

    [Test]
    public void RecordDailyCompletion_IsIdempotentForTheSameDate()
    {
        var stats = new GameStats();
        var date = new DateOnly(2026, 10, 20);

        StatsCalculator.RecordDailyCompletion(stats, date, true, 3);
        StatsCalculator.RecordDailyCompletion(stats, date, true, 3);

        Assert.That(stats.Played, Is.EqualTo(1));
        Assert.That(stats.Distribution[2], Is.EqualTo(1));
    }

    [TestCase(0, 0, 0)]
    [TestCase(1, 1, 100)]
    [TestCase(3, 2, 66.666)]
    public void WinPercent_IsZeroWhenUnplayedAndRoundedByTheUi(
        int played,
        int wins,
        double expected)
    {
        var stats = new GameStats { Played = played, Wins = wins };

        Assert.That(
            StatsCalculator.WinPercent(stats),
            Is.EqualTo(expected).Within(0.001));
    }
}
