using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Tasks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Tasks.Queries;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _context.HrTasks
            .Include(t => t.Checklists.OrderBy(c => c.SortOrder))
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
            .Include(t => t.Activities.OrderByDescending(a => a.Timestamp))
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null) throw new NotFoundException("Task", request.Id);

        return _mapper.Map<TaskDto>(task);
    }
}
