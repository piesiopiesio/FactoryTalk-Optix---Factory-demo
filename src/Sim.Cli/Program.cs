// @summary: CLI entry: `Sim.Cli [--manifest factory.json] [--minutes 30] [--frame 2] [--out design/data]`; exit 1 if a check fails.
using System.Text.Json;
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;
using Sim.Cli;

var opts = Args.Parse(args);
var manifestPath = opts.Get("manifest", "factory.json");
var minutes = double.Parse(opts.Get("minutes", "30"), System.Globalization.CultureInfo.InvariantCulture);
var frameS = double.Parse(opts.Get("frame", "2"), System.Globalization.CultureInfo.InvariantCulture);
var outDir = opts.Get("out", "design/data");
const double Dt = 0.1;

var manifest = FactoryLoader.Parse(File.ReadAllText(manifestPath));
var hall = FactoryLoader.Build(manifest);
var engine = new SimEngine(hall);
var recorder = new TraceRecorder(engine);
var everyTicks = Math.Max(1, (int)Math.Round(frameS / Dt));
var tick = 0;

recorder.Capture();
engine.Run(minutes * 60, Dt, e => { if (++tick % everyTicks == 0) recorder.Capture(); });

var checks = Checks.Run(engine, manifest);
Directory.CreateDirectory(outDir);
var json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

var trace = new
{
    meta = new { seed = hall.Seed, dt = Dt, frameS, minutes, palette = Enum.GetValues<MachineState>().ToDictionary(s => s.ToString(), StatePalette.Hex) },
    schema = recorder.Schema(),
    frames = recorder.Frames,
    events = engine.Events.Select(e => new object[] { e.Time, e.NodeId, e.Kind.ToString(), e.Code, e.Text }),
};
File.WriteAllText(Path.Combine(outDir, "trace.json"), JsonSerializer.Serialize(trace, json));
File.WriteAllText(Path.Combine(outDir, "layout.json"), JsonSerializer.Serialize(LayoutExport.Build(manifest), new JsonSerializerOptions(json) { WriteIndented = true }));

var summary = new
{
    simMinutes = minutes,
    seed = hall.Seed,
    lines = hall.Lines.Select(l => new
    {
        l.Id, l.Name,
        kpi = Signals.Snapshot(l),
        stations = l.Equipment.Select(e => new
        {
            e.Id, e.TypeKey,
            timePct = Enum.GetValues<MachineState>().Where(s => e.TimeIn(s) > 0)
                .ToDictionary(s => s.ToString(), s => Math.Round(100 * e.TimeIn(s) / engine.Time, 1)),
            counts = e is Station st ? new { st.Processed, st.Good, st.Reject } : null,
        }),
    }),
    faults = engine.Events.Where(e => e.Kind == SimEventKind.FaultRaised).GroupBy(e => e.NodeId).ToDictionary(g => g.Key, g => g.Count()),
    checks,
    ok = checks.All(c => c.Ok),
};
File.WriteAllText(Path.Combine(outDir, "trace-summary.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions(json) { WriteIndented = true }));

foreach (var c in checks) Console.WriteLine($"{(c.Ok ? "PASS" : "FAIL")}  {c.Name}  ({c.Detail})");
Console.WriteLine($"trace: {recorder.Frames.Count} frames, {engine.Events.Count} events -> {outDir}");
return checks.All(c => c.Ok) ? 0 : 1;

sealed class Args(Dictionary<string, string> map)
{
    public static Args Parse(string[] a) => new(Enumerable.Range(0, a.Length / 2).ToDictionary(i => a[2 * i].TrimStart('-'), i => a[2 * i + 1]));
    public string Get(string key, string fallback) => map.TryGetValue(key, out var v) ? v : fallback;
}
