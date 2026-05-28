# Saudi HR Cloud — Design System v1.0
# OOP-Style Callable Design Objects

---

## 1. Design Tokens

### 1.1 Typography

```ts
const Typography = {
  fontFamily: {
    primary: "'Thmanyah Sans', sans-serif",
    mono: "'IBM Plex Mono', monospace",
  },
  fontWeight: {
    light: 300,
    regular: 400,
    medium: 500,
    bold: 700,
    black: 900,
  },
  fontSize: {
    xs: "0.75rem",     // 12px — captions, badges
    sm: "0.8125rem",   // 13px — secondary text, table cells
    base: "0.875rem",  // 14px — body text
    md: "1rem",        // 16px — input text, nav items
    lg: "1.125rem",    // 18px — subheadings
    xl: "1.375rem",    // 22px — section titles
    "2xl": "1.75rem",  // 28px — page titles
    "3xl": "2.25rem",  // 36px — hero / dashboard KPIs
    "4xl": "3rem",     // 48px — display numbers
  },
  lineHeight: {
    tight: 1.2,
    base: 1.5,
    relaxed: 1.7,
  },
  letterSpacing: {
    tight: "-0.02em",
    normal: "0em",
    wide: "0.02em",
  },
} as const;
```

### 1.2 Color Palettes (6 Themes)

```ts
// Theme 1: Midnight Corporate
const PaletteMidnight = {
  id: "midnight",
  label: "Midnight Corporate",
  bg:        { primary: "#0B0F1A", secondary: "#111827", tertiary: "#1F2937" },
  surface:   { card: "#161E2E", elevated: "#1C2640", overlay: "rgba(0,0,0,0.6)" },
  border:    { default: "#2D3A4F", subtle: "#1E293B", focus: "#6366F1" },
  text:      { primary: "#F1F5F9", secondary: "#94A3B8", muted: "#64748B", inverse: "#0B0F1A" },
  accent:    { primary: "#6366F1", primaryHover: "#818CF8", secondary: "#10B981" },
  semantic:  { success: "#22C55E", warning: "#F59E0B", error: "#EF4444", info: "#3B82F6" },
  chart:     ["#6366F1", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899"],
};

// Theme 2: Desert Light
const PaletteDesert = {
  id: "desert",
  label: "Desert Light",
  bg:        { primary: "#FFFDF7", secondary: "#FEF9EF", tertiary: "#FDF2E0" },
  surface:   { card: "#FFFFFF", elevated: "#FFFEF9", overlay: "rgba(120,80,30,0.08)" },
  border:    { default: "#E8DCC8", subtle: "#F0E8D8", focus: "#C2956B" },
  text:      { primary: "#2C1810", secondary: "#6B4C3B", muted: "#9B8272", inverse: "#FFFDF7" },
  accent:    { primary: "#C2956B", primaryHover: "#A67B52", secondary: "#2D6A4F" },
  semantic:  { success: "#2D6A4F", warning: "#E09F3E", error: "#C1121F", info: "#457B9D" },
  chart:     ["#C2956B", "#2D6A4F", "#E09F3E", "#457B9D", "#9B2226", "#6B705C"],
};

// Theme 3: Royal Indigo
const PaletteRoyal = {
  id: "royal",
  label: "Royal Indigo",
  bg:        { primary: "#FAF8FF", secondary: "#F3EFFE", tertiary: "#EDE5FD" },
  surface:   { card: "#FFFFFF", elevated: "#FDFBFF", overlay: "rgba(79,55,139,0.08)" },
  border:    { default: "#DDD4EE", subtle: "#EDE5FD", focus: "#7C3AED" },
  text:      { primary: "#1A0B2E", secondary: "#4F378B", muted: "#8B7AAF", inverse: "#FAF8FF" },
  accent:    { primary: "#7C3AED", primaryHover: "#6D28D9", secondary: "#0EA5E9" },
  semantic:  { success: "#059669", warning: "#D97706", error: "#DC2626", info: "#0EA5E9" },
  chart:     ["#7C3AED", "#0EA5E9", "#D97706", "#059669", "#EC4899", "#F97316"],
};

// Theme 4: Emerald Enterprise
const PaletteEmerald = {
  id: "emerald",
  label: "Emerald Enterprise",
  bg:        { primary: "#F7FBF9", secondary: "#EFF6F2", tertiary: "#E0EDE6" },
  surface:   { card: "#FFFFFF", elevated: "#F9FCFA", overlay: "rgba(5,96,69,0.06)" },
  border:    { default: "#C6DDD0", subtle: "#E0EDE6", focus: "#047857" },
  text:      { primary: "#0A1F14", secondary: "#1E4D38", muted: "#5E8B74", inverse: "#F7FBF9" },
  accent:    { primary: "#047857", primaryHover: "#065F46", secondary: "#2563EB" },
  semantic:  { success: "#15803D", warning: "#CA8A04", error: "#B91C1C", info: "#2563EB" },
  chart:     ["#047857", "#2563EB", "#CA8A04", "#B91C1C", "#7C3AED", "#0891B2"],
};

// Theme 5: Arctic Minimal
const PaletteArctic = {
  id: "arctic",
  label: "Arctic Minimal",
  bg:        { primary: "#FFFFFF", secondary: "#FAFBFC", tertiary: "#F3F4F6" },
  surface:   { card: "#FFFFFF", elevated: "#FFFFFF", overlay: "rgba(0,0,0,0.04)" },
  border:    { default: "#E5E7EB", subtle: "#F3F4F6", focus: "#111827" },
  text:      { primary: "#111827", secondary: "#4B5563", muted: "#9CA3AF", inverse: "#FFFFFF" },
  accent:    { primary: "#111827", primaryHover: "#1F2937", secondary: "#6366F1" },
  semantic:  { success: "#16A34A", warning: "#EAB308", error: "#EF4444", info: "#3B82F6" },
  chart:     ["#111827", "#6366F1", "#EAB308", "#16A34A", "#EC4899", "#F97316"],
};

// Theme 6: Slate Industrial
const PaletteSlate = {
  id: "slate",
  label: "Slate Industrial",
  bg:        { primary: "#18181B", secondary: "#1C1C20", tertiary: "#27272A" },
  surface:   { card: "#212125", elevated: "#2C2C31", overlay: "rgba(0,0,0,0.5)" },
  border:    { default: "#3F3F46", subtle: "#2C2C31", focus: "#FBBF24" },
  text:      { primary: "#FAFAFA", secondary: "#A1A1AA", muted: "#71717A", inverse: "#18181B" },
  accent:    { primary: "#FBBF24", primaryHover: "#F59E0B", secondary: "#38BDF8" },
  semantic:  { success: "#4ADE80", warning: "#FBBF24", error: "#F87171", info: "#38BDF8" },
  chart:     ["#FBBF24", "#38BDF8", "#4ADE80", "#F87171", "#A78BFA", "#FB923C"],
};
```

### 1.3 Spacing Scale

```ts
const Spacing = {
  0: "0px",
  1: "4px",
  2: "8px",
  3: "12px",
  4: "16px",
  5: "20px",
  6: "24px",
  8: "32px",
  10: "40px",
  12: "48px",
  16: "64px",
  20: "80px",
  24: "96px",
} as const;
```

### 1.4 Radius

```ts
const Radius = {
  none: "0px",
  sm: "4px",
  md: "8px",
  lg: "12px",
  xl: "16px",
  "2xl": "20px",
  full: "9999px",
} as const;
```

### 1.5 Shadows

```ts
const Shadow = {
  none: "none",
  xs: "0 1px 2px rgba(0,0,0,0.04)",
  sm: "0 2px 4px rgba(0,0,0,0.06)",
  md: "0 4px 12px rgba(0,0,0,0.08)",
  lg: "0 8px 24px rgba(0,0,0,0.10)",
  xl: "0 16px 48px rgba(0,0,0,0.12)",
  inner: "inset 0 2px 4px rgba(0,0,0,0.04)",
} as const;
```

### 1.6 Motion

```ts
const Motion = {
  duration: {
    instant: "100ms",
    fast: "150ms",
    normal: "250ms",
    slow: "400ms",
    page: "500ms",
  },
  easing: {
    default: "cubic-bezier(0.4, 0, 0.2, 1)",
    in: "cubic-bezier(0.4, 0, 1, 1)",
    out: "cubic-bezier(0, 0, 0.2, 1)",
    bounce: "cubic-bezier(0.34, 1.56, 0.64, 1)",
  },
} as const;
```

### 1.7 Z-Index

```ts
const ZIndex = {
  base: 0,
  dropdown: 10,
  sticky: 20,
  overlay: 30,
  modal: 40,
  toast: 50,
  tooltip: 60,
} as const;
```

### 1.8 Breakpoints

```ts
const Breakpoints = {
  sm: "640px",
  md: "768px",
  lg: "1024px",
  xl: "1280px",
  "2xl": "1536px",
} as const;
```

---

## 2. Component Objects

### 2.1 Button

```ts
const Button = {
  base: "inline-flex items-center justify-center font-bold transition-all focus-visible:outline-2 focus-visible:outline-offset-2 disabled:opacity-50 disabled:pointer-events-none",
  variant: {
    primary:   "bg-accent-primary text-inverse hover:bg-accent-primaryHover",
    secondary: "bg-surface-card text-primary border border-default hover:bg-tertiary",
    ghost:     "bg-transparent text-secondary hover:bg-tertiary",
    danger:    "bg-semantic-error text-inverse hover:opacity-90",
    link:      "bg-transparent text-accent-primary underline-offset-4 hover:underline p-0",
  },
  size: {
    sm: { h: "32px", px: "12px", fontSize: "xs", radius: "md", iconSize: "14px" },
    md: { h: "40px", px: "16px", fontSize: "sm", radius: "md", iconSize: "16px" },
    lg: { h: "48px", px: "24px", fontSize: "base", radius: "lg", iconSize: "18px" },
    xl: { h: "56px", px: "32px", fontSize: "md", radius: "lg", iconSize: "20px" },
  },
  iconOnly: {
    sm: "32x32",
    md: "40x40",
    lg: "48x48",
  },
};
```

### 2.2 Input

```ts
const Input = {
  base: "w-full bg-surface-card border border-default text-primary placeholder:text-muted transition-colors focus:border-focus focus:ring-1 focus:ring-focus",
  size: {
    sm: { h: "32px", px: "10px", fontSize: "xs", radius: "md" },
    md: { h: "40px", px: "12px", fontSize: "sm", radius: "md" },
    lg: { h: "48px", px: "16px", fontSize: "base", radius: "lg" },
  },
  state: {
    error: "border-semantic-error focus:ring-semantic-error",
    disabled: "opacity-50 cursor-not-allowed bg-tertiary",
  },
  withIcon: { paddingInlineStart: "40px" },
  withAction: { paddingInlineEnd: "40px" },
};
```

### 2.3 Card

```ts
const Card = {
  base: "bg-surface-card border border-default overflow-hidden",
  variant: {
    flat:     { shadow: "none", radius: "lg" },
    raised:   { shadow: "sm", radius: "lg" },
    elevated: { shadow: "md", radius: "xl" },
    outlined: { shadow: "none", radius: "lg", borderWidth: "2px" },
  },
  padding: {
    compact: "12px",
    default: "20px",
    spacious: "28px",
  },
  header: "flex items-center justify-between pb-4 border-b border-subtle",
  body: "py-4",
  footer: "flex items-center justify-end gap-3 pt-4 border-t border-subtle",
};
```

### 2.4 Badge

```ts
const Badge = {
  base: "inline-flex items-center font-bold whitespace-nowrap",
  variant: {
    solid:   "text-inverse",
    soft:    "bg-opacity-10",
    outline: "bg-transparent border",
    dot:     "bg-transparent pl-3 before:absolute before:start-0 before:top-1/2 before:-translate-y-1/2 before:w-2 before:h-2 before:rounded-full relative",
  },
  size: {
    sm: { h: "20px", px: "6px", fontSize: "10px", radius: "full" },
    md: { h: "24px", px: "8px", fontSize: "xs", radius: "full" },
    lg: { h: "28px", px: "10px", fontSize: "sm", radius: "full" },
  },
  semantic: {
    success: "success",
    warning: "warning",
    error: "error",
    info: "info",
    neutral: "muted",
  },
};
```

### 2.5 Table

```ts
const Table = {
  wrapper: "w-full overflow-x-auto",
  table: "w-full border-collapse",
  thead: "bg-tertiary sticky top-0 z-sticky",
  th: "text-start text-xs font-bold text-muted uppercase tracking-wide px-4 py-3 border-b border-default whitespace-nowrap",
  td: "px-4 py-3 text-sm text-primary border-b border-subtle",
  tr: {
    base: "transition-colors",
    hover: "hover:bg-tertiary/50",
    selected: "bg-accent-primary/5",
    striped: "even:bg-tertiary/30",
  },
  empty: "text-center text-muted py-16",
  pagination: {
    wrapper: "flex items-center justify-between px-4 py-3",
    info: "text-sm text-muted",
    controls: "flex items-center gap-1",
  },
};
```

### 2.6 Modal / Dialog

```ts
const Modal = {
  overlay: "fixed inset-0 bg-overlay z-modal flex items-center justify-center p-4",
  container: "bg-surface-card border border-default shadow-xl w-full max-h-[85vh] overflow-y-auto",
  size: {
    sm: { maxWidth: "400px", radius: "xl" },
    md: { maxWidth: "560px", radius: "xl" },
    lg: { maxWidth: "720px", radius: "xl" },
    xl: { maxWidth: "960px", radius: "xl" },
    full: { maxWidth: "calc(100vw - 64px)", radius: "xl" },
  },
  header: "flex items-center justify-between px-6 py-4 border-b border-default",
  body: "px-6 py-5",
  footer: "flex items-center justify-end gap-3 px-6 py-4 border-t border-default",
};
```

### 2.7 Sidebar / Navigation

```ts
const Sidebar = {
  container: "fixed inset-y-0 end-0 flex flex-col bg-secondary border-s border-default z-sticky",
  width: {
    collapsed: "64px",
    expanded: "260px",
  },
  logo: "flex items-center justify-center h-16 border-b border-default",
  nav: "flex-1 overflow-y-auto py-4",
  navItem: {
    base: "flex items-center gap-3 px-4 py-2.5 text-sm text-secondary rounded-lg mx-2 transition-colors",
    active: "bg-accent-primary/10 text-accent-primary font-bold",
    hover: "hover:bg-tertiary",
  },
  navGroup: {
    label: "text-xs font-bold text-muted uppercase tracking-wide px-4 py-2 mt-4 mb-1",
  },
  footer: "border-t border-default p-4",
};
```

### 2.8 Topbar

```ts
const Topbar = {
  container: "sticky top-0 z-sticky flex items-center justify-between h-16 px-6 bg-primary border-b border-default",
  start: "flex items-center gap-4",
  center: "flex-1 max-w-md mx-4",
  end: "flex items-center gap-3",
  search: {
    wrapper: "relative w-full",
    input: "w-full h-9 ps-10 pe-4 bg-tertiary border-none rounded-lg text-sm placeholder:text-muted",
    icon: "absolute start-3 top-1/2 -translate-y-1/2 text-muted",
  },
  avatar: "w-8 h-8 rounded-full bg-accent-primary text-inverse flex items-center justify-center text-xs font-bold",
  notification: "relative w-9 h-9 flex items-center justify-center rounded-lg hover:bg-tertiary transition-colors",
  notificationDot: "absolute top-1 end-1 w-2 h-2 rounded-full bg-semantic-error",
};
```

### 2.9 KPI / Stat Card

```ts
const StatCard = {
  base: "flex flex-col gap-2 p-5 bg-surface-card border border-default rounded-xl",
  label: "text-xs font-medium text-muted uppercase tracking-wide",
  value: "text-3xl font-black text-primary leading-tight",
  delta: {
    wrapper: "flex items-center gap-1 text-xs font-bold",
    positive: "text-semantic-success",
    negative: "text-semantic-error",
    neutral: "text-muted",
  },
  icon: "w-10 h-10 rounded-lg flex items-center justify-center bg-accent-primary/10 text-accent-primary",
  sparkline: "h-10 w-24",
};
```

### 2.10 Avatar

```ts
const Avatar = {
  base: "inline-flex items-center justify-center rounded-full font-bold bg-accent-primary text-inverse overflow-hidden",
  size: {
    xs: { wh: "24px", fontSize: "10px" },
    sm: { wh: "32px", fontSize: "xs" },
    md: { wh: "40px", fontSize: "sm" },
    lg: { wh: "56px", fontSize: "lg" },
    xl: { wh: "80px", fontSize: "2xl" },
  },
  group: "flex -space-x-2 rtl:space-x-reverse",
  status: {
    online: "bg-semantic-success",
    offline: "bg-muted",
    busy: "bg-semantic-error",
    away: "bg-semantic-warning",
  },
};
```

### 2.11 Toast / Notification

```ts
const Toast = {
  container: "fixed bottom-6 start-6 z-toast flex flex-col gap-2 max-w-sm",
  base: "flex items-start gap-3 p-4 bg-surface-elevated border border-default rounded-xl shadow-lg",
  icon: "w-5 h-5 mt-0.5 shrink-0",
  content: "flex-1 min-w-0",
  title: "text-sm font-bold text-primary",
  description: "text-xs text-secondary mt-0.5",
  close: "shrink-0 w-6 h-6 flex items-center justify-center rounded hover:bg-tertiary text-muted",
  variants: {
    success: "border-l-4 border-l-semantic-success",
    error: "border-l-4 border-l-semantic-error",
    warning: "border-l-4 border-l-semantic-warning",
    info: "border-l-4 border-l-semantic-info",
  },
};
```

### 2.12 Form Group

```ts
const FormGroup = {
  wrapper: "flex flex-col gap-1.5",
  label: "text-sm font-bold text-primary",
  required: "text-semantic-error ms-0.5",
  hint: "text-xs text-muted",
  error: "text-xs text-semantic-error flex items-center gap-1",
  fieldset: "grid gap-5",
  fieldsetRow: "grid grid-cols-2 gap-4",
};
```

### 2.13 Dropdown / Select

```ts
const Dropdown = {
  trigger: "...Input.base with chevron icon",
  content: "bg-surface-elevated border border-default rounded-lg shadow-lg overflow-hidden py-1 z-dropdown min-w-[180px]",
  item: {
    base: "flex items-center gap-2 px-3 py-2 text-sm text-primary cursor-pointer transition-colors",
    hover: "bg-tertiary",
    active: "bg-accent-primary/10 text-accent-primary font-bold",
    disabled: "opacity-50 cursor-not-allowed",
  },
  separator: "h-px bg-border-default my-1",
  group: {
    label: "text-xs font-bold text-muted uppercase px-3 py-1.5",
  },
};
```

### 2.14 Tabs

```ts
const Tabs = {
  list: "flex items-center gap-0 border-b border-default",
  trigger: {
    base: "px-4 py-2.5 text-sm font-medium text-muted border-b-2 border-transparent transition-colors -mb-px",
    active: "text-accent-primary border-accent-primary font-bold",
    hover: "hover:text-primary hover:border-subtle",
  },
  content: "py-5",
  variant: {
    underline: "border-b",
    pills: "bg-tertiary rounded-lg p-1 border-none gap-1",
    pillTrigger: "rounded-md px-3 py-1.5",
    pillActive: "bg-surface-card shadow-sm text-primary",
  },
};
```

### 2.15 Empty State

```ts
const EmptyState = {
  wrapper: "flex flex-col items-center justify-center py-16 text-center",
  icon: "w-16 h-16 text-muted mb-4",
  title: "text-lg font-bold text-primary mb-1",
  description: "text-sm text-muted max-w-sm mb-6",
  action: "...Button.primary.md",
};
```

### 2.16 Stepper / Timeline

```ts
const Stepper = {
  wrapper: "flex flex-col gap-0",
  step: {
    wrapper: "flex items-start gap-3",
    indicator: {
      base: "w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold shrink-0",
      pending: "bg-tertiary text-muted border border-default",
      active: "bg-accent-primary text-inverse",
      completed: "bg-semantic-success text-inverse",
    },
    connector: "w-0.5 bg-border-default flex-1 ms-[15px] my-1 min-h-[24px]",
    content: "pb-6",
    title: "text-sm font-bold text-primary",
    description: "text-xs text-muted mt-0.5",
  },
};
```

---

## 3. Layout Objects

### 3.1 App Shell

```ts
const AppShell = {
  root: "flex min-h-screen bg-primary text-primary font-primary direction-rtl",
  sidebar: "...Sidebar",
  main: {
    wrapper: "flex-1 flex flex-col me-[var(--sidebar-width)]",
    topbar: "...Topbar",
    content: "flex-1 p-6",
  },
};
```

### 3.2 Page Layout

```ts
const PageLayout = {
  header: {
    wrapper: "flex items-center justify-between mb-6",
    title: "text-2xl font-black text-primary",
    subtitle: "text-sm text-secondary mt-1",
    actions: "flex items-center gap-3",
    breadcrumb: "flex items-center gap-2 text-xs text-muted mb-3",
    breadcrumbSeparator: "text-border-default",
  },
  body: "flex flex-col gap-6",
  section: {
    title: "text-lg font-bold text-primary mb-4",
  },
};
```

### 3.3 Grid System

```ts
const Grid = {
  stats: "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4",
  cards: "grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5",
  form: "grid grid-cols-1 md:grid-cols-2 gap-5",
  formWide: "grid grid-cols-1 md:grid-cols-3 gap-5",
  dashboard: "grid grid-cols-12 gap-5",
  twoThirds: "col-span-8",
  oneThird: "col-span-4",
  half: "col-span-6",
  full: "col-span-12",
};
```

### 3.4 Content Width

```ts
const ContentWidth = {
  narrow: "max-w-2xl",     // forms, settings
  default: "max-w-6xl",    // standard pages
  wide: "max-w-7xl",       // dashboards
  full: "max-w-full",      // data tables
};
```

---

## 4. Pattern Objects (HR-Specific Composites)

### 4.1 Employee Profile Header

```ts
const EmployeeProfileHeader = {
  wrapper: "flex items-start gap-6 p-6 bg-surface-card border border-default rounded-xl",
  avatar: "...Avatar.xl",
  info: "flex-1",
  name: "text-2xl font-black text-primary",
  position: "text-sm text-secondary mt-0.5",
  department: "...Badge.soft.md",
  meta: "flex items-center gap-6 mt-3 text-xs text-muted",
  metaItem: "flex items-center gap-1.5",
  actions: "flex items-center gap-2",
  quickStats: "grid grid-cols-4 gap-4 mt-4 pt-4 border-t border-subtle",
};
```

### 4.2 Request Card (ESS)

```ts
const RequestCard = {
  wrapper: "p-4 bg-surface-card border border-default rounded-xl hover:shadow-sm transition-shadow",
  header: "flex items-center justify-between mb-3",
  type: "flex items-center gap-2",
  typeIcon: "w-8 h-8 rounded-lg flex items-center justify-center bg-accent-primary/10 text-accent-primary",
  typeLabel: "text-sm font-bold text-primary",
  status: "...Badge.soft.sm",
  body: "text-sm text-secondary",
  meta: "flex items-center justify-between mt-3 pt-3 border-t border-subtle text-xs text-muted",
  actions: "flex items-center gap-2",
};
```

### 4.3 Payroll Summary Row

```ts
const PayrollRow = {
  wrapper: "flex items-center gap-4 p-4 border-b border-subtle last:border-b-0",
  employee: "flex items-center gap-3 flex-1 min-w-0",
  avatar: "...Avatar.sm",
  name: "text-sm font-bold text-primary truncate",
  department: "text-xs text-muted",
  columns: "grid grid-cols-4 gap-4 flex-1 text-end",
  amount: "text-sm font-bold text-primary tabular-nums",
  label: "text-xs text-muted",
  status: "...Badge",
  netPay: "text-lg font-black text-primary",
};
```

### 4.4 Attendance Timeline

```ts
const AttendanceTimeline = {
  wrapper: "relative",
  day: {
    wrapper: "flex items-center gap-4 py-3 border-b border-subtle",
    date: "w-20 shrink-0",
    dateDay: "text-sm font-bold text-primary",
    dateLabel: "text-xs text-muted",
    bar: "flex-1 h-8 bg-tertiary rounded-full overflow-hidden relative",
    barFill: "absolute inset-y-0 start-[var(--start)] bg-accent-primary/20 rounded-full",
    times: "flex items-center gap-4 text-xs text-secondary w-40",
    checkIn: "text-semantic-success",
    checkOut: "text-semantic-error",
    hours: "font-bold text-primary",
    status: "...Badge.dot.sm",
  },
};
```

### 4.5 Workflow Builder Node

```ts
const WorkflowNode = {
  wrapper: "p-4 bg-surface-card border-2 border-default rounded-xl min-w-[200px] cursor-move",
  selected: "border-accent-primary shadow-md",
  header: "flex items-center gap-2 mb-2",
  icon: "w-6 h-6 rounded bg-accent-primary/10 text-accent-primary flex items-center justify-center",
  title: "text-sm font-bold text-primary",
  body: "text-xs text-muted",
  handle: {
    base: "w-3 h-3 rounded-full border-2 border-accent-primary bg-surface-card absolute",
    top: "top-0 start-1/2 -translate-x-1/2 -translate-y-1/2",
    bottom: "bottom-0 start-1/2 -translate-x-1/2 translate-y-1/2",
  },
  connector: "stroke-border-default stroke-2 fill-none",
};
```

### 4.6 Document Template Card

```ts
const DocumentCard = {
  wrapper: "group p-5 bg-surface-card border border-default rounded-xl hover:shadow-md transition-all cursor-pointer",
  preview: "w-full h-32 bg-tertiary rounded-lg mb-3 flex items-center justify-center text-muted",
  title: "text-sm font-bold text-primary group-hover:text-accent-primary transition-colors",
  meta: "text-xs text-muted mt-1",
  actions: "flex items-center gap-2 mt-3 opacity-0 group-hover:opacity-100 transition-opacity",
};
```

### 4.7 Recruitment Pipeline Column

```ts
const PipelineColumn = {
  wrapper: "flex flex-col bg-tertiary/50 rounded-xl min-w-[280px] max-h-[calc(100vh-200px)]",
  header: "flex items-center justify-between p-3 border-b border-subtle",
  title: "text-sm font-bold text-primary",
  count: "w-6 h-6 rounded-full bg-surface-card text-xs font-bold text-muted flex items-center justify-center",
  body: "flex-1 overflow-y-auto p-2 space-y-2",
  card: {
    wrapper: "p-3 bg-surface-card border border-default rounded-lg shadow-xs cursor-grab active:cursor-grabbing",
    name: "text-sm font-bold text-primary",
    position: "text-xs text-muted mt-0.5",
    tags: "flex items-center gap-1 mt-2 flex-wrap",
    tag: "...Badge.soft.sm",
    footer: "flex items-center justify-between mt-2 pt-2 border-t border-subtle",
    avatar: "...Avatar.xs",
    date: "text-xs text-muted",
  },
};
```

### 4.8 Report Chart Container

```ts
const ChartContainer = {
  wrapper: "bg-surface-card border border-default rounded-xl overflow-hidden",
  header: "flex items-center justify-between px-5 py-4 border-b border-subtle",
  title: "text-sm font-bold text-primary",
  controls: "flex items-center gap-2",
  body: "p-5",
  legend: "flex items-center gap-4 px-5 pb-4 text-xs text-muted",
  legendItem: "flex items-center gap-1.5",
  legendDot: "w-2 h-2 rounded-full",
};
```

---

## 5. RTL & Internationalization

```ts
const RTL = {
  direction: "rtl",
  fontFeatureSettings: "'ss01'",
  rules: {
    useLogicalProperties: true,
    marginInlineStart: "ms-*",
    marginInlineEnd: "me-*",
    paddingInlineStart: "ps-*",
    paddingInlineEnd: "pe-*",
    insetInlineStart: "start-*",
    insetInlineEnd: "end-*",
    borderInlineStart: "border-s-*",
    borderInlineEnd: "border-e-*",
    textAlign: "text-start | text-end",
    flexDirection: "auto-flipped by Tailwind rtl:",
    iconMirroring: ["chevron-left", "chevron-right", "arrow-left", "arrow-right"],
  },
  numberFormatting: {
    locale: "ar-SA",
    currency: "SAR",
    useWesternNumerals: true,
  },
  calendarSupport: ["hijri", "gregorian"],
};
```

---

## 6. Iconography

```ts
const Icons = {
  library: "lucide-react",
  size: {
    xs: 14,
    sm: 16,
    md: 18,
    lg: 20,
    xl: 24,
  },
  strokeWidth: 1.75,
  moduleIcons: {
    dashboard: "LayoutDashboard",
    employees: "Users",
    attendance: "Clock",
    payroll: "Banknote",
    expenses: "Receipt",
    loans: "HandCoins",
    requests: "FileText",
    recruitment: "UserPlus",
    reports: "BarChart3",
    documents: "FolderOpen",
    settings: "Settings",
    notifications: "Bell",
  },
};
```

---

## 7. Font Face Declaration

```css
@font-face {
  font-family: 'Thmanyah Sans';
  src: url('/fonts/thmanyahsans-Light.otf') format('opentype');
  font-weight: 300;
  font-style: normal;
  font-display: swap;
}
@font-face {
  font-family: 'Thmanyah Sans';
  src: url('/fonts/thmanyahsans-Regular.otf') format('opentype');
  font-weight: 400;
  font-style: normal;
  font-display: swap;
}
@font-face {
  font-family: 'Thmanyah Sans';
  src: url('/fonts/thmanyahsans-Medium.otf') format('opentype');
  font-weight: 500;
  font-style: normal;
  font-display: swap;
}
@font-face {
  font-family: 'Thmanyah Sans';
  src: url('/fonts/thmanyahsans-Bold.otf') format('opentype');
  font-weight: 700;
  font-style: normal;
  font-display: swap;
}
@font-face {
  font-family: 'Thmanyah Sans';
  src: url('/fonts/thmanyahsans-Black.otf') format('opentype');
  font-weight: 900;
  font-style: normal;
  font-display: swap;
}
```

---

## 8. Theme Application Map

| Prototype | Palette          | Layout Density | Card Style | Nav Pattern       | Radius | Shadow |
|-----------|------------------|----------------|------------|-------------------|--------|--------|
| 1         | Midnight         | Dense          | Flat       | Sidebar collapsed | sm     | none   |
| 2         | Desert Light     | Airy           | Raised     | Sidebar expanded  | xl     | md     |
| 3         | Royal Indigo     | Balanced       | Elevated   | Sidebar + tabs    | lg     | lg     |
| 4         | Emerald Enterprise| Compact       | Outlined   | Top nav + sidebar | md     | sm     |
| 5         | Arctic Minimal   | Spacious       | Flat       | Minimal sidebar   | sm     | xs     |
| 6         | Slate Industrial | Dense          | Flat       | Sidebar collapsed | none   | none   |
