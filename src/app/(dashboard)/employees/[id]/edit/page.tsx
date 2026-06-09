"use client";

import { use, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { EmployeeForm } from "@/components/employees/employee-form";
import { getEmployeeRaw, ApiEmployee } from "@/lib/api/employees";
import { ApiError } from "@/lib/api-client";

export default function EditEmployeePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const [employee, setEmployee] = useState<ApiEmployee | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let active = true;
    (async () => {
      setLoading(true);
      setNotFound(false);
      try {
        const data = await getEmployeeRaw(id);
        if (active) setEmployee(data);
      } catch (err) {
        if (!active) return;
        if (err instanceof ApiError && err.status === 404) {
          setNotFound(true);
        } else if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
          toast.error(err instanceof ApiError ? err.message : "تعذر تحميل بيانات الموظف");
          setNotFound(true);
        }
      } finally {
        if (active) setLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, [id]);

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <Link href="/employees" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" />
          الموظفين
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>{employee ? `تعديل ${employee.fullNameAr || employee.fullName}` : "تعديل موظف"}</span>
      </div>

      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-bold">تعديل بيانات الموظف</h1>
        <p className="text-sm text-muted-foreground mt-1">تحديث بيانات الموظف</p>
      </div>

      {loading ? (
        <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
          <Loader2 className="h-6 w-6 animate-spin mb-3" />
          <p className="text-sm">جاري التحميل...</p>
        </div>
      ) : notFound || !employee ? (
        <div className="flex flex-col items-center justify-center py-20">
          <p className="text-lg text-muted-foreground mb-4">الموظف غير موجود</p>
          <Link href="/employees" className="inline-flex items-center justify-center h-8 px-4 border border-border bg-background text-sm font-medium hover:bg-muted transition-colors">
            العودة للموظفين
          </Link>
        </div>
      ) : (
        <EmployeeForm mode="edit" employeeId={id} initial={employee ?? undefined} />
      )}
    </div>
  );
}
