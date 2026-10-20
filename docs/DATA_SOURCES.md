# Company data sources

`src/Companydle/wwwroot/data/companies.json` is a hand-reviewed seed snapshot.
The file is bundled with the app and is never fetched from a third party at
runtime.

- Snapshot date: `2026-10-20`
- Currency: USD
- 50 guessable companies, 40 answer candidates, 10 non-candidate companies
- Rounding: 3 significant figures for money, employees and market cap;
  effective tax rates are stored to one decimal place.
- This is the phase-3 seed. Expansion to roughly 150-250 guessable companies
  (100-150 answer candidates) is intentionally deferred until the seed has been
  reviewed.
- Every record currently has `confidence: "medium"`. Financial statements are
  from primary SEC filings, but market caps and employee counts are secondary
  snapshots. A record is not marked `high` unless every field has been checked
  against a primary source.

## Financial statements

Annual income-statement figures (revenue, pre-tax income, income tax expense,
net profit, fiscal year) come from the SEC's XBRL data:

- US filers: `https://data.sec.gov/api/xbrl/companyfacts/CIK##########.json`,
  using the latest annual `10-K` period filed on or before the snapshot date.
- Foreign private issuers: the latest `20-F` filing. Where the company-facts
  API had not yet incorporated the newest 20-F, the annual values were read
  directly from the filing's XBRL instance (`*_htm.xml`) in the SEC EDGAR
  archive. Those filings are still primary sources.
- Filing index example:
  `https://www.sec.gov/Archives/edgar/data/1046179/000162828026025362/index.json`

Net profit is the amount attributable to the parent where the filing provides
that distinction; it can be negative. Revenue is total consolidated revenue as
reported, not a segment subtotal.

### Hand-checked exceptions

| Company | Treatment |
|---|---|
| Exxon Mobil | The SEC ticker map now points at a new holding-company CIK with no annual facts at build time. The FY2025 10-K from the existing Exxon Mobil CIK `34088` was used. |
| McDonald's | The filing does not tag a single total pre-tax line in company facts. Pre-tax income is domestic `$3.291B` + foreign `$7.606B` = `$10.897B`. |
| Oracle | Same issue: domestic `$8.693B` + foreign `$10.861B` = `$19.554B` pre-tax income. |
| TotalEnergies | Pre-tax income uses the IFRS `AccountingProfit` concept (`$22.449B`); income tax expense is `$9.092B`, giving an effective tax rate of 40.5%. |
| Sanofi | The 20-F does not tag `ProfitLossBeforeTax`. Pre-tax income is derived as `ProfitLoss` (`€7.851B`) + `IncomeTaxExpenseContinuingOperations` (`€1.043B`) = `€8.894B`, giving 11.7%. |
| TSMC | FY2025 values are from the 20-F XBRL instance's USD convenience translation: revenue `$121.4235B`, pre-tax `$65.083B`, tax `$11.0465B`, net profit `$54.1155B`. |
| Toyota | FY2026 20-F instance; JPY amounts converted at the FY average below. |
| Sony | FY2026 20-F instance. The group reports a net loss attributable to owners for the year (a large discontinued-operations loss), while pre-tax income and tax expense remain positive. The tax-rate field therefore uses the continuing-operations tax expense and pre-tax income. |
| Infosys | FY2026 20-F instance; reports in USD, so no FX conversion is needed. |
| Spotify | Reports in EUR; converted at the 2025 average. Spotify is a non-candidate seed company. |

## Exchange rates

Non-USD income statements were converted with these rates:

| Currency | Rate used | Source |
|---|---|---|
| EUR | `1.1306 USD/EUR` | IRS 2025 yearly average currency exchange rate |
| DKK | `6.6137 DKK/USD` | IRS 2025 yearly average currency exchange rate |
| JPY | `150.66772 JPY/USD` | Federal Reserve H.10 daily rates, average 2025-04-01 to 2026-03-31 |
| CNY | `7.1019104 CNY/USD` | Federal Reserve H.10 daily rates, average 2025-04-01 to 2026-03-31 |

TSMC and Infosys report in USD (or provide a USD convenience translation), so
they are not converted. Shell, BP, TotalEnergies, AstraZeneca and Novartis
report in USD.

## Secondary fields

Market capitalisation and employee counts are snapshots from the StockAnalysis
statistics pages (`https://stockanalysis.com/stocks/{ticker}/statistics/`),
accessed on 2026-10-20. They are rounded to three significant figures and can
change daily. Market cap is quoted in USD; employee counts are the latest
reported group totals.

Country, continent, sector, industry, exchange, founded year and aliases are
editorial fields. They were checked against company histories, annual-report
cover pages and exchange listings. Exchange is the primary listing, not a US
ADR listing. `founded` is the year of the current company or its direct
predecessor; merger and renaming dates can make a different year defensible.

Industry labels are deliberately coarser than full GICS sub-industries so that
the same-sector/same-industry comparison is understandable in a game. For
example, Microsoft, Adobe, Salesforce, Oracle, SAP and Shopify share the
`Software` industry; semiconductor companies share `Semiconductors`.

## Seed composition

The 40 answer candidates are a cross-section of large, well-known public
companies from the US, Europe and Asia. The 10 non-candidate companies broaden
the guessable pool and deliberately include edge cases:

- Spotify: European company with EUR statements and a US listing.
- Shopify: Canadian company with a US listing.
- Intel: net loss in the selected fiscal year.
- Broadcom and IBM: negative tax expense.
- Qualcomm: unusually high effective tax rate.
- Home Depot: January/February fiscal-year end with a different year label.
- Costco, Starbucks, Cisco: additional consumer/technology guesses.

## Building and updating

The JSON is static; there is no runtime API. To add companies:

1. Add the company to the JSON with the same fields and complete financial data.
2. Set `isAnswerCandidate: false` initially unless the data is fully reviewed.
3. Append new candidate ids to the existing `answerOrder`; never reorder or
   remove existing ids. Existing puzzle slots must not change.
4. Re-run the data validation tests.

The initial answer order was produced once by sorting candidate ids with
32-bit FNV-1a of `id + "|" + "companydle-v1"`, tie-breaking by id. The salt is
also stored in `GameRules.AnswerOrderSalt`. `tools/AnswerOrderGenerator` can
append missing candidates to an existing order without changing the prefix.

## Limitations

- Market caps are a point-in-time market snapshot, not an annual-report figure.
- Employee counts are approximate and may not be directly comparable across
  reporting regimes.
- Effective tax rates can be negative, unusually high, or affected by
  one-time items; they are calculated exactly as tax expense divided by
  pre-tax income, not normalised.
- Net profit can include discontinued operations. This is intentional; the
  game's net-profit tile handles negative values and requires matching signs
  for the yellow "close" state.
- Founded years and industry labels are editorial judgements, not audited
  financial facts.

## Company table

| Ticker | Company | Fiscal year | Period end | Financial source | Notes |
|---|---|---:|---|---|---|
| AAPL | Apple | 2025 | 2025-09-27 | SEC 10-K facts | |
| MSFT | Microsoft | 2026 | 2026-06-30 | SEC 10-K facts | |
| GOOGL | Alphabet | 2025 | 2025-12-31 | SEC 10-K facts | |
| AMZN | Amazon | 2025 | 2025-12-31 | SEC 10-K facts | |
| NVDA | NVIDIA | 2026 | 2026-01-25 | SEC 10-K facts | |
| META | Meta | 2025 | 2025-12-31 | SEC 10-K facts | |
| TSLA | Tesla | 2025 | 2025-12-31 | SEC 10-K facts | |
| V | Visa | 2025 | 2025-09-30 | SEC 10-K facts | |
| MA | Mastercard | 2025 | 2025-12-31 | SEC 10-K facts | |
| JNJ | Johnson & Johnson | 2025 | 2025-12-28 | SEC 10-K facts | |
| WMT | Walmart | 2026 | 2026-01-31 | SEC 10-K facts | |
| PG | Procter & Gamble | 2026 | 2026-06-30 | SEC 10-K facts | |
| XOM | Exxon Mobil | 2025 | 2025-12-31 | SEC 10-K facts | Old CIK 34088 |
| CVX | Chevron | 2025 | 2025-12-31 | SEC 10-K facts | |
| KO | Coca-Cola | 2025 | 2025-12-31 | SEC 10-K facts | |
| PEP | PepsiCo | 2025 | 2025-12-27 | SEC 10-K facts | |
| MCD | McDonald's | 2025 | 2025-12-31 | SEC 10-K facts | Pre-tax = domestic + foreign |
| NKE | Nike | 2026 | 2026-05-31 | SEC 10-K facts | |
| DIS | Disney | 2025 | 2025-09-27 | SEC 10-K facts | Negative tax expense |
| NFLX | Netflix | 2025 | 2025-12-31 | SEC 10-K facts | |
| ADBE | Adobe | 2025 | 2025-11-28 | SEC 10-K facts | |
| CRM | Salesforce | 2026 | 2026-01-31 | SEC 10-K facts | |
| ORCL | Oracle | 2026 | 2026-05-31 | SEC 10-K facts | Pre-tax = domestic + foreign |
| TXN | Texas Instruments | 2025 | 2025-12-31 | SEC 10-K facts | |
| AMD | AMD | 2025 | 2025-12-27 | SEC 10-K facts | Negative tax expense |
| SAP | SAP | 2025 | 2025-12-31 | SEC 20-F facts | EUR |
| ASML | ASML | 2025 | 2025-12-31 | SEC 20-F facts | EUR |
| AZN | AstraZeneca | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| NVO | Novo Nordisk | 2025 | 2025-12-31 | SEC 20-F facts | DKK |
| UL | Unilever | 2025 | 2025-12-31 | SEC 20-F facts | EUR |
| TTE | TotalEnergies | 2025 | 2025-12-31 | SEC 20-F facts | `AccountingProfit` pre-tax |
| SHEL | Shell | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| BP | BP | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| SNY | Sanofi | 2025 | 2025-12-31 | SEC 20-F facts | EUR; pre-tax derived |
| NVS | Novartis | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| TM | Toyota | 2026 | 2026-03-31 | SEC 20-F XBRL instance | JPY |
| SONY | Sony | 2026 | 2026-03-31 | SEC 20-F XBRL instance | JPY; net loss |
| TSM | TSMC | 2025 | 2025-12-31 | SEC 20-F XBRL instance | USD convenience translation |
| BABA | Alibaba | 2026 | 2026-03-31 | SEC 20-F facts | CNY |
| INFY | Infosys | 2026 | 2026-03-31 | SEC 20-F XBRL instance | USD reporting |
| SBUX | Starbucks | 2025 | 2025-09-28 | SEC 10-K facts | Non-candidate |
| COST | Costco | 2026 | 2026-08-30 | SEC 10-K facts | Non-candidate |
| HD | Home Depot | 2025 | 2026-02-01 | SEC 10-K facts | Non-candidate; January-end fiscal year |
| QCOM | Qualcomm | 2025 | 2025-09-28 | SEC 10-K facts | Non-candidate; high tax rate |
| AVGO | Broadcom | 2025 | 2025-11-02 | SEC 10-K facts | Non-candidate; negative tax expense |
| IBM | IBM | 2025 | 2025-12-31 | SEC 10-K facts | Non-candidate; negative tax expense |
| CSCO | Cisco | 2026 | 2026-07-25 | SEC 10-K facts | Non-candidate |
| SPOT | Spotify | 2025 | 2025-12-31 | SEC 20-F facts | Non-candidate; EUR |
| SHOP | Shopify | 2025 | 2025-12-31 | SEC 10-K facts | Non-candidate; USD reporting |
| INTC | Intel | 2025 | 2025-12-27 | SEC 10-K facts | Non-candidate; net loss |
