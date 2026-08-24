using StudentManagementApp.Models;

namespace StudentManagementApp.Views;

// Task 4.10 (stretch) - Teacher's own thin, logic-free view, mirroring
// StudentView.
public class TeacherView
{
    public string ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("--- Teacher Menu ---");
        Console.WriteLine("1. Add Teacher");
        Console.WriteLine("2. List All Teachers");
        Console.WriteLine("3. Find Teacher by Id");
        Console.WriteLine("4. Update Teacher");
        Console.WriteLine("5. Delete Teacher");
        Console.WriteLine("0. Back");
        Console.Write("Choose an option: ");
        return Console.ReadLine() ?? string.Empty;
    }

    public void PrintTeachers(IEnumerable<Teacher> teachers)
    {
        var list = teachers.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("No teachers to show.");
            return;
        }

        Console.WriteLine($"{"Id",-4} {"Name",-20} {"Designation",-15} {"Email",-25}");
        Console.WriteLine(new string('-', 65));
        foreach (var teacher in list)
            Console.WriteLine($"{teacher.Id,-4} {teacher.Name,-20} {teacher.Designation,-15} {teacher.Email,-25}");
    }

    public (string Name, string Email, string Designation) PromptForTeacher()
    {
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Email: ");
        var email = Console.ReadLine() ?? string.Empty;

        Console.Write("Designation: ");
        var designation = Console.ReadLine() ?? string.Empty;

        return (name, email, designation);
    }

    public int PromptForId()
    {
        Console.Write("Teacher Id: ");
        int value;
        while (!int.TryParse(Console.ReadLine(), out value))
            Console.Write("Please enter a whole number: ");

        return value;
    }

    public void ShowMessage(string message) => Console.WriteLine(message);
}
