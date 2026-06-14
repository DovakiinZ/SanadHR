import { redirect } from "next/navigation";

// Organization Settings merged into the unified Company & Organization module
// (sub-pages under /settings/organization/* remain). The "Organization" tab lists them.
export default function OrganizationRedirect() {
  redirect("/settings/company-organization");
}
