import { redirect } from "next/navigation";

// Company Settings merged into the unified Company & Organization module.
export default function CompanyRedirect() {
  redirect("/settings/company-organization");
}
