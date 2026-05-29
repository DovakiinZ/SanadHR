using MediatR;

namespace HR.Modules.Tasks.Commands;

public record DeleteTaskCommand(Guid Id) : IRequest<Unit>;
