// Client-side export helpers for widgets — CSV (data) and PNG (chart image).
import { WidgetDataResult } from "@/types/dashboard";

function download(filename: string, blob: Blob) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

function csvCell(v: unknown): string {
  if (v === null || v === undefined) return "";
  const s = typeof v === "boolean" ? (v ? "true" : "false") : String(v);
  return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
}

export function resultToCsv(result: WidgetDataResult): string {
  if (result.kind === "scalar") {
    return `value\n${result.value ?? 0}\n`;
  }
  if (result.kind === "series") {
    const rows = ["label,value", ...result.series.map((p) => `${csvCell(p.label)},${p.value}`)];
    return rows.join("\n") + "\n";
  }
  // table
  const header = result.columns.map((c) => csvCell(c.label)).join(",");
  const body = result.rows.map((r) => result.columns.map((c) => csvCell(r[c.code])).join(","));
  return [header, ...body].join("\n") + "\n";
}

export function exportCsv(title: string, result: WidgetDataResult) {
  // BOM so Excel reads UTF-8 (Arabic) correctly.
  const blob = new Blob(["﻿" + resultToCsv(result)], { type: "text/csv;charset=utf-8" });
  download(`${sanitize(title)}.csv`, blob);
}

export async function exportPng(title: string, container: HTMLElement) {
  const svg = container.querySelector("svg");
  if (!svg) { exportSvgFallback(title, container); return; }

  const clone = svg.cloneNode(true) as SVGSVGElement;
  const rect = svg.getBoundingClientRect();
  const w = Math.max(1, Math.round(rect.width));
  const h = Math.max(1, Math.round(rect.height));
  clone.setAttribute("width", String(w));
  clone.setAttribute("height", String(h));
  clone.setAttribute("xmlns", "http://www.w3.org/2000/svg");

  const xml = new XMLSerializer().serializeToString(clone);
  const svgBlob = new Blob([xml], { type: "image/svg+xml;charset=utf-8" });
  const url = URL.createObjectURL(svgBlob);

  try {
    const img = new Image();
    img.width = w; img.height = h;
    await new Promise<void>((resolve, reject) => {
      img.onload = () => resolve();
      img.onerror = () => reject(new Error("img load failed"));
      img.src = url;
    });
    const scale = 2;
    const canvas = document.createElement("canvas");
    canvas.width = w * scale; canvas.height = h * scale;
    const ctx = canvas.getContext("2d");
    if (!ctx) throw new Error("no ctx");
    ctx.fillStyle = "#18181B";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.scale(scale, scale);
    ctx.drawImage(img, 0, 0, w, h);
    canvas.toBlob((blob) => { if (blob) download(`${sanitize(title)}.png`, blob); }, "image/png");
  } finally {
    URL.revokeObjectURL(url);
  }
}

function exportSvgFallback(title: string, container: HTMLElement) {
  // No <svg> (e.g. KPI/table) — export the rendered text content as a small note.
  const blob = new Blob([container.innerText], { type: "text/plain;charset=utf-8" });
  download(`${sanitize(title)}.txt`, blob);
}

function sanitize(name: string): string {
  return (name || "widget").replace(/[\\/:*?"<>|]+/g, "_").slice(0, 60);
}
