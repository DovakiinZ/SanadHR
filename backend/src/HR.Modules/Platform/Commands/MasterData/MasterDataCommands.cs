using System.Text.Json;
using AutoMapper;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.MasterData;
using HR.Domain.Engines.ObjectRegistry;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.MasterData;
using HR.Modules.Platform.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.MasterData;

// ─── Commands ────────────────────────────────────────────────────────────────

public record CreateMasterDataItemCommand : IRequest<MasterDataItemDto>
{
    public string ObjectType { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record UpdateMasterDataItemCommand : IRequest<MasterDataItemDto>
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record DeactivateMasterDataItemCommand(Guid Id) : IRequest<MasterDataItemDto>;

public record DeleteMasterDataItemCommand(Guid Id) : IRequest;

public record ReorderMasterDataItemsCommand : IRequest
{
    public string ObjectType { get; init; } = null!;
    public List<Guid> OrderedIds { get; init; } = new();
}

public record MergeMasterDataItemsCommand(Guid SourceId, Guid TargetId) : IRequest<MasterDataItemDto>;

public record SeedDefaultMasterDataCommand : IRequest<SeedMasterDataResultDto>;

// ─── Handlers ────────────────────────────────────────────────────────────────

public class CreateMasterDataItemCommandHandler : IRequestHandler<CreateMasterDataItemCommand, MasterDataItemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateMasterDataItemCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MasterDataItemDto> Handle(CreateMasterDataItemCommand request, CancellationToken cancellationToken)
    {
        var canonical = MasterDataObjectType.Normalize(request.ObjectType)
            ?? throw new InvalidOperationException($"Unknown object type '{request.ObjectType}'");

        var exists = await _context.MasterDataItems
            .AnyAsync(x => x.ObjectType == canonical && x.Code == request.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"A {canonical} with code '{request.Code}' already exists");

        var entity = new MasterDataItem
        {
            ObjectType = canonical,
            Code = request.Code,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            Color = request.Color,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsSystemDefault = false,
            IsActive = true,
            MetadataJson = Serialize(request.Metadata)
        };

        _context.MasterDataItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MasterDataItemDto>(entity);
    }

    internal static string? Serialize(Dictionary<string, object>? metadata) =>
        metadata is null || metadata.Count == 0 ? null : JsonSerializer.Serialize(metadata);
}

public class UpdateMasterDataItemCommandHandler : IRequestHandler<UpdateMasterDataItemCommand, MasterDataItemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateMasterDataItemCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MasterDataItemDto> Handle(UpdateMasterDataItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MasterDataItems.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Master data item not found");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.Color = request.Color;
        entity.Icon = request.Icon;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.MetadataJson = CreateMasterDataItemCommandHandler.Serialize(request.Metadata);

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MasterDataItemDto>(entity);
    }
}

public class DeactivateMasterDataItemCommandHandler : IRequestHandler<DeactivateMasterDataItemCommand, MasterDataItemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DeactivateMasterDataItemCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MasterDataItemDto> Handle(DeactivateMasterDataItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MasterDataItems.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Master data item not found");

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MasterDataItemDto>(entity);
    }
}

public class DeleteMasterDataItemCommandHandler : IRequestHandler<DeleteMasterDataItemCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IUsageTrackingService _usage;

    public DeleteMasterDataItemCommandHandler(
        ApplicationDbContext context, ICurrentUserService currentUser, IUsageTrackingService usage)
    {
        _context = context;
        _currentUser = currentUser;
        _usage = usage;
    }

    public async Task Handle(DeleteMasterDataItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MasterDataItems.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Master data item not found");

        if (entity.IsSystemDefault)
            throw new InvalidOperationException("System default items cannot be deleted. Deactivate it instead.");

        var usageCount = await _usage.GetTotalUsageAsync(entity.ObjectType, entity.Id, cancellationToken);
        if (usageCount > 0)
            throw new InvalidOperationException(
                $"This item is referenced {usageCount} time(s) and cannot be deleted. Deactivate or merge it instead.");

        // Soft delete — preserve the row so historical references stay resolvable.
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = _currentUser.Email;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class ReorderMasterDataItemsCommandHandler : IRequestHandler<ReorderMasterDataItemsCommand>
{
    private readonly ApplicationDbContext _context;

    public ReorderMasterDataItemsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReorderMasterDataItemsCommand request, CancellationToken cancellationToken)
    {
        var canonical = MasterDataObjectType.Normalize(request.ObjectType)
            ?? throw new InvalidOperationException($"Unknown object type '{request.ObjectType}'");

        var items = await _context.MasterDataItems
            .Where(x => x.ObjectType == canonical && request.OrderedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var order = request.OrderedIds.Select((id, index) => (id, index))
            .ToDictionary(t => t.id, t => t.index);

        foreach (var item in items)
            if (order.TryGetValue(item.Id, out var idx))
                item.SortOrder = idx;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class MergeMasterDataItemsCommandHandler : IRequestHandler<MergeMasterDataItemsCommand, MasterDataItemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IUsageTrackingService _usage;
    private readonly IMapper _mapper;

    public MergeMasterDataItemsCommandHandler(
        ApplicationDbContext context, ICurrentUserService currentUser,
        IUsageTrackingService usage, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _usage = usage;
        _mapper = mapper;
    }

    public async Task<MasterDataItemDto> Handle(MergeMasterDataItemsCommand request, CancellationToken cancellationToken)
    {
        if (request.SourceId == request.TargetId)
            throw new InvalidOperationException("Source and target must be different items");

        var source = await _context.MasterDataItems.FindAsync(new object[] { request.SourceId }, cancellationToken)
            ?? throw new InvalidOperationException("Source item not found");
        var target = await _context.MasterDataItems.FindAsync(new object[] { request.TargetId }, cancellationToken)
            ?? throw new InvalidOperationException("Target item not found");

        if (source.ObjectType != target.ObjectType)
            throw new InvalidOperationException("Cannot merge items of different object types");

        // Repoint every consumer reference from source -> target, then retire the source.
        await _usage.ReassignReferencesAsync(source.ObjectType, source.Id, target.Id, cancellationToken);

        source.IsActive = false;
        source.IsDeleted = true;
        source.DeletedAt = DateTime.UtcNow;
        source.DeletedBy = _currentUser.Email;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MasterDataItemDto>(target);
    }
}

public class SeedDefaultMasterDataCommandHandler : IRequestHandler<SeedDefaultMasterDataCommand, SeedMasterDataResultDto>
{
    private readonly ApplicationDbContext _context;

    public SeedDefaultMasterDataCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SeedMasterDataResultDto> Handle(SeedDefaultMasterDataCommand request, CancellationToken cancellationToken)
    {
        // Idempotent: only insert defaults the current tenant doesn't already have.
        // Query filters scope all reads/writes to the current tenant automatically.
        var existing = await _context.MasterDataItems
            .Select(x => new { x.ObjectType, x.Code })
            .ToListAsync(cancellationToken);
        var existingKeys = existing.Select(x => $"{x.ObjectType}::{x.Code}").ToHashSet();

        var sortCounters = new Dictionary<string, int>();
        var itemsSeeded = 0;

        foreach (var def in MasterDataDefaults.All)
        {
            var key = $"{def.ObjectType}::{def.Code}";
            if (existingKeys.Contains(key)) continue;

            var sort = sortCounters.TryGetValue(def.ObjectType, out var s) ? s : 0;
            sortCounters[def.ObjectType] = sort + 1;

            _context.MasterDataItems.Add(new MasterDataItem
            {
                ObjectType = def.ObjectType,
                Code = def.Code,
                NameEn = def.NameEn,
                NameAr = def.NameAr,
                Color = def.Color,
                Icon = def.Icon,
                MetadataJson = def.MetadataJson,
                SortOrder = sort,
                IsSystemDefault = true,
                IsActive = true
            });
            itemsSeeded++;
        }

        var typesRegistered = await RegisterObjectTypesAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return new SeedMasterDataResultDto { ItemsSeeded = itemsSeeded, TypesRegistered = typesRegistered };
    }

    /// <summary>Registers every master data object type in the Object Registry so it is
    /// discoverable for forms, reports, dashboards, workflows and permissions.</summary>
    private async Task<int> RegisterObjectTypesAsync(CancellationToken cancellationToken)
    {
        var existingCodes = await _context.ObjectDefinitions
            .Where(o => o.Module == "MasterData")
            .Select(o => o.Code)
            .ToListAsync(cancellationToken);
        var existingSet = existingCodes.ToHashSet();

        var registered = 0;
        foreach (var type in MasterDataObjectType.All)
        {
            if (existingSet.Contains(type)) continue;
            _context.ObjectDefinitions.Add(new ObjectDefinition
            {
                Code = type,
                NameEn = type,
                NameAr = type,
                Module = "MasterData",
                TableName = "tenant_master_data_items",
                IsSystem = true,
                IsActive = true
            });
            registered++;
        }
        return registered;
    }
}
