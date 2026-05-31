using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.OrgGraph;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.OrgGraph;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.OrgGraph;

// --- Commands ---

public record CreateOrgNodeCommand : IRequest<OrgNodeDto>
{
    public string NodeType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? ParentNodeId { get; init; }
    public int Level { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
    public string? Metadata { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateOrgNodeCommand : IRequest<OrgNodeDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? ParentNodeId { get; init; }
    public int Level { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
    public string? Metadata { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record DeleteOrgNodeCommand(Guid Id) : IRequest;

public record CreateOrgEdgeCommand : IRequest<OrgEdgeDto>
{
    public Guid SourceNodeId { get; init; }
    public Guid TargetNodeId { get; init; }
    public string RelationType { get; init; } = null!;
    public string? Label { get; init; }
}

public record DeleteOrgEdgeCommand(Guid Id) : IRequest;

public record CreateOrgGraphLayoutCommand : IRequest<OrgGraphLayoutDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string GraphType { get; init; } = null!;
    public string LayoutData { get; init; } = null!;
    public bool IsDefault { get; init; }
}

public record UpdateOrgGraphLayoutCommand : IRequest<OrgGraphLayoutDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string LayoutData { get; init; } = null!;
    public bool IsDefault { get; init; }
}

public record DeleteOrgGraphLayoutCommand(Guid Id) : IRequest;

public record CreateEmployeeReportingLineCommand : IRequest<EmployeeReportingLineDto>
{
    public Guid EmployeeId { get; init; }
    public Guid ManagerId { get; init; }
    public string ReportingType { get; init; } = null!;
    public bool IsPrimary { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public record UpdateEmployeeReportingLineCommand : IRequest<EmployeeReportingLineDto>
{
    public Guid Id { get; init; }
    public Guid ManagerId { get; init; }
    public string ReportingType { get; init; } = null!;
    public bool IsPrimary { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}

public record DeleteEmployeeReportingLineCommand(Guid Id) : IRequest;

public record MoveOrgNodeCommand : IRequest<OrgNodeDto>
{
    public Guid Id { get; init; }
    public Guid? NewParentNodeId { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
}

public record NodePositionUpdate
{
    public Guid NodeId { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
}

public record BulkUpdateNodePositionsCommand : IRequest
{
    public List<NodePositionUpdate> Positions { get; init; } = new();
}

// --- Handlers ---

public class CreateOrgNodeCommandHandler : IRequestHandler<CreateOrgNodeCommand, OrgNodeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateOrgNodeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgNodeDto> Handle(CreateOrgNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrgNode
        {
            NodeType = request.NodeType,
            EntityId = request.EntityId,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            ParentNodeId = request.ParentNodeId,
            Level = request.Level,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Metadata = request.Metadata,
            SortOrder = request.SortOrder
        };

        _context.Set<OrgNode>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrgNodeDto>(entity);
    }
}

public class UpdateOrgNodeCommandHandler : IRequestHandler<UpdateOrgNodeCommand, OrgNodeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateOrgNodeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgNodeDto> Handle(UpdateOrgNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgNode>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("OrgNode", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.ParentNodeId = request.ParentNodeId;
        entity.Level = request.Level;
        entity.PositionX = request.PositionX;
        entity.PositionY = request.PositionY;
        entity.Metadata = request.Metadata;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrgNodeDto>(entity);
    }
}

public class DeleteOrgNodeCommandHandler : IRequestHandler<DeleteOrgNodeCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteOrgNodeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteOrgNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgNode>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("OrgNode", request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateOrgEdgeCommandHandler : IRequestHandler<CreateOrgEdgeCommand, OrgEdgeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateOrgEdgeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgEdgeDto> Handle(CreateOrgEdgeCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrgEdge
        {
            SourceNodeId = request.SourceNodeId,
            TargetNodeId = request.TargetNodeId,
            RelationType = request.RelationType,
            Label = request.Label
        };

        _context.Set<OrgEdge>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrgEdgeDto>(entity);
    }
}

public class DeleteOrgEdgeCommandHandler : IRequestHandler<DeleteOrgEdgeCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteOrgEdgeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteOrgEdgeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgEdge>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("OrgEdge", request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateOrgGraphLayoutCommandHandler : IRequestHandler<CreateOrgGraphLayoutCommand, OrgGraphLayoutDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateOrgGraphLayoutCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgGraphLayoutDto> Handle(CreateOrgGraphLayoutCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrgGraphLayout
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            GraphType = request.GraphType,
            LayoutData = request.LayoutData,
            IsDefault = request.IsDefault
        };

        _context.Set<OrgGraphLayout>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrgGraphLayoutDto>(entity);
    }
}

public class UpdateOrgGraphLayoutCommandHandler : IRequestHandler<UpdateOrgGraphLayoutCommand, OrgGraphLayoutDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateOrgGraphLayoutCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgGraphLayoutDto> Handle(UpdateOrgGraphLayoutCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgGraphLayout>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("OrgGraphLayout", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.LayoutData = request.LayoutData;
        entity.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrgGraphLayoutDto>(entity);
    }
}

public class DeleteOrgGraphLayoutCommandHandler : IRequestHandler<DeleteOrgGraphLayoutCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteOrgGraphLayoutCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteOrgGraphLayoutCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgGraphLayout>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("OrgGraphLayout", request.Id);

        _context.Set<OrgGraphLayout>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateEmployeeReportingLineCommandHandler : IRequestHandler<CreateEmployeeReportingLineCommand, EmployeeReportingLineDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateEmployeeReportingLineCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmployeeReportingLineDto> Handle(CreateEmployeeReportingLineCommand request, CancellationToken cancellationToken)
    {
        var entity = new EmployeeReportingLine
        {
            EmployeeId = request.EmployeeId,
            ManagerId = request.ManagerId,
            ReportingType = request.ReportingType,
            IsPrimary = request.IsPrimary,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        _context.Set<EmployeeReportingLine>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<EmployeeReportingLineDto>(entity);
    }
}

public class UpdateEmployeeReportingLineCommandHandler : IRequestHandler<UpdateEmployeeReportingLineCommand, EmployeeReportingLineDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateEmployeeReportingLineCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmployeeReportingLineDto> Handle(UpdateEmployeeReportingLineCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<EmployeeReportingLine>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("EmployeeReportingLine", request.Id);

        entity.ManagerId = request.ManagerId;
        entity.ReportingType = request.ReportingType;
        entity.IsPrimary = request.IsPrimary;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<EmployeeReportingLineDto>(entity);
    }
}

public class DeleteEmployeeReportingLineCommandHandler : IRequestHandler<DeleteEmployeeReportingLineCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteEmployeeReportingLineCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteEmployeeReportingLineCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<EmployeeReportingLine>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("EmployeeReportingLine", request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class MoveOrgNodeCommandHandler : IRequestHandler<MoveOrgNodeCommand, OrgNodeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MoveOrgNodeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgNodeDto> Handle(MoveOrgNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgNode>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("OrgNode", request.Id);

        entity.ParentNodeId = request.NewParentNodeId;
        entity.PositionX = request.PositionX;
        entity.PositionY = request.PositionY;

        // Recalculate level based on new parent
        if (request.NewParentNodeId.HasValue)
        {
            var parent = await _context.Set<OrgNode>().FindAsync(new object[] { request.NewParentNodeId.Value }, cancellationToken);
            entity.Level = parent != null ? parent.Level + 1 : 0;
        }
        else
        {
            entity.Level = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrgNodeDto>(entity);
    }
}

public class BulkUpdateNodePositionsCommandHandler : IRequestHandler<BulkUpdateNodePositionsCommand>
{
    private readonly ApplicationDbContext _context;

    public BulkUpdateNodePositionsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(BulkUpdateNodePositionsCommand request, CancellationToken cancellationToken)
    {
        var nodeIds = request.Positions.Select(p => p.NodeId).ToList();
        var nodes = await _context.Set<OrgNode>()
            .Where(n => nodeIds.Contains(n.Id))
            .ToListAsync(cancellationToken);

        foreach (var node in nodes)
        {
            var position = request.Positions.First(p => p.NodeId == node.Id);
            node.PositionX = position.PositionX;
            node.PositionY = position.PositionY;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
