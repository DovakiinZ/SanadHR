"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { Download, Plus, RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { EmployeeTable } from "@/components/employees/employee-table";
import { ExportDialog } from "@/components/employees/export-dialog";
import { getEmployees } from "@/lib/api/employees";
import { ApiError } from "@/lib/api-client";
import { Employee } from "@/types";
import { usePermissions } from "@/lib/permissions";

export default function EmployeesPage() {
  const { has } = usePermissions();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [exporting, setExporting] = useState(false);

  const fetchEmployees = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getEmployees();
      setEmployees(data);
    } catch (err) {
      const message = err instanceof ApiError ? err.message : "تعذر تحميل بيانات الموظفين";
      // 401/403/500 already show a toast from the client; avoid duplicates for those.
      if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
        toast.error(message);
      }
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchEmployees();
  }, [fetchEmployees]);

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">الموظفين</h1>
          <p className="text-sm text-muted-foreground mt-1">إدارة بيانات الموظفين</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={fetchEmployees}
            disabled={loading}
            className="inline-flex items-center gap-2 h-10 px-3 border border-border bg-background text-sm hover:bg-muted transition-colors disabled:opacity-50"
            title="تحديث"
          >
            <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
          </button>
          {has("Employees.View") && (
            <button
              onClick={() => setExporting(true)}
              className="inline-flex items-center gap-2 h-10 px-3 border border-border bg-background text-sm hover:bg-muted transition-colors"
            >
              <Download className="h-4 w-4" /> تصدير Excel
            </button>
          )}
          {has("Employees.Create") && (
            <Link
              href="/employees/new"
              className="inline-flex items-center gap-2 h-10 px-4 bg-primary text-primary-foreground font-bold uppercase tracking-wider text-sm hover:bg-primary/80 transition-colors"
            >
              <Plus className="h-4 w-4" />
              إضافة موظف
            </Link>
          )}
        </div>
      </div>

      <ExportDialog open={exporting} onClose={() => setExporting(false)} />

      {error && !loading && (
        <div className="border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive flex items-center justify-between">
          <span>{error}</span>
          <button onClick={fetchEmployees} className="underline hover:no-underline">
            إعادة المحاولة
          </button>
        </div>
      )}

      {/* Employee Table */}
      <EmployeeTable employees={employees} loading={loading} onChanged={fetchEmployees} />
    </div>
  );
}
