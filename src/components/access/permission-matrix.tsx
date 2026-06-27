"use client";

import { useMemo } from "react";
import { ACTION_AR, MODULE_AR, type PermissionCatalogModule } from "@/lib/api/access";

/// A HubSpot-style permission grid: one row per module, one column per distinct action.
/// A cell renders a checkbox only where that module actually defines the action, so the grid
/// stays accurate as the catalog grows. `value` is the set of granted permission codes.
export function PermissionMatrix({
  catalog,
  value,
  onChange,
  disabled = false,
}: {
  catalog: PermissionCatalogModule[];
  value: Set<string>;
  onChange: (next: Set<string>) => void;
  disabled?: boolean;
}) {
  // Distinct action suffixes (the part after the last dot) → columns, in a sensible order.
  const columns = useMemo(() => {
    const order = ["View", "Create", "Edit", "Delete", "Approve", "Reject", "Export", "Run", "Lock",
      "Assign", "Cancel", "Terminate", "ViewSettlement", "Generate",
      "ManageUsers", "ManageRoles", "ManageTemplates", "ViewAudit",
      "ViewUsers", "CreateUsers", "EditUsers", "DeleteUsers", "ViewRoles", "CreateRoles", "EditRoles", "DeleteRoles"];
    const present = new Set<string>();
    for (const m of catalog) for (const p of m.permissions) present.add(action(p.code));
    const ordered = order.filter((a) => present.has(a));
    const extras = [...present].filter((a) => !order.includes(a)).sort();
    return [...ordered, ...extras];
  }, [catalog]);

  const codeFor = (module: string, act: string) =>
    catalog.find((m) => m.module === module)?.permissions.find((p) => action(p.code) === act)?.code;

  function toggle(code: string) {
    const next = new Set(value);
    if (next.has(code)) next.delete(code); else next.add(code);
    onChange(next);
  }

  function toggleRow(module: string, on: boolean) {
    const next = new Set(value);
    const codes = catalog.find((m) => m.module === module)?.permissions.map((p) => p.code) ?? [];
    for (const c of codes) { if (on) next.add(c); else next.delete(c); }
    onChange(next);
  }

  return (
    <div className="overflow-x-auto border border-border">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-secondary">
            <th className="sticky right-0 z-10 bg-secondary p-3 text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">
              الوحدة
            </th>
            {columns.map((c) => (
              <th key={c} className="p-2 text-center text-[11px] font-bold text-muted-foreground whitespace-nowrap">
                {ACTION_AR[c] ?? c}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {catalog.map((m) => {
            const rowCodes = m.permissions.map((p) => p.code);
            const allOn = rowCodes.length > 0 && rowCodes.every((c) => value.has(c));
            return (
              <tr key={m.module} className="border-t border-border hover:bg-card/50">
                <td className="sticky right-0 z-10 bg-background p-3 text-right">
                  <button
                    type="button"
                    disabled={disabled}
                    onClick={() => toggleRow(m.module, !allOn)}
                    className="font-medium hover:text-primary disabled:cursor-not-allowed"
                    title="تحديد/إلغاء الكل"
                  >
                    {MODULE_AR[m.module] ?? m.module}
                  </button>
                </td>
                {columns.map((act) => {
                  const code = codeFor(m.module, act);
                  return (
                    <td key={act} className="p-2 text-center">
                      {code ? (
                        <input
                          type="checkbox"
                          className="h-4 w-4 accent-[var(--primary)] cursor-pointer disabled:cursor-not-allowed"
                          checked={value.has(code)}
                          disabled={disabled}
                          onChange={() => toggle(code)}
                          aria-label={code}
                        />
                      ) : (
                        <span className="text-border">—</span>
                      )}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

function action(code: string): string {
  const i = code.lastIndexOf(".");
  return i >= 0 ? code.slice(i + 1) : code;
}
