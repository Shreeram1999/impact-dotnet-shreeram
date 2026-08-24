using StudentManagementApp.Controllers;
using StudentManagementApp.Data;
using StudentManagementApp.Models;
using StudentManagementApp.Services;
using StudentManagementApp.Views;

namespace StudentManagementApp;

// Task 4.7 - manual DI: new up repo -> service -> controller by hand.
//
// Program.cs calls Demonstrate() once at startup, then moves on to build the
// SAME kind of object graph again using the Microsoft DI container
// (Task 4.8) for the actual interactive session. Keeping this method around
// - rather than deleting it once the container refactor landed - is what
// lets both Day 4 "done when" checks stay provable from the committed code:
// the manual chain below still compiles and runs, and DI_Notes.md walks
// through exactly what changed on the way to the container version.
public static class ManualDiBootstrap
{
    public static void Demonstrate()
    {
        Console.WriteLine("--- Task 4.7: manual DI wiring ---");

        // This line is the only "which repository implementation" seam in
        // the whole chain below. Swapping InMemoryRepository<Student> for
        // any other IRepository<Student> implementation would mean changing
        // ONLY this line - StudentService, StudentController and StudentView
        // are all written against interfaces/concrete classes that never
        // mention InMemoryRepository by name, so none of them would need to
        // change.
        IRepository<Student> repository = new InMemoryRepository<Student>();

        var transactionLog = new TransactionLog();
        IStudentService service = new StudentService(repository, transactionLog);
        var view = new StudentView();
        var controller = new StudentController(service, view);

        var result = service.AddStudent("Manual-DI Demo Student", 20, "MD-1", "demo@example.com");
        Console.WriteLine($"Manually-wired service call result: {result.Message}");
        Console.WriteLine($"Manually-wired repository now holds {repository.GetAll().Count()} student(s).");
        Console.WriteLine($"Manually-wired controller ready: {controller.GetType().Name} (not run interactively here).");
        Console.WriteLine();
    }
}
