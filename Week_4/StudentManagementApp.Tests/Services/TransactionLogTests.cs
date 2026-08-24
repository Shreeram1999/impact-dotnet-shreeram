using StudentManagementApp.Services;

namespace StudentManagementApp.Tests.Services;

// Task 4.9 - the log itself, independent of Student/Teacher: entries land in
// the order they were recorded, and History is read-only from the outside.
public class TransactionLogTests
{
    [Fact]
    public void Record_MultipleEntries_KeepsThemInOrder()
    {
        var log = new TransactionLog();

        log.Record("First action");
        log.Record("Second action");
        log.Record("Third action");

        Assert.Equal(3, log.History.Count);
        Assert.Contains("First action", log.History[0]);
        Assert.Contains("Second action", log.History[1]);
        Assert.Contains("Third action", log.History[2]);
    }

    [Fact]
    public void History_IsReadOnly_CannotBeCastBackToAMutableList()
    {
        var log = new TransactionLog();
        log.Record("An action");

        Assert.IsNotType<List<string>>(log.History);
    }
}
