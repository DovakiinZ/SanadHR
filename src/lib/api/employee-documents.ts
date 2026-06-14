import { apiFetch } from "../api-client";

// Personnel documents attached to an employee (ID/Iqama/passport/contract/…).
// Files are uploaded first via uploadFile() (POST /api/files); the returned URL
// is stored here together with type, expiry and notes.

export interface EmployeeDocument {
  id: string;
  employeeId: string;
  type: string;
  title: string;
  documentNumber?: string | null;
  issueDate?: string | null;
  expiryDate?: string | null;
  notes?: string | null;
  fileUrl: string;
  fileName?: string | null;
  contentType?: string | null;
  sizeBytes: number;
  createdAt: string;
}

export interface EmployeeDocumentInput {
  type: string;
  title: string;
  documentNumber?: string | null;
  issueDate?: string | null;
  expiryDate?: string | null;
  notes?: string | null;
  fileUrl: string;
  fileName?: string | null;
  contentType?: string | null;
  sizeBytes: number;
}

// Stable type codes shared with the backend + their Arabic labels.
export const DOCUMENT_TYPES: { code: string; labelAr: string }[] = [
  { code: "Id", labelAr: "الهوية الوطنية" },
  { code: "Iqama", labelAr: "الإقامة" },
  { code: "Passport", labelAr: "جواز السفر" },
  { code: "Contract", labelAr: "عقد العمل" },
  { code: "Certificate", labelAr: "شهادة" },
  { code: "MedicalReport", labelAr: "تقرير طبي" },
  { code: "Custom", labelAr: "مستند آخر" },
];

export function documentTypeLabel(code: string): string {
  return DOCUMENT_TYPES.find((t) => t.code === code)?.labelAr ?? code;
}

export function getEmployeeDocuments(employeeId: string): Promise<EmployeeDocument[]> {
  return apiFetch<EmployeeDocument[]>(`/api/employees/${employeeId}/documents`);
}

export function createEmployeeDocument(employeeId: string, input: EmployeeDocumentInput): Promise<EmployeeDocument> {
  return apiFetch<EmployeeDocument>(`/api/employees/${employeeId}/documents`, { method: "POST", body: input });
}

export function updateEmployeeDocument(employeeId: string, id: string, input: EmployeeDocumentInput): Promise<EmployeeDocument> {
  return apiFetch<EmployeeDocument>(`/api/employees/${employeeId}/documents/${id}`, { method: "PUT", body: input });
}

export function deleteEmployeeDocument(employeeId: string, id: string): Promise<unknown> {
  return apiFetch<unknown>(`/api/employees/${employeeId}/documents/${id}`, { method: "DELETE" });
}
