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
        var excDep = Guid.NewGuid();
        var incEmp = Guid.NewGuid();
        var excEmp = Guid.NewGuid();
        var original = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { dep }) },
            new[] { new ScopeCriterion("Status", new[] { excDep }) },
            new[] { incEmp },
            new[] { excEmp });
        var back = SelectionScopeJson.Parse(SelectionScopeJson.Serialize(original));
        Assert.Equal("Criteria", back.Mode);
        Assert.Equal(dep, back.Include[0].ValueIds[0]);
        Assert.Equal("Status", back.Exclude[0].Dimension);
        Assert.Equal(excDep, back.Exclude[0].ValueIds[0]);
        Assert.Equal(incEmp, back.IncludeEmployeeIds[0]);
        Assert.Equal(excEmp, back.ExcludeEmployeeIds[0]);
    }

    [Fact]
    public void Parse_malformed_returns_mode_All()
    {
        Assert.Equal("All", SelectionScopeJson.Parse("{ not json").Mode);
    }

    // Finding 1: non-string values in employee id arrays must be skipped, not thrown
    [Fact]
    public void Parse_non_string_employee_ids_are_skipped_not_thrown()
    {
        var s = SelectionScopeJson.Parse("{\"mode\":\"Criteria\",\"includeEmployeeIds\":[123, true]}");
        Assert.Empty(s.IncludeEmployeeIds);   // non-string elements skipped, no throw
        Assert.Equal("Criteria", s.Mode);
    }

    // Finding 3: empty/whitespace input returns All
    [Fact]
    public void Parse_empty_or_whitespace_returns_All()
    {
        Assert.Equal("All", SelectionScopeJson.Parse("").Mode);
        Assert.Equal("All", SelectionScopeJson.Parse("   ").Mode);
    }
}
