using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Attendance;

public static class DependencyInjection
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services)
    {
        return services;
    }
}
