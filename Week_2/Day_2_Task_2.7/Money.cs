// Task 2.7 - operator overloading.
// Operator overloading lets you teach C#'s built-in symbols (+, ==, >, ...)
// how to work with YOUR OWN types, so `moneyA + moneyB` reads naturally
// instead of forcing everyone to write `moneyA.Add(moneyB)`.
//
// This is a `struct`, not a `class`. Structs are "value types" - good for
// small, simple pieces of data (like Money: just an amount + a currency
// code) that should behave like plain values rather than objects with a
// long lifetime and identity. `readonly` on the struct means none of its
// properties can be changed after it's created - operations like `+`
// always produce a brand-new Money instead of modifying an existing one.
public readonly struct Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // `public static Money operator +(...)` is the special syntax for
    // overloading `+`. Whenever code writes `someMoney + otherMoney`, C#
    // calls this method behind the scenes. We reject mixing currencies
    // (10 USD + 5 EUR isn't a meaningful "15" of anything) by throwing.
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    // C# requires you to overload == and != as a matching pair - you can't
    // define one without the other.
    public static bool operator ==(Money left, Money right) =>
        left.Currency == right.Currency && left.Amount == right.Amount;

    public static bool operator !=(Money left, Money right) => !(left == right);

    // Comparing amounts across different currencies is just as meaningless
    // as adding them, so > and < also check the currency matches first.
    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    // A small private helper reused by +, >, and < instead of repeating
    // this same currency check three times.
    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot combine {left.Currency} with {right.Currency}.");
    }

    // Whenever you overload ==, C# best practice says you should also
    // implement Equals() and GetHashCode() to match, otherwise things like
    // Dictionary<Money, ...> or List<Money>.Contains(...) - which use
    // Equals/GetHashCode instead of == - could disagree with your ==
    // operator and give confusing results.
    public bool Equals(Money other) => this == other;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:F2} {Currency}";
}
