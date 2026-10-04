using Moq;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Tests.Auth;

public class InMemoryUserStoreTests
{
    private static InMemoryUserStore CreateStore()
    {
        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => $"hashed:{p.Length}");
        return new InMemoryUserStore(hasher.Object);
    }

    [Fact]
    public void Seeds_OneTeacherAndOneStudent_WithHashedPasswordsOnly()
    {
        var store = CreateStore();

        var teacher = store.FindByUsername("teacher1")!;
        var student = store.FindByUsername("student1")!;
        Assert.Equal(Roles.Teacher, teacher.Role);
        Assert.Equal(Roles.Student, student.Role);
        Assert.StartsWith("hashed:", teacher.PasswordHash);
    }

    [Fact]
    public void FindByUsername_IsCaseInsensitive()
    {
        Assert.NotNull(CreateStore().FindByUsername("TEACHER1"));
    }

    [Fact]
    public void FindByUsername_Unknown_ReturnsNull()
    {
        Assert.Null(CreateStore().FindByUsername("nobody"));
    }

    [Fact]
    public void Add_NewUser_CanBeFound()
    {
        var store = CreateStore();

        store.Add(new User { Username = "teacher2", PasswordHash = "h", Role = Roles.Teacher });

        Assert.NotNull(store.FindByUsername("teacher2"));
    }

    [Fact]
    public void Add_DuplicateUsername_Throws()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() => store.Add(new User { Username = "Teacher1", PasswordHash = "h", Role = Roles.Teacher }));
    }
}
