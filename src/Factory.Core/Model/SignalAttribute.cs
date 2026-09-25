// @summary: [Signal] marks a property exported to Optix variables, trace.json and the preview.
#nullable enable
using System.Reflection;

namespace Factory.Core.Model;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SignalAttribute : Attribute
{
    public string? Unit { get; init; }
    /// <summary>Decimals shown in UI and kept in trace (rounding).</summary>
    public int Decimals { get; init; } = 2;
}

public sealed record SignalInfo(string Name, string? Unit, int Decimals, Type Type, PropertyInfo Property)
{
    public object Read(object owner)
    {
        var v = Property.GetValue(owner)!;
        return v switch
        {
            Enum e => Convert.ToInt32(e),
            double d => Math.Round(d, Decimals),
            _ => v,
        };
    }
}

public static class Signals
{
    static readonly Dictionary<Type, IReadOnlyList<SignalInfo>> Cache = new();

    /// <summary>All [Signal] properties of a type, base class first, stable order.</summary>
    public static IReadOnlyList<SignalInfo> Of(Type t)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(t, out var list)) return list;
            var chain = new List<Type>();
            for (var c = t; c != null && c != typeof(object); c = c.BaseType) chain.Insert(0, c);
            list = chain
                .SelectMany(c => c.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(p => (p, a: p.GetCustomAttribute<SignalAttribute>()))
                .Where(x => x.a != null)
                .Select(x => new SignalInfo(x.p.Name, x.a!.Unit, x.a.Decimals, x.p.PropertyType, x.p))
                .ToList();
            Cache[t] = list;
            return list;
        }
    }

    public static Dictionary<string, object> Snapshot(object owner) =>
        Of(owner.GetType()).ToDictionary(s => s.Name, s => s.Read(owner));
}
