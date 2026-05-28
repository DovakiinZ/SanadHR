"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { departments } from "@/lib/mock-data";

const employeeSchema = z.object({
  name: z.string().min(2, "الاسم مطلوب"),
  nationalId: z.string().min(10, "رقم الهوية يجب أن يكون 10 أرقام").max(10),
  phone: z.string().min(10, "رقم الجوال مطلوب"),
  email: z.string().email("البريد الإلكتروني غير صالح"),
  dateOfBirth: z.string().min(1, "تاريخ الميلاد مطلوب"),
  gender: z.enum(["ذكر", "أنثى"], { message: "الجنس مطلوب" }),
  nationality: z.string().min(1, "الجنسية مطلوبة"),
  department: z.string().min(1, "القسم مطلوب"),
  position: z.string().min(1, "المسمى الوظيفي مطلوب"),
  joinDate: z.string().min(1, "تاريخ الانضمام مطلوب"),
  contractType: z.enum(["دوام كامل", "دوام جزئي", "عقد مؤقت"], {
    message: "نوع العقد مطلوب",
  }),
  salary: z.string().min(1, "الراتب مطلوب").transform(Number),
});

type EmployeeFormData = z.infer<typeof employeeSchema>;

export function EmployeeForm() {
  const router = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  } = useForm<EmployeeFormData>({
    resolver: zodResolver(employeeSchema) as any,
  });

  const onSubmit = (data: EmployeeFormData) => {
    // Mock: store in localStorage
    const existing = JSON.parse(localStorage.getItem("hr_new_employees") || "[]");
    existing.push({
      ...data,
      id: `new-${Date.now()}`,
      employeeId: `EMP-${String(24 + existing.length).padStart(3, "0")}`,
      status: "نشط",
      leaveBalance: 30,
      attendanceRate: 100,
    });
    localStorage.setItem("hr_new_employees", JSON.stringify(existing));
    router.push("/employees");
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
            <Label className="text-xs font-bold uppercase tracking-wider">القسم</Label>
            <select {...register("department")} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
              <option value="">اختر القسم</option>
              {departments.map((d) => (
                <option key={d.id} value={d.name}>{d.name}</option>
              ))}
            </select>
            {errors.department && <p className="text-xs text-destructive">{errors.department.message}</p>}
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
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-3">
        <Button type="submit" disabled={isSubmitting} className="h-10 px-8 font-bold uppercase tracking-wider">
          {isSubmitting ? "جاري الحفظ..." : "حفظ الموظف"}
        </Button>
        <Button type="button" variant="outline" onClick={() => router.back()} className="h-10 px-8">
          إلغاء
        </Button>
      </div>
    </form>
  );
}
