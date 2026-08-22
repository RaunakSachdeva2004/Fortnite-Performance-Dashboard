# SQLite Migration & FortniteAPI.io Integration Notes

This file documents what changed when the project moved from SQL Server to
SQLite, and the assumptions made about FortniteAPI.io's response shape. It's
meant to sit alongside the existing requirement-analysis and system-design
PDFs in this folder, not replace them.

## 1. Why SQLite

SQLite needs no server install (no LocalDB/SQL Server Express), stores the
whole database in a single file (`fortnite_dashboard.db`) next to the
executable, and works identically on Windows, macOS, and Linux. That makes it
a better fit for a small internship project that needs to run on someone
else's machine (e.g. an instructor's laptop) with minimal setup.

## 2. What changed in the schema

The original design stored one `Stats` row **per Player** (a 1:1
relationship, enforced with a unique index on `PlayerId`), and every "Sync"
overwrote that same row. That made the dashboard's historical trend charts
impossible to build honestly — there was never more than one data point.

`Stats` is now a **history table**: one row is inserted every time a player
syncs, so `Player` has a one-to-many relationship with `Stats`. The "current"
KPI cards use the most recent row (`OrderByDescending(RecordedAt).First()`),
and the trend charts use the last N rows.

Two smaller schema changes went along with this:
- `Deaths` was added. FortniteAPI.io doesn't report deaths directly; it's
  derived as `MatchesPlayed - Wins` (every non-winning match counts as one
  death), which is the standard convention most Fortnite stat trackers use.
- `WinRate` used to be a SQL Server *computed column* (`PERSISTED`,
  calculated by the database itself). SQLite/EF Core has no direct
  equivalent for that T-SQL feature, so `WinRate` is now a normal column,
  computed once in C# (`StatsCalculator.ComputeWinRate`) before saving —
  which also means the exact same code works if this project is ever pointed
  at a different database later.

See `Database/FortniteDashboard_Schema_SQLite.sql` for the full DDL, and
`Database/FortniteDashboard_Schema_SqlServer_Legacy.sql` for the original.

## 3. FortniteAPI.io shut down — switched to Fortnite-API.com

**Update (August 2026):** FortniteAPI.io, the service originally specified
for this project, has shut down (its own homepage stated *"This API will
close on March 31, 2026"*, and the domain no longer resolves). The
integration now targets **Fortnite-API.com** instead — a different,
long-running, actively maintained service (uptime history going back to
2019). Note the hyphen: `fortnite-api.com`, not `fortniteapi.io`.

**Verified** (checked directly against Fortnite-API.com's own docs at
`dash.fortnite-api.com`):
- Endpoint: `GET https://fortnite-api.com/v2/stats/br/v2?name={username}&accountType=epic&timeWindow=lifetime`
  — a single call resolves the username directly; no separate account-ID
  lookup step is needed (simpler than the old FortniteAPI.io flow).
- Authentication: an `Authorization` header carrying an API key (a GUID),
  generated for free from `dash.fortnite-api.com`.

**Assumed / not independently verified** (isolated to
`Models/FortniteApiModels.cs` and `Services/StatsService.cs`):
- The exact response body shape (`data.data.account`,
  `data.data.stats.all.overall.{wins,kills,matches}`). The vendor's own
  example responses are rendered client-side and weren't retrievable
  through automated fetching; this shape is reconstructed from independent
  third-party integration examples using this same API, not from a raw
  response captured directly. **Confirm this once you have a real key** —
  see the action item below.
- This dashboard uses the `all` input devices / `overall` mode totals
  (combined across solo/duo/squad and every input device) as the single
  headline number, rather than a per-mode breakdown. Deliberate scope
  decision for a small project, not an API limitation.
- `KDRatio` and `WinRate` are **not** taken from the API's own `kd`/`winRate`
  fields, even though they likely exist — they're computed from the raw
  `wins`/`kills`/`matches` counts via `StatsCalculator`, so the exact same
  tested formula applies regardless of which provider is behind
  `IFortniteApiClient`.
- **Accuracy is still not populated.** No verified accuracy field exists in
  this API's stats response either. `Stats.Accuracy` defaults to `0`, which
  the recommendation engine treats as "unknown" and ignores.

**Action item once you have a real API key:** call `Sync Stats` once, then
inspect the row it inserts into `Stats` (or add a temporary log line in
`StatsService`) to confirm the mapped numbers look sane for a real account.
If the response comes back shaped differently than expected — e.g. a 200
response but `MapApiResponseToSnapshot` produces all zeros — the fix belongs
in `FortniteApiModels.cs` (the DTOs) and `FortniteApiClient.GetPlayerStatsAsync`
— nowhere else in the app touches the raw API response.

## 4. Getting a Fortnite-API.com key

1. Create a free account at `dash.fortnite-api.com` and generate an API key
   from the **Account** page there.
2. Store it with User Secrets — **never** commit it to `appsettings.json`:
   ```
   dotnet user-secrets set "FortniteApi:ApiKey" "your-real-key-here"
   ```
   In Visual Studio: right-click the project → **Manage User Secrets** →
   add the same key/value into the JSON that opens.
