import type { Metadata } from "next";
import { Toaster } from "sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import "./globals.css";

export const metadata: Metadata = {
  title: "سند — نظام إدارة الموارد البشرية",
  description: "نظام متكامل لإدارة الموارد البشرية",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ar" dir="rtl" className="h-full antialiased">
      <body className="min-h-full flex flex-col font-sans">
        <TooltipProvider>{children}</TooltipProvider>
        {/* Global credit — shown on every page */}
        <div className="pointer-events-none fixed bottom-2 left-2 z-50 select-none text-[10px] tracking-wide text-muted-foreground/60">
          Designed by Dovakin
        </div>
        <Toaster
          position="top-center"
          dir="rtl"
          theme="light"
          richColors
          closeButton
          toastOptions={{ style: { fontFamily: "inherit" } }}
        />
      </body>
    </html>
  );
}
