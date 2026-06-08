@AGENTS.md

---

# ✅ DATABASE STATUS — PROVISIONED & INTEGRATED

**The database is live.** Provisioned on Azure and the `InitialCreate` migration has been applied successfully (verified 2026-06-09). The mandatory start-of-response reminder is **stopped**.

## Live instance
- **Engine:** PostgreSQL 16
- **Host:** `hrcloud-pg-v4xd.postgres.database.azure.com` (Azure Database for PostgreSQL — Flexible Server)
- **Region:** UAE North
- **Plan:** Burstable **B1ms** (1 vCore / 2 GiB, 32 GB storage) — covered by the Free Trial benefit / $200 credit
- **App database:** `hrcloud` · **admin user:** `hradmin`
- **Resource group:** `HR` (subscription `5588e3fa-bddf-4657-979a-d99c64221ca4`)
- **Password:** stored in Key Vault `secretpulse` as secret `hrcloud-db-password` (and locally at `~/.hrcloud-db-pass.txt`)

## Connection strings
- **Local dev:** `appsettings.json` keeps `localhost:5432` — unchanged, do NOT commit prod secrets here.
- **Production:** inject via env var `ConnectionStrings__DefaultConnection`:
  `Host=hrcloud-pg-v4xd.postgres.database.azure.com;Port=5432;Database=hrcloud;Username=hradmin;Password=<secret>;Ssl Mode=Require;Trust Server Certificate=true`

## Backend API — DEPLOYED (2026-06-09)
- **App Service:** `hrcloud-api-v4xd` → **https://hrcloud-api-v4xd.azurewebsites.net** (Swagger at `/swagger`)
- **Plan:** `hrcloud-plan`, F1 **Free** Linux, **West Europe** (UAE North had 0 App Service quota on the free trial; cross-region to the UAE-North DB is fine, ~40ms)
- **Runtime:** DOTNETCORE:8.0 · deployed via `az webapp deploy --type zip`
- **App settings:** `ConnectionStrings__DefaultConnection` → Azure Postgres; `ConnectionStrings__Redis` blanked (Redis not provisioned); `ASPNETCORE_ENVIRONMENT=Development` (so Swagger is on — switch to Production later); `Cors__Origins__0/1` → Vercel domains.
- **DB connectivity VERIFIED:** `POST /api/auth/login` returns a real "user not found" from a Postgres query (clean round-trip).
- **Zip-deploy gotcha:** PowerShell `Compress-Archive` writes nested paths with `\` which Linux Kudu rsync rejects. Build the zip via `System.IO.Compression.ZipFile` with `.Replace('\\','/')` on entry names.

## Remaining
- **Frontend still uses mock data** (`src/lib/mock-data.ts`, `tasks-mock-data.ts`) — Vercel does NOT call the API yet. Need a data layer + `NEXT_PUBLIC_API_URL` env on Vercel pointing at the App Service, then verify CORS.
- DB firewall allows `AllowAzureServices` (0.0.0.0) + dev machine IP. Tighten if needed.
- Consider switching API to `ASPNETCORE_ENVIRONMENT=Production` (disables public Swagger) once wiring is done.
