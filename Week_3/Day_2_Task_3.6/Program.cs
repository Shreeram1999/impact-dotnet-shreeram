Console.WriteLine("=== Custom IStockObserver interface ===");
var customTicker = new StockTickerCustom();
customTicker.Attach(new Investor("Asha"));
customTicker.Attach(new Investor("Rohit"));
customTicker.SetPrice("ACME", 101.50m);

Console.WriteLine();
Console.WriteLine("=== C# event ===");
var eventTicker = new StockTickerEvents();
var investor1 = new InvestorEventSubscriber("Asha");
var investor2 = new InvestorEventSubscriber("Rohit");
eventTicker.PriceChanged += investor1.OnPriceChanged;
eventTicker.PriceChanged += investor2.OnPriceChanged;
eventTicker.SetPrice("ACME", 101.50m);

Console.WriteLine();
Console.WriteLine("Both approaches notified every investor on the same price change -");
Console.WriteLine("see EventObserver.cs for a comment comparing the two styles.");
