// Mini Project Q4 - Employee Payroll.
// Employee is abstract (same idea as Task 2.4's Shape): every real
// employee type computes salary differently, so we can't write a generic
// "CalculateSalary" formula here - each subclass MUST supply its own.
public abstract class Employee
{
    public string Name { get; }
    public string Department { get; }

    // `protected` (instead of `public`) means only Employee itself and its
    // subclasses can call this constructor - outside code can never do
    // `new Employee(...)` directly anyway, since it's abstract, but
    // `protected` documents the intent clearly.
    protected Employee(string name, string department)
    {
        Name = name;
        Department = department;
    }

    public abstract double CalculateSalary();
}

// A separate interface (not baked into the Employee hierarchy) because
// only SOME employees are taxed - see below.
public interface ITaxable
{
    double CalculateTax();
}

// FullTimeEmployee implements BOTH Employee (it "is an" employee) AND
// ITaxable (it "can" calculate tax). PartTimeEmployee and ContractEmployee
// further down only implement Employee, not ITaxable - modeling "taxable"
// as an optional extra ability, rather than a field every employee type
// would need to carry around even when it doesn't apply to them.
public class FullTimeEmployee : Employee, ITaxable
{
    public double MonthlySalary { get; }

    // `const` means this value is fixed at compile time and can never
    // change - a simple way to name a "magic number" (10%) so it's clear
    // what it means.
    private const double TaxRate = 0.10;

    public FullTimeEmployee(string name, string department, double monthlySalary)
        : base(name, department)
    {
        MonthlySalary = monthlySalary;
    }

    public override double CalculateSalary() => MonthlySalary;

    public double CalculateTax() => MonthlySalary * TaxRate;
}

public class PartTimeEmployee : Employee
{
    public double HourlyRate { get; }
    public double HoursWorked { get; }

    public PartTimeEmployee(string name, string department, double hourlyRate, double hoursWorked)
        : base(name, department)
    {
        HourlyRate = hourlyRate;
        HoursWorked = hoursWorked;
    }

    public override double CalculateSalary() => HourlyRate * HoursWorked;
}

public class ContractEmployee : Employee
{
    public double ContractAmount { get; }

    public ContractEmployee(string name, string department, double contractAmount)
        : base(name, department)
    {
        ContractAmount = contractAmount;
    }

    public override double CalculateSalary() => ContractAmount;
}
