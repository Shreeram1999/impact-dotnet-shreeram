public class EmployeeSortingTests
{
    [Fact]
    public void CompareTo_LowerSalary_SortsBefore()
    {
        var lower = new Employee("A", 30000);
        var higher = new Employee("B", 50000);

        Assert.True(lower.CompareTo(higher) < 0);
        Assert.True(higher.CompareTo(lower) > 0);
    }

    [Fact]
    public void CompareTo_EqualSalary_ReturnsZero()
    {
        var a = new Employee("A", 50000);
        var b = new Employee("B", 50000);

        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_Null_SortsAfter()
    {
        var employee = new Employee("A", 50000);

        Assert.True(employee.CompareTo(null) > 0);
    }

    [Fact]
    public void DefaultSort_OrdersEmployeesBySalaryAscending()
    {
        List<Employee> employees =
        [
            new("Farhan", 72000),
            new("Meera", 45000),
            new("Asha", 85000),
        ];

        employees.Sort();

        Assert.Equal(["Meera", "Farhan", "Asha"], employees.Select(e => e.Name));
    }

    [Fact]
    public void EmployeeNameComparer_OrdersEmployeesByNameAlphabetically()
    {
        List<Employee> employees =
        [
            new("Farhan", 72000),
            new("Asha", 85000),
            new("Meera", 45000),
        ];

        employees.Sort(new EmployeeNameComparer());

        Assert.Equal(["Asha", "Farhan", "Meera"], employees.Select(e => e.Name));
    }

    [Fact]
    public void EmployeeNameComparer_DoesNotChangeWhatDefaultSortDoes()
    {
        // Sorting by name with the comparer, then sorting again with the
        // default Sort(), should give back the salary order - proving the
        // comparer is a completely separate, non-destructive alternative
        // rather than something that permanently changes Employee's
        // default order.
        List<Employee> employees =
        [
            new("Farhan", 72000),
            new("Meera", 45000),
            new("Asha", 85000),
        ];

        employees.Sort(new EmployeeNameComparer());
        employees.Sort();

        Assert.Equal(["Meera", "Farhan", "Asha"], employees.Select(e => e.Name));
    }
}
