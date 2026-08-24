// Task 3.7 - the Strategy pattern.
//
// "Strategy" means: pull an ALGORITHM (here, "how do I pay for this cart?")
// out into its own interchangeable object, instead of hardcoding it with an
// if/else chain inside ShoppingCart. Each concrete strategy below
// implements the same IPaymentStrategy contract, so ShoppingCart can work
// with ANY of them without ever needing to know which one it's actually
// using - it just calls Pay(amount) and trusts the strategy to know how.
public interface IPaymentStrategy
{
    // Returns a receipt string (instead of just printing to the console)
    // so calling code - including automated tests - can check exactly
    // what happened, rather than having to capture console output.
    string Pay(decimal amount);
}

public class CreditCardPayment : IPaymentStrategy
{
    private readonly string cardNumberLastFour;

    public CreditCardPayment(string cardNumberLastFour)
    {
        this.cardNumberLastFour = cardNumberLastFour;
    }

    public string Pay(decimal amount) =>
        $"Paid {amount:C} via Credit Card ending {cardNumberLastFour}.";
}

public class UpiPayment : IPaymentStrategy
{
    private readonly string upiId;

    public UpiPayment(string upiId)
    {
        this.upiId = upiId;
    }

    public string Pay(decimal amount) =>
        $"Paid {amount:C} via UPI ({upiId}).";
}

public class NetBankingPayment : IPaymentStrategy
{
    private readonly string bankName;

    public NetBankingPayment(string bankName)
    {
        this.bankName = bankName;
    }

    public string Pay(decimal amount) =>
        $"Paid {amount:C} via NetBanking ({bankName}).";
}
