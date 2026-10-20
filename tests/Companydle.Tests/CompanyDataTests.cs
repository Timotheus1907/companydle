using System.Text.Json;
using Companydle.Models;

namespace Companydle.Tests;

public class CompanyDataTests
{
    private static readonly Lazy<CompanyData> LazyData = new(LoadData);

    private static CompanyData Data => LazyData.Value;

    private static readonly Dictionary<string, string> ExpectedContinents = new(StringComparer.Ordinal)
    {
        ["United States"] = "North America",
        ["Canada"] = "North America",
        ["Germany"] = "Europe",
        ["Netherlands"] = "Europe",
        ["United Kingdom"] = "Europe",
        ["Denmark"] = "Europe",
        ["France"] = "Europe",
        ["Switzerland"] = "Europe",
        ["Sweden"] = "Europe",
        ["Japan"] = "Asia",
        ["Taiwan"] = "Asia",
        ["China"] = "Asia",
        ["India"] = "Asia",
    };

    private static CompanyData LoadData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "companies.json");
        Assert.That(File.Exists(path), Is.True, $"Company data file not found at {path}.");

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Deserialize<CompanyData>(json, options)
            ?? throw new InvalidOperationException("Company data file did not deserialize.");
    }

    [Test]
    public void DataFile_HasExpectedShape()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Data.DataAsOf, Is.Not.EqualTo(default(DateOnly)));
            Assert.That(Data.Currency, Is.EqualTo("USD"));
            Assert.That(Data.Companies, Is.Not.Empty);
            Assert.That(Data.Companies.Count, Is.GreaterThanOrEqualTo(50));
            Assert.That(Data.Companies.Count(company => company.IsAnswerCandidate), Is.GreaterThanOrEqualTo(40));
            Assert.That(Data.AnswerOrder, Is.Not.Empty);
        });
    }

    [Test]
    public void CompanyIds_AreUnique()
    {
        var ids = Data.Companies.Select(company => company.Id).ToList();
        Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(ids.Count));
    }

    [Test]
    public void RequiredFields_ArePresentAndPlausible()
    {
        foreach (var company in Data.Companies)
        {
            Assert.Multiple(() =>
            {
                Assert.That(company.Id, Does.Match("^[a-z0-9-]+$"), company.Name);
                Assert.That(company.Name, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(company.Aliases, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(company.Country, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(company.Continent, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(company.Sector, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(company.Industry, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(company.Exchange, Is.Not.Null.And.Not.Empty, company.Id);
                Assert.That(new[] { "high", "medium", "low" }, Does.Contain(company.Confidence), company.Id);
                Assert.That(company.Founded, Is.InRange(1800, Data.DataAsOf.Year), company.Id);
                Assert.That(company.Employees, Is.InRange(1_000, 5_000_000), company.Id);
                Assert.That(company.RevenueUsd, Is.GreaterThan(0m), company.Id);
                Assert.That(company.NetProfitUsd, Is.GreaterThan(-200_000_000_000m), company.Id);
                Assert.That(company.MarketCapUsd, Is.GreaterThan(0m), company.Id);
                Assert.That(company.FiscalYear, Is.InRange(2000, Data.DataAsOf.Year), company.Id);

                if (company.EffectiveTaxRate is not null)
                {
                    Assert.That(company.EffectiveTaxRate.Value, Is.InRange(-100m, 200m), company.Id);
                }
            });
        }
    }

    [Test]
    public void Countries_HaveConsistentContinents()
    {
        foreach (var company in Data.Companies)
        {
            Assert.That(ExpectedContinents.TryGetValue(company.Country, out var continent), Is.True, company.Id);
            Assert.That(company.Continent, Is.EqualTo(continent), company.Id);
        }
    }

    [Test]
    public void AnswerOrder_ContainsEveryCandidateExactlyOnce()
    {
        var candidateIds = Data.Companies
            .Where(company => company.IsAnswerCandidate)
            .Select(company => company.Id)
            .ToHashSet(StringComparer.Ordinal);

        Assert.That(Data.AnswerOrder.Count, Is.EqualTo(Data.AnswerOrder.Distinct(StringComparer.Ordinal).Count()));
        Assert.That(Data.AnswerOrder, Is.EquivalentTo(candidateIds));
    }

    [Test]
    public void AnswerCandidates_HaveNonNullTaxRateAndAreNotLowConfidence()
    {
        foreach (var company in Data.Companies.Where(company => company.IsAnswerCandidate))
        {
            Assert.Multiple(() =>
            {
                Assert.That(company.EffectiveTaxRate, Is.Not.Null, company.Id);
                Assert.That(company.Confidence, Is.Not.EqualTo("low"), company.Id);
            });
        }
    }

    [TestCase("aapl", true)]
    [TestCase("sbux", false)]
    [TestCase("sony", true)]
    public void SampleCandidateFlags_AreCorrect(string id, bool expected)
    {
        var company = Data.Companies.Single(company => company.Id == id);
        Assert.That(company.IsAnswerCandidate, Is.EqualTo(expected));
    }
}
