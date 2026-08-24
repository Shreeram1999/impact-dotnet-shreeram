using System.Reflection;

// Task 3.11 - Reflection.
//
// "Reflection" is .NET's ability to look at TYPES AT RUNTIME - their
// properties, methods, constructors - as data you can loop over, instead
// of something only the compiler knows about ahead of time. Normally, if
// you write `someInvoice.Amount`, the COMPILER already knows Invoice has
// an Amount property. With reflection, we can ask "what properties does
// THIS type have?" while the program is running, without having written
// `.Amount` anywhere - useful for things like serializers, ORMs, and
// validators that need to work with ANY type, not just Invoice specifically.

// `typeof(Invoice)` gives us a `Type` object - reflection's description of
// the Invoice class itself (not an instance of it).
var invoiceType = typeof(Invoice);

Console.WriteLine($"Class name: {invoiceType.Name}");

Console.WriteLine();
Console.WriteLine("Properties:");
// GetProperties() returns one PropertyInfo per public property, each of
// which knows the property's Name and its PropertyType (e.g. string, decimal).
foreach (var property in invoiceType.GetProperties())
    Console.WriteLine($"  {property.Name} : {property.PropertyType.Name}");

Console.WriteLine();
Console.WriteLine("Methods (declared directly on Invoice):");
// DeclaredOnly filters out inherited methods like ToString()/Equals() that
// every class gets for free from Object, so we only see the ones Invoice
// itself actually defines.
foreach (var method in invoiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"  {method.Name}");

Console.WriteLine();
Console.WriteLine("Constructors:");
foreach (var constructor in invoiceType.GetConstructors())
{
    var parameterList = string.Join(", ", constructor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    Console.WriteLine($"  Invoice({parameterList})");
}

Console.WriteLine();
Console.WriteLine("Creating an Invoice and setting a property, entirely via reflection:");

// Activator.CreateInstance builds a new object of the given type at
// runtime, calling its parameterless constructor - the reflection
// equivalent of writing `new Invoice()`, except the TYPE itself is just a
// variable (`invoiceType`) here, not hardcoded as `Invoice` in the source.
var invoiceInstance = (Invoice)Activator.CreateInstance(invoiceType)!;

// GetProperty("InvoiceNumber") looks up that one property by name at
// runtime, and SetValue(...) assigns it - the reflection equivalent of
// writing `invoiceInstance.InvoiceNumber = "INV-1001";`.
var invoiceNumberProperty = invoiceType.GetProperty(nameof(Invoice.InvoiceNumber))!;
invoiceNumberProperty.SetValue(invoiceInstance, "INV-1001");

var amountProperty = invoiceType.GetProperty(nameof(Invoice.Amount))!;
amountProperty.SetValue(invoiceInstance, 2500m);

// Reading it back the NORMAL way (not through reflection) proves the
// values really did get set.
Console.WriteLine($"  Result: InvoiceNumber = {invoiceInstance.InvoiceNumber}, Amount = {invoiceInstance.Amount:C}");
