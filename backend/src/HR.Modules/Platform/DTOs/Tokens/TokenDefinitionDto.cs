namespace HR.Modules.Platform.DTOs.Tokens;

public class TokenDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string CategoryCode { get; set; } = null!;
    public string CategoryNameEn { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string DataType { get; set; } = null!;
    public string ResolverKey { get; set; } = null!;
}

public class TokenCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public int SortOrder { get; set; }
    public List<TokenDefinitionDto> Tokens { get; set; } = new();
}
