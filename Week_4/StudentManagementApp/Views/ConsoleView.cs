namespace StudentManagementApp.Views;

// Task 4.9/4.10 - the parts of the console UI that aren't specific to
// Student or Teacher: the top-level "which entity am I managing" menu, and
// printing the shared TransactionLog. Kept separate from StudentView so
// that class stays exactly what Task 4.5 asked for, and so this one class
// can be reused by both the Student and Teacher menu loops.
public class ConsoleView
{
    public string ShowMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== Student Management Console App ===");
        Console.WriteLine("1. Manage Students");
        Console.WriteLine("2. Manage Teachers");
        Console.WriteLine("3. View Transaction Log");
        Console.WriteLine("0. Exit");
        Console.Write("Choose an option: ");
        return Console.ReadLine() ?? string.Empty;
    }

    public void PrintTransactionLog(IEnumerable<string> history)
    {
        var entries = history.ToList();
        if (entries.Count == 0)
        {
            Console.WriteLine("No transactions recorded yet.");
            return;
        }

        Console.WriteLine("--- Transaction Log ---");
        foreach (var entry in entries)
            Console.WriteLine(entry);
    }

    public void ShowMessage(string message) => Console.WriteLine(message);
}
