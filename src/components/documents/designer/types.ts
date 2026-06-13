// Visual document block model — the source of truth for a DocumentTemplate's LayoutJson.
// The backend renderer (DocumentRenderer.cs) reads the same shape (it ignores the client-only `id`).

export type BlockType =
  | "title" | "text" | "token" | "table" | "image" | "qr" | "signature" | "stamp" | "divider" | "spacer";

export interface TableRow { label: string; value: string }

export interface DocBlock {
  id: string;                 // client-only (for dnd + React keys); stripped on save
  type: BlockType;
  text?: string;
  token?: string;
  align?: "right" | "center" | "left";
  size?: "sm" | "md" | "lg" | "xl";
  bold?: boolean;
  fileId?: string;
  width?: number;
  height?: number;
  role?: "hr" | "ceo";
  label?: string;
  rows?: TableRow[];
}

export interface DocLayout { blocks: Omit<DocBlock, "id">[] }

let counter = 0;
export function uid(): string {
  counter += 1;
  return `b${Date.now().toString(36)}${counter}`;
}

export interface BlockDef {
  type: BlockType;
  label: string;       // Arabic palette label
  icon: string;        // lucide icon name (resolved in the component)
}

export const BLOCK_DEFS: BlockDef[] = [
  { type: "title", label: "عنوان", icon: "Heading" },
  { type: "text", label: "نص", icon: "Type" },
  { type: "token", label: "حقل رمز", icon: "Braces" },
  { type: "table", label: "جدول", icon: "Table" },
  { type: "image", label: "صورة", icon: "Image" },
  { type: "qr", label: "رمز QR", icon: "QrCode" },
  { type: "signature", label: "توقيع", icon: "PenLine" },
  { type: "stamp", label: "ختم", icon: "Stamp" },
  { type: "divider", label: "فاصل", icon: "Minus" },
  { type: "spacer", label: "مسافة", icon: "MoveVertical" },
];

export function newBlock(type: BlockType): DocBlock {
  const base: DocBlock = { id: uid(), type };
  switch (type) {
    case "title": return { ...base, text: "عنوان المستند", align: "center", size: "lg", bold: true };
    case "text": return { ...base, text: "اكتب النص هنا… يمكنك إدراج رموز مثل {{Employee.FullName}}", align: "right", size: "md" };
    case "token": return { ...base, token: "{{Employee.FullName}}", align: "right", size: "md" };
    case "table": return { ...base, rows: [{ label: "الحقل", value: "{{Employee.FullName}}" }] };
    case "image": return { ...base, align: "center", width: 160 };
    case "qr": return { ...base, align: "center", width: 90 };
    case "signature": return { ...base, role: "hr", label: "إدارة الموارد البشرية", width: 140 };
    case "stamp": return { ...base, align: "left", width: 100 };
    case "spacer": return { ...base, height: 16 };
    default: return base;
  }
}

export function serializeLayout(blocks: DocBlock[]): string {
  // strip the client-only id
  const out = { blocks: blocks.map(({ id, ...rest }) => rest) };
  return JSON.stringify(out);
}

export function parseLayout(json?: string | null): DocBlock[] {
  if (!json) return [];
  try {
    const parsed = JSON.parse(json) as DocLayout;
    return (parsed.blocks ?? []).map((b) => ({ ...b, id: uid() }));
  } catch {
    return [];
  }
}
