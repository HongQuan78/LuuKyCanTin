# Stories

One file per story, grouped by epic. They're planning artifacts only, so no source code lives here. Spike code goes in `spikes/` at the repo root (outside `LuuKyCanTin.slnx`), and spike outcomes go in `docs/decisions/`.

Source of the stories: `_bmad-output/planning-artifacts/epics/epic-NN-*.md`. Each story keeps its epic's acceptance criteria word for word, and adds tasks, dev notes and references for the developer.

## Epic 1: Foundation & first printed receipt (Sprint 0)

| Story | File | Size | Depends on | Status |
|---|---|---|---|---|
| 1.1 Solution skeleton, host and CI | [1-1-solution-skeleton-host-ci.md](epic-01/1-1-solution-skeleton-host-ci.md) | M | — | done |
| 1.2 Database migrations, schema-version check, enum ↔ CHECK test | [1-2-database-migrations-schema-version-enum-check.md](epic-01/1-2-database-migrations-schema-version-enum-check.md) | M | 1.1 | done |
| 1.3 IClock and automatic audit-log interceptor | [1-3-iclock-audit-log-interceptor.md](epic-01/1-3-iclock-audit-log-interceptor.md) | M | 1.2 | done |
| 1.4 Amount in Vietnamese words | [1-4-amount-in-vietnamese-words.md](epic-01/1-4-amount-in-vietnamese-words.md) | S | 1.1 | review |
| 1.5 Spike: QuestPDF printing with Vietnamese fonts | [1-5-spike-questpdf-vietnamese-printing.md](epic-01/1-5-spike-questpdf-vietnamese-printing.md) | M (spike) | 1.1 | done |
| 1.6 Spike: Velopack updates from a LAN shared folder | [1-6-spike-velopack-lan-updates.md](epic-01/1-6-spike-velopack-lan-updates.md) | M (spike) | 1.1 | done |
| 1.7 Spike: Vietnamese diacritic-insensitive incremental search | [1-7-spike-vietnamese-incremental-search.md](epic-01/1-7-spike-vietnamese-incremental-search.md) | S (spike) | 1.2 | done |
| 1.8 Walking skeleton: sign in, register a detainee, post and print one receipt | [1-8-walking-skeleton-first-printed-receipt.md](epic-01/1-8-walking-skeleton-first-printed-receipt.md) | L | 1.2, 1.3, 1.4, 1.5 | done |

**Suggested order:** 1.1, then 1.2, 1.4, 1.5 and 1.6 in parallel, then 1.3 and 1.7, then 1.8.

**Decisions to settle before 1.8:**

1. Can Application reference `Microsoft.EntityFrameworkCore` (core, no provider)? — **Settled: yes** (Epic 2.1 and the merge; the architecture test allows EF Core core and still forbids every provider). See 1.1 Dev Notes and the 1.8 T0 note.
2. Document number format `BNT-2026-00001` (epics.md Open Question 1).
3. 24 → "hai mươi bốn" or "hai mươi tư" (1.4, non-blocking).
4. Application may reference `Microsoft.Extensions.DependencyInjection.Abstractions` (for `AddApplication`) beyond AC 2's "plus FluentValidation". **Settled: accepted deviation** (1.1 code review, 2026-10-02).

**Sprint 0 gate:** CI green, spike notes merged, and DEC-02, DEC-05 and DEC-09 decided by the PO.

## Epic 2: Secure access & unit setup (R0.5)

| Story | File | Size | Depends on | Status |
|---|---|---|---|---|
| 2.1 Staff register | [2-1-staff-register.md](epic-02/2-1-staff-register.md) | S | 1.2, 1.3 | review |
| 2.2 Secure sign-in, password policy and lockout | [2-2-secure-sign-in-password-policy-lockout.md](epic-02/2-2-secure-sign-in-password-policy-lockout.md) | M | 1.8, 2.1 | review |
| 2.3 Roles and permission catalogue | [2-3-roles-permission-catalogue.md](epic-02/2-3-roles-permission-catalogue.md) | M | 2.2 | review |
| 2.4 User accounts linked to staff and role assignment | [2-4-user-accounts-role-assignment.md](epic-02/2-4-user-accounts-role-assignment.md) | M | 2.1, 2.3 | done |
| 2.5 Permission-driven shell and service-level authorization | [2-5-permission-driven-shell-service-authorization.md](epic-02/2-5-permission-driven-shell-service-authorization.md) | M | 2.3, 2.4 | done |
| 2.6 Segregation-of-duties policy | [2-6-segregation-of-duties-policy.md](epic-02/2-6-segregation-of-duties-policy.md) | S | 2.5 | done |
| 2.7 Session auto-lock | [2-7-session-auto-lock.md](epic-02/2-7-session-auto-lock.md) | S | 2.2 | done |
| 2.8 Unit information | [2-8-unit-information.md](epic-02/2-8-unit-information.md) | S | 2.5 | done |
| 2.9 Signatory configuration per print template | [2-9-signatory-configuration.md](epic-02/2-9-signatory-configuration.md) | M | 2.1, 2.8 | ready-for-dev |
| 2.10 Audit-log viewer | [2-10-audit-log-viewer.md](epic-02/2-10-audit-log-viewer.md) | M | 1.3, 2.5 | ready-for-dev |

**Suggested order:** 2.1 (can start before 1.8 is finished), then 2.2, then 2.3 and 2.7 in parallel, then 2.4, then 2.5, then 2.6, 2.8 and 2.9, then 2.10 last so its diff view meets every audit payload the epic writes.

**Cross-story decisions made in these files (no AC changed except A22):**

1. `IKiemTraQuyen` (the DB-backed permission check) and `KhongCoQuyenException` arrive in 2.3, not 2.5, because 2.4's AC already needs a service-level check. 2.5 adds the UI side, retrofits 2.1, and adds the bypass test.
2. `HanhDong` keeps its 6 DB-design values. Sign-in, sign-out, lockout and lock/unlock events are `DangNhap` rows, and a refused approval is a `Duyet` row, each with a `SuKien`/`KetQua` payload.
3. `IGhiNhatKy` gains a `duLieuCu` overload (2.3). Join tables (`VaiTroQuyen`, `NguoiDungVaiTro`) and `CauHinhKyTen` (replaced as a set) are logged explicitly, because the interceptor refuses deleted `IAuditable` rows.
4. Leadership gets `HT.Xem` so the audit log (2.10) is usable by its intended user (epics.md alignment A22).

**Epic 2 gate:** every seeded role signs in and sees only its own menus (2.5 theory test), the bypass test is green, and the unit header and the signers for all 12 templates are configured.

## Refactor

| Story | File | Size | Depends on | Status |
|---|---|---|---|---|
| R.1 Rename existing identifiers to English | [r-1-rename-identifiers-to-english.md](refactor/r-1-rename-identifiers-to-english.md) | L | 1.4, 2.1, 2.2, 2.3 | review |
| R.2 UI theme, shared controls, shell and sign-in aligned with the prototypes | [r-2-ui-theme-shell-sign-in.md](refactor/r-2-ui-theme-shell-sign-in.md) | L | R.1, 2.2 | review |
| R.3 Bring the existing screens into line with the UI prototypes | [r-3-align-existing-screens-with-prototypes.md](refactor/r-3-align-existing-screens-with-prototypes.md) | L | R.2, 1.8, 2.1, 2.3 | review |

**Why:** on 2026-10-03 the naming convention changed to English identifiers with Vietnamese user-visible text (`docs/conventions/naming-conventions.md`). R.1 renames the code written before that. **Suggested timing:** before 2.4, so the rest of Epic 2 is built on English names.

**Why R.2 and R.3:** the UI prototypes (`ux-designs/ux-TienGuiLuuKy-2026-10-02/`) became binding on 2026-10-02 (`docs/conventions/ui-prototype-conventions.md`), after the first screens were built. R.2 creates `AppTheme`, `CardPanel`, `InputFrame` and the sidebar shell and redesigns sign-in and change password. R.3 brings the other existing screens into line. Both are visual and interaction only; behaviour the prototypes show but no story has delivered yet stays with its own story. **Suggested timing:** R.2 before 2.4, so new screens start from the theme. Then 2.5 makes the R.2 sidebar its permission-driven menu, and 2.7 builds the lock overlay (key-01 D) on the R.2 shell.
