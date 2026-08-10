// A mix of all three employee types, all stored together as
// `List<Employee>`. The list itself doesn't "know" which items are
// full-time vs part-time vs contract - that information only matters when
// we call CalculateSalary() (polymorphism) or filter by ITaxable (below).
List<Employee> employees =
[
    new FullTimeEmployee("Asha", "Engineering", 85000),
    new FullTimeEmployee("Rohit", "Engineering", 72000),
    new PartTimeEmployee("Meera", "Engineering", 500, 80),
    new ContractEmployee("Karan", "Sales", 60000),
    new FullTimeEmployee("Divya", "Sales", 55000),
    new PartTimeEmployee("Farhan", "HR", 450, 60),
];

Console.WriteLine("Per-employee salary (polymorphic CalculateSalary):");
foreach (var e in employees)
    Console.WriteLine($"  {e.Name} ({e.GetType().Name}): {e.CalculateSalary():C}");

// `OfType<ITaxable>()` is a handy LINQ method: it walks the list and keeps
// only the items that implement ITaxable, automatically converting them to
// that interface type. Since only FullTimeEmployee implements ITaxable,
// this filters straight to just the full-timers, with no manual
// type-checking `if (e is FullTimeEmployee)` needed.
Console.WriteLine();
Console.WriteLine("Tax (ITaxable, full-time only):");
foreach (var taxable in employees.OfType<ITaxable>())
    Console.WriteLine($"  {((Employee)taxable).Name}: {taxable.CalculateTax():C}");

var totalPayroll = PayrollService.CalculateTotalPayroll(employees);
Console.WriteLine();
Console.WriteLine($"Total payroll: {totalPayroll:C}");

Console.WriteLine();
Console.WriteLine("By department:");
foreach (var group in PayrollService.GroupByDepartment(employees))
    Console.WriteLine($"  {group.Department}: {group.Count} employees, total {group.TotalSalary:C}");
