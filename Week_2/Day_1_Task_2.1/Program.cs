// This is a "top-level statements" Program.cs - modern C# lets you skip
// writing `class Program { static void Main() { ... } }` and just write
// the code directly in the file. It still compiles to a normal Main
// method behind the scenes; this is purely less typing.

// Create an account with an opening balance of 1000, then do a normal
// deposit and withdrawal - both should succeed without any errors.
var account = new BankAccount(1000);

account.Deposit(500);
account.Withdraw(200);

// try/catch: we're deliberately calling Withdraw() with an amount bigger
// than the balance, which we know will throw an exception (see
// BankAccount.Withdraw). `try` lets us attempt that risky call, and
// `catch (InvalidOperationException ex)` catches it so the program keeps
// running instead of crashing, and lets us print a friendly message.
try
{
    account.Withdraw(10_000);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Overdraw rejected as expected: {ex.Message}");
}

Console.WriteLine($"Final balance: {account.GetBalance():C}");
Console.WriteLine();
Console.WriteLine("Transaction history:");
account.PrintHistory();
