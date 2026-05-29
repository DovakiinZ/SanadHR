using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Identity.Entities;
using HR.Modules.Tenancy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HR.Modules.Identity.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, JwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<(string AccessToken, string RefreshToken, User User)> RegisterAsync(
        string companyName, string fullName, string email, string password, CancellationToken ct = default)
    {
        // Check if email already exists (ignoring query filters)
        var exists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct);
        if (exists)
            throw new ConflictException("A user with this email already exists.");

        // Create tenant
        var tenant = new Tenant
        {
            CompanyName = companyName,
            CompanyNameAr = companyName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);

        // Create admin user
        var user = new User
        {
            TenantId = tenant.Id,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        // Create default admin role
        var adminRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Admin",
            NameAr = "مدير",
            Description = "Full system access",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Roles.Add(adminRole);

        // Assign all permissions to admin role
        var permissions = await _context.Permissions.ToListAsync(ct);
        foreach (var permission in permissions)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }

        // Assign admin role to user
        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id
        });

        await _context.SaveChangesAsync(ct);

        // Generate tokens
        var permissionNames = permissions.Select(p => $"{p.Module}.{p.Name}").ToList();
        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissionNames);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ct);

        return (accessToken, refreshToken, user);
    }

    public async Task<(string AccessToken, string RefreshToken, User User)> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new NotFoundException("User", email);

        if (!user.IsActive)
            throw new ForbiddenException("Account is deactivated.");

        // Collect permissions
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Name}")
            .Union(user.UserPermissions.Select(up => $"{up.Permission.Module}.{up.Permission.Name}"))
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissions);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ct);

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return (accessToken, refreshToken, user);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(
        string accessToken, string refreshToken, CancellationToken ct = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
            throw new ForbiddenException("Invalid token.");

        var userId = Guid.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow, ct);

        if (storedToken == null)
            throw new ForbiddenException("Invalid refresh token.");

        storedToken.IsRevoked = true;

        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstAsync(u => u.Id == userId && !u.IsDeleted, ct);

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Name}")
            .Union(user.UserPermissions.Select(up => $"{up.Permission.Module}.{up.Permission.Name}"))
            .Distinct()
            .ToList();

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, permissions);
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id, ct);

        await _context.SaveChangesAsync(ct);

        return (newAccessToken, newRefreshToken);
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        var days = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
        var token = new RefreshToken
        {
            UserId = userId,
            Token = _jwtTokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        };
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync(ct);
        return token.Token;
    }
}
