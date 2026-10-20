using System.Text.Json;
using Companydle;

const string Salt = "companydle-v1";

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/AnswerOrderGenerator -- <path-to-companies.json>");
    return 1;
}

var path = Path.GetFullPath(args[0]);
using var document = JsonDocument.Parse(File.ReadAllText(path));
var root = document.RootElement;

var companies = root.GetProperty("companies")
    .EnumerateArray()
    .Select(company => new
    {
        Id = company.GetProperty("id").GetString() ?? throw new InvalidDataException("A company is missing its id."),
        IsCandidate = company.GetProperty("isAnswerCandidate").GetBoolean(),
    })
    .ToList();

var candidates = companies
    .Where(company => company.IsCandidate)
    .Select(company => company.Id)
    .ToHashSet(StringComparer.Ordinal);

var existing = root.TryGetProperty("answerOrder", out var orderElement)
    ? orderElement.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToList()
    : [];

var invalid = existing.Where(id => !candidates.Contains(id)).ToList();
if (invalid.Count > 0)
{
    Console.Error.WriteLine("answerOrder contains ids that are not answer candidates:");
    foreach (var id in invalid)
    {
        Console.Error.WriteLine($"  {id}");
    }

    return 1;
}

var duplicate = existing
    .GroupBy(id => id, StringComparer.Ordinal)
    .FirstOrDefault(group => group.Count() > 1);
if (duplicate is not null)
{
    Console.Error.WriteLine($"answerOrder contains a duplicate id: {duplicate.Key}");
    return 1;
}

var missing = candidates
    .Except(existing, StringComparer.Ordinal)
    .OrderBy(id => StableHash.Fnv1a32(id + "|" + Salt))
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToList();

var result = existing.Concat(missing).ToList();

Console.WriteLine($"Existing: {existing.Count}; missing: {missing.Count}; total: {result.Count}");
Console.WriteLine();
foreach (var id in result)
{
    Console.WriteLine($"  {id,-8} {StableHash.Fnv1a32(id + "|" + Salt),10}");
}

Console.WriteLine();
Console.WriteLine(JsonSerializer.Serialize(result));
return 0;
