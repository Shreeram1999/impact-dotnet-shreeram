// --- Part 1: try / catch / finally with a custom exception ---
var wallet = new Wallet(100m);

try
{
    wallet.Withdraw(250m);
}
catch (InsufficientFundsException ex)
{
    // ex.DeficitAmount is OUR extra property from InsufficientFundsException.cs
    // - a plain built-in Exception wouldn't have given us this directly.
    Console.WriteLine($"Withdrawal failed: {ex.Message}");
    Console.WriteLine($"You were short by: {ex.DeficitAmount:C}");
}
finally
{
    // `finally` runs NO MATTER WHAT - whether the try block succeeded,
    // threw an exception that got caught above, or even threw something
    // uncaught. It's the right place for "always do this cleanup/logging"
    // code, like recording that a withdrawal was attempted at all.
    Console.WriteLine("Logged: a withdrawal attempt was made.");
}

Console.WriteLine();

// --- Part 2: catch-order demo ---
Console.WriteLine("Parsing a valid number:");
Console.WriteLine($"  Result: {NumberParser.ParseStrictly("42")}");

Console.WriteLine();
Console.WriteLine("Parsing text that isn't a number:");
try
{
    NumberParser.ParseStrictly("not-a-number");
}
catch (FormatException)
{
    Console.WriteLine("  (caught again here in Program.cs, since ParseStrictly re-threw it)");
}

Console.WriteLine();
Console.WriteLine("See NumberParser.cs for the commented-out wrong-order example");
Console.WriteLine("and the CS0160 compile error it produces.");
