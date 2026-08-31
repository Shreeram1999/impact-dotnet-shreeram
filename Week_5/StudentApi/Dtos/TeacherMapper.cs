using StudentApi.Models;

namespace StudentApi.Dtos;

// Task 5.10 (stretch) - mirrors StudentMapper's role for Teacher.
public static class TeacherMapper
{
    public static Teacher ToEntity(TeacherCreateDto dto) => new()
    {
        Name = dto.Name,
        Email = dto.Email,
        Designation = dto.Designation
    };

    public static TeacherReadDto ToReadDto(Teacher teacher) => new()
    {
        Id = teacher.Id,
        Name = teacher.Name,
        Email = teacher.Email,
        Designation = teacher.Designation
    };
}
