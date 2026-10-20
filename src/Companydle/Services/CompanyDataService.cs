using System.Net.Http.Json;
using Companydle.Models;

namespace Companydle.Services;

/// <summary>
/// Loads the bundled JSON through the app's relative base address. The result
/// is cached for the lifetime of the scoped service, and a retry can force a
/// fresh request after a failed load.
/// </summary>
public sealed class CompanyDataService(HttpClient http)
{
    public const string DataPath = "data/companies.json";

    private CompanyData? _data;

    public async Task<CompanyData> LoadAsync(bool forceReload = false, CancellationToken cancellationToken = default)
    {
        if (_data is not null && !forceReload)
        {
            return _data;
        }

        var data = await http.GetFromJsonAsync<CompanyData>(DataPath, cancellationToken)
            ?? throw new InvalidDataException("The company data file was empty.");

        _data = data;
        return data;
    }
}
