// Company Settings — the single canonical company object (used by documents/reports/printing).
import { apiFetch } from "../api-client";

export interface CompanyProfile {
  id: string;
  nameEn: string;
  nameAr: string;
  logoUrl?: string | null;
  stampUrl?: string | null;
  commercialRegistration?: string | null;
  vatNumber?: string | null;
  website?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  city?: string | null;
  country?: string | null;
  postalCode?: string | null;
  defaultCurrency?: string | null;
  defaultLanguage?: string | null;
  timeZone?: string | null;
  fiscalYearStart?: string | null;
}

const EMPTY_ID = "00000000-0000-0000-0000-000000000000";

export const getCompanyProfile = () =>
  apiFetch<CompanyProfile | null>("/api/platform/company-config/profile");

export function saveCompanyProfile(p: Partial<CompanyProfile>): Promise<CompanyProfile> {
  return apiFetch<CompanyProfile>("/api/platform/company-config/profile", {
    method: "PUT",
    body: { ...p, id: p.id || EMPTY_ID },
  });
}
