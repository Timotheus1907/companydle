namespace Companydle;

/// <summary>
/// Central home for every tunable game rule. Keep thresholds here so balancing
/// the game never requires touching the evaluator or the UI.
/// </summary>
public static class GameRules
{
    public const string AppName = "Companydle";

    public const int MaxGuesses = 8;
    public const int HintFirstLetterAfterGuesses = 4;
    public const int HintWordCountAfterGuesses = 6;

    public const int FoundedCloseYears = 10;
    public const decimal NumericGreenRatio = 0.05m;
    public const decimal NumericYellowRatio = 0.25m;
    public const decimal TaxGreenPercentagePoints = 1m;
    public const decimal TaxYellowPercentagePoints = 5m;

    /// <summary>
    /// Puzzle 0. The first live puzzle number is the number of local days
    /// since this date. Change this only before the game is launched.
    /// </summary>
    public static readonly DateOnly LaunchDate = new(2026, 10, 19);

    public const int StateVersion = 1;
    public const string StorageKey = "companydle:state";
    public const string AnswerOrderSalt = "companydle-v1";
}
