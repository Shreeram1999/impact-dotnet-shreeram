// Overload resolution in action: the compiler looks at the number and
// type of arguments in each call below, and picks the matching Add()
// overload from Calculator.cs automatically. You never say WHICH overload
// you want - the compiler works it out for you.
var calculator = new Calculator();
Console.WriteLine($"Add(int,int): {calculator.Add(2, 3)}");
Console.WriteLine($"Add(double,double): {calculator.Add(2.5, 3.5)}");
Console.WriteLine($"Add(int,int,int): {calculator.Add(1, 2, 3)}");
Console.WriteLine($"Add(params int[]): {calculator.Add(1, 2, 3, 4, 5)}");

// Runtime polymorphism, for contrast: `shapes` is a List<Shape>, so the
// loop variable is always typed as Shape - yet CalculateArea() still runs
// the correct Circle or Rectangle version for each item, decided at
// runtime based on what the object actually is.
Console.WriteLine();
Console.WriteLine("Runtime polymorphism over List<Shape>:");
List<Shape> shapes = [new Circle(2), new Rectangle(3, 4)];
foreach (var shape in shapes)
    Console.WriteLine($"  {shape.GetType().Name} area: {shape.CalculateArea():F2}");

// Method hiding demo: `fileLogger` and `baseReference` point at the exact
// same object in memory. The only difference is the DECLARED type of the
// variable - `fileLogger` is typed FileLogger, `baseReference` is typed
// Logger. Because Log() uses `new` (not `override`), each variable calls
// the version matching ITS OWN declared type, not the object's real type.
Console.WriteLine();
Console.WriteLine("Method hiding (new) vs overriding (override):");
var fileLogger = new FileLogger();
Logger baseReference = fileLogger;

fileLogger.Log("via derived reference");   // compile-time type FileLogger -> FileLogger.Log runs
baseReference.Log("via base reference");   // compile-time type Logger    -> Logger.Log runs (hiding, not polymorphic)
