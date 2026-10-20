# Companydle

A static, client-only daily company-guessing puzzle built with .NET 10 Blazor
WebAssembly. The app is designed for GitHub Pages project-site hosting at
`https://timotheus1907.github.io/companydle/` and works offline after the first
visit.

## Game rules

- Guess the hidden public company in eight tries.
- Search by company name, ticker or alias.
- Every guess compares ten attributes:
  country, sector, industry, founded year, employees, revenue, net profit,
  market cap, effective tax rate and exchange.
- Green means an exact match. Yellow means close:
  - same continent for country;
  - same sector for industry;
  - within 10 years for founded;
  - within 5% for numeric values (green) or 25% (yellow);
  - within 1 percentage point for tax (green) or 5 points (yellow).
- Net profit also requires matching signs for green/yellow feedback.
- Numeric and founded tiles show an arrow pointing toward the answer.
- Hints appear after guesses 4 (first letter) and 6 (word count).
- A new daily puzzle is based on the player's local date.
- Previous puzzles are available in the archive selector. Archive progress is
  saved separately and never changes daily statistics.

## Statistics

The Stats panel shows daily-only played count, win percentage, current streak,
maximum streak and a nine-bucket guess distribution (eight win buckets plus a
fail bucket). Daily statistics are updated once when a daily puzzle is finished.
Archive games are stored separately and do not affect them.

Game state is stored as one versioned JSON object under the `companydle:state`
localStorage key. Daily reset preserves statistics and archive progress. Corrupt
or unknown-version data is reset safely. If localStorage is blocked, the game
continues with an in-memory fallback and displays a notice.

## Requirements

- .NET 10 SDK
- A modern browser with WebAssembly support
- No server, database, accounts or runtime third-party APIs

## Run locally

```powershell
dotnet run --project src/Companydle/Companydle.csproj
```

The development profile listens on `http://localhost:5088`.

## Test

```powershell
dotnet test Companydle.sln -c Release
```

The NUnit tests cover the dataset schema, evaluator thresholds, number
formatting, puzzle selection, answer-order stability, statistics, share text and
state persistence/fallback behavior.

## Publish

```powershell
dotnet publish src/Companydle/Companydle.csproj -c Release
```

The project uses `PublishSPAforGitHubPages.Build` to rewrite the published
`<base href>` to `/companydle/`. The committed source keeps `<base href="/" />`
so local development works without the GitHub Pages base path.

The generated `wwwroot/service-worker.published.js` handles the project-site
scope and precaches the bundled company data. No external API calls are made at
runtime.

## Data

`src/Companydle/wwwroot/data/companies.json` contains 150 companies: 100 answer
candidates and 50 non-candidates. Financial values are normalised to USD and
rounded to three significant figures. See
[`docs/DATA_SOURCES.md`](docs/DATA_SOURCES.md) for SEC filing sources, exchange
rates, overrides and data limitations.

`answerOrder` is append-only. Existing answer slots must never be reordered or
removed. `tools/AnswerOrderGenerator` can append new candidate IDs using the
stable FNV-1a ordering while preserving the existing prefix.

## Project layout

- `src/Companydle/Pages/Home.razor` — daily/archive game, search and share UI
- `src/Companydle/Components/` — guess input, attribute tiles and stats panel
- `src/Companydle/Services/` — evaluator, puzzle selection, formatting,
  persistence and statistics
- `src/Companydle/wwwroot/data/companies.json` — bundled dataset
- `tests/Companydle.Tests/` — NUnit test suite
- `docs/DATA_SOURCES.md` — data provenance and review notes

## Status

This repository contains the local build of the game. Deployment is handled
separately after GitHub Pages is enabled for the repository.
