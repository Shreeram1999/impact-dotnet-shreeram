Console.WriteLine("=== MathHelper (static) ===");
Console.WriteLine($"Factorial(5) = {MathHelper.Factorial(5)}");
Console.WriteLine($"Factorial(0) = {MathHelper.Factorial(0)}");
Console.WriteLine($"IsPrime(7) = {MathHelper.IsPrime(7)}");
Console.WriteLine($"IsPrime(1) = {MathHelper.IsPrime(1)}");
Console.WriteLine($"GCD(48, 18) = {MathHelper.GCD(48, 18)}");

Console.WriteLine();
Console.WriteLine("=== OrderProcessor (instance) ===");
// Two SEPARATE processors, each with its own independent list - proving
// why these had to be instance methods, not static ones.
var warehouseA = new OrderProcessor();
var warehouseB = new OrderProcessor();

Console.WriteLine(warehouseA.ProcessOrder("A-1001"));
Console.WriteLine(warehouseA.ProcessOrder("A-1002"));
Console.WriteLine(warehouseB.ProcessOrder("B-2001"));

Console.WriteLine($"Warehouse A has processed {warehouseA.ProcessedOrderIds.Count} orders.");
Console.WriteLine($"Warehouse B has processed {warehouseB.ProcessedOrderIds.Count} orders.");

Console.WriteLine();
Console.WriteLine("See InterfaceVsAbstractNotes.md for the interface-vs-abstract comparison.");
