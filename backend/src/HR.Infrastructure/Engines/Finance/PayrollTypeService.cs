using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>CRUD + version lifecycle for payroll types. Editing is only allowed on Draft versions;
/// publishing supersedes the prior published version and closes its EffectiveTo, preserving history.</summary>
public sealed class PayrollTypeService : IPayrollTypeService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IPayrollPreviewEngine? _preview;

    // Two ctors: tests use the (db,user) form; DI passes the preview engine too.
    public PayrollTypeService(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }
    public PayrollTypeService(ApplicationDbContext db, ICurrentUserService user, IPayrollPreviewEngine preview)
        : this(db, user) => _preview = preview;

    public async Task<Guid> CreateTypeAsync(CreatePayrollTypeArgs args, CancellationToken ct)
    {
        var def = new PayrollDefinition
        {
            Code = args.Code, Name = args.Name, NameAr = args.NameAr, CategoryId = args.CategoryId,
            Status = PayrollDefinitionStatus.Draft,
        };
        _db.PayrollDefinitions.Add(def);
        var v = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = def.Id, VersionNumber = 1, Status = VersionStatus.Draft,
            CutoffDay = 27, DayBasis = DayBasis.CalendarMonth, CarryToNextPeriod = true,
        };
        _db.PayrollDefinitionVersions.Add(v);
        await _db.SaveChangesAsync(ct);
        return def.Id;
    }

    public async Task UpdateHeaderAsync(Guid typeId, string name, string? nameAr, Guid? categoryId,
        PayrollDefinitionStatus status, CancellationToken ct)
    {
        var def = await Def(typeId, ct);
        def.Name = name; def.NameAr = nameAr; def.CategoryId = categoryId; def.Status = status;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid> CreateDraftVersionAsync(Guid typeId, CancellationToken ct)
    {
        var next = await NextVersionNumber(typeId, ct);
        var v = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = typeId, VersionNumber = next, Status = VersionStatus.Draft,
            CutoffDay = 27, DayBasis = DayBasis.CalendarMonth, CarryToNextPeriod = true,
        };
        _db.PayrollDefinitionVersions.Add(v);
        await _db.SaveChangesAsync(ct);
        return v.Id;
    }

    public async Task UpdateDraftVersionAsync(Guid typeId, Guid versionId, UpdatePayrollVersionArgs a, CancellationToken ct)
    {
        var v = await Version(typeId, versionId, ct);
        if (v.Status != VersionStatus.Draft)
            throw new InvalidOperationException("Only a Draft version can be edited. Clone it first.");
        if (a.CutoffDay is { } cd)
        {
            if (cd < 1 || cd > 31) throw new InvalidOperationException("CutoffDay must be between 1 and 31.");
            v.CutoffDay = cd;
        }
        if (a.DayBasis is { } basis) v.DayBasis = basis;
        if (a.ClosingDate is { } cdt) v.ClosingDate = DateTime.SpecifyKind(cdt, DateTimeKind.Utc);
        if (a.PaymentDate is { } pdt) v.PaymentDate = DateTime.SpecifyKind(pdt, DateTimeKind.Utc);
        if (a.CarryToNextPeriod is { } carry) v.CarryToNextPeriod = carry;
        if (a.DefaultExportFormatId is { } ef) v.DefaultExportFormatId = ef;
        if (a.PaymentMethodId is { } pm) v.PaymentMethodId = pm;
        if (a.ApprovalWorkflowId is { } wf) v.ApprovalWorkflowId = wf;
        if (a.RuleSetVersionId is { } rs) v.RuleSetVersionId = rs;
        if (a.Currency is { } cur) v.Currency = cur;
        if (a.Frequency is { } freq) v.Frequency = freq;
        if (a.SelectionScopeJson is not null) v.SelectionScopeJson = a.SelectionScopeJson;
        if (a.CalcSettingsJson is not null) v.CalcSettingsJson = a.CalcSettingsJson;
        if (a.PaymentMethodScopeJson is not null) v.PaymentMethodScopeJson = a.PaymentMethodScopeJson;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid> CloneVersionAsync(Guid typeId, Guid versionId, CancellationToken ct)
    {
        var src = await Version(typeId, versionId, ct);
        var next = await NextVersionNumber(typeId, ct);
        var copy = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = typeId, VersionNumber = next, Status = VersionStatus.Draft,
            Frequency = src.Frequency, Currency = src.Currency, CutoffDay = src.CutoffDay,
            DayBasis = src.DayBasis, ClosingDate = src.ClosingDate, PaymentDate = src.PaymentDate,
            CarryToNextPeriod = src.CarryToNextPeriod, DefaultExportFormatId = src.DefaultExportFormatId,
            PaymentMethodId = src.PaymentMethodId, ApprovalWorkflowId = src.ApprovalWorkflowId,
            RuleSetVersionId = src.RuleSetVersionId, SelectionScopeJson = src.SelectionScopeJson,
            CalcSettingsJson = src.CalcSettingsJson, PaymentMethodScopeJson = src.PaymentMethodScopeJson,
        };
        _db.PayrollDefinitionVersions.Add(copy);
        await _db.SaveChangesAsync(ct);
        return copy.Id;
    }

    public async Task PublishVersionAsync(Guid typeId, Guid versionId, CancellationToken ct)
    {
        var def = await Def(typeId, ct);
        var v = await Version(typeId, versionId, ct);
        if (v.Status == VersionStatus.Superseded)
            throw new InvalidOperationException("A superseded version cannot be published.");

        var now = DateTime.UtcNow;
        if (def.CurrentVersionId is { } currentId && currentId != versionId)
        {
            var prior = await _db.PayrollDefinitionVersions.FirstOrDefaultAsync(x => x.Id == currentId, ct);
            if (prior is not null) { prior.Status = VersionStatus.Superseded; prior.EffectiveTo = now; }
        }
        v.Status = VersionStatus.Published;
        v.PublishedAt = now;
        v.PublishedByUserId = _user.IsAuthenticated ? _user.UserId : null;
        v.EffectiveFrom ??= now;
        def.CurrentVersionId = v.Id;
        if (def.Status == PayrollDefinitionStatus.Draft) def.Status = PayrollDefinitionStatus.Active;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PayrollPreview> SimulateAsync(Guid typeId, Guid versionId, int year, int month, CancellationToken ct)
    {
        _ = await Version(typeId, versionId, ct);
        if (_preview is null) throw new InvalidOperationException("Preview engine not available.");
        return await _preview.PreviewAsync(versionId, PayrollPeriod.Monthly(year, month), ct);
    }

    private async Task<PayrollDefinition> Def(Guid typeId, CancellationToken ct) =>
        await _db.PayrollDefinitions.FirstOrDefaultAsync(d => d.Id == typeId, ct)
        ?? throw new InvalidOperationException($"Payroll type {typeId} not found.");

    private async Task<PayrollDefinitionVersion> Version(Guid typeId, Guid versionId, CancellationToken ct) =>
        await _db.PayrollDefinitionVersions.FirstOrDefaultAsync(v => v.Id == versionId && v.PayrollDefinitionId == typeId, ct)
        ?? throw new InvalidOperationException($"Version {versionId} not found for type {typeId}.");

    private async Task<int> NextVersionNumber(Guid typeId, CancellationToken ct) =>
        (await _db.PayrollDefinitionVersions.Where(v => v.PayrollDefinitionId == typeId)
            .Select(v => (int?)v.VersionNumber).MaxAsync(ct) ?? 0) + 1;
}
