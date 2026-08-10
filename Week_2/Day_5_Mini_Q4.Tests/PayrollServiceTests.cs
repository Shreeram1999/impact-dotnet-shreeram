// This file tests each employee type's own salary formula individually,
// checks that ITaxable is present on FullTimeEmployee but genuinely absent
// on the other two (not just untested - actually impossible to call), and
// exercises both PayrollService LINQ helpers, including what happens with
// an empty list (an easy case to accidentally break).
public class PayrollServiceTests
{
    [Fact]
    public void FullTimeEmployee_CalculateSalary_ReturnsMonthlySalary()
    {
        var employee = new FullTimeEmployee("Asha", "Engineering", 85000);
        Assert.Equal(85000, employee.CalculateSalary());
    }

    [Fact]
    public void PartTimeEmployee_CalculateSalary_ReturnsRateTimesHours()
    {
        var employee = new PartTimeEmployee("Meera", "Engineering", 500, 80);
        Assert.Equal(40000, employee.CalculateSalary());
    }

    [Fact]
    public void ContractEmployee_CalculateSalary_ReturnsContractAmount()
    {
        var employee = new ContractEmployee("Karan", "Sales", 60000);
        Assert.Equal(60000, employee.CalculateSalary());
    }

    [Fact]
    public void FullTimeEmployee_CalculateTax_IsTenPercentOfSalary()
    {
        ITaxable employee = new FullTimeEmployee("Asha", "Engineering", 85000);
        Assert.Equal(8500, employee.CalculateTax());
    }

    [Fact]
    public void PartTimeAndContractEmployees_AreNotTaxable()
    {
        // Assert.IsNotAssignableFrom checks that the object CANNOT be used
        // as an ITaxable - proving these two types truly don't implement
        // the interface (there's no CalculateTax() to even call on them).
        Employee partTime = new PartTimeEmployee("Meera", "Engineering", 500, 80);
        Employee contract = new ContractEmployee("Karan", "Sales", 60000);

        Assert.IsNotAssignableFrom<ITaxable>(partTime);
        Assert.IsNotAssignableFrom<ITaxable>(contract);
    }

    [Fact]
    public void CalculateTotalPayroll_MixedEmployeeTypes_SumsViaPolymorphism()
    {
        List<Employee> employees =
        [
            new FullTimeEmployee("Asha", "Engineering", 85000),
            new PartTimeEmployee("Meera", "Engineering", 500, 80),
            new ContractEmployee("Karan", "Sales", 60000),
        ];

        Assert.Equal(185000, PayrollService.CalculateTotalPayroll(employees));
    }

    [Fact]
    public void CalculateTotalPayroll_EmptyList_ReturnsZero()
    {
        Assert.Equal(0, PayrollService.CalculateTotalPayroll([]));
    }

    [Fact]
    public void GroupByDepartment_MultipleDepartments_GroupsAndSumsCorrectly()
    {
        List<Employee> employees =
        [
            new FullTimeEmployee("Asha", "Engineering", 85000),
            new PartTimeEmployee("Meera", "Engineering", 500, 80),
            new ContractEmployee("Karan", "Sales", 60000),
            new FullTimeEmployee("Divya", "Sales", 55000),
        ];

        var groups = PayrollService.GroupByDepartment(employees).ToDictionary(g => g.Department);

        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups["Engineering"].Count);
        Assert.Equal(125000, groups["Engineering"].TotalSalary);
        Assert.Equal(2, groups["Sales"].Count);
        Assert.Equal(115000, groups["Sales"].TotalSalary);
    }
}
