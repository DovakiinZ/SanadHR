using HR.Modules.Tasks.DTOs;
using MediatR;

namespace HR.Modules.Tasks.Commands;

public record AddTaskCommentCommand(Guid TaskId, string Content) : IRequest<CommentDto>;
