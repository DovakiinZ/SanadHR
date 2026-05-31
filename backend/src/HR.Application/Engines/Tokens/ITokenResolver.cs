using HR.Domain.Engines.Tokens;

namespace HR.Application.Engines.Tokens;

public interface ITokenResolver
{
    Task<string> ResolveTokens(string templateString, Dictionary<string, object> context, CancellationToken ct = default);
    Task<List<TokenDefinition>> GetAvailableTokens(string? category = null, CancellationToken ct = default);
}
