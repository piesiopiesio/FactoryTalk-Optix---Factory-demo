// @summary: Minimal test harness: discovers static methods marked [Test], runs them, prints PASS/FAIL, exit code.
using System.Reflection;

namespace Factory.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TestAttribute : Attribute;

public static class Assert
{
    public static void True(bool cond, string msg) { if (!cond) throw new Exception(msg); }
    public static void Equal<T>(T expected, T actual, string what) =>
        True(EqualityComparer<T>.Default.Equals(expected, actual), $"{what}: expected {expected}, got {actual}");
    public static void InRange(double v, double lo, double hi, string what) => True(v >= lo && v <= hi, $"{what}: {v} not in [{lo}, {hi}]");
    public static void Throws<TEx>(Action a, string what) where TEx : Exception
    {
        try { a(); } catch (TEx) { return; }
        throw new Exception($"{what}: expected {typeof(TEx).Name}");
    }
}

public static class TestRunner
{
    public static int Main(string[] args)
    {
        var filter = args.FirstOrDefault() ?? "";
        var tests = typeof(TestRunner).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null && $"{m.DeclaringType!.Name}.{m.Name}".Contains(filter))
            .OrderBy(m => m.DeclaringType!.Name).ThenBy(m => m.Name).ToList();
        var failed = 0;
        foreach (var m in tests)
        {
            var name = $"{m.DeclaringType!.Name}.{m.Name}";
            try { m.Invoke(null, null); Console.WriteLine($"PASS  {name}"); }
            catch (TargetInvocationException ex) { failed++; Console.WriteLine($"FAIL  {name}: {ex.InnerException?.Message}"); }
        }
        Console.WriteLine($"{tests.Count - failed}/{tests.Count} passed");
        return failed == 0 ? 0 : 1;
    }
}
