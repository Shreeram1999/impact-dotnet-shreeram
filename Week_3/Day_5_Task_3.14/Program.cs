List<Employee> employees =
[
    new("Farhan", 72000),
    new("Asha", 85000),
    new("Meera", 45000),
    new("Karan", 55000),
    new("Divya", 48000),
    new("Priya", 51000),
    new("Vikram", 39000),
    new("Neha", 67000),
    new("Suresh", 58000),
    new("Anjali", 44000),
];

// `Sort()` with no arguments uses Employee's own CompareTo() -
// IComparable<Employee> - which we wrote to compare by Salary.
var bySalary = new List<Employee>(employees);
bySalary.Sort();
Console.WriteLine("Sorted by salary (default Sort(), via IComparable<Employee>):");
foreach (var e in bySalary)
    Console.WriteLine($"  {e}");

Console.WriteLine();

// Passing a SEPARATE IComparer<Employee> object overrides that default,
// without changing anything about Employee itself.
var byName = new List<Employee>(employees);
byName.Sort(new EmployeeNameComparer());
Console.WriteLine("Sorted by name (Sort(EmployeeNameComparer), via IComparer<Employee>):");
foreach (var e in byName)
    Console.WriteLine($"  {e}");
