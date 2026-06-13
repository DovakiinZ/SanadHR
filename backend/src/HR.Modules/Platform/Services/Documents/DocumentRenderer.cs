using System.Text.Json;
using HR.Domain.Engines.MasterData;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HR.Modules.Platform.Services.Documents;

/// <summary>Renders an official PDF for an approved request (logo, CR/VAT, employee +
/// request details, approvals, QR, stamp) using QuestPDF — works on Linux, no native deps.</summary>
public interface IDocumentRenderer
{
    Task<(byte[] pdf, string fileName)> RenderRequestPdfAsync(Guid requestInstanceId, CancellationToken ct);
}

public sealed class DocumentRenderer : IDocumentRenderer
{
    private readonly ApplicationDbContext _db;
    private const string FontFamily = "Thmanyah Sans";

    static DocumentRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "Fonts");
            if (Directory.Exists(dir))
                foreach (var f in Directory.GetFiles(dir, "*.otf"))
                    QuestPDF.Drawing.FontManager.RegisterFont(File.OpenRead(f));
        }
        catch { /* fall back to system fonts */ }
    }

    public DocumentRenderer(ApplicationDbContext db) => _db = db;

    public async Task<(byte[] pdf, string fileName)> RenderRequestPdfAsync(Guid requestInstanceId, CancellationToken ct)
    {
        var instance = await _db.RequestInstances.Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == requestInstanceId, ct)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("RequestInstance", requestInstanceId);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == instance.EmployeeId, ct);
        var company = await _db.CompanyProfiles.FirstOrDefaultAsync(ct);
        var approvals = await _db.RequestApprovals.Where(a => a.RequestInstanceId == requestInstanceId)
            .OrderBy(a => a.StepOrder).ToListAsync(ct);

        string? title = null, bodyTemplate = null;
        if (instance.RequestType.PrintTemplateId is { } tplId)
        {
            var tpl = await _db.DocumentTemplates.Where(t => t.Id == tplId).Select(t => new { t.NameAr, t.BodyTemplate }).FirstOrDefaultAsync(ct);
            title = tpl?.NameAr; bodyTemplate = tpl?.BodyTemplate;
        }
        title ??= instance.RequestType.NameAr;

        var department = employee?.DepartmentId is { } depId
            ? await _db.Departments.Where(d => d.Id == depId).Select(d => d.NameAr).FirstOrDefaultAsync(ct) : null;
        var jobTitle = employee?.JobTitleId is { } jtId
            ? await _db.MasterDataItems.Where(m => m.Id == jtId).Select(m => m.NameAr).FirstOrDefaultAsync(ct) : null;
        var leaveType = instance.LeaveTypeId is { } ltId
            ? await _db.MasterDataItems.Where(m => m.Id == ltId).Select(m => m.NameAr).FirstOrDefaultAsync(ct) : null;
        var managerName = employee?.ManagerId is { } mId
            ? await _db.Employees.Where(e => e.Id == mId).Select(e => (e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName)).FirstOrDefaultAsync(ct) : null;

        var logo = await LoadImageAsync(company?.LogoUrl, ct);
        var stamp = await LoadImageAsync(company?.StampUrl, ct);
        var contact = ParseJson(company?.ContactInfo);
        var address = ParseJson(company?.NationalAddress);

        var employeeName = employee is null ? "—" : $"{employee.FirstNameAr ?? employee.FirstName} {employee.LastNameAr ?? employee.LastName}".Trim();
        var generatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var qr = MakeQr($"{company?.NameAr} | {instance.RequestNumber} | {employeeName} | {generatedDate}");

        var details = new List<(string, string)>
        {
            ("اسم الموظف", employeeName),
            ("الرقم الوظيفي", employee?.EmployeeNumber ?? "—"),
            ("الإدارة", department ?? "—"),
            ("المسمى الوظيفي", jobTitle ?? "—"),
        };
        if (leaveType is not null) details.Add(("نوع الإجازة", leaveType));
        if (instance.StartDate is { } sd) details.Add(("من", sd.ToString("yyyy-MM-dd")));
        if (instance.EndDate is { } ed) details.Add(("إلى", ed.ToString("yyyy-MM-dd")));
        if (instance.DaysCount is { } dc) details.Add(("عدد الأيام", $"{dc}"));

        // Admin-authored template body: resolve tokens, render its HTML subset (else fall back to the details table).
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Employee.FullName"] = employeeName,
            ["Employee.EmployeeNumber"] = employee?.EmployeeNumber ?? "",
            ["Employee.Department"] = department ?? "",
            ["Employee.JobTitle"] = jobTitle ?? "",
            ["Employee.Manager"] = managerName ?? "",
            ["Request.Number"] = instance.RequestNumber,
            ["Request.CreatedDate"] = instance.SubmittedAt.ToString("yyyy-MM-dd"),
            ["Request.LeaveType"] = leaveType ?? "",
            ["Request.StartDate"] = instance.StartDate?.ToString("yyyy-MM-dd") ?? "",
            ["Request.EndDate"] = instance.EndDate?.ToString("yyyy-MM-dd") ?? "",
            ["Request.Days"] = instance.DaysCount?.ToString() ?? "",
            ["Company.Name"] = company?.NameAr ?? "",
            ["Company.CR"] = company?.CommercialRegistration ?? "",
            ["Company.VAT"] = company?.VatNumber ?? "",
            ["System.Today"] = generatedDate,
            // Aliases for the originally-seeded template token names
            ["EmployeeName"] = employeeName,
            ["EmployeeNumber"] = employee?.EmployeeNumber ?? "",
            ["Department"] = department ?? "",
            ["JobTitle"] = jobTitle ?? "",
            ["LeaveType"] = leaveType ?? "",
            ["StartDate"] = instance.StartDate?.ToString("yyyy-MM-dd") ?? "",
            ["EndDate"] = instance.EndDate?.ToString("yyyy-MM-dd") ?? "",
            ["CompanyName"] = company?.NameAr ?? "",
            ["CRNumber"] = company?.CommercialRegistration ?? "",
            ["VATNumber"] = company?.VatNumber ?? "",
            ["GeneratedDate"] = generatedDate,
        };
        var bodyBlocks = string.IsNullOrWhiteSpace(bodyTemplate) ? null : ParseHtmlBlocks(ResolveTokens(bodyTemplate, tokens));

        var pdf = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(11).DirectionFromRightToLeft());

                // Header — company identity
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(company?.NameAr ?? "الشركة").FontSize(16).Bold();
                            if (!string.IsNullOrWhiteSpace(company?.NameEn)) c.Item().Text(company!.NameEn).FontSize(10).FontColor(Colors.Grey.Medium);
                            var line = string.Join("  •  ", new[]
                            {
                                company?.CommercialRegistration is { Length: > 0 } cr ? $"س.ت: {cr}" : null,
                                company?.VatNumber is { Length: > 0 } vat ? $"ض.ق.م: {vat}" : null,
                            }.Where(s => s is not null));
                            if (line.Length > 0) c.Item().Text(line).FontSize(9).FontColor(Colors.Grey.Medium);
                            var contactLine = string.Join("  •  ", new[]
                            {
                                company?.Phone ?? Str(contact, "phone"),
                                company?.Email ?? Str(contact, "email"),
                                company?.Website ?? Str(contact, "website"),
                                string.Join(" ", new[] { company?.Address, company?.City, company?.Country }.Where(s => !string.IsNullOrWhiteSpace(s))) is { Length: > 0 } addr ? addr : Str(address, "city"),
                            }.Where(s => !string.IsNullOrWhiteSpace(s)));
                            if (contactLine.Length > 0) c.Item().Text(contactLine).FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                        if (logo is not null) row.ConstantItem(90).AlignLeft().Height(60).Image(logo).FitHeight();
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Spacing(12);
                    col.Item().AlignCenter().Text(title).FontSize(15).Bold();

                    if (bodyBlocks is not null)
                    {
                        // Render the admin's template body (token-resolved HTML subset).
                        foreach (var (kind, text) in bodyBlocks)
                        {
                            if (string.IsNullOrWhiteSpace(text) && kind != "hr") continue;
                            switch (kind)
                            {
                                case "h1": col.Item().Text(text).FontSize(14).Bold(); break;
                                case "h2": col.Item().Text(text).FontSize(12).Bold(); break;
                                case "h3": col.Item().Text(text).FontSize(11).Bold(); break;
                                case "li": col.Item().Text($"•  {text}").FontSize(11); break;
                                case "hr": col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1); break;
                                default: col.Item().Text(text).FontSize(11); break;
                            }
                        }
                    }
                    else
                    {
                        // Fallback: structured details table.
                        col.Item().Text($"رقم الطلب: {instance.RequestNumber}").FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); });
                            foreach (var (k, v) in details)
                            {
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(k).Bold().FontSize(10);
                                table.Cell().Padding(6).Text(v).FontSize(10);
                            }
                        });
                    }

                    // Approval block (always appended for official documents).
                    if (approvals.Count > 0)
                    {
                        col.Item().PaddingTop(6).Text("سلسلة الموافقات").Bold();
                        col.Item().Column(ac =>
                        {
                            foreach (var a in approvals)
                                ac.Item().Text($"{a.StepOrder}. {a.StepNameAr} — {StatusAr(a.Status.ToString())}").FontSize(10);
                        });
                    }
                });

                // Footer — QR, stamp, date
                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.ConstantItem(70).Height(70).Image(qr).FitArea();
                        row.RelativeItem().AlignMiddle().Text($"تاريخ الإصدار: {generatedDate}").FontSize(9).FontColor(Colors.Grey.Medium);
                        if (stamp is not null) row.ConstantItem(80).Height(70).AlignLeft().Image(stamp).FitArea();
                    });
                });
            });
        }).GeneratePdf();

        return (pdf, $"{instance.RequestType.Code}-{instance.RequestNumber}.pdf");
    }

    // ── helpers ──

    private static byte[] MakeQr(string text)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(data).GetGraphic(20);
    }

    private async Task<byte[]?> LoadImageAsync(string? url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var idx = url.LastIndexOf('/');
        var idPart = idx >= 0 ? url[(idx + 1)..] : url;
        if (!Guid.TryParse(idPart, out var fileId)) return null;
        return await _db.Files.Where(f => f.Id == fileId).Select(f => f.Data).FirstOrDefaultAsync(ct);
    }

    private static Dictionary<string, JsonElement>? ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json); } catch { return null; }
    }

    private static string? Str(Dictionary<string, JsonElement>? d, string key)
        => d is not null && d.TryGetValue(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string StatusAr(string status) => status switch
    {
        "Approved" => "تمت الموافقة", "Rejected" => "مرفوض", "Pending" => "بانتظار", "Skipped" => "تم التخطي", _ => status,
    };

    /// <summary>Replace {{Token.Path}} placeholders with resolved values.</summary>
    public static string ResolveTokens(string template, IReadOnlyDictionary<string, string> tokens)
        => System.Text.RegularExpressions.Regex.Replace(template, @"\{\{\s*([\w.]+)\s*\}\}",
            m => tokens.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

    /// <summary>Parse a token-resolved HTML body into a renderable block sequence (subset).</summary>
    public static List<(string kind, string text)> ParseHtmlBlocks(string html)
    {
        var s = html ?? "";
        var R = System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase;
        string Rep(string input, string pat, string repl) => System.Text.RegularExpressions.Regex.Replace(input, pat, repl, R);

        s = Rep(s, @"<br\s*/?>", "\n");
        s = Rep(s, @"<hr\s*/?>", "\n@@HR@@\n");
        s = Rep(s, @"<h1[^>]*>(.*?)</h1>", "\n@@H1@@$1\n");
        s = Rep(s, @"<h2[^>]*>(.*?)</h2>", "\n@@H2@@$1\n");
        s = Rep(s, @"<h3[^>]*>(.*?)</h3>", "\n@@H3@@$1\n");
        s = Rep(s, @"<li[^>]*>(.*?)</li>", "\n@@LI@@$1\n");
        s = Rep(s, @"</(p|div|tr)>", "\n");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"<[^>]+>", "", R);          // strip remaining tags
        s = s.Replace("&nbsp;", " ").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"");

        var blocks = new List<(string, string)>();
        foreach (var raw in s.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith("@@HR@@")) { blocks.Add(("hr", "")); continue; }
            if (line.StartsWith("@@H1@@")) { blocks.Add(("h1", line[6..].Trim())); continue; }
            if (line.StartsWith("@@H2@@")) { blocks.Add(("h2", line[6..].Trim())); continue; }
            if (line.StartsWith("@@H3@@")) { blocks.Add(("h3", line[6..].Trim())); continue; }
            if (line.StartsWith("@@LI@@")) { blocks.Add(("li", line[6..].Trim())); continue; }
            blocks.Add(("p", line));
        }
        return blocks;
    }
}
