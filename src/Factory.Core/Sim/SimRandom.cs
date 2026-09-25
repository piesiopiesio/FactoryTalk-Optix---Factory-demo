// @summary: Seeded RNG (deterministic runs) with normal, exponential and chance helpers.
#nullable enable
namespace Factory.Core.Sim;

public sealed class SimRandom
{
    readonly Random rng;
    public SimRandom(int seed) => rng = new Random(seed);

    public double Uniform() => rng.NextDouble();
    public bool Chance(double p) => rng.NextDouble() < p;

    public double Normal(double mean, double sigma)
    {
        var u1 = 1.0 - rng.NextDouble();
        var u2 = rng.NextDouble();
        return mean + sigma * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }

    public double Exponential(double mean) => mean <= 0 ? double.PositiveInfinity : -mean * Math.Log(1.0 - rng.NextDouble());
}
