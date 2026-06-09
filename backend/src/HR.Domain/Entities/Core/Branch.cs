using HR.Domain.Common;

namespace HR.Modules.Core.Entities;

public class Branch : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Code { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }

    // Geolocation for mobile/geofence attendance (used when geofence check-in is enabled).
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? GeofenceRadiusMeters { get; set; }

    public bool IsActive { get; set; } = true;
}
