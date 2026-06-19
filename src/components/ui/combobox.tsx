"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Check, ChevronsUpDown, Search, X } from "lucide-react";
import { cn } from "@/lib/utils";

export interface ComboboxOption {
  value: string;
  label: string;
  hint?: string;
}

/**
 * Searchable single-select. Replaces bare <select> for entity pickers (employees, roles,
 * departments…). Filters client-side over the provided options; the parent loads the data.
 */
export function Combobox({
  value, onChange, options, placeholder = "اختر…", searchPlaceholder = "بحث…",
  disabled, allowClear = true, className, emptyText = "لا نتائج",
}: {
  value: string | null;
  onChange: (value: string | null) => void;
  options: ComboboxOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  disabled?: boolean;
  allowClear?: boolean;
  className?: string;
  emptyText?: string;
}) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const ref = useRef<HTMLDivElement>(null);

  const selected = options.find((o) => o.value === value) ?? null;

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return options.slice(0, 100);
    return options.filter((o) => o.label.toLowerCase().includes(q) || o.hint?.toLowerCase().includes(q)).slice(0, 100);
  }, [options, query]);

  useEffect(() => {
    if (!open) return;
    const onDoc = (e: MouseEvent) => { if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false); };
    document.addEventListener("mousedown", onDoc);
    return () => document.removeEventListener("mousedown", onDoc);
  }, [open]);

  return (
    <div ref={ref} className={cn("relative", className)}>
      <button
        type="button"
        disabled={disabled}
        onClick={() => setOpen((o) => !o)}
        className="flex h-9 w-full items-center justify-between gap-2 rounded-lg border border-input bg-secondary px-3 text-sm outline-none transition-colors hover:bg-muted focus-visible:border-ring disabled:opacity-50"
      >
        <span className={cn("truncate", !selected && "text-muted-foreground")}>{selected?.label ?? placeholder}</span>
        <span className="flex items-center gap-1 text-muted-foreground">
          {allowClear && selected && !disabled && (
            <span role="button" tabIndex={-1} aria-label="مسح" onClick={(e) => { e.stopPropagation(); onChange(null); }}
              className="rounded p-0.5 hover:text-foreground"><X className="h-3.5 w-3.5" /></span>
          )}
          <ChevronsUpDown className="h-3.5 w-3.5" />
        </span>
      </button>

      {open && (
        <div className="absolute z-50 mt-1 w-full overflow-hidden rounded-lg border border-border bg-popover shadow-lg">
          <div className="flex items-center gap-2 border-b border-border px-2.5 py-1.5">
            <Search className="h-3.5 w-3.5 text-muted-foreground" />
            <input
              autoFocus value={query} onChange={(e) => setQuery(e.target.value)} placeholder={searchPlaceholder}
              className="h-6 w-full bg-transparent text-sm outline-none placeholder:text-muted-foreground"
            />
          </div>
          <div className="max-h-60 overflow-auto py-1">
            {filtered.length === 0 ? (
              <div className="px-3 py-4 text-center text-xs text-muted-foreground">{emptyText}</div>
            ) : filtered.map((o) => (
              <button
                key={o.value}
                type="button"
                onClick={() => { onChange(o.value); setOpen(false); setQuery(""); }}
                className="flex w-full items-center gap-2 px-3 py-1.5 text-start text-sm hover:bg-muted"
              >
                <Check className={cn("h-3.5 w-3.5 shrink-0", o.value === value ? "opacity-100 text-primary" : "opacity-0")} />
                <span className="truncate">{o.label}</span>
                {o.hint && <span className="ms-auto truncate text-xs text-muted-foreground">{o.hint}</span>}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
