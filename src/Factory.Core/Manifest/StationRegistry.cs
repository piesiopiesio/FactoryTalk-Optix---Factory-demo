// @summary: Maps manifest "type" keys to Station classes via [StationType]; the single extension point for new stations.
#nullable enable
using System.Reflection;
using Factory.Core.Model;

namespace Factory.Core.Manifest;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StationTypeAttribute : Attribute
{
    public StationTypeAttribute(string key) { Key = key; }
    public string Key { get; }
}

public static class StationRegistry
{
    static readonly Lazy<IReadOnlyDictionary<string, Type>> Map = new(() =>
        typeof(Station).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Station).IsAssignableFrom(t))
            .Select(t => (t, a: t.GetCustomAttribute<StationTypeAttribute>()))
            .Where(x => x.a != null)
            .ToDictionary(x => x.a!.Key, x => x.t));

    public static IReadOnlyDictionary<string, Type> Types => Map.Value;

    public static bool Has(string key) => Map.Value.ContainsKey(key);

    public static Station Create(string key) =>
        Map.Value.TryGetValue(key, out var t)
            ? (Station)Activator.CreateInstance(t)!
            : throw new ManifestException($"Unknown station type '{key}'. Known: {string.Join(", ", Map.Value.Keys)}");
}

public sealed class ManifestException : Exception
{
    public ManifestException(string message) : base(message) { }
}
