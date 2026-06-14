import { apiFetch } from "../api-client";

export interface NotificationRule {
  id: string;
  name: string;
  event: string;
  daysBefore: number;
  documentType?: string | null;
  notifyEmployee: boolean;
  notifyDirectManager: boolean;
  notifyDepartmentManager: boolean;
  extraEmployeeId?: string | null;
  roleId?: string | null;
  channelBell: boolean;
  channelEmail: boolean;
  channelSms: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface NotificationRuleInput {
  name: string;
  event: string;
  daysBefore: number;
  documentType?: string | null;
  notifyEmployee: boolean;
  notifyDirectManager: boolean;
  notifyDepartmentManager: boolean;
  extraEmployeeId?: string | null;
  roleId?: string | null;
  channelBell: boolean;
  channelEmail: boolean;
  channelSms: boolean;
  isActive: boolean;
}

export function getNotificationRules(): Promise<NotificationRule[]> {
  return apiFetch<NotificationRule[]>("/api/notifications/rules");
}

export function createNotificationRule(input: NotificationRuleInput): Promise<NotificationRule> {
  return apiFetch<NotificationRule>("/api/notifications/rules", { method: "POST", body: input });
}

export function updateNotificationRule(id: string, input: NotificationRuleInput): Promise<NotificationRule> {
  return apiFetch<NotificationRule>(`/api/notifications/rules/${id}`, { method: "PUT", body: input });
}

export function deleteNotificationRule(id: string): Promise<unknown> {
  return apiFetch<unknown>(`/api/notifications/rules/${id}`, { method: "DELETE" });
}

// Runs the document-expiry scan now (also runs automatically in the background). Returns count created.
export function runDocumentExpiryScan(): Promise<number> {
  return apiFetch<number>("/api/notifications/rules/run", { method: "POST" });
}

export interface RoleLite { id: string; name: string; nameAr: string }
export function getRoles(): Promise<RoleLite[]> {
  return apiFetch<RoleLite[]>("/api/roles");
}
