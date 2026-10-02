# Epic 1 Context: Foundation & first printed receipt

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Prove the whole stack before building features (Sprint 0). An administrator signs in, registers one detainee, records one 500,000 đ custodial receipt through the real ledger engine, and prints it as a PDF with the unit header and the amount in Vietnamese words. Along the way the technical risks are retired: offline PDF printing, LAN-based updates and diacritic-insensitive Vietnamese search. Exit criteria: the walking skeleton runs through all 4 layers, CI is green, the three spike outcomes are written down, and decisions DEC-02 (send-to-relative payout type), DEC-05 (gift-slip money handling) and DEC-09 (opening balances as approved carried-forward documents) are settled with the PO.

## Stories

- Story 1.1: Solution skeleton, host and CI
- Story 1.2: Database migrations, schema-version check and enum ↔ CHECK test
- Story 1.3: IClock and automatic audit-log interceptor
- Story 1.4: Amount in Vietnamese words
- Story 1.5: Spike: QuestPDF printing with Vietnamese fonts
- Story 1.6: Spike: Velopack updates from a LAN shared folder
- Story 1.7: Spike: Vietnamese diacritic-insensitive incremental search
- Story 1.8: Walking skeleton: sign in, register a detainee, post and print one receipt

## Requirements & Constraints

- **Clean Architecture, inward-only.** Domain references nothing and has no NuGet packages; Application references only Domain (plus FluentValidation and `Microsoft.EntityFrameworkCore` core — no provider); Infrastructure references Application and Domain; WinForms references Application and Infrastructure — Infrastructure only for DI composition. An architecture test fails the build if Application pulls in EF SqlServer, QuestPDF, ClosedXML or WinForms. No library that is not free/permissively licensed (QuestPDF Community licence: NOT eligible for this unit — see docs/decisions/0001-pdf-engine-questpdf.md, PO decision pending; no FluentAssertions).
- **Testing conventions.** xUnit + NSubstitute + Shouldly. Domain and Application must be unit-testable without a UI or DB; Presenters are tested with a fake View and no Form. Integration tests run on LocalDB and each one ends with a ledger reconciliation assertion: `SoDuLuuKy` = total posted receipts − total posted payouts.
- **App host.** Generic Host in `Program.cs` with DI and `appsettings.json`; Serilog daily rolling file logs to `%ProgramData%\LuuKyCanTin\logs` (30 files kept); a global handler catches unhandled UI-thread and background exceptions, logs them and shows a friendly Vietnamese message without a stack trace.
- **MVP.** Views are passive interfaces, Presenters call Application services, services never touch `MessageBox`, and Forms never hold a `DbContext` (each operation uses a fresh DI scope).
- **Persistence.** EF Core 10 code-first migrations with `Vietnamese_CI_AI` collation and shared conventions: `int IDENTITY` keys, `decimal(18,0)` money (integer đồng, never float), `date` document dates, `datetime2(0)` timestamps, audit columns (`NgayTao`, `NguoiTaoId`, `NgaySua`, `NguoiSuaId`, `RowVer`) via a base configuration, enums as `tinyint` + CHECK. Reference data (roles, permissions, counters, UoMs, signer config) is seeded inside migrations so fresh install and upgrade match; dev/demo data is a separate command that never runs in production.
- **Schema-version discipline.** Workstations never apply migrations; they check at startup that the database's last applied migration equals the build's and refuse to run (log both versions, friendly message, exit). Migrations run only from the admin `--migrate` command.
- **Every enum value must exist in its DB CHECK constraint and vice versa**, verified by an integration test that reads `sys.check_constraints`.
- **All date logic through `IClock`**; using `DateTime.Now`/`Today`/`UtcNow` anywhere except `SystemClock` fails the build.
- **Audit log.** `NhatKyThaoTac` is append-only; a `SaveChanges` interceptor writes one row per affected auditable record in the same transaction (who, workstation, before/after JSON, `Huy` for cancellations, `Them`/`Sua`/`In`/`Duyet`/`DangNhap` for explicit events). Updating or deleting log rows through EF throws.
- **Ledger engine.** `GhiSoLuuKyService` is the only writer of `ChungTuLuuKy`/`SoDuLuuKy`; in this epic it supports receipts only (Epic 4 completes it). Posting allocates the number (`BNT`-year) via `DemSoChungTu` and updates the balance with a conditional `UPDATE … WITH (UPDLOCK, ROWLOCK) OUTPUT` in one transaction; balance is never negative; no code outside the service writes the balance.
- **Printing.** QuestPDF spike must prove embedded Unicode Vietnamese fonts on A4/A5 with no extra fonts installed, and preview/print/save via an embedded WebView2 window that works offline (the WebView2 Runtime offline-install requirement is recorded). Snapshotted document data means a reprint matches the original. The spike agrees the `IReportRenderer` shape (input model → PDF bytes) for all later templates; one class per template lives in `Infrastructure/Reports`.
- **Updates.** Velopack spike proves self-contained install and one-touch update from a UNC LAN folder (`\\server\LuuKyCanTin\releases`, read for workstations, write for admin) with no .NET runtime pre-installed; the note records how the DB-version check interacts with updates.
- **Search.** Spike proves diacritic-insensitive incremental search (prefix/contains pattern recorded) over 10,000 Vietnamese names under `Vietnamese_CI_AI` with an index; results refresh while typing with ~300 ms debounce, top 50 rows, each query under 300 ms; the pattern feeds Story 3.2.
- **Amount in words.** Domain `SoTienBangChu` converts integer đồng, starts with a capital letter, ends "đồng", default style "lẻ" (alternative one setting), throws `ArgumentOutOfRangeException` for negatives, and depends on nothing outside Domain.
- **Skeleton login.** Seeded `admin` account with salted PBKDF2 hash (`Rfc2898DeriveBytes.Pbkdf2`); success opens the shell and populates `ICurrentUser`; wrong password shows an error. Full login/account rules come in Epic 2.
- **Print frame.** The skeleton's receipt print shows the unit header from `ThongTinDonVi` (single-row table) plus document number, detainee name, amount in figures and words. The full shared frame (signature block, reprint watermark, print counting, A4/A5 for all 12 templates) is Epic 4's FR42/FR43 work.

## Technical Decisions

- Solution layout: `src/Libraries/LuuKyCanTin.Domain|Application|Infrastructure`, `src/Presentation/LuuKyCanTin.WinForms`, tests `Domain.UnitTests`, `Application.UnitTests`, `IntegrationTests`, buildable via `LuuKyCanTin.slnx`. Every source project has module folders `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/`, `Common/` (Application adds `Abstractions/`).
- Application abstractions: `IAppDbContext`, `IReportRenderer`, `ICurrentUser`, `IClock`, `INumberingService`.
- DB tables this epic creates: `NguoiDung`, `ThongTinDonVi` (single row, `CHECK (Id = 1)`), `DoiTuong` (with `SoDuLuuKy decimal(18,0) DEFAULT 0 CHECK (>= 0)`), `ChungTuLuuKy`, `DemSoChungTu`, `NhatKyThaoTac`. Later stories add their own tables in their own migrations.
- Document state machine Draft → Posted → Cancelled (with reason) and snapshot columns on postings are introduced here and shared by all later voucher types.
- Spikes are time-boxed and each must end with a written decision note under `docs/decisions/` — that note is the story's deliverable.
- CI runs on a Windows agent: restore, build, run all tests (including LocalDB integration tests), fail on any failure.

## UX & Interaction Patterns

- Unhandled exceptions surface as a friendly Vietnamese message with no stack trace; business-rule failures (e.g. insufficient balance) show clear messages.
- PDF output opens in the integrated WebView2 preview window (UX-DR6), the shared preview/print surface for all 12 templates later.
- The login form and a minimal detainee form are enough for the skeleton; full form patterns land with their epics.

## Cross-Story Dependencies

- Chain: 1.1 → 1.2 → 1.3; 1.4 and 1.5 depend on 1.1; 1.7 depends on 1.2; 1.8 depends on 1.2, 1.3, 1.4, 1.5. Exit gate: 1.1–1.8 all done, CI green, DEC-02/DEC-05/DEC-09 decided.
- Hand-offs to later epics: full login/unit setup/accounts (Epic 2), complete ledger engine + receipt features + shared print frame (Epic 4), detainee search pattern (Story 3.2), Velopack rollout (Story 7.4), `IReportRenderer` consumption by all print templates (Epic 4 onward).