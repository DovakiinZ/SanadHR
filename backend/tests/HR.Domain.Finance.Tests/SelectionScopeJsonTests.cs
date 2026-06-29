using HR.Application.Engines.Scope;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class SelectionScopeJsonTests
{
    [Fact]
    public void Parse_null_returns_mode_All()
    {
        var s = SelectionScopeJson.Parse(null);
        Assert.Equal("All", s.Mode);
        Assert.Empty(s.Include);
    }

    [Fact]
    public void Parse_reads_include_exclude_and_employee_ids()
    {
        var dep = Guid.NewGuid();
        var emp = Guid.NewGuid();
        var json = $@"{{""mode"":""Criteria"",
            ""include"":[{{""dimension"":""Department"",""valueIds"":[""{dep}""]}}],
            ""exclude"":[{{""dimension"":""Status"",""valueIds"":[],""employeeIds"":[]}}],
            ""includeEmployeeIds"":[""{emp}""],""excludeEmployeeIds"":[]}}";
        var s = SelectionScopeJson.Parse(json);
        Assert.Equal("Criteria", s.Mode);
        Assert.Equal("Department", s.Include[0].Dimension);
        Assert.Equal(dep, s.Include[0].ValueIds[0]);
        Assert.Equal(emp, s.IncludeEmployeeIds[0]);
    }

    [Fact]
    public void Serialize_then_Parse_roundtrips()
    {
        var dep = Guid.NewGuid();
        var original = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { dep }) },
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), Array.Empty<Guid>());
        var back = SelectionScopeJson.Parse(SelectionScopeJson.Serialize(original));
        Assert.Equal(dep, back.Include[0].ValueIds[0]);
    }

    [Fact]
    public void Parse_malformed_returns_mode_All()
    {
        Assert.Equal("All", SelectionScopeJson.Parse("{ not json").Mode);
    }
}
