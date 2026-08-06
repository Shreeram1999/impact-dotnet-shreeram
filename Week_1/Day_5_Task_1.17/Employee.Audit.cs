public partial class Employee
{
    // This is the IMPLEMENTATION half of the partial method declared in
    // Employee.Core.cs. Because an implementation exists, the compiler
    // keeps the OnEmployeeCreated() call in the constructor and wires it to
    // this method body.
    partial void OnEmployeeCreated()
    {
        Console.WriteLine($"[Audit] Employee record created for {Name} at salary {Salary:C}");
    }
}
