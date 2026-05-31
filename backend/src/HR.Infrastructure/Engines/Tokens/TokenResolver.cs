using System.Text.RegularExpressions;
using HR.Application.Engines.Tokens;
using HR.Domain.Engines.Tokens;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Tokens;

public class TokenResolver : ITokenResolver
{
    private readonly ApplicationDbContext _context;

    public TokenResolver(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> ResolveTokens(string templateString, Dictionary<string, object> context, CancellationToken ct = default)
    {
        var tokens = await _context.TokenDefinitions
            .Include(t => t.Category)
            .ToListAsync(ct);

        var result = templateString;
        foreach (var token in tokens)
        {
            var placeholder = $"{{{{{token.Code}}}}}";
            if (result.Contains(placeholder) && context.TryGetValue(token.ResolverKey, out var value))
            {
                result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
            }
        }

        return result;
    }

    public async Task<List<TokenDefinition>> GetAvailableTokens(string? category = null, CancellationToken ct = default)
    {
        var query = _context.TokenDefinitions
            .Include(t => t.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category.Code == category);

        return await query.OrderBy(t => t.Category.SortOrder).ThenBy(t => t.Code).ToListAsync(ct);
    }
}
