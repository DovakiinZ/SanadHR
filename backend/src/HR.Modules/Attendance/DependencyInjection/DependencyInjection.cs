using HR.Modules.Attendance.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Attendance;

public static class DependencyInjection
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services)
    {
        services.AddScoped<IAttendanceCalculationService, AttendanceCalculationService>();
        services.AddScoped<IShiftResolver, ShiftResolver>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        return services;
    }
}
