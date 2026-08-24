// Same small trick used in Week 2's Repository<T> task: any entity that
// wants to live in one of our repositories needs an Id, so both Student
// and Course implement this shared interface.
public interface IEntity
{
    int Id { get; set; }
}

public class Student : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => $"Student #{Id}: {Name}";
}

public class Course : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public override string ToString() => $"Course #{Id}: {Title}";
}
