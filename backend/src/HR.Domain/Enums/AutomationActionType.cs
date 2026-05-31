namespace HR.Domain.Enums;

public enum AutomationActionType
{
    SendNotification = 1,
    UpdateField = 2,
    CreateTask = 3,
    StartWorkflow = 4,
    SendEmail = 5,
    WebhookCall = 6,
    AssignRole = 7
}
