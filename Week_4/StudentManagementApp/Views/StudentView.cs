using StudentManagementApp.Models;

namespace StudentManagementApp.Views;

// Task 4.5 - View = console rendering, no logic.
//
// Every method here either prints something or reads raw console input -
// there is no `if (age < 5)`, no duplicate-check, nothing that decides
// whether data is VALID. The one `if` in PromptForStudent (retrying when
// int.TryParse fails) is a UI concern - "the user typed letters where a
// number goes" - not a business rule; it never touches what counts as a
// valid age (5-100), which stays entirely in the Model/Service.
public class StudentView
{
    public string ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("--- Student Menu ---");
        Console.WriteLine("1. Add Student");
        Console.WriteLine("2. List All Students");
        Console.WriteLine("3. Find Student by Id");
        Console.WriteLine("4. Update Student");
        Console.WriteLine("5. Delete Student");
        Console.WriteLine("0. Back");
        Console.Write("Choose an option: ");
        return Console.ReadLine() ?? string.Empty;
    }

    public void PrintStudents(IEnumerable<Student> students)
    {
        var list = students.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("No students to show.");
            return;
        }

        Console.WriteLine($"{"Id",-4} {"Name",-20} {"Age",-4} {"Roll No.",-12} {"Email",-25}");
        Console.WriteLine(new string('-', 65));
        foreach (var student in list)
            Console.WriteLine($"{student.Id,-4} {student.Name,-20} {student.Age,-4} {student.RollNumber,-12} {student.Email,-25}");
    }

    public (string Name, int Age, string RollNumber, string Email) PromptForStudent()
    {
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Age: ");
        var age = ReadInt();

        Console.Write("Roll Number: ");
        var rollNumber = Console.ReadLine() ?? string.Empty;

        Console.Write("Email: ");
        var email = Console.ReadLine() ?? string.Empty;

        return (name, age, rollNumber, email);
    }

    public int PromptForId()
    {
        Console.Write("Student Id: ");
        return ReadInt();
    }

    public void ShowMessage(string message) => Console.WriteLine(message);

    // Retries on unparsable input - a UI-input concern, not a business rule.
    private static int ReadInt()
    {
        int value;
        while (!int.TryParse(Console.ReadLine(), out value))
            Console.Write("Please enter a whole number: ");

        return value;
    }
}
