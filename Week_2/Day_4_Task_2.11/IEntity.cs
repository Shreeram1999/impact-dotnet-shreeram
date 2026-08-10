// Task 2.11 - generics.
// IEntity is a tiny interface that just guarantees "this type has an Id".
// Repository<T> (in Repository.cs) needs SOME way to find/replace/remove a
// specific item, and it can't assume every possible T has an Id property
// unless we say so explicitly - that's what this interface is for.
public interface IEntity
{
    int Id { get; set; }
}

// Student and Product have nothing in common except both implementing
// IEntity - they don't inherit from each other, and one isn't "a kind of"
// the other. That's intentional: it proves Repository<T> further down
// really can work with any unrelated type, as long as that type has an Id.
public class Student : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;

    // Overriding ToString() controls what gets printed when you do
    // Console.WriteLine(someStudent) or use it in a $"..." string, like
    // Program.cs does below.
    public override string ToString() => $"Student #{Id}: {Name} ({Grade})";
}

public class Product : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString() => $"Product #{Id}: {Name} ({Price:C})";
}
