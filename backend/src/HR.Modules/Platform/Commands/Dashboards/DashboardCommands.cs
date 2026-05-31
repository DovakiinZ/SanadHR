using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Dashboards;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Dashboards;
using MediatR;

namespace HR.Modules.Platform.Commands.Dashboards;

public record CreateDashboardDefinitionCommand : IRequest<DashboardDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public bool IsDefault { get; init; }
}

public record UpdateDashboardDefinitionCommand : IRequest<DashboardDefinitionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public bool IsDefault { get; init; }
}

public record DeleteDashboardDefinitionCommand(Guid Id) : IRequest;

// Handlers

public class CreateDashboardDefinitionCommandHandler : IRequestHandler<CreateDashboardDefinitionCommand, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateDashboardDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DashboardDefinitionDto> Handle(CreateDashboardDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new DashboardDefinition
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            IsDefault = request.IsDefault
        };

        _context.DashboardDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DashboardDefinitionDto>(entity);
    }
}

public class UpdateDashboardDefinitionCommandHandler : IRequestHandler<UpdateDashboardDefinitionCommand, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateDashboardDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DashboardDefinitionDto> Handle(UpdateDashboardDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DashboardDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("DashboardDefinition", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DashboardDefinitionDto>(entity);
    }
}

public class DeleteDashboardDefinitionCommandHandler : IRequestHandler<DeleteDashboardDefinitionCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteDashboardDefinitionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDashboardDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DashboardDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("DashboardDefinition", request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
