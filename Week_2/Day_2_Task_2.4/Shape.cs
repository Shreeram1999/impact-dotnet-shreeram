// Task 2.4 - abstract classes.
// `abstract` on a class means: you can never do `new Shape()` directly.
// It exists only to be a base class that other classes inherit from.
public abstract class Shape
{
    // `abstract` on a method means: this class does NOT provide an
    // implementation - it just declares "every non-abstract class that
    // inherits from Shape MUST provide its own CalculateArea()". The
    // compiler enforces this for you; Circle and Rectangle below would
    // fail to compile if they forgot to implement it.
    public abstract double CalculateArea();

    // This method is NOT abstract - it has a real body, and every
    // subclass (Circle, Rectangle, ...) inherits this exact same
    // implementation for free. Notice it calls CalculateArea() even
    // though Shape itself doesn't know how to calculate an area - at
    // runtime it'll call whichever subclass's version is actually there.
    public void DisplayArea()
    {
        Console.WriteLine($"{GetType().Name} area: {CalculateArea():F2}");
    }
}

public class Circle : Shape
{
    public double Radius { get; }

    public Circle(double radius) => Radius = radius;

    // `=>` here is an "expression-bodied member" - shorthand for a method
    // whose body is just one expression, equivalent to writing
    // `{ return Math.PI * Radius * Radius; }`.
    public override double CalculateArea() => Math.PI * Radius * Radius;
}

public class Rectangle : Shape
{
    public double Width { get; }
    public double Height { get; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public override double CalculateArea() => Width * Height;
}

// Because Shape is abstract, writing `new Shape()` anywhere in the code
// simply won't compile. This block is commented out on purpose - it's here
// to show you the exact error message you'd get if you tried it:
//
// var shape = new Shape();
//
// CS0144: Cannot create an instance of the abstract type or interface 'Shape'.
