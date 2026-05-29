using AutoMapper;
using AutoMapper.QueryableExtensions;
using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Queries;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEmployeeByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Where(e => e.Id == request.Id)
            .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (employee == null)
            throw new NotFoundException("Employee", request.Id);

        return employee;
    }
}
