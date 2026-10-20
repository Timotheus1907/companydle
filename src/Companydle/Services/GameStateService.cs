using System.Text.Json;
using Companydle.Models;

namespace Companydle.Services;

/// <summary>
/// Loads and saves the one versioned game-state object. The public API never
/// throws for storage or JSON failures: a corrupt/unknown state is reset and the
/// game continues with an in-memory fallback when browser storage is blocked.
/// </summary>
public sealed class GameStateService(IStateStorage storage)
{
    public const string StorageUnavailableNotice =
        "Local storage is unavailable. You can keep playing, but progress will be lost when this tab closes.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly InMemoryStateStorage _fallback = new();
    private GameState? _cached;
    private bool _usingFallback;

    public bool IsPersistent => !_usingFallback;
    public string? StorageNotice { get; private set; }

    public async Task<GameState> LoadAsync(DateOnly today, int todayPuzzleNumber)
    {
        if (_cached is null)
        {
            var raw = await ReadRawAsync();
            var reset = raw is null;

            GameState? state = null;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    state = JsonSerializer.Deserialize<GameState>(raw, JsonOptions);
                }
                catch (Exception)
                {
                    state = null;
                }
            }

            if (state is null || state.Version != GameRules.StateVersion)
            {
                state = new GameState();
                reset = true;
            }

            reset |= Normalize(state);
            _cached = state;
            if (reset || raw is null)
            {
                await SaveAsync(state);
            }
        }

        var changed = EnsureDaily(_cached, today, todayPuzzleNumber);
        changed |= StatsCalculator.ResetMissedStreak(_cached.Stats, today);
        if (changed)
        {
            await SaveAsync(_cached);
        }

        return _cached;
    }

    public async Task SaveAsync(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _cached = state;
        state.Version = GameRules.StateVersion;

        string json;
        try
        {
            json = JsonSerializer.Serialize(state, JsonOptions);
        }
        catch (Exception)
        {
            StorageNotice ??= StorageUnavailableNotice;
            return;
        }

        if (_usingFallback)
        {
            await SaveFallbackAsync(json);
            return;
        }

        try
        {
            await storage.SetAsync(GameRules.StorageKey, json);
        }
        catch (Exception)
        {
            EnableFallback();
            await SaveFallbackAsync(json);
        }
    }

    private static bool EnsureDaily(GameState state, DateOnly today, int todayPuzzleNumber)
    {
        if (state.Daily is not null
            && state.Daily.PuzzleNumber == todayPuzzleNumber
            && state.Daily.Date == today)
        {
            return false;
        }

        state.Daily = new DailyProgress
        {
            PuzzleNumber = todayPuzzleNumber,
            Date = today,
        };
        return true;
    }

    private static bool Normalize(GameState state)
    {
        var changed = false;

        state.Archive ??= [];
        state.Stats ??= new GameStats();

        if (state.Daily is not null)
        {
            if (state.Daily.Guesses is null)
            {
                state.Daily.Guesses = [];
                changed = true;
            }
            else
            {
                changed |= NormalizeGuesses(state.Daily.Guesses);
            }
        }

        var normalizedArchive = new Dictionary<int, ArchiveProgress>();
        foreach (var (key, progress) in state.Archive)
        {
            if (progress is null)
            {
                changed = true;
                continue;
            }

            if (progress.PuzzleNumber != key)
            {
                progress.PuzzleNumber = key;
                changed = true;
            }

            if (progress.Guesses is null)
            {
                progress.Guesses = [];
                changed = true;
            }
            else
            {
                changed |= NormalizeGuesses(progress.Guesses);
            }

            normalizedArchive[key] = progress;
        }

        if (normalizedArchive.Count != state.Archive.Count)
        {
            changed = true;
        }
        state.Archive = normalizedArchive;

        var stats = state.Stats;
        if (stats.Played < 0)
        {
            stats.Played = 0;
            changed = true;
        }
        if (stats.Wins < 0 || stats.Wins > stats.Played)
        {
            stats.Wins = Math.Clamp(stats.Wins, 0, stats.Played);
            changed = true;
        }
        if (stats.CurrentStreak < 0)
        {
            stats.CurrentStreak = 0;
            changed = true;
        }
        if (stats.MaxStreak < stats.CurrentStreak)
        {
            stats.MaxStreak = stats.CurrentStreak;
            changed = true;
        }

        var expectedDistributionLength = GameRules.MaxGuesses + 1;
        if (stats.Distribution is null || stats.Distribution.Length != expectedDistributionLength)
        {
            var normalizedDistribution = new int[expectedDistributionLength];
            if (stats.Distribution is not null)
            {
                Array.Copy(
                    stats.Distribution,
                    normalizedDistribution,
                    Math.Min(stats.Distribution.Length, normalizedDistribution.Length));
            }
            stats.Distribution = normalizedDistribution;
            changed = true;
        }

        for (var i = 0; i < stats.Distribution.Length; i++)
        {
            if (stats.Distribution[i] < 0)
            {
                stats.Distribution[i] = 0;
                changed = true;
            }
        }

        if (state.Version != GameRules.StateVersion)
        {
            state.Version = GameRules.StateVersion;
            changed = true;
        }

        return changed;
    }

    private static bool NormalizeGuesses(List<string>? guesses)
    {
        if (guesses is null)
        {
            return true;
        }

        var normalized = guesses
            .Where(guess => !string.IsNullOrWhiteSpace(guess))
            .Distinct(StringComparer.Ordinal)
            .Take(GameRules.MaxGuesses)
            .ToList();

        if (normalized.Count == guesses.Count && normalized.SequenceEqual(guesses, StringComparer.Ordinal))
        {
            return false;
        }

        guesses.Clear();
        guesses.AddRange(normalized);
        return true;
    }

    private async Task<string?> ReadRawAsync()
    {
        if (_usingFallback)
        {
            return await ReadFallbackAsync();
        }

        try
        {
            return await storage.GetAsync(GameRules.StorageKey);
        }
        catch (Exception)
        {
            EnableFallback();
            return await ReadFallbackAsync();
        }
    }

    private async Task<string?> ReadFallbackAsync()
    {
        try
        {
            return await _fallback.GetAsync(GameRules.StorageKey);
        }
        catch (Exception)
        {
            StorageNotice ??= StorageUnavailableNotice;
            return null;
        }
    }

    private async Task SaveFallbackAsync(string json)
    {
        try
        {
            await _fallback.SetAsync(GameRules.StorageKey, json);
        }
        catch (Exception)
        {
            StorageNotice ??= StorageUnavailableNotice;
        }
    }

    private void EnableFallback()
    {
        _usingFallback = true;
        StorageNotice ??= StorageUnavailableNotice;
    }
}
