using System;
using System.Collections.Generic;
using System.Globalization;

namespace PluginManager.Core.Mappers;

public static class CountryToCultureMapper
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "BY", "ru-RU" }, { "RU", "ru-RU" }, { "UA", "uk-UA" },
        { "PL", "pl-PL" }, { "DE", "de-DE" }, { "FR", "fr-FR" },
        { "ES", "es-ES" }, { "IT", "it-IT" }, { "GB", "en-GB" },
        { "US", "en-US" }, { "KZ", "ru-RU" }
    };

    public static CultureInfo Get(string countryCode, bool simple = false)
    {
        return TryGet(countryCode, out var culture, simple) ? culture : null;
    }

    public static bool TryGet(string countryCode, out CultureInfo culture, bool simple = false)
    {
        culture = null;

        if (!Map.TryGetValue(countryCode, out var cultureCode))
            return false;

        try
        {
            culture = new CultureInfo(
                simple && cultureCode.Length >= 2 ? cultureCode.Substring(0, 2) : cultureCode
            );

            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}