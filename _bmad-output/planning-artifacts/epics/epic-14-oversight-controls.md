---
epic: 14
title: Oversight & extended controls
release: R2
frsCovered: [FR68, FR71, FR73, FR74, FR75, FR76, FR77, FR78, FR80, FR81, FR82, FR83, FR84, FR85, FR87]
backlogItems: [TI-04, TI-07, TI-09, TI-10, TI-11, TI-12, TI-13, TI-14, TI-16, TI-17, TI-18, TI-19, TI-20, TI-21, TI-23]
dependsOn: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
---

## Epic 14: Oversight & extended controls

Once the system runs for real, leadership gets stronger supervision: a dashboard of pending work, monitoring of transfers and unusual canteen purchases, approval of goods receipts, and tamper detection on the custodial ledger. Operations tighten too: stock counts and adjustments, cancel-and-replace, an optional spending limit, and archived PDFs of every printed document. Administrators gain a safe training mode and a support bundle that works without Internet.

**Exit criteria (R2 gate for this epic):** DEC-07 is decided (14.7 assumes stock count and adjustment are wanted). DEC-06 keeps the spending limit off by default (14.6). Ledger and stock reconciliation (6.7, 11.4), including the hash-chain check from 14.13, show zero differences on the R2 demo scenario.

**Applies to every story in this epic:** services re-check permissions (2.5); creators never approve their own documents (2.6); every write and every approval goes through the audit log (1.3); any new enum value ships with its DB CHECK and passes the enum ↔ CHECK test (1.2); balances change only through `GhiSoLuuKyService` (4.3) and stock only through the `TheKho` posting path (9.1).

---

### Story 14.1: Commander dashboard

`TI-04` · Size M · Depends on: 5.5, 4.7, 7.2 · FR68

As a commander or unit leader,
I want a dashboard of what is waiting for me and what looks stuck,
So that approvals don't pile up and unfinished documents are noticed.

**Acceptance Criteria:**

**Given** a user with permission `TI-04.Xem`
**When** the dashboard opens (optionally as the start page for that role)
**Then** it shows transfer requests (`DeNghiChoTien`) with status pending and their age, draft documents (receipts, payouts, goods receipts, sales) not posted for more than N days (default 2), and the last successful backup time and verification result (7.2)

**Given** a pending transfer request on the dashboard
**When** the commander opens it
**Then** the approval form from 5.5 opens, and the segregation-of-duties check (2.6) still applies

**Given** the data changes on another workstation
**When** the user clicks Refresh, or 5 minutes pass
**Then** the counts are reloaded

---

### Story 14.2: Training mode

`TI-07` · Size M · Depends on: 1.2, 1.3 · FR71

As an administrator,
I want a training mode that uses a separate sandbox database and a simulated date,
So that new staff can practise, month-end close included, without any risk to live data.

**Acceptance Criteria:**

**Given** a sandbox connection string (a separate database, e.g. `LuuKyCanTin_TapHuan`) configured by the administrator
**When** a user starts the app in training mode (a separate shortcut or `--training`)
**Then** the app connects only to the sandbox database, and a red banner "CHẾ ĐỘ TẬP HUẤN" stays visible on every form and is printed as a watermark on every PDF

**Given** training mode is active
**When** any service writes
**Then** a startup guard has verified that the connected database is the sandbox (a marker row or the DB name), never the live DB, and refuses to start otherwise

**Given** training mode with a simulated date set by the trainer
**When** any date logic runs
**Then** it uses the simulated `IClock`, so for example the month end can be practised on any day

**Given** a user with `HT.QuanTri`
**When** they choose "Reset sandbox"
**Then** the sandbox is recreated from migrations plus the demo seed (200 detainees, 50 goods), and the live DB is untouched

---

### Story 14.3: Auto-archive printed PDFs

`TI-09` · Size S · Depends on: 4.4 · FR73

As a unit leader,
I want every printed document saved automatically as a PDF on the shared folder,
So that an exact copy of what was handed out exists even if the paper is lost.

**Acceptance Criteria:**

**Given** the archive root is configured (e.g. `\\server\LuuKyCanTin\archive`)
**When** any document is printed (original or reprint)
**Then** the exact PDF sent to the printer is saved as `<root>\YYYY\MM\<SoChungTu>_<SoLanIn>.pdf`, with year and month taken from the document date

**Given** the share is unreachable or the write fails
**When** the user prints
**Then** printing still succeeds, the failure is logged with the document number, and the status bar shows a warning

**Given** an archived file already exists for that number and print count
**When** the save happens
**Then** the file is not overwritten; a new file name with a suffix is used

---

### Story 14.4: Cancel and replace a document

`TI-10` · Size M · Depends on: 4.7, 5.3, 6.6 · FR74

As a custodial officer,
I want to cancel a wrong posted document and get a pre-filled replacement linked to it,
So that corrections are quick and the audit trail shows exactly what replaced what.

**Acceptance Criteria:**

**Given** a posted receipt or payout in an open period, and the new column `ChungTuLuuKy.ChungTuThayTheChoId` (bigint, NULL, FK → ChungTuLuuKy) added in this story
**When** a user with cancel permission chooses "Cancel and replace" and enters the mandatory reason
**Then** the original is cancelled through the existing cancel rules (period not locked, balance never negative), and a draft replacement opens pre-filled with the original's data and linked by `ChungTuThayTheChoId`

**Given** the replacement is posted
**When** it receives a new document number
**Then** the original keeps its number and cancelled status, and both documents show the link ("replaces BNT-…" / "replaced by BNT-…")

**Given** the user abandons the replacement draft
**When** it is discarded
**Then** the original stays cancelled, and a cancelled document can be replaced at most once

**Given** the cancelled-documents report (6.6)
**When** it runs for the period
**Then** each cancelled document shows its replacement number, if any

---

### Story 14.5: QR code on the purchase slip

`TI-11` · Size S · P3 · Depends on: 10.4, 13.3 · FR75

As a canteen officer,
I want a QR code of the document number on each purchase slip,
So that scanning a slip opens its record immediately.

**Acceptance Criteria:**

**Given** a posted sale invoice
**When** the purchase slip is printed
**Then** a QR code that encodes the document number (`SoPhieu`) appears in a corner without shifting the agreed layout, and the golden-file test is updated

**Given** a USB scanner reads the QR into the global search (13.3) or the counter screen
**When** the number is entered
**Then** the matching sale invoice opens

---

### Story 14.6: Monthly canteen spending limit

`TI-12` · Size S · Depends on: 10.3 · FR76 · DEC-06

As a unit leader,
I want an optional monthly limit on how much a detainee can spend in the canteen,
So that the unit can enforce a policy if it adopts one, without a code change.

**Acceptance Criteria:**

**Given** the setting "monthly canteen limit" (on/off, amount in đồng), **off by default**, editable only with `HT.CauHinh` and audit-logged when changed
**When** it is off
**Then** sales behave exactly as in Epic 10

**Given** the limit is on with amount L
**When** a sale debiting detainee A would make A's posted purchase payouts (`NghiepVu` 21) in the calendar month of the sale date exceed L
**Then** the sale service blocks it, with a message showing the month-to-date total, L, and the amount over the limit
**And** a cancelled sale's refund no longer counts toward the month-to-date total

**Given** two workstations sell to A at the same time near the limit
**When** both post
**Then** the limit check runs inside the same transaction and row lock as the balance debit, so the limit is never exceeded

---

### Story 14.7: Stock count and adjustment notes

`TI-13` · Size M · Depends on: 9.1, 9.4, 11.1 · FR77 · DEC-07

As a canteen officer and commander,
I want to record a stock count, see the differences, and post an approved adjustment for damaged or expired goods,
So that book stock matches physical stock and every write-off is justified and approved.

**Acceptance Criteria:**

**Given** this story's migration
**When** it is applied
**Then** it adds `PhieuDieuChinhKho` (`Id`, `SoPhieu` UQ from `INumberingService` with a new type `DCK`, `NgayKiemKe date`, `LyDo`, `TrangThai` draft / awaiting approval / posted / cancelled, `NguoiDuyetId`, `NgayDuyet`), `PhieuDieuChinhKhoChiTiet` (`Id bigint`, `HangHoaId`, `SoLuongSoSach`, `SoLuongThucTe decimal(18,3)`, `ChenhLech`, `NguyenNhan` tinyint 1 hỏng / 2 hết hạn / 3 khác), new `TheKho.LoaiBienDong` values 5 (điều chỉnh tăng) and 6 (điều chỉnh giảm), and `TheKho.PhieuDieuChinhKhoChiTietId` (bigint NULL FK)
**And** the `TheKho` CHECK is changed to "exactly one of the three source FKs is set", the matching Domain enums are extended, and the enum ↔ CHECK test passes

**Given** a count sheet created for a date (`IClock.Today` by default)
**When** the officer enters the physical quantities
**Then** book quantity is taken from the last `TheKho` row on or before that date, and differences are calculated and shown per item

**Given** a count sheet submitted for approval
**When** a commander other than the creator (2.6) approves it
**Then** in one transaction a `TheKho` row is posted per non-zero difference (5 for a surplus, 6 for a shortage) valued at the current `GiaVonBQ` (decrease) or at that same average cost (increase, so the average is unchanged), `HangHoa.SoLuongTon` is updated under a row lock, and the approval is audit-logged

**Given** a shortage larger than the stock at any later point, or a count date earlier than other movements
**When** it is posted
**Then** the recalculation engine (9.4) replays later movements from the count date, and the posting is blocked if stock would go negative at any later point

**Given** the stock movement report (11.1)
**When** it runs for a period that contains adjustments
**Then** it shows separate adjustment-increase and adjustment-decrease columns (quantity and value), and opening + in − out ± adjustments = closing still holds for every item

---

### Story 14.8: Goods alerts and automatic price-list print

`TI-14` · Size S · Depends on: 8.3, 8.4, 9.1 · FR78

As a canteen officer,
I want alerts for low or discontinued stock and a reminder to print the price list when prices change,
So that I reorder in time and the posted prices are always current.

**Acceptance Criteria:**

**Given** a per-item low-stock threshold (new nullable column `HangHoa.NguongTonToiThieu decimal(18,3)`)
**When** `SoLuongTon` falls to or below the threshold after any posting
**Then** the item appears in the "Low stock" alert list on the canteen start screen and the POS screen

**Given** a discontinued item that still has stock, or an active item with no effective price
**When** alerts are computed
**Then** each appears in its own alert group

**Given** new prices take effect today (`IClock.Today`, from 8.4)
**When** the first canteen user signs in that day
**Then** the app offers to print the posted price list (*Bảng niêm yết giá*) with today's prices in one click

---

### Story 14.9: Bulk detainee type change from a sentence list

`TI-16` · Size S · Depends on: 3.3, 3.4 · FR80

As a custodial officer,
I want to change many detainees from temporary detention to prisoner from one Excel list of sentences,
So that a batch of court judgments is applied in minutes instead of one by one.

**Acceptance Criteria:**

**Given** a downloadable template (`MaSo`, `TuNgay`, `GhiChu` such as the sentence number)
**When** a user with `DM-02.Sua` uploads a filled file
**Then** a preview grid validates every row with the same rules as 3.3 (detainee exists and is managed, the change is valid, `TuNgay` is not before `NgayVao` and is unique for that detainee) and shows a per-row error message

**Given** the preview contains errors
**When** the user confirms
**Then** nothing is written until every row is valid (or the user removes the invalid rows); valid rows are applied in one transaction, each writing `LichSuLoaiDoiTuong` and an audit row

**Given** documents already posted for these detainees
**When** the batch is applied
**Then** their snapshot `LoaiDoiTuong` is unchanged

---

### Story 14.10: Transfer monitoring

`TI-17` · Size S · Depends on: 5.5, 6.1 · FR81

As a unit leader,
I want to be alerted to unusually frequent transfers between detainees and to see recurring giver–receiver pairs,
So that I can spot pressure, extortion or laundering through custodial transfers.

**Acceptance Criteria:**

**Given** configurable thresholds (default: more than 3 transfers given or received by one detainee within 30 days)
**When** a transfer request is created or approved (5.4/5.5)
**Then** if the giver or receiver crosses a threshold, the approver sees a warning with the recent transfer history; approval is not blocked

**Given** the report "Repeated transfer pairs" with the shared period filter (6.1)
**When** a user with `LK-BC.Xem` runs it
**Then** it lists giver–receiver pairs with 2 or more approved transfers in the period, ordered by count and total amount, exportable to PDF and Excel

---

### Story 14.11: Canteen anomaly and price-history reports

`TI-18` · Size M · Depends on: 11.3, 8.4, 6.1 · FR82

As a unit leader,
I want reports that flag unusual detainee purchases and show every price change,
So that I can detect favouritism, coercion or price manipulation in the canteen.

**Acceptance Criteria:**

**Given** the report "Unusual purchases" with the period filter (6.1)
**When** it runs
**Then** it lists detainees whose purchase total or number of purchases in the period exceeds the unit-wide average by a configurable factor (default 3×), with their totals, the average, and a drill-down to the purchase history (11.3)

**Given** the report "Price change history" for a period
**When** it runs
**Then** it lists each `BangGia` row that takes effect in the period with item, old price, new price, % change, effective date and the user who created it (from the audit columns)

**Given** either report
**When** it is exported
**Then** PDF and Excel are available through the shared export (6.1)

---

### Story 14.12: Goods-receipt control with scanned invoice and approval

`TI-19` · Size M · Depends on: 9.2, 2.6 · FR83

As a commander,
I want each goods receipt to carry a scan of the supplier invoice and my approval before it is posted,
So that stock and cost cannot be inflated with unsupported receipts.

**Acceptance Criteria:**

**Given** the setting "goods receipts require approval" (on/off, audit-logged when changed)
**When** it is on
**Then** a goods-receipt note moves draft → awaiting approval → posted, the new status value is added to the enum and CHECK (enum test passes), and only a user with `NH.Duyet` who is not the creator (2.6) can approve, which runs the 9.2 posting

**Given** a goods-receipt note
**When** the officer attaches a scanned invoice (PDF/JPG/PNG, max 10 MB)
**Then** the file is copied to the share (`\\server\LuuKyCanTin\attachments\PN\YYYY\<SoPhieu>_<n>.<ext>`), a new `TepDinhKem` row stores the path, original file name and uploader, and the attachment cannot be removed after posting

**Given** the setting is on and no attachment exists
**When** submission for approval is attempted
**Then** it is blocked with "Attach the supplier invoice"

**Given** an approver rejects the note
**When** they enter a reason
**Then** it returns to draft with the reason shown, and the rejection is audit-logged

---

### Story 14.13: Hash chain over the custodial ledger

`TI-20` · Size M · Depends on: 4.3, 6.7 · FR84

As an accountant or unit leader,
I want every posted ledger entry chained by a cryptographic hash and checked during reconciliation,
So that any change made directly in the database, outside the app, is detected.

**Acceptance Criteria:**

**Given** new columns `ChungTuLuuKy.HashTruoc` and `HashHienTai` (`binary(32)`) and a new single-row `ChuoiHash` table holding the latest hash, added in this story
**When** `GhiSoLuuKyService` posts or cancels a document
**Then** in the same transaction, under a lock on the `ChuoiHash` row so the chain stays linear across workstations, it writes `HashHienTai = SHA-256(HashTruoc ‖ canonical fields)` and updates `ChuoiHash`
**And** the canonical fields are, in a fixed order and invariant format: `Id`, `SoChungTu`, `NgayChungTu`, `LoaiPhieu`, `NghiepVu`, `DoiTuongId`, `SoTien`, `SoDuTruoc`, `SoDuSau`, `TrangThai`, `NguoiTaoId`, plus the posting or cancel timestamp

**Given** a cancellation of a posted document
**When** it is recorded
**Then** a new chain entry is appended (event-style); the earlier entry's hash is never recomputed

**Given** existing posted rows when this story ships
**When** the one-time backfill runs from the admin tool
**Then** it chains all existing rows in `Id` order, records the backfill date in the audit log, and reconciliation reports "chain verified since backfill"

**Given** the one-click reconciliation (6.7)
**When** it runs
**Then** it recomputes the chain and reports the first broken link (document number and expected vs stored hash) whenever a row's amount, balance, status or order was changed outside the app

**Given** an integration test that updates `SoTien` on one row directly with SQL
**When** reconciliation runs
**Then** it reports exactly that document as tampered

---

### Story 14.14: Support bundle export

`TI-21` · Size S · Depends on: 1.1 · FR85

As an administrator,
I want to export a zip of logs and version information for a support request,
So that a technician can diagnose problems without remote or Internet access.

**Acceptance Criteria:**

**Given** a user with `HT.QuanTri`
**When** they choose "Export support bundle" and pick a folder (e.g. a USB drive)
**Then** a zip `LuuKyCanTin_support_<machine>_<yyyyMMdd_HHmm>.zip` is created with the Serilog files of the last N days (default 7), app version, DB server and database name, last applied migration, OS version and settings with secrets removed

**Given** the bundle contents
**When** it is built
**Then** connection strings, passwords and database data are never included, only log files and metadata, and the export is audit-logged

---

### Story 14.15: Monthly revenue chart

`TI-23` · Size S · P3 · Depends on: 11.2 · FR87

As a unit leader,
I want a chart of monthly revenue and profit by buyer type,
So that I can see canteen trends at a glance.

**Acceptance Criteria:**

**Given** a year selection
**When** a user with `HH-BC.Xem` opens the chart
**Then** it shows 12 monthly columns of revenue, stacked by buyer type (detainee, relative, staff, visiting unit), with a net-profit line, using the same SQL source as the revenue report (11.2) so the totals match

**Given** the chart
**When** the user clicks a month
**Then** the revenue report for that month opens
