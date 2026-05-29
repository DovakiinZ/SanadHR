using AutoMapper;
using HR.Domain.Enums;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;

namespace HR.Modules.Employees.MappingProfiles;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.Gender, opt => opt.MapFrom(s => s.Gender.ToString()))
            .ForMember(d => d.GenderAr, opt => opt.MapFrom(s => MapGenderAr(s.Gender)))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StatusAr, opt => opt.MapFrom(s => MapStatusAr(s.Status)))
            .ForMember(d => d.ContractType, opt => opt.MapFrom(s => s.ContractType.ToString()))
            .ForMember(d => d.ContractTypeAr, opt => opt.MapFrom(s => MapContractTypeAr(s.ContractType)))
            .ForMember(d => d.DepartmentName, opt => opt.Ignore())
            .ForMember(d => d.BranchName, opt => opt.Ignore())
            .ForMember(d => d.ManagerName, opt => opt.Ignore());
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

    private static string MapContractTypeAr(ContractType type) => type switch
    {
        ContractType.FullTime => "دوام كامل",
        ContractType.PartTime => "دوام جزئي",
        ContractType.Contract => "عقد",
        ContractType.Temporary => "مؤقت",
        ContractType.Internship => "تدريب",
        _ => ""
    };
}
