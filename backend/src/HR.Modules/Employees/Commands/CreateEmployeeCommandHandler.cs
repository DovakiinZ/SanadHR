using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;
using HR.Modules.Employees.Queries;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            FirstNameAr = request.FirstNameAr,
            LastName = request.LastName,
            LastNameAr = request.LastNameAr,
            Email = request.Email,
            Phone = request.Phone,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            NationalId = request.NationalId,
            NationalityId = request.NationalityId,
            ContractTypeId = request.ContractTypeId,
            HireDate = request.HireDate,
            JobTitleId = request.JobTitleId,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            ManagerId = request.ManagerId,
            BasicSalary = request.BasicSalary,
            Currency = request.Currency ?? "SAR"
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await EmployeeProjection.MapAsync(
            _context.Employees.Where(e => e.Id == employee.Id), _context, cancellationToken);
        return result.First();
    }
}
