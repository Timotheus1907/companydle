using Companydle.Models;

namespace Companydle.Services;

/// <summary>
/// Builds the Wordle-style share payload: one row of ten squares per guess,
/// followed by the game URL.
/// </summary>
public static class ShareTextBuilder
{
    public static string Build(
        int puzzleNumber,
        bool won,
        IReadOnlyList<GuessResult> guesses,
        string url,
        bool isArchive = false)
    {
        ArgumentNullException.ThrowIfNull(guesses);
        ArgumentNullException.ThrowIfNull(url);

        var score = won ? $"{guesses.Count}/{GameRules.MaxGuesses}" : $"X/{GameRules.MaxGuesses}";
        var title = isArchive
            ? $"Companydle Archive #{puzzleNumber} {score}"
            : $"Companydle #{puzzleNumber} {score}";

        var lines = new List<string> { title, string.Empty };
        lines.AddRange(guesses.Select(guess => guess.EmojiRow));
        if (!string.IsNullOrWhiteSpace(url))
        {
            lines.Add(string.Empty);
            lines.Add(url.Trim());
        }

        return string.Join("\n", lines);
    }
}
