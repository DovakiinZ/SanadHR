// Centralized auth/session storage (localStorage-backed).
// Single source of truth for the access token, refresh token and current user.

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
}

const ACCESS_TOKEN_KEY = "hr_access_token";
const REFRESH_TOKEN_KEY = "hr_refresh_token";
const USER_KEY = "hr_user";
// Legacy lightweight flag still read by the root redirect + dashboard guard.
const LEGACY_FLAG_KEY = "hr_auth";

export function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function getUser(): AuthUser | null {
  if (typeof window === "undefined") return null;
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}

export function setSession(accessToken: string, refreshToken: string, user: AuthUser): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
  // Keep the legacy flag in sync so existing guards keep working.
  localStorage.setItem(LEGACY_FLAG_KEY, JSON.stringify({ email: user.email, name: user.fullName }));
}

export function clearSession(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(LEGACY_FLAG_KEY);
}

export function isAuthenticated(): boolean {
  return !!getAccessToken();
}
