using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Queries;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
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
        employee.NationalityId = request.NationalityId;
        employee.Status = request.Status;
        employee.ContractTypeId = request.ContractTypeId;
        employee.JobTitleId = request.JobTitleId;
        employee.DepartmentId = request.DepartmentId;
        employee.BranchId = request.BranchId;
        employee.ManagerId = request.ManagerId;
        employee.BasicSalary = request.BasicSalary;

        await _context.SaveChangesAsync(cancellationToken);

        var result = await EmployeeProjection.MapAsync(
            _context.Employees.Where(e => e.Id == employee.Id), _context, cancellationToken);
        return result.First();
    }
}
