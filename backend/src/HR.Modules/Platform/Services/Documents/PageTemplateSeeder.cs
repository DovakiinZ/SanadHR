using System.Text.Json;
using HR.Domain.Engines.Documents;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Documents;

/// <summary>Seeds the built-in page-template presets (document chrome). Idempotent on Code.
/// Returns the default preset's id so the library seeder can attach templates to it.</summary>
public interface IPageTemplateSeeder
{
    Task<Guid> SeedAsync(CancellationToken ct);
}

public sealed class PageTemplateSeeder : IPageTemplateSeeder
{
    private readonly ApplicationDbContext _db;
    public PageTemplateSeeder(ApplicationDbContext db) => _db = db;

    public async Task<Guid> SeedAsync(CancellationToken ct)
    {
        var modern = await Ensure("PT_MODERN", "Modern Corporate", "مؤسسي حديث",
            header: new { showLogo = true, logoPlacement = "Left", showIdentity = true, showCrVat = true, showContact = true },
            footer: new { showQr = true, qrPlacement = "Left", showStamp = true, showSignatures = false, showGeneratedDate = true },
            margins: new { top = 40, right = 40, bottom = 40, left = 40 }, watermark: null, sort: 1, ct);

        await Ensure("PT_FORMAL", "Formal Letter", "خطاب رسمي",
            header: new { showLogo = true, logoPlacement = "Right", showIdentity = true, showCrVat = true, showContact = false },
            footer: new { showQr = true, qrPlacement = "Left", showStamp = true, showSignatures = true, showGeneratedDate = true },
            margins: new { top = 56, right = 56, bottom = 56, left = 56 }, watermark: null, sort: 2, ct);

        await Ensure("PT_GOV", "Government Style", "نمط حكومي",
            header: new { showLogo = true, logoPlacement = "Center", showIdentity = true, showCrVat = true, showContact = true },
            footer: new { showQr = true, qrPlacement = "Center", showStamp = true, showSignatures = true, showGeneratedDate = true },
            margins: new { top = 48, right = 48, bottom = 48, left = 48 }, watermark: new { text = "نسخة رسمية" }, sort: 3, ct);

        await Ensure("PT_EXECUTIVE", "Executive Style", "نمط تنفيذي",
            header: new { showLogo = true, logoPlacement = "Left", showIdentity = true, showCrVat = false, showContact = false },
            footer: new { showQr = false, showStamp = true, showSignatures = true, showGeneratedDate = true },
            margins: new { top = 60, right = 64, bottom = 60, left = 64 }, watermark: null, sort: 4, ct);

        await Ensure("PT_MINIMAL", "Clean Minimal", "بسيط أنيق",
            header: new { showLogo = true, logoPlacement = "Left", showIdentity = true, showCrVat = false, showContact = false },
            footer: new { showQr = false, showStamp = false, showSignatures = false, showGeneratedDate = true },
            margins: new { top = 36, right = 36, bottom = 36, left = 36 }, watermark: null, sort: 5, ct);

        await _db.SaveChangesAsync(ct);
        return modern;
    }

    private async Task<Guid> Ensure(string code, string en, string ar, object header, object footer, object margins, object? watermark, int sort, CancellationToken ct)
    {
        var existing = await _db.PageTemplates.FirstOrDefaultAsync(p => p.Code == code, ct);
        if (existing is not null) return existing.Id;
        var pt = new PageTemplate
        {
            Code = code, NameEn = en, NameAr = ar, IsSystem = true, IsActive = true, SortOrder = sort,
            HeaderConfig = JsonSerializer.Serialize(header),
            FooterConfig = JsonSerializer.Serialize(footer),
            Margins = JsonSerializer.Serialize(margins),
            Watermark = watermark is null ? null : JsonSerializer.Serialize(watermark),
        };
        _db.PageTemplates.Add(pt);
        await _db.SaveChangesAsync(ct);
        return pt.Id;
    }
}
