"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { login } from "@/lib/api/auth";
import { ApiError } from "@/lib/api-client";
import { SanadLogo } from "@/components/brand/sanad-logo";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    if (!email || !password) {
      setError("جميع الحقول مطلوبة");
      return;
    }
    setLoading(true);
    try {
      const user = await login(email, password);
      toast.success(`مرحباً ${user.fullName}`);
      router.push("/dashboard");
    } catch (err) {
      const message =
        err instanceof ApiError
          ? err.status === 404 || err.status === 403
            ? "البريد الإلكتروني أو كلمة المرور غير صحيحة"
            : err.message
          : "تعذر تسجيل الدخول";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="border border-border bg-card p-8">
      {/* Logo */}
      <div className="mb-8 text-center">
        <div className="mb-3 flex justify-center">
          <SanadLogo size={40} />
        </div>
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

        <Button type="submit" disabled={loading} className="w-full h-10 font-bold uppercase tracking-wider">
          {loading ? "جاري تسجيل الدخول..." : "تسجيل الدخول"}
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
