// Centralized API client for the live Azure backend.
// - Reads NEXT_PUBLIC_API_URL (falls back to the deployed App Service URL).
// - Attaches the Bearer token from storage.
// - Unwraps the backend ApiResponse envelope: { success, data, message, errors }.
// - Centrally handles 401 (session expired → logout/redirect), 403 (forbidden) and 5xx (server)
//   with toast messages. Other failures (400/404/409) throw ApiError for the caller to surface.

import { toast } from "sonner";
import { getAccessToken, clearSession } from "./auth-storage";

export const API_BASE_URL =
  (process.env.NEXT_PUBLIC_API_URL?.replace(/\/+$/, "")) ||
  "https://hrcloud-api-v4xd.azurewebsites.net";

export class ApiError extends Error {
  status: number;
  errors?: string[];
  constructor(message: string, status: number, errors?: string[]) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}

interface ApiEnvelope<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[] | null;
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE" | "PATCH";
  body?: unknown;
  signal?: AbortSignal;
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, signal } = options;
  const token = getAccessToken();

  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal,
    });
  } catch (e) {
    if ((e as Error)?.name === "AbortError") throw e;
    toast.error("تعذر الاتصال بالخادم");
    throw new ApiError("Network error", 0);
  }

  const text = await res.text();
  let envelope: ApiEnvelope<T> | null = null;
  if (text) {
    try {
      envelope = JSON.parse(text) as ApiEnvelope<T>;
    } catch {
      envelope = null;
    }
  }

  if (res.status === 401) {
    clearSession();
    toast.error("انتهت الجلسة. يرجى تسجيل الدخول من جديد");
    if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
      window.location.href = "/login";
    }
    throw new ApiError(envelope?.message || "Unauthorized", 401);
  }

  if (res.status === 403) {
    toast.error("ليس لديك صلاحية لتنفيذ هذا الإجراء");
    throw new ApiError(envelope?.message || "Forbidden", 403, envelope?.errors ?? undefined);
  }

  if (res.status >= 500) {
    toast.error("حدث خطأ في الخادم. حاول مرة أخرى لاحقاً");
    throw new ApiError(envelope?.message || "Server error", res.status);
  }

  if (!res.ok || (envelope && !envelope.success)) {
    throw new ApiError(envelope?.message || "حدث خطأ غير متوقع", res.status, envelope?.errors ?? undefined);
  }

  return envelope ? (envelope.data as T) : (undefined as T);
}
