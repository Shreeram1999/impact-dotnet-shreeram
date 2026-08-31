using StudentApi.Models;

namespace StudentApi.Dtos;

// Task 5.7 - the manual entity<->DTO mapping, kept out of the controller
// entirely so StudentsController's actions stay pure orchestration (read
// input, call the Service, pick a status code) with no property-copying
// mixed in.
public static class StudentMapper
{
    public static Student ToEntity(StudentCreateDto dto) => new()
    {
        Name = dto.Name,
        Age = dto.Age,
        RollNumber = dto.RollNumber,
        Email = dto.Email,
        Score = dto.Score
    };

    public static StudentReadDto ToReadDto(Student student) => new()
    {
        Id = student.Id,
        Name = student.Name,
        Age = student.Age,
        RollNumber = student.RollNumber,
        Email = student.Email,
        Score = student.Score
    };
}
