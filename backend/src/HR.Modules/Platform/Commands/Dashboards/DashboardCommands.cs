using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Dashboards;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Dashboards;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Dashboards;

// === Dashboard Category Commands ===

public record CreateDashboardCategoryCommand : IRequest<DashboardCategoryDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateDashboardCategoryCommand : IRequest<DashboardCategoryDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
}

public record DeleteDashboardCategoryCommand(Guid Id) : IRequest;

// === Dashboard Template Commands ===

public record CreateDashboardTemplateCommand : IRequest<DashboardTemplateDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string? PreviewImageUrl { get; init; }
    public DashboardScope DefaultScope { get; init; }
    public string LayoutConfiguration { get; init; } = null!;
    public string WidgetConfiguration { get; init; } = null!;
}

public record UpdateDashboardTemplateCommand : IRequest<DashboardTemplateDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string? PreviewImageUrl { get; init; }
    public DashboardScope DefaultScope { get; init; }
    public string LayoutConfiguration { get; init; } = null!;
    public string WidgetConfiguration { get; init; } = null!;
}

public record DeleteDashboardTemplateCommand(Guid Id) : IRequest;

// === Dashboard Definition Commands ===

public record CreateDashboardCommand : IRequest<DashboardDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? TemplateId { get; init; }
    public DashboardScope Scope { get; init; }
    public Guid? DepartmentId { get; init; }
    public string? LayoutConfiguration { get; init; }
}

public record UpdateDashboardCommand : IRequest<DashboardDefinitionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public DashboardScope Scope { get; init; }
    public Guid? DepartmentId { get; init; }
    public bool IsDefault { get; init; }
    public string? LayoutConfiguration { get; init; }
    public int SortOrder { get; init; }
}

public record DeleteDashboardCommand(Guid Id) : IRequest;

public record CloneDashboardCommand : IRequest<DashboardDefinitionDto>
{
    public Guid SourceDashboardId { get; init; }
    public string NewCode { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
}

public record ShareDashboardCommand : IRequest<DashboardShareDto>
{
    public Guid DashboardDefinitionId { get; init; }
    public Guid? SharedWithUserId { get; init; }
    public Guid? SharedWithDepartmentId { get; init; }
    public Guid? SharedWithRoleId { get; init; }
    public bool CanEdit { get; init; }
}

public record RevokeDashboardShareCommand(Guid ShareId) : IRequest;

public record SaveDashboardLayoutCommand : IRequest
{
    public Guid DashboardDefinitionId { get; init; }
    public string LayoutConfiguration { get; init; } = null!;
    public List<WidgetLayoutInput> WidgetLayouts { get; init; } = new();
}

public class WidgetLayoutInput
{
    public Guid WidgetId { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

// === Widget Commands ===

public record AddDashboardWidgetCommand : IRequest<DashboardWidgetDto>
{
    public Guid DashboardDefinitionId { get; init; }
    public Guid? WidgetDefinitionId { get; init; }
    public WidgetType WidgetType { get; init; }
    public Guid? ObjectDefinitionId { get; init; }
    public string TitleEn { get; init; } = null!;
    public string TitleAr { get; init; } = null!;
    public string? Configuration { get; init; }
    public string? DataSourceConfig { get; init; }
    public int SortOrder { get; init; }
    public WidgetLayoutInput? Layout { get; init; }
}

public record UpdateDashboardWidgetCommand : IRequest<DashboardWidgetDto>
{
    public Guid Id { get; init; }
    public WidgetType WidgetType { get; init; }
    public string TitleEn { get; init; } = null!;
    public string TitleAr { get; init; } = null!;
    public string? Configuration { get; init; }
    public string? DataSourceConfig { get; init; }
    public int SortOrder { get; init; }
    public bool IsVisible { get; init; }
}

public record DeleteDashboardWidgetCommand(Guid Id) : IRequest;

// === Widget Definition (Library) Commands ===

public record CreateWidgetDefinitionCommand : IRequest<WidgetDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public WidgetType WidgetType { get; init; }
    public string? Icon { get; init; }
    public string? DefaultConfiguration { get; init; }
    public int DefaultWidth { get; init; } = 4;
    public int DefaultHeight { get; init; } = 3;
}

public record UpdateWidgetDefinitionCommand : IRequest<WidgetDefinitionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public WidgetType WidgetType { get; init; }
    public string? Icon { get; init; }
    public string? DefaultConfiguration { get; init; }
    public int DefaultWidth { get; init; }
    public int DefaultHeight { get; init; }
}

public record DeleteWidgetDefinitionCommand(Guid Id) : IRequest;

public record AddWidgetDataSourceCommand : IRequest<WidgetDataSourceDto>
{
    public Guid WidgetDefinitionId { get; init; }
    public DataSourceType SourceType { get; init; }
    public Guid? ObjectDefinitionId { get; init; }
    public string? QueryTemplate { get; init; }
    public string? ApiEndpoint { get; init; }
    public AggregationType? Aggregation { get; init; }
    public string? AggregationField { get; init; }
    public string? GroupByField { get; init; }
    public string? DateRangeField { get; init; }
    public int RefreshIntervalSeconds { get; init; } = 300;
}

public record DeleteWidgetDataSourceCommand(Guid Id) : IRequest;

// === Handlers ===

public class CreateDashboardCategoryCommandHandler : IRequestHandler<CreateDashboardCategoryCommand, DashboardCategoryDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateDashboardCategoryCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardCategoryDto> Handle(CreateDashboardCategoryCommand request, CancellationToken ct)
    {
        var entity = new DashboardCategory { Code = request.Code, NameEn = request.NameEn, NameAr = request.NameAr, Icon = request.Icon, SortOrder = request.SortOrder };
        _context.DashboardCategories.Add(entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardCategoryDto>(entity);
    }
}

public class UpdateDashboardCategoryCommandHandler : IRequestHandler<UpdateDashboardCategoryCommand, DashboardCategoryDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public UpdateDashboardCategoryCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardCategoryDto> Handle(UpdateDashboardCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardCategories.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardCategory", request.Id);
        entity.NameEn = request.NameEn; entity.NameAr = request.NameAr; entity.Icon = request.Icon; entity.SortOrder = request.SortOrder;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardCategoryDto>(entity);
    }
}

public class DeleteDashboardCategoryCommandHandler : IRequestHandler<DeleteDashboardCategoryCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteDashboardCategoryCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteDashboardCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardCategories.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardCategory", request.Id);
        entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}

public class CreateDashboardTemplateCommandHandler : IRequestHandler<CreateDashboardTemplateCommand, DashboardTemplateDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateDashboardTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardTemplateDto> Handle(CreateDashboardTemplateCommand request, CancellationToken ct)
    {
        var entity = new DashboardTemplate { Code = request.Code, NameEn = request.NameEn, NameAr = request.NameAr, Description = request.Description, PreviewImageUrl = request.PreviewImageUrl, DefaultScope = request.DefaultScope, LayoutConfiguration = request.LayoutConfiguration, WidgetConfiguration = request.WidgetConfiguration };
        _context.DashboardTemplates.Add(entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardTemplateDto>(entity);
    }
}

public class UpdateDashboardTemplateCommandHandler : IRequestHandler<UpdateDashboardTemplateCommand, DashboardTemplateDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public UpdateDashboardTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardTemplateDto> Handle(UpdateDashboardTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardTemplates.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardTemplate", request.Id);
        entity.NameEn = request.NameEn; entity.NameAr = request.NameAr; entity.Description = request.Description; entity.PreviewImageUrl = request.PreviewImageUrl; entity.DefaultScope = request.DefaultScope; entity.LayoutConfiguration = request.LayoutConfiguration; entity.WidgetConfiguration = request.WidgetConfiguration;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardTemplateDto>(entity);
    }
}

public class DeleteDashboardTemplateCommandHandler : IRequestHandler<DeleteDashboardTemplateCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteDashboardTemplateCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteDashboardTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardTemplates.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardTemplate", request.Id);
        entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}

public class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateDashboardCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardDefinitionDto> Handle(CreateDashboardCommand request, CancellationToken ct)
    {
        var entity = new DashboardDefinition { Code = request.Code, NameEn = request.NameEn, NameAr = request.NameAr, Description = request.Description, CategoryId = request.CategoryId, TemplateId = request.TemplateId, Scope = request.Scope, DepartmentId = request.DepartmentId, LayoutConfiguration = request.LayoutConfiguration };
        _context.DashboardDefinitions.Add(entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardDefinitionDto>(entity);
    }
}

public class UpdateDashboardCommandHandler : IRequestHandler<UpdateDashboardCommand, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public UpdateDashboardCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardDefinitionDto> Handle(UpdateDashboardCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardDefinitions.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardDefinition", request.Id);
        entity.NameEn = request.NameEn; entity.NameAr = request.NameAr; entity.Description = request.Description; entity.CategoryId = request.CategoryId; entity.Scope = request.Scope; entity.DepartmentId = request.DepartmentId; entity.IsDefault = request.IsDefault; entity.LayoutConfiguration = request.LayoutConfiguration; entity.SortOrder = request.SortOrder;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardDefinitionDto>(entity);
    }
}

public class DeleteDashboardCommandHandler : IRequestHandler<DeleteDashboardCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteDashboardCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteDashboardCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardDefinitions.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardDefinition", request.Id);
        entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}

public class CloneDashboardCommandHandler : IRequestHandler<CloneDashboardCommand, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CloneDashboardCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardDefinitionDto> Handle(CloneDashboardCommand request, CancellationToken ct)
    {
        var source = await _context.DashboardDefinitions.Include(d => d.Widgets).ThenInclude(w => w.Layout).Include(d => d.Widgets).ThenInclude(w => w.Filters).FirstOrDefaultAsync(d => d.Id == request.SourceDashboardId, ct) ?? throw new NotFoundException("DashboardDefinition", request.SourceDashboardId);
        var clone = new DashboardDefinition { Code = request.NewCode, NameEn = request.NameEn, NameAr = request.NameAr, Description = source.Description, CategoryId = source.CategoryId, Scope = DashboardScope.Personal, LayoutConfiguration = source.LayoutConfiguration };
        _context.DashboardDefinitions.Add(clone);
        await _context.SaveChangesAsync(ct);
        foreach (var widget in source.Widgets)
        {
            var clonedWidget = new DashboardWidget { DashboardDefinitionId = clone.Id, WidgetDefinitionId = widget.WidgetDefinitionId, WidgetType = widget.WidgetType, ObjectDefinitionId = widget.ObjectDefinitionId, TitleEn = widget.TitleEn, TitleAr = widget.TitleAr, Configuration = widget.Configuration, DataSourceConfig = widget.DataSourceConfig, SortOrder = widget.SortOrder, IsVisible = widget.IsVisible };
            _context.DashboardWidgets.Add(clonedWidget);
            await _context.SaveChangesAsync(ct);
            if (widget.Layout != null)
                _context.WidgetLayouts.Add(new WidgetLayout { DashboardWidgetId = clonedWidget.Id, Column = widget.Layout.Column, Row = widget.Layout.Row, Width = widget.Layout.Width, Height = widget.Layout.Height });
            foreach (var filter in widget.Filters)
                _context.WidgetFilters.Add(new WidgetFilter { DashboardWidgetId = clonedWidget.Id, FieldCode = filter.FieldCode, Operator = filter.Operator, Value = filter.Value });
        }
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardDefinitionDto>(clone);
    }
}

public class ShareDashboardCommandHandler : IRequestHandler<ShareDashboardCommand, DashboardShareDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public ShareDashboardCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardShareDto> Handle(ShareDashboardCommand request, CancellationToken ct)
    {
        var entity = new DashboardShare { DashboardDefinitionId = request.DashboardDefinitionId, SharedWithUserId = request.SharedWithUserId, SharedWithDepartmentId = request.SharedWithDepartmentId, SharedWithRoleId = request.SharedWithRoleId, CanEdit = request.CanEdit, SharedAt = DateTime.UtcNow };
        _context.DashboardShares.Add(entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardShareDto>(entity);
    }
}

public class RevokeDashboardShareCommandHandler : IRequestHandler<RevokeDashboardShareCommand>
{
    private readonly ApplicationDbContext _context;
    public RevokeDashboardShareCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(RevokeDashboardShareCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardShares.FindAsync(new object[] { request.ShareId }, ct) ?? throw new NotFoundException("DashboardShare", request.ShareId);
        _context.DashboardShares.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}

public class SaveDashboardLayoutCommandHandler : IRequestHandler<SaveDashboardLayoutCommand>
{
    private readonly ApplicationDbContext _context;
    public SaveDashboardLayoutCommandHandler(ApplicationDbContext context) { _context = context; }

    public async Task Handle(SaveDashboardLayoutCommand request, CancellationToken ct)
    {
        var dashboard = await _context.DashboardDefinitions.FindAsync(new object[] { request.DashboardDefinitionId }, ct) ?? throw new NotFoundException("DashboardDefinition", request.DashboardDefinitionId);
        dashboard.LayoutConfiguration = request.LayoutConfiguration;
        foreach (var input in request.WidgetLayouts)
        {
            var layout = await _context.WidgetLayouts.FirstOrDefaultAsync(l => l.DashboardWidgetId == input.WidgetId, ct);
            if (layout != null) { layout.Column = input.Column; layout.Row = input.Row; layout.Width = input.Width; layout.Height = input.Height; }
            else { _context.WidgetLayouts.Add(new WidgetLayout { DashboardWidgetId = input.WidgetId, Column = input.Column, Row = input.Row, Width = input.Width, Height = input.Height }); }
        }
        await _context.SaveChangesAsync(ct);
    }
}

public class AddDashboardWidgetCommandHandler : IRequestHandler<AddDashboardWidgetCommand, DashboardWidgetDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public AddDashboardWidgetCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardWidgetDto> Handle(AddDashboardWidgetCommand request, CancellationToken ct)
    {
        var widget = new DashboardWidget { DashboardDefinitionId = request.DashboardDefinitionId, WidgetDefinitionId = request.WidgetDefinitionId, WidgetType = request.WidgetType, ObjectDefinitionId = request.ObjectDefinitionId, TitleEn = request.TitleEn, TitleAr = request.TitleAr, Configuration = request.Configuration, DataSourceConfig = request.DataSourceConfig, SortOrder = request.SortOrder };
        _context.DashboardWidgets.Add(widget);
        await _context.SaveChangesAsync(ct);
        if (request.Layout != null)
        {
            _context.WidgetLayouts.Add(new WidgetLayout { DashboardWidgetId = widget.Id, Column = request.Layout.Column, Row = request.Layout.Row, Width = request.Layout.Width, Height = request.Layout.Height });
            await _context.SaveChangesAsync(ct);
        }
        return _mapper.Map<DashboardWidgetDto>(widget);
    }
}

public class UpdateDashboardWidgetCommandHandler : IRequestHandler<UpdateDashboardWidgetCommand, DashboardWidgetDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public UpdateDashboardWidgetCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardWidgetDto> Handle(UpdateDashboardWidgetCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardWidgets.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardWidget", request.Id);
        entity.WidgetType = request.WidgetType; entity.TitleEn = request.TitleEn; entity.TitleAr = request.TitleAr; entity.Configuration = request.Configuration; entity.DataSourceConfig = request.DataSourceConfig; entity.SortOrder = request.SortOrder; entity.IsVisible = request.IsVisible;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DashboardWidgetDto>(entity);
    }
}

public class DeleteDashboardWidgetCommandHandler : IRequestHandler<DeleteDashboardWidgetCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteDashboardWidgetCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteDashboardWidgetCommand request, CancellationToken ct)
    {
        var entity = await _context.DashboardWidgets.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DashboardWidget", request.Id);
        _context.DashboardWidgets.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}

public class CreateWidgetDefinitionCommandHandler : IRequestHandler<CreateWidgetDefinitionCommand, WidgetDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateWidgetDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<WidgetDefinitionDto> Handle(CreateWidgetDefinitionCommand request, CancellationToken ct)
    {
        var entity = new WidgetDefinition { Code = request.Code, NameEn = request.NameEn, NameAr = request.NameAr, Description = request.Description, WidgetType = request.WidgetType, Icon = request.Icon, DefaultConfiguration = request.DefaultConfiguration, DefaultWidth = request.DefaultWidth, DefaultHeight = request.DefaultHeight };
        _context.WidgetDefinitions.Add(entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<WidgetDefinitionDto>(entity);
    }
}

public class UpdateWidgetDefinitionCommandHandler : IRequestHandler<UpdateWidgetDefinitionCommand, WidgetDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public UpdateWidgetDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<WidgetDefinitionDto> Handle(UpdateWidgetDefinitionCommand request, CancellationToken ct)
    {
        var entity = await _context.WidgetDefinitions.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("WidgetDefinition", request.Id);
        entity.NameEn = request.NameEn; entity.NameAr = request.NameAr; entity.Description = request.Description; entity.WidgetType = request.WidgetType; entity.Icon = request.Icon; entity.DefaultConfiguration = request.DefaultConfiguration; entity.DefaultWidth = request.DefaultWidth; entity.DefaultHeight = request.DefaultHeight;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<WidgetDefinitionDto>(entity);
    }
}

public class DeleteWidgetDefinitionCommandHandler : IRequestHandler<DeleteWidgetDefinitionCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteWidgetDefinitionCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteWidgetDefinitionCommand request, CancellationToken ct)
    {
        var entity = await _context.WidgetDefinitions.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("WidgetDefinition", request.Id);
        entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}

public class AddWidgetDataSourceCommandHandler : IRequestHandler<AddWidgetDataSourceCommand, WidgetDataSourceDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public AddWidgetDataSourceCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<WidgetDataSourceDto> Handle(AddWidgetDataSourceCommand request, CancellationToken ct)
    {
        var entity = new WidgetDataSource { WidgetDefinitionId = request.WidgetDefinitionId, SourceType = request.SourceType, ObjectDefinitionId = request.ObjectDefinitionId, QueryTemplate = request.QueryTemplate, ApiEndpoint = request.ApiEndpoint, Aggregation = request.Aggregation, AggregationField = request.AggregationField, GroupByField = request.GroupByField, DateRangeField = request.DateRangeField, RefreshIntervalSeconds = request.RefreshIntervalSeconds };
        _context.WidgetDataSources.Add(entity);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<WidgetDataSourceDto>(entity);
    }
}

public class DeleteWidgetDataSourceCommandHandler : IRequestHandler<DeleteWidgetDataSourceCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteWidgetDataSourceCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteWidgetDataSourceCommand request, CancellationToken ct)
    {
        var entity = await _context.WidgetDataSources.FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("WidgetDataSource", request.Id);
        _context.WidgetDataSources.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
