namespace Companydle.Tests;

public class GameRulesTests
{
    [Test]
    public void MaxGuesses_IsEight()
    {
        Assert.That(GameRules.MaxGuesses, Is.EqualTo(8));
    }

    [Test]
    public void HintThresholds_AreFourAndSix()
    {
        Assert.That(GameRules.HintFirstLetterAfterGuesses, Is.EqualTo(4));
        Assert.That(GameRules.HintWordCountAfterGuesses, Is.EqualTo(6));
    }

    [Test]
    public void LaunchDate_IsTheDayBeforeTheSeedLaunch()
    {
        Assert.That(GameRules.LaunchDate, Is.EqualTo(new DateOnly(2026, 10, 19)));
    }

    [Test]
    public void ComparisonThresholds_AreTightAndLoose()
    {
        Assert.That(GameRules.NumericGreenRatio, Is.EqualTo(0.05m));
        Assert.That(GameRules.NumericYellowRatio, Is.EqualTo(0.25m));
        Assert.That(GameRules.TaxGreenPercentagePoints, Is.EqualTo(1m));
        Assert.That(GameRules.TaxYellowPercentagePoints, Is.EqualTo(5m));
    }
}
