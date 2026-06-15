using HR.Domain.Engines.Attendance;

namespace HR.Modules.Attendance.Services;

/// <summary>Minimal employee scope used to resolve a shift assignment.</summary>
public readonly record struct EmployeeScope(Guid Id, Guid? DepartmentId, Guid? BranchId, Guid? JobTitleId);

public interface IShiftResolver
{
    /// <summary>Pick the effective shift for an employee on a date from preloaded assignments.
    /// Most-specific wins: Employee &gt; Department &gt; Branch &gt; JobTitle, then highest Priority,
    /// then latest EffectiveFrom.</summary>
    Shift? Resolve(
        IReadOnlyList<ShiftAssignment> assignments,
        IReadOnlyDictionary<Guid, Shift> shiftsById,
        EmployeeScope emp, DateTime date);
}

public sealed class ShiftResolver : IShiftResolver
{
    public Shift? Resolve(
        IReadOnlyList<ShiftAssignment> assignments,
        IReadOnlyDictionary<Guid, Shift> shiftsById,
        EmployeeScope emp, DateTime date)
    {
        var d = date.Date;
        ShiftAssignment? best = null;
        var bestSpecificity = -1;

        foreach (var a in assignments)
        {
            if (!a.IsActive) continue;
            if (a.EffectiveFrom.Date > d) continue;
            if (a.EffectiveTo is { } to && to.Date < d) continue;

            var specificity = Match(a, emp);
            if (specificity < 0) continue;

            if (best is null
                || specificity > bestSpecificity
                || (specificity == bestSpecificity && a.Priority > best.Priority)
                || (specificity == bestSpecificity && a.Priority == best.Priority && a.EffectiveFrom > best.EffectiveFrom))
            {
                best = a;
                bestSpecificity = specificity;
            }
        }

        if (best is null) return null;
        return shiftsById.TryGetValue(best.ShiftId, out var shift) ? shift : null;
    }

    /// <summary>Returns a specificity rank (Employee=4 … JobTitle=1), or -1 when the assignment does
    /// not target this employee.</summary>
    private static int Match(ShiftAssignment a, EmployeeScope emp)
    {
        if (a.EmployeeId is { } e) return e == emp.Id ? 4 : -1;
        if (a.DepartmentId is { } dep) return dep == emp.DepartmentId ? 3 : -1;
        if (a.BranchId is { } br) return br == emp.BranchId ? 2 : -1;
        if (a.JobTitleId is { } jt) return jt == emp.JobTitleId ? 1 : -1;
        return -1; // a global/untargeted assignment is ignored (no scope set)
    }
}
