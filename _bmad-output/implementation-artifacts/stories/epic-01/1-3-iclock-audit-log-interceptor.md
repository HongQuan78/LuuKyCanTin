---
story: "1.3"
epic: 1
title: IClock and automatic audit-log interceptor
status: done
size: M
backlogItems: [NEN-05, NEN-06]
frsCovered: []
nfrsTouched: [NFR5, NFR11]
dependsOn: ["1.2"]
---

# Story 1.3: IClock and automatic audit-log interceptor

Status: done

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

- [x] **T1. `IClock`** (AC: 1)
  - [x] Application `Abstractions/IClock.cs`: `DateOnly Today { get; }` and `DateTime Now { get; }`, both local time. The unit works in one time zone and the DB stores `datetime2(0)` local, so document that `Now` is local.
  - [x] Infrastructure `Common/SystemClock.cs`, registered as a singleton.
  - [x] Test helper `FakeClock` (settable `Now`, `Advance(TimeSpan)`) in a shared test-utilities location. A `TestUtilities/` folder inside each test project is enough (KISS). Add a shared project only if three projects need it.
- [x] **T2. Ban `DateTime.Now`** (AC: 1)
  - [x] Recommended: `Microsoft.CodeAnalysis.BannedApiAnalyzers` added through `Directory.Build.props` for `src/Libraries/**`. The `BannedSymbols.txt` entries are `P:System.DateTime.Now`, `P:System.DateTime.Today`, `P:System.DateTime.UtcNow`, plus `P:System.DateTimeOffset.Now` and `P:System.DateTimeOffset.UtcNow`. `SystemClock` suppresses RS0030 with `#pragma` and a comment.
  - [x] `TreatWarningsAsErrors` (Story 1.1) makes RS0030 fail the build. Confirm that Story 1.1's architecture test ignores this `PrivateAssets="all"` analyzer for Domain.
  - [x] Alternative if the analyzer causes friction: a source-scan test over `src/Libraries/**/*.cs`. Pick one and note it in the Completion Notes.
  - [x] WinForms is not in the AC list, but Presenters should use `IClock` too. Add WinForms to the analyzer scope if it costs nothing.
- [x] **T3. Current user and machine** (AC: 2)
  - [x] Application `Abstractions/ICurrentUser.cs`: `int? NguoiDungId`, `string? TenDangNhap`, `bool DaDangNhap`. Keep it small.
  - [x] WinForms (or Infrastructure) implementation: a singleton `CurrentUserSession` that Story 1.8's login sets. Before login, `NguoiDungId` is `null`.
  - [x] Machine name: `Environment.MachineName`, read in Infrastructure.
- [x] **T4. `NhatKyThaoTac` entity and table** (AC: 2)
  - [x] Domain `HeThong/NhatKyThaoTac.cs` (append-only, **not** an `AuditableEntity`, no `RowVer`), and Domain `HeThong/HanhDong.cs`.
  - [x] `HanhDong` is stored as `varchar(20)` text (`Them`, `Sua`, `Huy`, `In`, `Duyet`, `DangNhap`), so it's **not** a `tinyint` enum. Map it with `HasConversion<string>()` and add `CHECK (HanhDong IN ('Them','Sua','Huy','In','Duyet','DangNhap'))`. Extend Story 1.2's enum test, or add a sibling test, so string-mapped enums are checked too.
  - [x] Columns per the DB design: `Id bigint IDENTITY PK`, `ThoiDiem datetime2(0) NOT NULL`, `NguoiDungId int NULL`, `MayTram nvarchar(100)`, `HanhDong varchar(20) NOT NULL`, `TenBang varchar(50)`, `BanGhiId bigint NULL`, `DuLieuCu nvarchar(max) NULL`, `DuLieuMoi nvarchar(max) NULL`.
  - [x] Index `(TenBang, BanGhiId)` and `(ThoiDiem)` for the Epic 2 search screen.
  - [x] Add a migration `AddNhatKyThaoTac`.
- [x] **T5. Auditable marker** (AC: 2)
  - [x] Domain `Common/IAuditable.cs`, an empty marker. The audit **columns** (`AuditableEntity`) and the audit **log** marker are separate concerns. Every voucher implements both, some master data might implement only the base. The rule is "every voucher table".
  - [x] Domain `Common/ICoTrangThaiHuy.cs` (or a similar small interface) exposes `bool DaHuy`, so the interceptor can tell `Huy` from `Sua` without knowing each entity's enum.
- [x] **T6. `SaveChanges` interceptor** (AC: 2, 4)
  - [x] Infrastructure `Persistence/Interceptors/AuditInterceptor.cs : SaveChangesInterceptor`. It's registered scoped and added via `AddInterceptors` when `AppDbContext` is configured.
  - [x] `SavingChanges[Async]`:
    - (a) Throw `InvalidOperationException` if any `NhatKyThaoTac` entry is `Modified` or `Deleted` (AC 4).
    - (b) Fill `AuditableEntity` columns: Added sets `NgayTao`/`NguoiTaoId`; Modified sets `NgaySua`/`NguoiSuaId`.
    - (c) For each `IAuditable` entry, capture `HanhDong` (Added→`Them`; Modified with `DaHuy` changed false→true→`Huy`; other Modified→`Sua`; Deleted→throw, since vouchers are never hard-deleted) and the before/after values of **changed** properties only. Skip `RowVer` and the audit columns.
  - [x] **Same transaction** and the IDENTITY problem: Added entities have no `Id` until the INSERT runs. Recommended approach:
    - If `Database.CurrentTransaction` is null, begin one in `SavingChanges` and remember that the interceptor owns it.
    - After `SavedChanges`, build the `NhatKyThaoTac` rows (now with real ids) and save again through the same context, with a re-entrancy guard flag so the interceptor skips itself.
    - Commit if the interceptor owns the transaction. Roll back in `SaveChangesFailed`.
    - When a service already opened a transaction (posting operations do), just join it.
  - [x] JSON: `System.Text.Json` with `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, so Vietnamese stays readable in the audit screen, with property names exactly as the column names. `DuLieuCu` is null for `Them`.
  - [x] Never put secrets in the JSON. Add an `[KhongGhiNhatKy]`-style exclusion. Domain can't use EF attributes, but a plain custom attribute in Domain is fine. Alternatively use a configured exclusion set. `NguoiDung.MatKhauHash` (Story 1.8) **must** be excluded. Epic 2 states "passwords and hashes never appear in logs or audit JSON".
- [x] **T7. Explicit audit-log writer** (AC: 3)
  - [x] Application `Abstractions/IGhiNhatKy.cs`: `Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieu = null, CancellationToken ct = default)`.
  - [x] Infrastructure implementation: add a `NhatKyThaoTac` entity to the current scoped `AppDbContext` and save, filling user, machine and `IClock.Now` exactly like the interceptor. Share a small `NhatKyFactory` so both paths build rows identically.
- [x] **T8. Tests** (AC: 1–4)
  - [x] Integration (LocalDB, Story 1.2 fixture), using a test-only auditable entity in a test DbContext, or the first real voucher if timing allows:
    - insert → `Them` row with `DuLieuMoi`;
    - update → `Sua` row with only changed columns;
    - set cancelled → `Huy`;
    - a failing save rolls back **both** the entity and the audit row;
    - modifying or deleting an `NhatKyThaoTac` through EF throws.
  - [x] `IGhiNhatKy` writes a `DangNhap` row with the right user, machine and time (from a `FakeClock`).
  - [x] Unit: the `HanhDong` decision logic as a pure function (entry state + old/new `DaHuy` → `HanhDong`).

### Review Findings

Code review 2026-10-03 (diff `4306fe5^..4306fe5`, verified against HEAD `15773af`).

- [x] [Review][Decision] A detached `Update()`/`Attach` save of an `IAuditable` entity writes no audit row. EF sets `OriginalValue == CurrentValue` on every property, so the value-comparer filter in `GhiNhan` drops them all and returns null. The UPDATE still runs and `NgaySua`/`NguoiSuaId` are still stamped. A cancellation done this way is not logged as `Huy` either. No `src` code uses `Update`/`Attach` today, but nothing stops the next voucher service from doing so. Options: (a) throw when a Modified `IAuditable` entry has flagged columns but none really changed ("load, change, save"); (b) read the "before" values from `GetDatabaseValues()` (one extra query per modified voucher); (c) keep the code as is and only document it. **Resolved: (a).** `GhiNhan` throws in that case. The throw broke `VaiTroService.CapNhatQuyenAsync` (story 2.3), which marked the whole role `Modified` only to bump `RowVer`, so that method now flags just `NgaySua`. That still issues the UPDATE and keeps the concurrency check. [src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs:188]
- [x] [Review][Decision] AC 4 says "anything … through EF" must fail on an update or delete of `NhatKyThaoTac`. `ExecuteUpdate`/`ExecuteDelete` are EF APIs that bypass the `SaveChanges` guard, and the DB `DENY` waits for Epic 7. No `src` code calls them today. Options: (a) add a `DbCommandInterceptor` that rejects `UPDATE`/`DELETE` commands on `NhatKyThaoTac`; (b) accept the gap until Epic 7 and note it in AC 4's Completion Notes. **Resolved: (b).** Recorded in the Completion Notes and `deferred-work.md`. [src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs:145]
- [x] [Review][Decision] Enum values inside `DuLieuCu`/`DuLieuMoi` are serialized as numbers (`"TrangThai":3`). `HanhDong` is stored as its name precisely so the log reads well. Adding `JsonStringEnumConverter` to `NhatKyFactory` makes them names (`"DaHuy"`). That is cheap now and costly once log rows have piled up in the field. Options: (a) names; (b) keep numbers. **Resolved: (a).** [src/Libraries/LuuKyCanTin.Infrastructure/HeThong/NhatKyFactory.cs:12]
- [x] [Review][Patch] Rename story 1.3 code to `docs/conventions/naming-conventions.md`. The user asked for this explicitly. It covers `XacDinhHanhDong.Tu`, `KhoaCua`, `ToJson`, `ChuanBi` (a bool return that doesn't read as a question), `DuocGhiGiaTri`, `now` locals, collection names, `EnumModel.IsStoredAsName`/`EnumColumn.StoredAsName`, `EnumCheckVerifier.Normalize`/`Quote`, test helpers (`NewVoucher`, `InsertAsync`, `CreateAuditedContext`, …) and test-method names. Names prescribed by the spec or CLAUDE.md stay (`IAuditable`, `AuditInterceptor`, `ICurrentUser`, `CurrentUserSession`, `IClock`, `FakeClock.Advance`).
- [x] [Review][Patch] `TimeProvider.System` (`GetLocalNow`/`GetUtcNow`) reads the system clock and is not banned. [src/BannedSymbols.txt:1]
- [x] [Review][Patch] No test edits a voucher that is already cancelled. Hard-coding `daHuyTruoc = false` at line 192 keeps every test green, while a later edit of a cancelled voucher would be logged as a second `Huy`. [src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs:192]
- [x] [Review][Patch] The rollback on a failed save is masked by disposal. `FailingSave_RollsBack…` checks only after the context is disposed, so deleting `SaveChangesFailedAsync` stays green. `DoiTuongStore.ThemAsync` catches `DbUpdateException` and keeps the scope. Add a retry-in-the-same-context test. [src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs:124]
- [x] [Review][Defer] The "flagged but unchanged" comparer filter in `GhiNhan` is never exercised. `UpdateWithoutRealChange_WritesNoRow` leaves the entry `Unchanged`, so `GhiNhan` never runs. [src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs:200] — deferred: no caller flags properties without changing them. Settle together with the detached-update decision.
- [x] [Review][Defer] No model test enforces the `IAuditable` invariants: a single integer key (otherwise `BanGhiId` is null) and `ICoTrangThaiHuy` on every voucher (otherwise a cancellation is logged as `Sua`). [src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs:234] — deferred: every current `IAuditable` (`CanBo`, `NguoiDung`, `VaiTro`, `ChungTuLuuKy`) has an int key, and `ChungTuLuuKy` implements `ICoTrangThaiHuy`. Add the test when Epic 3 adds more vouchers.
- [x] [Review][Defer] CLAUDE.md doesn't mention the text-stored enum exception (`HanhDong` as `varchar` names, with `HasConversion<string>()` before `HasEnumCheck`). [CLAUDE.md] — deferred: the fix edits an agent-context file.

**Rejected:**
- `SaveChanges(acceptAllChangesOnSuccess: false)` makes the nested log save write the voucher again. Rejected (`low`): no caller uses that overload, and a fix adds a guard.
- A failed log write inside a caller's transaction leaves the voucher in that transaction. Rejected (`low`): the exception propagates, every caller disposes its transaction without committing on an exception, and the Completion Notes already say to discard the context.
- The change tracker no longer matches the DB after a failed log write. Rejected (`low`): documented in the Completion Notes, and the one-scope-per-operation rule discards the context.
- A rollback exception masks the original error, and `CommitAsync` uses the caller's token. Rejected (`low`): either way the user sees a failure, and the fix adds guards.
- The stamp and the log row read the clock and user separately (`SavingChanges` vs `SavedChanges`). Rejected (`low`): the difference is under a second, or a user switch mid-save, and the fix adds parameters.
- `CurrentUserSession` getters can tear between properties, which contradicts its comment. Rejected (`low`): every consumer reads only `NguoiDungId`.
- A failed `GhiAsync` leaves its row `Added`. Rejected (`low`): the next save on the same scope fails the same way, and the fix adds a guard.
- `NguoiTaoId = 0` when nobody is signed in. Rejected (`low`): there is no FK on `NguoiTaoId`, and the seed data uses 0 as well, as decided in the Completion Notes.
- `HasEnumCheck` depends on call order. Rejected (`low`): it is documented on the method, and the LocalDB enum test in CI catches a misorder.
- Owned types, complex properties, TPC and entity splitting. Rejected (`false`): the model has none.
- `MauChungTu.SoTien` is `decimal`. Rejected (`false`): `decimal(18,0)` is the project's money convention (`ConfigureConventions`).
- `CurrentUserSession` can't be set from Application. Rejected (`false`): `ICurrentUserSession` exists since story 2.2.
- `IGhiNhatKy.GhiAsync` saves the whole unit of work. Rejected: by design, and documented in its `<remarks>`.
- The cancellation reason is not checked. Rejected: out of scope (the voucher state machine stories).
- `XacDinhHanhDong` depends on `EntityState` and lives in Infrastructure. Rejected (`low`): it maps an EF state, so Infrastructure is the right layer.
- Missing tests for a sync log-write failure, `SaveChangesCanceled`, a cancellation with no user, and mixed audited and unaudited saves. Rejected (`low`): no named failure beyond the rollback patch above.

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

Claude Opus 5.5 (Amelia / bmad-agent-dev). Implemented directly from this story file because `uv` isn't installed, so the `bmad-build` workflow couldn't render.

### Debug Log References

- The first `dotnet build` after adding the analyzer failed in NuGet with "Cannot create a file when that file already exists" (a parallel-extraction race). `dotnet restore --disable-parallel` fixed it.
- Mutation check: disabling the interceptor's own transaction makes `FailingAuditWrite_RollsBackTheEntityToo` fail. Change reverted.

### Completion Notes List

- **T2 decision: analyzer, not a source scan.** `Microsoft.CodeAnalysis.BannedApiAnalyzers` 5.6.0 is wired through a new `src/Directory.Build.props`, which imports the root props, and `src/BannedSymbols.txt`. It covers all four `src` projects, including WinForms at no extra cost. With `TreatWarningsAsErrors`, RS0030 fails the build. I verified this with throw-away probes in Domain (`DateTime.Now`) and WinForms (`DateTimeOffset.UtcNow`). The architecture test reads raw `.csproj` XML, so the props-level reference never reaches it. It is `PrivateAssets="all"` either way. `BannedApiTests` guards the configuration. Tests are deliberately outside the ban.
- **`HanhDong` is stored as its name.** `HasEnumCheck` now emits `IN ('Them', …)` when the property was converted with `HasConversion<string>()`, so configure the conversion first. The enum ↔ CHECK verifier understands quoted values (`'A'`, `N'A'`, and `IN` lists). Name-stored columns are checked against enum names. The tinyint test skips them, and a separate `EveryEnum_IsDeclaredByte` test keeps the `: byte` rule for every enum.
- **Transaction.** The interceptor opens its own transaction only when a save has something to log and no caller transaction exists. Otherwise it joins the caller's. Log rows are built in `SavedChanges`, after IDENTITY ids exist, and saved through the same context behind a re-entrancy flag. If the log write fails, the owned transaction is rolled back and the queued log rows are detached. The entity's tracker state has already been accepted at that point, so discard the context after such a failure, which the one-scope-per-operation rule does anyway.
- **No signed-in user.** On insert, `NguoiTaoId` (non-nullable) gets `0` when nobody is signed in (seeding, admin commands). The log row keeps `NguoiDungId = null`. Story 1.8 may revisit this once `NguoiDung` exists.
- **AC 4 and bulk EF writes.** The `SaveChanges` guard rejects any update or delete of `NhatKyThaoTac`. `ExecuteUpdate`/`ExecuteDelete` bypass interceptors, so nothing blocks them until Epic 7 applies `DENY UPDATE, DELETE` at the database (review decision). No code calls them on the log.
- **Load, change, save.** Saving a Modified `IAuditable` whose flagged columns all equal their original values (a detached `Update()`/`Attach`) throws, because the log could not tell what changed. To bump `RowVer` without a real change, flag only a bookkeeping column (`NgaySua`), as `VaiTroService` does.
- **Secrets.** `[KhongGhiNhatKy]` (Domain attribute) keeps a property's values out of the JSON. A change to that property alone still writes a `Sua` row with empty `{}` JSON, so the change isn't silent. `NguoiDung.MatKhauHash` (Story 1.8) must carry this attribute.
- **Edits never rewrite creation stamps.** On Modified, `NgayTao`/`NguoiTaoId` are reset to their original values.
- `NhatKyThaoTac.NguoiDungId` has no FK, per the Dev Notes recommendation (recorded on the configuration). `MayTram`/`TenBang` are nullable, as the column list gives no NOT NULL for them.
- No `EnableRetryOnFailure`, as recommended (a comment in `AddInfrastructure`). The `ExecuteUpdate`/raw-SQL bypass is documented on `AuditInterceptor`.
- `CurrentUserSession` lives in Infrastructure, not WinForms, so `AddInfrastructure` resolves on its own. The sign-in state is one immutable snapshot that is swapped atomically.
- `FakeClock` is in `tests/LuuKyCanTin.IntegrationTests/TestUtilities/`. Only that project needs it so far.

### File List

- `Directory.Packages.props` (modified)
- `src/Directory.Build.props` (new)
- `src/BannedSymbols.txt` (new)
- `src/Libraries/LuuKyCanTin.Domain/Common/IAuditable.cs`, `ICoTrangThaiHuy.cs`, `KhongGhiNhatKyAttribute.cs` (new)
- `src/Libraries/LuuKyCanTin.Domain/HeThong/HanhDong.cs`, `NhatKyThaoTac.cs` (new; `.gitkeep` removed)
- `src/Libraries/LuuKyCanTin.Application/Abstractions/IClock.cs`, `ICurrentUser.cs`, `IGhiNhatKy.cs` (new; `.gitkeep` removed)
- `src/Libraries/LuuKyCanTin.Infrastructure/Common/SystemClock.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/HeThong/CurrentUserSession.cs`, `NhatKyFactory.cs`, `GhiNhatKy.cs` (new; `.gitkeep` removed)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs`, `HanhDongNhatKy.cs` (new; renamed from `XacDinhHanhDong.cs` in review)
- `src/Libraries/LuuKyCanTin.Application/HeThong/VaiTroService.cs` (modified in review: flag `NgaySua` instead of the whole entity)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/HeThong/NhatKyThaoTacConfiguration.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/Common/EnumCheckExtensions.cs` (modified)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/AppDbContext.cs`, `DependencyInjection.cs` (modified)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/20261001151151_AddNhatKyThaoTac.cs`, `.Designer.cs` (new), `AppDbContextModelSnapshot.cs` (modified)
- `tests/LuuKyCanTin.IntegrationTests/TestUtilities/FakeClock.cs`, `FakeClockTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/Audit/AuditDatabaseFixture.cs`, `AuditInterceptorTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/Interceptors/HanhDongNhatKyTests.cs` (new; renamed from `Persistence/Audit/XacDinhHanhDongTests.cs` in review)
- `tests/LuuKyCanTin.IntegrationTests/HeThong/GhiNhatKyTests.cs`, `CurrentUserSessionTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Common/SystemClockTests.cs` (new; renamed from `HeThong/ClockTests.cs` in review)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/Configurations/HeThong/NhatKyThaoTacConfigurationTests.cs` (new; renamed from `Persistence/NhatKyThaoTacModelTests.cs` in review)
- `tests/LuuKyCanTin.IntegrationTests/Architecture/BannedApiTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/EnumChecks/EnumCheckVerifier.cs`, `EnumCheckVerifierTests.cs`, `EnumColumn.cs`, `EnumModel.cs`, `EnumModelTests.cs` (modified)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/InfrastructureRegistrationTests.cs`, `TestModel/MauChungTu.cs` (modified)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-01 | Implemented: IClock + banned-API analyzer, NhatKyThaoTac + migration, audit interceptor, IGhiNhatKy; status → review |
| 2026-10-03 | Code review: 3 decisions resolved, 4 patches applied (naming-convention rename, `TimeProvider.System` ban, new tests for an edit after cancellation and a retry after a failed save), detached-update guard, enum names in the audit JSON, 3 deferred; status → done |
