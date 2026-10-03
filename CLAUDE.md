# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**LuuKyCanTin**: a WinForms .NET 10 desktop app for a detention facility. It handles custodial deposits (*tiền gửi lưu ký*) for detainees and the canteen (*căn tin*) that sells against those balances. A few LAN workstations connect directly to one SQL Server 2022 Express instance. There is no API server and no Internet access.

The domain language is Vietnamese, but **code is English and only user-visible text is Vietnamese**. New identifiers use the English glossary terms in `docs/conventions/naming-conventions.md` (`CustodyVoucher`, `Inmate`, `StockCard`, `AuditLog`). UI text, messages and printed reports stay Vietnamese with full diacritics.

The source-of-truth specs live in `document/`: the process (.docx), the feature list (49 features, codes like HT-01, LK-T01), the tech stack, and the DB design (PDFs). The prioritized backlog is `_bmad-output/brainstorming/.../backlog-draft.md`. Read these before designing a feature.

## Commands

```powershell
dotnet build LuuKyCanTin.slnx
dotnet test LuuKyCanTin.slnx
dotnet test tests/LuuKyCanTin.Domain.UnitTests
dotnet test --filter "FullyQualifiedName~AmountInWordsTests"          # single class
dotnet test --filter "FullyQualifiedName~AmountInWordsTests.MethodName" # single test
dotnet run --project src/Presentation/LuuKyCanTin.WinForms

# Migrations (dotnet-ef is a local tool: run `dotnet tool restore` once)
dotnet ef migrations add <Name> --project src/Libraries/LuuKyCanTin.Infrastructure --output-dir Persistence/Migrations
dotnet run --project src/Presentation/LuuKyCanTin.WinForms -- --migrate                 # admin only; workstations never migrate
dotnet run --project src/Presentation/LuuKyCanTin.WinForms -- --migrate --seed-demo --environment Development
```

The connection string lives in a gitignored `.env` (`ConnectionStrings__LuuKyCanTin=...`), never in `appsettings.json`. Copy `src/Presentation/LuuKyCanTin.WinForms/.env.example` to `.env` in the same folder. At startup the app refuses to run if the database's last applied migration is not the build's last migration. Run `--migrate` after you add one. Every enum column needs `HasEnumCheck(...)`, and every enum must be declared `: byte`. Tests enforce both.

Tests use xUnit, NSubstitute and Shouldly. Don't use FluentAssertions, because it now has a commercial license.

Integration tests (`[SqlServerFact]`) create throw-away databases on the server named by `LUUKYCANTIN_TEST_SQLSERVER` (environment or the root `.env`). Without it they use LocalDB, which is what CI does, and if neither exists they are skipped locally. For a local server, copy `.env.example` to `.env`, set the password, then run `docker compose up -d` (SQL Server 2022 in Docker; here Docker runs inside WSL Ubuntu: `wsl -d Ubuntu -- docker compose up -d`). Keep `127.0.0.1` in connection strings, because the WSL relay accepts but never answers SQL over `::1`. WSL stops its VM when idle, which also stops the container, so keep a WSL terminal open while testing.

## Architecture

The solution follows Clean Architecture, with dependencies pointing inward only:

```
WinForms (Presentation) ─► Application ─► Domain
        └──────────────► Infrastructure ─► Application, Domain
```

- **Domain** (`src/Libraries/LuuKyCanTin.Domain`) references nothing. It holds entities, enums, value objects and pure business rules, such as the balance rule, weighted-average cost (`WeightedAverageCost`), the document state machine and amount-in-words (`AmountInWords`).
- **Application** holds use-case services (`CustodyLedgerService`, `SalesService`, `GoodsReceiptService`, report queries), DTOs, FluentValidation validators and abstractions: `IAppDbContext`, `IReportRenderer`, `ICurrentUser`, `IClock`, `INumberingService`.
- **Infrastructure** implements those abstractions. It contains EF Core 10 (`Persistence/`: DbContext, Configurations, Migrations, Seed), QuestPDF print templates (`Reports/`, one class per template), ClosedXML import/export (`Excel/`), SQL backup and Serilog.
- **WinForms** uses MVP. Forms are passive Views behind interfaces, and Presenters call Application services. `Program.cs` builds the Generic Host (DI, config, Serilog, global exception handler). The reference to Infrastructure exists **only** for DI composition.
- Inside each project, organize folders by business module: `Custody/`, `Inventory/`, `MasterData/`, `Administration/`, `Reporting/` (plus `Common/` and `Abstractions/`).

### Key design rules (from the spec)

- **Ledger-first.** `CustodyLedgerService` is the one engine that writes `CustodyVoucher` and updates `Inmate.CustodyBalance` with a conditional UPDATE under a row lock. Receipts, payouts, canteen sales and settlements are all callers of this engine. They never modify balances directly.
- Each posting operation is **one service method, one transaction** (a unit of work). For example, `SalesService.PostAsync` writes the sale, the `StockCard` rows and the custodial debit together.
- A balance never goes negative. Money is stored as integer đồng, with no decimals or floats.
- Vouchers share one state machine: Draft → Posted → Cancelled (with a reason). Snapshot names and categories at posting time so a reprint matches the original.
- Document numbers come from `INumberingService` (`VoucherCounter` + UPDLOCK), counted per type and per year.
- All date logic goes through `IClock`. Never use `DateTime.Now` directly. `src/BannedSymbols.txt` (BannedApiAnalyzers, RS0030) fails the build otherwise; `SystemClock` is the only exception.
- Voucher entities implement `IAuditable` (and `ICancellable` so a cancellation logs as `AuditAction.Cancel`); mark secrets `[NotAudited]`. Write vouchers through `SaveChanges`, never `ExecuteUpdate`/raw SQL, or the audit log misses them. Log non-voucher events (sign-in, print, approve) through `IAuditLogWriter`.
- Services never depend on WinForms (no `MessageBox`). Services re-check permissions before writing, because hidden UI elements don't count as authorization.
- Forms never hold a `DbContext`. Each operation creates a fresh DI scope.
- The `SaveChanges` interceptor writes the audit log (`AuditLog`) automatically for every voucher table. The log is append-only. Rows written before the English rename keep their Vietnamese table and property names; translate them through `LegacyAuditNames`. The stored action codes (`Them`, `Sua`, …) are data and never change.
- Domain enum values must match the DB CHECK constraints, and a test enforces this.
- Use `rowversion` for optimistic concurrency. Write complex reports as SQL (EF `SqlQuery` or Dapper), not as complex LINQ.

## Conventions (mandatory)

- **Naming.** Every new file, type, method, property, field and variable follows `docs/conventions/naming-conventions.md`: identifiers in English (glossary terms), user-visible text in Vietnamese. Where a story's text names an identifier in Vietnamese, translate it with the glossary; the convention wins.
- **UI prototypes.** Every WinForms screen must strictly follow the style of the UI prototypes in `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/`. That means the layout, spacing, controls, colours, fonts, shortcuts and states in `DESIGN.md`, `EXPERIENCE.md` and `mockups/*.html`. Follow `docs/conventions/ui-prototype-conventions.md`. A new layout or a new kind of control is a UX decision, so ask before building it.
- **Story dependency gate.** Before writing code for a story, apply `docs/conventions/story-dependency-gate.md`. If any story in its `dependsOn` is not `review` or `done` (for example `ready-for-dev`), report the blocking dependencies and stop without changing files, unless the user explicitly overrides the gate.

## Engineering rules (always apply)

- **SOLID**:
  - Single responsibility per class. A Presenter coordinates, a service owns one use case, a repository or DbContext configuration owns persistence.
  - Extend through new implementations rather than by editing stable code.
  - Depend on abstractions defined in Application or Domain, never on Infrastructure types.
  - Keep interfaces small and client-specific.
- **KISS**: choose the simplest design that satisfies the current story. Don't add layers, generic repositories, patterns or configurability until a real need exists. (YAGNI applies too.)
- **Clean Architecture**:
  - Never add a project reference that points outward. Domain gets no NuGet/EF/UI dependencies. Application gets no EF-provider, QuestPDF, ClosedXML or WinForms dependencies.
  - Business rules belong in Domain or Application, never in Forms, Presenters or EF configurations.
  - Everything in Domain and Application must be unit-testable without a UI or a database.
