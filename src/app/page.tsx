"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

/* ────────────────────────────────────────────────────────────────────────────
   Sanad (سند) — Editorial landing page in the Thmanyah typographic tradition.
   Fully self-contained & RTL. Uses explicit warm-paper colors so it is immune
   to the dark dashboard theme applied on <html> by the root layout.
   Palette:  paper #FDFBF7 · charcoal #1A1A1A · terracotta #C25A3F / #8C3B24
             warm border #EFECE6 · muted ink #6B6358
   ──────────────────────────────────────────────────────────────────────────── */

const PAPER = "#FDFBF7";
const INK = "#1A1A1A";
const CLAY = "#C25A3F";
const CLAY_DEEP = "#8C3B24";
const LINE = "#EFECE6";
const MUTE = "#6B6358";

/* Subtle film-grain / paper texture rendered once and laid over everything. */
function PaperGrain() {
  return (
    <div
      aria-hidden
      className="pointer-events-none fixed inset-0 z-50 opacity-[0.035] mix-blend-multiply"
      style={{
        backgroundImage:
          "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='160' height='160'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E\")",
      }}
    />
  );
}

/* Minimal single-stroke palm — the mark beside the wordmark. */
function PalmMark({ size = 26, color = INK }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 32 32" fill="none" aria-hidden>
      <path
        d="M16 30V15"
        stroke={color}
        strokeWidth="1.4"
        strokeLinecap="round"
      />
      <path
        d="M16 15C16 15 14.5 8.5 8 7.5M16 15C16 15 17.5 8.5 24 7.5M16 15C16 15 11 10 5.5 12.5M16 15C16 15 21 10 26.5 12.5M16 15C16 15 16 8 16 4"
        stroke={color}
        strokeWidth="1.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <circle cx="16" cy="14.5" r="1.6" fill={color} />
    </svg>
  );
}

/* The hero centerpiece: a SIF / wage-protection row that reconciles itself,
   cycling red → reconciling → green, on a calm loop. */
function ReconciliationDemo() {
  const [phase, setPhase] = useState<0 | 1 | 2>(0); // 0 mismatch · 1 reconciling · 2 matched

  useEffect(() => {
    const seq: Array<[0 | 1 | 2, number]> = [
      [0, 2600],
      [1, 1500],
      [2, 3200],
    ];
    let i = 0;
    let t: ReturnType<typeof setTimeout>;
    const tick = () => {
      const [, hold] = seq[i];
      t = setTimeout(() => {
        i = (i + 1) % seq.length;
        setPhase(seq[i][0]);
        tick();
      }, hold);
    };
    tick();
    return () => clearTimeout(t);
  }, []);

  const matched = phase === 2;
  const reconciling = phase === 1;

  return (
    <div
      className="relative w-full select-none"
      style={{
        background: "#FFFFFF",
        border: `1px solid ${LINE}`,
        boxShadow: "0 1px 0 rgba(26,26,26,0.02), 0 30px 60px -40px rgba(26,26,26,0.25)",
      }}
    >
      {/* card header — a "file" tab */}
      <div
        className="flex items-center justify-between px-6 py-4"
        style={{ borderBottom: `1px solid ${LINE}` }}
      >
        <div className="flex items-center gap-2.5">
          <span
            className="inline-block h-2 w-2 rounded-full transition-colors duration-700"
            style={{ background: matched ? "#3F7A4F" : reconciling ? "#C9A227" : CLAY }}
          />
          <span className="text-[13px] tracking-wide" style={{ color: MUTE }}>
            ملف حماية الأجور · SIF
          </span>
        </div>
        <span className="font-mono text-[12px]" style={{ color: MUTE }}>
          مدد ↔ قوى ↔ التأمينات
        </span>
      </div>

      {/* the reconciled row */}
      <div className="px-6 py-7">
        <div className="flex items-center justify-between">
          <div className="text-right">
            <div className="text-[15px] font-medium" style={{ color: INK }}>
              أحمد العتيبي
            </div>
            <div className="mt-0.5 text-[12px]" style={{ color: MUTE }}>
              مهندس · 1023-ENG
            </div>
          </div>
          <div className="flex items-center gap-7 font-mono">
            <div className="text-center">
              <div className="text-[11px]" style={{ color: MUTE }}>
                مدد
              </div>
              <div className="text-[15px]" style={{ color: INK }}>
                8,000
              </div>
            </div>
            <span style={{ color: LINE }}>·</span>
            <div className="text-center">
              <div className="text-[11px]" style={{ color: MUTE }}>
                قوى
              </div>
              <div
                className="text-[15px] transition-colors duration-700"
                style={{ color: matched ? INK : CLAY }}
              >
                {matched ? "8,000" : "7,200"}
              </div>
            </div>
          </div>
        </div>

        {/* status line that morphs from fine to fault to fixed */}
        <div
          className="mt-6 flex items-center justify-between px-4 py-3 transition-colors duration-700"
          style={{
            background: matched ? "#F1F6F1" : reconciling ? "#FBF6E8" : "#FBF0EC",
            border: `1px solid ${
              matched ? "#D6E6D8" : reconciling ? "#EFE2BD" : "#F0D8D0"
            }`,
          }}
        >
          <span
            className="text-[13px] font-medium transition-colors duration-700"
            style={{ color: matched ? "#3F7A4F" : reconciling ? "#9A7B12" : CLAY_DEEP }}
          >
            {matched
              ? "✓ مطابَق — آمن للإرسال إلى البنك"
              : reconciling
                ? "… سند يوحّد الأجر عبر المنصّات"
                : "فرق في الأجر 800 ر.س — غرامة محتملة"}
          </span>
          <span className="font-mono text-[12px]" style={{ color: MUTE }}>
            {matched ? "0 ملاحظة" : reconciling ? "—" : "1 ملاحظة"}
          </span>
        </div>
      </div>
    </div>
  );
}

function SectionLabel({ children }: { children: React.ReactNode }) {
  return (
    <div
      className="mb-5 inline-flex items-center gap-3 text-[12px] tracking-[0.18em]"
      style={{ color: CLAY }}
    >
      <span className="inline-block h-px w-8" style={{ background: CLAY }} />
      {children}
    </div>
  );
}

export default function SanadLanding() {
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) return;
    setSent(true);
  };

  return (
    <main
      dir="rtl"
      className="relative min-h-screen w-full overflow-x-hidden"
      style={{
        background: PAPER,
        color: INK,
        fontFamily: '"Thmanyah Serif Text", Georgia, serif',
      }}
    >
      <PaperGrain />

      {/* ───────────────────────── NAV ───────────────────────── */}
      <header
        className="sticky top-0 z-40 backdrop-blur-sm"
        style={{ background: "rgba(253,251,247,0.82)", borderBottom: `1px solid ${LINE}` }}
      >
        <nav className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          {/* wordmark — right side in RTL */}
          <a href="#top" className="flex items-center gap-2.5">
            <PalmMark />
            <span
              className="text-[26px] leading-none"
              style={{ fontFamily: '"Thmanyah Serif Display", serif', fontWeight: 700 }}
            >
              سند
            </span>
          </a>

          {/* centered links */}
          <div
            className="hidden items-center gap-9 text-[15px] md:flex"
            style={{ color: INK }}
          >
            <a href="#product" className="transition-opacity hover:opacity-60">
              المنتج
            </a>
            <a href="#reconcile" className="transition-opacity hover:opacity-60">
              المطابقة الذكية
            </a>
            <a href="#pricing" className="transition-opacity hover:opacity-60">
              التسعير
            </a>
          </div>

          {/* Auth actions — left side */}
          <div className="flex items-center gap-3">
            <Link
              href="/login"
              className="text-[14px] transition-opacity hover:opacity-60"
              style={{ color: INK }}
            >
              تسجيل الدخول
            </Link>
            <Link
              href="/register"
              className="inline-flex items-center px-4 py-2 text-[14px] transition-colors"
              style={{ border: `1px solid ${INK}`, color: INK }}
              onMouseEnter={(e) => {
                e.currentTarget.style.background = INK;
                e.currentTarget.style.color = PAPER;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.background = "transparent";
                e.currentTarget.style.color = INK;
              }}
            >
              ابدأ الآن
            </Link>
          </div>
        </nav>
      </header>

      {/* ───────────────────────── HERO ───────────────────────── */}
      <section id="top" className="mx-auto max-w-6xl px-6 pt-20 pb-16 md:pt-28">
        <div className="grid items-center gap-14 md:grid-cols-[1.05fr_0.95fr]">
          {/* statement */}
          <div>
            <SectionLabel>نظام تشغيل الموارد البشرية في السعودية</SectionLabel>
            <h1
              className="text-[clamp(2.6rem,6vw,4.6rem)] font-bold leading-[1.08]"
              style={{ fontFamily: '"Thmanyah Serif Display", serif', color: INK }}
            >
              نظام تشغيل الموارد البشرية الذي{" "}
              <span style={{ color: CLAY }}>يتوقّع الغرامات</span> قبل حدوثها.
            </h1>

            <p
              className="mt-7 max-w-xl text-[clamp(1.05rem,1.6vw,1.25rem)] leading-[1.85]"
              style={{ color: MUTE }}
            >
              سند هو محرك المطابقة الأول في السعودية الذي يربط التأمينات، قوى،
              ومُدد في واجهة واحدة — ليحميك من أخطاء حماية الأجور ويُبسّط علاقتك
              مع موظفيك دون تطبيقات معقّدة.
            </p>

            {/* CTA capture */}
            <form onSubmit={submit} className="mt-9 max-w-md">
              {sent ? (
                <div
                  className="px-5 py-4 text-[15px]"
                  style={{ border: `1px solid ${LINE}`, background: "#F1F6F1", color: "#3F7A4F" }}
                >
                  ✓ تم الحجز — سنتواصل معك لتحديد موعد التجربة.
                </div>
              ) : (
                <div
                  className="flex items-stretch"
                  style={{ border: `1px solid ${INK}`, background: "#FFFFFF" }}
                >
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="بريدك في العمل"
                    className="w-full bg-transparent px-4 py-3.5 text-[15px] outline-none"
                    style={{ color: INK }}
                  />
                  <button
                    type="submit"
                    className="shrink-0 px-6 text-[14px] font-medium text-white transition-colors"
                    style={{ background: CLAY }}
                    onMouseEnter={(e) => (e.currentTarget.style.background = CLAY_DEEP)}
                    onMouseLeave={(e) => (e.currentTarget.style.background = CLAY)}
                  >
                    احجز موعد تجربة
                  </button>
                </div>
              )}
              <p className="mt-3 text-[13px]" style={{ color: MUTE }}>
                بلا بطاقة ائتمان · إعداد خلال يوم عمل واحد
              </p>
            </form>
          </div>

          {/* editorial infographic */}
          <div className="relative">
            <div
              className="absolute -top-8 right-6 hidden text-[13px] md:block"
              style={{ color: MUTE, fontFamily: '"Thmanyah Serif Text", serif' }}
            >
              لحظة قبل إرسال المسير ↓
            </div>
            <ReconciliationDemo />
            <div className="mt-4 text-center text-[13px]" style={{ color: MUTE }}>
              خطأ في ملف الأجور يتحوّل إلى مطابقة — تلقائيًا.
            </div>
          </div>
        </div>
      </section>

      {/* ───────────────────── TRUST STRIP ───────────────────── */}
      <section style={{ borderBlock: `1px solid ${LINE}` }}>
        <div className="mx-auto grid max-w-6xl grid-cols-2 md:grid-cols-4">
          {[
            ["٣ منصّات", "تُطابَق في الوقت الحقيقي"],
            ["٠ غرامة", "هدف حماية الأجور"],
            ["< ٦٠ ثانية", "للرد على الموظف"],
            ["المواد ٨٤ و ٨٥", "محاكاة نهاية الخدمة"],
          ].map(([big, small], i) => (
            <div
              key={i}
              className="px-6 py-8 text-center"
              style={{ borderInlineStart: i === 0 ? "none" : `1px solid ${LINE}` }}
            >
              <div
                className="text-[clamp(1.4rem,2.5vw,2rem)] font-bold"
                style={{ fontFamily: '"Thmanyah Serif Display", serif', color: INK }}
              >
                {big}
              </div>
              <div className="mt-1.5 text-[13px]" style={{ color: MUTE }}>
                {small}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* ──────────────── ANTI-SYSTEM-OF-RECORD MANIFESTO ──────────────── */}
      <section id="product" className="mx-auto max-w-4xl px-6 py-24 text-center">
        <SectionLabel>
          <span className="mx-auto">المبدأ</span>
        </SectionLabel>
        <h2
          className="text-[clamp(1.8rem,4vw,3rem)] font-bold leading-[1.25]"
          style={{ fontFamily: '"Thmanyah Serif Display", serif', color: INK }}
        >
          أنظمة الموارد البشرية اكتفت بـ«تسجيل» ما حدث.{" "}
          <span style={{ color: CLAY }}>سند يمنع ما لا يجب أن يحدث.</span>
        </h2>
        <p
          className="mx-auto mt-7 max-w-2xl text-[clamp(1.05rem,1.6vw,1.2rem)] leading-[1.9]"
          style={{ color: MUTE }}
        >
          لسنا «نظام سجلّات» آخر يكدّس بياناتك ثم يتركك تكتشف الخطأ بعد فوات الأوان.
          سند يقرأ الفروقات بين المنصّات الحكومية، ويتصرّف قبل أن تتحوّل إلى غرامة.
        </p>
      </section>

      {/* ───────────────────── THREE EDITORIAL BLOCKS ───────────────────── */}
      <section id="reconcile" className="mx-auto max-w-6xl px-6 pb-24">
        <div className="grid gap-px md:grid-cols-3" style={{ background: LINE }}>
          {[
            {
              n: "٠١",
              t: "رادار التأمينات وقوى",
              tag: "محرك المطابقة",
              d: "لا غرامات بعد اليوم. يقارن سند بيانات موظفيك عبر المنصّات الحكومية لحظة بلحظة، وينبّهك بأي فروقات في الأجور قبل أن ترسل المسير إلى البنك.",
            },
            {
              n: "٠٢",
              t: "تجربة بلا تطبيقات",
              tag: "نظام بلا احتكاك",
              d: "لأن الموظفين يكرهون لوحات التحكم الثقيلة. يطلبون الإجازات ويستفسرون عن الراتب مباشرة عبر واتساب أو Apple Messages — في لمح البصر، بلا تطبيق يُحمّل.",
            },
            {
              n: "٠٣",
              t: "محاكي قانون العمل",
              tag: "صندوق رملي",
              d: "احسب مكافأة نهاية الخدمة (المادتان ٨٤ و ٨٥) وقِس أثر توظيف أو إنهاء خدمة موظف على التزاماتك — قبل أن تتخذ القرار، لا بعده.",
            },
          ].map((b) => (
            <article key={b.n} className="px-8 py-10" style={{ background: PAPER }}>
              <div className="flex items-baseline justify-between">
                <span
                  className="text-[13px] tracking-[0.16em]"
                  style={{ color: CLAY }}
                >
                  {b.tag}
                </span>
                <span
                  className="text-[15px]"
                  style={{ color: MUTE, fontFamily: "monospace" }}
                >
                  {b.n}
                </span>
              </div>
              <h3
                className="mt-5 text-[1.6rem] font-bold leading-tight"
                style={{ fontFamily: '"Thmanyah Serif Display", serif', color: INK }}
              >
                {b.t}
              </h3>
              <p className="mt-4 text-[15px] leading-[1.95]" style={{ color: MUTE }}>
                {b.d}
              </p>
            </article>
          ))}
        </div>
      </section>

      {/* ───────────────────────── HOW IT WORKS ───────────────────────── */}
      <section style={{ borderBlock: `1px solid ${LINE}` }}>
        <div className="mx-auto max-w-6xl px-6 py-20">
          <SectionLabel>كيف يعمل</SectionLabel>
          <div className="grid gap-12 md:grid-cols-3">
            {[
              [
                "اربط منصّاتك",
                "صلاحية قراءة آمنة لـ قوى ومُدد والتأمينات الاجتماعية. لا إدخال يدوي، ولا جداول.",
              ],
              [
                "دع سند يطابق",
                "يقرأ الأجور والمسمّيات والاشتراكات عبر المنصّات الثلاث، ويرفع كل فرق فور ظهوره.",
              ],
              [
                "أرسل بثقة",
                "تصدِّر ملف حماية الأجور وأنت مطمئن — كل صف مُطابَق، وكل غرامة محتملة عُولجت مسبقًا.",
              ],
            ].map(([t, d], i) => (
              <div key={i}>
                <div
                  className="text-[15px]"
                  style={{ color: CLAY, fontFamily: "monospace" }}
                >
                  {`٠${i + 1}`}
                </div>
                <h3
                  className="mt-3 text-[1.35rem] font-bold"
                  style={{ fontFamily: '"Thmanyah Serif Display", serif', color: INK }}
                >
                  {t}
                </h3>
                <p className="mt-3 text-[15px] leading-[1.9]" style={{ color: MUTE }}>
                  {d}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ───────────────────────── PRICING ───────────────────────── */}
      <section id="pricing" className="mx-auto max-w-6xl px-6 py-24">
        <div className="mb-12 text-center">
          <SectionLabel>
            <span className="mx-auto">التسعير</span>
          </SectionLabel>
          <h2
            className="text-[clamp(1.8rem,4vw,2.6rem)] font-bold"
            style={{ fontFamily: '"Thmanyah Serif Display", serif', color: INK }}
          >
            تسعير يتّسع مع فريقك.
          </h2>
        </div>

        <div className="grid gap-px md:grid-cols-3" style={{ background: LINE }}>
          {[
            {
              name: "الأساس",
              price: "١٢",
              unit: "ر.س / موظف شهريًا",
              note: "للفِرق حتى ٥٠ موظفًا",
              feats: ["رادار المطابقة", "طلبات عبر واتساب", "تصدير ملف الأجور"],
              featured: false,
            },
            {
              name: "النمو",
              price: "٢٢",
              unit: "ر.س / موظف شهريًا",
              note: "الأكثر اختيارًا",
              feats: [
                "كل ما في الأساس",
                "محاكي قانون العمل",
                "تنبيهات لحظية للفروقات",
                "Apple Messages للأعمال",
              ],
              featured: true,
            },
            {
              name: "المنشآت",
              price: "حسب الطلب",
              unit: "",
              note: "لأكثر من ٥٠٠ موظف",
              feats: ["تكامل مخصّص", "مدير نجاح مخصّص", "اتفاقية مستوى خدمة"],
              featured: false,
            },
          ].map((p) => (
            <div
              key={p.name}
              className="flex flex-col px-8 py-10"
              style={{
                background: p.featured ? INK : PAPER,
                color: p.featured ? PAPER : INK,
              }}
            >
              <div
                className="text-[13px] tracking-[0.16em]"
                style={{ color: p.featured ? "#D9B5A8" : CLAY }}
              >
                {p.note}
              </div>
              <h3
                className="mt-3 text-[1.5rem] font-bold"
                style={{ fontFamily: '"Thmanyah Serif Display", serif' }}
              >
                {p.name}
              </h3>
              <div className="mt-5 flex items-baseline gap-2">
                <span
                  className="text-[2.4rem] font-bold leading-none"
                  style={{ fontFamily: '"Thmanyah Serif Display", serif' }}
                >
                  {p.price}
                </span>
                {p.unit && (
                  <span
                    className="text-[13px]"
                    style={{ color: p.featured ? "#C9B8AE" : MUTE }}
                  >
                    {p.unit}
                  </span>
                )}
              </div>
              <ul className="mt-7 flex-1 space-y-3 text-[14px]">
                {p.feats.map((f) => (
                  <li key={f} className="flex items-center gap-2.5">
                    <span style={{ color: p.featured ? "#E08E6F" : CLAY }}>—</span>
                    <span style={{ color: p.featured ? "#EDE4DF" : MUTE }}>{f}</span>
                  </li>
                ))}
              </ul>
              <a
                href="#cta"
                className="mt-8 inline-flex items-center justify-center px-5 py-3 text-[14px] font-medium transition-opacity hover:opacity-80"
                style={{
                  background: p.featured ? CLAY : "transparent",
                  color: p.featured ? "#FFFFFF" : INK,
                  border: p.featured ? "none" : `1px solid ${INK}`,
                }}
              >
                {p.price === "حسب الطلب" ? "تواصل مع المبيعات" : "ابدأ الآن"}
              </a>
            </div>
          ))}
        </div>
      </section>

      {/* ───────────────────────── CTA BAND ───────────────────────── */}
      <section id="cta" style={{ background: INK, color: PAPER }}>
        <div className="mx-auto max-w-4xl px-6 py-24 text-center">
          <PalmMark size={34} color={CLAY} />
          <h2
            className="mt-6 text-[clamp(2rem,4.5vw,3.2rem)] font-bold leading-[1.15]"
            style={{ fontFamily: '"Thmanyah Serif Display", serif' }}
          >
            دع أوّل غرامة تكون{" "}
            <span style={{ color: "#E08E6F" }}>هي الأخيرة.</span>
          </h2>
          <p
            className="mx-auto mt-6 max-w-xl text-[clamp(1.05rem,1.6vw,1.2rem)] leading-[1.9]"
            style={{ color: "#C9B8AE" }}
          >
            احجز موعد تجربة وسنُريك فروقات الأجور في بياناتك الحقيقية — خلال نصف ساعة.
          </p>
          <a
            href="#top"
            className="mt-9 inline-flex items-center px-8 py-4 text-[15px] font-medium text-white transition-colors"
            style={{ background: CLAY }}
            onMouseEnter={(e) => (e.currentTarget.style.background = CLAY_DEEP)}
            onMouseLeave={(e) => (e.currentTarget.style.background = CLAY)}
          >
            احجز موعد تجربة
          </a>
          <p className="mt-5 text-[14px]" style={{ color: "#C9B8AE" }}>
            لديك حساب بالفعل؟{" "}
            <Link
              href="/login"
              className="underline underline-offset-4 transition-opacity hover:opacity-70"
              style={{ color: "#E08E6F" }}
            >
              تسجيل الدخول
            </Link>
          </p>
        </div>
      </section>

      {/* ───────────────────────── FOOTER ───────────────────────── */}
      <footer style={{ background: PAPER }}>
        <div
          className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-6 px-6 py-10 md:flex-row"
          style={{ borderTop: `1px solid ${LINE}` }}
        >
          <div className="flex items-center gap-2.5">
            <PalmMark size={22} />
            <span
              className="text-[20px]"
              style={{ fontFamily: '"Thmanyah Serif Display", serif', fontWeight: 700 }}
            >
              سند
            </span>
          </div>
          <div className="flex items-center gap-7 text-[14px]" style={{ color: MUTE }}>
            <a href="#product" className="hover:opacity-60">المنتج</a>
            <a href="#reconcile" className="hover:opacity-60">المطابقة الذكية</a>
            <a href="#pricing" className="hover:opacity-60">التسعير</a>
          </div>
          <div className="text-[13px]" style={{ color: MUTE }}>
            © ٢٠٢٦ سند — صُنع في السعودية.
          </div>
        </div>
      </footer>
    </main>
  );
}
