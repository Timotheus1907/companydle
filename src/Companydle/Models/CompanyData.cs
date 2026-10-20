namespace Companydle.Models;

/// <summary>
/// Root document in wwwroot/data/companies.json.
/// </summary>
public sealed class CompanyData
{
    public DateOnly DataAsOf { get; set; }
    public string Currency { get; set; } = string.Empty;
    public List<string> AnswerOrder { get; set; } = [];
    public List<Company> Companies { get; set; } = [];
}
