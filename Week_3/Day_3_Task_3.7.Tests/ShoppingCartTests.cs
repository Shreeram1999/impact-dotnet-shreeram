// Covers each IPaymentStrategy implementation individually, and the
// "runtime swap" behavior - the same ShoppingCart, same total, producing a
// DIFFERENT result purely because SetPaymentStrategy() was called with a
// different strategy in between.
public class ShoppingCartTests
{
    [Fact]
    public void CreditCardPayment_Pay_MentionsCardAndAmount()
    {
        IPaymentStrategy strategy = new CreditCardPayment("4321");

        var receipt = strategy.Pay(100m);

        Assert.Contains("Credit Card", receipt);
        Assert.Contains("4321", receipt);
    }

    [Fact]
    public void UpiPayment_Pay_MentionsUpiId()
    {
        IPaymentStrategy strategy = new UpiPayment("asha@upi");

        var receipt = strategy.Pay(100m);

        Assert.Contains("UPI", receipt);
        Assert.Contains("asha@upi", receipt);
    }

    [Fact]
    public void NetBankingPayment_Pay_MentionsBankName()
    {
        IPaymentStrategy strategy = new NetBankingPayment("Example Bank");

        var receipt = strategy.Pay(100m);

        Assert.Contains("NetBanking", receipt);
        Assert.Contains("Example Bank", receipt);
    }

    [Fact]
    public void Checkout_WithoutStrategySelected_Throws()
    {
        var cart = new ShoppingCart();
        cart.AddItem(50m);

        Assert.Throws<InvalidOperationException>(() => cart.Checkout());
    }

    [Fact]
    public void Checkout_SwappingStrategyAtRuntime_ChangesReceiptWithoutChangingCartCode()
    {
        var cart = new ShoppingCart();
        cart.AddItem(100m);

        cart.SetPaymentStrategy(new CreditCardPayment("1111"));
        var creditCardReceipt = cart.Checkout();

        cart.SetPaymentStrategy(new UpiPayment("rohit@upi"));
        var upiReceipt = cart.Checkout();

        Assert.Contains("Credit Card", creditCardReceipt);
        Assert.Contains("UPI", upiReceipt);
        Assert.NotEqual(creditCardReceipt, upiReceipt);
    }

    [Fact]
    public void Checkout_UsesSumOfAddedItemsAsTheAmount()
    {
        var cart = new ShoppingCart();
        cart.AddItem(100m);
        cart.AddItem(50m);
        cart.SetPaymentStrategy(new UpiPayment("asha@upi"));

        var receipt = cart.Checkout();

        Assert.Contains("150.00", receipt);
    }
}
