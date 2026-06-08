using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Queries;

/// <summary>
/// Maps Employee entities to EmployeeDto, resolving governed master-data references
/// (JobTitle / Nationality / ContractType) and Core org assignments (Department /
/// Branch / Manager) to their display labels via left joins. Replaces AutoMapper for
/// Employee reads so id-based references resolve to Arabic/English labels.
/// </summary>
public static class EmployeeProjection
{
    public static async Task<List<EmployeeDto>> MapAsync(
        IQueryable<Employee> source, ApplicationDbContext ctx, CancellationToken ct)
    {
        var rows = await (
            from e in source
            join jtx in ctx.MasterDataItems on e.JobTitleId equals (Guid?)jtx.Id into jtj
            from jt in jtj.DefaultIfEmpty()
            join natx in ctx.MasterDataItems on e.NationalityId equals (Guid?)natx.Id into natj
            from nat in natj.DefaultIfEmpty()
            join ctpx in ctx.MasterDataItems on e.ContractTypeId equals (Guid?)ctpx.Id into ctpj
            from ctp in ctpj.DefaultIfEmpty()
            join depx in ctx.Departments on e.DepartmentId equals (Guid?)depx.Id into depj
            from dep in depj.DefaultIfEmpty()
            join brx in ctx.Branch on e.BranchId equals (Guid?)brx.Id into brj
            from br in brj.DefaultIfEmpty()
            join mgrx in ctx.Employees on e.ManagerId equals (Guid?)mgrx.Id into mgrj
            from mgr in mgrj.DefaultIfEmpty()
            select new Row
            {
                E = e,
                JobTitleEn = jt != null ? jt.NameEn : null,
                JobTitleAr = jt != null ? jt.NameAr : null,
                NationalityEn = nat != null ? nat.NameEn : null,
                NationalityAr = nat != null ? nat.NameAr : null,
                ContractEn = ctp != null ? ctp.NameEn : null,
                ContractAr = ctp != null ? ctp.NameAr : null,
                DepartmentName = dep != null ? (dep.NameAr ?? dep.Name) : null,
                BranchName = br != null ? (br.NameAr ?? br.Name) : null,
                ManagerFirst = mgr != null ? (mgr.FirstNameAr ?? mgr.FirstName) : null,
                ManagerLast = mgr != null ? (mgr.LastNameAr ?? mgr.LastName) : null,
            }).ToListAsync(ct);

        return rows
            .Select(r => new EmployeeDto
            {
                Id = r.E.Id,
                EmployeeNumber = r.E.EmployeeNumber,
                FirstName = r.E.FirstName,
                FirstNameAr = r.E.FirstNameAr,
                LastName = r.E.LastName,
                LastNameAr = r.E.LastNameAr,
                Email = r.E.Email,
                Phone = r.E.Phone,
                Gender = r.E.Gender.ToString(),
                GenderAr = MapGenderAr(r.E.Gender),
                DateOfBirth = r.E.DateOfBirth,
                NationalId = r.E.NationalId,
                NationalityId = r.E.NationalityId,
                Nationality = r.NationalityEn,
                NationalityAr = r.NationalityAr,
                Status = r.E.Status.ToString(),
                StatusAr = MapStatusAr(r.E.Status),
                ContractTypeId = r.E.ContractTypeId,
                ContractType = r.ContractEn,
                ContractTypeAr = r.ContractAr,
                HireDate = r.E.HireDate,
                TerminationDate = r.E.TerminationDate,
                JobTitleId = r.E.JobTitleId,
                JobTitle = r.JobTitleEn,
                JobTitleAr = r.JobTitleAr,
                DepartmentId = r.E.DepartmentId,
                DepartmentName = r.DepartmentName,
                BranchId = r.E.BranchId,
                BranchName = r.BranchName,
                ManagerId = r.E.ManagerId,
                ManagerName = BuildManagerName(r.ManagerFirst, r.ManagerLast),
                BasicSalary = r.E.BasicSalary,
                Currency = r.E.Currency,
                PhotoUrl = r.E.PhotoUrl,
                CreatedAt = r.E.CreatedAt,
            })
            .ToList();
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

    private class Row
    {
        public Employee E = null!;
        public string? JobTitleEn;
        public string? JobTitleAr;
        public string? NationalityEn;
        public string? NationalityAr;
        public string? ContractEn;
        public string? ContractAr;
        public string? DepartmentName;
        public string? BranchName;
        public string? ManagerFirst;
        public string? ManagerLast;
    }
}
