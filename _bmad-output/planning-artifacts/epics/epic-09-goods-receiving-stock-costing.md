---
epic: 9
title: Goods receiving & stock costing
release: R1
frsCovered: [FR49, FR50, FR51, FR52, FR53]
backlogItems: [NEN-28, NH-01, NH-02, NH-03, NEN-29, NH-04, GAP-02]
dependsOn: [1, 2, 4, 6, 8]
---

## Epic 9: Goods receiving & stock costing

Canteen staff receive goods from suppliers on a multi-line goods-receipt note (*Phiếu nhập*). Each posting updates the running stock card (`TheKho`) and the moving weighted-average cost, accurate to the đồng. Corrections and back-dated documents recalculate later costs deterministically, and opening stock enters through an approved initial receipt.

**Exit criteria:**
- After any sequence of receipts, issues and cancellations, `HangHoa.SoLuongTon` and `GiaVonBQ` equal the last `TheKho` row for the item.
- A full recalculation from the beginning gives identical rows.

**Applies to every story in this epic:**
- Per the DEC-01 assumption, cost is a **moving** weighted average, recalculated after each receipt. If DEC-01 decides otherwise, only Story 9.1's Domain rule and Story 9.4 change.
- Every posting or cancellation is one service method in one transaction (`NhapHangService`).
- Documents dated in a locked period are refused (Story 6.8), and back-dating follows the guard in Story 6.9.
- Permissions are checked in the service: `NH.Them`, `NH.Sua`, `NH.Huy`, `NH.In`, `NH.Duyet`.
- The interceptor audit-logs every voucher change.

---

### Story 9.1: Running stock card and moving weighted-average cost

`NEN-28` · Size M · Depends on: 8.3, 4.1, 1.2 · FRs: FR50 (rule)

As an accountant,
I want every stock movement to record the running quantity, value and average cost after it, using one explicit rounding rule,
So that stock and cost of goods are exact and never drift, however many movements accumulate.

**Acceptance Criteria:**

**Given** the Domain class `BinhQuanGiaQuyen` (no dependencies outside Domain) and the per DEC-01 assumption of a moving average
**When** a receipt of quantity `q` with value `v` (đồng) is applied to a state (`SoLuongTon`, `GiaTriTon`)
**Then** the new state is `SoLuongTon + q`, `GiaTriTon + v`, and `GiaVonBQ = Round(GiaTriTon / SoLuongTon, 4, MidpointRounding.AwayFromZero)`

**Given** an issue of quantity `q`
**When** it is applied
**Then** the issue value is `Round(q × GiaVonBQ, 0, AwayFromZero)`, `GiaTriTon` decreases by that value, and the average cost is unchanged
**And** when the issue brings `SoLuongTon` to exactly 0, the issue value equals the whole remaining `GiaTriTon` (absorbing any rounding residue), so the value after is 0
**And** an issue larger than `SoLuongTon` throws a Domain exception

**Given** the rounding rule above (proposal: unit cost to 4 decimals, amounts to đồng, AwayFromZero; the value is the source of truth and the average is derived from it)
**When** it is reviewed with the accountant
**Then** the rule is recorded in `docs/decisions/` and referenced by every story that values stock

**Given** the `TheKho` table (`Id bigint`, `HangHoaId` FK, `NgayChungTu date`, `LoaiBienDong tinyint` 1 nhập / 2 xuất / 3 huỷ nhập / 4 huỷ xuất, `PhieuNhapChiTietId bigint NULL`, `PhieuBanHangChiTietId bigint NULL` with CHECK that exactly one of the two is set, `SoLuongNhap`, `SoLuongXuat decimal(18,3)`, `DonGia decimal(18,4)`, `GiaTri decimal(18,0)`, `SoLuongTonSau decimal(18,3)`, `GiaTriTonSau decimal(18,0)`, `GiaVonBQSau decimal(18,4)`, index (`HangHoaId`, `NgayChungTu`, `Id`))
**When** the migration runs
**Then** the table and constraints exist, and the enum-consistency test covers `LoaiBienDong`
**And** the FK to `PhieuNhapChiTiet` is added in Story 9.2 and the FK to `PhieuBanHangChiTiet` in Story 10.2, when those tables are created

**Given** a property test of 5,000 random receipts and issues for one item
**When** it runs
**Then** after every step `GiaTriTonSau` = Σ receipt values − Σ issue values exactly, `SoLuongTonSau` = Σ quantities in − Σ quantities out, and `SoLuongTon = 0` implies `GiaTriTon = 0`

---

### Story 9.2: Create and post a goods-receipt note

`NH-01, NH-02` · Size M · Depends on: 9.1, 8.1, 4.2, 4.5, 4.7 · FR49, FR50

As a canteen officer,
I want to enter a supplier's invoice as a multi-line goods-receipt note and post it,
So that stock increases and the average cost updates as soon as goods arrive.

**Acceptance Criteria:**

**Given** the tables `PhieuNhap` (`Id`, `SoPhieu varchar(20)` UQ, `SoHoaDon varchar(30)` NOT NULL, `NgayHoaDon date`, `NgayNhap date`, `NhaCungCapId` FK, supplier name/address snapshot, `NguoiGiao nvarchar(100)`, `TongTien decimal(18,0)`, `TrangThai` 1 nháp / 2 đã ghi sổ / 3 đã huỷ, `LyDoHuy`, `NgayHuy`, `NguoiHuyId`, `SoLanIn`, audit columns) and `PhieuNhapChiTiet` (`Id bigint`, `PhieuNhapId`, `ThuTu smallint`, `HangHoaId` FK, `SoLuong decimal(18,3) CHECK (> 0)`, `DonGia decimal(18,2)`, `ThanhTien decimal(18,0)`)
**When** I create a note for supplier "NCC01" with invoice number, invoice date, receipt date and lines entered in the shared voucher grid
**Then** I can only pick active suppliers (`DangGiaoDich = 1`) and active goods (`DangKinhDoanh = 1`)
**And** each line's `ThanhTien = Round(SoLuong × DonGia, 0, AwayFromZero)`, the total is their sum, and the note saves as a draft without touching stock

**Given** a draft note missing an invoice number, with no lines, with a line quantity ≤ 0, or with the same item twice
**When** I try to post it
**Then** FluentValidation rejects it with a message per field or line

**Given** a valid draft
**When** I post it (pre-posting confirmation shows the total, Story 4.5)
**Then** in one transaction the number `PN` is allocated from `INumberingService`, the status becomes posted, one `TheKho` row (`LoaiBienDong = 1`) is written per line using `BinhQuanGiaQuyen`, and `HangHoa.SoLuongTon` / `GiaVonBQ` are updated to the new state under a row lock
**And** if any step fails, nothing is saved

**Given** two items in stock
**When** I post a receipt of 100 items at 3,500 đ to an item that already holds 50 at an average of 3,200 đ
**Then** the stock becomes 150 and the average cost becomes `Round((160,000 + 350,000) / 150, 4)` = 3,400.0000

**Given** a note dated earlier than the item's latest stock movement
**When** it is posted (allowed only within the back-date rules of Story 6.9)
**Then** the later rows for that item are recalculated by the engine in Story 9.4 (until 9.4 exists, back-dated posting is refused)

---

### Story 9.3: Print the goods-receipt note

`NH-03` · Size S · Depends on: 9.2, 4.4 · FR51

As a canteen officer,
I want to print the goods-receipt note in the official layout,
So that the deliverer, receiver, team commander and unit leader can sign it.

**Acceptance Criteria:**

**Given** a posted note
**When** I print it with template `PHIEU_NHAP`
**Then** the shared frame renders the unit name and address, the title "PHIẾU NHẬP HÀNG", Người giao, Tên đơn vị giao, Địa chỉ (from the snapshot), Số hoá đơn / bảng kê, Ngày hoá đơn, Ngày nhập, and the line table: Tên mặt hàng, Mã hàng, Đơn vị tính, Số lượng, Đơn giá, Thành tiền, plus the total in figures and words (Story 1.4)
**And** the signature block shows Người giao, Người nhận, Chỉ huy đội, Lãnh đạo đơn vị from `CauHinhKyTen`

**Given** a note printed before
**When** it is printed again
**Then** `SoLanIn` increments, the print is audit-logged and the PDF carries the "BẢN IN LẠI" watermark with the print count

**Given** a draft note
**When** I try to print it
**Then** printing is refused (draft preview is Story TI-08 in R2)

---

### Story 9.4: Stock-card recalculation engine

`NEN-29` · Size L · Depends on: 9.1, 9.2 · FRs: FR52 (enabler)

As an accountant,
I want stock running totals and the cost of every later issue recalculated automatically when an earlier movement changes,
So that costs stay correct after back-dated documents and cancellations.

**Acceptance Criteria:**

**Given** the Application service `TinhLaiTheKhoService.TinhLaiAsync(hangHoaId, tuNgay)`
**When** it runs
**Then** it takes the state from the last `TheKho` row before `tuNgay` (or zero), replays every row for that item ordered by (`NgayChungTu`, `Id`) from `tuNgay` with `BinhQuanGiaQuyen`, and rewrites `DonGia`/`GiaTri` of issue rows and the `*Sau` columns of every row
**And** for each issue row linked to a sale line it also rewrites `PhieuBanHangChiTiet.DonGiaVon` and `GiaVon` (active once Epic 10 exists)
**And** finally it sets `HangHoa.SoLuongTon` and `GiaVonBQ` to the last row's values

**Given** the engine runs inside the caller's transaction
**When** it is triggered by a back-dated posting or a cancellation
**Then** recalculation and the triggering change commit or roll back together, and the item row stays locked (`UPDLOCK`) for the duration

**Given** any history for an item
**When** `TinhLaiAsync(item, X)` runs twice in a row, or runs from X versus from the first movement
**Then** the resulting rows are identical (idempotent, and equal to a full rebuild), checked by a property test on LocalDB

**Given** a replay where stock would become negative at some row
**When** the engine detects it
**Then** it throws `TonKhoAmException` naming the item, date and document, and the whole transaction rolls back

**Given** a recalculation that changes cost for issues in a locked period
**When** it would rewrite them
**Then** it is refused with "Ảnh hưởng giá vốn kỳ đã khoá sổ"; the accountant must unlock the period first (Story 6.8)

---

### Story 9.5: Edit a draft and cancel a posted goods-receipt note

`NH-04` · Size M · Depends on: 9.4, 6.8 · FR52

As a canteen officer,
I want to correct drafts freely and cancel a posted note with a reason,
So that mistakes are fixed without breaking stock or the cost of later sales.

**Acceptance Criteria:**

**Given** a draft note
**When** I edit or delete its lines
**Then** the changes are saved with no stock effect

**Given** a posted note
**When** I open it
**Then** it is read-only; correcting it means cancelling it and creating a new note (no in-place edit of posted documents)

**Given** a posted note in an open period
**When** I cancel it and enter a reason (required, at most 300 characters)
**Then** in one transaction the status becomes cancelled with `LyDoHuy`, `NgayHuy` and `NguoiHuyId`, one `TheKho` row with `LoaiBienDong = 3` reverses each line, and the engine in Story 9.4 recalculates every later movement of the affected items
**And** the interceptor logs the cancellation as `Huy`

**Given** a cancellation that would make the stock of any item negative at any later point in time (goods already sold)
**When** I confirm it
**Then** it is blocked with the item and date where stock would go negative, and nothing changes

**Given** a note dated in a locked period, or a user without `NH.Huy`
**When** a cancellation is attempted
**Then** it is refused by the service

---

### Story 9.6: Opening stock at go-live

`GAP-02` · Size S · Depends on: 9.2, 2.6, 7.5 · FR53

As an accountant,
I want to enter opening stock quantities and costs as an initial goods-receipt note approved by leadership,
So that the canteen starts on the system with correct stock and cost, traceable to an approved document.

**Acceptance Criteria:**

**Given** per the DEC-09 assumption a goods-receipt note flagged `LaTonDauKy = 1` (new column, no supplier invoice required), dated the go-live date
**When** I enter one line per item with quantity and value
**Then** it saves as a draft awaiting approval, and lines can be loaded from the Excel import pattern (Story 3.4)

**Given** a draft opening-stock note
**When** a user with `NH.Duyet` approves it
**Then** it posts through the same posting path as Story 9.2 (`TheKho` `LoaiBienDong = 1`), recording the approver and approval time
**And** the creator cannot approve their own note (segregation-of-duties policy, Story 2.6)

**Given** an item that already has any stock movement
**When** it is added to an opening-stock note
**Then** it is rejected: opening stock is allowed only once per item and must be its first movement

**Given** the opening-stock note is posted
**When** the stock movement report later runs for the go-live month
**Then** these quantities appear as receipts on the go-live date
