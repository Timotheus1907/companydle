using Companydle.Models;
using Companydle.Services;

namespace Companydle.Tests;

public class GuessEvaluatorTests
{
    private static Company Answer() => MakeCompany(
        id: "answer",
        name: "Answer Corp",
        country: "United States",
        continent: "North America",
        sector: "Technology",
        industry: "Software",
        exchange: "NASDAQ",
        founded: 2000,
        employees: 100_000,
        revenue: 1_000_000_000m,
        netProfit: 100_000_000m,
        marketCap: 5_000_000_000m,
        taxRate: 20m);

    [Test]
    public void Evaluate_ReturnsAllTenAttributesInDisplayOrder()
    {
        var result = GuessEvaluator.Evaluate(Answer(), Answer());

        Assert.That(result.Attributes, Has.Count.EqualTo(10));
        Assert.That(
            result.Attributes.Select(attribute => attribute.Kind),
            Is.EqualTo(new[]
            {
                AttributeKind.Country,
                AttributeKind.Sector,
                AttributeKind.Industry,
                AttributeKind.Founded,
                AttributeKind.Employees,
                AttributeKind.Revenue,
                AttributeKind.NetProfit,
                AttributeKind.MarketCap,
                AttributeKind.EffectiveTaxRate,
                AttributeKind.Exchange,
            }));
        Assert.That(result.Attributes.All(attribute => attribute.Feedback == Feedback.Correct), Is.True);
        Assert.That(result.IsWin, Is.True);
        Assert.That(result.EmojiRow, Is.EqualTo("🟩🟩🟩🟩🟩🟩🟩🟩🟩🟩"));
    }

    [TestCase("United States", "North America", Feedback.Correct)]
    [TestCase("Canada", "North America", Feedback.Close)]
    [TestCase("Germany", "Europe", Feedback.Wrong)]
    public void Country_UsesCountryThenContinent(string country, string continent, Feedback expected)
    {
        var guess = MakeCompany(country: country, continent: continent);
        var attribute = Attribute(GuessEvaluator.Evaluate(guess, Answer()), AttributeKind.Country);

        Assert.That(attribute.Feedback, Is.EqualTo(expected));
        Assert.That(attribute.Direction, Is.EqualTo(Direction.None));
    }

    [Test]
    public void Sector_IsOnlyGreenOnExactMatch()
    {
        var same = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(sector: "Technology"), Answer()),
            AttributeKind.Sector);
        var different = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(sector: "Financials"), Answer()),
            AttributeKind.Sector);

        Assert.That(same.Feedback, Is.EqualTo(Feedback.Correct));
        Assert.That(different.Feedback, Is.EqualTo(Feedback.Wrong));
    }

    [Test]
    public void Industry_YellowWhenSameSectorButDifferentIndustry()
    {
        var exact = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(sector: "Technology", industry: "Software"), Answer()),
            AttributeKind.Industry);
        var sameSector = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(sector: "Technology", industry: "Semiconductors"), Answer()),
            AttributeKind.Industry);
        var otherSector = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(sector: "Financials", industry: "Banks"), Answer()),
            AttributeKind.Industry);

        Assert.That(exact.Feedback, Is.EqualTo(Feedback.Correct));
        Assert.That(sameSector.Feedback, Is.EqualTo(Feedback.Close));
        Assert.That(otherSector.Feedback, Is.EqualTo(Feedback.Wrong));
    }

    [TestCase(2000, Feedback.Correct, Direction.None)]
    [TestCase(2010, Feedback.Close, Direction.Down)]
    [TestCase(1990, Feedback.Close, Direction.Up)]
    [TestCase(2011, Feedback.Wrong, Direction.Down)]
    public void Founded_UsesTenYearWindowAndDirection(int founded, Feedback expected, Direction direction)
    {
        var attribute = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(founded: founded), Answer()),
            AttributeKind.Founded);

        Assert.That(attribute.Feedback, Is.EqualTo(expected));
        Assert.That(attribute.Direction, Is.EqualTo(direction));
    }

    [TestCase(100_000, Feedback.Correct, Direction.None)]
    [TestCase(105_000, Feedback.Correct, Direction.None)]
    [TestCase(125_000, Feedback.Close, Direction.Down)]
    [TestCase(75_000, Feedback.Close, Direction.Up)]
    [TestCase(125_001, Feedback.Wrong, Direction.Down)]
    [TestCase(74_999, Feedback.Wrong, Direction.Up)]
    public void Employees_UsesFiveAndTwentyFivePercentThresholds(
        int employees,
        Feedback expected,
        Direction direction)
    {
        var attribute = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(employees: employees), Answer()),
            AttributeKind.Employees);

        Assert.That(attribute.Feedback, Is.EqualTo(expected));
        Assert.That(attribute.Direction, Is.EqualTo(direction));
    }

    [TestCase(1_050_000_000, Feedback.Correct)]
    [TestCase(1_250_000_000, Feedback.Close)]
    [TestCase(1_250_001_000, Feedback.Wrong)]
    public void Revenue_UsesRelativeThresholds(decimal revenue, Feedback expected)
    {
        var attribute = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(revenue: revenue), Answer()),
            AttributeKind.Revenue);

        Assert.That(attribute.Feedback, Is.EqualTo(expected));
    }

    [Test]
    public void MarketCap_AndRevenueUseCompactDisplay()
    {
        var result = GuessEvaluator.Evaluate(MakeCompany(marketCap: 4_900_000_000m), Answer());
        var marketCap = Attribute(result, AttributeKind.MarketCap);

        Assert.That(marketCap.Feedback, Is.EqualTo(Feedback.Correct));
        Assert.That(marketCap.DisplayValue, Is.EqualTo("$4.9B"));
        Assert.That(marketCap.Arrow, Is.Empty);
    }

    [TestCase(-105_000_000, Feedback.Correct, Direction.None)]
    [TestCase(-125_000_000, Feedback.Close, Direction.Up)]
    [TestCase(-75_000_000, Feedback.Close, Direction.Down)]
    [TestCase(100_000_000, Feedback.Wrong, Direction.Down)]
    [TestCase(0, Feedback.Wrong, Direction.Down)]
    public void NetProfit_RequiresMatchingSignsForCloseFeedback(
        decimal profit,
        Feedback expected,
        Direction direction)
    {
        var answer = MakeCompany(netProfit: -100_000_000m);
        var attribute = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(netProfit: profit), answer),
            AttributeKind.NetProfit);

        Assert.That(attribute.Feedback, Is.EqualTo(expected));
        Assert.That(attribute.Direction, Is.EqualTo(direction));
    }

    [TestCase(21, Feedback.Correct, Direction.None)]
    [TestCase(25, Feedback.Close, Direction.Down)]
    [TestCase(26, Feedback.Wrong, Direction.Down)]
    [TestCase(14, Feedback.Wrong, Direction.Up)]
    public void TaxRate_UsesPercentagePointThresholds(
        decimal taxRate,
        Feedback expected,
        Direction direction)
    {
        var attribute = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(taxRate: taxRate), Answer()),
            AttributeKind.EffectiveTaxRate);

        Assert.That(attribute.Feedback, Is.EqualTo(expected));
        Assert.That(attribute.Direction, Is.EqualTo(direction));
    }

    [Test]
    public void NullTaxRate_IsUnknownAndUsesUnknownClass()
    {
        var attribute = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(taxRate: null), Answer()),
            AttributeKind.EffectiveTaxRate);

        Assert.That(attribute.Feedback, Is.EqualTo(Feedback.Unknown));
        Assert.That(attribute.FeedbackClass, Is.EqualTo("bg-body-secondary"));
        Assert.That(attribute.DisplayValue, Is.EqualTo("-"));
        Assert.That(attribute.StateText, Is.EqualTo("Unknown"));
        Assert.That(attribute.Direction, Is.EqualTo(Direction.None));
    }

    [Test]
    public void Exchange_IsExactOnly()
    {
        var exact = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(exchange: "NASDAQ"), Answer()),
            AttributeKind.Exchange);
        var different = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(exchange: "NYSE"), Answer()),
            AttributeKind.Exchange);

        Assert.That(exact.Feedback, Is.EqualTo(Feedback.Correct));
        Assert.That(different.Feedback, Is.EqualTo(Feedback.Wrong));
        Assert.That(different.FeedbackClass, Is.EqualTo("bg-body-secondary"));
    }

    [Test]
    public void FeedbackClasses_AreTheRequiredBootstrapClasses()
    {
        var green = Attribute(GuessEvaluator.Evaluate(Answer(), Answer()), AttributeKind.Country);
        var yellow = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(country: "Canada", continent: "North America"), Answer()),
            AttributeKind.Country);
        var grey = Attribute(
            GuessEvaluator.Evaluate(MakeCompany(country: "Germany", continent: "Europe"), Answer()),
            AttributeKind.Country);

        Assert.That(green.FeedbackClass, Is.EqualTo("bg-success"));
        Assert.That(yellow.FeedbackClass, Is.EqualTo("bg-warning text-dark"));
        Assert.That(grey.FeedbackClass, Is.EqualTo("bg-body-secondary"));
    }

    private static AttributeResult Attribute(GuessResult result, AttributeKind kind) =>
        result.Attributes.Single(attribute => attribute.Kind == kind);

    private static Company MakeCompany(
        string id = "guess",
        string name = "Guess Corp",
        string country = "United States",
        string continent = "North America",
        string sector = "Technology",
        string industry = "Software",
        string exchange = "NASDAQ",
        int founded = 2000,
        int employees = 100_000,
        decimal revenue = 1_000_000_000m,
        decimal netProfit = 100_000_000m,
        decimal marketCap = 5_000_000_000m,
        decimal? taxRate = 20m) =>
        new()
        {
            Id = id,
            Name = name,
            Country = country,
            Continent = continent,
            Sector = sector,
            Industry = industry,
            Exchange = exchange,
            Founded = founded,
            Employees = employees,
            RevenueUsd = revenue,
            NetProfitUsd = netProfit,
            MarketCapUsd = marketCap,
            EffectiveTaxRate = taxRate,
        };
}
