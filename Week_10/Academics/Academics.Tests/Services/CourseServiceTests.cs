using Academics.Api.Data;
using Academics.Api.Models;
using Academics.Api.Services;
using Moq;

namespace Academics.Tests.Services;

// Task 10.3 - Course + enrollment rules with every repository mocked.
public class CourseServiceTests
{
    private readonly Mock<IRepository<Course>> courses = new();
    private readonly Mock<IRepository<Teacher>> teachers = new();
    private readonly Mock<IRepository<Student>> students = new();
    private readonly Mock<IEnrollmentRepository> enrollments = new();

    private static readonly Course Math = new() { Id = 1, Code = "MATH101", Title = "Calculus I", Credits = 4 };
    private static readonly DateOnly Today = new(2026, 10, 1);

    public CourseServiceTests()
    {
        courses.Setup(r => r.GetAll()).Returns([Math]);
        courses.Setup(r => r.GetById(1)).Returns(Math);
        teachers.Setup(r => r.GetById(1)).Returns(new Teacher { Id = 1 });
        students.Setup(r => r.GetById(2)).Returns(new Student { Id = 2, Name = "Rohit" });
    }

    private CourseService Service() => new(courses.Object, teachers.Object, students.Object, enrollments.Object);

    [Fact]
    public void Add_NewCode_WithAnExistingTeacher_Succeeds()
    {
        var result = Service().Add(new Course { Code = "CS101", Title = "C#", Credits = 3, TeacherId = 1 });

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        courses.Verify(r => r.Add(It.IsAny<Course>()), Times.Once);
    }

    [Fact]
    public void Add_DuplicateCode_IgnoringCase_IsConflict()
    {
        Assert.Equal(OperationOutcome.Conflict, Service().Add(new Course { Code = "math101", Title = "x", Credits = 3 }).Outcome);
        courses.Verify(r => r.Add(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public void Add_UnknownTeacher_IsInvalid()
    {
        Assert.Equal(OperationOutcome.Invalid, Service().Add(new Course { Code = "CS101", Title = "x", Credits = 3, TeacherId = 99 }).Outcome);
    }

    [Fact]
    public void Update_CopiesFields_AndChecksCodeTeacherAndExistence()
    {
        var service = Service();

        Assert.Equal(OperationOutcome.NotFound, service.Update(5, new Course { Code = "X1" }).Outcome);
        Assert.Equal(OperationOutcome.Invalid, service.Update(1, new Course { Code = "MATH101", TeacherId = 42 }).Outcome);
        Assert.Equal(OperationOutcome.Success, service.Update(1, new Course { Code = "MATH101", Title = "Calculus", Credits = 5, TeacherId = 1 }).Outcome);
        Assert.Equal("Calculus", Math.Title);
        Assert.Equal(5, Math.Credits);
    }

    [Fact]
    public void Update_ToAnotherCoursesCode_IsConflict()
    {
        courses.Setup(r => r.GetAll()).Returns([Math, new Course { Id = 2, Code = "PHYS101" }]);

        Assert.Equal(OperationOutcome.Conflict, Service().Update(1, new Course { Code = "PHYS101", Title = "x", Credits = 1 }).Outcome);
    }

    [Fact]
    public void Delete_ExistingAndMissing()
    {
        Assert.Equal(OperationOutcome.Success, Service().Delete(1).Outcome);
        Assert.Equal(OperationOutcome.NotFound, Service().Delete(9).Outcome);
        courses.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public void Enroll_HappyPath_RecordsTheDate()
    {
        Enrollment? saved = null;
        enrollments.Setup(r => r.Add(It.IsAny<Enrollment>())).Callback<Enrollment>(e => saved = e);

        var result = Service().Enroll(1, 2, Today);

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        Assert.Equal(new Enrollment { StudentId = 2, CourseId = 1, EnrolledOn = Today }, saved, new EnrollmentComparer());
    }

    [Fact]
    public void Enroll_MissingCourse_MissingStudent_AlreadyEnrolled()
    {
        enrollments.Setup(r => r.Exists(2, 1)).Returns(true);
        var service = Service();

        Assert.Equal(OperationOutcome.NotFound, service.Enroll(7, 2, Today).Outcome);
        Assert.Equal(OperationOutcome.Invalid, service.Enroll(1, 99, Today).Outcome);
        Assert.Equal(OperationOutcome.Conflict, service.Enroll(1, 2, Today).Outcome);
        enrollments.Verify(r => r.Add(It.IsAny<Enrollment>()), Times.Never);
    }

    [Fact]
    public void Unenroll_ReportsWhetherAnythingWasRemoved()
    {
        enrollments.Setup(r => r.Remove(2, 1)).Returns(true);

        Assert.Equal(OperationOutcome.Success, Service().Unenroll(1, 2).Outcome);
        Assert.Equal(OperationOutcome.NotFound, Service().Unenroll(1, 3).Outcome);
    }

    [Fact]
    public void StudentsIn_ResolvesIds_SkippingAnyThatVanished_AndNullForAMissingCourse()
    {
        enrollments.Setup(r => r.StudentIdsFor(1)).Returns([2, 404]);

        var roster = Service().StudentsIn(1)!.ToList();

        Assert.Single(roster);
        Assert.Equal("Rohit", roster[0].Name);
        Assert.Null(Service().StudentsIn(77));
    }

    [Fact]
    public void GetAll_And_GetById_PassThrough()
    {
        Assert.Single(Service().GetAll());
        Assert.Same(Math, Service().GetById(1));
    }

    private sealed class EnrollmentComparer : IEqualityComparer<Enrollment?>
    {
        public bool Equals(Enrollment? x, Enrollment? y) =>
            x is not null && y is not null && x.StudentId == y.StudentId && x.CourseId == y.CourseId && x.EnrolledOn == y.EnrolledOn;

        public int GetHashCode(Enrollment? obj) => 0;
    }
}
