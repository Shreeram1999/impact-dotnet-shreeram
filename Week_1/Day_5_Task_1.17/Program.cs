// ===== Partial class + partial method =====
var employee = new Employee("Deepa Rao", 75000); // triggers OnEmployeeCreated() in Employee.Audit.cs
Console.WriteLine();

// ===== Access modifier matrix =====
var demo = new DerivedAccessDemo();
demo.ShowFromInside();
demo.ShowFromDerived();
Console.WriteLine();

// ===== Record: value equality with ==, and non-destructive copy with 'with' =====
var address1 = new Address("221B Baker Street", "London", "NW16XE");
var address2 = new Address("221B Baker Street", "London", "NW16XE");
var address3 = address1 with { City = "Manchester" }; // copies everything except City

Console.WriteLine($"address1 == address2 ? {address1 == address2}"); // True: records compare by VALUE, not reference
Console.WriteLine($"address1: {address1}");
Console.WriteLine($"address3 (with City changed): {address3}");
Console.WriteLine($"address1 == address3 ? {address1 == address3}"); // False: City differs
Console.WriteLine();

// ===== Playlist indexer with bounds checking + string indexer =====
var playlist = new Playlist();
playlist.Add("Bohemian Rhapsody");
playlist.Add("Hotel California");
playlist.Add("Stairway to Heaven");

Console.WriteLine($"playlist[1] = {playlist[1]}"); // int indexer

int foundIndex = playlist["hotel california"]; // string indexer, case-insensitive
Console.WriteLine($"playlist[\"hotel california\"] found at index {foundIndex}");

try
{
    var outOfRange = playlist[10];
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine($"Bounds check worked: {ex.Message}");
}
