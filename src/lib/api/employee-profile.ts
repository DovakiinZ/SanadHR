// Employee profile data — requests + activity timeline. Leave balances reuse request-center;
// compensation is computed client-side from the rich employee record.
import { apiFetch } from "../api-client";
import { RequestInstance } from "./request-center";

export interface TimelineEvent {
  id: string;
  category: string;
  action: string;
  descriptionAr?: string | null;
  descriptionEn?: string | null;
  actorName?: string | null;
  occurredAt: string;
}

export const getEmployeeRequests = (employeeId: string) =>
  apiFetch<RequestInstance[]>(`/api/requests/by-employee/${employeeId}`);

export const getEmployeeTimeline = (employeeId: string) =>
  apiFetch<TimelineEvent[]>(`/api/requests/by-employee/${employeeId}/timeline`);
