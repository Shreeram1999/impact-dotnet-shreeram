using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Shared;
using Academics.Api.Dtos;
using Academics.Api.Services;

namespace Academics.Api.Controllers;

// Task 5.10 (stretch) - the thin Teacher CRUD slice, same orchestration-only
// shape as StudentsController, minus search and the grade endpoint (neither
// applies to a Teacher).
[ApiController]
[Route("api/[controller]")]
// Week 10 - every action needs a valid Identity-issued JWT, even reads: the
// gateway already checks, but the service doesn't assume it can only ever
// be reached through the gateway (defence in depth).
[Authorize]
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
    [Authorize(Roles = Roles.Teacher)]
    public ActionResult<TeacherReadDto> Create(TeacherCreateDto dto)
    {
        var result = service.Add(TeacherMapper.ToEntity(dto));
        if (result.Outcome == OperationOutcome.Conflict)
            return Conflict(result.Message);

        var readDto = TeacherMapper.ToReadDto(result.Value!);
        logger.LogInformation("Created teacher {TeacherId}.", readDto.Id);
        return CreatedAtAction(nameof(GetById), new { id = readDto.Id }, readDto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public IActionResult Update(int id, TeacherCreateDto dto)
    {
        var result = service.Update(id, TeacherMapper.ToEntity(dto));
        return result.Outcome switch
        {
            OperationOutcome.NotFound => NotFound(result.Message),
            OperationOutcome.Conflict => Conflict(result.Message),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public IActionResult Delete(int id)
    {
        var result = service.Delete(id);
        return result.Outcome == OperationOutcome.NotFound ? NotFound(result.Message) : NoContent();
    }
}
