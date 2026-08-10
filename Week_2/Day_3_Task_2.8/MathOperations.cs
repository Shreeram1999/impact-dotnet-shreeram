// Task 2.8 - delegates.
// A delegate is basically a "variable that holds a method" instead of
// holding a number or a string. `MathOperation` here describes the SHAPE
// a method must have to be storable in this delegate: it must take two
// doubles and return a double. Add/Subtract/Multiply/Divide below all
// match that shape, so any of them can be assigned to a MathOperation
// variable (see Program.cs).
public delegate double MathOperation(double a, double b);

public static class MathOperations
{
    // `static` methods belong to the class itself, not to any particular
    // instance - that's why Program.cs can reference them directly as
    // `MathOperations.Add` without first writing `new MathOperations()`.
    public static double Add(double a, double b) => a + b;
    public static double Subtract(double a, double b) => a - b;
    public static double Multiply(double a, double b) => a * b;
    public static double Divide(double a, double b) => a / b;
}
