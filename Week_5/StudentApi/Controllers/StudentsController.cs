using Microsoft.AspNetCore.Mvc;
using StudentApi.Dtos;
using StudentApi.Services;
using StudentApi.Services.Grading;

namespace StudentApi.Controllers;

// Task 5.2/5.3/5.4 - read every action below and check: each one reads
// input, calls exactly one IStudentService (or IGradeStrategyFactory) member,
// maps the result through StudentMapper, and picks a status code from the
// Service's OperationOutcome. There is no duplicate-roll-number check, no
// age range check, and no string-formatting of a table anywhere in this
// class - that's the whole point of Week 4's layering carried into Week 5.
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService service;
    private readonly IGradeStrategyFactory gradeStrategyFactory;
    private readonly ILogger<StudentsController> logger;

    public StudentsController(IStudentService service, IGradeStrategyFactory gradeStrategyFactory, ILogger<StudentsController> logger)
    {
        this.service = service;
        this.gradeStrategyFactory = gradeStrategyFactory;
        this.logger = logger;
    }

    // GET api/students
    [HttpGet]
    public ActionResult<IEnumerable<StudentReadDto>> GetAll()
    {
        return Ok(service.GetAll().Select(StudentMapper.ToReadDto));
    }

    // GET api/students/search?name=...
    // Routed before {id:int} so "search" is never mistaken for an id.
    [HttpGet("search")]
    public ActionResult<IEnumerable<StudentReadDto>> Search([FromQuery] string? name)
    {
        return Ok(service.Search(name).Select(StudentMapper.ToReadDto));
    }

    // GET api/students/5
    [HttpGet("{id:int}")]
    public ActionResult<StudentReadDto> GetById(int id)
    {
        var student = service.GetById(id);
        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found.", id);
            return NotFound();
        }

        return Ok(StudentMapper.ToReadDto(student));
    }

    // GET api/students/5/grade?scale=percentage|gpa
    [HttpGet("{id:int}/grade")]
    public ActionResult<GradeDto> GetGrade(int id, [FromQuery] string scale = "percentage")
    {
        var student = service.GetById(id);
        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found.", id);
            return NotFound();
        }

        var strategy = gradeStrategyFactory.Create(scale);
        return Ok(new GradeDto(student.Id, scale, strategy.Describe(student.Score)));
    }

    // POST api/students
    [HttpPost]
    public ActionResult<StudentReadDto> Create(StudentCreateDto dto)
    {
        var result = service.Add(StudentMapper.ToEntity(dto));
        if (result.Outcome == OperationOutcome.Conflict)
            return Conflict(result.Message);

        var readDto = StudentMapper.ToReadDto(result.Value!);
        logger.LogInformation("Created student {StudentId} ({RollNumber}).", readDto.Id, readDto.RollNumber);
        return CreatedAtAction(nameof(GetById), new { id = readDto.Id }, readDto);
    }

    // PUT api/students/5
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, StudentCreateDto dto)
    {
        var result = service.Update(id, StudentMapper.ToEntity(dto));
        return result.Outcome switch
        {
            OperationOutcome.NotFound => NotFound(result.Message),
            OperationOutcome.Conflict => Conflict(result.Message),
            _ => NoContent()
        };
    }

    // DELETE api/students/5
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var result = service.Delete(id);
        return result.Outcome == OperationOutcome.NotFound ? NotFound(result.Message) : NoContent();
    }
}
