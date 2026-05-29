using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateEmployeeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.Id }, cancellationToken);
        if (employee == null) throw new NotFoundException("Employee", request.Id);

        employee.FirstName = request.FirstName;
        employee.FirstNameAr = request.FirstNameAr;
        employee.LastName = request.LastName;
        employee.LastNameAr = request.LastNameAr;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.Gender = request.Gender;
        employee.DateOfBirth = request.DateOfBirth;
        employee.NationalId = request.NationalId;
        employee.Nationality = request.Nationality;
        employee.Status = request.Status;
        employee.ContractType = request.ContractType;
        employee.JobTitle = request.JobTitle;
        employee.JobTitleAr = request.JobTitleAr;
        employee.DepartmentId = request.DepartmentId;
        employee.BranchId = request.BranchId;
        employee.ManagerId = request.ManagerId;
        employee.BasicSalary = request.BasicSalary;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<EmployeeDto>(employee);
    }
}
