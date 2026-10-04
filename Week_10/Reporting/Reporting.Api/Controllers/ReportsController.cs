using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reporting.Api.Services;

namespace Reporting.Api.Controllers;

// Reached through the gateway as /reporting/api/reports/... Read-only, and
// any signed-in user may read (only aggregates are exposed).
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService service;

    public ReportsController(IReportService service)
    {
        this.service = service;
    }

    [HttpGet("terms")]
    public ActionResult<IReadOnlyList<TermDto>> Terms() => Ok(service.Terms());

    // GET api/reports/enrollment-summary?termId=2 (omit termId for the latest term)
    [HttpGet("enrollment-summary")]
    public ActionResult<EnrollmentSummaryDto> EnrollmentSummary([FromQuery] int? termId) =>
        service.EnrollmentSummary(termId) is { } summary ? Ok(summary) : NotFound($"No term with Id {termId}.");

    [HttpGet("departments")]
    public ActionResult<IReadOnlyList<DepartmentSummaryDto>> Departments([FromQuery] int? termId) =>
        service.DepartmentSummary(termId) is { } summary ? Ok(summary) : NotFound($"No term with Id {termId}.");
}
