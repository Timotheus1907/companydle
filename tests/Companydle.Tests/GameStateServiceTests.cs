using System.Text.Json;
using Companydle;
using Companydle.Models;
using Companydle.Services;

namespace Companydle.Tests;

public class GameStateServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 20);
    private const int TodayPuzzle = 1;

    [Test]
    public async Task SaveAsync_WritesOneVersionedObject()
    {
        var storage = new InMemoryStateStorage();
        var service = new GameStateService(storage);
        var state = new GameState
        {
            Daily = new DailyProgress { PuzzleNumber = TodayPuzzle, Date = Today },
            Stats = new GameStats { Played = 1, Wins = 1 },
        };

        await service.SaveAsync(state);

        var raw = await storage.GetAsync(GameRules.StorageKey);
        Assert.That(raw, Is.Not.Null);
        using var document = JsonDocument.Parse(raw!);
        Assert.That(document.RootElement.GetProperty("v").GetInt32(), Is.EqualTo(1));
        Assert.That(document.RootElement.GetProperty("daily").GetProperty("puzzleNumber").GetInt32(), Is.EqualTo(1));
        Assert.That(document.RootElement.GetProperty("stats").GetProperty("played").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task LoadAsync_ResetsDailyButPreservesArchiveAndStats()
    {
        var storage = new InMemoryStateStorage();
        var first = new GameStateService(storage);
        await first.SaveAsync(new GameState
        {
            Daily = new DailyProgress
            {
                PuzzleNumber = 0,
                Date = Today.AddDays(-1),
                Guesses = ["aapl"],
                Won = true,
                StatsRecorded = true,
            },
            Archive = new Dictionary<int, ArchiveProgress>
            {
                [0] = new ArchiveProgress { PuzzleNumber = 0, Guesses = ["msft"] },
            },
            Stats = new GameStats
            {
                Played = 1,
                Wins = 1,
                CurrentStreak = 1,
                MaxStreak = 1,
                Distribution = [1, 0, 0, 0, 0, 0, 0, 0, 0],
                LastCompletedDate = Today.AddDays(-1),
            },
        });

        var second = new GameStateService(storage);
        var loaded = await second.LoadAsync(Today, TodayPuzzle);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Daily, Is.Not.Null);
            Assert.That(loaded.Daily!.PuzzleNumber, Is.EqualTo(TodayPuzzle));
            Assert.That(loaded.Daily.Date, Is.EqualTo(Today));
            Assert.That(loaded.Daily.Guesses, Is.Empty);
            Assert.That(loaded.Stats.Played, Is.EqualTo(1));
            Assert.That(loaded.Stats.Wins, Is.EqualTo(1));
            Assert.That(loaded.Archive[0].Guesses, Is.EquivalentTo(new[] { "msft" }));
        });
    }

    [Test]
    public async Task LoadAsync_CorruptJsonResetsSafely()
    {
        var storage = new InMemoryStateStorage();
        await storage.SetAsync(GameRules.StorageKey, "{ definitely not json");
        var service = new GameStateService(storage);

        var loaded = await service.LoadAsync(Today, TodayPuzzle);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Version, Is.EqualTo(1));
            Assert.That(loaded.Daily, Is.Not.Null);
            Assert.That(loaded.Daily!.PuzzleNumber, Is.EqualTo(TodayPuzzle));
            Assert.That(loaded.Daily.Guesses, Is.Empty);
            Assert.That(loaded.Stats.Played, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task LoadAsync_UnknownVersionResetsSafely()
    {
        var storage = new InMemoryStateStorage();
        await storage.SetAsync(
            GameRules.StorageKey,
            """{"v":99,"daily":{"puzzleNumber":1,"date":"2026-10-20","guesses":["aapl"]},"stats":{"played":5}}""");
        var service = new GameStateService(storage);

        var loaded = await service.LoadAsync(Today, TodayPuzzle);

        Assert.That(loaded.Version, Is.EqualTo(1));
        Assert.That(loaded.Daily!.Guesses, Is.Empty);
        Assert.That(loaded.Stats.Played, Is.EqualTo(0));
    }

    [Test]
    public async Task LoadAsync_NormalizesGuessesAndDistribution()
    {
        var storage = new InMemoryStateStorage();
        await storage.SetAsync(
            GameRules.StorageKey,
            """
            {
              "v": 1,
              "daily": {
                "puzzleNumber": 1,
                "date": "2026-10-20",
                "guesses": ["aapl", "aapl", "msft", "googl", "amzn", "meta", "nvda", "tsla", "intc", "ibm"],
                "won": false,
                "statsRecorded": false
              },
              "archive": {},
              "stats": {
                "played": 2,
                "wins": 3,
                "currentStreak": -1,
                "maxStreak": 0,
                "distribution": [1]
              }
            }
            """);
        var service = new GameStateService(storage);

        var loaded = await service.LoadAsync(Today, TodayPuzzle);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Daily!.Guesses, Has.Count.EqualTo(8));
            Assert.That(loaded.Daily.Guesses.Distinct().Count(), Is.EqualTo(loaded.Daily.Guesses.Count));
            Assert.That(loaded.Stats.Wins, Is.EqualTo(2));
            Assert.That(loaded.Stats.CurrentStreak, Is.EqualTo(0));
            Assert.That(loaded.Stats.Distribution, Has.Length.EqualTo(GameRules.MaxGuesses + 1));
            Assert.That(loaded.Stats.Distribution[0], Is.EqualTo(1));
        });
    }

    [Test]
    public async Task LoadAsync_NullGuessListsAreInitialized()
    {
        var storage = new InMemoryStateStorage();
        await storage.SetAsync(
            GameRules.StorageKey,
            """
            {
              "v": 1,
              "daily": {
                "puzzleNumber": 1,
                "date": "2026-10-20",
                "guesses": null,
                "won": false,
                "statsRecorded": false
              },
              "archive": {
                "0": {
                  "puzzleNumber": 0,
                  "guesses": null,
                  "won": false
                }
              },
              "stats": {
                "played": 0,
                "wins": 0,
                "currentStreak": 0,
                "maxStreak": 0,
                "distribution": [0,0,0,0,0,0,0,0,0]
              }
            }
            """);
        var service = new GameStateService(storage);

        var loaded = await service.LoadAsync(Today, TodayPuzzle);

        Assert.That(loaded.Daily!.Guesses, Is.Not.Null);
        Assert.That(loaded.Daily.Guesses, Is.Empty);
        Assert.That(loaded.Archive[0].Guesses, Is.Not.Null);
        Assert.That(loaded.Archive[0].Guesses, Is.Empty);
    }

    [Test]
    public async Task LoadAsync_FallsBackWhenStorageThrows()
    {
        var service = new GameStateService(new ThrowingStorage());

        var loaded = await service.LoadAsync(Today, TodayPuzzle);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Daily!.PuzzleNumber, Is.EqualTo(TodayPuzzle));
            Assert.That(service.IsPersistent, Is.False);
            Assert.That(service.StorageNotice, Is.EqualTo(GameStateService.StorageUnavailableNotice));
        });

        await service.SaveAsync(loaded);
        Assert.That(service.IsPersistent, Is.False);
    }

    [Test]
    public async Task SaveAsync_UsesInMemoryFallbackIfWriteFails()
    {
        var storage = new WriteThrowingStorage();
        var service = new GameStateService(storage);
        var loaded = await service.LoadAsync(Today, TodayPuzzle);

        await service.SaveAsync(loaded);

        Assert.That(service.IsPersistent, Is.False);
        Assert.That(service.StorageNotice, Is.EqualTo(GameStateService.StorageUnavailableNotice));
    }

    private sealed class ThrowingStorage : IStateStorage
    {
        public Task<string?> GetAsync(string key) =>
            throw new InvalidOperationException("Storage unavailable.");

        public Task SetAsync(string key, string value) =>
            throw new InvalidOperationException("Storage unavailable.");
    }

    private sealed class WriteThrowingStorage : IStateStorage
    {
        public Task<string?> GetAsync(string key) => Task.FromResult<string?>(null);

        public Task SetAsync(string key, string value) =>
            throw new InvalidOperationException("Storage full.");
    }
}
