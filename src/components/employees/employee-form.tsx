"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { createEmployee, updateEmployee, EmployeeInput } from "@/lib/api/employees";
import { getLookup, lookupLabel, LookupItem } from "@/lib/api/lookups";
import { getDepartments, getBranches, orgLabel, OrgOption } from "@/lib/api/org";
import { ApiError } from "@/lib/api-client";
import { Employee } from "@/types";

const employeeSchema = z.object({
  employeeNumber: z.string().optional(),
  name: z.string().min(2, "الاسم مطلوب"),
  nationalId: z.string().min(10, "رقم الهوية يجب أن يكون 10 أرقام").max(10),
  phone: z.string().min(10, "رقم الجوال مطلوب"),
  email: z.string().email("البريد الإلكتروني غير صالح"),
  dateOfBirth: z.string().min(1, "تاريخ الميلاد مطلوب"),
  gender: z.enum(["ذكر", "أنثى"], { message: "الجنس مطلوب" }),
  nationalityId: z.string().min(1, "الجنسية مطلوبة"),
  jobTitleId: z.string().min(1, "المسمى الوظيفي مطلوب"),
  contractTypeId: z.string().min(1, "نوع العقد مطلوب"),
  departmentId: z.string().optional(),
  branchId: z.string().optional(),
  joinDate: z.string().min(1, "تاريخ الانضمام مطلوب"),
  salary: z.string().min(1, "الراتب مطلوب"),
  status: z.string().optional(),
});

type EmployeeFormData = z.infer<typeof employeeSchema>;

const STATUS_OPTIONS = ["نشط", "في إجازة", "موقوف", "منتهي", "مستقيل"];

interface EmployeeFormProps {
  mode?: "create" | "edit";
  employeeId?: string;
  initial?: Employee;
}

export function EmployeeForm({ mode = "create", employeeId, initial }: EmployeeFormProps) {
  const router = useRouter();
  const isEdit = mode === "edit";

  const [jobTitles, setJobTitles] = useState<LookupItem[]>([]);
  const [nationalities, setNationalities] = useState<LookupItem[]>([]);
  const [contractTypes, setContractTypes] = useState<LookupItem[]>([]);
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [optionsLoading, setOptionsLoading] = useState(true);

  useEffect(() => {
    let active = true;
    (async () => {
      try {
        const [jt, nat, ct, deps, brs] = await Promise.all([
          getLookup("job-titles"),
          getLookup("nationalities"),
          getLookup("contract-types"),
          getDepartments(),
          getBranches(),
        ]);
        if (!active) return;
        setJobTitles(jt);
        setNationalities(nat);
        setContractTypes(ct);
        setDepartments(deps);
        setBranches(brs);
      } catch (err) {
        if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
          toast.error("تعذر تحميل قوائم الاختيار");
        }
      } finally {
        if (active) setOptionsLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, []);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<EmployeeFormData>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(employeeSchema) as any,
    defaultValues: initial
      ? {
          employeeNumber: initial.employeeId,
          name: initial.name,
          nationalId: initial.nationalId,
          phone: initial.phone,
          email: initial.email,
          dateOfBirth: initial.dateOfBirth,
          gender: initial.gender,
          nationalityId: initial.nationalityId ?? "",
          jobTitleId: initial.jobTitleId ?? "",
          contractTypeId: initial.contractTypeId ?? "",
          departmentId: initial.departmentId ?? "",
          branchId: initial.branchId ?? "",
          joinDate: initial.joinDate,
          salary: String(initial.salary),
          status: initial.status || "نشط",
        }
      : { status: "نشط" },
  });

  const onSubmit = async (data: EmployeeFormData) => {
    const input: EmployeeInput = {
      employeeNumber: data.employeeNumber,
      name: data.name,
      nationalId: data.nationalId,
      phone: data.phone,
      email: data.email,
      dateOfBirth: data.dateOfBirth,
      gender: data.gender,
      nationalityId: data.nationalityId,
      jobTitleId: data.jobTitleId,
      contractTypeId: data.contractTypeId,
      departmentId: data.departmentId,
      branchId: data.branchId,
      joinDate: data.joinDate,
      salary: Number(data.salary),
      status: data.status,
    };
    try {
      if (isEdit && employeeId) {
        await updateEmployee(employeeId, input);
        toast.success("تم تحديث بيانات الموظف");
      } else {
        await createEmployee(input);
        toast.success("تمت إضافة الموظف بنجاح");
      }
      router.push("/employees");
      router.refresh();
    } catch (err) {
      if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
        toast.error(err instanceof ApiError ? err.message : "تعذر حفظ بيانات الموظف");
      }
    }
  };

  const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground disabled:opacity-60";

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      {/* Personal Info */}
      <div className="border border-border bg-card p-6">
        <h3 className="text-sm font-bold uppercase tracking-wider text-muted-foreground mb-5">البيانات الشخصية</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الاسم الكامل</Label>
            <Input {...register("name")} className="bg-secondary border-border" />
            {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">رقم الهوية</Label>
            <Input {...register("nationalId")} className="bg-secondary border-border" />
            {errors.nationalId && <p className="text-xs text-destructive">{errors.nationalId.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">رقم الجوال</Label>
            <Input {...register("phone")} className="bg-secondary border-border" />
            {errors.phone && <p className="text-xs text-destructive">{errors.phone.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">البريد الإلكتروني</Label>
            <Input type="email" {...register("email")} className="bg-secondary border-border" />
            {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الميلاد</Label>
            <Input type="date" {...register("dateOfBirth")} className="bg-secondary border-border" />
            {errors.dateOfBirth && <p className="text-xs text-destructive">{errors.dateOfBirth.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الجنس</Label>
            <select {...register("gender")} className={selectClass}>
              <option value="">اختر</option>
              <option value="ذكر">ذكر</option>
              <option value="أنثى">أنثى</option>
            </select>
            {errors.gender && <p className="text-xs text-destructive">{errors.gender.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الجنسية</Label>
            <select {...register("nationalityId")} disabled={optionsLoading} className={selectClass}>
              <option value="">{optionsLoading ? "جاري التحميل..." : "اختر الجنسية"}</option>
              {nationalities.map((o) => (
                <option key={o.id} value={o.id}>{lookupLabel(o)}</option>
              ))}
            </select>
            {errors.nationalityId && <p className="text-xs text-destructive">{errors.nationalityId.message}</p>}
          </div>
        </div>
      </div>

      {/* Employment Info */}
      <div className="border border-border bg-card p-6">
        <h3 className="text-sm font-bold uppercase tracking-wider text-muted-foreground mb-5">بيانات التوظيف</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الرقم الوظيفي</Label>
            <Input
              {...register("employeeNumber")}
              disabled={isEdit}
              placeholder={isEdit ? undefined : "تلقائي إن تُرك فارغاً"}
              className="bg-secondary border-border disabled:opacity-60"
            />
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">المسمى الوظيفي</Label>
            <select {...register("jobTitleId")} disabled={optionsLoading} className={selectClass}>
              <option value="">{optionsLoading ? "جاري التحميل..." : "اختر المسمى"}</option>
              {jobTitles.map((o) => (
                <option key={o.id} value={o.id}>{lookupLabel(o)}</option>
              ))}
            </select>
            {errors.jobTitleId && <p className="text-xs text-destructive">{errors.jobTitleId.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">القسم</Label>
            <select {...register("departmentId")} disabled={optionsLoading} className={selectClass}>
              <option value="">{optionsLoading ? "جاري التحميل..." : "اختر القسم"}</option>
              {departments.map((o) => (
                <option key={o.id} value={o.id}>{orgLabel(o)}</option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الفرع</Label>
            <select {...register("branchId")} disabled={optionsLoading} className={selectClass}>
              <option value="">{optionsLoading ? "جاري التحميل..." : "اختر الفرع"}</option>
              {branches.map((o) => (
                <option key={o.id} value={o.id}>{orgLabel(o)}</option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الانضمام</Label>
            <Input type="date" {...register("joinDate")} className="bg-secondary border-border" />
            {errors.joinDate && <p className="text-xs text-destructive">{errors.joinDate.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">نوع العقد</Label>
            <select {...register("contractTypeId")} disabled={optionsLoading} className={selectClass}>
              <option value="">{optionsLoading ? "جاري التحميل..." : "اختر نوع العقد"}</option>
              {contractTypes.map((o) => (
                <option key={o.id} value={o.id}>{lookupLabel(o)}</option>
              ))}
            </select>
            {errors.contractTypeId && <p className="text-xs text-destructive">{errors.contractTypeId.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الراتب الشهري (ر.س)</Label>
            <Input type="number" {...register("salary")} className="bg-secondary border-border" />
            {errors.salary && <p className="text-xs text-destructive">{errors.salary.message}</p>}
          </div>
          {isEdit && (
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الحالة</Label>
              <select {...register("status")} className={selectClass}>
                {STATUS_OPTIONS.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-3">
        <Button type="submit" disabled={isSubmitting} className="h-10 px-8 font-bold uppercase tracking-wider">
          {isSubmitting ? "جاري الحفظ..." : isEdit ? "حفظ التعديلات" : "حفظ الموظف"}
        </Button>
        <Button type="button" variant="outline" onClick={() => router.back()} className="h-10 px-8">
          إلغاء
        </Button>
      </div>
    </form>
  );
}
