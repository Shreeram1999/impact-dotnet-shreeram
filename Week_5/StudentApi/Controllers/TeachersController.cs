using Microsoft.AspNetCore.Mvc;
using StudentApi.Dtos;
using StudentApi.Services;

namespace StudentApi.Controllers;

// Task 5.10 (stretch) - the thin Teacher CRUD slice, same orchestration-only
// shape as StudentsController, minus search and the grade endpoint (neither
// applies to a Teacher).
[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService service;
    private readonly ILogger<TeachersController> logger;

    public TeachersController(ITeacherService service, ILogger<TeachersController> logger)
    {
        this.service = service;
        this.logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TeacherReadDto>> GetAll()
    {
        return Ok(service.GetAll().Select(TeacherMapper.ToReadDto));
    }

    [HttpGet("{id:int}")]
    public ActionResult<TeacherReadDto> GetById(int id)
    {
        var teacher = service.GetById(id);
        if (teacher is null)
        {
            logger.LogWarning("Teacher {TeacherId} not found.", id);
            return NotFound();
        }

        return Ok(TeacherMapper.ToReadDto(teacher));
    }

    [HttpPost]
    public ActionResult<TeacherReadDto> Create(TeacherCreateDto dto)
    {
        var result = service.Add(TeacherMapper.ToEntity(dto));
        var readDto = TeacherMapper.ToReadDto(result.Value!);
        logger.LogInformation("Created teacher {TeacherId}.", readDto.Id);
        return CreatedAtAction(nameof(GetById), new { id = readDto.Id }, readDto);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, TeacherCreateDto dto)
    {
        var result = service.Update(id, TeacherMapper.ToEntity(dto));
        return result.Outcome == OperationOutcome.NotFound ? NotFound(result.Message) : NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var result = service.Delete(id);
        return result.Outcome == OperationOutcome.NotFound ? NotFound(result.Message) : NoContent();
    }
}
