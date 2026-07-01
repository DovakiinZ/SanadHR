using FluentAssertions;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HR.Application.Common.Interfaces;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class AttendanceReferenceEntityTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = Array.Empty<string>();
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    [Fact]
    public async Task Can_persist_and_read_attendance_reference()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var txnId = Guid.NewGuid();
        var recId = Guid.NewGuid();
        db.PayrollTransactionAttendanceReferences.Add(new PayrollTransactionAttendanceReference
        {
            PayrollTransactionId = txnId, AttendanceRecordId = recId,
            Date = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
            PenaltyKind = AttendancePenaltyKind.Late, Minutes = 30, Days = 0, AmountContribution = 12.5m,
        });
        await db.SaveChangesAsync();

        var row = await db.PayrollTransactionAttendanceReferences.SingleAsync(r => r.PayrollTransactionId == txnId);
        row.PenaltyKind.Should().Be(AttendancePenaltyKind.Late);
        row.AmountContribution.Should().Be(12.5m);
    }
}
