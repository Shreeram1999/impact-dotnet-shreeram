// Task 2.6 - method overloading.
// "Overloading" means giving several methods the SAME NAME but different
// parameter lists (different types, or a different number of parameters).
// The compiler figures out which one you meant based on what you pass in,
// at COMPILE time - this is different from the runtime polymorphism you
// saw with Shape/Vehicle, where the decision happens while the program runs.
public class Calculator
{
    public int Add(int a, int b) => a + b;

    // Same name "Add", but this one takes doubles instead of ints - a
    // different overload.
    public double Add(double a, double b) => a + b;

    // Same name again, but three parameters instead of two.
    public int Add(int a, int b, int c) => a + b + c;

    // `params int[] numbers` lets the CALLER pass any number of ints
    // (zero, one, five, whatever) as if they were separate arguments, e.g.
    // `calculator.Add(1, 2, 3, 4, 5)`. Behind the scenes C# packs them all
    // into one int array named `numbers` for us to use.
    public int Add(params int[] numbers) => numbers.Sum();
}
