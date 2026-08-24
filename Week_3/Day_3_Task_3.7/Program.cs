var cart = new ShoppingCart();
cart.AddItem(499.00m);
cart.AddItem(1299.00m);

Console.WriteLine($"Cart total: {cart.Total:C}");
Console.WriteLine();

// Same cart, same total - only the STRATEGY changes between checkouts.
// ShoppingCart.Checkout() itself never changes; only the object we hand it
// via SetPaymentStrategy does.
cart.SetPaymentStrategy(new CreditCardPayment("4321"));
Console.WriteLine(cart.Checkout());

cart.SetPaymentStrategy(new UpiPayment("asha@upi"));
Console.WriteLine(cart.Checkout());

cart.SetPaymentStrategy(new NetBankingPayment("Example Bank"));
Console.WriteLine(cart.Checkout());
