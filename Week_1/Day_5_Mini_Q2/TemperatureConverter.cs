public readonly record struct Celsius(double Value);
public readonly record struct Fahrenheit(double Value);
public readonly record struct Kelvin(double Value);

public static class TemperatureConverter
{
    // Overloading needs distinct parameter TYPES (or counts) — a plain
    // "double value, string unit" signature can only be written once. Each
    // unit gets its own tiny wrapper type, and Convert is overloaded once
    // per wrapper type so the compiler picks the right one at compile time.
    public static (Fahrenheit F, Kelvin K) Convert(Celsius c)
    {
        double f = c.Value * 9 / 5 + 32;
        double k = c.Value + 273.15;
        return (new Fahrenheit(f), new Kelvin(k));
    }

    public static (Celsius C, Kelvin K) Convert(Fahrenheit f)
    {
        double c = (f.Value - 32) * 5 / 9;
        double k = c + 273.15;
        return (new Celsius(c), new Kelvin(k));
    }

    public static (Celsius C, Fahrenheit F) Convert(Kelvin k)
    {
        double c = k.Value - 273.15;
        double f = c * 9 / 5 + 32;
        return (new Celsius(c), new Fahrenheit(f));
    }

    // A runtime entry point on top of the compile-time overloads above, for
    // callers whose unit only becomes known at runtime (e.g. user input).
    // Unlike the overloads, an unrecognized unit is a genuine runtime
    // failure, so it throws instead of being ruled out by the type system.
    public static (double First, double Second) ConvertByUnit(double value, string unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        return unit.Trim().ToUpperInvariant() switch
        {
            "C" => AsDoubles(Convert(new Celsius(value))),
            "F" => AsDoubles(Convert(new Fahrenheit(value))),
            "K" => AsDoubles(Convert(new Kelvin(value))),
            _ => throw new ArgumentException($"Unknown temperature unit '{unit}'. Expected C, F, or K.", nameof(unit))
        };
    }

    private static (double, double) AsDoubles((Fahrenheit F, Kelvin K) t) => (t.F.Value, t.K.Value);
    private static (double, double) AsDoubles((Celsius C, Kelvin K) t) => (t.C.Value, t.K.Value);
    private static (double, double) AsDoubles((Celsius C, Fahrenheit F) t) => (t.C.Value, t.F.Value);
}
