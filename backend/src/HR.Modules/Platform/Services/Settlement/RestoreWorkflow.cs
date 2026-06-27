using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Settlement;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Settlement;

/// <summary>Implements the employee-restore (reinstatement) approval flow. A former employee is routed
/// through a Manager → HR approval chain; on final approval the employee is reactivated (Status →
/// Active, termination data cleared). Mirrors <see cref="TerminationWorkflow"/>.</summary>
public sealed class RestoreWorkflow : IRestoreWorkflow
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _audit;

    public RestoreWorkflow(ApplicationDbContext db, ICurrentUserService currentUser, IAuditLogService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    private static string PermissionFor(SettlementApproverRole role) => role switch
    {
        SettlementApproverRole.Manager => "Employees.Edit",
        SettlementApproverRole.HR => "Employees.Terminate",
        _ => "Employees.Terminate",
    };

    public async Task<EmployeeRestoreRequest> RequestAsync(Guid employeeId, string? reason, CancellationToken ct = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, ct)
            ?? throw new NotFoundException("Employee", employeeId);
        if (employee.Status is not (EmployeeStatus.Terminated or EmployeeStatus.Resigned))
            throw new ConflictException("Only a former (terminated/resigned) employee can be restored.");
        if (await _db.EmployeeRestoreRequests.AnyAsync(r => r.EmployeeId == employeeId && r.Status == SettlementStatus.PendingApproval, ct))
            throw new ConflictException("A restore request is already pending approval for this employee.");

        var request = new EmployeeRestoreRequest
        {
            EmployeeId = employeeId,
            Reason = reason,
            Status = SettlementStatus.PendingApproval,
            CurrentStep = 1,
            RequestedByUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            RequestedAt = DateTime.UtcNow,
        };

        // Manager → HR. Manager step is pinned to the direct manager's user when resolvable.
        Guid? managerUserId = employee.ManagerId is { } mgrId
            ? await _db.Employees.Where(e => e.Id == mgrId).Select(e => e.UserId).FirstOrDefaultAsync(ct)
            : null;
        request.ApprovalSteps.Add(new RestoreApprovalStep { StepOrder = 1, Role = SettlementApproverRole.Manager, ApproverUserId = managerUserId });
        request.ApprovalSteps.Add(new RestoreApprovalStep { StepOrder = 2, Role = SettlementApproverRole.HR });

        _db.EmployeeRestoreRequests.Add(request);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("RestoreRequested", "Access.Restore", request.Id, null,
            new { employee.EmployeeNumber, reason }, ct);
        return await GetAsync(request.Id, ct);
    }

    public async Task<EmployeeRestoreRequest> DecideAsync(Guid requestId, bool approve, string? comment, CancellationToken ct = default)
    {
        var request = await _db.EmployeeRestoreRequests
            .Include(r => r.ApprovalSteps)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct)
            ?? throw new NotFoundException("EmployeeRestoreRequest", requestId);
        if (request.Status != SettlementStatus.PendingApproval)
            throw new ConflictException("This restore request is not awaiting approval.");

        var step = request.ApprovalSteps.FirstOrDefault(s => s.StepOrder == request.CurrentStep)
            ?? throw new ConflictException("No active approval step.");

        var perm = PermissionFor(step.Role);
        var allowed = (_currentUser.IsAuthenticated && step.ApproverUserId == _currentUser.UserId)
            || _currentUser.Permissions.Contains(perm);
        if (!allowed) throw new ForbiddenException("You are not allowed to decide this approval step.");

        step.DecidedByUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null;
        step.DecidedAt = DateTime.UtcNow;
        step.Comment = comment;

        if (!approve)
        {
            step.Status = SettlementApprovalStepStatus.Rejected;
            request.Status = SettlementStatus.Rejected;
            request.RejectionReason = comment;
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync("RestoreRejected", "Access.Restore", request.Id, null, new { step.Role, comment }, ct);
            return await GetAsync(request.Id, ct);
        }

        step.Status = SettlementApprovalStepStatus.Approved;
        var lastStep = request.ApprovalSteps.Max(s => s.StepOrder);
        if (request.CurrentStep < lastStep)
        {
            request.CurrentStep += 1;
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync("RestoreStepApproved", "Access.Restore", request.Id, null, new { step.Role }, ct);
            return await GetAsync(request.Id, ct);
        }

        await FinalizeAsync(request, ct);
        return await GetAsync(request.Id, ct);
    }

    private async Task FinalizeAsync(EmployeeRestoreRequest request, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, ct)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        employee.Status = EmployeeStatus.Active;
        employee.TerminationDate = null;

        request.Status = SettlementStatus.Approved;
        request.ApprovedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("RestoreApproved", "Access.Restore", request.Id, null,
            new { employee.EmployeeNumber }, ct);
    }

    public async Task<IReadOnlyList<EmployeeRestoreRequest>> GetPendingForCurrentUserAsync(CancellationToken ct = default)
    {
        var pending = await _db.EmployeeRestoreRequests
            .Include(r => r.ApprovalSteps)
            .Where(r => r.Status == SettlementStatus.PendingApproval)
            .ToListAsync(ct);

        return pending.Where(r =>
        {
            var step = r.ApprovalSteps.FirstOrDefault(x => x.StepOrder == r.CurrentStep);
            if (step is null) return false;
            return (_currentUser.IsAuthenticated && step.ApproverUserId == _currentUser.UserId)
                || _currentUser.Permissions.Contains(PermissionFor(step.Role));
        }).ToList();
    }

    public async Task<EmployeeRestoreRequest> GetAsync(Guid requestId, CancellationToken ct = default)
        => await _db.EmployeeRestoreRequests.Include(r => r.ApprovalSteps)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct)
           ?? throw new NotFoundException("EmployeeRestoreRequest", requestId);
}
