// A small, self-contained Shape/Circle/Rectangle set-up (same idea as
// Day_2_Task_2.4, just re-declared here so this project doesn't depend on
// another project). Program.cs uses this to demonstrate RUNTIME
// polymorphism, as a contrast to the COMPILE-TIME overload resolution
// shown in Calculator.cs above.
public abstract class Shape
{
    public abstract double CalculateArea();
}

public class Circle : Shape
{
    public double Radius { get; }
    public Circle(double radius) => Radius = radius;
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
