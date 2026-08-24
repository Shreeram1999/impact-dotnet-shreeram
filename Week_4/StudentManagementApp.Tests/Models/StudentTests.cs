using StudentManagementApp.Models;

namespace StudentManagementApp.Tests.Models;

// The Model's own validation, per Task 4.1 and the Testing Focus section's
// "second, smaller target" - the age invariant (5-100) enforced directly by
// Student.Age's setter, independent of anything in /Services.
public class StudentTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(101)]
    [InlineData(-5)]
    public void Age_OutOfRange_Throws(int invalidAge)
    {
        var student = new Student();

        Assert.Throws<ArgumentOutOfRangeException>(() => student.Age = invalidAge);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(21)]
    public void Age_WithinRange_IsAccepted(int validAge)
    {
        var student = new Student { Age = validAge };

        Assert.Equal(validAge, student.Age);
    }
}
