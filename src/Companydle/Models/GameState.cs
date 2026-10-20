using System.Text.Json.Serialization;

namespace Companydle.Models;

/// <summary>
/// The single versioned object stored under companydle:state. Daily and archive
/// progress are deliberately separate so archive games never affect stats.
/// </summary>
public sealed class GameState
{
    [JsonPropertyName("v")]
    public int Version { get; set; } = GameRules.StateVersion;

    public DailyProgress? Daily { get; set; }

    public Dictionary<int, ArchiveProgress> Archive { get; set; } = [];

    public GameStats Stats { get; set; } = new();
}

public sealed class DailyProgress
{
    public int PuzzleNumber { get; set; }
    public DateOnly Date { get; set; }
    public List<string> Guesses { get; set; } = [];
    public bool Won { get; set; }
    public bool StatsRecorded { get; set; }

    [JsonIgnore]
    public bool IsFinished => Won || Guesses.Count >= GameRules.MaxGuesses;
}

public sealed class ArchiveProgress
{
    public int PuzzleNumber { get; set; }
    public List<string> Guesses { get; set; } = [];
    public bool Won { get; set; }

    [JsonIgnore]
    public bool IsFinished => Won || Guesses.Count >= GameRules.MaxGuesses;
}

public sealed class GameStats
{
    public int Played { get; set; }
    public int Wins { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }

    /// <summary>Eight win buckets (1-8 guesses) followed by the fail bucket.</summary>
    public int[] Distribution { get; set; } = new int[GameRules.MaxGuesses + 1];

    public DateOnly? LastCompletedDate { get; set; }
}
