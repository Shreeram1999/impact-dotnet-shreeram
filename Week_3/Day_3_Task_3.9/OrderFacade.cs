// Task 3.9 - the Facade pattern.
//
// A real "place an order" flow usually needs to talk to SEVERAL separate
// subsystems - checking inventory, charging a payment, scheduling a
// shipment - each with their own methods and their own order of
// operations. A Facade wraps all of that behind ONE simple method, so
// callers (like Program.cs) don't need to know the individual subsystems
// exist at all, let alone in what order to call them correctly.
public class InventoryService
{
    public bool ReserveStock(string productName, int quantity)
    {
        Console.WriteLine($"  [Inventory] Reserved {quantity} x {productName}.");
        return true;
    }
}

public class PaymentService
{
    public bool ChargeCard(decimal amount)
    {
        Console.WriteLine($"  [Payment] Charged {amount:C}.");
        return true;
    }
}

public class ShippingService
{
    public string ScheduleShipping(string productName)
    {
        Console.WriteLine($"  [Shipping] Scheduled shipment for {productName}.");
        return "Estimated delivery: 3-5 business days";
    }
}

// OrderFacade doesn't do any of the actual work itself - it just knows the
// correct ORDER to call the three subsystems in, and presents that as one
// simple method. Program.cs only ever talks to PlaceOrder(); it has no
// idea InventoryService/PaymentService/ShippingService even exist.
public class OrderFacade
{
    private readonly InventoryService inventory = new();
    private readonly PaymentService payment = new();
    private readonly ShippingService shipping = new();

    public string PlaceOrder(string productName, int quantity, decimal totalPrice)
    {
        Console.WriteLine($"Placing order for {quantity} x {productName}...");

        inventory.ReserveStock(productName, quantity);
        payment.ChargeCard(totalPrice);
        var shippingEstimate = shipping.ScheduleShipping(productName);

        return $"Order placed for {quantity} x {productName}. {shippingEstimate}";
    }
}
