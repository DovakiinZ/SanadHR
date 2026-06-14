import type { NextConfig } from "next";

// In local development the browser runs on http://localhost:3001 but the API
// lives on the Azure App Service, whose CORS policy only allows the Vercel
// origins. Rather than loosen production CORS, we proxy /api/* through the Next
// dev server (same-origin → no CORS preflight). Server-to-server has no CORS.
// Point the client at this same origin via .env.local: NEXT_PUBLIC_API_URL=http://localhost:3001
const API_PROXY_TARGET = "https://hrcloud-api-v4xd.azurewebsites.net";

const nextConfig: NextConfig = {
  async rewrites() {
    // Dev-only: on Vercel the client calls the Azure API directly (allowed by CORS).
    if (process.env.NODE_ENV === "production") return [];
    return [
      {
        source: "/api/:path*",
        destination: `${API_PROXY_TARGET}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
