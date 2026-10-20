namespace Companydle.Models;

/// <summary>
/// One guessable company. Financial values are normalised to USD and rounded
/// to three significant figures by the data build.
/// </summary>
public sealed class Company
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = [];
    public string Country { get; set; } = string.Empty;
    public string Continent { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public int Founded { get; set; }
    public int Employees { get; set; }
    public decimal RevenueUsd { get; set; }
    public decimal NetProfitUsd { get; set; }
    public decimal MarketCapUsd { get; set; }
    public decimal? EffectiveTaxRate { get; set; }
    public int FiscalYear { get; set; }
    public bool IsAnswerCandidate { get; set; }
    public string Confidence { get; set; } = string.Empty;
}
