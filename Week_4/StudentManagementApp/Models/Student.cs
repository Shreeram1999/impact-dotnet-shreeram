using StudentManagementApp.Data;

namespace StudentManagementApp.Models;

// Task 4.1 - Model = data only.
//
// This class holds state and enforces the ONE rule that is really an
// invariant of "what a Student even is" (an age of 5-100) directly in its
// own property setter. That is different from a BUSINESS rule like "no
// duplicate roll numbers" - checking against other students requires
// looking at the whole collection, which is exactly what /Services is for
// (see StudentService.AddStudent). A Model can only ever validate itself,
// never compare itself to other records - it has no way to reach the
// repository, and it must not: no Console.* calls and no List<T> anywhere
// in this file, by design.
public class Student : IEntity
{
    private int age;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int Age
    {
        get => age;
        set
        {
            if (value < 5 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "Student age must be between 5 and 100.");

            age = value;
        }
    }

    public override string ToString() => $"Student #{Id}: {Name} (Roll {RollNumber}, Age {Age}, {Email})";
}
