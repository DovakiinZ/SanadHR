"use client";

import { ShieldAlert, Loader2 } from "lucide-react";
import { usePermissions } from "@/lib/permissions";

/// Gates a whole page/section. If the user opens the route directly without the required
/// permission, they see a clean "access denied" panel instead of a broken page. Pass one
/// permission or several (any-of).
export function AccessGuard({ anyOf, children }: { anyOf: string | string[]; children: React.ReactNode }) {
  const { hasAny, ready } = usePermissions();
  const required = Array.isArray(anyOf) ? anyOf : [anyOf];

  if (!ready) {
    return (
      <div className="flex items-center justify-center py-24 text-muted-foreground">
        <Loader2 className="h-5 w-5 animate-spin" />
      </div>
    );
  }

  if (!hasAny(...required)) {
    return (
      <div className="mx-auto max-w-md py-24 text-center">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center bg-destructive/10 text-destructive">
          <ShieldAlert className="h-6 w-6" />
        </div>
        <h2 className="text-lg font-bold">لا تملك صلاحية الوصول</h2>
        <p className="mt-1 text-sm text-muted-foreground">
          هذه الصفحة تتطلب صلاحية لا تملكها. تواصل مع مدير النظام إذا كنت تعتقد أن هذا خطأ.
        </p>
      </div>
    );
  }

  return <>{children}</>;
}
