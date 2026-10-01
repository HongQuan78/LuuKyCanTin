---
epic: 3
title: Detainee register
release: R0.5
frsCovered: [FR14, FR15, FR22]
backlogItems: [DM-01, NEN-23, DM-02, DM-09, NEN-02]
dependsOn: [1, 2]
---

## Epic 3: Detainee register

Staff keep an accurate register of detainees (*đối tượng*): code, name, type, entry date, cell, warden and status. They find anyone in a second by typing part of a code or name, with or without diacritics. They record the change from temporary detention to convicted prisoner without rewriting history, and they load the initial list from Excel so the one-month dry-run can start with real data.

**Exit criteria:** the full detainee list of the unit can be imported and searched; the shared detainee picker is ready for every money form in Epics 4–5.

**Applies to every story in this epic:** writes require `DM.Them` / `DM.Sua` checked in the service; every change is audited by the interceptor; detainees are never hard-deleted.

---

### Story 3.1: Manage detainees

`DM-01, NEN-02` · Size M · Depends on: 1.8, 2.1, 2.5 · FR14

As a custodial officer,
I want to add and update detainee records with all the fields the forms need,
So that every receipt, payout and report names the right person with the right details.

**Acceptance Criteria:**

**Given** the `DoiTuong` table from Story 1.8 extended with `CanBoQuanGiaoId` (FK → `CanBo`) and an index on `HoTen`
**When** a user with `DM.Them` adds a detainee with `MaSo`, `HoTen`, `NamSinh`, `LoaiDoiTuong` (1 tạm giữ/tạm giam, 2 phạm nhân), `NgayVao`, `BuongGiam` and a warden
**Then** the detainee is saved with `TrangThai = 1` (đang quản lý) and `SoDuLuuKy = 0`

**Given** the warden selector
**When** it is opened
**Then** it lists only staff with `LaQuanGiao = 1` and `DangCongTac = 1`

**Given** an existing `MaSo`
**When** another detainee is saved with the same code
**Then** it is rejected with "Mã số đã tồn tại" (code already exists)

**Given** the validator
**When** a detainee is saved
**Then** `MaSo`, `HoTen`, `LoaiDoiTuong` and `NgayVao` are required; `NgayVao` cannot be after `IClock.Today`; `NamSinh` is plausible (1900 to the current year)

**Given** the edit form
**When** a user opens an existing detainee
**Then** `TrangThai`, `NgayRa` and `SoDuLuuKy` are read-only; status 2 (đã chuyển trại) and 3 (đã chấp hành xong án) are set only by settlement (Story 5.6), and the DB keeps `NgayRa` required when `TrangThai ≠ 1`
**And** `LoaiDoiTuong` is read-only here and changes only through Story 3.3

**Given** two users edit the same detainee
**When** the second one saves
**Then** the `RowVer` conflict is detected and the user is asked to reload

**Given** the dev/demo seed command
**When** it runs on a dev database
**Then** it creates 200 realistic detainees across both types, several cells and wardens (it never runs in production)

---

### Story 3.2: Find a detainee fast (shared detainee picker)

`NEN-23 (part)` · Size M · Depends on: 1.7, 3.1 · FR14 · UX-DR2

As a custodial officer,
I want one search box that finds a detainee by code or by name without diacritics as I type,
So that I pick the right person quickly and never confuse namesakes.

**Acceptance Criteria:**

**Given** the reusable WinForms control `DoiTuongPicker` and the query service from the SP-03 outcome
**When** the user types "nguyen van a" or "NGUYỄN VĂN A" or part of a `MaSo`
**Then** matches appear after a ~300 ms debounce, top 50, ranked by exact code match first, then name

**Given** the result list
**When** it is displayed
**Then** each row shows `MaSo`, `HoTen`, `NamSinh`, `BuongGiam` and type, so two people with the same name can be told apart
**And** detainees with `TrangThai ≠ 1` are shown with a red label "Đã chuyển trại" (transferred) or "Đã chấp hành xong án" (sentence completed)

**Given** a picker configured with "only active detainees" (the default for money forms)
**When** the user tries to select an inactive detainee
**Then** the selection is refused with a clear message (the engine re-checks in Story 4.3)

**Given** the keyboard
**When** the user presses ↑/↓ and Enter
**Then** they can select a result without the mouse, and Esc clears the search

**Given** the detainee list screen
**When** it uses the same picker query
**Then** the list supports the same search plus filters by type, status, cell and warden

---

### Story 3.3: Change detainee type with history

`DM-02` · Size M · Depends on: 3.1 · FR15

As a custodial officer,
I want to record when a temporary detainee becomes a convicted prisoner, with the date and sentence reference,
So that new documents show the new type while old documents keep the type they had.

**Acceptance Criteria:**

**Given** the `LichSuLoaiDoiTuong` table (`DoiTuongId` FK, `LoaiDoiTuong`, `TuNgay date`, UQ (`DoiTuongId`, `TuNgay`), `GhiChu nvarchar(300)` e.g. sentence number)
**When** a user with `DM.Sua` changes a detainee from type 1 (tạm giữ/tạm giam) to type 2 (phạm nhân) with an effective date and a note
**Then** in one transaction a history row is inserted and `DoiTuong.LoaiDoiTuong` is updated, and the change is audited

**Given** a detainee who is already a prisoner
**When** someone tries to change them back to type 1
**Then** the change is refused (only detention → prisoner is allowed)

**Given** an effective date before the detainee's `NgayVao` or after `IClock.Today`
**When** the change is saved
**Then** validation rejects it

**Given** documents created before the change
**When** they are viewed or reprinted
**Then** they still show the type stored in their snapshot (`ChungTuLuuKy.LoaiDoiTuong`), never the current type

**Given** the detainee detail screen
**When** it is opened
**Then** it shows the type history (date, type, note, who recorded it)

---

### Story 3.4: Import detainees from Excel

`DM-09 (part)` · Size M · Depends on: 3.1 · FR22 · UX-DR9

As an administrator,
I want to load the current detainee list from an Excel file with a preview of every error before anything is saved,
So that go-live and the dry-run start from the real list without retyping it.

**Acceptance Criteria:**

**Given** the import screen
**When** the user clicks "Tải file mẫu" (download template)
**Then** a ClosedXML `.xlsx` template is saved with the headers `MaSo, HoTen, NamSinh, LoaiDoiTuong, NgayVao, BuongGiam, MaCanBoQuanGiao` and one example row

**Given** a filled file
**When** the user selects it
**Then** a preview grid shows every row with a status, and each invalid row lists its errors: missing required field, duplicate `MaSo` within the file or already in the DB, unknown or non-warden `MaCanBoQuanGiao`, invalid type, invalid or future date

**Given** a preview with at least one error
**When** the user looks at the Import button
**Then** it is disabled until the file is fixed and reloaded; the user can export the error list to Excel

**Given** a preview with no errors
**When** the user confirms the import
**Then** all rows are inserted in one transaction (all-or-nothing) with `TrangThai = 1`, `SoDuLuuKy = 0`, one audit summary row (file name, row count) plus the interceptor rows
**And** opening balances are not imported here; they go through the ledger engine in Story 7.5

**Given** a 5,000-row file
**When** it is validated and imported
**Then** the preview appears in under 10 seconds and the import completes in under 30 seconds

**Given** a user without `DM.Them`
**When** they try to import
**Then** the service refuses
