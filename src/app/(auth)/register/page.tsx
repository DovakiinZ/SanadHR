"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

export default function RegisterPage() {
  const router = useRouter();
  const [form, setForm] = useState({
    companyName: "",
    fullName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.companyName || !form.fullName || !form.email || !form.password) {
      setError("جميع الحقول مطلوبة");
      return;
    }
    if (form.password !== form.confirmPassword) {
      setError("كلمات المرور غير متطابقة");
      return;
    }
    // Mock auth
    localStorage.setItem(
      "hr_auth",
      JSON.stringify({ email: form.email, name: form.fullName, role: "admin" })
    );
    router.push("/dashboard");
  };

  const update = (field: string, value: string) =>
    setForm((prev) => ({ ...prev, [field]: value }));

  return (
    <div className="border border-border bg-card p-8">
      {/* Logo */}
      <div className="mb-8 text-center">
        <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center bg-primary">
          <span className="text-xl font-bold text-primary-foreground">HR</span>
        </div>
        <h1 className="text-xl font-bold">إنشاء حساب</h1>
        <p className="text-sm text-muted-foreground mt-1">ابدأ باستخدام HR Cloud</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="border border-destructive/50 bg-destructive/10 px-4 py-2 text-sm text-destructive">
            {error}
          </div>
        )}

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">اسم الشركة</Label>
          <Input
            value={form.companyName}
            onChange={(e) => update("companyName", e.target.value)}
            className="bg-secondary border-border"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">الاسم الكامل</Label>
          <Input
            value={form.fullName}
            onChange={(e) => update("fullName", e.target.value)}
            className="bg-secondary border-border"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">البريد الإلكتروني</Label>
          <Input
            type="email"
            value={form.email}
            onChange={(e) => update("email", e.target.value)}
            className="bg-secondary border-border"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">كلمة المرور</Label>
          <Input
            type="password"
            value={form.password}
            onChange={(e) => update("password", e.target.value)}
            className="bg-secondary border-border"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">تأكيد كلمة المرور</Label>
          <Input
            type="password"
            value={form.confirmPassword}
            onChange={(e) => update("confirmPassword", e.target.value)}
            className="bg-secondary border-border"
          />
        </div>

        <Button type="submit" className="w-full h-10 font-bold uppercase tracking-wider">
          إنشاء حساب
        </Button>
      </form>

      <div className="mt-6 text-center">
        <Link href="/login" className="text-sm text-muted-foreground hover:text-primary transition-colors">
          لديك حساب؟ تسجيل الدخول
        </Link>
      </div>
    </div>
  );
}
