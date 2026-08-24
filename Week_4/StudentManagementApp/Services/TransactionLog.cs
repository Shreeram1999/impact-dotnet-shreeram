namespace StudentManagementApp.Services;

// Task 4.9 - reusing the encapsulation idea from Weeks 1-2: the history
// itself is a private field, and the ONLY way anything outside this class
// can add to it is through Record(...). Callers can look at the log
// (History), but they can never reach in and add, remove, or reorder an
// entry directly - the class alone controls how its own state changes.
//
// This is registered as a Singleton in the DI container (see Program.cs /
// Task 4.8) and injected into BOTH StudentService and TeacherService, so
// one shared log captures every mutation across both entities in the order
// they actually happened.
public class TransactionLog
{
    private readonly List<string> history = new();

    public void Record(string action) => history.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}");

    public IReadOnlyList<string> History => history.AsReadOnly();
}
