using Companydle.Models;
using Companydle.Services;

namespace Companydle.Tests;

public class DailyPuzzleServiceTests
{
    private static readonly CompanyData Data = new()
    {
        DataAsOf = new DateOnly(2026, 10, 20),
        Currency = "USD",
        AnswerOrder = ["alpha", "beta", "gamma"],
        Companies =
        [
            new Company { Id = "alpha", Name = "Alpha" },
            new Company { Id = "beta", Name = "Beta" },
            new Company { Id = "gamma", Name = "Gamma" },
        ],
    };

    [TestCase("2026-10-19", 0)]
    [TestCase("2026-10-20", 1)]
    [TestCase("2026-10-21", 2)]
    public void GetPuzzleNumber_CountsLocalDaysSinceLaunch(string date, int expected)
    {
        var localDate = DateOnly.Parse(date);

        Assert.That(DailyPuzzleService.GetPuzzleNumber(localDate), Is.EqualTo(expected));
    }

    [Test]
    public void GetPuzzleNumber_ClampsDatesBeforeLaunch()
    {
        Assert.That(
            DailyPuzzleService.GetPuzzleNumber(new DateOnly(2026, 10, 18)),
            Is.EqualTo(0));
    }

    [Test]
    public void GetPuzzleDate_IsTheInverseOfThePuzzleNumber()
    {
        var date = DailyPuzzleService.GetPuzzleDate(12);

        Assert.That(date, Is.EqualTo(new DateOnly(2026, 10, 31)));
        Assert.That(DailyPuzzleService.GetPuzzleNumber(date), Is.EqualTo(12));
    }

    [Test]
    public void GetAnswer_UsesAnswerOrderModuloLength()
    {
        Assert.That(DailyPuzzleService.GetAnswer(Data, 0).Id, Is.EqualTo("alpha"));
        Assert.That(DailyPuzzleService.GetAnswer(Data, 1).Id, Is.EqualTo("beta"));
        Assert.That(DailyPuzzleService.GetAnswer(Data, 4).Id, Is.EqualTo("beta"));
    }

    [Test]
    public void GetArchiveNumbers_ReturnsPreviousPuzzlesNewestFirst()
    {
        var archive = DailyPuzzleService.GetArchiveNumbers(new DateOnly(2026, 10, 22));

        Assert.That(archive, Is.EqualTo(new[] { 2, 1, 0 }));
    }

    [Test]
    public void GetArchiveNumbers_IsEmptyOnLaunchDay()
    {
        Assert.That(
            DailyPuzzleService.GetArchiveNumbers(new DateOnly(2026, 10, 19)),
            Is.Empty);
    }

    [Test]
    public void GetAnswer_RejectsUnknownAnswerId()
    {
        var data = new CompanyData
        {
            AnswerOrder = ["missing"],
            Companies = [new Company { Id = "alpha", Name = "Alpha" }],
        };

        Assert.Throws<InvalidOperationException>(() => DailyPuzzleService.GetAnswer(data, 0));
    }
}
