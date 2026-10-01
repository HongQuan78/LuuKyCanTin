---
story: "1.3"
epic: 1
title: IClock and automatic audit-log interceptor
status: ready-for-dev
size: M
backlogItems: [NEN-05, NEN-06]
frsCovered: []
nfrsTouched: [NFR5, NFR11]
dependsOn: ["1.2"]
---

# Story 1.3: IClock and automatic audit-log interceptor

Status: ready-for-dev

## Story

As a unit leader,
I want every change to a voucher automatically recorded with who, when, from which workstation, and the before/after values,
So that every operation can be traced and nobody can alter records silently.

## Acceptance Criteria

1. **Given** the Application abstraction `IClock` (`Today`, `Now`)
   **When** any Domain or Application code needs the current date
   **Then** it uses `IClock`; a test or analyzer rule fails the build if `DateTime.Now`, `DateTime.Today` or `DateTime.UtcNow` appears in Domain, Application or Infrastructure (except the `SystemClock` implementation)
   **And** tests can substitute a fixed or simulated clock

2. **Given** the `NhatKyThaoTac` table (`Id bigint`, `ThoiDiem`, `NguoiDungId`, `MayTram`, `HanhDong` [Them, Sua, Huy, In, Duyet, DangNhap], `TenBang`, `BanGhiId`, `DuLieuCu`, `DuLieuMoi` as JSON)
   **When** an entity marked as auditable (a marker interface) is added, modified or cancelled and `SaveChanges` runs
   **Then** a `SaveChanges` interceptor inserts one audit row per affected record in the same transaction, with before/after JSON of the changed columns, the user from `ICurrentUser`, the machine name and `IClock.Now`
   **And** a cancellation (`TrangThai` → cancelled) is logged as `Huy`, not `Sua`

3. **Given** an explicit business event that does not change a voucher row (login, print, approve)
   **When** a service calls the audit-log writer
   **Then** the row is written through the same append-only path

4. **Given** the application code
   **When** anything tries to update or delete an `NhatKyThaoTac` row through EF
   **Then** an exception is thrown (the log is append-only); the DB-level `DENY UPDATE, DELETE` is applied in Epic 7

## Tasks / Subtasks

- [ ] **T1. `IClock`** (AC: 1)
  - [ ] Application `Abstractions/IClock.cs`: `DateOnly Today { get; }` and `DateTime Now { get; }`, both local time. The unit works in one time zone and the DB stores `datetime2(0)` local, so document that `Now` is local.
  - [ ] Infrastructure `Common/SystemClock.cs`, registered as a singleton.
  - [ ] Test helper `FakeClock` (settable `Now`, `Advance(TimeSpan)`) in a shared test-utilities location. A `TestUtilities/` folder inside each test project is enough (KISS). Add a shared project only if three projects need it.
- [ ] **T2. Ban `DateTime.Now`** (AC: 1)
  - [ ] Recommended: `Microsoft.CodeAnalysis.BannedApiAnalyzers` added through `Directory.Build.props` for `src/Libraries/**`. The `BannedSymbols.txt` entries are `P:System.DateTime.Now`, `P:System.DateTime.Today`, `P:System.DateTime.UtcNow`, plus `P:System.DateTimeOffset.Now` and `P:System.DateTimeOffset.UtcNow`. `SystemClock` suppresses RS0030 with `#pragma` and a comment.
  - [ ] `TreatWarningsAsErrors` (Story 1.1) makes RS0030 fail the build. Confirm that Story 1.1's architecture test ignores this `PrivateAssets="all"` analyzer for Domain.
  - [ ] Alternative if the analyzer causes friction: a source-scan test over `src/Libraries/**/*.cs`. Pick one and note it in the Completion Notes.
  - [ ] WinForms is not in the AC list, but Presenters should use `IClock` too. Add WinForms to the analyzer scope if it costs nothing.
- [ ] **T3. Current user and machine** (AC: 2)
  - [ ] Application `Abstractions/ICurrentUser.cs`: `int? NguoiDungId`, `string? TenDangNhap`, `bool DaDangNhap`. Keep it small.
  - [ ] WinForms (or Infrastructure) implementation: a singleton `CurrentUserSession` that Story 1.8's login sets. Before login, `NguoiDungId` is `null`.
  - [ ] Machine name: `Environment.MachineName`, read in Infrastructure.
- [ ] **T4. `NhatKyThaoTac` entity and table** (AC: 2)
  - [ ] Domain `HeThong/NhatKyThaoTac.cs` (append-only, **not** an `AuditableEntity`, no `RowVer`), and Domain `HeThong/HanhDong.cs`.
  - [ ] `HanhDong` is stored as `varchar(20)` text (`Them`, `Sua`, `Huy`, `In`, `Duyet`, `DangNhap`), so it's **not** a `tinyint` enum. Map it with `HasConversion<string>()` and add `CHECK (HanhDong IN ('Them','Sua','Huy','In','Duyet','DangNhap'))`. Extend Story 1.2's enum test, or add a sibling test, so string-mapped enums are checked too.
  - [ ] Columns per the DB design: `Id bigint IDENTITY PK`, `ThoiDiem datetime2(0) NOT NULL`, `NguoiDungId int NULL`, `MayTram nvarchar(100)`, `HanhDong varchar(20) NOT NULL`, `TenBang varchar(50)`, `BanGhiId bigint NULL`, `DuLieuCu nvarchar(max) NULL`, `DuLieuMoi nvarchar(max) NULL`.
  - [ ] Index `(TenBang, BanGhiId)` and `(ThoiDiem)` for the Epic 2 search screen.
  - [ ] Add a migration `AddNhatKyThaoTac`.
- [ ] **T5. Auditable marker** (AC: 2)
  - [ ] Domain `Common/IAuditable.cs`, an empty marker. The audit **columns** (`AuditableEntity`) and the audit **log** marker are separate concerns. Every voucher implements both, some master data might implement only the base. The rule is "every voucher table".
  - [ ] Domain `Common/ICoTrangThaiHuy.cs` (or a similar small interface) exposes `bool DaHuy`, so the interceptor can tell `Huy` from `Sua` without knowing each entity's enum.
- [ ] **T6. `SaveChanges` interceptor** (AC: 2, 4)
  - [ ] Infrastructure `Persistence/Interceptors/AuditInterceptor.cs : SaveChangesInterceptor`. It's registered scoped and added via `AddInterceptors` when `AppDbContext` is configured.
  - [ ] `SavingChanges[Async]`:
    - (a) Throw `InvalidOperationException` if any `NhatKyThaoTac` entry is `Modified` or `Deleted` (AC 4).
    - (b) Fill `AuditableEntity` columns: Added sets `NgayTao`/`NguoiTaoId`; Modified sets `NgaySua`/`NguoiSuaId`.
    - (c) For each `IAuditable` entry, capture `HanhDong` (Added→`Them`; Modified with `DaHuy` changed false→true→`Huy`; other Modified→`Sua`; Deleted→throw, since vouchers are never hard-deleted) and the before/after values of **changed** properties only. Skip `RowVer` and the audit columns.
  - [ ] **Same transaction** and the IDENTITY problem: Added entities have no `Id` until the INSERT runs. Recommended approach:
    - If `Database.CurrentTransaction` is null, begin one in `SavingChanges` and remember that the interceptor owns it.
    - After `SavedChanges`, build the `NhatKyThaoTac` rows (now with real ids) and save again through the same context, with a re-entrancy guard flag so the interceptor skips itself.
    - Commit if the interceptor owns the transaction. Roll back in `SaveChangesFailed`.
    - When a service already opened a transaction (posting operations do), just join it.
  - [ ] JSON: `System.Text.Json` with `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, so Vietnamese stays readable in the audit screen, with property names exactly as the column names. `DuLieuCu` is null for `Them`.
  - [ ] Never put secrets in the JSON. Add an `[KhongGhiNhatKy]`-style exclusion. Domain can't use EF attributes, but a plain custom attribute in Domain is fine. Alternatively use a configured exclusion set. `NguoiDung.MatKhauHash` (Story 1.8) **must** be excluded. Epic 2 states "passwords and hashes never appear in logs or audit JSON".
- [ ] **T7. Explicit audit-log writer** (AC: 3)
  - [ ] Application `Abstractions/IGhiNhatKy.cs`: `Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieu = null, CancellationToken ct = default)`.
  - [ ] Infrastructure implementation: add a `NhatKyThaoTac` entity to the current scoped `AppDbContext` and save, filling user, machine and `IClock.Now` exactly like the interceptor. Share a small `NhatKyFactory` so both paths build rows identically.
- [ ] **T8. Tests** (AC: 1–4)
  - [ ] Integration (LocalDB, Story 1.2 fixture), using a test-only auditable entity in a test DbContext, or the first real voucher if timing allows:
    - insert → `Them` row with `DuLieuMoi`;
    - update → `Sua` row with only changed columns;
    - set cancelled → `Huy`;
    - a failing save rolls back **both** the entity and the audit row;
    - modifying or deleting an `NhatKyThaoTac` through EF throws.
  - [ ] `IGhiNhatKy` writes a `DangNhap` row with the right user, machine and time (from a `FakeClock`).
  - [ ] Unit: the `HanhDong` decision logic as a pure function (entry state + old/new `DaHuy` → `HanhDong`).

## Dev Notes

### Current codebase state

- After Story 1.2: `AppDbContext`, `AuditableEntity` + base configuration, the `InitialCreate` migration (collation only), the LocalDB fixture and the enum-check test. No business tables yet.

### Design notes

- **No FK from `NhatKyThaoTac.NguoiDungId` yet.** `NguoiDung` is created in Story 1.8. Keep the column nullable (system and pre-login events have no user). Story 1.8 decides whether to add the FK. Recommendation: **no FK** on an append-only log, so log rows survive any future user clean-up and inserts stay cheap.
- **Only `IAuditable` entities are logged**, plus `NhatKyThaoTac` itself is excluded. Audit rows must never audit themselves.
- **"Cancelled" is generic.** Every voucher has `TrangThai` with 3 = cancelled (receipts, payouts, goods receipts, sales). `ICoTrangThaiHuy.DaHuy => TrangThai == TrangThaiChungTu.DaHuy`. That keeps the interceptor free of per-entity knowledge.
- Logging follows CLAUDE.md: "The `SaveChanges` interceptor writes the audit log (`NhatKyThaoTac`) automatically for every voucher table. The log is append-only."

### Gotchas

- Interceptor re-entrancy: calling `SaveChanges` inside `SavedChanges` re-enters the interceptor. Guard with an instance flag. The interceptor is scoped, so it has one instance per DbContext.
- `ExecuteUpdate`/`ExecuteDelete` and raw SQL **bypass** interceptors. Story 1.8's conditional balance `UPDATE` is raw SQL on `DoiTuong` (not a voucher), so it isn't logged by the interceptor, but the `ChungTuLuuKy` insert is. Write this down in the code comment so nobody later "fixes" it by moving voucher writes to `ExecuteUpdate`.
- Retrying execution strategies (`EnableRetryOnFailure`) conflict with user-initiated transactions. Don't enable retry, or wrap the work in `strategy.ExecuteAsync`. Recommendation: don't enable retry on a LAN.
- `OriginalValues` for a Modified entity are only correct if the entity was tracked from a query. Attaching a detached entity and marking it Modified loses the "before" values, so the code must load, change, then save.

### Out of scope

- The audit-log search screen (Epic 2, HT-07). The DB-level `DENY UPDATE, DELETE` (Epic 7). Print counting (Epic 4).

### Testing

- See T8. Integration tests must run in parallel safely, with a unique DB per fixture.

### References

- Epic 1 › Story 1.3; `epics.md` › NFR5, Additional Requirements (NEN-05, NEN-06)
- DB design PDF: `NhatKyThaoTac` (p.6)
- `CLAUDE.md`: "All date logic goes through `IClock`", audit interceptor rule

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
