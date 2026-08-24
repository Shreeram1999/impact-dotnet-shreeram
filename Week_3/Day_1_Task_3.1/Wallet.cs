// A tiny class just to give InsufficientFundsException somewhere real to
// come from. Compare this to Week 2's BankAccount - same idea (validate
// before mutating state), but this time the "no overdraw" rule is enforced
// by THROWING OUR OWN EXCEPTION TYPE instead of the built-in
// InvalidOperationException we used back then.
public class Wallet
{
    public decimal Balance { get; private set; }

    public Wallet(decimal openingBalance)
    {
        Balance = openingBalance;
    }

    public void Withdraw(decimal amount)
    {
        if (amount > Balance)
        {
            // How much money is missing, so the caller can display it.
            var deficit = amount - Balance;
            throw new InsufficientFundsException(deficit);
        }

        Balance -= amount;
    }
}
