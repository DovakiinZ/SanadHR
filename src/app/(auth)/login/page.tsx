"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      setError("جميع الحقول مطلوبة");
      return;
    }
    // Mock auth
    localStorage.setItem(
      "hr_auth",
      JSON.stringify({ email, name: "عبدالله المدير", role: "admin" })
    );
    router.push("/dashboard");
  };

  return (
    <div className="border border-border bg-card p-8">
      {/* Logo */}
      <div className="mb-8 text-center">
        <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center bg-primary">
          <span className="text-xl font-bold text-primary-foreground">HR</span>
        </div>
        <h1 className="text-xl font-bold">HR Cloud</h1>
        <p className="text-sm text-muted-foreground mt-1">نظام إدارة الموارد البشرية</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">
        {error && (
          <div className="border border-destructive/50 bg-destructive/10 px-4 py-2 text-sm text-destructive">
            {error}
          </div>
        )}

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">البريد الإلكتروني</Label>
          <Input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="admin@company.sa"
            className="bg-secondary border-border"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-xs font-bold uppercase tracking-wider">كلمة المرور</Label>
          <Input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            className="bg-secondary border-border"
          />
        </div>

        <Button type="submit" className="w-full h-10 font-bold uppercase tracking-wider">
          تسجيل الدخول
        </Button>
      </form>

      <div className="mt-6 text-center">
        <Link href="/register" className="text-sm text-muted-foreground hover:text-primary transition-colors">
          إنشاء حساب جديد
        </Link>
      </div>
    </div>
  );
}
