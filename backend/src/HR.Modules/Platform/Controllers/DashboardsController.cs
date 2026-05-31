using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Dashboards;
using HR.Modules.Platform.DTOs.Dashboards;
using HR.Modules.Platform.Queries.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/dashboards")]
public class DashboardsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.Dashboards.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<DashboardDefinitionDto>>>> GetAll([FromQuery] GetDashboardsQuery query, CancellationToken ct)
    { var result = await Mediator.Send(query, ct); return OkResponse(result); }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.Dashboards.View")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> GetById(Guid id, CancellationToken ct)
    { var result = await Mediator.Send(new GetDashboardByIdQuery(id), ct); return OkResponse(result); }

    [HttpGet("my/{userId:guid}")]
    [RequirePermission("Platform.Dashboards.View")]
    public async Task<ActionResult<ApiResponse<List<DashboardDefinitionDto>>>> GetMyDashboards(Guid userId, CancellationToken ct)
    { var result = await Mediator.Send(new GetMyDashboardsQuery(userId), ct); return OkResponse(result); }

    [HttpPost]
    [RequirePermission("Platform.Dashboards.Create")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> Create([FromBody] CreateDashboardCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> Update(Guid id, [FromBody] UpdateDashboardCommand command, CancellationToken ct)
    { if (id != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Dashboards.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    { await Mediator.Send(new DeleteDashboardCommand(id), ct); return OkResponse("Dashboard deleted"); }

    [HttpPost("{id:guid}/clone")]
    [RequirePermission("Platform.Dashboards.Create")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> Clone(Guid id, [FromBody] CloneDashboardCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command with { SourceDashboardId = id }, ct); return CreatedResponse(result); }

    [HttpPost("{id:guid}/share")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardShareDto>>> Share(Guid id, [FromBody] ShareDashboardCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command with { DashboardDefinitionId = id }, ct); return CreatedResponse(result); }

    [HttpDelete("shares/{shareId:guid}")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse>> RevokeShare(Guid shareId, CancellationToken ct)
    { await Mediator.Send(new RevokeDashboardShareCommand(shareId), ct); return OkResponse("Share revoked"); }

    [HttpPut("{id:guid}/layout")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse>> SaveLayout(Guid id, [FromBody] SaveDashboardLayoutCommand command, CancellationToken ct)
    { await Mediator.Send(command with { DashboardDefinitionId = id }, ct); return OkResponse("Layout saved"); }

    // Widgets
    [HttpPost("{id:guid}/widgets")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardWidgetDto>>> AddWidget(Guid id, [FromBody] AddDashboardWidgetCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command with { DashboardDefinitionId = id }, ct); return CreatedResponse(result); }

    [HttpPut("widgets/{widgetId:guid}")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardWidgetDto>>> UpdateWidget(Guid widgetId, [FromBody] UpdateDashboardWidgetCommand command, CancellationToken ct)
    { if (widgetId != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("widgets/{widgetId:guid}")]
    [RequirePermission("Platform.Dashboards.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteWidget(Guid widgetId, CancellationToken ct)
    { await Mediator.Send(new DeleteDashboardWidgetCommand(widgetId), ct); return OkResponse("Widget deleted"); }

    // Categories
    [HttpGet("categories")]
    [RequirePermission("Platform.Dashboards.View")]
    public async Task<ActionResult<ApiResponse<List<DashboardCategoryDto>>>> GetCategories(CancellationToken ct)
    { var result = await Mediator.Send(new GetDashboardCategoriesQuery(), ct); return OkResponse(result); }

    [HttpPost("categories")]
    [RequirePermission("Platform.Dashboards.Create")]
    public async Task<ActionResult<ApiResponse<DashboardCategoryDto>>> CreateCategory([FromBody] CreateDashboardCategoryCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpPut("categories/{id:guid}")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardCategoryDto>>> UpdateCategory(Guid id, [FromBody] UpdateDashboardCategoryCommand command, CancellationToken ct)
    { if (id != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("categories/{id:guid}")]
    [RequirePermission("Platform.Dashboards.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteCategory(Guid id, CancellationToken ct)
    { await Mediator.Send(new DeleteDashboardCategoryCommand(id), ct); return OkResponse("Category deleted"); }

    // Templates
    [HttpGet("templates")]
    [RequirePermission("Platform.Dashboards.View")]
    public async Task<ActionResult<ApiResponse<List<DashboardTemplateDto>>>> GetTemplates(CancellationToken ct)
    { var result = await Mediator.Send(new GetDashboardTemplatesQuery(), ct); return OkResponse(result); }

    [HttpPost("templates")]
    [RequirePermission("Platform.Dashboards.Create")]
    public async Task<ActionResult<ApiResponse<DashboardTemplateDto>>> CreateTemplate([FromBody] CreateDashboardTemplateCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpPut("templates/{id:guid}")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardTemplateDto>>> UpdateTemplate(Guid id, [FromBody] UpdateDashboardTemplateCommand command, CancellationToken ct)
    { if (id != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("templates/{id:guid}")]
    [RequirePermission("Platform.Dashboards.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteTemplate(Guid id, CancellationToken ct)
    { await Mediator.Send(new DeleteDashboardTemplateCommand(id), ct); return OkResponse("Template deleted"); }

    // Widget Library
    [HttpGet("widget-library")]
    [RequirePermission("Platform.Dashboards.View")]
    public async Task<ActionResult<ApiResponse<List<WidgetDefinitionDto>>>> GetWidgetLibrary(CancellationToken ct)
    { var result = await Mediator.Send(new GetWidgetDefinitionsQuery(), ct); return OkResponse(result); }

    [HttpPost("widget-library")]
    [RequirePermission("Platform.Dashboards.Create")]
    public async Task<ActionResult<ApiResponse<WidgetDefinitionDto>>> CreateWidgetDefinition([FromBody] CreateWidgetDefinitionCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpPut("widget-library/{id:guid}")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<WidgetDefinitionDto>>> UpdateWidgetDefinition(Guid id, [FromBody] UpdateWidgetDefinitionCommand command, CancellationToken ct)
    { if (id != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("widget-library/{id:guid}")]
    [RequirePermission("Platform.Dashboards.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteWidgetDefinition(Guid id, CancellationToken ct)
    { await Mediator.Send(new DeleteWidgetDefinitionCommand(id), ct); return OkResponse("Widget definition deleted"); }

    [HttpPost("widget-library/{id:guid}/data-sources")]
    [RequirePermission("Platform.Dashboards.Edit")]
    public async Task<ActionResult<ApiResponse<WidgetDataSourceDto>>> AddDataSource(Guid id, [FromBody] AddWidgetDataSourceCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command with { WidgetDefinitionId = id }, ct); return CreatedResponse(result); }

    [HttpDelete("widget-library/data-sources/{dataSourceId:guid}")]
    [RequirePermission("Platform.Dashboards.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteDataSource(Guid dataSourceId, CancellationToken ct)
    { await Mediator.Send(new DeleteWidgetDataSourceCommand(dataSourceId), ct); return OkResponse("Data source deleted"); }
}
