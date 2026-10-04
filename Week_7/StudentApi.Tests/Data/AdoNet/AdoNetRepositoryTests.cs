using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentApi.Auth;
using StudentApi.Data.AdoNet;
using StudentApi.Models;

namespace StudentApi.Tests.Data.AdoNet;

// Tasks 7.2-7.5 against a real (throwaway) SQL Server database. All tests in
// the collection share one database, so each one works on rows it created
// itself (unique roll numbers / usernames) or only reads the seed.
[Collection(SqlServerCollection.Name)]
public class AdoNetRepositoryTests
{
    private readonly SqlServerTestDatabase database;

    public AdoNetRepositoryTests(SqlServerTestDatabase database)
    {
        this.database = database;
    }

    private AdoNetStudentRepository Students()
    {
        Skip.IfNot(database.Available, database.UnavailableReason);
        return new AdoNetStudentRepository(database.ConnectionFactory);
    }

    private static Student NewStudent(string tag, string? name = null) => new()
    {
        Name = name ?? $"Student {tag}",
        Age = 20,
        RollNumber = $"T{tag}",
        Email = $"{tag}@ado.example",
        Score = 60,
        EnrolledOn = new DateOnly(2026, 9, 1)
    };

    // Task 7.3 - THE injection test: the classic payload goes in as a name,
    // comes back out byte-for-byte, and the table is still there.
    [SkippableFact]
    public void InjectionString_IsStoredAsLiteralText_NotExecuted()
    {
        var repository = Students();
        const string payload = "'; DROP TABLE Students;--";
        var student = NewStudent("inj", payload);

        repository.Add(student);

        Assert.Equal(payload, repository.GetById(student.Id)!.Name);
        Assert.True(database.ExecuteScalarInt("SELECT COUNT(*) FROM dbo.Students") >= 4);
    }

    [SkippableFact]
    public void GetAll_ReadsTheSeedThroughSqlDataReader()
    {
        var all = Students().GetAll().ToList();

        var divya = all.Single(s => s.RollNumber == "R003");
        Assert.Equal("Scholarship review pending", divya.InternalNotes);
        Assert.Equal(new DateOnly(2026, 6, 15), divya.EnrolledOn);
        Assert.Equal(2, all.Single(s => s.RollNumber == "R001").UserId);
    }

    // Task 7.4 - usp_GetStudentById / usp_InsertStudent.
    [SkippableFact]
    public void StoredProcedures_InsertReturnsTheNewId_AndGetByIdFindsIt()
    {
        var repository = Students();
        var student = NewStudent("sp");

        repository.Add(student);

        Assert.True(student.Id > 3);
        Assert.Equal("Tsp", repository.GetById(student.Id)!.RollNumber);
    }

    [SkippableFact]
    public void GetById_Missing_ReturnsNull()
    {
        Assert.Null(Students().GetById(987654));
    }

    [SkippableFact]
    public void Update_ExistingRow_Persists_NullEnrolledOnIncluded()
    {
        var repository = Students();
        var student = NewStudent("upd");
        repository.Add(student);

        student.Score = 95;
        student.EnrolledOn = null;
        Assert.True(repository.Update(student));

        var reloaded = repository.GetById(student.Id)!;
        Assert.Equal(95, reloaded.Score);
        Assert.Null(reloaded.EnrolledOn);
    }

    [SkippableFact]
    public void UpdateAndDelete_MissingRow_ReturnFalse()
    {
        var repository = Students();

        Assert.False(repository.Update(new Student { Id = 987654, Name = "x", RollNumber = "x", Email = "x" }));
        Assert.False(repository.Delete(987654));
    }

    [SkippableFact]
    public void Delete_ExistingRow_RemovesIt()
    {
        var repository = Students();
        var student = NewStudent("del");
        repository.Add(student);

        Assert.True(repository.Delete(student.Id));
        Assert.Null(repository.GetById(student.Id));
    }

    // The database's own constraints still back up the Service's 409 check.
    [SkippableFact]
    public void DuplicateRollNumber_IsRejectedByTheUniqueConstraint()
    {
        var repository = Students();

        var ex = Assert.Throws<SqlException>(() => repository.Add(NewStudent("dup").WithRoll("R001")));
        Assert.Contains("UQ_Students_RollNumber", ex.Message);
    }

    [SkippableFact]
    public void DataAdapter_FillsADisconnectedDataTable()
    {
        var table = Students().GetAllAsDataTable();

        Assert.Equal("Students", table.TableName);
        Assert.True(table.Rows.Count >= 3);
        Assert.Contains(table.Columns.Cast<System.Data.DataColumn>(), c => c.ColumnName == "EnrolledOn");
    }

    [SkippableFact]
    public void TeacherRepository_FullCrud()
    {
        Skip.IfNot(database.Available, database.UnavailableReason);
        var repository = new AdoNetTeacherRepository(database.ConnectionFactory);
        var teacher = new Teacher { Name = "Dr. Test", Email = "dr.test@ado.example", Designation = "Biology" };

        repository.Add(teacher);
        teacher.Designation = "Head of Biology";

        Assert.True(repository.Update(teacher));
        Assert.Equal("Head of Biology", repository.GetById(teacher.Id)!.Designation);
        Assert.Contains(repository.GetAll(), t => t.Email == "meera.iyer@school.example" && t.UserId == 1);
        Assert.True(repository.Delete(teacher.Id));
        Assert.False(repository.Delete(teacher.Id));
        Assert.False(repository.Update(teacher));
        Assert.Null(repository.GetById(teacher.Id));
    }

    // Task 7.5 - login now verifies against dbo.Users.
    [SkippableFact]
    public void UserStore_FindsTheSeededLogin_AndItsHashVerifies()
    {
        Skip.IfNot(database.Available, database.UnavailableReason);
        var store = new AdoNetUserStore(database.ConnectionFactory);

        var teacher = store.FindByUsername("TEACHER1")!;

        Assert.Equal(Roles.Teacher, teacher.Role);
        Assert.True(new Pbkdf2PasswordHasher().Verify("Teacher@123", teacher.PasswordHash));
        Assert.Null(store.FindByUsername("nobody"));
    }

    [SkippableFact]
    public void UserStore_Add_ThenDuplicateBecomesInvalidOperation()
    {
        Skip.IfNot(database.Available, database.UnavailableReason);
        var store = new AdoNetUserStore(database.ConnectionFactory);
        var user = new User { Username = "ado-user", PasswordHash = "hash", Role = Roles.Student };

        store.Add(user);

        Assert.True(user.Id > 2);
        Assert.Throws<InvalidOperationException>(() => store.Add(new User { Username = "ado-user", PasswordHash = "x", Role = Roles.Student }));
    }

    [Fact]
    public void ConnectionFactory_WithoutAConnectionString_FailsFast()
    {
        var emptyConfiguration = new ConfigurationBuilder().Build();

        Assert.Throws<InvalidOperationException>(() => new SqlConnectionFactory(emptyConfiguration));
    }

    [Fact]
    public void ConnectionFactory_ReadsTheNamedConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:StudentPortalSql"] = "Server=example;Database=db" })
            .Build();

        using var connection = new SqlConnectionFactory(configuration).Create();

        Assert.Equal("db", connection.Database);
    }
}

internal static class StudentTestExtensions
{
    public static Student WithRoll(this Student student, string roll)
    {
        student.RollNumber = roll;
        return student;
    }
}
