using Companydle.Models;

namespace Companydle.Services;

/// <summary>
/// Pure comparison logic for the ten tiles. Keep all threshold checks here so
/// the rules can be tuned centrally and tested without rendering a component.
/// </summary>
public static class GuessEvaluator
{
    public static GuessResult Evaluate(Company guess, Company answer)
    {
        ArgumentNullException.ThrowIfNull(guess);
        ArgumentNullException.ThrowIfNull(answer);

        var attributes = new List<AttributeResult>(10)
        {
            Country(guess, answer),
            Sector(guess, answer),
            Industry(guess, answer),
            Founded(guess, answer),
            Numeric(
                AttributeKind.Employees,
                "Employees",
                guess.Employees,
                answer.Employees,
                NumberFormatter.FormatEmployees(guess.Employees)),
            Numeric(
                AttributeKind.Revenue,
                "Revenue",
                guess.RevenueUsd,
                answer.RevenueUsd,
                NumberFormatter.FormatUsd(guess.RevenueUsd)),
            SignedNumeric(
                AttributeKind.NetProfit,
                "Net profit",
                guess.NetProfitUsd,
                answer.NetProfitUsd,
                NumberFormatter.FormatUsd(guess.NetProfitUsd)),
            Numeric(
                AttributeKind.MarketCap,
                "Market cap",
                guess.MarketCapUsd,
                answer.MarketCapUsd,
                NumberFormatter.FormatUsd(guess.MarketCapUsd)),
            TaxRate(guess, answer),
            Exchange(guess, answer),
        };

        return new GuessResult
        {
            Guess = guess,
            Attributes = attributes,
            IsWin = string.Equals(guess.Id, answer.Id, StringComparison.Ordinal),
        };
    }

    private static AttributeResult Country(Company guess, Company answer)
    {
        var feedback = string.Equals(guess.Country, answer.Country, StringComparison.OrdinalIgnoreCase)
            ? Feedback.Correct
            : string.Equals(guess.Continent, answer.Continent, StringComparison.OrdinalIgnoreCase)
                ? Feedback.Close
                : Feedback.Wrong;

        return Category(AttributeKind.Country, "Country", guess.Country, feedback);
    }

    private static AttributeResult Sector(Company guess, Company answer) =>
        Category(
            AttributeKind.Sector,
            "Sector",
            guess.Sector,
            string.Equals(guess.Sector, answer.Sector, StringComparison.OrdinalIgnoreCase)
                ? Feedback.Correct
                : Feedback.Wrong);

    private static AttributeResult Industry(Company guess, Company answer)
    {
        var feedback = string.Equals(guess.Industry, answer.Industry, StringComparison.OrdinalIgnoreCase)
            ? Feedback.Correct
            : string.Equals(guess.Sector, answer.Sector, StringComparison.OrdinalIgnoreCase)
                ? Feedback.Close
                : Feedback.Wrong;

        return Category(AttributeKind.Industry, "Industry", guess.Industry, feedback);
    }

    private static AttributeResult Exchange(Company guess, Company answer) =>
        Category(
            AttributeKind.Exchange,
            "Exchange",
            guess.Exchange,
            string.Equals(guess.Exchange, answer.Exchange, StringComparison.OrdinalIgnoreCase)
                ? Feedback.Correct
                : Feedback.Wrong);

    private static AttributeResult Founded(Company guess, Company answer)
    {
        var difference = Math.Abs(guess.Founded - answer.Founded);
        var feedback = difference == 0
            ? Feedback.Correct
            : difference <= GameRules.FoundedCloseYears
                ? Feedback.Close
                : Feedback.Wrong;
        var direction = feedback == Feedback.Correct
            ? Direction.None
            : answer.Founded > guess.Founded ? Direction.Up : Direction.Down;

        return new AttributeResult
        {
            Kind = AttributeKind.Founded,
            Label = "Founded",
            DisplayValue = guess.Founded.ToString(),
            Feedback = feedback,
            Direction = direction,
        };
    }

    private static AttributeResult Numeric(
        AttributeKind kind,
        string label,
        decimal guessValue,
        decimal answerValue,
        string displayValue)
    {
        var (feedback, direction) = CompareRatio(guessValue, answerValue);
        return new AttributeResult
        {
            Kind = kind,
            Label = label,
            DisplayValue = displayValue,
            Feedback = feedback,
            Direction = direction,
        };
    }

    private static AttributeResult SignedNumeric(
        AttributeKind kind,
        string label,
        decimal guessValue,
        decimal answerValue,
        string displayValue)
    {
        var (feedback, direction) = CompareSignedRatio(guessValue, answerValue);
        return new AttributeResult
        {
            Kind = kind,
            Label = label,
            DisplayValue = displayValue,
            Feedback = feedback,
            Direction = direction,
        };
    }

    private static AttributeResult TaxRate(Company guess, Company answer)
    {
        if (guess.EffectiveTaxRate is null || answer.EffectiveTaxRate is null)
        {
            return new AttributeResult
            {
                Kind = AttributeKind.EffectiveTaxRate,
                Label = "Effective tax rate",
                DisplayValue = NumberFormatter.FormatTaxRate(guess.EffectiveTaxRate),
                Feedback = Feedback.Unknown,
            };
        }

        var difference = Math.Abs(guess.EffectiveTaxRate.Value - answer.EffectiveTaxRate.Value);
        var feedback = difference <= GameRules.TaxGreenPercentagePoints
            ? Feedback.Correct
            : difference <= GameRules.TaxYellowPercentagePoints
                ? Feedback.Close
                : Feedback.Wrong;
        var direction = feedback == Feedback.Correct
            ? Direction.None
            : answer.EffectiveTaxRate.Value > guess.EffectiveTaxRate.Value
                ? Direction.Up
                : Direction.Down;

        return new AttributeResult
        {
            Kind = AttributeKind.EffectiveTaxRate,
            Label = "Effective tax rate",
            DisplayValue = NumberFormatter.FormatTaxRate(guess.EffectiveTaxRate),
            Feedback = feedback,
            Direction = direction,
        };
    }

    private static AttributeResult Category(
        AttributeKind kind,
        string label,
        string value,
        Feedback feedback) =>
        new()
        {
            Kind = kind,
            Label = label,
            DisplayValue = value,
            Feedback = feedback,
        };

    private static (Feedback Feedback, Direction Direction) CompareRatio(decimal guess, decimal answer)
    {
        if (answer == 0m)
        {
            if (guess == 0m)
            {
                return (Feedback.Correct, Direction.None);
            }

            return (Feedback.Wrong, answer > guess ? Direction.Up : Direction.Down);
        }

        var ratio = Math.Abs((guess - answer) / answer);
        var feedback = ratio <= GameRules.NumericGreenRatio
            ? Feedback.Correct
            : ratio <= GameRules.NumericYellowRatio
                ? Feedback.Close
                : Feedback.Wrong;
        var direction = feedback == Feedback.Correct
            ? Direction.None
            : answer > guess ? Direction.Up : Direction.Down;
        return (feedback, direction);
    }

    private static (Feedback Feedback, Direction Direction) CompareSignedRatio(decimal guess, decimal answer)
    {
        if (guess == answer)
        {
            return (Feedback.Correct, Direction.None);
        }

        if (guess == 0m || answer == 0m || decimal.Sign(guess) != decimal.Sign(answer))
        {
            return (Feedback.Wrong, answer > guess ? Direction.Up : Direction.Down);
        }

        var ratio = Math.Abs((guess - answer) / answer);
        var feedback = ratio <= GameRules.NumericGreenRatio
            ? Feedback.Correct
            : ratio <= GameRules.NumericYellowRatio
                ? Feedback.Close
                : Feedback.Wrong;
        var direction = feedback == Feedback.Correct
            ? Direction.None
            : answer > guess ? Direction.Up : Direction.Down;
        return (feedback, direction);
    }
}
