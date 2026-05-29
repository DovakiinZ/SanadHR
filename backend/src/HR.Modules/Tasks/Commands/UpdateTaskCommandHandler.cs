using System.Text.Json;
using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using HR.Modules.Tasks.DTOs;
using HR.Modules.Tasks.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Tasks.Commands;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public UpdateTaskCommandHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.HrTasks
            .Include(t => t.Checklists)
            .Include(t => t.Activities)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null) throw new NotFoundException("Task", request.Id);

        var oldStatus = task.Status;
        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssigneeId = request.AssigneeId;
        task.Category = request.Category;
        task.Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null;
        task.Progress = request.Progress;
        task.Notes = request.Notes;

        if (request.Status == HR.Domain.Enums.HrTaskStatus.Completed && oldStatus != HR.Domain.Enums.HrTaskStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;

        task.Activities.Add(new HrTaskActivity
        {
            UserId = _currentUser.UserId,
            Action = "Updated",
            Details = oldStatus != request.Status ? $"Status changed from {oldStatus} to {request.Status}" : "Task updated"
        });

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }
}
