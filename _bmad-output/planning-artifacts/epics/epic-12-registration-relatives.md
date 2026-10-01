---
epic: 12
title: Warden purchase registration & relatives
release: R2
frsCovered: [FR16, FR26, FR54, FR56]
backlogItems: [DM-03, LK-T04, BH-01, BH-03]
dependsOn: [1, 2, 3, 4, 8, 9, 10, 11]
---

## Epic 12: Warden purchase registration & relatives

Wardens register canteen purchases for the detainees they manage, and canteen staff turn those registrations into sale invoices in one batch per cell instead of keying each one. Custodial officers stop retyping relatives' details: the relatives register suggests the sender and bank account on every receipt and flags a second deposit in the same month.

**Exit criteria (R2 gate for this epic):** DEC-08 is decided by the PO. Stories 12.3 and 12.4 assume wardens enter registrations in the software and are **blocked until DEC-08 is decided**. Balance and stock reconciliation show zero difference after a batch conversion.

**Applies to every story in this epic:** services re-check permissions before writing (2.5); every write goes through the audit interceptor (1.3); dates come from `IClock`; no balance is changed outside `GhiSoLuuKyService` (4.3).

---

### Story 12.1: Relatives register and suggestion on receipts

`DM-03` · Size M · Depends on: 3.2, 4.5 · FR16

As a custodial officer,
I want to keep each detainee's relatives with their relationship and bank account, and have them suggested when I record a receipt,
So that I don't retype the same sender details every month and transfer receipts always carry a correct account number.

**Acceptance Criteria:**

**Given** the new `NguoiThan` table (`Id`, `DoiTuongId` FK → DoiTuong, `HoTen nvarchar(100) NOT NULL`, `QuanHe nvarchar(50)`, `DiaChi nvarchar(300)`, `SoTaiKhoan varchar(30)`, `NganHang nvarchar(100)`, audit columns) and the column `ChungTuLuuKy.NguoiThanId` (FK, NULL) added in this story's migration
**When** a user with permission `DM-03.Them` adds or edits a relative from the detainee form
**Then** the relative is saved and audit-logged, and a user without the permission is refused by the service even if the button is shown

**Given** the receipt form (4.5) with source "sent by a relative" (`NghiepVu` 12) and a detainee selected
**When** the officer focuses the sender field
**Then** that detainee's relatives are listed (name, relationship, account), and choosing one fills `NguoiGuiHoTen`, `QuanHe` and, for a bank transfer, `SoTaiKhoanNguoiGui`, and sets `NguoiThanId`

**Given** a receipt posted with a chosen relative
**When** the relative's name or account is later edited in the register
**Then** the posted receipt and its reprint still show the snapshot values captured at posting

**Given** a sender who is not in the register
**When** the officer types a new name on the receipt
**Then** the receipt posts with `NguoiThanId = NULL`, and the form offers "Save to relatives register" without requiring it

---

### Story 12.2: Warning on a repeat deposit in the same month

`LK-T04` · Size S · Depends on: 12.1 · FR26

As a custodial officer,
I want to be warned when a relative has already deposited for the same detainee this month,
So that I can check the monthly-visit rule without being stopped from recording money that has actually been handed over.

**Acceptance Criteria:**

**Given** a posted receipt with `NghiepVu` 12 for detainee A from relative R dated in the current calendar month (`IClock.Today`)
**When** the officer prepares another receipt with `NghiepVu` 12 for A, matched by the same `NguoiThanId`, or by the same sender name compared case- and diacritic-insensitively when either side has no `NguoiThanId`
**Then** before posting, a warning lists the earlier receipt(s) (number, date, amount) and the officer can still post

**Given** the warning was shown and the officer posts anyway
**When** the receipt is saved
**Then** posting completes normally, and the audit row for the posting records that the repeat-deposit warning was acknowledged

**Given** an earlier receipt that was cancelled, or is dated in a previous month, or is from another source (11, 13, 14)
**When** the check runs
**Then** it does not trigger the warning

---

### Story 12.3: Warden purchase registration

`BH-01` · Size M · Depends on: 2.5, 3.2, 8.3, 8.4, 4.3 · FR54 · **Blocked until DEC-08**

As a warden,
I want to register canteen purchases for the detainees I manage, with the balance checked as I register,
So that the canteen receives clean, affordable orders instead of paper slips.

**Acceptance Criteria:**

**Given** the new tables `DangKyMuaHang` (`Id`, `SoPhieu varchar(20)` UQ from `INumberingService` type `DK`, `NgayDangKy date`, `DoiTuongId`, `CanBoQuanGiaoId`, `TongTienDuKien decimal(18,0)`, `TrangThai tinyint` CHECK 1 mới / 2 đã chuyển bán / 3 huỷ, `PhieuBanHangId` NULL FK, `LyDoHuy`) and `DangKyMuaHangChiTiet` (`Id`, `DangKyMuaHangId`, `HangHoaId`, `SoLuong decimal(18,3) CHECK (> 0)`, `DonGiaDuKien decimal(18,0)`)
**When** the migration is applied
**Then** the matching Domain enum passes the enum ↔ CHECK test (1.2)

**Given** a user with permission `BH-01.Them` linked to staff member W
**When** W registers for a detainee whose `CanBoQuanGiaoId = W`
**Then** the registration is saved with status 1, each line's `DonGiaDuKien` is the effective price on `NgayDangKy` (8.4), and `TongTienDuKien` = Σ quantity × price
**And** discontinued goods cannot be chosen

**Given** a detainee assigned to another warden, or a detainee whose status ≠ managed
**When** W tries to register for them
**Then** the service refuses with a clear message, whatever the UI showed

**Given** the detainee's current `SoDuLuuKy` and the total of their other open registrations (status 1)
**When** open total + new `TongTienDuKien` > `SoDuLuuKy`
**Then** the registration is blocked with a message showing balance, open total and the shortfall

**Given** a saved registration
**When** it is saved, edited or cancelled (status 3 with a mandatory reason, allowed only while status 1)
**Then** nothing is posted to `ChungTuLuuKy` or `TheKho`, no stock is reserved, and each change is audit-logged

---

### Story 12.4: Convert registrations into sale invoices by batch or by cell

`BH-03` · Size M · Depends on: 12.3, 10.3 · FR56 · **Blocked until DEC-08**

As a canteen officer,
I want to convert open registrations into sale invoices for a whole batch or one cell at once,
So that I can serve a cell in one pass while each purchase still debits the right detainee correctly.

**Acceptance Criteria:**

**Given** open registrations (status 1)
**When** a user with permission `BH-03.Them` filters by registration date range and/or `BuongGiam`, then selects some or all of them
**Then** the screen shows each registration with detainee, expected total and current balance before anything is posted

**Given** the selection is confirmed
**When** the conversion runs
**Then** each registration becomes exactly one sale invoice through the same service as 10.3, at the effective price on the sale date (not the registration price), with stock deducted under a row lock and the custodial debit posted through `GhiSoLuuKyService`, **one transaction per registration**
**And** a converted registration gets `TrangThai = 2` and `PhieuBanHangId` set in that same transaction

**Given** one registration fails (insufficient balance, out of stock, detainee no longer managed, locked period)
**When** the batch continues
**Then** that registration stays at status 1 with nothing posted, the others are still converted, and a result report lists each registration as success (invoice number) or failure (reason)

**Given** a converted registration
**When** someone tries to convert, edit or cancel it again
**Then** the service refuses; cancelling its sale invoice (10.5) does not reopen the registration

**Given** the batch integration test on LocalDB
**When** it finishes
**Then** ledger reconciliation (6.7) and stock reconciliation (11.4) report zero difference
