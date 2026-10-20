using Microsoft.JSInterop;

namespace Companydle.Services;

/// <summary>
/// Small abstraction around browser storage so the persistence rules can be
/// tested without a browser. Every implementation can throw; GameStateService
/// treats any failure as "use the in-memory fallback".
/// </summary>
public interface IStateStorage
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
}

public sealed class BrowserStateStorage(IJSRuntime js) : IStateStorage
{
    public async Task<string?> GetAsync(string key) =>
        await js.InvokeAsync<string?>("localStorage.getItem", key);

    public async Task SetAsync(string key, string value) =>
        await js.InvokeVoidAsync("localStorage.setItem", key, value);
}

public sealed class InMemoryStateStorage : IStateStorage
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public Task<string?> GetAsync(string key) =>
        Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);

    public Task SetAsync(string key, string value)
    {
        _values[key] = value;
        return Task.CompletedTask;
    }
}
