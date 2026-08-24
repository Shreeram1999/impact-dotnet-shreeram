public class ShoppingCart
{
    private readonly List<decimal> itemPrices = new();

    // The cart only ever talks to the IPaymentStrategy INTERFACE, never to
    // CreditCardPayment/UpiPayment/NetBankingPayment directly. That's what
    // lets us swap the strategy at runtime (see Program.cs) without
    // touching a single line of ShoppingCart's own code.
    private IPaymentStrategy? paymentStrategy;

    public void AddItem(decimal price) => itemPrices.Add(price);

    public decimal Total => itemPrices.Sum();

    // Changing HOW you pay is just swapping out this one field - the cart
    // itself has no idea (and doesn't need to know) which concrete payment
    // type it's currently using.
    public void SetPaymentStrategy(IPaymentStrategy strategy)
    {
        paymentStrategy = strategy;
    }

    public string Checkout()
    {
        if (paymentStrategy is null)
            throw new InvalidOperationException("No payment strategy has been selected.");

        return paymentStrategy.Pay(Total);
    }
}
