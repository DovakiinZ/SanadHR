using FluentAssertions;
using HR.Infrastructure.Services;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class BackgroundExecutionContextTests
{
    [Fact]
    public void Inactive_by_default()
    {
        var ctx = new BackgroundExecutionContext();
        ctx.IsActive.Should().BeFalse();
        ctx.TenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Begin_sets_and_dispose_restores()
    {
        var ctx = new BackgroundExecutionContext();
        var tenant = Guid.NewGuid();
        var user = Guid.NewGuid();

        using (ctx.Begin(tenant, user, "ops@thamania.test"))
        {
            ctx.IsActive.Should().BeTrue();
            ctx.TenantId.Should().Be(tenant);
            ctx.UserId.Should().Be(user);
            ctx.Email.Should().Be("ops@thamania.test");
        }

        ctx.IsActive.Should().BeFalse();
        ctx.TenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Scopes_nest_and_restore_the_outer_tenant()
    {
        var ctx = new BackgroundExecutionContext();
        var outer = Guid.NewGuid();
        var inner = Guid.NewGuid();

        using (ctx.Begin(outer))
        {
            ctx.TenantId.Should().Be(outer);
            using (ctx.Begin(inner))
            {
                ctx.TenantId.Should().Be(inner);
            }
            ctx.TenantId.Should().Be(outer);
        }
    }

    [Fact]
    public async Task Flows_into_async_continuations()
    {
        var ctx = new BackgroundExecutionContext();
        var tenant = Guid.NewGuid();
        using (ctx.Begin(tenant))
        {
            await Task.Yield();
            ctx.TenantId.Should().Be(tenant);
        }
    }
}
