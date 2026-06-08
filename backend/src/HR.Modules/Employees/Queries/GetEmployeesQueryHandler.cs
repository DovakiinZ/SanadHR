using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Queries;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PaginatedList<EmployeeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(e =>
                e.FirstName.Contains(request.Search) ||
                e.LastName.Contains(request.Search) ||
                e.Email.Contains(request.Search) ||
                e.EmployeeNumber.Contains(request.Search));
        }

        if (request.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == request.DepartmentId);

        if (request.BranchId.HasValue)
            query = query.Where(e => e.BranchId == request.BranchId);

        var totalCount = await query.CountAsync(cancellationToken);

        var paged = query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var items = await EmployeeProjection.MapAsync(paged, _context, cancellationToken);
        items = items.OrderByDescending(e => e.CreatedAt).ToList();

        return new PaginatedList<EmployeeDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
