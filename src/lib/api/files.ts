import { API_BASE_URL } from "../api-client";
import { getAccessToken } from "../auth-storage";

export interface FileUploadResult {
  id: string;
  url: string;        // relative, e.g. /api/files/{id}
  fileName: string;
  contentType: string;
  sizeBytes: number;
}

// Upload a file via multipart. apiFetch can't be used here (it forces JSON), so we
// build the request directly while reusing the same auth + envelope conventions.
export async function uploadFile(file: File, category?: string): Promise<FileUploadResult> {
  const fd = new FormData();
  fd.append("file", file);
  const token = getAccessToken();
  const q = category ? `?category=${encodeURIComponent(category)}` : "";
  const res = await fetch(`${API_BASE_URL}/api/files${q}`, {
    method: "POST",
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: fd,
  });
  const text = await res.text();
  let env: { success?: boolean; data?: FileUploadResult; message?: string } | null = null;
  try { env = JSON.parse(text); } catch { /* ignore */ }
  if (!res.ok || (env && env.success === false)) {
    throw new Error(env?.message || "تعذر رفع الملف");
  }
  return env!.data as FileUploadResult;
}

// Resolve a stored relative file url (/api/files/{id}) to an absolute, displayable URL.
export function fileUrl(url?: string | null): string | undefined {
  if (!url) return undefined;
  if (/^https?:\/\//i.test(url)) return url;
  return `${API_BASE_URL}${url}`;
}
