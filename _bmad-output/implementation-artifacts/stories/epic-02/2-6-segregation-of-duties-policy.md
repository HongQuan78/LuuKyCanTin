---
story: "2.6"
epic: 2
title: Segregation-of-duties policy
status: done
size: S
backlogItems: [NEN-09]
frsCovered: [FR3]
nfrsTouched: [NFR5, NFR6, NFR11]
dependsOn: ["2.5"]
baseline_commit: bdc00bb
---

# Story 2.6: Segregation-of-duties policy

Status: done

## Story

As a unit leader,
I want a rule that whoever created a request or document can never approve it,
So that a single person cannot both originate and authorize a movement of money.

## Acceptance Criteria

1. **Given** a reusable Application policy `KiemTraTachNhiemVu` that takes the creator's user id and the approver's user id
   **When** they are the same person (same user, or two accounts linked to the same `CanBoId`)
   **Then** it throws `ViPhamTachNhiemVuException` with "Người lập không được tự duyệt" (the creator cannot approve their own document)

2. **Given** unit tests
   **When** they run
   **Then** they cover same user, different user with the same staff member, different staff member, and the admin account with no staff member

3. **Given** the policy
   **When** later approval flows are built (transfer approval in Story 5.5, opening-balance approval in Story 7.5)
   **Then** they call this policy instead of re-implementing the check, and every refused attempt writes an audit row

## Tasks / Subtasks

- [x] **T1. Pure rule in Domain** (AC: 1, 2)
  - [x] Domain `HeThong/TachNhiemVu.cs`: `static bool LaCungNguoi(int nguoiLapId, int? canBoLapId, int nguoiDuyetId, int? canBoDuyetId)` → `true` when the user ids are equal, or when both staff ids are non-null and equal.
  - [x] Domain `HeThong/ViPhamTachNhiemVuException.cs` with the fixed message "Người lập không được tự duyệt". Make it derive from the business-error base (Story 2.1 T4), so the shell shows it as a business message (2.5 T4).
- [x] **T2. Application policy** (AC: 1, 3)
  - [x] Application `HeThong/KiemTraTachNhiemVu.cs` (registered scoped): `Task KiemTraAsync(int nguoiLapId, int nguoiDuyetId, string tenBang, long banGhiId, CancellationToken ct)`.
    1. Load `CanBoId` for both user ids in one query.
    2. Apply `TachNhiemVu.LaCungNguoi`.
    3. On violation, write the audit row (T3), then throw `ViPhamTachNhiemVuException`.
  - [x] Keep the signature in user ids, as the AC says. Callers pass the document's `NguoiTaoId` (or a dedicated creator column) and `ICurrentUser.NguoiDungId`.
  - [x] An unknown user id is a programming error: throw `InvalidOperationException`.
- [x] **T3. Audit row for a refused attempt** (AC: 3)
  - [x] Write via `IGhiNhatKy`: `HanhDong.Duyet`, `TenBang`/`BanGhiId` = the document being approved, payload `{ "KetQua": "TuChoi", "LyDo": "TachNhiemVu", "NguoiLapId": …, "NguoiDuyetId": … }`.
  - [x] **Ordering gotcha:** if the caller has already opened its approval transaction, throwing rolls back this audit row too. The contract (in the XML doc) is: **call `KiemTraAsync` before beginning the approval transaction**, right after the permission check. `IGhiNhatKy` saves immediately, so the row is committed on its own. Stories 5.5 and 7.5 follow that order, and their AC already refers to this policy.
- [x] **T4. Tests** (AC: 1–3)
  - [x] Domain unit tests (AC 2 exact cases, `[Theory]`):
    - same user id → violation;
    - different user ids, same `CanBoId` → violation;
    - different users, different staff → allowed;
    - `admin` (no `CanBoId`) approving its own → violation;
    - `admin` approving another user's document → allowed by *this* policy. Alignment A17 forbids an account without staff from approving at all, but that's Story 5.5's rule, so note it in the test name.
    - both `CanBoId` null and different users → allowed.
  - [x] Application unit test (NSubstitute `IGhiNhatKy`): a violation writes exactly one `Duyet` row with `KetQua = TuChoi` *before* throwing; an allowed case writes nothing.
  - [x] Integration: with two accounts linked to the same staff member (one inactive, so the 2.4 index allows it), the policy refuses, and the audit row survives because no outer transaction is open.

## Dev Notes

### Current codebase state (after 2.5)

- `ICurrentUser.CanBoId` (2.4), `IKiemTraQuyen` and the friendly business-error display (2.5), and `IGhiNhatKy` with the before/after overload (2.3).

### Design notes

- **"Same person" means the same staff member**, not just the same login. Story 2.4 already allows only one *active* account per person, but an old inactive account could have created the document, so compare `CanBoId` as well.
- **No consumer in this epic.** The first callers are 5.5 (transfer approval) and 7.5 (opening balances), with more in Epic 9 (opening stock, 9.6) and 14.12. That's why it's a small reusable policy plus tests, with no UI.
- KISS: one method, no generic "policy framework".

### Out of scope

- The approval flows themselves. The rule that an account without a staff record can't approve (5.5, alignment A17).

### References

- Epic 2 › Story 2.6; `epics.md` › FR3, Backlog Alignment A17
- Backlog: NEN-09 "Người lập không được tự duyệt; ghi nhật ký mọi thao tác quản trị tài khoản"
- Epic 5 › Story 5.5 and Epic 7 › Story 7.5, which reference this policy

## Dev Agent Record

### Agent Model Used

opencode / deepseek-v4.1-flash

### Debug Log References

- `dotnet build LuuKyCanTin.slnx` → 0 errors (only the pre-existing WebView2 spike MSB3277 warning).
- `dotnet test LuuKyCanTin.slnx` → 758 passed, 0 failed, 0 skipped (Domain 91, Application 146, WinForms 266, Integration 255).
- `dotnet ef migrations has-pending-model-changes --project src/Libraries/LuuKyCanTin.Infrastructure` → no changes; the story needs no migration.

### Completion Notes List

**Translation table (story text → code, per `docs/conventions/naming-conventions.md`)**

| Story (pre-R.1 Vietnamese) | Implemented English |
|---|---|
| `TachNhiemVu.LaCungNguoi` | `SeparationOfDuties.IsSamePerson` |
| `ViPhamTachNhiemVuException` | `SeparationOfDutiesViolationException` |
| `KiemTraTachNhiemVu` / `KiemTraAsync` | `SeparationOfDutiesPolicy.EnsureCanApproveAsync` |
| `HeThong/` | `Administration/` |
| `CanBoId` | `OfficerId` |
| `IGhiNhatKy` | `IAuditLogWriter` (existing from 2.3, not renamed) |
| `HanhDong.Duyet` | `AuditAction.Approve` |
| `TenBang` / `BanGhiId` | `tableName` / `recordId` |
| `KetQua = TuChoi` / `LyDo = TachNhiemVu` | `Result` / `Reason` keys with `SeparationOfDutiesPolicy.RejectedResult` = `"TuChoi"` and `SeparationOfDutiesReason` = `"TachNhiemVu"` |

**Base-class deviation (recorded on purpose).** `BusinessRuleException` lives in `Application.Common` (Story 2.1 T4), and `ProjectReferenceRules` forbids Domain → Application, so a Domain exception cannot derive from it. `SeparationOfDutiesViolationException` therefore derives from `Exception`, and `GlobalExceptionHandler.BusinessError` recognizes it next to `PermissionDeniedException` (the same existing pattern for Domain business errors), so the shell shows the fixed message as a business warning. Moving `BusinessRuleException` into Domain would satisfy the literal mapping but is a repo-wide refactor outside this story.

**What changed**

- Domain `SeparationOfDuties.IsSamePerson` is the pure rule: same user id, or both staff ids non-null and equal. No dependencies.
- Domain `SeparationOfDutiesViolationException` carries the fixed `ViolationMessage` = "Người lập không được tự duyệt." (with the period).
- `IAppDbContext` now exposes `DbSet<User> User` (it previously had no User set); `AppDbContext` already had the property, and the Application test context implements it. The policy loads both `OfficerId`s in one query, and an unknown user id throws `InvalidOperationException` before anything is written.
- `SeparationOfDutiesPolicy` (registered scoped) writes one `AuditAction.Approve` row through `IAuditLogWriter` with payload `{ Result, Reason, CreatorUserId, ApproverUserId }` and then throws. Its XML doc states the contract: call it before beginning the approval transaction, right after the permission check, so the refusal row commits on its own.
- Tests: Domain `[Theory]` covers the six AC pairs, including the admin case whose name says Story 5.5 owns the no-staff approver rule; the Application unit tests assert one rejection row with the payload for a violation, nothing for an allowed case, and `InvalidOperationException` with no row for an unknown user; the `[SqlServerFact]` integration test creates one inactive and one active account for the same officer (2.4 filtered index), proves the refusal, and reads the audit row from a fresh context (no outer transaction).
- No model change, so no migration.

### File List

**Source**
- src/Libraries/LuuKyCanTin.Domain/Administration/SeparationOfDuties.cs (new)
- src/Libraries/LuuKyCanTin.Domain/Administration/SeparationOfDutiesViolationException.cs (new)
- src/Libraries/LuuKyCanTin.Application/Administration/SeparationOfDutiesPolicy.cs (new)
- src/Libraries/LuuKyCanTin.Application/Abstractions/IAppDbContext.cs
- src/Libraries/LuuKyCanTin.Application/ApplicationServiceCollectionExtensions.cs
- src/Presentation/LuuKyCanTin.WinForms/Common/GlobalExceptionHandler.cs

**Tests**
- tests/LuuKyCanTin.Domain.UnitTests/Administration/SeparationOfDutiesTests.cs (new)
- tests/LuuKyCanTin.Application.UnitTests/Administration/SeparationOfDutiesPolicyTests.cs (new)
- tests/LuuKyCanTin.Application.UnitTests/TestUtilities/InMemoryAppDbContext.cs
- tests/LuuKyCanTin.IntegrationTests/Administration/SeparationOfDutiesPolicyTests.cs (new)
- tests/LuuKyCanTin.WinForms.UnitTests/Common/GlobalExceptionHandlerTests.cs

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | Change Log says "status stays in-progress" while the diff moves it to review | false | The presentation step owns the final transition; the Change Log is corrected there. |
| 2 | Change Log wording conflates the baseline with the change and uses an abbreviated SHA | low — patch (docs) | Metadata only; record the full baseline/commit hashes in the presentation step. |
| 3 | T1 checked although the exception does not derive from the business-error base | false | Domain cannot reference Application's `BusinessRuleException`; the exception follows the existing Domain `PermissionDeniedException` precedent and `GlobalExceptionHandler` recognizes it. Recorded in the Dev Agent Record. |
| 4 | AC quotes the message without a period; the constant adds one | low — patch | Keep the period (sibling messages end with one), pin the literal in the application test, and note the reconciliation in the Dev Agent Record. |
| 5 | Two English names for one concept (`SeparationOfDuties` vs `SegregationOfDutiesReason`) and no glossary entry | low — patch | Rename the constant to `SeparationOfDutiesReason`; glossary/category rows are a rules-file fix, deferred. |
| 6 | The `Policy` suffix/interface pattern is not in the naming-conventions table | low — defer | Rules-file fix; recorded in `deferred-work.md`. |
| 7 | `IAppDbContext.User` exposes the full `User` entity | false | Consistent with the DbSet-per-entity design; Application already reads the entity through `IUserStore` and the hash carries `[NotAudited]`. |
| 8 | The "call before the transaction" contract is documented but not enforced | medium — patch | Add a `HasActiveTransaction` check on `IAppDbContext` and throw `InvalidOperationException` when a transaction is already open; add a test. |
| 9 | The XML contract omits that `IAuditLogWriter` also saves pending tracked changes | low — patch | Document that callers must have no pending changes when calling the policy. |
| 10 | The integration test never signs in, so the refusal row records a null user | low — patch | Sign the fixture user in before the call and assert the audit row's user id. |
| 11 | Application tests assert anonymous-type `ToString()` substrings instead of the serialized JSON | low — patch | Capture the payload argument and assert the serialized JSON. |
| 12 | The domain theory omits the asymmetric null-staff case and splits one case into a fact | low — patch | Add the asymmetric case to the theory. |
| 13 | `IsSamePerson` XML docs omit the two user-id parameters | low — patch | Add the missing `<param>` entries. |
| 14 | The shell warning for this exception is unpinned, and `BusinessError` is a hand-maintained type list | low — patch (message test) / defer (marker interface) | Add a `GlobalExceptionHandler` test asserting the violation message in the warning hook; a Domain marker interface is a separate refactor, recorded in `deferred-work.md`. |
| 15 | `tableName`/`recordId` are unvalidated, so an unreadable audit row can be written | low — patch | Guard with `ArgumentException.ThrowIfNullOrWhiteSpace(tableName)` and a positive record id. |
| 16 | A refusal is stored as `AuditAction.Approve`, discriminated only by the payload | false | Prescribed by T3; the audit viewer (2.10) filters by payload by design. |
| 17 | The refusal message literal is never pinned (all assertions compare the constant to itself) | low — patch | Assert `"Người lập không được tự duyệt."` literally once in the application tests. |
| 18 | The stored `"TuChoi"`/`"TachNhiemVu"` values are never pinned literally | low — patch | Assert both literals in the integration test, like `EnumModelTests` pins audit action codes. |

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
| 2026-10-03 | Implemented on baseline `bdc00bb`: Domain rule + violation exception, scoped Application policy with refusal audit row, shell recognizes the exception as a business warning, tests at all three levels. |
| 2026-10-03 | Reviewed (blind hunter, edge cases, verification gaps) and patched: literal message/value pins, transaction-order and argument guards, JSON payload assertions, signed-in integration case; status → done. |
