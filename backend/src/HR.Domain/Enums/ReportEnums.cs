namespace HR.Domain.Enums;

public enum ReportType
{
    Tabular = 1,
    Summary = 2,
    Matrix = 3,
    Chart = 4
}

public enum ReportScope
{
    Personal = 1,
    Department = 2,
    Company = 3,
    Shared = 4
}

public enum ReportScheduleFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4
}

public enum ReportFieldType
{
    ObjectField = 1,
    CalculatedField = 2,
    AggregateField = 3,
    RelationshipField = 4
}

public enum ReportFilterOperator
{
    Equals = 1,
    NotEquals = 2,
    Contains = 3,
    StartsWith = 4,
    EndsWith = 5,
    GreaterThan = 6,
    LessThan = 7,
    GreaterThanOrEqual = 8,
    LessThanOrEqual = 9,
    Between = 10,
    In = 11,
    NotIn = 12,
    IsNull = 13,
    IsNotNull = 14
}

public enum SortDirection
{
    Ascending = 1,
    Descending = 2
}
