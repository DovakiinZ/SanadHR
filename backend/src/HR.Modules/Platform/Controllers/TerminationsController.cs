using System.Text.Json.Serialization;
using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Application.Engines.Settlement;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/terminations")]
public class TerminationsController : BaseApiController
{
    private readonly ITerminationWorkflow _workflow;
    private readonly ApplicationDbContext _db;

    public TerminationsController(ITerminationWorkflow workflow, ApplicationDbContext db)
    {
        _workflow = workflow;
        _db = db;
    }

    [HttpPost("request")]
    [RequirePermission("Employees.Terminate")]
    public async Task<ActionResult<ApiResponse<TerminationDto>>> Request([FromBody] TerminationRequestDto req, CancellationToken ct)
    {
        var s = await _workflow.RequestAsync(new SettlementRequest
        {
            EmployeeId = req.EmployeeId,
            TerminationDate = req.TerminationDate,
            Scenario = req.Scenario,
            ContractTermType = req.ContractTermType,
            ContractEndDate = req.ContractEndDate,
            Notes = req.Notes,
        }, ct);
        return CreatedResponse(await MapAsync(s.Id, ct));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<TerminationDto>>>> Pending(CancellationToken ct)
    {
        var list = await _workflow.GetPendingForCurrentUserAsync(ct);
        var dtos = new List<TerminationDto>();
        foreach (var s in list) dtos.Add(await MapAsync(s.Id, ct));
        return OkResponse(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TerminationDto>>> Get(Guid id, CancellationToken ct)
        => OkResponse(await MapAsync(id, ct));

    [HttpPost("{id:guid}/decide")]
    public async Task<ActionResult<ApiResponse<TerminationDto>>> Decide(Guid id, [FromBody] DecideDto body, CancellationToken ct)
    {
        await _workflow.DecideAsync(id, body.Approve, body.Comment, ct);
        return OkResponse(await MapAsync(id, ct));
    }

    private async Task<TerminationDto> MapAsync(Guid id, CancellationToken ct)
    {
        var s = await _workflow.GetAsync(id, ct);
        var emp = await _db.Employees.AsNoTracking().Where(e => e.Id == s.EmployeeId)
            .Select(e => new { Name = (e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName), e.EmployeeNumber })
            .FirstOrDefaultAsync(ct);

        return new TerminationDto
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            EmployeeName = emp?.Name?.Trim() ?? "",
            EmployeeNumber = emp?.EmployeeNumber ?? "",
            Status = s.Status.ToString(),
            CurrentStep = s.CurrentStep,
            Scenario = s.Scenario.ToString(),
            TerminationDate = s.TerminationDate,
            MonthlyWage = s.MonthlyWage,
            ServiceYears = s.ServiceYears,
            GratuityAmount = s.GratuityAmount,
            Article77Award = s.Article77Award,
            TotalAward = s.TotalAward,
            Currency = s.Currency,
            ExpenseId = s.ExpenseId,
            DocumentFileId = s.DocumentFileId,
            ApprovedAt = s.ApprovedAt,
            RejectionReason = s.RejectionReason,
            Items = s.Items.Select(i => new TerminationItemDto { LabelAr = i.LabelAr, ArticleRef = i.ArticleRef, Amount = i.Amount }).ToList(),
            Steps = s.ApprovalSteps.OrderBy(x => x.StepOrder).Select(x => new TerminationStepDto
            {
                Order = x.StepOrder, Role = x.Role.ToString(), Status = x.Status.ToString(),
                DecidedAt = x.DecidedAt, Comment = x.Comment,
            }).ToList(),
        };
    }
}

public class TerminationRequestDto
{
    public Guid EmployeeId { get; set; }
    public DateTime TerminationDate { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))] public TerminationScenario Scenario { get; set; } = TerminationScenario.NormalEmployerTermination;
    [JsonConverter(typeof(JsonStringEnumConverter))] public ContractTermType ContractTermType { get; set; } = ContractTermType.Indefinite;
    public DateTime? ContractEndDate { get; set; }
    public string? Notes { get; set; }
}

public class DecideDto { public bool Approve { get; set; } public string? Comment { get; set; } }

public class TerminationStepDto
{
    public int Order { get; set; }
    public string Role { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? DecidedAt { get; set; }
    public string? Comment { get; set; }
}
public class TerminationItemDto { public string LabelAr { get; set; } = ""; public string ArticleRef { get; set; } = ""; public decimal Amount { get; set; } }

public class TerminationDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string EmployeeNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public int CurrentStep { get; set; }
    public string Scenario { get; set; } = "";
    public DateTime TerminationDate { get; set; }
    public decimal MonthlyWage { get; set; }
    public decimal ServiceYears { get; set; }
    public decimal GratuityAmount { get; set; }
    public decimal Article77Award { get; set; }
    public decimal TotalAward { get; set; }
    public string Currency { get; set; } = "SAR";
    public Guid? ExpenseId { get; set; }
    public Guid? DocumentFileId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<TerminationItemDto> Items { get; set; } = new();
    public List<TerminationStepDto> Steps { get; set; } = new();
}
