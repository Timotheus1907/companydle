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

## Daily puzzle and archive

Puzzle number `N` is the number of local calendar days since
`GameRules.LaunchDate` (`2026-10-19` in this build). Puzzle `0` is launch day;
puzzle `1` is `2026-10-20`. The answer is `answerOrder[N % answerOrder.Length]`.
To set a new launch date, change `LaunchDate` before the first public puzzle and
update the documented date and `dataAsOf` with it.

The archive selector exposes puzzle numbers `0` through `N - 1` (newest first).
Archive progress is stored under its own archive entry and never updates daily
statistics.

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
dotnet publish src/Companydle/Companydle.csproj -c Release -o publish -p:GHPages=true -p:GHPagesBase=/companydle/
```

The project uses `PublishSPAforGitHubPages.Build` to rewrite the published
`<base href>` to `/companydle/`. The committed source keeps `<base href="/" />`
so local development works without the GitHub Pages base path. The workflow also
patches the published service worker's `const base` value; see below.

The generated `wwwroot/service-worker.published.js` handles the project-site
scope and precaches the bundled company data. No external API calls are made at
runtime. When a new service worker is installed, an in-app update banner offers
a reload; the new worker is activated with `SKIP_WAITING` and the page reloads
once on `controllerchange`.

## PWA and offline testing

Development builds use a deliberate no-op service worker, so offline caching
must be tested against the published output:

1. Publish with `dotnet publish src/Companydle/Companydle.csproj -c Release -o publish -p:GHPages=true -p:GHPagesBase=/companydle/`.
2. For a manual local test, change `const base = "/";` to
   `const base = "/companydle/";` in `publish/wwwroot/service-worker.js` (the
   deploy workflow does this automatically), then serve `publish/wwwroot` under
   a `/companydle/` URL prefix.
3. Open the site once and confirm the service worker scope is
   `/companydle/` and the manifest and icons load.
4. In DevTools → Application → Service Workers, tick **Offline** and reload. The
   game and company data should still load.
5. Deploy a new build to see the update banner; click **Reload** to activate it.

## Data

`src/Companydle/wwwroot/data/companies.json` contains 150 companies: 100 answer
candidates and 50 non-candidates. Financial values are normalised to USD and
rounded to three significant figures. See
[`docs/DATA_SOURCES.md`](docs/DATA_SOURCES.md) for SEC filing sources, exchange
rates, overrides and data limitations.

`answerOrder` is append-only. Existing answer slots must never be reordered or
removed. `tools/AnswerOrderGenerator` can append new candidate IDs using the
stable FNV-1a ordering while preserving the existing prefix. When editing data:

1. Add the company with all required fields, mark candidates explicitly and use
   `medium` confidence unless every field has been checked against a primary
   source.
2. Append candidate IDs to `answerOrder`; never insert them in the middle.
3. Run `dotnet test Companydle.sln -c Release` and update
   `docs/DATA_SOURCES.md` for new filing periods or exceptions.

If the app is already deployed, changing the logic or data changes the service
worker asset hash. The next visit installs the new worker and shows the update
banner.

## Deployment

The checked-in workflow at `.github/workflows/deploy.yml` runs on pushes to
`main` and manual dispatches. It tests, publishes with
`-p:GHPages=true -p:GHPagesBase=/companydle/`, patches the published service
worker base to `/companydle/`, and deploys `publish/wwwroot` with GitHub Pages.
The repository must have Pages enabled for that workflow to publish.

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
