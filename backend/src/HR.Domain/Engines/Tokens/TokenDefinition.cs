using HR.Domain.Common;

namespace HR.Domain.Engines.Tokens;

public class TokenDefinition : BaseEntity
{
    public string Code { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string DataType { get; set; } = null!;
    public string ResolverKey { get; set; } = null!;

    public TokenCategory Category { get; set; } = null!;
}
