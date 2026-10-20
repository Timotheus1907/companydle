# Company data sources

`src/Companydle/wwwroot/data/companies.json` is a hand-reviewed seed snapshot.
The file is bundled with the app and is never fetched from a third party at
runtime.

- Snapshot date: `2026-10-20`
- Currency: USD
- 150 guessable companies, 100 answer candidates, 50 non-candidate companies
- Rounding: 3 significant figures for money, employees and market cap;
  effective tax rates are stored to one decimal place.
- The original 50-company seed was expanded with 100 additional companies. The
  original 40 answer candidates remain first in `answerOrder`; the ten existing
  non-candidates were promoted, and the 50 new candidates were appended. This
  keeps every already-published or previous archive puzzle stable.
- Every record has `confidence: "medium"`. Financial statements are from primary
  SEC filings, but market caps and employee counts are secondary snapshots. A
  record is not marked `high` unless every field has been checked against a
  primary source.

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
- The expanded dataset uses latest annual periods filed on or before
  `2026-10-20`. Some foreign filers have a fiscal year that ends earlier than
  their filing date; the period end and fiscal-year label are shown per company
  in the table below.

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
| Spotify | Reports in EUR; converted at the 2025 average. Promoted to a candidate in the expansion. |
| GSK | FY2025 20-F XBRL instance. Reports in GBP; converted at `1.3192 USD/GBP`. Revenue `£32.667B`, pre-tax `£7.401B`, tax `£1.112B`, net profit `£5.716B`. |
| HSBC | Pre-tax income and tax expense are `$29.907B` and `$6.776B`; net profit uses profit attributable to ordinary equity holders (`$21.102B`). |
| UBS | Net profit uses profit attributable to owners (`$7.767B`); the group total profit line includes non-controlling interests. |
| BHP | FY2026 20-F XBRL instance. Revenue `$58.760B`, pre-tax `$22.414B`, tax `$9.388B`, net profit attributable to owners `$9.833B`. |
| HDFC Bank | FY2026 20-F XBRL instance, USD convenience translation. Revenue `$24.804B`, tax `$2.259B`, net profit `$7.511B`; pre-tax income is derived as net profit + tax = `$9.770B`. |
| Petrobras | FY2025 20-F XBRL instance. Revenue `$89.195B`, pre-tax `$26.795B`, tax `$7.075B`, net profit `$19.634B`. |
| Wipro | FY2026 20-F XBRL instance. Revenue `$9.871B`, pre-tax `$1.848B`, tax `$0.434B`, net profit `$1.407B`. |
| Simon Property Group | The generic facts picker did not expose a total pre-tax line. Pre-tax income is derived as net profit + tax expense. |
| ITW, PayPal, Phillips 66 | The same net-profit + tax derivation is used where the filing does not tag a consolidated pre-tax total. |
| Booking Holdings | Net profit is the `NetIncomeLossAvailableToCommonStockholdersBasic` fallback because the consolidated `NetIncomeLoss` tag was not present in the selected facts. |

## Exchange rates

Non-USD income statements were converted with these rates:

| Currency | Rate used | Source |
|---|---|---|
| EUR | `1.1306 USD/EUR` | IRS 2025 yearly average currency exchange rate |
| DKK | `6.6137 DKK/USD` | IRS 2025 yearly average currency exchange rate |
| GBP | `1.3192 USD/GBP` | Federal Reserve H.10 2025 average |
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

## Dataset composition

The original 40 answer candidates were a cross-section of large, well-known
public companies from the US, Europe and Asia. The expansion added 60 more
candidates: the ten former non-candidates were promoted, and 50 new companies
were appended to `answerOrder`. The result is 100 answer candidates.

The 50 non-candidates broaden the guessable pool and deliberately include edge
cases:

- Spotify: European company with EUR statements and a US listing; promoted to a
  candidate in the expansion.
- Shopify: Canadian company with a US listing; promoted to a candidate.
- Intel: net loss in the selected fiscal year; promoted to a candidate.
- Broadcom and IBM: negative tax expense; promoted to candidates.
- Qualcomm: unusually high effective tax rate; promoted to a candidate.
- Home Depot: January/February fiscal-year end with a different year label;
  promoted to a candidate.
- Costco, Starbucks, Cisco: additional consumer/technology guesses; promoted to
  candidates.
- The new non-candidates include loss-making automakers (Ford, Rivian, Lucid),
  media and gaming companies, retailers, industrials, REITs and foreign issuers
  such as Equinor, Vale, Petrobras, JD.com, Baidu, NetEase, Trip.com, Yum China,
  ZTO Express and Wipro.

The 100 candidate IDs are all present in `answerOrder`. The prefix of the order
is never rewritten; this is what keeps daily and archive answers stable.

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
| SBUX | Starbucks | 2025 | 2025-09-28 | SEC 10-K facts | Promoted to candidate in expansion |
| COST | Costco | 2026 | 2026-08-30 | SEC 10-K facts | Promoted to candidate in expansion |
| HD | Home Depot | 2025 | 2026-02-01 | SEC 10-K facts | Promoted to candidate; January-end fiscal year |
| QCOM | Qualcomm | 2025 | 2025-09-28 | SEC 10-K facts | Promoted to candidate; high tax rate |
| AVGO | Broadcom | 2025 | 2025-11-02 | SEC 10-K facts | Promoted to candidate; negative tax expense |
| IBM | IBM | 2025 | 2025-12-31 | SEC 10-K facts | Promoted to candidate; negative tax expense |
| CSCO | Cisco | 2026 | 2026-07-25 | SEC 10-K facts | Promoted to candidate in expansion |
| SPOT | Spotify | 2025 | 2025-12-31 | SEC 20-F facts | Promoted to candidate; EUR |
| SHOP | Shopify | 2025 | 2025-12-31 | SEC 10-K facts | Promoted to candidate; USD reporting |
| INTC | Intel | 2025 | 2025-12-27 | SEC 10-K facts | Promoted to candidate; net loss |
| JPM | JPMorgan Chase | 2025 | 2025-12-31 | SEC 10-K facts |  |
| BAC | Bank of America | 2025 | 2025-12-31 | SEC 10-K facts |  |
| WFC | Wells Fargo | 2025 | 2025-12-31 | SEC 10-K facts |  |
| GS | Goldman Sachs | 2025 | 2025-12-31 | SEC 10-K facts |  |
| MS | Morgan Stanley | 2025 | 2025-12-31 | SEC 10-K facts |  |
| BLK | BlackRock | 2025 | 2025-12-31 | SEC 10-K facts |  |
| AXP | American Express | 2025 | 2025-12-31 | SEC 10-K facts |  |
| UNH | UnitedHealth Group | 2025 | 2025-12-31 | SEC 10-K facts |  |
| LLY | Eli Lilly | 2025 | 2025-12-31 | SEC 10-K facts |  |
| PFE | Pfizer | 2025 | 2025-12-31 | SEC 10-K facts | Negative tax expense |
| MRK | Merck | 2025 | 2025-12-31 | SEC 10-K facts |  |
| ABBV | AbbVie | 2025 | 2025-12-31 | SEC 10-K facts |  |
| AMGN | Amgen | 2025 | 2025-12-31 | SEC 10-K facts |  |
| TMO | Thermo Fisher Scientific | 2025 | 2025-12-31 | SEC 10-K facts |  |
| ABT | Abbott Laboratories | 2025 | 2025-12-31 | SEC 10-K facts |  |
| CAT | Caterpillar | 2025 | 2025-12-31 | SEC 10-K facts |  |
| BA | Boeing | 2025 | 2025-12-31 | SEC 10-K facts |  |
| GE | GE Aerospace | 2025 | 2025-12-31 | SEC 10-K facts |  |
| HON | Honeywell | 2025 | 2025-12-31 | SEC 10-K facts |  |
| UPS | UPS | 2025 | 2025-12-31 | SEC 10-K facts |  |
| FDX | FedEx | 2026 | 2026-05-31 | SEC 10-K facts |  |
| RTX | RTX | 2025 | 2025-12-31 | SEC 10-K facts |  |
| LMT | Lockheed Martin | 2025 | 2025-12-31 | SEC 10-K facts |  |
| COP | ConocoPhillips | 2025 | 2025-12-31 | SEC 10-K facts |  |
| VZ | Verizon | 2025 | 2025-12-31 | SEC 10-K facts |  |
| T | AT&T | 2025 | 2025-12-31 | SEC 10-K facts |  |
| TMUS | T-Mobile US | 2025 | 2025-12-31 | SEC 10-K facts |  |
| CMCSA | Comcast | 2025 | 2025-12-31 | SEC 10-K facts |  |
| NEE | NextEra Energy | 2025 | 2025-12-31 | SEC 10-K facts | Negative effective tax rate |
| LIN | Linde | 2025 | 2025-12-31 | SEC 10-K facts | Irish incorporation; treated as UK/Europe editorially |
| SHW | Sherwin-Williams | 2025 | 2025-12-31 | SEC 10-K facts |  |
| PLD | Prologis | 2025 | 2025-12-31 | SEC 10-K facts |  |
| AMT | American Tower | 2025 | 2025-12-31 | SEC 10-K facts |  |
| EQIX | Equinix | 2025 | 2025-12-31 | SEC 10-K facts |  |
| PYPL | PayPal | 2025 | 2025-12-31 | SEC 10-K facts | Pre-tax = net + tax |
| SPGI | S&P Global | 2025 | 2025-12-31 | SEC 10-K facts |  |
| MCO | Moody's | 2025 | 2025-12-31 | SEC 10-K facts |  |
| CME | CME Group | 2025 | 2025-12-31 | SEC 10-K facts |  |
| ICE | Intercontinental Exchange | 2025 | 2025-12-31 | SEC 10-K facts |  |
| MDLZ | Mondelez International | 2025 | 2025-12-31 | SEC 10-K facts |  |
| NOW | ServiceNow | 2025 | 2025-12-31 | SEC 10-K facts |  |
| GSK | GSK | 2025 | 2025-12-31 | SEC 20-F XBRL instance | GBP; converted at 1.3192 USD/GBP |
| HSBC | HSBC | 2025 | 2025-12-31 | SEC 20-F facts | Net profit attributable to ordinary shareholders |
| RIO | Rio Tinto | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| BHP | BHP | 2026 | 2026-06-30 | SEC 20-F XBRL instance | FY2026 instance; net attributable to owners |
| UBS | UBS | 2025 | 2025-12-31 | SEC 20-F facts | Net profit attributable to owners |
| MUFG | Mitsubishi UFJ Financial | 2026 | 2026-03-31 | SEC 20-F facts | JPY; converted at 150.66772 JPY/USD |
| HDB | HDFC Bank | 2026 | 2026-03-31 | SEC 20-F XBRL instance | FY2026 instance; USD convenience translation; pre-tax derived as net + tax |
| PDD | PDD Holdings | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| ACN | Accenture | 2026 | 2026-08-31 | SEC 10-K facts | August fiscal-year end |
| CHTR | Charter Communications | 2025 | 2025-12-31 | SEC 10-K facts |  |
| EA | Electronic Arts | 2026 | 2026-03-31 | SEC 10-K facts |  |
| TTWO | Take-Two Interactive | 2026 | 2026-03-31 | SEC 10-K facts |  |
| WBD | Warner Bros. Discovery | 2025 | 2025-12-31 | SEC 10-K facts |  |
| FOXA | Fox | 2026 | 2026-06-30 | SEC 10-K facts | Fox Corporation Class A |
| NWSA | News Corp | 2026 | 2026-06-30 | SEC 10-K facts | News Corporation Class A |
| F | Ford | 2025 | 2025-12-31 | SEC 10-K facts |  |
| GM | General Motors | 2025 | 2025-12-31 | SEC 10-K facts |  |
| RIVN | Rivian | 2025 | 2025-12-31 | SEC 10-K facts |  |
| LCID | Lucid Group | 2025 | 2025-12-31 | SEC 10-K facts |  |
| TGT | Target | 2025 | 2026-01-31 | SEC 10-K facts |  |
| LOW | Lowe's | 2025 | 2026-01-30 | SEC 10-K facts |  |
| TJX | TJX Companies | 2025 | 2026-01-31 | SEC 10-K facts |  |
| BKNG | Booking Holdings | 2025 | 2025-12-31 | SEC 10-K facts | Net income available to common shareholders fallback |
| ABNB | Airbnb | 2025 | 2025-12-31 | SEC 10-K facts |  |
| MAR | Marriott International | 2025 | 2025-12-31 | SEC 10-K facts |  |
| HLT | Hilton Worldwide | 2025 | 2025-12-31 | SEC 10-K facts |  |
| YUM | Yum! Brands | 2025 | 2025-12-31 | SEC 10-K facts |  |
| CMG | Chipotle Mexican Grill | 2025 | 2025-12-31 | SEC 10-K facts |  |
| DPZ | Domino's Pizza | 2025 | 2025-12-28 | SEC 10-K facts |  |
| DE | Deere | 2025 | 2025-11-02 | SEC 10-K facts |  |
| MMM | 3M | 2025 | 2025-12-31 | SEC 10-K facts |  |
| UNP | Union Pacific | 2025 | 2025-12-31 | SEC 10-K facts |  |
| CSX | CSX | 2025 | 2025-12-31 | SEC 10-K facts |  |
| NSC | Norfolk Southern | 2025 | 2025-12-31 | SEC 10-K facts |  |
| ETN | Eaton | 2025 | 2025-12-31 | SEC 10-K facts |  |
| EMR | Emerson Electric | 2025 | 2025-09-30 | SEC 10-K facts |  |
| ITW | Illinois Tool Works | 2025 | 2025-12-31 | SEC 10-K facts | Pre-tax = net + tax |
| PH | Parker Hannifin | 2026 | 2026-06-30 | SEC 10-K facts |  |
| CMI | Cummins | 2025 | 2025-12-31 | SEC 10-K facts |  |
| SLB | SLB | 2025 | 2025-12-31 | SEC 10-K facts |  |
| HAL | Halliburton | 2025 | 2025-12-31 | SEC 10-K facts |  |
| BKR | Baker Hughes | 2025 | 2025-12-31 | SEC 10-K facts |  |
| OXY | Occidental Petroleum | 2025 | 2025-12-31 | SEC 10-K facts |  |
| PSX | Phillips 66 | 2025 | 2025-12-31 | SEC 10-K facts | Pre-tax = net + tax |
| MPC | Marathon Petroleum | 2025 | 2025-12-31 | SEC 10-K facts |  |
| VLO | Valero Energy | 2025 | 2025-12-31 | SEC 10-K facts |  |
| O | Realty Income | 2025 | 2025-12-31 | SEC 10-K facts |  |
| SPG | Simon Property Group | 2025 | 2025-12-31 | SEC 10-K facts | Pre-tax = net + tax |
| DLR | Digital Realty | 2025 | 2025-12-31 | SEC 10-K facts |  |
| EQNR | Equinor | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting; high effective tax rate |
| VALE | Vale | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting; high effective tax rate |
| PBR | Petrobras | 2025 | 2025-12-31 | SEC 20-F XBRL instance | FY2025 instance |
| JD | JD.com | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| BIDU | Baidu | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| NTES | NetEase | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| TCOM | Trip.com Group | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| YUMC | Yum China | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| ZTO | ZTO Express | 2025 | 2025-12-31 | SEC 20-F facts | USD reporting |
| WIT | Wipro | 2026 | 2026-03-31 | SEC 20-F XBRL instance | FY2026 instance |
