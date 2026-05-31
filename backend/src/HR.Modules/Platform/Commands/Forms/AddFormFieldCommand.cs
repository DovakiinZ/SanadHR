using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Forms;
using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record AddFormFieldCommand : IRequest<FormFieldDto>
{
    public Guid FormDefinitionId { get; init; }
    public string Code { get; init; } = null!;
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

public class AddFormFieldCommandHandler : IRequestHandler<AddFormFieldCommand, FormFieldDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddFormFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormFieldDto> Handle(AddFormFieldCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.FormDefinitions.FindAsync(new object[] { request.FormDefinitionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormDefinition", request.FormDefinitionId);

        var entity = new HR.Domain.Engines.Forms.FormField
        {
            FormDefinitionId = request.FormDefinitionId,
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
            SectionName = request.SectionName,
            Placeholder = request.Placeholder,
            DefaultValue = request.DefaultValue,
            ValidationRules = request.ValidationRules,
            Options = request.Options
        };

        _context.FormFields.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FormFieldDto>(entity);
    }
}
