import { redirect } from "next/navigation";

// Document Branding merged into the unified Company & Organization module (Branding tab).
export default function DocumentBrandingRedirect() {
  redirect("/settings/company-organization");
}
