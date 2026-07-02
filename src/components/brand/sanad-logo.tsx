import { cn } from "@/lib/utils";

/** Minimal single-stroke palm — the Sanad mark. Uses currentColor so it adapts to its container. */
export function PalmMark({ size = 26, className }: { size?: number; className?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 32 32" fill="none" aria-hidden className={className}>
      <path d="M16 30V15" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
      <path
        d="M16 15C16 15 14.5 8.5 8 7.5M16 15C16 15 17.5 8.5 24 7.5M16 15C16 15 11 10 5.5 12.5M16 15C16 15 21 10 26.5 12.5M16 15C16 15 16 8 16 4"
        stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round"
      />
      <circle cx="16" cy="14.5" r="1.6" fill="currentColor" />
    </svg>
  );
}

/** The "سند" wordmark in the brand serif (globally loaded in globals.css). */
export function SanadWordmark({ className, style }: { className?: string; style?: React.CSSProperties }) {
  return (
    <span className={cn("leading-none", className)}
      style={{ fontFamily: '"Thmanyah Serif Display", serif', fontWeight: 700, ...style }}>
      سند
    </span>
  );
}

/** Full Sanad logo: mark + wordmark. `markClassName` colors the mark (default brand primary). */
export function SanadLogo({
  size = 28, className, markClassName = "text-primary",
}: { size?: number; className?: string; markClassName?: string }) {
  return (
    <span className={cn("inline-flex items-center gap-2.5 text-foreground", className)}>
      <PalmMark size={size} className={markClassName} />
      <SanadWordmark style={{ fontSize: size }} />
    </span>
  );
}
