"use client";

import { use, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Loader2, Pencil } from "lucide-react";
import { toast } from "sonner";
import { EmployeeProfile } from "@/components/employees/employee-profile";
import { getEmployee } from "@/lib/api/employees";
import { ApiError } from "@/lib/api-client";
import { Employee } from "@/types";

export default function EmployeeProfilePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let active = true;
    (async () => {
      setLoading(true);
      setNotFound(false);
      try {
        const data = await getEmployee(id);
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

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
        <Loader2 className="h-6 w-6 animate-spin mb-3" />
        <p className="text-sm">جاري التحميل...</p>
      </div>
    );
  }

  if (notFound || !employee) {
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
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/employees" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
            <ArrowRight className="h-4 w-4" />
            الموظفين
          </Link>
          <span className="text-muted-foreground">/</span>
          <span>{employee.name}</span>
        </div>
        <Link
          href={`/employees/${id}/edit`}
          className="inline-flex items-center gap-2 h-9 px-4 border border-border bg-background text-sm font-medium hover:bg-muted transition-colors"
        >
          <Pencil className="h-4 w-4" />
          تعديل
        </Link>
      </div>

      <EmployeeProfile employee={employee} />
    </div>
  );
}
