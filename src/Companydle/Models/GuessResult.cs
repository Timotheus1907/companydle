namespace Companydle.Models;

/// <summary>
/// The ten comparable attributes shown for each guess, in display order.
/// </summary>
public enum AttributeKind
{
    Country,
    Sector,
    Industry,
    Founded,
    Employees,
    Revenue,
    NetProfit,
    MarketCap,
    EffectiveTaxRate,
    Exchange,
}

public enum Feedback
{
    Correct,
    Close,
    Wrong,
    Unknown,
}

public enum Direction
{
    None,
    Up,
    Down,
}

/// <summary>
/// One evaluated tile. The enum and direction are kept separate from the CSS
/// class so the same result can drive the UI, accessibility text and share text.
/// </summary>
public sealed class AttributeResult
{
    public required AttributeKind Kind { get; init; }
    public required string Label { get; init; }
    public required string DisplayValue { get; init; }
    public required Feedback Feedback { get; init; }
    public Direction Direction { get; init; }

    public string FeedbackClass => Feedback switch
    {
        Feedback.Correct => "bg-success",
        Feedback.Close => "bg-warning text-dark",
        _ => "bg-body-secondary",
    };

    public string StateText => Feedback switch
    {
        Feedback.Correct => "Correct",
        Feedback.Close => "Close",
        Feedback.Wrong => "Wrong",
        _ => "Unknown",
    };

    public string Arrow => Direction switch
    {
        Direction.Up => "↑",
        Direction.Down => "↓",
        _ => string.Empty,
    };

    public string ShareEmoji => Feedback switch
    {
        Feedback.Correct => "🟩",
        Feedback.Close => "🟨",
        _ => "⬛",
    };

    public string AriaLabel
    {
        get
        {
            var direction = Direction switch
            {
                Direction.Up => " Answer is higher.",
                Direction.Down => " Answer is lower.",
                _ => string.Empty,
            };
            return $"{Label}: {DisplayValue}. {StateText}.{direction}";
        }
    }
}

public sealed class GuessResult
{
    public required Company Guess { get; init; }
    public required IReadOnlyList<AttributeResult> Attributes { get; init; }
    public bool IsWin { get; init; }

    public string EmojiRow => string.Concat(Attributes.Select(attribute => attribute.ShareEmoji));
}
