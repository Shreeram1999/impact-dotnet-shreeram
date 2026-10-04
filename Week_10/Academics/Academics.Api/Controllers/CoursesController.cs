using Academics.Api.Dtos;
using Academics.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Shared;

namespace Academics.Api.Controllers;

// Task 10.3 - Course CRUD + enrollments, Teacher-only writes, same thin
// shape as StudentsController.
[ApiController]
[Route("api/[controller]")]
// Week 10 - every action needs a valid Identity-issued JWT, even reads: the
// gateway already checks, but the service doesn't assume it can only ever
// be reached through the gateway (defence in depth).
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService service;
    private readonly TimeProvider timeProvider;

    public CoursesController(ICourseService service, TimeProvider timeProvider)
    {
        this.service = service;
        this.timeProvider = timeProvider;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CourseReadDto>> GetAll() => Ok(service.GetAll().Select(CourseMapper.ToReadDto));

    [HttpGet("{id:int}")]
    public ActionResult<CourseReadDto> GetById(int id) =>
        service.GetById(id) is { } course ? Ok(CourseMapper.ToReadDto(course)) : NotFound();

    [HttpPost]
    [Authorize(Roles = Roles.Teacher)]
    public ActionResult<CourseReadDto> Create(CourseCreateDto dto)
    {
        var result = service.Add(CourseMapper.ToEntity(dto));
        return result.Outcome switch
        {
            OperationOutcome.Conflict => Conflict(result.Message),
            OperationOutcome.Invalid => BadRequest(result.Message),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, CourseMapper.ToReadDto(result.Value))
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public IActionResult Update(int id, CourseCreateDto dto) => ToStatus(service.Update(id, CourseMapper.ToEntity(dto)));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public IActionResult Delete(int id) => ToStatus(service.Delete(id));

    // GET api/courses/1/students - who is enrolled.
    [HttpGet("{id:int}/students")]
    public ActionResult<IEnumerable<StudentReadDto>> Students(int id) =>
        service.StudentsIn(id) is { } students ? Ok(students.Select(StudentMapper.ToReadDto)) : NotFound();

    // POST api/courses/1/enrollments { "studentId": 2 }
    [HttpPost("{id:int}/enrollments")]
    [Authorize(Roles = Roles.Teacher)]
    public IActionResult Enroll(int id, EnrollmentCreateDto dto) =>
        ToStatus(service.Enroll(id, dto.StudentId, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)));

    // DELETE api/courses/1/enrollments/2
    [HttpDelete("{id:int}/enrollments/{studentId:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public IActionResult Unenroll(int id, int studentId) => ToStatus(service.Unenroll(id, studentId));

    private IActionResult ToStatus(OperationResult result) => result.Outcome switch
    {
        OperationOutcome.NotFound => NotFound(result.Message),
        OperationOutcome.Conflict => Conflict(result.Message),
        OperationOutcome.Invalid => BadRequest(result.Message),
        _ => NoContent()
    };
}
