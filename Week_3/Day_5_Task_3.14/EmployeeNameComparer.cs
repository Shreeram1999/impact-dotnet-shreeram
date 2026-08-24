// `IComparer<Employee>` is a SEPARATE object from Employee, used when you
// want an ALTERNATIVE sort order without changing Employee's own "default"
// sort order (which stays salary-based, from IComparable above). You can
// have as many different IComparer<Employee> classes as you want - one
// for name, one for hire date, one for department, etc. - and pass
// whichever one you need into Sort() for that particular call.
public class EmployeeNameComparer : IComparer<Employee>
{
    public int Compare(Employee? x, Employee? y)
    {
        if (x is null || y is null)
            return 0;

        // string.Compare with StringComparison.Ordinal does a simple,
        // predictable character-by-character comparison (as opposed to
        // culture-aware comparison, which can sort differently depending
        // on the user's language/region settings) - a good default when
        // you just want consistent alphabetical sorting.
        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}
