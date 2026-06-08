import { apiFetch } from "../api-client";
import { setSession, clearSession, AuthUser } from "../auth-storage";

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: AuthUser;
}

export async function login(email: string, password: string): Promise<AuthUser> {
  const data = await apiFetch<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: { email, password },
  });
  setSession(data.accessToken, data.refreshToken, data.user);
  return data.user;
}

export function logout(): void {
  clearSession();
  if (typeof window !== "undefined") window.location.href = "/login";
}
