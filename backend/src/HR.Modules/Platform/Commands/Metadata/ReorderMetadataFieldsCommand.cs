using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Metadata;

public record ReorderMetadataFieldsCommand : IRequest
{
    public Guid MetadataDefinitionId { get; init; }
    public List<FieldOrderItem> Items { get; init; } = new();
}

public record FieldOrderItem(Guid FieldId, int SortOrder);

public class ReorderMetadataFieldsCommandHandler : IRequestHandler<ReorderMetadataFieldsCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public ReorderMetadataFieldsCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReorderMetadataFieldsCommand request, CancellationToken cancellationToken)
    {
        var fields = await _context.MetadataFields
            .Where(f => f.MetadataDefinitionId == request.MetadataDefinitionId)
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
