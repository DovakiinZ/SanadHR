using HR.Modules.Platform.Services.Notifications;

namespace HR.Api.Services;

/// <summary>
/// Runs the document-expiry scan shortly after startup and then every 12 hours, creating
/// notifications for documents that fall inside any active rule's window. The scan itself is
/// idempotent (deduped via the dispatch ledger), so the cadence only affects timeliness.
/// </summary>
public sealed class DocumentExpiryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentExpiryHostedService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(12);

    public DocumentExpiryHostedService(IServiceScopeFactory scopeFactory, ILogger<DocumentExpiryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var scanner = scope.ServiceProvider.GetRequiredService<IDocumentExpiryScanner>();
                var count = await scanner.RunAsync(stoppingToken);
                if (count > 0) _logger.LogInformation("Document-expiry scan created {Count} notification(s).", count);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Document-expiry scan failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
