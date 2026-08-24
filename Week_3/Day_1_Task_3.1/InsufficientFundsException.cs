// Task 3.1 - a custom exception type.
//
// In C#, "exceptions" are just objects (like any other class), and every
// exception type has to eventually inherit from the built-in Exception
// class. Making your OWN exception type - instead of always throwing a
// plain `new Exception("some message")` - lets you attach extra data that
// is specific to YOUR problem. Here, we don't just want to say "insufficient
// funds", we also want to say exactly HOW MUCH was missing, so callers can
// show that number to the user without having to re-calculate it.
public class InsufficientFundsException : Exception
{
    // Extra piece of information that a plain Exception wouldn't carry.
    public decimal DeficitAmount { get; }

    // `: base(message)` passes the message up to the built-in Exception
    // class, so things like ex.Message still work normally, exactly like
    // any other exception.
    public InsufficientFundsException(decimal deficitAmount)
        : base($"Insufficient funds: short by {deficitAmount:C}.")
    {
        DeficitAmount = deficitAmount;
    }
}
