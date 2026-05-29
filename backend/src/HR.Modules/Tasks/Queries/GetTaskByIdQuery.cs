using HR.Modules.Tasks.DTOs;
using MediatR;

namespace HR.Modules.Tasks.Queries;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto>;
