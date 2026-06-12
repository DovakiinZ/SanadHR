"use client";

import { type ElementType, useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Activity, ArrowRight, Banknote, LayoutTemplate, Loader2, Sparkles, Users } from "lucide-react";
import { toast } from "sonner";
import { usePermissions } from "@/lib/permissions";
import { getReadyTemplates, seedTemplate } from "@/lib/api/dashboards";
import { ReadyTemplate } from "@/types/dashboard";

const ICONS: Record<string, ElementType> = {
  Sparkles, Users, Banknote, Activity, LayoutTemplate,
};

export default function TemplatesPage() {
  const router = useRouter();
  const { has } = usePermissions();
  const canCreate = has("Platform.Dashboards.Create");

  const [templates, setTemplates] = useState<ReadyTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setTemplates(await getReadyTemplates()); }
    catch { setTemplates([]); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  const onUse = async (key: string) => {
    setBusy(key);
    try {
      await seedTemplate(key);
      toast.success("تم تطبيق القالب");
      router.push("/dashboard");
    } catch {
      toast.error("تعذر تطبيق القالب");
    } finally {
      setBusy(null);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">قوالب اللوحات</h1>
          <p className="mt-1 text-sm text-muted-foreground">ابدأ من قالب جاهز يُبنى تلقائياً من بياناتك</p>
        </div>
        <Link href="/dashboard" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted">
          <ArrowRight className="h-4 w-4" /> العودة للوحات
        </Link>
      </div>

      {loading ? (
        <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {templates.map((t) => {
            const Icon = ICONS[t.icon] ?? LayoutTemplate;
            return (
              <div key={t.key} className="flex flex-col border border-border bg-card p-5">
                <div className="mb-3 flex h-10 w-10 items-center justify-center bg-primary/10 text-primary"><Icon className="h-5 w-5" /></div>
                <h3 className="font-bold">{t.nameAr}</h3>
                <p className="mt-1 flex-1 text-sm text-muted-foreground">{t.description}</p>
                {canCreate && (
                  <button onClick={() => onUse(t.key)} disabled={busy !== null}
                    className="mt-4 inline-flex h-10 items-center justify-center gap-2 bg-primary px-4 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">
                    {busy === t.key ? <Loader2 className="h-4 w-4 animate-spin" /> : null} استخدام القالب
                  </button>
                )}
              </div>
            );
          })}
          {templates.length === 0 && <p className="text-sm text-muted-foreground">لا توجد قوالب متاحة</p>}
        </div>
      )}
    </div>
  );
}
