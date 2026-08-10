// Each of these variables is a MathOperation delegate "pointing at" one of
// the static methods from MathOperations.cs. Once assigned, calling
// `add(4, 2)` is just like calling `MathOperations.Add(4, 2)` - the
// delegate forwards the call to whichever method it's currently holding.
MathOperation add = MathOperations.Add;
MathOperation subtract = MathOperations.Subtract;
MathOperation multiply = MathOperations.Multiply;
MathOperation divide = MathOperations.Divide;

Console.WriteLine($"Add(4, 2) = {add(4, 2)}");
Console.WriteLine($"Subtract(4, 2) = {subtract(4, 2)}");
Console.WriteLine($"Multiply(4, 2) = {multiply(4, 2)}");
Console.WriteLine($"Divide(4, 2) = {divide(4, 2)}");

Console.WriteLine();
Console.WriteLine("Multicast delegate (Add + Multiply):");
// Delegates aren't limited to holding just ONE method - using `+=` combines
// several methods into a single "multicast" delegate. Calling `combined(...)`
// would then run BOTH Add and Multiply, one after another.
MathOperation combined = add;
combined += multiply;

// Here's a gotcha worth knowing: if you just called `combined(4, 2)`
// directly, C# would run both methods but only hand you back the result of
// the LAST one (Multiply) - the Add result would be silently discarded.
// GetInvocationList() gives you every method inside the multicast delegate
// as a separate list, so you can call each one individually and see both
// results.
foreach (MathOperation op in combined.GetInvocationList())
    Console.WriteLine($"  {op.Method.Name}(4, 2) = {op(4, 2)}");

Console.WriteLine();
Console.WriteLine("Same operations expressed with Func<double,double,double>:");
// .NET already ships a family of ready-made generic delegate types so you
// don't have to declare your own `delegate ...` every time. Func<double,
// double,double> means "a method that takes two doubles in and returns one
// double" - exactly the same shape as our custom MathOperation delegate
// above, just using a built-in name instead.
Func<double, double, double> addFunc = MathOperations.Add;
Func<double, double, double> multiplyFunc = MathOperations.Multiply;

Console.WriteLine($"addFunc(4, 2) = {addFunc(4, 2)}");
Console.WriteLine($"multiplyFunc(4, 2) = {multiplyFunc(4, 2)}");
