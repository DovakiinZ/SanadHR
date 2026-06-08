"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { departments } from "@/lib/mock-data";
import { createEmployee, updateEmployee, EmployeeInput } from "@/lib/api/employees";
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
  nationality: z.string().min(1, "الجنسية مطلوبة"),
  department: z.string().optional(),
  position: z.string().min(1, "المسمى الوظيفي مطلوب"),
  joinDate: z.string().min(1, "تاريخ الانضمام مطلوب"),
  contractType: z.enum(["دوام كامل", "دوام جزئي", "عقد مؤقت"], {
    message: "نوع العقد مطلوب",
  }),
  salary: z.string().min(1, "الراتب مطلوب"),
  status: z.string().optional(),
});

type EmployeeFormData = z.infer<typeof employeeSchema>;

const STATUS_OPTIONS = ["نشط", "في إجازة", "موقوف", "منتهي", "مستقيل"];

function normalizeContract(value: string): "دوام كامل" | "دوام جزئي" | "عقد مؤقت" {
  if (value === "دوام كامل" || value === "دوام جزئي" || value === "عقد مؤقت") return value;
  if (value === "عقد" || value === "مؤقت") return "عقد مؤقت";
  return "دوام كامل";
}

interface EmployeeFormProps {
  mode?: "create" | "edit";
  employeeId?: string;
  initial?: Employee;
}

export function EmployeeForm({ mode = "create", employeeId, initial }: EmployeeFormProps) {
  const router = useRouter();
  const isEdit = mode === "edit";

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
          nationality: initial.nationality,
          department: "",
          position: initial.position === "—" ? "" : initial.position,
          joinDate: initial.joinDate,
          contractType: normalizeContract(initial.contractType),
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
      nationality: data.nationality,
      position: data.position,
      joinDate: data.joinDate,
      contractType: data.contractType,
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

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      {/* Personal Info */}
      <div className="border border-border bg-card p-6">
        <h3 className="text-sm font-bold uppercase tracking-wider text-muted-foreground mb-5">
          البيانات الشخصية
        </h3>
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
            <select {...register("gender")} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
              <option value="">اختر</option>
              <option value="ذكر">ذكر</option>
              <option value="أنثى">أنثى</option>
            </select>
            {errors.gender && <p className="text-xs text-destructive">{errors.gender.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الجنسية</Label>
            <Input {...register("nationality")} className="bg-secondary border-border" />
            {errors.nationality && <p className="text-xs text-destructive">{errors.nationality.message}</p>}
          </div>
        </div>
      </div>

      {/* Employment Info */}
      <div className="border border-border bg-card p-6">
        <h3 className="text-sm font-bold uppercase tracking-wider text-muted-foreground mb-5">
          بيانات التوظيف
        </h3>
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
            <Label className="text-xs font-bold uppercase tracking-wider">القسم</Label>
            <select {...register("department")} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
              <option value="">اختر القسم</option>
              {departments.map((d) => (
                <option key={d.id} value={d.name}>{d.name}</option>
              ))}
            </select>
            <p className="text-[10px] text-muted-foreground">لا يُحفظ بعد — وحدة الأقسام قريباً</p>
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">المسمى الوظيفي</Label>
            <Input {...register("position")} className="bg-secondary border-border" />
            {errors.position && <p className="text-xs text-destructive">{errors.position.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الانضمام</Label>
            <Input type="date" {...register("joinDate")} className="bg-secondary border-border" />
            {errors.joinDate && <p className="text-xs text-destructive">{errors.joinDate.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">نوع العقد</Label>
            <select {...register("contractType")} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
              <option value="">اختر نوع العقد</option>
              <option value="دوام كامل">دوام كامل</option>
              <option value="دوام جزئي">دوام جزئي</option>
              <option value="عقد مؤقت">عقد مؤقت</option>
            </select>
            {errors.contractType && <p className="text-xs text-destructive">{errors.contractType.message}</p>}
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الراتب الشهري (ر.س)</Label>
            <Input type="number" {...register("salary")} className="bg-secondary border-border" />
            {errors.salary && <p className="text-xs text-destructive">{errors.salary.message}</p>}
          </div>
          {isEdit && (
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الحالة</Label>
              <select {...register("status")} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
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
