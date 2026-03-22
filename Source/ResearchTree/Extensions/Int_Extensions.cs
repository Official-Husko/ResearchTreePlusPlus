using System.Collections.Generic;

namespace FluffyResearchTree;

public static class Int_Extensions
{
    private static readonly Dictionary<int, string> ToStringCache = [];

    public static string ToStringCached(this int value)
    {
        if (ToStringCache.TryGetValue(value, out var cached))
        {
            return cached;
        }

        cached = value.ToString();
        ToStringCache[value] = cached;
        return cached;
    }
}
