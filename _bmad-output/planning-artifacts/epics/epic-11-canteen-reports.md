---
epic: 11
title: Canteen reports & stock reconciliation
release: R1
frsCovered: [FR41, FR62, FR63, FR64]
backlogItems: [HH-BC01, HH-BC02, HH-BC03, NEN-27]
dependsOn: [1, 2, 4, 6, 8, 9, 10]
---

## Epic 11: Canteen reports & stock reconciliation

Leadership and the accountant get the three canteen reports: stock movement, revenue and profit, and the purchase history of one buyer. Their figures match the stock card and the custodial ledger exactly. The accountant's one-click reconciliation now covers stock as well as balances, which makes it the R1 release gate.

**Exit criteria (R1 gate):**
- DEC-01, DEC-03, DEC-04 and DEC-06 are decided.
- On the R1 demo data, one-click reconciliation shows zero differences for balances and for stock.

**Applies to every story in this epic:**
- Reports are SQL queries (EF `SqlQuery` or Dapper) in `Application/BaoCao`, not complex LINQ.
- Each report uses the shared period filter with preview, print and PDF/Excel export (Story 6.1).
- Each report renders in the shared print frame with signers from `CauHinhKyTen` (Story 4.4).
- Only posted documents count; cancelled ones are excluded.
- Viewing needs `HH-BC.Xem` and printing or exporting needs `HH-BC.In`, checked in the service. Prints are audit-logged.

---

### Story 11.1: Stock movement report

`HH-BC01` · Size M · Depends on: 9.1, 10.2, 6.1 · FR62

As a unit leader,
I want a stock movement report (opening / in / out / closing in quantity and value) for any period,
So that I can check stock and the cost of goods sold against the stock card.

**Acceptance Criteria:**

**Given** the template `BAO_CAO_NHAP_XUAT` and a period (month / quarter / year / from–to)
**When** I preview the report
**Then** it shows the unit header, the title "BÁO CÁO NHẬP, XUẤT HÀNG HOÁ", the period, and one row per item with columns STT, Tên hàng, Mã hàng, Đơn vị tính, Tồn đầu kỳ (SL, Thành tiền), Nhập (SL, Thành tiền), Xuất (SL, Thành tiền), Tồn cuối kỳ (SL, Thành tiền), plus a total row
**And** the signers are Cán bộ bán hàng, Chỉ huy phụ trách, Lãnh đạo đơn vị

**Given** the stock card
**When** the query computes a row
**Then** the opening figures come from the item's last `TheKho` row before the period start and the closing figures from its last row on or before the period end, using index (`HangHoaId`, `NgayChungTu`, `Id`)
**And** "Nhập" = Σ receipts minus cancelled receipts (`LoaiBienDong` 1 − 3) and "Xuất" = Σ issues minus cancelled issues (2 − 4) in the period, with issue value at the moving weighted average (per DEC-01 assumption)

**Given** any period
**When** the report is checked by an integration test
**Then** for every item, opening + in − out = closing, in both quantity and value, exactly

**Given** items with no movement and zero stock in the period
**When** the report runs
**Then** they are omitted by default, with an option to include them

---

### Story 11.2: Revenue report

`HH-BC02` · Size M · Depends on: 10.2, 6.1 · FR63

As a unit leader,
I want revenue, cost and net profit per sale line for a period, filtered by buyer type,
So that I can see how the canteen is performing for each group of buyers.

**Acceptance Criteria:**

**Given** the template `BAO_CAO_DOANH_THU`, a period and an optional buyer type (1 đối tượng / 2 người thân / 3 CBCS / 4 đơn vị đến công tác, or all)
**When** I preview the report
**Then** it lists posted sale lines with columns STT, Số phiếu, Ngày xuất, Tên hàng, Mã hàng, Số lượng bán, Đơn giá bán, Doanh thu, Đơn giá vốn, Giá vốn, Lãi ròng, and totals for revenue, cost and profit
**And** signers are Cán bộ căn tin, Chỉ huy phụ trách, Lãnh đạo đơn vị

**Given** each line
**When** values are computed
**Then** Doanh thu = `ThanhTien`, Giá vốn = `GiaVon` as stored on the line (after any recalculation, Story 9.4), Lãi ròng = Doanh thu − Giá vốn, and cancelled sales are excluded

**Given** the same period
**When** total cost on this report is compared to the "Xuất" value on the stock movement report (Story 11.1)
**Then** the two figures are equal (integration test)

---

### Story 11.3: Purchase history of one buyer

`HH-BC03` · Size S · Depends on: 10.3, 6.1 · FR64

As a canteen officer,
I want to print every purchase one buyer made in a period, with their custodial balance for detainee buyers,
So that I can answer a detainee's or a leader's questions about what was bought.

**Acceptance Criteria:**

**Given** the template `BANG_THEO_DOI_MUA_HANG`, a period and a buyer (a detainee picked with Story 3.2, or a buyer name for types 3/4)
**When** I preview the report
**Then** it shows the unit header, the title "BẢNG THEO DÕI MUA HÀNG", Thời gian mua, Tên người mua, Địa chỉ, Loại, Mã số đối tượng, and the lines Tên mặt hàng, Mã hàng, Đơn vị tính, Số lượng, Đơn giá, Thành tiền from posted sales in the period
**And** signers are Cán bộ căn tin, Chỉ huy phụ trách, Lãnh đạo đơn vị

**Given** a buyer of type 1 or 2
**When** the report renders
**Then** it adds Số tiền kỳ trước chuyển sang (the detainee's ledger balance at period start), Số tiền mua hàng kỳ này (Σ purchases in the period) and Số tiền còn được sử dụng (the ledger balance at period end), all computed from `ChungTuLuuKy`

**Given** a detainee with no purchases in the period
**When** the report runs
**Then** it prints with the balance block and an empty line table, not an error

---

### Story 11.4: Stock reconciliation in the one-click check

`NEN-27` (stock part) · Size M · Depends on: 6.7, 9.4, 10.3 · FR41

As an accountant,
I want the one-click reconciliation to also check stock, cost and the link between canteen sales and the custodial ledger,
So that before any period close I know that stock and money are both exactly right.

**Acceptance Criteria:**

**Given** the reconciliation screen from Story 6.7
**When** I run it
**Then** besides the balance check it lists every item where `HangHoa.SoLuongTon` ≠ the last `TheKho.SoLuongTonSau` or `HangHoa.GiaVonBQ` ≠ the last `GiaVonBQSau`, with both values and the difference

**Given** posted sales with `HinhThucThanhToan = 1`
**When** the check runs
**Then** it lists every sale that does not have exactly one posted type-21 `ChungTuLuuKy` with `SoTien = TongTien`, every type-21 payout without a posted sale, and every cancelled sale whose payout is not cancelled

**Given** no differences
**When** the check finishes
**Then** it shows "Không có chênh lệch" for balances, stock and the sale ↔ ledger link, and the result is audit-logged with its run time and the user

**Given** the R1 integration test suite
**When** any test finishes
**Then** the stock and sale ↔ ledger checks run as assertions alongside the balance reconciliation (NEN-19)
