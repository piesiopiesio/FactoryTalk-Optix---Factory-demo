// @summary: Signals worth trending (line KPIs, station doubles, belt occupancy) with DB column names and readable pen labels.
#nullable enable
namespace Factory.Core.Model;

/// <summary>One logged variable: <see cref="Column"/> is the DataLogger column (never change its format — history
/// lives under it), <see cref="Label"/> is what the operator sees as the pen name, e.g. "L1 OEE [%]".</summary>
public sealed record TrendPen(string Path, SignalInfo Signal, string Column, string Label);

public static class TrendPens
{
    /// <summary>Line KPIs (all doubles), each station's own doubles (tank, torque...), belt occupancy — stable order.</summary>
    public static IEnumerable<TrendPen> Of(Hall hall)
    {
        foreach (var line in hall.Lines)
        {
            foreach (var s in Signals.Of(typeof(Line)).Where(s => s.Type == typeof(double)))
                yield return Pen(line.Id, s, line.Id);
            foreach (var st in line.Stations)
                foreach (var s in Signals.Of(st.GetType()).Where(s => s.Type == typeof(double) && s.Property.DeclaringType == st.GetType()))
                    yield return Pen(st.Path, s, $"{line.Id} {st.Name}:");
            foreach (var c in line.Conveyors)
                foreach (var s in Signals.Of(typeof(Conveyor)).Where(s => s.Name == nameof(Conveyor.Occupancy)))
                    yield return Pen(c.Path, s, $"{line.Id} Taśma {c.Id}:");
        }
    }

    /// <summary>Column = path with '_' + camelCase signal ("L1_FILL_tankLevel"), as logged since the first Build.</summary>
    public static string Column(string path, string signal) =>
        path.Replace('/', '_') + "_" + char.ToLowerInvariant(signal[0]) + signal.Substring(1);

    public static string Label(string owner, SignalInfo s) =>
        string.IsNullOrEmpty(s.Unit) ? $"{owner} {s.Caption}" : $"{owner} {s.Caption} [{s.Unit}]";

    static TrendPen Pen(string path, SignalInfo s, string owner) => new(path, s, Column(path, s.Name), Label(owner, s));
}
