---
epic: 8
title: Canteen catalogue & pricing
release: R1
frsCovered: [FR18, FR19, FR20, FR21, FR22]
backlogItems: [DM-05, DM-06, DM-07, DM-08, DM-09]
dependsOn: [1, 2, 3, 4, 6]
---

## Epic 8: Canteen catalogue & pricing

Canteen staff keep the master data the canteen runs on: suppliers, units of measure, goods, and dated sale prices whose history can never be rewritten. They print the goods catalogue (*Danh mục hàng hoá*) and the posted price list (*Bảng niêm yết giá*), and load the initial goods list from Excel. Stock and cost columns exist from this epic on, but only the goods-receipt and sales engines (Epics 9–10) change them.

**Exit criteria:** every active item has a code, UoM, category and an effective price; both templates print and match the paper forms.

**Applies to every story in this epic:**
- Permissions are checked in the service (Story 2.5): view, add and edit use the `DM` permissions; printing uses `.In`.
- Every change is audit-logged by the interceptor (Story 1.3).
- Names use `Vietnamese_CI_AI`, so search ignores diacritics.
- Records are deactivated, never hard-deleted, once anything references them.

---

### Story 8.1: Suppliers

`DM-05` · Size S · Depends on: 1.2, 2.5 · FR18

As a canteen officer,
I want to maintain the list of suppliers (*cửa hàng bách hoá*) I buy goods from,
So that every goods receipt names a known supplier with the correct address.

**Acceptance Criteria:**

**Given** the `NhaCungCap` table (`Id`, `MaNCC varchar(20)` UQ, `Ten nvarchar(200)` UQ, `DiaChi nvarchar(300)`, `DienThoai varchar(20)`, `DangGiaoDich bit` default 1, audit columns)
**When** I add a supplier with code "NCC01", name "Bách hoá Minh Anh" and an address
**Then** it is saved and appears in the supplier list, searchable by code or name without diacritics

**Given** an existing supplier code or name
**When** I save another supplier with the same code or the same name
**Then** a validation message names the duplicate field and nothing is saved

**Given** a supplier that is already referenced by a goods receipt (from Epic 9 on)
**When** I try to delete it
**Then** deletion is refused and I'm told to set `DangGiaoDich = 0` instead; inactive suppliers are hidden from selection lists but remain on historical documents

**Given** a user without `DM.Them` or `DM.Sua`
**When** the service is called directly (bypassing hidden buttons)
**Then** it throws an authorization error and writes nothing

---

### Story 8.2: Units of measure

`DM-06` · Size S · Depends on: 1.2, 2.5 · FR19

As a canteen officer,
I want a shared list of units of measure,
So that goods are counted the same way on receipts, sales and reports.

**Acceptance Criteria:**

**Given** the `DonViTinh` table (`Id`, `Ten nvarchar(30)` UQ)
**When** the migration runs on a new database
**Then** it seeds "Cái", "Gói", "Hộp", "Chai", "Kg", "Thùng" inside the migration (Story 1.2 convention)

**Given** the UoM list
**When** I add "Lon" or rename an unused unit
**Then** the change is saved; a name that duplicates an existing one ignoring case and diacritics is rejected

**Given** a unit used by at least one item
**When** I try to delete it
**Then** deletion is refused with "Đơn vị tính đang được sử dụng"

---

### Story 8.3: Goods and goods catalogue print

`DM-07` · Size M · Depends on: 8.2, 4.4 · FR20

As a canteen officer,
I want to maintain the goods list with code, name, UoM, category and a discontinued flag, and print the goods catalogue,
So that sales and receipts use one consistent catalogue and leadership can sign the official list.

**Acceptance Criteria:**

**Given** the `HangHoa` table (`Id`, `MaHang varchar(30)` UQ, `TenHang nvarchar(200)` NOT NULL with index, `DonViTinhId` FK, `LoaiMatHang tinyint` 1 = phục vụ sinh hoạt / 2 = thực phẩm with CHECK, `GhiChu nvarchar(500)`, `DangKinhDoanh bit` default 1, `SoLuongTon decimal(18,3)` default 0 `CHECK (>= 0)`, `GiaVonBQ decimal(18,4)` default 0, audit columns, `RowVer`)
**When** I add item "MI001 – Mì tôm Hảo Hảo", UoM "Gói", category "Thực phẩm"
**Then** it is saved with stock 0 and average cost 0
**And** the enum-consistency test (Story 1.2) covers `LoaiMatHang`

**Given** the goods form
**When** I edit an item
**Then** `SoLuongTon` and `GiaVonBQ` are shown read-only; no service in this epic can change them (only the stock engines in Epics 9–10)
**And** a concurrent edit by another workstation is detected through `RowVer` and I'm asked to reload

**Given** an item with stock movements
**When** I set `DangKinhDoanh = 0`
**Then** it disappears from sale and receipt pickers but its history, reports and stock card stay intact; it cannot be deleted

**Given** the template `DANH_MUC_HANG_HOA`
**When** I print the catalogue (optionally only active items)
**Then** the shared print frame (Story 4.4) renders the unit header, title "DANH MỤC HÀNG HOÁ", columns STT, Mã hàng, Tên hàng, Đơn vị tính, Ghi chú, and the signers configured in `CauHinhKyTen`: Cán bộ căn tin, Chỉ huy phụ trách, Lãnh đạo đơn vị
**And** the print is counted and audit-logged

---

### Story 8.4: Sale prices with effective dates and the posted price list

`DM-08` · Size M · Depends on: 8.3, 1.3, 4.4 · FR21

As a canteen officer,
I want to set a sale price that takes effect from a future date, keep a full price history, and print the posted price list,
So that detainees always see the official price and no past sale can be repriced.

**Acceptance Criteria:**

**Given** the `BangGia` table (`Id`, `HangHoaId` FK, `GiaBan decimal(18,0) CHECK (> 0)`, `ApDungTuNgay date`, UQ (`HangHoaId`, `ApDungTuNgay`))
**When** I enter a new price for an item
**Then** `ApDungTuNgay` defaults to tomorrow (`IClock.Today + 1`) and any date ≤ today is rejected with "Giá mới chỉ có hiệu lực từ ngày mai"

**Given** a price row whose `ApDungTuNgay` ≤ today (current or historical)
**When** anyone tries to edit or delete it, through the UI or a direct service call
**Then** the change is refused; price history is immutable

**Given** a future price row not yet in effect
**When** I change or remove it before its date
**Then** the change is allowed and audit-logged with before/after values

**Given** an item with prices dated 01/09 (10,000 đ) and 15/09 (12,000 đ)
**When** the Application query `GetGiaBanHieuLuc(hangHoaId, ngay)` runs for 14/09, 15/09 and 31/08
**Then** it returns 10,000; 12,000; and "no price" (the row with the greatest `ApDungTuNgay` ≤ the date), covered by unit tests

**Given** the template `BANG_NIEM_YET_GIA`
**When** I print the posted price list for a chosen date (default today)
**Then** it lists active items with an effective price on that date, with columns STT, Tên mặt hàng, Loại mặt hàng (sinh hoạt / thực phẩm), Đơn vị tính, Giá bán, and signers Cán bộ căn tin and Lãnh đạo đơn vị

---

### Story 8.5: Import goods from Excel

`DM-09` (goods part) · Size S · Depends on: 8.3, 8.4, 3.4 · FR22

As a canteen officer,
I want to load the initial goods list, and optionally the first prices, from an Excel template,
So that go-live doesn't need hundreds of items typed by hand.

**Acceptance Criteria:**

**Given** the Excel import pattern from Story 3.4 (ClosedXML)
**When** I download the goods template
**Then** it has the columns Mã hàng, Tên hàng, Đơn vị tính, Loại mặt hàng, Ghi chú, Giá bán (optional), Áp dụng từ ngày (optional), with one example row

**Given** a filled file
**When** I open it in the import wizard (UX-DR9)
**Then** the preview grid flags each invalid row with its reason: missing code or name, code duplicated in the file or in the DB, unknown UoM, category not 1/2, price ≤ 0, or effective date ≤ today
**And** nothing is written while any error remains (all-or-nothing)

**Given** a file with no errors
**When** I confirm the import
**Then** all items, and any prices, are inserted in one transaction, the audit log records the import, and a summary shows the number of items and prices created
