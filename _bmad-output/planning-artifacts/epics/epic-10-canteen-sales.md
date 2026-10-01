---
epic: 10
title: Canteen sales against custodial balance
release: R1
frsCovered: [FR33, FR55, FR57, FR58, FR59, FR60, FR61]
backlogItems: [NEN-30, BH-04, BH-02, BH-06, BH-05, LK-C05, BH-07, BH-08]
dependsOn: [1, 2, 3, 4, 5, 6, 8, 9]
---

## Epic 10: Canteen sales against custodial balance

The canteen cashier sells on a fast POS-style screen to the 4 buyer types: detainee, relative, staff member (CBCS) and visiting unit. One `BanHangService.GhiSoAsync` transaction writes the sale invoice, the stock-card issue rows, the stock deduction with the cost of goods, and, for a detainee or relative buyer, the custodial debit through `GhiSoLuuKyService`. A sale is never possible beyond stock or beyond balance, and cancelling a sale returns both the stock and the money.

**Exit criteria:**
- After any mix of sales and cancellations, every detainee balance reconciles with the ledger.
- Every stock figure reconciles with `TheKho`.
- Every debit sale has exactly one type-21 payout.

**Applies to every story in this epic:**
- Permissions are checked in the service: `BH.Them`, `BH.Huy`, `BH.In`.
- The interceptor audit-logs every change.
- All dates come from `IClock`.
- Documents dated in a locked period are refused (Story 6.8).
- Per the DEC-06 assumption, there is no monthly spending limit (the flag exists and is off; TI-12 is R2).

---

### Story 10.1: Effective price and concurrency-safe stock deduction

`BH-04, NEN-30` · Size M · Depends on: 8.4, 9.1 · FR57

As a canteen cashier,
I want each item priced at its effective sale price and blocked when stock is insufficient, even when two counters sell at once,
So that the canteen never sells at a wrong price or below zero stock.

**Acceptance Criteria:**

**Given** the price query from Story 8.4
**When** a sale line is added for date D
**Then** `DonGiaBan` is the `BangGia` row with the greatest `ApDungTuNgay` ≤ D; if no price exists the line is rejected with "Mặt hàng chưa có giá bán"
**And** the cashier cannot type over the price

**Given** the Infrastructure stock deduction used inside the sale transaction
**When** it executes
**Then** it runs `UPDATE HangHoa WITH (UPDLOCK, ROWLOCK) SET SoLuongTon = SoLuongTon - @SoLuong OUTPUT deleted.SoLuongTon, deleted.GiaVonBQ WHERE Id = @Id AND SoLuongTon >= @SoLuong AND DangKinhDoanh = 1`
**And** 0 affected rows raises `HetHangException` (out of stock or discontinued), never a read-then-write

**Given** an item with stock 5 and two sessions on LocalDB each selling 4 at the same time
**When** both commit
**Then** exactly one succeeds, the other gets `HetHangException`, the final stock is 1, and the `TheKho` rows reconcile (integration test)

**Given** a line quantity with more than 3 decimals or ≤ 0
**When** it is validated
**Then** it is rejected (quantities are `decimal(18,3)`)

---

### Story 10.2: POS sale invoice for cash buyers

`BH-02, BH-06` · Size L · Depends on: 10.1, 4.2, 4.5, 4.7 · FR55, FR59

As a canteen cashier,
I want to ring up a cash sale quickly on a POS screen for a staff member or a visiting unit,
So that serving a queue is fast and every line records its cost of goods.

**Acceptance Criteria:**

**Given** the tables `PhieuBanHang` (`Id`, `SoPhieu varchar(20)` UQ, `NgayXuat date`, `LoaiNguoiMua tinyint` 1 đối tượng / 2 người thân / 3 CBCS / 4 đơn vị đến công tác, `DoiTuongId NULL` FK, `TenNguoiMua`, `DiaChiNguoiMua` snapshot, `HinhThucThanhToan tinyint` 1 trừ lưu ký / 2 tiền mặt, `TongTien decimal(18,0)`, `SoDuTruoc`, `SoDuSau NULL`, `CanBoBanId` FK → CanBo, `TrangThai` 1 / 2 / 3, `LyDoHuy`, `NgayHuy`, `NguoiHuyId`, `SoLanIn`, audit columns) and `PhieuBanHangChiTiet` (`Id bigint`, `PhieuBanHangId`, `ThuTu`, `HangHoaId` FK indexed, `SoLuong decimal(18,3) CHECK (> 0)`, `DonGiaBan decimal(18,0)`, `ThanhTien decimal(18,0)`, `DonGiaVon decimal(18,4)`, `GiaVon decimal(18,0)`)
**When** the migration runs
**Then** the DB design's CHECKs exist: `LoaiNguoiMua NOT IN (1,2) OR DoiTuongId IS NOT NULL` and `HinhThucThanhToan <> 1 OR DoiTuongId IS NOT NULL`; the `TheKho.PhieuBanHangChiTietId` FK is added; the enum test covers both new enums

**Given** the POS screen (UX-DR8)
**When** I type a code or part of a name (diacritics optional), or scan a barcode with a USB keyboard-wedge scanner
**Then** the matching item is found and Enter adds a line with quantity 1 (scanning the same item again increases the quantity), the quantity is editable inline, and the running total is shown in a large font

**Given** buyer type 3 (CBCS) or 4 (visiting unit)
**When** I enter the buyer name (and address for a unit)
**Then** per the DEC-04 assumption the payment method is fixed to cash (2), no detainee picker is shown and no balance fields are filled

**Given** a completed cart
**When** I post the sale
**Then** in one transaction the number `PB` is allocated, the invoice and lines are saved as posted, each line's stock is deducted (Story 10.1) and `DonGiaVon` is set to `GiaVonBQ` at issue with `GiaVon = Round(SoLuong × DonGiaVon, 0, AwayFromZero)` (the same rule as Story 9.1), and one `TheKho` row with `LoaiBienDong = 2` is written per line
**And** `ThanhTien = SoLuong × DonGiaBan` rounded to đồng, `TongTien` = Σ `ThanhTien`, and the screen clears for the next buyer
**And** any failure (out of stock, no price) rolls back the whole sale and names the line

---

### Story 10.3: Sale to a detainee or relative with automatic custodial debit

`BH-05, LK-C05` · Size M · Depends on: 10.2, 4.3, 3.2, 5.1 · FR58, FR33

As a canteen cashier,
I want a sale to a detainee, or to a relative buying for them, debited straight from that detainee's custodial balance in the same transaction,
So that no separate payout is ever typed by hand and the balance can never go negative.

**Acceptance Criteria:**

**Given** buyer type 1 (detainee) or 2 (relative)
**When** I select the detainee with the shared picker (Story 3.2)
**Then** `DoiTuongId` is required and, per the DEC-03 assumption, the payment method is fixed to custodial debit (1); for type 2 the relative's name is entered as `TenNguoiMua`
**And** the screen shows the current balance and the balance after this cart

**Given** a posted cart for a detainee
**When** the sale posts
**Then** in the same transaction `GhiSoLuuKyService` posts a payout `ChungTuLuuKy` with `NghiepVu = 21` (mua hàng), `SoTien = TongTien`, `PhieuBanHangId` = the invoice (filtered UQ: one sale → exactly one payout), and the engine's `SoDuTruoc` / `SoDuSau` are copied onto the invoice
**And** the pre-posting confirmation (Story 4.5) shows "balance before → amount → balance after"

**Given** a balance of 50,000 đ and a cart of 60,000 đ
**When** I post
**Then** the whole sale is refused with "Không đủ số dư lưu ký" (the engine's conditional UPDATE returns 0 rows), and no invoice, stock or ledger change is saved

**Given** a detainee whose status is not "đang quản lý"
**When** a sale is attempted
**Then** it is refused by the engine rule (FR35)

**Given** the canteen module is enabled (setting `CanTinDaKichHoat = true` at R1 go-live)
**When** a user tries to create a manual payout of type 21 (Story 5.1)
**Then** it is refused: type-21 payouts can only be generated by a sale (LK-C05)

**Given** two counters selling to the same detainee at the same time with a combined total above the balance
**When** both post
**Then** exactly one succeeds (integration test), and reconciliation of both balance and stock passes afterwards

---

### Story 10.4: Print the purchase slip

`BH-07` · Size S · Depends on: 10.3, 4.4 · FR60

As a canteen cashier,
I want to print the purchase slip, showing the balance before and after for custodial buyers,
So that the buyer signs for exactly what was bought and what money is left.

**Acceptance Criteria:**

**Given** a posted sale
**When** I print it with template `PHIEU_MUA_HANG`
**Then** the frame renders the unit name and address, the title "PHIẾU MUA HÀNG", Tên người mua, Địa chỉ, Loại người mua, Mã số đối tượng (types 1/2), and the line table Tên mặt hàng, Mã hàng, Đơn vị tính, Số lượng, Đơn giá, Thành tiền, with the total in figures and words

**Given** buyer type 1 or 2
**When** the slip prints
**Then** it also shows Số tiền kỳ trước chuyển sang (`SoDuTruoc`), Số tiền mua hàng kỳ này (`TongTien`) and Số tiền còn được sử dụng (`SoDuSau`), all taken from the stored snapshot, not recalculated
**And** the detainee's signature line is always printed and cannot be removed from the configuration for this template

**Given** the signature block
**When** it renders
**Then** it shows Người mua hàng, Cán bộ căn tin, Lãnh đạo đơn vị from `CauHinhKyTen`; reprints show "BẢN IN LẠI" with the count and are audit-logged

---

### Story 10.5: Cancel a sale

`BH-08` · Size M · Depends on: 10.3, 9.4, 6.8 · FR61

As a canteen cashier,
I want to cancel a wrong sale with a reason and have the stock and the detainee's money returned automatically,
So that corrections never leave stock or balances out of step.

**Acceptance Criteria:**

**Given** a posted sale in an open period and a user with `BH.Huy`
**When** I cancel it with a mandatory reason
**Then** in one transaction the invoice status becomes cancelled (`LyDoHuy`, `NgayHuy`, `NguoiHuyId`), a `TheKho` row with `LoaiBienDong = 4` returns each line's quantity at its original `DonGiaVon`, `HangHoa.SoLuongTon` increases, and for types 1/2 the linked type-21 payout is cancelled through `GhiSoLuuKyService`'s reversal, restoring the balance
**And** if the sale is not the item's latest movement, the engine in Story 9.4 recalculates the later rows

**Given** the type-21 payout generated by a sale
**When** someone tries to cancel that payout directly from the payout screens
**Then** it is refused with "Huỷ qua phiếu bán hàng"

**Given** a sale already printed
**When** I cancel it
**Then** I am warned that the slip was printed (`SoLanIn > 0`) and must confirm, and the cancelled-documents report (Story 6.6) includes it

**Given** a sale dated in a locked period
**When** a cancellation is attempted
**Then** it is refused by the service

**Given** the cancellation integration test
**When** it finishes
**Then** both balance and stock reconciliation pass and the detainee's balance equals its value before the sale
