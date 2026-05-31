using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Forms;
using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record UpdateFormFieldCommand : IRequest<FormFieldDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public FieldType FieldType { get; init; }
    public bool IsRequired { get; init; }
    public int SortOrder { get; init; }
    public string? SectionName { get; init; }
    public string? Placeholder { get; init; }
    public string? DefaultValue { get; init; }
    public string? ValidationRules { get; init; }
    public string? Options { get; init; }
}

public class UpdateFormFieldCommandHandler : IRequestHandler<UpdateFormFieldCommand, FormFieldDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateFormFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormFieldDto> Handle(UpdateFormFieldCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormFields.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormField", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.FieldType = request.FieldType;
        entity.IsRequired = request.IsRequired;
        entity.SortOrder = request.SortOrder;
        entity.SectionName = request.SectionName;
        entity.Placeholder = request.Placeholder;
        entity.DefaultValue = request.DefaultValue;
        entity.ValidationRules = request.ValidationRules;
        entity.Options = request.Options;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FormFieldDto>(entity);
    }
}
