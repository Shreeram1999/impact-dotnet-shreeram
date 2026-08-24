Console.WriteLine("=== Adapter: JSON in, XML-based third-party generator underneath ===");
IJsonReportGenerator reportGenerator = new XmlReportAdapter(new ThirdPartyXmlReportGenerator());
var json = """{"Title":"Sales Report","Total":"5000"}""";
Console.WriteLine(reportGenerator.GenerateReport(json));

Console.WriteLine();
Console.WriteLine("=== Facade: one call drives Inventory + Payment + Shipping ===");
var orderFacade = new OrderFacade();
var confirmation = orderFacade.PlaceOrder("Wireless Mouse", 2, 1198.00m);
Console.WriteLine(confirmation);

Console.WriteLine();
Console.WriteLine("See ShortPatternNotes.md for brief notes on 8 more patterns");
Console.WriteLine("(Builder, Prototype, Decorator, Command, Template Method, Mediator,");
Console.WriteLine("Chain of Responsibility, State).");
