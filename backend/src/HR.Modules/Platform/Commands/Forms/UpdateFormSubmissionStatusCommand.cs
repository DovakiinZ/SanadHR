using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Forms;
using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record UpdateFormSubmissionStatusCommand : IRequest<FormSubmissionDto>
{
    public Guid Id { get; init; }
    public FormSubmissionStatus Status { get; init; }
}

public class UpdateFormSubmissionStatusCommandHandler : IRequestHandler<UpdateFormSubmissionStatusCommand, FormSubmissionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateFormSubmissionStatusCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormSubmissionDto> Handle(UpdateFormSubmissionStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormSubmissions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormSubmission", request.Id);

        entity.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FormSubmissionDto>(entity);
    }
}
