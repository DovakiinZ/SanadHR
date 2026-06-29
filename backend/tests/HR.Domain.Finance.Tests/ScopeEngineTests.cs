using HR.Application.Engines.Scope;
using HR.Infrastructure.Engines.Scope;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class ScopeEngineTests
{
    private static readonly Guid A = Guid.NewGuid(), B = Guid.NewGuid(), C = Guid.NewGuid();

    private sealed class FakeBase : IBasePopulationProvider
    {
        private readonly Guid[] _all;
        public FakeBase(params Guid[] all) => _all = all;
        public Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct) =>
            Task.FromResult<ISet<Guid>>(new HashSet<Guid>(_all));
    }

    private sealed class FakeDim : IScopeDimensionProvider
    {
        private readonly Dictionary<Guid, Guid[]> _map;
        public FakeDim(string key, Dictionary<Guid, Guid[]> map) { DimensionKey = key; _map = map; }
        public string DimensionKey { get; }
        public ScopeDimensionInfo Info => new(DimensionKey, DimensionKey, DimensionKey,
            new ScopeValueSource(ScopeValueSourceKind.Custom, null), true, null);
        public Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken ct)
        {
            var set = new HashSet<Guid>();
            foreach (var v in valueIds) if (_map.TryGetValue(v, out var emps)) set.UnionWith(emps);
            return Task.FromResult<ISet<Guid>>(set);
        }
    }

    private static ScopeEngine Engine(IBasePopulationProvider basePop, params IScopeDimensionProvider[] dims) =>
        new(dims, basePop);

    [Fact]
    public async Task Mode_All_returns_base_population()
    {
        var e = Engine(new FakeBase(A, B, C));
        var r = await e.ResolveAsync(SelectionScope.All(), default);
        Assert.Equal(new[] { A, B, C }.OrderBy(x => x), r.IncludedEmployeeIds.OrderBy(x => x));
    }

    [Fact]
    public async Task Within_dimension_is_OR_across_dimensions_is_AND()
    {
        var depSales = Guid.NewGuid(); var depOps = Guid.NewGuid(); var branchHQ = Guid.NewGuid();
        var dep = new FakeDim("Department", new() { [depSales] = new[] { A, B }, [depOps] = new[] { C } });
        var br  = new FakeDim("Branch", new() { [branchHQ] = new[] { B, C } });
        var e = Engine(new FakeBase(A, B, C), dep, br);
        var scope = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { depSales, depOps }),  // A,B,C
                    new ScopeCriterion("Branch", new[] { branchHQ }) },            // B,C
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), Array.Empty<Guid>());
        var r = await e.ResolveAsync(scope, default);
        Assert.Equal(new[] { B, C }.OrderBy(x => x), r.IncludedEmployeeIds.OrderBy(x => x)); // intersection
    }

    [Fact]
    public async Task Exclude_wins()
    {
        var depSales = Guid.NewGuid();
        var dep = new FakeDim("Department", new() { [depSales] = new[] { A, B, C } });
        var e = Engine(new FakeBase(A, B, C), dep);
        var scope = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { depSales }) },
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), new[] { B });
        var r = await e.ResolveAsync(scope, default);
        Assert.DoesNotContain(B, r.IncludedEmployeeIds);
        Assert.Contains(B, r.ExcludedByScope.Select(x => x.EmployeeId));
    }

    [Fact]
    public async Task Unavailable_dimension_is_skipped_with_warning()
    {
        var e = Engine(new FakeBase(A, B));
        var scope = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("CostCenter", new[] { Guid.NewGuid() }) },
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), Array.Empty<Guid>());
        var r = await e.ResolveAsync(scope, default);
        Assert.NotEmpty(r.Warnings);            // CostCenter has no provider
        Assert.Empty(r.IncludedEmployeeIds);    // no include matched
    }

    [Fact]
    public void Dimensions_includes_disabled_ones_with_notes()
    {
        var e = Engine(new FakeBase(), new FakeDim("Department", new()));
        var dims = e.Dimensions();
        Assert.Contains(dims, d => d.Key == "Department" && d.IsAvailable);
        Assert.Contains(dims, d => d.Key == "CostCenter" && !d.IsAvailable && d.UnavailableNote != null);
    }
}
