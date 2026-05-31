using HR.Domain.Common;

namespace HR.Domain.Engines.Tokens;

public class TokenCategory : BaseEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public int SortOrder { get; set; }

    public ICollection<TokenDefinition> Tokens { get; set; } = new List<TokenDefinition>();
}
