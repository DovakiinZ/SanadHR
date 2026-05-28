"use client";

import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { EmployeeForm } from "@/components/employees/employee-form";

export default function NewEmployeePage() {
  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <Link href="/employees" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" />
          الموظفين
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>إضافة موظف جديد</span>
      </div>

      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-bold">إضافة موظف جديد</h1>
        <p className="text-sm text-muted-foreground mt-1">إدخال بيانات الموظف الجديد</p>
      </div>

      <EmployeeForm />
    </div>
  );
}
