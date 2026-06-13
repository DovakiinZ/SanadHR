// In-app (bell) notifications API.
import { apiFetch } from "../api-client";

export interface AppNotification {
  id: string;
  titleAr: string;
  titleEn: string;
  bodyAr?: string | null;
  bodyEn?: string | null;
  category?: string | null;
  link?: string | null;
  entityId?: string | null;
  isRead: boolean;
  createdAt: string;
}

export const getNotifications = (unreadOnly = false) =>
  apiFetch<AppNotification[]>(`/api/notifications${unreadOnly ? "?unreadOnly=true" : ""}`);

export const getUnreadCount = () =>
  apiFetch<number>("/api/notifications/unread-count");

export const markNotificationRead = (id: string) =>
  apiFetch<unknown>(`/api/notifications/${id}/read`, { method: "POST" });

export const markAllNotificationsRead = () =>
  apiFetch<unknown>("/api/notifications/read-all", { method: "POST" });
