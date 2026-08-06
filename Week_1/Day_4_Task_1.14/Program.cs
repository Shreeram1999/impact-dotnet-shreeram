int[] numbers = { 15, 4, 27, 9, 42, 1 };

// ---- Named tuple return: (int Min, int Max) ----
(int Min, int Max) range = GetMinMax(numbers);
Console.WriteLine($"Min = {range.Min}, Max = {range.Max}"); // access by the names given in the signature

// ---- Deconstruction: unpack the tuple straight into separate variables ----
var (min, max) = GetMinMax(numbers);
Console.WriteLine($"Deconstructed -> min={min}, max={max}");

// ---- Employee lookup returning a 3-element named tuple ----
var employee = LookupEmployee(101);
Console.WriteLine($"Lookup by property: {employee.Name}, {employee.Age}, {employee.Department}");

var (empName, empAge, empDept) = LookupEmployee(101); // deconstruction again
Console.WriteLine($"Deconstructed -> {empName}, age {empAge}, dept {empDept}");

static (int Min, int Max) GetMinMax(int[] values)
{
    int min = values[0];
    int max = values[0];
    foreach (int v in values)
    {
        if (v < min) min = v;
        if (v > max) max = v;
    }
    // The returned literal tuple's member names don't even need to be
    // repeated here — they're already declared in the method's return type.
    return (min, max);
}

static (string Name, int Age, string Department) LookupEmployee(int employeeId)
{
    // Stand-in for what would normally be a database/repository lookup.
    return ("Priya Sharma", 29, "Engineering");
}
