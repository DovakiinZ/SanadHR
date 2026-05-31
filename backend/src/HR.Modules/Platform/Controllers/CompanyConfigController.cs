using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.CompanyConfig;
using HR.Modules.Platform.DTOs.CompanyConfig;
using HR.Modules.Platform.Queries.CompanyConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/company-config")]
public class CompanyConfigController : BaseApiController
{
    // ─── Company Profile ─────────────────────────────────────────────────────

    [HttpGet("profile")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto?>>> GetProfile(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCompanyProfileQuery(), ct);
        return OkResponse(result);
    }

    [HttpPut("profile")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto>>> UpdateProfile(
        [FromBody] UpdateCompanyProfileCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    // ─── Positions ───────────────────────────────────────────────────────────

    [HttpGet("positions")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<List<PositionDto>>>> GetPositions(
        [FromQuery] Guid? departmentId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPositionsQuery { DepartmentId = departmentId }, ct);
        return OkResponse(result);
    }

    [HttpGet("positions/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<PositionDto>>> GetPositionById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPositionByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost("positions")]
    [RequirePermission("Platform.CompanyConfig.Create")]
    public async Task<ActionResult<ApiResponse<PositionDto>>> CreatePosition(
        [FromBody] CreatePositionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("positions/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<PositionDto>>> UpdatePosition(
        Guid id, [FromBody] UpdatePositionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("positions/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Delete")]
    public async Task<ActionResult<ApiResponse>> DeletePosition(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePositionCommand(id), ct);
        return OkResponse("Position deleted");
    }

    // ─── Grades ──────────────────────────────────────────────────────────────

    [HttpGet("grades")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<List<GradeDto>>>> GetGrades(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetGradesQuery(), ct);
        return OkResponse(result);
    }

    [HttpPost("grades")]
    [RequirePermission("Platform.CompanyConfig.Create")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> CreateGrade(
        [FromBody] CreateGradeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("grades/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> UpdateGrade(
        Guid id, [FromBody] UpdateGradeCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("grades/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteGrade(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteGradeCommand(id), ct);
        return OkResponse("Grade deleted");
    }

    // ─── Cost Centers ────────────────────────────────────────────────────────

    [HttpGet("cost-centers")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<List<CostCenterDto>>>> GetCostCenters(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCostCentersQuery(), ct);
        return OkResponse(result);
    }

    [HttpGet("cost-centers/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<CostCenterDto>>> GetCostCenterById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCostCenterByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost("cost-centers")]
    [RequirePermission("Platform.CompanyConfig.Create")]
    public async Task<ActionResult<ApiResponse<CostCenterDto>>> CreateCostCenter(
        [FromBody] CreateCostCenterCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("cost-centers/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<CostCenterDto>>> UpdateCostCenter(
        Guid id, [FromBody] UpdateCostCenterCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("cost-centers/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteCostCenter(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteCostCenterCommand(id), ct);
        return OkResponse("Cost center deleted");
    }

    // ─── Calendar Settings ───────────────────────────────────────────────────

    [HttpGet("calendars")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<List<CalendarSettingDto>>>> GetCalendarSettings(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCalendarSettingsQuery(), ct);
        return OkResponse(result);
    }

    [HttpPost("calendars")]
    [RequirePermission("Platform.CompanyConfig.Create")]
    public async Task<ActionResult<ApiResponse<CalendarSettingDto>>> CreateCalendarSetting(
        [FromBody] CreateCalendarSettingCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("calendars/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<CalendarSettingDto>>> UpdateCalendarSetting(
        Guid id, [FromBody] UpdateCalendarSettingCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("calendars/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteCalendarSetting(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteCalendarSettingCommand(id), ct);
        return OkResponse("Calendar setting deleted");
    }

    // ─── Fiscal Periods ──────────────────────────────────────────────────────

    [HttpGet("fiscal-periods")]
    [RequirePermission("Platform.CompanyConfig.View")]
    public async Task<ActionResult<ApiResponse<List<FiscalPeriodDto>>>> GetFiscalPeriods(
        [FromQuery] int? year, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetFiscalPeriodsQuery { Year = year }, ct);
        return OkResponse(result);
    }

    [HttpPost("fiscal-periods")]
    [RequirePermission("Platform.CompanyConfig.Create")]
    public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> CreateFiscalPeriod(
        [FromBody] CreateFiscalPeriodCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("fiscal-periods/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> UpdateFiscalPeriod(
        Guid id, [FromBody] UpdateFiscalPeriodCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpPost("fiscal-periods/{id:guid}/close")]
    [RequirePermission("Platform.CompanyConfig.Edit")]
    public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> CloseFiscalPeriod(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CloseFiscalPeriodCommand(id), ct);
        return OkResponse(result);
    }

    [HttpDelete("fiscal-periods/{id:guid}")]
    [RequirePermission("Platform.CompanyConfig.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteFiscalPeriod(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteFiscalPeriodCommand(id), ct);
        return OkResponse("Fiscal period deleted");
    }
}
