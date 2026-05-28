"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { EmployeeProfile } from "@/components/employees/employee-profile";
import { getEmployeeById } from "@/lib/mock-data";

export default function EmployeeProfilePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const employee = getEmployeeById(id);

  if (!employee) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <p className="text-lg text-muted-foreground mb-4">الموظف غير موجود</p>
        <Link href="/employees" className="inline-flex items-center justify-center h-8 px-4 border border-border bg-background text-sm font-medium hover:bg-muted transition-colors">
          العودة للموظفين
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <Link href="/employees" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" />
          الموظفين
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>{employee.name}</span>
      </div>

      <EmployeeProfile employee={employee} />
    </div>
  );
}
