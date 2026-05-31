using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Forms;

public record ReorderFormFieldsCommand : IRequest
{
    public Guid FormDefinitionId { get; init; }
    public List<FormFieldOrderItem> Items { get; init; } = new();
}

public record FormFieldOrderItem(Guid FieldId, int SortOrder);

public class ReorderFormFieldsCommandHandler : IRequestHandler<ReorderFormFieldsCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public ReorderFormFieldsCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReorderFormFieldsCommand request, CancellationToken cancellationToken)
    {
        var fields = await _context.FormFields
            .Where(f => f.FormDefinitionId == request.FormDefinitionId)
            .ToListAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            var field = fields.FirstOrDefault(f => f.Id == item.FieldId);
            if (field != null)
                field.SortOrder = item.SortOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
