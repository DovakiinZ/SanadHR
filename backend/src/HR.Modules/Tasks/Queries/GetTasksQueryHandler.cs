using AutoMapper;
using AutoMapper.QueryableExtensions;
using HR.Application.Common.Models;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Tasks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Tasks.Queries;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PaginatedList<TaskDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.HrTasks
            .Include(t => t.Checklists.OrderBy(c => c.SortOrder))
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
            .Include(t => t.Activities.OrderByDescending(a => a.Timestamp))
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(t => t.Title.Contains(request.Search) || (t.Description != null && t.Description.Contains(request.Search)));

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<HrTaskStatus>(request.Status, out var status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(request.Priority) && Enum.TryParse<TaskPriority>(request.Priority, out var priority))
            query = query.Where(t => t.Priority == priority);

        if (request.AssigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == request.AssigneeId);

        if (!string.IsNullOrEmpty(request.Category))
            query = query.Where(t => t.Category == request.Category);

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<TaskDto>>(tasks);

        return new PaginatedList<TaskDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
