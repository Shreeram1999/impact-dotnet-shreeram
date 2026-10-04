using System.ComponentModel.DataAnnotations;
using Academics.Api.Models;

namespace Academics.Api.Dtos;

public class CourseCreateDto
{
    [Required, StringLength(20, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Credits { get; set; }

    public int? TeacherId { get; set; }
}

public record CourseReadDto(int Id, string Code, string Title, int Credits, int? TeacherId);

public class EnrollmentCreateDto
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }
}

public static class CourseMapper
{
    public static Course ToEntity(CourseCreateDto dto) => new()
    {
        Code = dto.Code.Trim().ToUpperInvariant(),
        Title = dto.Title.Trim(),
        Credits = dto.Credits,
        TeacherId = dto.TeacherId
    };

    public static CourseReadDto ToReadDto(Course course) => new(course.Id, course.Code, course.Title, course.Credits, course.TeacherId);
}
