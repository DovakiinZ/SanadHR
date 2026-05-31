using HR.Modules.Platform.DTOs.Forms;
using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record PublishFormCommand(Guid Id) : IRequest<FormDefinitionDto>;

public class PublishFormCommandHandler : IRequestHandler<PublishFormCommand, FormDefinitionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public PublishFormCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormDefinitionDto> Handle(PublishFormCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormDefinition", request.Id);

        entity.IsPublished = true;
        entity.Version++;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FormDefinitionDto>(entity);
    }
}
