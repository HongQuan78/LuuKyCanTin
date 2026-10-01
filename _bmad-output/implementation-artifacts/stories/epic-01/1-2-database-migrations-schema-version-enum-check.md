---
story: "1.2"
epic: 1
title: Database migrations, schema-version check and enum ↔ CHECK test
status: review
size: M
backlogItems: [NEN-02, NEN-03, NEN-04]
frsCovered: []
nfrsTouched: [NFR1, NFR11, NFR13]
dependsOn: ["1.1"]
---

# Story 1.2: Database migrations, schema-version check and enum ↔ CHECK test

Status: review

## Story

As an administrator,
I want the database created and upgraded only through versioned migrations, and workstations that refuse to run against the wrong schema version,
So that every machine always works on a consistent database.

## Acceptance Criteria

1. **Given** an empty SQL Server (LocalDB in tests)
   **When** the migrations are applied
   **Then** the database is created with the default collation `Vietnamese_CI_AI` and the shared conventions: `int IDENTITY` keys, `decimal(18,0)` money, `date` document dates, `datetime2(0)` timestamps, and audit columns `NgayTao, NguoiTaoId, NgaySua, NguoiSuaId, RowVer` applied through a base configuration
   **And** this story creates only the tables it needs; later stories add their own tables in their own migrations

2. **Given** reference data (roles, permissions, counters, units of measure, signer configuration)
   **When** a later story needs it
   **Then** it is seeded inside an EF migration (`HasData` or SQL in the migration), so a fresh install and an upgrade produce identical data
   **And** dev/demo data is seeded by a separate command that never runs in production

3. **Given** a workstation whose build expects migration `X`
   **When** it starts against a database whose last applied migration is not `X`
   **Then** the app shows "Database version does not match, contact the administrator", logs both versions, and exits without opening the main form
   **And** workstations never apply migrations themselves; migrations run only from the admin tool or command (`--migrate`)

4. **Given** every Domain enum mapped to a `tinyint` column
   **When** the enum-consistency integration test runs against the migrated database
   **Then** it reads each CHECK constraint from `sys.check_constraints` and fails if any enum value is missing from the constraint or any allowed value is missing from the enum

## Tasks / Subtasks

- [x] **T1. EF Core packages and DbContext** (AC: 1)
  - [x] Infrastructure: `Microsoft.EntityFrameworkCore.SqlServer` 10.x and `Microsoft.EntityFrameworkCore.Design` 10.x (with `PrivateAssets=all`). Add `dotnet-ef` as a local tool (`.config/dotnet-tools.json`).
  - [x] `Infrastructure/Persistence/AppDbContext.cs`. Call `modelBuilder.UseCollation("Vietnamese_CI_AI")` and `ApplyConfigurationsFromAssembly`.
  - [x] Add an `IDesignTimeDbContextFactory<AppDbContext>` so `dotnet ef` works without starting the WinForms host. It reads the connection string from an environment variable and falls back to LocalDB.
  - [x] Add `AddInfrastructure(IConfiguration)` to register `AppDbContext` as scoped, with connection string `ConnectionStrings:LuuKyCanTin` from `appsettings.json`.
- [x] **T2. Shared conventions** (AC: 1)
  - [x] Domain `Common/`: add the abstract base entity `AuditableEntity` with `NgayTao`, `NguoiTaoId`, `NgaySua`, `NguoiSuaId` and `RowVer` (`byte[]`). This is plain C# with no attributes, because Domain has no EF reference.
  - [x] Infrastructure `Persistence/Configurations/Common/`: add `AuditableEntityConfiguration<T>` (abstract `IEntityTypeConfiguration<T>`). It maps `NgayTao`/`NgaySua` as `datetime2(0)` and `RowVer` with `.IsRowVersion()`. `NguoiTaoId`/`NguoiSuaId` are `int`, and `NgaySua`/`NguoiSuaId` are nullable.
  - [x] In `ConfigureConventions`: `decimal` defaults to `decimal(18,0)`, `DateOnly` maps to `date`, `DateTime` to `datetime2(0)`, `string` to `nvarchar` (the `varchar` exceptions are set per property), and enums become `byte` (`tinyint`).
  - [x] Add a small helper `HasEnumCheck<TEnum>(this EntityTypeBuilder, column)` that emits `CHECK ([Col] IN (1,2,...))` from `Enum.GetValues<TEnum>()`. Later stories use it for every enum column. The CHECK and the enum then come from one source, and the AC 4 test still checks the **deployed** DB against the **current** enum.
- [x] **T3. Initial migration** (AC: 1)
  - [x] Add a migration `InitialCreate` that sets the DB collation and creates **no business tables**. The first business tables arrive in Stories 1.3 and 1.8. Check the generated migration: it must contain `migrationBuilder.AlterDatabase(collation: "Vietnamese_CI_AI")`.
- [x] **T4. Seed conventions** (AC: 2)
  - [x] Write the rule in `Infrastructure/Persistence/Seed/README.md`: reference data goes in migrations via `HasData` or `migrationBuilder.Sql`, and **never** in startup code.
  - [x] Add a `--seed-demo` command (same entry point as `--migrate`, see T5) that runs `DemoDataSeeder` from `Persistence/Seed/Demo/`. For now it is an empty implementation. It refuses to run unless `Environment` is `Development` or the `--force` flag is passed with a confirmation.
- [x] **T5. Admin-only migration command** (AC: 3)
  - [x] In `Program.Main`, parse the args. `--migrate` builds the host, runs `db.Database.MigrateAsync()`, logs the applied migrations, writes a summary to the console and exits with code 0 or 1. No UI opens.
  - [x] WinForms is a GUI app with no console. Call `AttachConsole(-1)` (P/Invoke) when a command-line verb is used, so admins see the output. If that gets messy, write a log line only.
  - [x] The normal startup path **never** calls `Migrate()` or `EnsureCreated()`.
- [x] **T6. Schema-version check at startup** (AC: 3)
  - [x] Application `HeThong/` (or `Common/Abstractions`): add `ISchemaVersionChecker` that returns `(string Expected, string? Actual, bool Matches)`.
  - [x] Infrastructure implements it. `Expected = db.Database.GetMigrations().Last()`. `Actual = (await db.Database.GetAppliedMigrationsAsync()).LastOrDefault()`. A failed connection is a separate result (a friendly "cannot connect to the database" message), not a version mismatch.
  - [x] `Program`: run the check before `MainForm` is created. On a mismatch, log `Expected`/`Actual` at Error level, show the message box (English per AC; Vietnamese text "Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên." is the user-facing version), and exit.
- [x] **T7. Enum ↔ CHECK integration test** (AC: 4)
  - [x] Add an `IntegrationTests/Infrastructure/LocalDbFixture`. It creates a uniquely named LocalDB database, applies **all** migrations, and drops the database on dispose. Every later integration test reuses it.
  - [x] Add `EnumCheckConstraintTests`. Walk `AppDbContext.Model` for every property whose CLR type is an enum (or nullable enum) mapped to `tinyint`. For each one, query `sys.check_constraints` joined to `sys.columns`, parse the integer list out of the `definition` (`([Col]=(1) OR [Col]=(2))` or `IN (...)`), and compare the sets both ways. Report missing enum values and missing allowed values separately.
  - [x] Also fail when an enum column has **no** CHECK constraint at all.
  - [x] No enum tables exist yet, so the test passes vacuously. Add a test-only DbContext (or a fixture SQL table plus a test enum) that proves the comparer catches each mismatch direction.

## Dev Notes

### Current codebase state

- After Story 1.1 there is an empty `Persistence/` folder in Infrastructure, `IntegrationTests` with a LocalDB smoke test, and a Generic Host in `Program.cs`. No EF code exists yet.

### DB conventions (from the DB design PDF, page 1–2)

| Item | Convention |
|---|---|
| Names | Vietnamese without diacritics, PascalCase; PK `Id`, FK `<Bang>Id` |
| PK | `int IDENTITY`; `bigint` for `ChungTuLuuKy`, `TheKho` and detail tables |
| Money | `decimal(18,0)`, integer đồng |
| Quantity | `decimal(18,3)` · average cost `decimal(18,4)` |
| Dates | document date `date`; created/modified `datetime2(0)` |
| Strings | `nvarchar`, collation `Vietnamese_CI_AI` |
| Enums | `tinyint` + CHECK, mapped to a C# enum |
| Audit columns | `NgayTao`, `NguoiTaoId`, `NgaySua`, `NguoiSuaId`, `RowVer rowversion` |
| Deletion | never hard-delete documents: `TrangThai` = cancelled + `LyDoHuy`, `NgayHuy`, `NguoiHuyId` |

### Design guidance

- **No FK from `NguoiTaoId`/`NguoiSuaId` yet.** `NguoiDung` arrives in Story 1.8. Decide now whether the audit columns get an FK to `NguoiDung` at all. The recommendation is **no FK**: it avoids multiple-cascade-path errors and a FK on every table. The audit log (`NhatKyThaoTac`) holds the authoritative user. Record the decision in the Completion Notes.
- **Who fills the audit columns.** Story 1.3's interceptor populates `NgayTao`/`NguoiTaoId`/`NgaySua`/`NguoiSuaId` from `IClock` + `ICurrentUser`. This story only maps them.
- **Enum mapping without attributes.** Domain enums should declare `: byte` so the mapping to `tinyint` is natural. Configure it in `ConfigureConventions` (`configurationBuilder.Properties<Enum>()` doesn't work generically). Either register each enum as it is added, or use a value-converter convention that iterates the model in `OnModelCreating`.
- **Workstation vs admin.** Workstations use a least-privilege SQL login later (Epic 7). That login won't have DDL rights, which is one more reason the normal path must never migrate.

### Gotchas

- **Collation is set at DB creation.** `UseCollation` on the model emits `ALTER DATABASE ... COLLATE` in the first migration. Changing the collation on a DB that already has objects fails if columns depend on the old one. That's why it must be in `InitialCreate`, before any table.
- `ALTER DATABASE COLLATE` needs exclusive access. EF runs it outside a transaction, but parallel integration tests must each use their own database (unique name per fixture).
- `GetMigrations()` reads the migrations compiled into the assembly. `GetAppliedMigrationsAsync()` reads `__EFMigrationsHistory`. A missing history table means "never migrated", which counts as a mismatch, not a crash.
- CHECK definitions in `sys.check_constraints.definition` are normalized by SQL Server (`IN (1,2)` comes back as `([X]=(1) OR [X]=(2))`). Parse with a regex on `\((\d+)\)` scoped to the column name.
- A `--migrate` path that builds the full WinForms host would also require a display. Build a host without resolving any Form.

### Out of scope

- Business tables. `NhatKyThaoTac` comes in 1.3, `NguoiDung`/`ThongTinDonVi`/`DoiTuong`/`ChungTuLuuKy`/`DemSoChungTu` in 1.8. Roles and permissions come in Epic 2, the least-privilege login and DENY script in Epic 7.

### Testing

- Integration: `EnumCheckConstraintTests` (plus its self-test), and a migration test: apply all migrations to an empty DB and assert `SELECT DATABASEPROPERTYEX(DB_NAME(),'Collation') = 'Vietnamese_CI_AI'`.
- Unit: test `SchemaVersionChecker`'s decision logic (match, mismatch, null actual) with plain values. Keep the comparison in a pure function.

### References

- Epic 1 › Story 1.2; `epics.md` › Additional Requirements › Persistence and data (seed list, conventions)
- DB design PDF: Conventions (p.1–2), Seed data on install (p.15)
- `CLAUDE.md`: "Domain enum values must match the DB CHECK constraints, and a test enforces this"

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5[1m])

### Debug Log References

- `dotnet ef migrations add InitialCreate` generated an **empty** `Up()`. EF applies the model collation through `CREATE DATABASE` only when `Migrate` creates the database itself. `AlterDatabase(collation: "Vietnamese_CI_AI")` was added by hand, so a database an admin pre-creates on SQL Express also gets the collation. `Migrate_DatabaseCreatedByAdmin_ChangesItsCollation` covers this.
- Smoke-ran the built exe against an unreachable server: `--migrate` printed the SQL error to the console and exited 1, `--seed-demo` refused outside Development and exited 1, and neither opened a window.

### Completion Notes List

- **⚠ LocalDB tests not run locally.** This machine has no SQL Server LocalDB, so the 9 new `[LocalDbFact]` tests (migration/collation, schema-checker against a real DB, enum ↔ CHECK and its self-test) were skipped here. They run on CI, where `CI` is set and a missing LocalDB fails the build. Confirm on the first CI run.
- **Decision: no FK from `NguoiTaoId`/`NguoiSuaId` to `NguoiDung`.** This follows the Dev Notes recommendation: one FK per table buys nothing over the audit log (`NhatKyThaoTac`, the authoritative user) and risks multiple-cascade-path errors. It's recorded on `AuditableEntityConfiguration<T>`.
- **Enums → tinyint without a converter.** EF maps an enum to its underlying type, so Domain enums must be declared `: byte`. `EveryEnumColumn_IsTinyint` enforces this on the model (no DB needed). `HasEnumCheck(e => e.Prop)` (nullable and non-nullable overloads) emits `CK_<Table>_<Column>` = `[Col] IN (…)` from `Enum.GetValues`.
- **AC 4 test design.** EF check constraints are table-level, so `sys.check_constraints.parent_column_id` is 0 and the "join to sys.columns" from T7 wouldn't find them. The verifier reads every constraint of the table instead and parses the values scoped to `[Col]`. It accepts both the normalized `([Col]=(1) OR …)` form and `IN (…)`. When several constraints mention the column, the allowed set is their intersection. The pure logic is tested in `EnumCheckVerifierTests`. `SelfTest_DetectsEachKindOfMismatchInADeployedDatabase` runs it against constraints SQL Server actually stored.
- **Schema check.** `SchemaVersionCheckResult` (Application) holds the pure decision. A missing database or a missing history table counts as a mismatch with `Actual = null`. A `DbException` counts as `ConnectionFailed` and gets a separate message. A DB migrated by a *newer* build is also a mismatch. On mismatch `Program` logs Expected/Actual at Error level, shows the Vietnamese message from `SchemaVersionGate` and exits 1 before `MainForm` is resolved. The normal path never calls `Migrate`/`EnsureCreated`.
- **Admin commands.** `--migrate`, `--seed-demo` and `--force=<DatabaseName>` are parsed by `AdminCommandLine` and stripped from the host args (otherwise the host's command-line provider would read `--migrate --x` as a key). `AdminCommandRunner` resolves no Form. Exit code 0 means success, 1 means failure. `--migrate --seed-demo` runs both, migration first. Output reaches the shell through `AttachConsole(-1)`. A WinExe doesn't make the shell wait, so admins should use `Start-Process … -Wait -PassThru` (PowerShell) or `start /wait` (cmd) to see the exit code. Everything is also logged to the Serilog file.
- **`--force` confirmation is non-interactive.** `--force=<name>` has to name the database the seeder is connected to (case-insensitive). Reading stdin from an attached console in a GUI process is unreliable, so this replaces an interactive prompt. The policy lives in `DemoSeedPolicy` (Application) and is enforced inside `DemoDataSeeder`, so a caller can't bypass it.
- **Package change.** `Microsoft.Data.SqlClient` went from 7.1.1 to **6.1.6**, the version EF Core SqlServer 10.0.12 depends on, so the test process never mixes major versions. `dotnet-ef` 10.0.12 is a local tool in `.config/dotnet-tools.json`.
- **Connection string comes from `.env`, never from `appsettings.json`** (review feedback). Before the host is built, `DotEnvFile.Load` reads the `.env` file next to the exe and sets `ConnectionStrings__LuuKyCanTin` as an environment variable, unless one with that name is already set (a real environment variable wins). The host's environment-variable provider then supplies `ConnectionStrings:LuuKyCanTin`. `.env` is gitignored, and `src/Presentation/LuuKyCanTin.WinForms/.env.example` documents it. A developer's `.env` in the project folder is copied to the output on build. Values are literal, so `.\SQLEXPRESS` needs no JSON escaping. `AddInfrastructure` fails fast with a message that points to `.env`. The design-time factory reads the same variable name. The least-privilege login comes in Epic 7.
- **⚠ For Spike 1.6 (Velopack):** an update replaces the app folder, so a `.env` beside the exe may not survive it. The spike needs to decide where the workstation `.env` lives, for example `%ProgramData%\LuuKyCanTin\.env`.
- **Known limitation.** If `appsettings.json` itself is unreadable, the failure happens before Serilog is configured. The error then reaches the console or the global handler but not the log file.
- Application gets no EF reference: the three new ports (`ISchemaVersionChecker`, `IDatabaseMigrator`, `IDemoDataSeeder`) are plain interfaces. The open Application/EF Core decision from Story 1.1 is still open.

### File List

- `.config/dotnet-tools.json` (new)
- `Directory.Packages.props` (modified)
- `CLAUDE.md` (modified: migration commands)
- `src/Libraries/LuuKyCanTin.Domain/Common/AuditableEntity.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/HeThong/ISchemaVersionChecker.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/HeThong/SchemaVersionCheckResult.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/HeThong/IDatabaseMigrator.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/HeThong/IDemoDataSeeder.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/HeThong/DemoSeedPolicy.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/LuuKyCanTin.Infrastructure.csproj` (modified)
- `src/Libraries/LuuKyCanTin.Infrastructure/DependencyInjection.cs` (modified)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/AppDbContext.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/DesignTimeDbContextFactory.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/SchemaVersionChecker.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/DatabaseMigrator.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/Common/AuditableEntityConfiguration.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/Common/EnumCheckExtensions.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/20261001141501_InitialCreate.cs` (+ `.Designer.cs`, `AppDbContextModelSnapshot.cs`) (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Seed/README.md` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Seed/Demo/DemoDataSeeder.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Program.cs` (modified)
- `src/Presentation/LuuKyCanTin.WinForms/appsettings.json` (modified)
- `src/Presentation/LuuKyCanTin.WinForms/Common/ParentConsole.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Common/DotEnvFile.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/.env.example` (new)
- `src/Presentation/LuuKyCanTin.WinForms/LuuKyCanTin.WinForms.csproj` (modified: copy `.env` to output)
- `tests/LuuKyCanTin.WinForms.UnitTests/Common/DotEnvFileTests.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/HeThong/AdminCommandLine.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/HeThong/AdminCommandRunner.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/HeThong/SchemaVersionGate.cs` (new)
- `tests/LuuKyCanTin.Application.UnitTests/HeThong/SchemaVersionCheckResultTests.cs` (new)
- `tests/LuuKyCanTin.Application.UnitTests/HeThong/DemoSeedPolicyTests.cs` (new)
- `tests/LuuKyCanTin.WinForms.UnitTests/HeThong/AdminCommandLineTests.cs` (new)
- `tests/LuuKyCanTin.WinForms.UnitTests/HeThong/AdminCommandRunnerTests.cs` (new)
- `tests/LuuKyCanTin.WinForms.UnitTests/HeThong/SchemaVersionGateTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/LuuKyCanTin.IntegrationTests.csproj` (modified)
- `tests/LuuKyCanTin.IntegrationTests/Common/LocalDbFactAttribute.cs` (modified: `ShouldRun`)
- `tests/LuuKyCanTin.IntegrationTests/Common/TestDatabase.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Common/LocalDbFixture.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/TestModel/MauChungTu.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/ModelConventionTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/InfrastructureRegistrationTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/MigrationTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/SchemaVersionCheckerTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/SchemaVersionCheckerUnreachableServerTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/EnumChecks/EnumCheckVerifier.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/EnumChecks/EnumCheckVerifierTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/EnumChecks/EnumCheckConstraintTests.cs` (new)
- Removed `.gitkeep` from `Domain/Common`, `Application/HeThong`, `Infrastructure/Persistence`, `WinForms/HeThong`

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-01 | Implemented: EF Core 10 + InitialCreate (collation), shared conventions, enum CHECK helper, `--migrate`/`--seed-demo`, startup schema check, enum ↔ CHECK integration test. Status → review |
