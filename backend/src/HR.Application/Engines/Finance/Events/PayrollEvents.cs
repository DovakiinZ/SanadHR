using MediatR;

namespace HR.Application.Engines.Finance.Events;

/// <summary>Domain events emitted across the payroll run lifecycle. Each is a MediatR notification; any
/// module can subscribe with an INotificationHandler without the payroll engine knowing about it.</summary>
public sealed record PayrollRunCreatedEvent(Guid RunId, string RunNumber, Guid TenantId) : INotification;

public sealed record PayrollCalculatedEvent(Guid RunId, int EmployeeCount, decimal NetTotal) : INotification;

public sealed record PayrollValidatedEvent(Guid RunId, bool IsValid, int Errors, int Warnings) : INotification;

public sealed record PayrollApprovedEvent(Guid RunId, string RunNumber, Guid? ApprovedByUserId) : INotification;

public sealed record PayrollExecutionStartedEvent(Guid RunId, int ItemCount) : INotification;

/// <summary>Raised after one employee's payslip has been posted to the financial ledger.</summary>
public sealed record PayslipPostedEvent(Guid RunId, Guid EmployeeId, Guid PayslipId, decimal NetAmount, int LedgerEntries) : INotification;

public sealed record PayrollCompletedEvent(Guid RunId, string RunNumber, int Completed, int Failed, decimal NetTotal) : INotification;

public sealed record PayrollExecutionFailedEvent(Guid RunId, int Completed, int Failed) : INotification;
