---
story: "1.4"
epic: 1
title: Amount in Vietnamese words
status: review
size: S
backlogItems: [LK-T03]
frsCovered: [FR25]
nfrsTouched: [NFR1, NFR8]
dependsOn: ["1.1"]
---

# Story 1.4: Amount in Vietnamese words

Status: review

## Story

As a custodial officer,
I want every printed amount also written out correctly in Vietnamese words,
So that receipts and reports match the legal paper forms and cannot be misread.

## Acceptance Criteria

1. **Given** the Domain class `SoTienBangChu`
   **When** it converts an integer number of đồng
   **Then** the result starts with a capital letter and ends with "đồng" (e.g. 500000 → "Năm trăm nghìn đồng")
   **And** it has no dependency outside Domain

2. **Given** the boundary table test
   **When** it runs
   **Then** these cases pass: 0 → "Không đồng"; 10 → "Mười đồng"; 15 → "Mười lăm đồng"; 21 → "Hai mươi mốt đồng"; 101 → "Một trăm lẻ một đồng"; 105 → "Một trăm lẻ năm đồng"; 1,000,005 → "Một triệu không trăm lẻ năm đồng"; 25 → "Hai mươi lăm đồng"; 11 → "Mười một đồng"; 1,000,000,000 → "Một tỷ đồng"; and amounts above one billion with groups of zeros

3. **Given** the style choice "lẻ" vs "linh"
   **When** the converter is configured
   **Then** "lẻ" is the default and the alternative is one setting, covered by a test

4. **Given** a negative amount
   **When** it is converted
   **Then** an `ArgumentOutOfRangeException` is thrown (money is never negative)

## Tasks / Subtasks

- [x] **T1. Write the table test first** (AC: 2, 3, 4)
  - [x] `tests/LuuKyCanTin.Domain.UnitTests/Common/SoTienBangChuTests.cs` with `[Theory]` + `[InlineData]` for every AC 2 case, plus the extra cases below. Shouldly: `result.ShouldBe(expected)`.
  - [x] Extra cases worth pinning (so the dev confirms the reading rules with the PO if one looks wrong):

    | Input | Expected |
    |---|---|
    | 1 | Một đồng |
    | 5 | Năm đồng |
    | 14 | Mười bốn đồng |
    | 20 | Hai mươi đồng |
    | 24 | Hai mươi bốn đồng (see the open question) |
    | 31 | Ba mươi mốt đồng |
    | 55 | Năm mươi lăm đồng |
    | 100 | Một trăm đồng |
    | 110 | Một trăm mười đồng |
    | 115 | Một trăm mười lăm đồng |
    | 1,000 | Một nghìn đồng |
    | 1,005 | Một nghìn không trăm lẻ năm đồng |
    | 1,050 | Một nghìn không trăm năm mươi đồng |
    | 10,000 | Mười nghìn đồng |
    | 21,000 | Hai mươi mốt nghìn đồng |
    | 105,000 | Một trăm lẻ năm nghìn đồng |
    | 500,000 | Năm trăm nghìn đồng |
    | 1,000,000 | Một triệu đồng |
    | 1,200,500 | Một triệu hai trăm nghìn năm trăm đồng |
    | 1,000,000,005 | Một tỷ không trăm lẻ năm đồng |
    | 2,000,001,000 | Hai tỷ không trăm lẻ một nghìn đồng |
    | 1,000,000,000,000 | Một nghìn tỷ đồng |
    | 999,999,999,999,999,999 | Chín trăm chín mươi chín triệu … tỷ (max `decimal(18,0)`, no exception) |

  - [x] Linh style: 101 → "Một trăm linh một đồng"; 1,000,005 → "Một triệu không trăm linh năm đồng".
  - [x] Negative input throws `ArgumentOutOfRangeException`. A fractional `decimal` (e.g. 10.5m) also throws `ArgumentOutOfRangeException`, because money is integer đồng.
- [x] **T2. Implement `SoTienBangChu`** (AC: 1, 3)
  - [x] Location: `src/Libraries/LuuKyCanTin.Domain/Common/SoTienBangChu.cs` (the tech stack doc puts it in `Common/`).
  - [x] Public API (keep it small): `public static string Doc(decimal soTien, KieuDocLe kieu = KieuDocLe.Le)`. Money is `decimal` everywhere in the model (`decimal(18,0)`), so accept `decimal`, not `long`, to save every caller a cast.
  - [x] `public enum KieuDocLe : byte { Le = 1, Linh = 2 }` in Domain `Common/`. Wiring it to `appsettings.json` happens later, when the print frame needs it (Epic 4). This story only has to make it one parameter.
  - [x] Algorithm, by 3-digit groups from the right, with units `"" / nghìn / triệu / tỷ` repeating after `tỷ` (nghìn tỷ, triệu tỷ, tỷ tỷ):
    - Skip a group equal to 000, unless the whole number is 0.
    - The **most significant** group reads without leading zeros (21 → "hai mươi mốt"). **Every other non-zero group** always reads its hundreds, using "không trăm" when the hundreds digit is 0.
    - Tens: 0 with a non-zero unit after a read hundreds digit gives `lẻ`/`linh` + unit; 1 gives `mười`; 2–9 give digit + `mươi`.
    - Units: 1 after tens ≥ 2 gives `mốt` (but `một` after `mười` and after `lẻ`); 5 after tens ≥ 1 gives `lăm` (but `năm` after `lẻ` and standalone); 4 after tens ≥ 2 gives `bốn` (see the open question); 0 gives nothing.
    - Join with single spaces, capitalize the first letter, and append " đồng".
  - [x] No `CultureInfo`-dependent casing surprises. Capitalize with `char.ToUpperInvariant` on the first character (all first letters are plain Latin or `Đ`/`Ă`-style precomposed characters, which `ToUpperInvariant` handles).
- [x] **T3. Verify** (AC: 1)
  - [x] `dotnet test --filter "FullyQualifiedName~SoTienBangChuTests"` passes. The Story 1.1 architecture test still shows Domain with no references.

## Dev Notes

### Current codebase state

- Domain is empty apart from the module folders (Story 1.1). Nothing else uses `SoTienBangChu` yet. Story 1.8 calls it to fill `ChungTuLuuKy.SoTienBangChu` at posting time, and every print template uses the stored text afterwards.

### Rules and design

- **Domain only**: no NuGet packages, no `IClock`, no I/O. It's a pure function, so this story is a natural first TDD exercise for the team.
- The result is **stored** (`ChungTuLuuKy.SoTienBangChu nvarchar(300)`, `BangKeNop.TongTienBangChu`) when a document is posted, so a reprint matches the original even if the converter changes later. That's why a style switch is safe.
- Check the length: the max value produces well under 300 characters. Add an assertion test that the max-value output length is ≤ 300, to protect the column size.

### Open question for the PO (not blocking)

- **24 → "hai mươi bốn" or "hai mươi tư"?** Both are in use, and the AC doesn't pin it. The default here is **"bốn"** (always unambiguous). If the paper forms or the accountant's habit say "tư", make it part of the same style setting. Record the PO's answer in the Completion Notes.
- **"nghìn" vs "ngàn"**: the AC example uses "nghìn" (northern, the official usage). Keep "nghìn".

### Gotchas

- `mốt` applies only when the tens digit is ≥ 2: 11 is "mười một", 101 is "một trăm lẻ một".
- `lăm` applies when the tens digit is ≥ 1, including "mười lăm": 105 is "một trăm lẻ **năm**", not "lẻ lăm".
- Zero groups in the middle are skipped entirely, but the next group still reads "không trăm…": 1,000,005 is "Một triệu **không trăm lẻ năm**".
- Repeated `tỷ`: 10^12 is "một nghìn tỷ", **not** "một nghìn tỷ tỷ". Recurse on the `tỷ` quotient rather than extending the unit list blindly.

### Out of scope

- Wiring the `lẻ`/`linh` setting into configuration (Epic 4 print frame). UI money input (UX-DR1, Epic 4).

### Testing

- Domain unit tests only. No mocks needed.

### References

- Epic 1 › Story 1.4; `epics.md` › FR25 (LK-T03 edge cases)
- Tech stack PDF: "Số tiền bằng chữ, tự viết trong Domain"
- DB design PDF: `ChungTuLuuKy.SoTienBangChu nvarchar(300)` (p.10)

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5[1m])

### Debug Log References

- Red: test project failed to compile (`SoTienBangChu`, `KieuDocLe` missing). Green: 42/42 Domain tests. Full suite: 212 passed.
- Integration tests first failed with a SQL timeout because the WSL VM had stopped. They passed after `docker compose up -d`. This had nothing to do with the story.

### Completion Notes List

- `SoTienBangChu.Doc(decimal, KieuDocLe = Le)` reads by 3-digit groups (triệu / nghìn / units) and recurses on the `tỷ` quotient, so 10^12 reads "Một nghìn tỷ". It takes the remainder first and then divides exactly, so very large decimals never round.
- Test inputs are `long` and `decimal` literals, not parsed strings, because `decimal.Parse` under vi-VN reads "10.5" as 105.
- Negative and fractional amounts throw `ArgumentOutOfRangeException`. A trailing-zero scale (10.00m) is accepted.
- Max `decimal(18,0)` reads every group and is pinned. A test checks that its length fits `nvarchar(300)`.
- Open PO question still pending: 24 reads "hai mươi bốn" by default, not "tư". "nghìn" is kept.

### File List

- `src/Libraries/LuuKyCanTin.Domain/Common/SoTienBangChu.cs` (new)
- `src/Libraries/LuuKyCanTin.Domain/Common/KieuDocLe.cs` (new)
- `tests/LuuKyCanTin.Domain.UnitTests/Common/SoTienBangChuTests.cs` (new)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-01 | Implemented `SoTienBangChu` and `KieuDocLe` with the table tests; status set to review |
