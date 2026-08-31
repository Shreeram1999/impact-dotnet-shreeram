using System.Text.Json;
using StudentApi.Dtos;
using StudentApi.Models;

namespace StudentApi.Tests.Dtos;

// Task 5.7 - "DTO-mapping correctness (internal field never leaks) is a
// cheap, valuable test" per the Testing Focus. StudentReadDto simply has no
// InternalNotes property, so this is provable at the JSON level: serialize
// a mapped ReadDto the same way the API would, and confirm the field is
// nowhere in the output - not null, not empty, structurally absent.
public class StudentMapperTests
{
    [Fact]
    public void ToReadDto_NeverIncludesInternalNotesInSerializedOutput()
    {
        var student = new Student
        {
            Id = 1,
            Name = "Asha",
            Age = 20,
            RollNumber = "R1",
            Email = "asha@example.com",
            Score = 82,
            InternalNotes = "flagged for review - do not expose"
        };

        var readDto = StudentMapper.ToReadDto(student);
        var json = JsonSerializer.Serialize(readDto);

        Assert.DoesNotContain("InternalNotes", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("flagged for review", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToReadDto_CopiesEveryPublicField()
    {
        var student = new Student { Id = 5, Name = "Asha", Age = 20, RollNumber = "R1", Email = "asha@example.com", Score = 82 };

        var readDto = StudentMapper.ToReadDto(student);

        Assert.Equal(5, readDto.Id);
        Assert.Equal("Asha", readDto.Name);
        Assert.Equal(20, readDto.Age);
        Assert.Equal("R1", readDto.RollNumber);
        Assert.Equal("asha@example.com", readDto.Email);
        Assert.Equal(82, readDto.Score);
    }

    [Fact]
    public void ToEntity_HasNoWayToSetInternalNotes_SoItDefaultsEmpty()
    {
        var dto = new StudentCreateDto { Name = "Asha", Age = 20, RollNumber = "R1", Email = "asha@example.com", Score = 82 };

        var student = StudentMapper.ToEntity(dto);

        Assert.Equal(string.Empty, student.InternalNotes);
    }
}
