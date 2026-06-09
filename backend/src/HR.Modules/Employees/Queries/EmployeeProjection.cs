using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Queries;

/// <summary>
/// Maps Employee entities to EmployeeDto, resolving governed master-data references
/// (JobTitle / Nationality / ContractType / EmploymentType / PaymentMethod / Bank /
/// WorkLocation / LeavePolicy / PayrollGroup) and Core org assignments (Department /
/// Branch / Manager) to display labels. Resolves all master-data refs in a single
/// dictionary lookup rather than a wide join, and loads per-employee allowances.
/// </summary>
public static class EmployeeProjection
{
    public static async Task<List<EmployeeDto>> MapAsync(
        IQueryable<Employee> source, ApplicationDbContext ctx, CancellationToken ct)
    {
        var employees = await source.AsNoTracking().ToListAsync(ct);
        if (employees.Count == 0) return new List<EmployeeDto>();

        var empIds = employees.Select(e => e.Id).ToList();

        // Per-employee allowance overrides.
        var allowances = await ctx.EmployeeAllowances
            .Where(a => empIds.Contains(a.EmployeeId))
            .Select(a => new { a.Id, a.EmployeeId, a.AllowanceTypeId, a.Amount, a.IsActive })
            .ToListAsync(ct);

        // Gather every master-data id referenced across the page, resolve in one query.
        var mdIds = new HashSet<Guid>();
        void Add(Guid? id) { if (id.HasValue) mdIds.Add(id.Value); }
        foreach (var e in employees)
        {
            Add(e.JobTitleId); Add(e.NationalityId); Add(e.ContractTypeId); Add(e.EmploymentTypeId);
            Add(e.PaymentMethodId); Add(e.BankId); Add(e.WorkLocationId); Add(e.LeavePolicyId); Add(e.PayrollGroupId);
        }
        foreach (var a in allowances) mdIds.Add(a.AllowanceTypeId);

        var md = await ctx.MasterDataItems
            .Where(m => mdIds.Contains(m.Id))
            .Select(m => new { m.Id, m.Code, m.NameEn, m.NameAr })
            .ToDictionaryAsync(m => m.Id, ct);

        var depIds = employees.Where(e => e.DepartmentId.HasValue).Select(e => e.DepartmentId!.Value).ToHashSet();
        var brIds = employees.Where(e => e.BranchId.HasValue).Select(e => e.BranchId!.Value).ToHashSet();
        var mgrIds = employees.Where(e => e.ManagerId.HasValue).Select(e => e.ManagerId!.Value).ToHashSet();

        var deps = await ctx.Departments.Where(d => depIds.Contains(d.Id))
            .Select(d => new { d.Id, Name = d.NameAr ?? d.Name }).ToDictionaryAsync(d => d.Id, d => d.Name, ct);
        var brs = await ctx.Branch.Where(b => brIds.Contains(b.Id))
            .Select(b => new { b.Id, Name = b.NameAr ?? b.Name }).ToDictionaryAsync(b => b.Id, b => b.Name, ct);
        var mgrs = await ctx.Employees.Where(m => mgrIds.Contains(m.Id))
            .Select(m => new { m.Id, First = m.FirstNameAr ?? m.FirstName, Last = m.LastNameAr ?? m.LastName })
            .ToDictionaryAsync(m => m.Id, ct);

        string? En(Guid? id) => id.HasValue && md.TryGetValue(id.Value, out var x) ? x.NameEn : null;
        string? Ar(Guid? id) => id.HasValue && md.TryGetValue(id.Value, out var x) ? x.NameAr : null;
        string? Code(Guid? id) => id.HasValue && md.TryGetValue(id.Value, out var x) ? x.Code : null;

        var allowancesByEmp = allowances
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(a => new EmployeeAllowanceDto
            {
                Id = a.Id,
                AllowanceTypeId = a.AllowanceTypeId,
                AllowanceType = En(a.AllowanceTypeId),
                AllowanceTypeAr = Ar(a.AllowanceTypeId),
                Amount = a.Amount,
                IsActive = a.IsActive,
            }).ToList());

        return employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            EmployeeNumber = e.EmployeeNumber,
            FirstName = e.FirstName,
            FirstNameAr = e.FirstNameAr,
            LastName = e.LastName,
            LastNameAr = e.LastNameAr,
            Email = e.Email,
            Phone = e.Phone,
            Gender = e.Gender.ToString(),
            GenderAr = MapGenderAr(e.Gender),
            DateOfBirth = e.DateOfBirth,
            NationalId = e.NationalId,
            NationalityId = e.NationalityId,
            Nationality = En(e.NationalityId),
            NationalityAr = Ar(e.NationalityId),
            Status = e.Status.ToString(),
            StatusAr = MapStatusAr(e.Status),
            ContractTypeId = e.ContractTypeId,
            ContractType = En(e.ContractTypeId),
            ContractTypeAr = Ar(e.ContractTypeId),
            EmploymentTypeId = e.EmploymentTypeId,
            EmploymentType = En(e.EmploymentTypeId),
            EmploymentTypeAr = Ar(e.EmploymentTypeId),
            HireDate = e.HireDate,
            TerminationDate = e.TerminationDate,
            JobTitleId = e.JobTitleId,
            JobTitle = En(e.JobTitleId),
            JobTitleAr = Ar(e.JobTitleId),
            DepartmentId = e.DepartmentId,
            DepartmentName = e.DepartmentId.HasValue && deps.TryGetValue(e.DepartmentId.Value, out var dn) ? dn : null,
            BranchId = e.BranchId,
            BranchName = e.BranchId.HasValue && brs.TryGetValue(e.BranchId.Value, out var bn) ? bn : null,
            ManagerId = e.ManagerId,
            ManagerName = e.ManagerId.HasValue && mgrs.TryGetValue(e.ManagerId.Value, out var m)
                ? BuildManagerName(m.First, m.Last) : null,
            Address = e.Address,
            City = e.City,
            EmergencyContactName = e.EmergencyContactName,
            EmergencyContactPhone = e.EmergencyContactPhone,
            BasicSalary = e.BasicSalary,
            Currency = e.Currency,
            PaymentMethodId = e.PaymentMethodId,
            PaymentMethod = En(e.PaymentMethodId),
            PaymentMethodAr = Ar(e.PaymentMethodId),
            PaymentMethodCode = Code(e.PaymentMethodId),
            BankId = e.BankId,
            Bank = En(e.BankId),
            BankAr = Ar(e.BankId),
            BankAccountNumber = e.BankAccountNumber,
            Iban = e.Iban,
            SalaryCardNumber = e.SalaryCardNumber,
            CardProvider = e.CardProvider,
            WorkLocationId = e.WorkLocationId,
            WorkLocation = En(e.WorkLocationId),
            WorkLocationAr = Ar(e.WorkLocationId),
            LeavePolicyId = e.LeavePolicyId,
            LeavePolicy = En(e.LeavePolicyId),
            LeavePolicyAr = Ar(e.LeavePolicyId),
            PayrollGroupId = e.PayrollGroupId,
            PayrollGroup = En(e.PayrollGroupId),
            PayrollGroupAr = Ar(e.PayrollGroupId),
            PhotoUrl = e.PhotoUrl,
            Notes = e.Notes,
            Allowances = allowancesByEmp.TryGetValue(e.Id, out var al) ? al : new List<EmployeeAllowanceDto>(),
            CreatedAt = e.CreatedAt,
        }).ToList();
    }

    private static string? BuildManagerName(string? first, string? last)
    {
        var name = $"{first} {last}".Trim();
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static string MapGenderAr(Gender gender) => gender switch
    {
        Gender.Male => "ذكر",
        Gender.Female => "أنثى",
        _ => ""
    };

    private static string MapStatusAr(EmployeeStatus status) => status switch
    {
        EmployeeStatus.Active => "نشط",
        EmployeeStatus.OnLeave => "في إجازة",
        EmployeeStatus.Suspended => "موقوف",
        EmployeeStatus.Terminated => "منتهي",
        EmployeeStatus.Resigned => "مستقيل",
        _ => ""
    };
}
