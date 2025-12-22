using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Translations;

public sealed class JsonStringLocalizer : ProxyObject, IStringLocalizer
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resources;

    public JsonStringLocalizer(string langPath)
    {
        _resources = LoadResources(langPath);
    }

    private ConcurrentDictionary<string, Dictionary<string, string>> LoadResources(string langPath)
    {
        var result = new ConcurrentDictionary<string, Dictionary<string, string>>();

        foreach (var file in Directory.GetFiles(langPath, "*.json"))
        {
            var cultureName = Path.GetFileNameWithoutExtension(file);
            var json = File.ReadAllText(file);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            result[cultureName] = dict;
        }

        return result;
    }

    public LocalizedString this[string name]
    {
        get
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var value = GetStringSafely(name);

            return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var format = GetStringSafely(name);
            var value = string.Format(format ?? name, arguments);

            return new LocalizedString(name, value, resourceNotFound: format == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        if (_resources.TryGetValue(culture, out var dict))
        {
            foreach (var kv in dict)
                yield return new LocalizedString(kv.Key, kv.Value, false);
        }
    }

    private string GetStringSafely(string name, CultureInfo culture = null)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        culture ??= CultureInfo.CurrentUICulture;

        if (_resources.TryGetValue(culture.Name, out var dict) && dict.TryGetValue(name, out var value))
            return value;

        if (_resources.TryGetValue("en", out var enDict) && enDict.TryGetValue(name, out var enValue))
            return enValue;

        return null;
    }
}