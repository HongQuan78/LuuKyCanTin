---
epic: 6
title: Custodial reports, reconciliation & period close
release: R0.5
frsCovered: [FR9, FR10, FR36, FR37, FR38, FR39, FR40, FR41, FR45, FR46]
backlogItems: [NEN-25, HH-BC04, LK-BC04, LK-BC01, LK-BC02, LK-BC03, NEN-26, NEN-27, HT-06, NEN-11]
dependsOn: [1, 2, 3, 4, 5]
---

## Epic 6: Custodial reports, reconciliation & period close

The accountant and leadership print every custodial report the process requires, check in one click that every balance matches the ledger, and lock the month so posted figures cannot change. Officers look up any detainee's balance and history instantly.

**Applies to every story in this epic:** reports are read-only queries written as SQL (EF `SqlQuery` or Dapper), not complex LINQ; they include only posted documents (`TrangThai = 2`) unless stated otherwise; amounts are in đồng; every report opens in the shared period filter and preview (Story 6.1); printing and exporting are audit-logged as `In`; services check the report's view permission (`LK-BC.Xem`) before running.

---

### Story 6.1: Shared report-period filter, preview and PDF/Excel export

`NEN-25, HH-BC04` · Size M · Depends on: 4.4 · FR45, FR46, UX-DR5

As an accountant,
I want every report to share the same period picker and the same Preview, Print, PDF and Excel buttons,
So that I learn one screen and can file or send any report.

**Acceptance Criteria:**

**Given** the shared period filter
**When** the user chooses Month (month + year), Quarter (quarter + year), Year, or From–To dates
**Then** it resolves to an inclusive `TuNgay`–`DenNgay` range; the default is the current month from `IClock`; From > To is rejected

**Given** a report built on the shared report host
**When** the user clicks Preview
**Then** the report renders through `IReportRenderer` into the WebView2 preview with the unit header, the period line ("Kỳ báo cáo: Tháng 09/2026" or "Từ ngày … đến ngày …"), "Đơn vị tính: đồng" and the signer block from `CauHinhKyTen`

**Given** a rendered report
**When** the user clicks Export PDF or Export Excel
**Then** a PDF identical to the preview is saved, or an `.xlsx` is produced with ClosedXML with the same title, period, columns, totals row and number format `#,##0`
**And** the export is audit-logged with the report code and period

**Given** an Application report query
**When** it is unit-tested
**Then** it depends only on Application abstractions; the SQL lives in Infrastructure and is covered by a LocalDB integration test

---

### Story 6.2: Balance lookup and transaction timeline

`LK-BC04` · Size M · Depends on: 3.2, 6.1 · FR39

As a custodial officer or warden,
I want to pick a detainee and see their current balance and full transaction history,
So that I can answer "how much does this detainee have and why" on the spot.

**Acceptance Criteria:**

**Given** a detainee picked with the shared picker
**When** the lookup opens
**Then** it shows code, name, type, cell, status and the current balance `SoDuLuuKy` in a large figure

**Given** the detainee's posted documents
**When** the timeline loads
**Then** each row shows date, document number, receipt or payout, `NghiepVu` name, description, amount (+/−) and the balance after (`SoDuSau`), newest first, with an optional date filter
**And** cancelled documents can be shown with a toggle, struck through, with their cancellation reason

**Given** a warden whose permissions are limited to their own detainees
**When** they look up a detainee assigned to another warden
**Then** the service refuses it

**Given** a timeline row
**When** the user double-clicks it
**Then** the source receipt or payout opens read-only

---

### Story 6.3: Per-detainee custodial statement

`LK-BC01` · Size M · Depends on: 6.1 · FR36

As a canteen officer or warden,
I want to print the *Bảng kê theo dõi số tiền gửi lưu ký* for one detainee and a period,
So that the detainee can check and sign for every movement on their account.

**Acceptance Criteria:**

**Given** a detainee and a period
**When** the statement runs
**Then** the header shows unit name and address, the title, the detainee's name, code and type, and the period
**And** the opening balance is computed from the ledger as Σ posted receipts − Σ posted payouts dated before `TuNgay`, using the index `(DoiTuongId, NgayChungTu, Id)`

**Given** posted documents in the period
**When** the table renders
**Then** each line shows document number, document date, description ("Số tiền đã nhận" / "Số tiền đã chi" plus `NoiDung`), the amount in the received column (`NghiepVu` 11–15) or the paid column (21–25), and the running balance
**And** the footer shows opening balance, total received, total paid and "Số tiền còn được sử dụng" = opening + received − paid, in figures and words

**Given** the signer configuration for this template
**When** it prints
**Then** the signers are "Cán bộ căn tin", "Cán bộ quản giáo", "Người bị tạm giữ, tạm giam/phạm nhân", "Thủ trưởng đơn vị"

**Given** a test detainee with a known history crossing the period boundary
**When** the integration test runs the statement
**Then** the closing balance for a period ending today equals `DoiTuong.SoDuLuuKy`

---

### Story 6.4: Unit-wide custodial ledger book

`LK-BC02` · Size M · Depends on: 6.3 · FR37

As an accountant,
I want the *Sổ theo dõi tiền gửi lưu ký* for the whole unit and a period,
So that I can check every detainee's movement on one page and sign off the month.

**Acceptance Criteria:**

**Given** a period
**When** the ledger book runs
**Then** it lists one row per detainee with any balance or movement: STT, code, name, opening balance, received in period, paid in period, remaining usable balance, sorted by code
**And** a totals row sums each money column

**Given** the totals
**When** they are checked
**Then** total opening + total received − total paid = total remaining, and total remaining for a period ending today equals Σ `DoiTuong.SoDuLuuKy`

**Given** the signer configuration
**When** it prints
**Then** the signers are "Cán bộ căn tin", "Chỉ huy phụ trách", "Kế toán đơn vị", "Thủ trưởng đơn vị"

**Given** 2,000 detainees with a year of documents
**When** the book runs for one month
**Then** it completes within 5 seconds on target hardware, using the filtered index on `ChungTuLuuKy (NgayChungTu) WHERE TrangThai = 2`

---

### Story 6.5: Deposit remittance list

`LK-BC03` · Size M · Depends on: 6.1 · FR38

As a custodial officer,
I want to build the *Bảng kê nộp tiền gửi lưu ký* from the receipts not yet remitted,
So that the cash I hand over is listed exactly, and no receipt is remitted twice.

**Acceptance Criteria:**

**Given** a migration that creates `BangKeNop` (`SoBangKe` UQ, `TuNgay`, `DenNgay`, `NguoiNopHoTen`, `DiaChiNguoiNop`, `TongTien`, `TongTienBangChu`, `TrangThai`) and `BangKeNopChiTiet` (`BangKeNopId`, `ChungTuLuuKyId` **UQ**, `ThuTu`)
**When** the officer chooses a from–to range
**Then** the list proposes every posted receipt (`LoaiPhieu = 1`) in the range that is not yet on a list; the officer may deselect lines (alignment note A6)

**Given** the officer enters the depositor's name and address and saves
**When** the list is posted
**Then** a `BKN` number is allocated, `TongTien` and `TongTienBangChu` are computed, and the lines are stored in order

**Given** a receipt already on another list
**When** someone tries to add it (any path, including concurrent saves)
**Then** the UQ on `ChungTuLuuKyId` blocks it and the user sees which list holds it

**Given** a posted list
**When** it prints
**Then** it shows unit name and address, title, period, depositor name and address, and a table of STT, detainee name, code, source document number (`SoPhieuGoc`), receipt method, description, amount; a total in figures and words; signers "Người nộp", "Chỉ huy phụ trách", "Thủ trưởng đơn vị"

**Given** a receipt included on a list
**When** someone tries to cancel that receipt
**Then** it is refused until the list is cancelled (with a reason, audit-logged), which releases its receipts

---

### Story 6.6: Cancelled documents report

`NEN-26` · Size S · Depends on: 6.1 · FR40

As a unit leader,
I want a report of every custodial document cancelled in a period,
So that I can review cancellations and spot misuse.

**Acceptance Criteria:**

**Given** a period
**When** the report runs
**Then** it lists every `ChungTuLuuKy` with `TrangThai = 3` and `NgayHuy` in the period: number, document date, type, detainee, amount, cancellation reason, who cancelled, when, and "Đã in" yes/no from `SoLanIn > 0`

**Given** cancelled documents that had been printed
**When** the report renders
**Then** they are highlighted and counted separately in the summary

**Given** a user without leadership report permission
**When** the report is requested
**Then** the service refuses it

---

### Story 6.7: One-click balance reconciliation

`NEN-27` (balance part) · Size M · Depends on: 4.3, 6.1 · FR41

As an accountant,
I want one button that proves every detainee's balance matches the ledger,
So that I can sign off the books with confidence and catch any corruption early.

**Acceptance Criteria:**

**Given** the reconciliation routine from Story 4.3
**When** the accountant clicks "Đối chiếu"
**Then** for every detainee it compares `DoiTuong.SoDuLuuKy` with Σ posted receipts − Σ posted payouts
**And** it checks the `SoDuTruoc`/`SoDuSau` chain: in posting order, each document's `SoDuTruoc` equals the previous document's `SoDuSau`

**Given** no differences
**When** the check finishes
**Then** a green "0 chênh lệch" result shows with the count of detainees and documents checked and the time taken

**Given** differences
**When** the check finishes
**Then** each difference is listed (detainee, stored balance, ledger balance, difference, first broken chain link), can be exported to Excel, and nothing is corrected automatically

**Given** any run
**When** it completes
**Then** the run is audit-logged with its result; the check is read-only

---

### Story 6.8: Lock and unlock accounting periods

`HT-06` · Size M · Depends on: 4.3 · FR9

As an accountant,
I want to lock a closed month and, rarely and with a reason, unlock it,
So that figures that have been reported and signed cannot change afterwards.

**Acceptance Criteria:**

**Given** the `KyKhoaSo` table (created in Story 4.3: `TuNgay`, `DenNgay`, `CHECK (TuNgay <= DenNgay)`, `DaKhoa`, `NguoiKhoaId`, `NgayKhoa`)
**When** the accountant opens the period screen
**Then** periods are listed with status, who locked them and when

**Given** a user with `HT.KhoaSo`
**When** they lock a month
**Then** the period is created (if needed) and set `DaKhoa = 1`; a period that overlaps an existing one is rejected

**Given** a locked period
**When** anyone adds, edits, posts or cancels any document dated inside it
**Then** the engine refuses it with "Kỳ đã khoá sổ" (rule from Story 4.3, now covered by an end-to-end test)

**Given** a user with `HT.KhoaSo`
**When** they unlock a period
**Then** a reason is required and the unlock is audit-logged with the user and time

**Given** an attempt to lock a period while documents in it are still drafts
**When** the lock is requested
**Then** the user is warned with the list of drafts and must confirm

---

### Story 6.9: Back-date guard

`NEN-11` · Size S · Depends on: 6.8 · FR10

As a unit leader,
I want documents to be dated only within an allowed window unless a user holds a special permission,
So that nobody can quietly insert transactions into earlier periods.

**Acceptance Criteria:**

**Given** the setting "Số ngày được lùi" (default 3 days) and today's date from `IClock`
**When** a user creates, edits or posts a document dated before today minus that many days
**Then** the service refuses it with "Ngày chứng từ ngoài phạm vi cho phép"

**Given** a user holding `LK.LuiNgay`
**When** they back-date a document within an open period
**Then** it is allowed, and the audit log marks the action as back-dated

**Given** any user, including those with `LK.LuiNgay`
**When** they date a document inside a locked period or after today
**Then** it is refused

**Given** a simulated clock in tests
**When** the date moves across a month boundary
**Then** the window is evaluated against the simulated date
