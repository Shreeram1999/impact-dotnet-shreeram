var tenUsd = new Money(10, "USD");
var fiveUsd = new Money(5, "USD");
var fiveEur = new Money(5, "EUR");

// Reading naturally thanks to the overloaded operators: +, ==, !=, >, <
// all "just work" on Money the same way they do on int or double, because
// Money.cs taught the compiler how to handle them for this type.
Console.WriteLine($"{tenUsd} + {fiveUsd} = {tenUsd + fiveUsd}");
Console.WriteLine($"{tenUsd} == {fiveUsd}: {tenUsd == fiveUsd}");
Console.WriteLine($"{tenUsd} != {fiveUsd}: {tenUsd != fiveUsd}");
Console.WriteLine($"{tenUsd} > {fiveUsd}: {tenUsd > fiveUsd}");
Console.WriteLine($"{fiveUsd} < {tenUsd}: {fiveUsd < tenUsd}");

// Deliberately trying to add two different currencies. The `+` operator
// we wrote throws an exception instead of returning a nonsense value, and
// try/catch lets us handle that gracefully instead of crashing.
try
{
    _ = tenUsd + fiveEur;
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Currency mismatch rejected as expected: {ex.Message}");
}
