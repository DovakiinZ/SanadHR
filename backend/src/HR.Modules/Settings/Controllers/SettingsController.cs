using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Settings.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Settings.Controllers;

[Authorize]
[Route("api/settings")]
public class SettingsController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("Settings.View")]
    public async Task<ActionResult<ApiResponse<CompanySettings>>> Get(CancellationToken ct)
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync(ct);
        return OkResponse(settings);
    }

    [HttpPut]
    [RequirePermission("Settings.Edit")]
    public async Task<ActionResult<ApiResponse<CompanySettings>>> Update([FromBody] CompanySettings request, CancellationToken ct)
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync(ct);

        if (settings == null)
        {
            _context.CompanySettings.Add(request);
        }
        else
        {
            settings.CompanyName = request.CompanyName;
            settings.CompanyNameAr = request.CompanyNameAr;
            settings.LogoUrl = request.LogoUrl;
            settings.Website = request.Website;
            settings.Email = request.Email;
            settings.Phone = request.Phone;
            settings.Address = request.Address;
            settings.City = request.City;
            settings.Country = request.Country;
            settings.Currency = request.Currency;
            settings.Timezone = request.Timezone;
            settings.DateFormat = request.DateFormat;
            settings.WorkingDaysPerWeek = request.WorkingDaysPerWeek;
            settings.WeekStartDay = request.WeekStartDay;
            settings.AnnualLeaveDays = request.AnnualLeaveDays;
        }

        await _context.SaveChangesAsync(ct);
        return OkResponse(settings ?? request, "Settings updated");
    }
}
