import {
  FileText, CalendarDays, Wallet, Plane, Receipt, Mail, Laptop, Wrench,
  UserCog, Banknote, ClipboardList, Stethoscope, GraduationCap, ShieldCheck,
  Clock, Home, Car, MessageSquareWarning, type LucideIcon,
} from "lucide-react";

// Curated icon set for Request Types. Stored as a stable string key on the
// MasterDataItem.Icon column and resolved here for display.
export const REQUEST_ICONS: Record<string, LucideIcon> = {
  "file-text": FileText,
  "calendar": CalendarDays,
  "wallet": Wallet,
  "plane": Plane,
  "receipt": Receipt,
  "mail": Mail,
  "laptop": Laptop,
  "wrench": Wrench,
  "user-cog": UserCog,
  "banknote": Banknote,
  "clipboard": ClipboardList,
  "stethoscope": Stethoscope,
  "graduation": GraduationCap,
  "shield": ShieldCheck,
  "clock": Clock,
  "home": Home,
  "car": Car,
  "complaint": MessageSquareWarning,
};

export const REQUEST_ICON_KEYS = Object.keys(REQUEST_ICONS);

export function requestIcon(key?: string | null): LucideIcon {
  return REQUEST_ICONS[key ?? ""] ?? FileText;
}

// A small palette of accent colours for request cards.
export const REQUEST_COLORS = [
  "#3b82f6", "#22c55e", "#f59e0b", "#ef4444", "#a855f7",
  "#06b6d4", "#ec4899", "#14b8a6", "#f97316", "#6366f1",
];
