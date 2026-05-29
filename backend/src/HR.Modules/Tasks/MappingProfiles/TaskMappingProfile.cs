using AutoMapper;
using HR.Domain.Enums;
using HR.Modules.Tasks.DTOs;
using HR.Modules.Tasks.Entities;

namespace HR.Modules.Tasks.MappingProfiles;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<HrTask, TaskDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StatusAr, opt => opt.MapFrom(s => MapStatusAr(s.Status)))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.PriorityAr, opt => opt.MapFrom(s => MapPriorityAr(s.Priority)))
            .ForMember(d => d.Source, opt => opt.MapFrom(s => s.Source.ToString()))
            .ForMember(d => d.Tags, opt => opt.MapFrom(s =>
                s.Tags != null ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(s.Tags) : null))
            .ForMember(d => d.AssigneeName, opt => opt.Ignore());

        CreateMap<HrTaskChecklist, ChecklistItemDto>();
        CreateMap<HrTaskComment, CommentDto>();
        CreateMap<HrTaskActivity, ActivityDto>();
    }

    private static string MapStatusAr(HrTaskStatus status) => status switch
    {
        HrTaskStatus.NotStarted => "لم يبدأ",
        HrTaskStatus.InProgress => "قيد التنفيذ",
        HrTaskStatus.Completed => "مكتمل",
        HrTaskStatus.Cancelled => "ملغي",
        HrTaskStatus.OnHold => "معلق",
        _ => ""
    };

    private static string MapPriorityAr(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "منخفض",
        TaskPriority.Medium => "متوسط",
        TaskPriority.High => "عالي",
        TaskPriority.Urgent => "عاجل",
        _ => ""
    };
}
