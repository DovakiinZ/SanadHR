using System.Text.Json;
using AutoMapper;
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using HR.Modules.Tasks.DTOs;
using HR.Modules.Tasks.Entities;
using MediatR;

namespace HR.Modules.Tasks.Commands;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateTaskCommandHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new HrTask
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssigneeId = request.AssigneeId,
            AssignedById = _currentUser.UserId,
            Category = request.Category,
            Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null
        };

        if (request.Checklists != null)
        {
            foreach (var item in request.Checklists)
            {
                task.Checklists.Add(new HrTaskChecklist
                {
                    Title = item.Title,
                    SortOrder = item.SortOrder
                });
            }
        }

        task.Activities.Add(new HrTaskActivity
        {
            UserId = _currentUser.UserId,
            Action = "Created",
            Details = "Task created"
        });

        _context.HrTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }
}
