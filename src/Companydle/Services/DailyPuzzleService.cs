using Companydle.Models;

namespace Companydle.Services;

/// <summary>
/// Deterministic daily and archive puzzle selection. The answer order is
/// append-only data, so existing puzzle numbers never change when the dataset
/// grows.
/// </summary>
public static class DailyPuzzleService
{
    public static int GetPuzzleNumber(DateOnly localDate) =>
        Math.Max(0, localDate.DayNumber - GameRules.LaunchDate.DayNumber);

    public static DateOnly GetPuzzleDate(int puzzleNumber) =>
        GameRules.LaunchDate.AddDays(puzzleNumber);

    public static IReadOnlyList<int> GetArchiveNumbers(DateOnly localDate)
    {
        var today = GetPuzzleNumber(localDate);
        return Enumerable.Range(0, today).Reverse().ToArray();
    }

    public static Company GetAnswer(CompanyData data, int puzzleNumber)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (puzzleNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(puzzleNumber));
        }

        if (data.AnswerOrder.Count == 0)
        {
            throw new InvalidOperationException("The company data has no answer order.");
        }

        var id = data.AnswerOrder[puzzleNumber % data.AnswerOrder.Count];
        var answer = data.Companies.FirstOrDefault(company => company.Id == id);
        if (answer is null)
        {
            throw new InvalidOperationException($"The answer order references unknown company '{id}'.");
        }

        return answer;
    }

    public static DailyPuzzle GetPuzzle(CompanyData data, int puzzleNumber) =>
        new(puzzleNumber, GetPuzzleDate(puzzleNumber), GetAnswer(data, puzzleNumber));
}

public sealed record DailyPuzzle(int Number, DateOnly Date, Company Answer);
