# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**LuuKyCanTin**: a WinForms .NET 10 desktop app for a detention facility. It handles custodial deposits (*tiền gửi lưu ký*) for detainees and the canteen (*căn tin*) that sells against those balances. A few LAN workstations connect directly to one SQL Server 2022 Express instance. There is no API server and no Internet access.

The domain language is Vietnamese, and entity and service names use it (`ChungTuLuuKy`, `SoDuLuuKy`, `DoiTuong`, `PhieuBanHang`, `TheKho`, `NhatKyThaoTac`). Keep new names in the same language and style.

The source-of-truth specs live in `document/`: the process (.docx), the feature list (49 features, codes like HT-01, LK-T01), the tech stack, and the DB design (PDFs). The prioritized backlog is `_bmad-output/brainstorming/.../backlog-draft.md`. Read these before designing a feature.

## Commands

```powershell
dotnet build LuuKyCanTin.slnx
dotnet test LuuKyCanTin.slnx
dotnet test tests/LuuKyCanTin.Domain.UnitTests
dotnet test --filter "FullyQualifiedName~SoDuLuuKyTests"          # single class
dotnet test --filter "FullyQualifiedName~SoDuLuuKyTests.MethodName" # single test
dotnet run --project src/Presentation/LuuKyCanTin.WinForms
```

Tests use xUnit, NSubstitute and Shouldly. Don't use FluentAssertions, because it now has a commercial license. The planned integration tests run against SQL Server LocalDB.

## Architecture

The solution follows Clean Architecture, with dependencies pointing inward only:

```
WinForms (Presentation) ─► Application ─► Domain
        └──────────────► Infrastructure ─► Application, Domain
```

- **Domain** (`src/Libraries/LuuKyCanTin.Domain`) references nothing. It holds entities, enums, value objects and pure business rules, such as the balance rule, weighted-average cost (`BinhQuanGiaQuyen`), the document state machine and amount-in-words (`SoTienBangChu`).
- **Application** holds use-case services (`GhiSoLuuKyService`, `BanHangService`, `NhapHangService`, report queries), DTOs, FluentValidation validators and abstractions: `IAppDbContext`, `IReportRenderer`, `ICurrentUser`, `IClock`, `INumberingService`.
- **Infrastructure** implements those abstractions. It contains EF Core 10 (`Persistence/`: DbContext, Configurations, Migrations, Seed), QuestPDF print templates (`Reports/`, one class per template), ClosedXML import/export (`Excel/`), SQL backup and Serilog.
- **WinForms** uses MVP. Forms are passive Views behind interfaces, and Presenters call Application services. `Program.cs` builds the Generic Host (DI, config, Serilog, global exception handler). The reference to Infrastructure exists **only** for DI composition.
- Inside each project, organize folders by business module: `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/` (plus `Common/` and `Abstractions/`).

### Key design rules (from the spec)

- **Ledger-first.** `GhiSoLuuKyService` is the one engine that writes `ChungTuLuuKy` and updates `SoDuLuuKy` with a conditional UPDATE under a row lock. Receipts, payouts, canteen sales and settlements are all callers of this engine. They never modify balances directly.
- Each posting operation is **one service method, one transaction** (a unit of work). For example, `BanHangService.GhiSoAsync` writes the sale, the `TheKho` rows and the custodial debit together.
- A balance never goes negative. Money is stored as integer đồng, with no decimals or floats.
- Vouchers share one state machine: Draft → Posted → Cancelled (with a reason). Snapshot names and categories at posting time so a reprint matches the original.
- Document numbers come from `INumberingService` (`DemSoChungTu` + UPDLOCK), counted per type and per year.
- All date logic goes through `IClock`. Never use `DateTime.Now` directly.
- Services never depend on WinForms (no `MessageBox`). Services re-check permissions before writing, because hidden UI elements don't count as authorization.
- Forms never hold a `DbContext`. Each operation creates a fresh DI scope.
- The `SaveChanges` interceptor writes the audit log (`NhatKyThaoTac`) automatically for every voucher table. The log is append-only.
- Domain enum values must match the DB CHECK constraints, and a test enforces this.
- Use `rowversion` for optimistic concurrency. Write complex reports as SQL (EF `SqlQuery` or Dapper), not as complex LINQ.

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
