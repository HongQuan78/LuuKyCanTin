# Stories

One file per story, grouped by epic. They're planning artifacts only, so no source code lives here. Spike code goes in `spikes/` at the repo root (outside `LuuKyCanTin.slnx`), and spike outcomes go in `docs/decisions/`.

Source of the stories: `_bmad-output/planning-artifacts/epics/epic-NN-*.md`. Each story keeps its epic's acceptance criteria word for word, and adds tasks, dev notes and references for the developer.

## Epic 1: Foundation & first printed receipt (Sprint 0)

| Story | File | Size | Depends on | Status |
|---|---|---|---|---|
| 1.1 Solution skeleton, host and CI | [1-1-solution-skeleton-host-ci.md](epic-01/1-1-solution-skeleton-host-ci.md) | M | — | review |
| 1.2 Database migrations, schema-version check, enum ↔ CHECK test | [1-2-database-migrations-schema-version-enum-check.md](epic-01/1-2-database-migrations-schema-version-enum-check.md) | M | 1.1 | review |
| 1.3 IClock and automatic audit-log interceptor | [1-3-iclock-audit-log-interceptor.md](epic-01/1-3-iclock-audit-log-interceptor.md) | M | 1.2 | review |
| 1.4 Amount in Vietnamese words | [1-4-amount-in-vietnamese-words.md](epic-01/1-4-amount-in-vietnamese-words.md) | S | 1.1 | review |
| 1.5 Spike: QuestPDF printing with Vietnamese fonts | [1-5-spike-questpdf-vietnamese-printing.md](epic-01/1-5-spike-questpdf-vietnamese-printing.md) | M (spike) | 1.1 | ready-for-dev |
| 1.6 Spike: Velopack updates from a LAN shared folder | [1-6-spike-velopack-lan-updates.md](epic-01/1-6-spike-velopack-lan-updates.md) | M (spike) | 1.1 | ready-for-dev |
| 1.7 Spike: Vietnamese diacritic-insensitive incremental search | [1-7-spike-vietnamese-incremental-search.md](epic-01/1-7-spike-vietnamese-incremental-search.md) | S (spike) | 1.2 | ready-for-dev |
| 1.8 Walking skeleton: sign in, register a detainee, post and print one receipt | [1-8-walking-skeleton-first-printed-receipt.md](epic-01/1-8-walking-skeleton-first-printed-receipt.md) | L | 1.2, 1.3, 1.4, 1.5 | ready-for-dev |

**Suggested order:** 1.1, then 1.2, 1.4, 1.5 and 1.6 in parallel, then 1.3 and 1.7, then 1.8.

**Decisions to settle before 1.8:**

1. Can Application reference `Microsoft.EntityFrameworkCore` (core, no provider)? See 1.1 Dev Notes.
2. Document number format `BNT-2026-00001` (epics.md Open Question 1).
3. 24 → "hai mươi bốn" or "hai mươi tư" (1.4, non-blocking).

**Sprint 0 gate:** CI green, spike notes merged, and DEC-02, DEC-05 and DEC-09 decided by the PO.
