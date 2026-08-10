// DisplayArea() itself is defined only once, on Shape - Circle and
// Rectangle don't have their own copy of it. But because DisplayArea()
// calls the abstract CalculateArea(), each shape still ends up printing
// its OWN correct area. This shows abstract classes can mix "shared code"
// (DisplayArea) with "each subclass fills in the blank" code (CalculateArea).
Shape[] shapes =
[
    new Circle(3.0),
    new Rectangle(4.0, 5.0),
];

foreach (var shape in shapes)
    shape.DisplayArea();

Console.WriteLine();
Console.WriteLine("See Shape.cs for the commented-out `new Shape()` attempt");
Console.WriteLine("and the CS0144 compile error it produces.");
