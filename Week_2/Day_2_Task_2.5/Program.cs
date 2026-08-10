var triangle = new RightTriangle(3, 4);

triangle.Draw();
Console.WriteLine($"Area: {triangle.CalculateArea():F2}");
Console.WriteLine($"Perimeter: {triangle.CalculatePerimeter():F2}");

// `triangle` is one single object in memory. Here we're pointing two
// DIFFERENT variables at that same object - one declared as IShape, one as
// IDrawable. C# allows this because RightTriangle implements both
// interfaces. ReferenceEquals checks "are these two variables pointing at
// literally the same object?" - the answer is true, proving it's one
// object wearing two different "hats", not two separate objects.
IShape shapeView = triangle;
IDrawable drawableView = triangle;
Console.WriteLine($"Same instance through both interfaces: {ReferenceEquals(shapeView, drawableView)}");
