// Task 2.5 - interfaces, and implementing more than one on the same class.
// An interface is like a checklist/contract: it lists method signatures
// with NO bodies, and any class that "implements" the interface is
// promising to provide real code for every item on that checklist.
public interface IShape
{
    double CalculateArea();
    double CalculatePerimeter();
}

public interface IDrawable
{
    void Draw();
}

// Interface vs abstract class - a common newbie question:
// - An INTERFACE (IShape, IDrawable) is just a list of promises: no fields,
//   no shared code (mostly). A class can implement as many interfaces as
//   it wants. Use it for "this type CAN DO X", even for otherwise unrelated
//   things - here, a triangle both "is a shape" and "can be drawn", which
//   are two separate abilities.
// - An ABSTRACT CLASS (see Day_2_Task_2.4/Shape.cs) CAN hold real fields and
//   shared method bodies, but a class can only inherit from ONE class
//   (single inheritance in C#). Use it for "this type IS-A X" when several
//   subclasses share common code.
//
// RightTriangle below implements BOTH interfaces at once - proof that one
// class can satisfy multiple unrelated contracts simultaneously, something
// a single abstract base class can't do as cleanly.
public class RightTriangle : IShape, IDrawable
{
    public double Base { get; }
    public double Height { get; }

    public RightTriangle(double @base, double height)
    {
        // `@base` (with the `@`) is needed because `base` alone is a
        // reserved C# keyword (used for calling the parent class, as seen
        // in Task 2.2). The `@` prefix tells the compiler "treat this as a
        // plain identifier, not the keyword".
        Base = @base;
        Height = height;
    }

    // `private` and no interface lists this - it's just an internal helper
    // used by CalculatePerimeter() below, not something callers need to see.
    private double Hypotenuse => Math.Sqrt(Base * Base + Height * Height);

    // These next three methods are how RightTriangle fulfills its promises
    // to IShape (the first two) and IDrawable (the last one).
    public double CalculateArea() => 0.5 * Base * Height;

    public double CalculatePerimeter() => Base + Height + Hypotenuse;

    public void Draw()
    {
        Console.WriteLine($"Drawing a right triangle (base={Base}, height={Height}):");
        Console.WriteLine("|\\");
        Console.WriteLine("| \\");
        Console.WriteLine("|__\\");
    }
}
