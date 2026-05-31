namespace HR.Domain.Enums;

public enum WidgetType
{
    KpiCard = 1,
    Table = 2,
    BarChart = 3,
    LineChart = 4,
    PieChart = 5,
    DonutChart = 6,
    TrendChart = 7,
    ProgressWidget = 8,
    ActivityFeed = 9,
    CalendarWidget = 10
}

public enum DashboardScope
{
    Personal = 1,
    Department = 2,
    Company = 3,
    Shared = 4
}

public enum DataSourceType
{
    ObjectQuery = 1,
    SqlQuery = 2,
    ApiEndpoint = 3,
    Aggregation = 4
}

public enum AggregationType
{
    Count = 1,
    Sum = 2,
    Average = 3,
    Min = 4,
    Max = 5,
    Percentage = 6
}

public enum DrilldownType
{
    DetailView = 1,
    FilteredList = 2,
    SubDashboard = 3,
    ExternalLink = 4
}

public enum ExportFormat
{
    Pdf = 1,
    Xlsx = 2,
    Csv = 3,
    Png = 4
}
