using System.Text.Json;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Automation;
using HR.Domain.Engines.Automation;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Engines.Automation;

public class AutomationEngine : IAutomationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AutomationEngine> _logger;

    public AutomationEngine(ApplicationDbContext context, ICurrentUserService currentUser, ILogger<AutomationEngine> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task PublishEvent(string eventType, string entityType, Guid entityId, object? payload = null, CancellationToken ct = default)
    {
        var matchingRules = await _context.AutomationRules
            .Include(r => r.Triggers)
            .Include(r => r.Conditions)
            .Include(r => r.Actions.OrderBy(a => a.SortOrder))
            .Where(r => r.IsActive)
            .Where(r => r.Triggers.Any(t => t.EventType == eventType && t.EntityType == entityType))
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        foreach (var rule in matchingRules)
        {
            var log = new AutomationExecutionLog
            {
                AutomationRuleId = rule.Id,
                TriggerEventType = eventType,
                EntityType = entityType,
                EntityId = entityId,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                // Execute actions (placeholder - actual action execution would be expanded)
                log.Status = AutomationExecutionStatus.Success;
            }
            catch (Exception ex)
            {
                log.Status = AutomationExecutionStatus.Failed;
                log.Error = ex.Message;
                _logger.LogError(ex, "Automation rule {RuleId} failed for {EntityType}/{EntityId}", rule.Id, entityType, entityId);
            }

            _context.AutomationExecutionLogs.Add(log);
        }

        await _context.SaveChangesAsync(ct);
    }
}
