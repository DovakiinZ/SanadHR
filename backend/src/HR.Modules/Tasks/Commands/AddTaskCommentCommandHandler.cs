using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using HR.Modules.Tasks.DTOs;
using HR.Modules.Tasks.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Tasks.Commands;

public class AddTaskCommentCommandHandler : IRequestHandler<AddTaskCommentCommand, CommentDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public AddTaskCommentCommandHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<CommentDto> Handle(AddTaskCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.HrTasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null) throw new NotFoundException("Task", request.TaskId);

        var comment = new HrTaskComment
        {
            TaskId = request.TaskId,
            UserId = _currentUser.UserId,
            UserName = _currentUser.Email,
            Content = request.Content
        };

        _context.HrTaskComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CommentDto>(comment);
    }
}
