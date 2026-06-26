using HR.Api.Controllers;
using HR.Application.Common.Models;
using HR.Modules.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Identity.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken, user) = await _authService.RegisterAsync(
            request.CompanyName, request.FullName, request.Email, request.Password, ct);

        return CreatedResponse(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserInfo { Id = user.Id, Email = user.Email, FullName = user.FullName }
        }, "Registration successful");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken, user) = await _authService.LoginAsync(request.Email, request.Password, ct);

        return OkResponse(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserInfo { Id = user.Id, Email = user.Email, FullName = user.FullName }
        }, "Login successful");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, ct);

        return OkResponse(new TokenResponse { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] HR.Modules.Identity.DTOs.AcceptResetRequest request, CancellationToken ct)
    {
        await _authService.AcceptResetAsync(request.Token, request.NewPassword, ct);
        return OkResponse("Password set. You can now sign in.");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserInfo>>> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        // Simple implementation - just return claims
        return OkResponse(new UserInfo
        {
            Id = userId,
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
            FullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? ""
        });
    }
}

// Request/Response DTOs
public class RegisterRequest
{
    public string CompanyName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RefreshRequest
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserInfo User { get; set; } = null!;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
}
