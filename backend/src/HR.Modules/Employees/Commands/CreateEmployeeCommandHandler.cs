using HR.Application.Engines.Audit;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;
using HR.Modules.Employees.Queries;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditEngine _audit;

    public CreateEmployeeCommandHandler(ApplicationDbContext context, IAuditEngine audit)
    {
        _context = context;
        _audit = audit;
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
            EmploymentTypeId = request.EmploymentTypeId,
            HireDate = request.HireDate,
            JobTitleId = request.JobTitleId,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            ManagerId = request.ManagerId,
            Address = request.Address,
            City = request.City,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            BasicSalary = request.BasicSalary,
            Currency = request.Currency ?? "SAR",
            PaymentMethodId = request.PaymentMethodId,
            BankId = request.BankId,
            BankAccountNumber = request.BankAccountNumber,
            Iban = request.Iban,
            SalaryCardNumber = request.SalaryCardNumber,
            CardProvider = request.CardProvider,
            WorkLocationId = request.WorkLocationId,
            LeavePolicyId = request.LeavePolicyId,
            PayrollGroupId = request.PayrollGroupId,
            PhotoUrl = request.PhotoUrl,
            Notes = request.Notes,
        };

        foreach (var a in request.Allowances.Where(a => a.AllowanceTypeId != Guid.Empty))
            employee.Allowances.Add(new EmployeeAllowance
            {
                AllowanceTypeId = a.AllowanceTypeId,
                Amount = a.Amount,
                IsActive = true,
            });

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogChange("Employee", employee.Id, "Created",
            null, new { employee.EmployeeNumber, employee.FirstName, employee.LastName, employee.Email }, cancellationToken);

        var result = await EmployeeProjection.MapAsync(
            _context.Employees.Where(e => e.Id == employee.Id), _context, cancellationToken);
        return result.First();
    }
}
