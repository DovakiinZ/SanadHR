"use client";

import Link from "next/link";
import { Plus } from "lucide-react";
import { EmployeeTable } from "@/components/employees/employee-table";
import { employees } from "@/lib/mock-data";

export default function EmployeesPage() {
  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">الموظفين</h1>
          <p className="text-sm text-muted-foreground mt-1">إدارة بيانات الموظفين</p>
        </div>
        <Link href="/employees/new" className="inline-flex items-center gap-2 h-10 px-4 bg-primary text-primary-foreground font-bold uppercase tracking-wider text-sm hover:bg-primary/80 transition-colors">
          <Plus className="h-4 w-4" />
          إضافة موظف
        </Link>
      </div>

      {/* Employee Table */}
      <EmployeeTable employees={employees} />
    </div>
  );
}
