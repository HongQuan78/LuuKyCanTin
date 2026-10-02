---
story: "2.6"
epic: 2
title: Segregation-of-duties policy
status: ready-for-dev
size: S
backlogItems: [NEN-09]
frsCovered: [FR3]
nfrsTouched: [NFR5, NFR6, NFR11]
dependsOn: ["2.5"]
---

# Story 2.6: Segregation-of-duties policy

Status: ready-for-dev

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

- [ ] **T1. Pure rule in Domain** (AC: 1, 2)
  - [ ] Domain `HeThong/TachNhiemVu.cs`: `static bool LaCungNguoi(int nguoiLapId, int? canBoLapId, int nguoiDuyetId, int? canBoDuyetId)` → `true` when the user ids are equal, or when both staff ids are non-null and equal.
  - [ ] Domain `HeThong/ViPhamTachNhiemVuException.cs` with the fixed message "Người lập không được tự duyệt". Make it derive from the business-error base (Story 2.1 T4), so the shell shows it as a business message (2.5 T4).
- [ ] **T2. Application policy** (AC: 1, 3)
  - [ ] Application `HeThong/KiemTraTachNhiemVu.cs` (registered scoped): `Task KiemTraAsync(int nguoiLapId, int nguoiDuyetId, string tenBang, long banGhiId, CancellationToken ct)`.
    1. Load `CanBoId` for both user ids in one query.
    2. Apply `TachNhiemVu.LaCungNguoi`.
    3. On violation, write the audit row (T3), then throw `ViPhamTachNhiemVuException`.
  - [ ] Keep the signature in user ids, as the AC says. Callers pass the document's `NguoiTaoId` (or a dedicated creator column) and `ICurrentUser.NguoiDungId`.
  - [ ] An unknown user id is a programming error: throw `InvalidOperationException`.
- [ ] **T3. Audit row for a refused attempt** (AC: 3)
  - [ ] Write via `IGhiNhatKy`: `HanhDong.Duyet`, `TenBang`/`BanGhiId` = the document being approved, payload `{ "KetQua": "TuChoi", "LyDo": "TachNhiemVu", "NguoiLapId": …, "NguoiDuyetId": … }`.
  - [ ] **Ordering gotcha:** if the caller has already opened its approval transaction, throwing rolls back this audit row too. The contract (in the XML doc) is: **call `KiemTraAsync` before beginning the approval transaction**, right after the permission check. `IGhiNhatKy` saves immediately, so the row is committed on its own. Stories 5.5 and 7.5 follow that order, and their AC already refers to this policy.
- [ ] **T4. Tests** (AC: 1–3)
  - [ ] Domain unit tests (AC 2 exact cases, `[Theory]`):
    - same user id → violation;
    - different user ids, same `CanBoId` → violation;
    - different users, different staff → allowed;
    - `admin` (no `CanBoId`) approving its own → violation;
    - `admin` approving another user's document → allowed by *this* policy. Alignment A17 forbids an account without staff from approving at all, but that's Story 5.5's rule, so note it in the test name.
    - both `CanBoId` null and different users → allowed.
  - [ ] Application unit test (NSubstitute `IGhiNhatKy`): a violation writes exactly one `Duyet` row with `KetQua = TuChoi` *before* throwing; an allowed case writes nothing.
  - [ ] Integration: with two accounts linked to the same staff member (one inactive, so the 2.4 index allows it), the policy refuses, and the audit row survives because no outer transaction is open.

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
