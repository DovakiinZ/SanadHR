using AutoMapper;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateEmployeeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
            Nationality = request.Nationality,
            ContractType = request.ContractType,
            HireDate = request.HireDate,
            JobTitle = request.JobTitle,
            JobTitleAr = request.JobTitleAr,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            ManagerId = request.ManagerId,
            BasicSalary = request.BasicSalary,
            Currency = request.Currency ?? "SAR"
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmployeeDto>(employee);
    }
}
