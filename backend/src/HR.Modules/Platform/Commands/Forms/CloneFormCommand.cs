using HR.Modules.Platform.DTOs.Forms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Forms;

public record CloneFormCommand : IRequest<FormDefinitionDto>
{
    public Guid SourceFormId { get; init; }
    public string NewCode { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
}

public class CloneFormCommandHandler : IRequestHandler<CloneFormCommand, FormDefinitionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public CloneFormCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormDefinitionDto> Handle(CloneFormCommand request, CancellationToken cancellationToken)
    {
        var source = await _context.FormDefinitions
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == request.SourceFormId, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormDefinition", request.SourceFormId);

        var clone = new HR.Domain.Engines.Forms.FormDefinition
        {
            Code = request.NewCode,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Description = source.Description,
            Module = source.Module,
            Version = 1,
            IsPublished = false,
            IsActive = true
        };

        _context.FormDefinitions.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        // Clone fields
        foreach (var field in source.Fields)
        {
            var clonedField = new HR.Domain.Engines.Forms.FormField
            {
                FormDefinitionId = clone.Id,
                Code = field.Code,
                NameEn = field.NameEn,
                NameAr = field.NameAr,
                FieldType = field.FieldType,
                IsRequired = field.IsRequired,
                SortOrder = field.SortOrder,
                SectionName = field.SectionName,
                Placeholder = field.Placeholder,
                DefaultValue = field.DefaultValue,
                ValidationRules = field.ValidationRules,
                Options = field.Options
            };
            _context.FormFields.Add(clonedField);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FormDefinitionDto>(clone);
    }
}
