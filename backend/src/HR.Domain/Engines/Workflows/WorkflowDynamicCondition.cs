using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowDynamicCondition : BaseEntity
{
    public Guid WorkflowNodeId { get; set; }
    public string ConditionType { get; set; } = null!; // EmployeeDepartment, EmployeePosition, EmployeeGrade, EmployeeBranch, LeaveDays, Salary, RequestType, CustomField
    public string FieldPath { get; set; } = null!; // e.g., employee.department.id
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? LogicalOperator { get; set; } // AND / OR
    public int SortOrder { get; set; }

    public WorkflowNode WorkflowNode { get; set; } = null!;
}
