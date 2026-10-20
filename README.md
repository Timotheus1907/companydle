# Companydle

A daily guess-the-company puzzle built with .NET 10 Blazor WebAssembly. It is a
static, client-only PWA designed for GitHub Pages at
`https://timotheus1907.github.io/companydle/` and works offline after the first
visit.

> **Disclaimer:** This project is quite different from the rest of my projects as it was generated completely with **DeepSeek-V4.1-Flash-1017**.

## Requirements

- .NET 10 SDK
- A modern browser with WebAssembly support
- No server, database, accounts or runtime third-party APIs

## Run locally

From the repository root:

```powershell
dotnet run --project src/Companydle/Companydle.csproj
```

Open **http://localhost:5088**. If port 5088 is already occupied, choose another
port:

```powershell
dotnet run --project src/Companydle/Companydle.csproj --urls http://localhost:5090
```

## Test

```powershell
dotnet test Companydle.sln -c Release
```

The NUnit suite covers the dataset, evaluator rules, puzzle selection,
formatting, share text, statistics and localStorage persistence.

## Publish

```powershell
dotnet publish src/Companydle/Companydle.csproj -c Release -o publish -p:GHPages=true -p:GHPagesBase=/companydle/
```

The committed `index.html` keeps `<base href="/" />` so local development works.
`PublishSPAforGitHubPages.Build` rewrites the published `<base>` to
`/companydle/`. For a manual PWA test, patch the published service worker and
serve the output under a `/companydle/` prefix:

```powershell
Remove-Item publish/wwwroot/service-worker.js.br, publish/wwwroot/service-worker.js.gz -ErrorAction SilentlyContinue
(Get-Content publish/wwwroot/service-worker.js -Raw).Replace('const base = "/";', 'const base = "/companydle/";') |
    Set-Content publish/wwwroot/service-worker.js -Encoding utf8
```

Then open the site once, go to **DevTools → Application → Service Workers**,
tick **Offline**, and reload. The app and company data should still load. The
deployment workflow performs the same service-worker patch automatically.

## Daily puzzles and stats

Puzzle `N` is the number of local calendar days since `LaunchDate` in
`src/Companydle/GameRules.cs` (`2026-10-19` in this build). The answer is
`answerOrder[N % answerOrder.Length]`.

The archive selector exposes earlier puzzles. Archive progress is stored
separately and never affects daily statistics. Daily and archive progress plus
stats use one versioned localStorage object under `companydle:state`; corrupt
state and blocked storage are handled safely.

## Data

`src/Companydle/wwwroot/data/companies.json` contains 150 companies: 100 answer
candidates and 50 non-candidates. Financial values are normalised to USD and
rounded to three significant figures. See
[`docs/DATA_SOURCES.md`](docs/DATA_SOURCES.md) for filing sources, FX rates and
data caveats.

`answerOrder` is append-only. To add companies, add complete records, mark
eligible answers with `isAnswerCandidate`, append their ids to `answerOrder`,
then run the tests and update the data-source documentation.

## Deployment

The workflow in `.github/workflows/deploy.yml` tests the solution, publishes
with the GitHub Pages base path, patches the service-worker base, and deploys
`publish/wwwroot`. GitHub Pages must be enabled for the repository.

## Project layout

- `src/Companydle/` — Blazor app, game services, components and bundled data
- `tests/Companydle.Tests/` — NUnit tests
- `docs/DATA_SOURCES.md` — data provenance and review notes
- `.github/workflows/deploy.yml` — GitHub Pages deployment
