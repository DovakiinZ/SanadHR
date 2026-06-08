using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using MediatR;

namespace HR.Modules.Employees.Queries;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await EmployeeProjection.MapAsync(
            _context.Employees.Where(e => e.Id == request.Id), _context, cancellationToken);

        var employee = result.FirstOrDefault();
        if (employee == null)
            throw new NotFoundException("Employee", request.Id);

        return employee;
    }
}
