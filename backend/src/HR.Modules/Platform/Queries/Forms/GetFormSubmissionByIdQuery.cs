using HR.Modules.Platform.DTOs.Forms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Forms;

public record GetFormSubmissionByIdQuery(Guid Id) : IRequest<FormSubmissionDto>;

public class GetFormSubmissionByIdQueryHandler : IRequestHandler<GetFormSubmissionByIdQuery, FormSubmissionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public GetFormSubmissionByIdQueryHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormSubmissionDto> Handle(GetFormSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormSubmissions
            .Include(s => s.Values)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormSubmission", request.Id);

        return _mapper.Map<FormSubmissionDto>(entity);
    }
}
