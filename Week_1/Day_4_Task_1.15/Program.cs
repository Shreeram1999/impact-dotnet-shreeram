// ===== Part 1: type pattern matching on an object parameter =====
Describe(42);
Describe("hello");
Describe(3.14);
Describe(null);

// ===== Part 2: switch expression with relational patterns =====
Console.WriteLine($"Score 95 -> Grade {GetGrade(95)}");
Console.WriteLine($"Score 82 -> Grade {GetGrade(82)}");
Console.WriteLine($"Score 71 -> Grade {GetGrade(71)}");
Console.WriteLine($"Score 40 -> Grade {GetGrade(40)}");

// ===== Part 3: property patterns on an Order =====
Console.WriteLine($"Order (Shipped, 250) -> discount {GetDiscount(new Order("Shipped", 250)):P0}");
Console.WriteLine($"Order (Pending, 250) -> discount {GetDiscount(new Order("Pending", 250)):P0}");
Console.WriteLine($"Order (Shipped, 50)  -> discount {GetDiscount(new Order("Shipped", 50)):P0}");

static void Describe(object? input)
{
    // The "is" pattern here does double duty: it checks the runtime type AND
    // binds it to a strongly-typed variable, all in one switch branch.
    string result = input switch
    {
        null => "It's null",
        int i => $"It's an int: {i}",
        string s => $"It's a string of length {s.Length}: \"{s}\"",
        double d => $"It's a double: {d}",
        _ => "Unknown type" // fallback — required so the switch is exhaustive
    };
    Console.WriteLine(result);
}

static char GetGrade(int score) => score switch
{
    >= 90 => 'A',   // relational pattern: score >= 90
    >= 75 => 'B',   // only reached if the previous arm didn't match, i.e. 75-89
    >= 60 => 'C',   // 60-74
    _ => 'F'        // anything else (< 60)
};

static double GetDiscount(Order order) => order switch
{
    // Property patterns destructure the object's properties directly in the pattern.
    { Status: "Shipped", Amount: > 100 } => 0.10, // 10% off shipped orders over 100
    { Status: "Shipped" } => 0.05,                // any other shipped order: 5%
    { Status: "Pending" } => 0.0,                 // no discount while pending
    _ => 0.0
};

record Order(string Status, double Amount);
