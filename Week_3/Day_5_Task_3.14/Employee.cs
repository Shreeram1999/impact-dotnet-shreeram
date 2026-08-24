// Task 3.14 - IComparable<T> vs IComparer<T>.
//
// `IComparable<Employee>` is implemented BY Employee itself - it defines
// Employee's own "natural" or "default" sort order. Any code that calls
// `someList.Sort()` with no arguments will use this automatically.
public class Employee : IComparable<Employee>
{
    public string Name { get; }
    public decimal Salary { get; }

    public Employee(string name, decimal salary)
    {
        Name = name;
        Salary = salary;
    }

    // CompareTo returns: negative if THIS employee sorts before `other`,
    // zero if they're equal, positive if THIS employee sorts after
    // `other`. decimal already has its own CompareTo that does exactly
    // this for numbers, so we just delegate to it - this makes salary the
    // "default"/"natural" way to sort a list of Employees.
    public int CompareTo(Employee? other)
    {
        if (other is null)
            return 1;

        return Salary.CompareTo(other.Salary);
    }

    public override string ToString() => $"{Name} ({Salary:C})";
}
