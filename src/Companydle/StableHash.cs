using System.Text;

namespace Companydle;

/// <summary>
/// Stable hash used only when building the explicit, append-only answer order.
/// It is never used at runtime to choose today's puzzle.
/// </summary>
public static class StableHash
{
    public static uint Fnv1a32(string value)
    {
        var hash = 2166136261u;
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            hash ^= b;
            hash *= 16777619u;
        }

        return hash;
    }
}
