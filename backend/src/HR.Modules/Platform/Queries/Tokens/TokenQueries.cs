using AutoMapper;
using HR.Application.Engines.Tokens;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Tokens;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Tokens;

public record GetTokenCategoriesQuery : IRequest<List<TokenCategoryDto>>;
public record GetAvailableTokensQuery(string? Category) : IRequest<List<TokenDefinitionDto>>;

// Handlers

public class GetTokenCategoriesQueryHandler : IRequestHandler<GetTokenCategoriesQuery, List<TokenCategoryDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTokenCategoriesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TokenCategoryDto>> Handle(GetTokenCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.TokenCategories
            .Include(c => c.Tokens)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<TokenCategoryDto>>(categories);
    }
}

public class GetAvailableTokensQueryHandler : IRequestHandler<GetAvailableTokensQuery, List<TokenDefinitionDto>>
{
    private readonly ITokenResolver _tokenResolver;
    private readonly IMapper _mapper;

    public GetAvailableTokensQueryHandler(ITokenResolver tokenResolver, IMapper mapper)
    {
        _tokenResolver = tokenResolver;
        _mapper = mapper;
    }

    public async Task<List<TokenDefinitionDto>> Handle(GetAvailableTokensQuery request, CancellationToken cancellationToken)
    {
        var tokens = await _tokenResolver.GetAvailableTokens(request.Category, cancellationToken);
        return _mapper.Map<List<TokenDefinitionDto>>(tokens);
    }
}
