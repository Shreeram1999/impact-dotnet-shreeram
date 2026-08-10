// Task 2.1 - Encapsulation.
// Encapsulation just means: hide a class's internal data, and only let the
// outside world change it through methods you control. Here, `balance` and
// `history` are marked `private`, which means only code INSIDE this
// BankAccount class can touch them directly. Any other class (like
// Program.cs) can only go through Deposit(), Withdraw(), GetBalance() and
// PrintHistory(). That's what lets us guarantee "the balance can never go
// negative" - there's no back door that skips the validation checks below.
public class BankAccount
{
    // `decimal` is the type C# recommends for money (more precise than
    // double/float for currency math). `readonly` on the list means the
    // list reference itself can't be swapped out for a different list
    // after the constructor runs - but we can still Add() items into it.
    private decimal balance;
    private readonly List<string> history = new();

    // A constructor: the special method that runs when you write
    // `new BankAccount(...)`. `= 0` is a default parameter value, so
    // `new BankAccount()` with no arguments also works and opens with a
    // zero balance.
    public BankAccount(decimal openingBalance = 0)
    {
        if (openingBalance < 0)
            throw new ArgumentException("Opening balance cannot be negative.", nameof(openingBalance));

        balance = openingBalance;
        history.Add($"Opened account with balance {openingBalance:C}");
    }

    // Every public method here follows the same pattern: check the input
    // is valid FIRST, and only touch `balance`/`history` if it passes.
    // `throw` stops the method immediately and hands the problem back to
    // whoever called Deposit() - it's how C# reports "this went wrong".
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive.", nameof(amount));

        balance += amount;
        history.Add($"Deposited {amount:C} -> balance {balance:C}");
    }

    // Two rules enforced here: the amount must be positive, AND it can't be
    // more than what's currently in the account (no overdrawing). Notice
    // the rejected attempt still gets logged to `history` before the
    // exception is thrown - so even failed withdrawals leave a paper trail.
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive.", nameof(amount));

        if (amount > balance)
        {
            history.Add($"Rejected withdrawal of {amount:C} -> insufficient funds (balance {balance:C})");
            throw new InvalidOperationException("Insufficient funds: withdrawal would overdraw the account.");
        }

        balance -= amount;
        history.Add($"Withdrew {amount:C} -> balance {balance:C}");
    }

    // A read-only "getter" method: it lets outside code SEE the balance
    // without being able to directly SET it (there's no SetBalance method).
    public decimal GetBalance() => balance;

    // `foreach` walks through every string that was added to `history`, in
    // the order they were added, and prints each one on its own line.
    public void PrintHistory()
    {
        foreach (var entry in history)
            Console.WriteLine(entry);
    }
}
