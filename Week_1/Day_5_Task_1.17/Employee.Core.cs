// Splitting a class across files with "partial" is purely organizational —
// at compile time the compiler stitches every "partial class Employee"
// piece back into one single type. Commonly used to separate generated
// code from hand-written code, or to split a large class by concern.
public partial class Employee
{
    public string Name { get; set; }
    public double Salary { get; set; }

    public Employee(string name, double salary)
    {
        Name = name;
        Salary = salary;
        OnEmployeeCreated(); // calls into whichever file implements it
    }

    // A partial method DECLARATION with no body. It must be implicitly
    // private and return void. If no other file provides an implementation,
    // the compiler removes the call site entirely (zero runtime cost) — that
    // is what makes it "optional". Employee.Audit.cs supplies the body.
    partial void OnEmployeeCreated();
}
