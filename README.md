# LvlUp ⚔️

A Solo Leveling-inspired self-improvement app. You are a **Hunter**: complete real-life quests
(workouts, skincare, Bible reading, coding, conversation practice…) to gain XP, level up, raise
your rank from **E to S**, and grow seven stats — **Strength, Stamina, Physique, Looks,
WellBeing, Intelligence, Charisma**.

New account? Seed it with the starter habit pack (4 body, 1 skincare, 2 reading, 1 coding,
2 voice/confidence dailies + 4 milestone quests):

```powershell
./scripts/seed-quests.ps1 -Email you@example.com -Password "your-password" `
    -Name You -Surname Yourself -Username your_username   # name fields only needed to register
```

## Stack

| Part | Tech |
| --- | --- |
| `backend/` | C# / .NET 10, Clean Architecture, CQRS (no MediatR), Result pattern, Minimal APIs, EF Core (PostgreSQL via Npgsql), JWT auth, Serilog (Console + Seq), FluentValidation, Scrutor |
| `frontend/` | React Native + Expo (SDK 53), expo-router, TypeScript (strict), AsyncStorage, Jest |

## Backend architecture

Five projects in `backend/src/`, dependencies flow inward only (enforced by NetArchTest in
`LvlUp.ArchitectureTests`):

``` txt
SharedKernel  ←  Domain  ←  Application  ←  Infrastructure  ←  Api
```

- **CQRS without MediatR** — `IQueryHandler<TQuery,TResponse>` / `ICommandHandler<TCommand[,TResponse]>`,
  auto-registered via Scrutor assembly scanning; endpoints inject handlers directly.
- **Decorators** — command handlers wrapped by `ValidationDecorator` (FluentValidation) +
  `LoggingDecorator`; query handlers get `LoggingDecorator` only.
- **Result pattern** — everything returns `Result`/`Result<T>`; endpoints convert with
  `.Match(Results.Ok, CustomResults.Problem)` → RFC 7807 problem details by `ErrorType`.
- **Minimal API endpoints** — each is an `internal sealed class : IEndpoint`, reflection-scanned and
  mapped at startup; `.HasPermission(Permissions.X)` for authz, `.WithTags(Tags.X)` for OpenAPI.
- **Data gateways** — read-heavy/aggregating queries go through raw-SQL gateways
  (parameterized `FormattableString` + `Database.SqlQuery<T>`): the interface + row DTO live next
  to the feature in Application, the implementation in `Infrastructure/DataGateways/`
  (e.g. `ConsistencyDataGateway` aggregates per-day completion counts in PostgreSQL).
- **Domain** — entities inherit `Entity` (domain events via `Raise()`); EF configs via
  `ApplyConfigurationsFromAssembly`; events dispatched after `SaveChanges` (eventual consistency).
- **Auth** — JWT bearer (register/login issue tokens); `IUserContext.UserId` from claims;
  permission-based authz via custom `PermissionAuthorizationPolicyProvider` (currently grants all
  permissions to any authenticated hunter).
- **Strict build** — `TreatWarningsAsErrors`, `AnalysisMode=All`, SonarAnalyzer; intentional
  rule relaxations are documented in `backend/.editorconfig`.

## Game rules

- **Consistency** — streaks are computed from the immutable, server-timestamped completion log
  (no backfilling, one daily completion per UTC day). A day with ≥1 completion extends the
  streak; today never breaks it while in progress. Every 7 consecutive active days banks a
  **streak shield** (max 3) that auto-absorbs one missed day. The status window shows current/
  longest streak, shields, a 30-day discipline score, and a 12-week heatmap
  (`GET /hunters/me/consistency`).
- **Proof of work** — completing a quest prompts for an optional reflection note (stored on the
  completion record). Intelligence quests can require **GitHub verification**: completion is
  rejected unless the linked GitHub account (set on the status screen) has a public push event
  today. Note: the unauthenticated GitHub events feed only sees public repositories.

- **Awakening assessment** — new hunters answer 7 self-assessment questions (one per stat,
  scored 1–5) before entering the app. Scores set the *starting* stats (`4 + 2×score`, so 6–14
  instead of a flat 10), and the starter quest pack is calibrated to them: stat ≤ 8 → Easy
  quests, 9–11 → Medium, ≥ 12 → Hard. You start at your level, not above it. One-time only,
  and unavailable once any XP has been earned.

- XP to next level: `level × 100`. Excess XP carries over; multiple level-ups in one gain are possible.
- Rank by level: E (<10), D (10+), C (20+), B (30+), A (40+), S (50+).
- Quest difficulty → rewards: Easy +10 XP/+1 stat, Medium +25/+2, Hard +50/+3, Elite +100/+5.
- Quest types: **Daily** (completable once per UTC day, resets at midnight) and **OneTime**.
- Every completion is recorded in `QuestCompletions` as an immutable history log.

## Database

PostgreSQL 17 (Dockerized for local dev via `backend/docker-compose.yml`), managed by EF Core
migrations in `backend/src/LvlUp.Infrastructure/Database/Migrations/`. All tables live in the
`lvlup` schema with snake_case naming (EFCore.NamingConventions):

| Table | Columns | Notes |
| --- | --- | --- |
| `lvlup.hunters` | `id` (uuid PK), `email` (varchar 256, unique), `password_hash` (varchar 512), `name` (varchar 50), `surname` (varchar 50), `username` (varchar 30, unique, lowercase), `display_name_preference` (int enum: 0 FullName / 1 Username), `github_username` (varchar 39, null), `level`, `current_xp`, `total_xp` (int), `created_at_utc`, `assessed_at_utc` (timestamptz) | One row per account; display name is computed from the preference |
| `lvlup.hunter_stats` | `hunter_id` (uuid PK, FK → hunters, cascade delete), `strength`, `stamina`, `physique`, `looks`, `well_being`, `intelligence`, `charisma` (int) | One row per hunter; mapped as an EF owned entity, always loaded with the hunter |
| `lvlup.quests` | `id` (uuid PK), `hunter_id` (uuid FK → hunters, cascade delete, indexed), `title` (varchar 100), `description` (varchar 500, null), `category`, `difficulty`, `type` (int enums), `created_at_utc`, `last_completed_at_utc` (timestamptz, null) | Daily completion state derives from `last_completed_at_utc` |
| `lvlup.quest_completions` | `id` (uuid PK), `quest_id`, `hunter_id` (uuid, indexed), `category`, `xp_awarded`, `stat_awarded` (int), `completed_at_utc` (timestamptz) | Append-only history; intentionally no FK to quests so it survives quest deletion |

## Running the backend

Requires .NET 10 SDK and Docker (for PostgreSQL):

```powershell
cd backend
docker compose up -d                                        # PostgreSQL 17 on localhost:5432
dotnet run --project src/LvlUp.Api --launch-profile Local   # http://localhost:5180
```

EF Core migrations are applied automatically at startup in the Local environment. To manage them
manually:

```powershell
dotnet ef migrations add <Name> --project src/LvlUp.Infrastructure --startup-project src/LvlUp.Api --output-dir Database/Migrations
dotnet ef database update --project src/LvlUp.Infrastructure --startup-project src/LvlUp.Api
```

OpenAPI document (Local/Development): `http://localhost:5180/openapi/v1.json`.

### Tests

```powershell
cd backend
dotnet test          # 12 architecture tests + 53 unit tests
```

## Running the frontend

```powershell
cd frontend
npm install
npm start            # Expo dev server — press a (Android), i (iOS), w (web)
```

### Tests & checks

```powershell
npm test             # Jest (29 tests)
npm run typecheck    # tsc --noEmit
```

### Branding assets

The logo masters are SVGs in `frontend/assets/` (`logo.svg`, `adaptive-icon.svg`,
`splash-icon.svg`). The PNGs Expo consumes (app icon, Android adaptive icon, splash image,
favicon) are generated from them — after editing an SVG, run:

```powershell
npm run generate-assets
```

## Environments

### Backend

Environment is selected by `ASPNETCORE_ENVIRONMENT` (launch profiles pin it — `Local` and
`Development` profiles exist; use `dotnet run --no-launch-profile` to honor the variable). Settings
layer as `appsettings.json` → `appsettings.{Environment}.json` → user secrets → environment
variables.

| Environment | Config file | DB / JWT secret come from | Notes |
| --- | --- | --- | --- |
| Local | `appsettings.Local.json` | Checked-in Docker PostgreSQL string + dev-only JWT secret | Your machine. Migrations applied at startup; OpenAPI on; binds `http://*:5180` so LAN devices (your phone) can reach it |
| Development | `appsettings.Development.json` | `ConnectionStrings__Database`, `Jwt__Secret` env vars | Deployed dev server. OpenAPI on; Serilog: Information |
| Staging | `appsettings.Staging.json` | `ConnectionStrings__Database`, `Jwt__Secret` env vars | Serilog: Information |
| Production | `appsettings.Production.json` | `ConnectionStrings__Database`, `Jwt__Secret` env vars | Serilog: Warning, console sink only |

Missing configuration **fails fast at startup**: the connection string is checked when services are
registered, and `JwtOptions` is validated via `ValidateOnStart` (secret ≥ 32 chars, issuer,
audience, expiration > 0) with actionable error messages.

For local secrets beyond the checked-in dev values, prefer user secrets:

```powershell
dotnet user-secrets set "Jwt:Secret" "<your-secret>" --project backend/src/LvlUp.Api
```

### Frontend

The API base URL resolves from `EXPO_PUBLIC_API_URL` (inlined at bundle time), falling back to the
platform-aware localhost default in `src/constants/api.ts` (`10.0.2.2:5180` on Android emulators).

| Environment | Source of `EXPO_PUBLIC_API_URL` | When it applies |
| --- | --- | --- |
| Development | `.env.development` (unset by default → localhost fallback) | `expo start` |
| Staging | `eas.json` → `build.staging.env` | `eas build --profile staging` |
| Production | `.env.production` or `eas.json` → `build.production.env` | `expo export`, `eas build --profile production` |

`.env*.local` files are gitignored for machine-specific overrides (e.g. your LAN IP for
physical-device testing). The committed `.env` files contain no secrets — only public URLs.
Replace the `lvlup.example.com` placeholders with real origins when you have them.

## CI (GitHub Actions)

Branching model: feature work → `dev` → `qa` → `main`.

- **`ci-gated.yml`** — on PRs to `dev`/`qa`/`main`: backend build (warnings are errors) + tests,
  frontend typecheck + Jest.
- **`ci-build.yml`** — on push to those branches, branch-driven:

| Branch | App env | API artifact | App build |
| --- | --- | --- | --- |
| `dev` | development | `lvlup-api-Development` | Debug APK built on the runner (no EAS account needed) |
| `qa` | qa | `lvlup-api-Staging` | EAS build, `staging` profile |
| `main` | production | `lvlup-api-Production` | EAS build, `production` profile |

Repo configuration (Settings → Secrets and variables → Actions):
- Secret `EXPO_TOKEN` — enables the EAS builds on `qa`/`main` (skipped with a warning if absent).
- Variables `DEV_API_URL`, `STAGING_API_URL`, `PRODUCTION_API_URL` — override the placeholder API
  origins baked into app builds.

## API

Base URL `http://localhost:5180`. Errors are RFC 7807 problem details; validation failures include
an `errors` array. Authenticated routes need `Authorization: Bearer <token>`.

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/auth/register` | — | `{ email, password, name, surname, username }` → `{ token, hunterId }` |
| POST | `/auth/login` | — | `{ email, password }` → `{ token, hunterId }` |
| GET | `/hunters/me` | ✓ | Hunter status: names, displayName, level, XP, rank, five stats |
| PUT | `/hunters/me/display-preference` | ✓ | `{ preference: "FullName" \| "Username" }` → 204 |
| POST | `/hunters/me/assessment` | ✓ | `{ scores: { Strength: 1-5, … } }` (all 7) → starting stats + recommended difficulties; 409 if repeated or XP > 0 |
| POST | `/quests/starter-pack` | ✓ | Creates the starter habit quests calibrated to current stats; idempotent |
| GET | `/hunters/me/consistency` | ✓ | Streaks, shields, discipline score, 84-day heatmap |
| PUT | `/hunters/me/github` | ✓ | `{ username }` (null to unlink) → 204 |
| GET | `/quests` | ✓ | All quests with computed `isCompleted` |
| POST | `/quests` | ✓ | `{ title, description?, category, difficulty, type }` → `{ id }` |
| POST | `/quests/{id}/complete` | ✓ | `{ note? }` — awards XP + stat; 409 if already completed; 400 if GitHub verification fails |
| DELETE | `/quests/{id}` | ✓ | 204; completion history is preserved |

Enums are JSON strings: `category` ∈ Strength/Stamina/Physique/Looks/WellBeing/Intelligence/Charisma,
`difficulty` ∈ Easy/Medium/Hard/Elite, `type` ∈ Daily/OneTime.
