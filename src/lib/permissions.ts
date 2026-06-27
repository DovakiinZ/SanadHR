"use client";

import { useEffect, useState } from "react";
import { getAccessToken } from "./auth-storage";

// Decode a JWT payload (base64url) without verifying — we only read claims the
// server already issued. Permission strings are ASCII (e.g. "Employees.View").
function decodePayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split(".")[1];
    if (!part) return null;
    const b64 = part.replace(/-/g, "+").replace(/_/g, "/");
    const json = atob(b64);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function getPermissions(): string[] {
  const token = getAccessToken();
  if (!token) return [];
  const payload = decodePayload(token);
  if (!payload) return [];
  const claim = payload["permission"] ?? payload["permissions"];
  if (Array.isArray(claim)) return claim.map(String);
  if (typeof claim === "string") return [claim];
  return [];
}

export function usePermissions() {
  const [perms, setPerms] = useState<string[]>([]);
  const [ready, setReady] = useState(false);
  // Read the token after mount (client-only). Deferred to a microtask so the effect body
  // itself doesn't call setState synchronously.
  useEffect(() => { queueMicrotask(() => { setPerms(getPermissions()); setReady(true); }); }, []);
  const has = (p: string) => perms.includes(p);
  const hasAny = (...ps: string[]) => ps.some((x) => perms.includes(x));
  return { perms, has, hasAny, loaded: perms.length > 0, ready };
}

/// Convenience single-permission hook. `ready` flips true after the token is read
/// (use it to avoid flashing "access denied" before permissions have loaded).
export function usePermission(permission: string): { allowed: boolean; ready: boolean } {
  const { has, ready } = usePermissions();
  return { allowed: has(permission), ready };
}
