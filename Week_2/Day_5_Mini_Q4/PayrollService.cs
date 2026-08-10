public static class PayrollService
{
    // `IEnumerable<Employee>` is called out here (instead of List<Employee>)
    // to accept ANY collection type that can be looped over, not just a
    // List specifically - it's the most flexible/general parameter type
    // for "give me some employees to sum up".
    //
    // Even though this only sees each item as `Employee`, calling
    // e.CalculateSalary() runs the CORRECT override for whatever the
    // object actually is (FullTime/PartTime/Contract) - that's runtime
    // polymorphism again, same idea as Task 2.4/2.6.
    public static double CalculateTotalPayroll(IEnumerable<Employee> employees) =>
        employees.Sum(e => e.CalculateSalary());

    // GroupBy(e => e.Department) buckets employees by department (like
    // Task 2.13's grouping query), and Select(...) reduces each bucket
    // down to a small summary. The return type here is a tuple
    // `(string Department, double TotalSalary, int Count)` - similar in
    // spirit to an anonymous type, but with a name you CAN write as a
    // return type (unlike the anonymous type problem from Task 2.14).
    public static IEnumerable<(string Department, double TotalSalary, int Count)> GroupByDepartment(
        IEnumerable<Employee> employees) =>
        employees
            .GroupBy(e => e.Department)
            .Select(g => (Department: g.Key, TotalSalary: g.Sum(e => e.CalculateSalary()), Count: g.Count()));
}
