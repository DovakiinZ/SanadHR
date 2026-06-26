using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Application.Engines.Finance.Events;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>The batch orchestrator. Materializes one <see cref="PayrollRunItem"/> per payslip, then posts
/// them to the ledger with bounded concurrency — each worker in its own DI scope (and DbContext) under an
/// ambient tenant scope, so it is safe in-request and on a background worker alike. Resumable (skips
/// completed items), retriable (failed items re-run), idempotent (never double-posts), and progress is
/// queryable from the item rows at any time.</summary>
public sealed class PayrollExecutionEngine : IPayrollExecutionEngine
{
    private readonly ApplicationDbContext _db;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundExecutionContext _background;
    private readonly ICurrentUserService _currentUser;
    private readonly IDomainEventPublisher _events;

    private static readonly int MaxDegreeOfParallelism = Math.Max(1, Math.Min(8, Environment.ProcessorCount));

    public PayrollExecutionEngine(
        ApplicationDbContext db,
        IServiceScopeFactory scopeFactory,
        IBackgroundExecutionContext background,
        ICurrentUserService currentUser,
        IDomainEventPublisher events)
    {
        _db = db;
        _scopeFactory = scopeFactory;
        _background = background;
        _currentUser = currentUser;
        _events = events;
    }

    public async Task<PayrollRun> ExecuteAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _db.PayrollRuns.FirstOrDefaultAsync(r => r.Id == runId, ct)
            ?? throw new InvalidOperationException($"Payroll run {runId} not found.");

        switch (run.State)
        {
            case PayrollRunState.Approved:
                ApplyTransition(run, PayrollRunState.Executing, "Execution started");
                await EnsureItemsAsync(run, ct);
                break;
            case PayrollRunState.Failed:
                ApplyTransition(run, PayrollRunState.Executing, "Execution resumed");
                await EnsureItemsAsync(run, ct);
                break;
            case PayrollRunState.Executing:
                await EnsureItemsAsync(run, ct); // resume an interrupted run
                break;
            default:
                throw new InvalidOperationException($"A run can only be executed from Approved/Executing/Failed (was {run.State}).");
        }
        await _db.SaveChangesAsync(ct);

        var pendingItemIds = await _db.PayrollRunItems.AsNoTracking()
            .Where(i => i.PayrollRunId == run.Id && i.State != PayrollRunItemState.Completed)
            .Select(i => i.Id)
            .ToListAsync(ct);

        var totalItems = await _db.PayrollRunItems.AsNoTracking().CountAsync(i => i.PayrollRunId == run.Id, ct);
        await _events.PublishAsync(new PayrollExecutionStartedEvent(run.Id, totalItems), ct);

        // Capture the tenant/actor to propagate into each worker's ambient scope.
        var tenantId = run.TenantId;
        var actorUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;
        var actorEmail = _currentUser.Email;

        await Parallel.ForEachAsync(
            pendingItemIds,
            new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism, CancellationToken = ct },
            async (itemId, token) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var background = scope.ServiceProvider.GetRequiredService<IBackgroundExecutionContext>();
                using (background.Begin(tenantId, actorUserId, actorEmail))
                {
                    var executor = scope.ServiceProvider.GetRequiredService<PayrollItemExecutor>();
                    await executor.ExecuteItemAsync(itemId, token);
                }
            });

        // Tally fresh results and settle the run state.
        var states = await _db.PayrollRunItems.AsNoTracking()
            .Where(i => i.PayrollRunId == run.Id)
            .Select(i => i.State)
            .ToListAsync(ct);
        var completed = states.Count(s => s == PayrollRunItemState.Completed);
        var failed = states.Count(s => s == PayrollRunItemState.Failed);

        if (failed == 0)
        {
            ApplyTransition(run, PayrollRunState.Completed, $"All {completed} item(s) posted");
            await _db.SaveChangesAsync(ct);
            await _events.PublishAsync(new PayrollCompletedEvent(run.Id, run.RunNumber, completed, failed, run.NetTotal), ct);
        }
        else
        {
            ApplyTransition(run, PayrollRunState.Failed, $"{failed} item(s) failed; {completed} posted");
            await _db.SaveChangesAsync(ct);
            await _events.PublishAsync(new PayrollExecutionFailedEvent(run.Id, completed, failed), ct);
        }

        return run;
    }

    /// <summary>Create a Pending item for every payslip that doesn't already have one (resume-safe).</summary>
    private async Task EnsureItemsAsync(PayrollRun run, CancellationToken ct)
    {
        var payslips = await _db.PayrollPayslips.AsNoTracking()
            .Where(p => p.PayrollRunId == run.Id)
            .Select(p => new { p.Id, p.EmployeeId })
            .ToListAsync(ct);

        var withItems = await _db.PayrollRunItems.AsNoTracking()
            .Where(i => i.PayrollRunId == run.Id)
            .Select(i => i.PayslipId)
            .ToListAsync(ct);
        var existing = withItems.ToHashSet();

        foreach (var p in payslips.Where(p => !existing.Contains(p.Id)))
        {
            _db.PayrollRunItems.Add(new PayrollRunItem
            {
                PayrollRunId = run.Id,
                EmployeeId = p.EmployeeId,
                PayslipId = p.Id,
                State = PayrollRunItemState.Pending,
            });
        }
    }

    private void ApplyTransition(PayrollRun run, PayrollRunState to, string? reason)
    {
        PayrollRunStateMachine.EnsureCanTransition(run.State, to);
        _db.PayrollRunTransitions.Add(new PayrollRunTransition
        {
            PayrollRunId = run.Id,
            FromState = run.State,
            ToState = to,
            At = DateTime.UtcNow,
            ActorUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            Reason = reason,
        });
        run.State = to;
    }
}
