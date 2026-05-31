using HR.Modules.Platform.DTOs.Forms;
using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record UnpublishFormCommand(Guid Id) : IRequest<FormDefinitionDto>;

public class UnpublishFormCommandHandler : IRequestHandler<UnpublishFormCommand, FormDefinitionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UnpublishFormCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormDefinitionDto> Handle(UnpublishFormCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormDefinition", request.Id);

        entity.IsPublished = false;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FormDefinitionDto>(entity);
    }
}
