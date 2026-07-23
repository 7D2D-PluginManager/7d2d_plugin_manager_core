using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PluginManager.Api.Capabilities.Implementations.Storage;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Storage;

public sealed class JsonStorage : ProxyObject, IStorage
{
    public string Name => nameof(JsonStorage);

    private sealed class CacheEntry
    {
        public Dictionary<string, string> Data;
        public DateTime FileTimeUtc;
    }

    private readonly string _root;
    private readonly object _gate = new();
    private readonly Dictionary<string, CacheEntry> _cache = new();

    public JsonStorage(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    public string Read(string scope, string collection, string key)
    {
        lock (_gate)
        {
            var data = Load(scope, collection);
            return data.TryGetValue(key, out var value) ? value : null;
        }
    }

    public void Write(string scope, string collection, string key, string value)
    {
        lock (_gate)
        {
            var data = Load(scope, collection);
            data[key] = value;
            Persist(scope, collection, data);
        }
    }

    public bool Delete(string scope, string collection, string key)
    {
        lock (_gate)
        {
            var data = Load(scope, collection);
            if (!data.Remove(key))
                return false;

            Persist(scope, collection, data);
            return true;
        }
    }

    public List<string> Keys(string scope, string collection)
    {
        lock (_gate)
        {
            return new List<string>(Load(scope, collection).Keys);
        }
    }

    public Dictionary<string, string> ReadAll(string scope, string collection)
    {
        lock (_gate)
        {
            return new Dictionary<string, string>(Load(scope, collection));
        }
    }

    private Dictionary<string, string> Load(string scope, string collection)
    {
        var cacheKey = scope + "/" + collection;
        var path = FilePath(scope, collection);
        var fileTimeUtc = File.GetLastWriteTimeUtc(path);

        if (_cache.TryGetValue(cacheKey, out var cached) && cached.FileTimeUtc == fileTimeUtc)
            return cached.Data;

        var data = File.Exists(path)
            ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path))
            : null;

        data ??= new Dictionary<string, string>();
        _cache[cacheKey] = new CacheEntry { Data = data, FileTimeUtc = fileTimeUtc };
        return data;
    }

    private void Persist(string scope, string collection, Dictionary<string, string> data)
    {
        var path = FilePath(scope, collection);
        var tmp = path + ".tmp";

        File.WriteAllText(tmp, JsonConvert.SerializeObject(data, Formatting.Indented));

        if (File.Exists(path))
            File.Replace(tmp, path, null);
        else
            File.Move(tmp, path);

        var cacheKey = scope + "/" + collection;
        _cache[cacheKey] = new CacheEntry { Data = data, FileTimeUtc = File.GetLastWriteTimeUtc(path) };
    }

    private string FilePath(string scope, string collection)
    {
        return Path.Combine(_root, scope + "." + collection + ".json");
    }
}
