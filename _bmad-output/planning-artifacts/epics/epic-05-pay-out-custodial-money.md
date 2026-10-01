---
epic: 5
title: Pay out custodial money
release: R0.5
frsCovered: [FR29, FR30, FR31, FR32, FR34]
backlogItems: [LK-C01, LK-C02, LK-C03, LK-C04, LK-C06, NEN-17]
dependsOn: [1, 2, 3, 4]
---

## Epic 5: Pay out custodial money

The custodial officer pays money out of a detainee's custodial balance, moves money between detainees after Board of Wardens approval, and settles a detainee's account in one click on transfer or release. Every payout goes through `GhiSoLuuKyService`, so a balance never goes negative, even when two workstations pay out for the same detainee at the same moment.

**Payout-type rules (alignment note A11):**

| `NghiepVu` | Meaning | How it is created |
|---|---|---|
| 21 mua hàng | Canteen purchase | Manual payout form **only while the canteen module is not active** (during R0.5 the canteen still runs on paper). Story 10.3 disables manual 21 once sales generate it automatically |
| 22 cho tiền | Give money to another detainee | Generated only by transfer approval (Story 5.5) |
| 23 chuyển về người thân | Send money to a relative | Manual payout form (own type per DEC-02) |
| 24 chuyển trại | Transfer to another facility | Generated only by settlement (Story 5.6) |
| 25 chấp hành xong án | Sentence completed / release | Generated only by settlement (Story 5.6) |

**Alignment note A13:** the spec does not say explicitly that a payout can be edited or cancelled. Payouts follow the shared voucher state machine (Draft → Posted → Cancelled with reason), the same as receipts.

**Applies to every story in this epic:** services check the caller's permission before writing (codes `LK-C.Xem`, `LK-C.Them`, `LK-C.Sua`, `LK-C.Huy`, `LK-C.In`, `LK-C.Duyet`); every change is audit-logged through the interceptor (Story 1.3); all dates come from `IClock`; each operation is one service method and one transaction; the reconciliation assertion (Story 4.3) runs after every integration test.

---

### Story 5.1: Create and post a payout voucher

`LK-C01, LK-C02, NEN-17` · Size M · Depends on: 4.3, 4.5 · FR29, FR30

As a custodial officer,
I want to record a payout from a detainee's custodial balance and post it,
So that money leaving the balance is recorded with a numbered voucher and never exceeds what the detainee has.

**Acceptance Criteria:**

**Given** the payout form (`LoaiPhieu = 2`)
**When** the officer opens it
**Then** it shows the fields from process A.II.5: document number (assigned at posting), document date, detainee picked with the shared picker (name, code, type shown), description (`NoiDung`), payout type, amount (shared money input), amount in words (generated, read-only)
**And** the type list offers only 23 "Chuyển về người thân" and, while the canteen module is not active (setting `CanTinDaKichHoat`, default `false`, introduced here), 21 "Mua hàng" (A11); 22, 24 and 25 are never offered

**Given** a draft payout
**When** it is saved
**Then** it is stored with `TrangThai = 1` (nháp), no document number and no balance change
**And** FluentValidation rejects a missing detainee, a missing type, `SoTien <= 0` and an empty description, each with a clear message

**Given** a draft payout of 300,000 đ for a detainee whose balance is 500,000 đ
**When** the officer clicks Post
**Then** the pre-posting confirmation shows "Số dư trước 500.000 → Chi 300.000 → Số dư sau 200.000" and the amount in words
**And** on confirm, in one transaction: the `PC` number for the current year is allocated, `GhiSoLuuKyService` debits the balance through the conditional `UPDATE … WITH (UPDLOCK, ROWLOCK) … WHERE SoDuLuuKy >= @SoTien`, the voucher becomes `TrangThai = 2` with `SoDuTruoc = 500000`, `SoDuSau = 200000`, the amount in words and the detainee name/type snapshot

**Given** a payout larger than the current balance
**When** it is posted
**Then** nothing is written (no number consumed, no balance change) and the officer sees "Số dư không đủ. Số dư hiện tại: 200.000 đồng"

**Given** a detainee with `TrangThai ≠ 1` (not managed)
**When** a payout is posted
**Then** the engine refuses it with "Đối tượng không còn quản lý, không được lập chứng từ"

**Given** two parallel tasks on LocalDB, each posting a 450,000 đ payout for a detainee with a 500,000 đ balance
**When** both run at the same time
**Then** exactly one succeeds, the other gets the insufficient-balance result, and the final balance is 50,000 đ (NEN-17)

**Given** a user without `LK-C.Them`
**When** the service is called directly
**Then** it throws an authorization error and writes nothing

---

### Story 5.2: Print the payout voucher

`LK-C06` · Size S · Depends on: 4.4, 5.1 · FR34

As a custodial officer,
I want to print the *Phiếu chi xuất tiền lưu ký* for a posted payout,
So that the detainee, the warden and the leader sign a paper copy that matches the record.

**Acceptance Criteria:**

**Given** a posted payout
**When** the officer clicks Print
**Then** the `PHIEU_CHI` template renders through the shared print frame with: unit name and address, title "PHIẾU CHI XUẤT TIỀN LƯU KÝ", document number and date, detainee name, code and type (from the snapshot), "Số tiền lưu ký kỳ trước chuyển sang" (`SoDuTruoc`), description, payout type, amount in figures and words, "Số tiền lưu ký còn được sử dụng" (`SoDuSau`)

**Given** the signer configuration for `PHIEU_CHI` (seeded per the process doc)
**When** the voucher is printed
**Then** 4 signature columns appear left to right: "Cán bộ theo dõi tiền lưu ký", "Người bị tạm giữ, tạm giam/phạm nhân xác nhận", "Cán bộ quản giáo xác nhận", "Lãnh đạo đơn vị xác nhận", with default names where configured

**Given** a voucher already printed once
**When** it is printed again
**Then** `SoLanIn` increases, the print is audit-logged as `In`, and the copy carries the "BẢN IN LẠI (lần n)" watermark

**Given** a draft payout
**When** the officer tries to print it
**Then** printing is disabled (draft preview is a later option, TI-08)

**Given** the golden-file test for `PHIEU_CHI`
**When** it renders a fixed sample voucher
**Then** the output matches the approved snapshot, which has been checked against the paper form in use

---

### Story 5.3: Payout list, edit draft and cancel a posted payout

`LK-C01` (state machine, A13) · Size M · Depends on: 5.1, 5.2

As a custodial officer,
I want to find payouts, correct drafts and cancel a wrong posted payout with a reason,
So that mistakes are fixed openly and the balance is restored exactly.

**Acceptance Criteria:**

**Given** the payout list (shared voucher grid)
**When** the officer filters by date range, detainee, type and status
**Then** matching payouts are listed with number, date, detainee, type, amount, status and print count, and the grid exports to Excel from the right-click menu

**Given** a draft payout
**When** the officer edits and saves it
**Then** the changes are saved and audit-logged as `Sua`; posted and cancelled payouts are read-only

**Given** a posted manual payout (type 21 or 23) dated in an open period
**When** the officer cancels it with a reason
**Then** in one transaction the engine reverses the debit (balance increases by `SoTien`), the voucher becomes `TrangThai = 3` with `LyDoHuy`, `NgayHuy` (from `IClock`) and `NguoiHuyId`, and the audit log records `Huy`
**And** cancelling without a reason is rejected

**Given** a posted payout that has been printed (`SoLanIn > 0`)
**When** the officer starts to cancel it
**Then** a warning says the paper copy must be recovered, and the officer must confirm

**Given** a posted payout dated in a locked period
**When** a cancel is attempted
**Then** the engine refuses with "Kỳ đã khoá sổ"

**Given** a generated payout (22 from a transfer, 24/25 from a settlement, 21 created from a sale)
**When** the officer tries to edit or cancel it here
**Then** the action is unavailable and the service refuses it; it can only be undone through its source document

**Given** a user without `LK-C.Huy`
**When** the cancel service is called
**Then** it is refused and nothing changes

---

### Story 5.4: Request a transfer to another detainee

`LK-C03` (request part) · Size M · Depends on: 5.1

As a custodial officer,
I want to record a detainee's request to give money to another detainee,
So that the Board of Wardens can review it before any money moves.

**Acceptance Criteria:**

**Given** a migration that creates `DeNghiChoTien` (`Id`, `SoBienBan varchar(30) UQ`, `NgayDeNghi date`, `DoiTuongChoId`, `DoiTuongNhanId` with `CHECK (DoiTuongChoId <> DoiTuongNhanId)`, `SoTien decimal(18,0) CHECK (> 0)`, `LyDo nvarchar(500)`, `TrangThai tinyint` 1 chờ duyệt / 2 đã duyệt / 3 từ chối, `NguoiDuyetId` FK → CanBo, `NgayDuyet`, `ChungTuChiId`, `ChungTuThuId` FK → ChungTuLuuKy) and `ChungTuLuuKy.DeNghiChoTienId`
**When** the enum-consistency test runs
**Then** the request-status enum matches the CHECK constraint

**Given** the request form
**When** the officer picks a giver and a receiver with the detainee picker and enters the minutes number, date, amount and reason
**Then** the request is saved as `TrangThai = 1` with the creator recorded
**And** a duplicate `SoBienBan`, the same detainee on both sides, or an amount ≤ 0 is rejected

**Given** a giver whose current balance is lower than the requested amount
**When** the request is saved
**Then** the form shows the giver's current balance and a warning, but the request may still be saved (the balance is re-checked at approval)

**Given** a giver or receiver whose `TrangThai ≠ 1`
**When** the request is saved
**Then** it is rejected

**Given** a pending request
**When** its creator edits or withdraws it
**Then** editing is allowed only while it is pending; withdrawal sets it to rejected with the reason "Rút đề nghị"; both are audit-logged

---

### Story 5.5: Approve or reject a transfer request

`LK-C03` · Size L · Depends on: 2.6, 5.4 · FR31

As a member of the Board of Wardens (commander or unit leader),
I want to approve or reject pending transfer requests,
So that money moves between detainees only with authorization, and both sides are recorded at once.

**Acceptance Criteria:**

**Given** a user with `LK-C.Duyet`
**When** they open the pending-requests list
**Then** they see each pending request with minutes number, date, giver (name, code, current balance), receiver, amount, reason and creator

**Given** a request created by the current user
**When** that user tries to approve it
**Then** the segregation-of-duties policy (Story 2.6) refuses it with "Người lập không được tự duyệt", both in the UI and in the service

**Given** a pending request of 100,000 đ where the giver's balance is 400,000 đ
**When** the approver approves it
**Then** in **one transaction**: a payout `NghiepVu = 22` (cho tiền) is posted for the giver and a receipt `NghiepVu = 14` (nhận từ đối tượng khác) for the receiver, both through `GhiSoLuuKyService`, both with `SoPhieuGoc = SoBienBan` and `DeNghiChoTienId` set; the request becomes `TrangThai = 2` with `NguoiDuyetId`, `NgayDuyet`, `ChungTuChiId` and `ChungTuThuId`
**And** the audit log records `Duyet` for the request plus the two generated documents

**Given** the giver's balance is lower than the amount at the moment of approval
**When** the approver approves
**Then** the whole approval is rolled back (no payout, no receipt, request still pending) and the message shows the giver's current balance

**Given** either detainee is no longer managed, or the document date falls in a locked period
**When** the approver approves
**Then** the approval is refused and nothing is written

**Given** a pending request
**When** the approver rejects it
**Then** a reason is required, the request becomes `TrangThai = 3`, no money moves, and the action is audit-logged

**Given** an approved request
**When** someone tries to cancel one of its generated documents directly
**Then** it is refused; reversing a transfer means cancelling both documents together through the request, with a reason, in one transaction

---

### Story 5.6: One-click settlement on transfer or release

`LK-C04` · Size M · Depends on: 5.2, 5.5 · FR32

As a custodial officer,
I want to settle a detainee's account in one step when they transfer to another facility or are released,
So that the full balance is paid out, the paperwork is printed, and the record cannot be used again.

**Acceptance Criteria:**

**Given** a managed detainee
**When** the officer starts settlement
**Then** the dialog shows the current balance and asks for the reason, 24 "Chuyển trại" or 25 "Chấp hành xong án", the exit date `NgayRa` (default `IClock.Today`) and a description

**Given** a detainee with pending transfer requests (as giver or receiver)
**When** settlement starts
**Then** it is blocked with a list of those requests, which must be approved or rejected first

**Given** a detainee with a balance of 250,000 đ
**When** the officer confirms settlement with reason 25
**Then** in one transaction: a payout `NghiepVu = 25` of 250,000 đ is posted through the engine (`SoDuSau = 0`), `DoiTuong.TrangThai = 3` and `NgayRa` are set, and both changes are audit-logged
**And** the payout opens in print preview immediately afterwards

**Given** a detainee with a balance of 0
**When** settlement is confirmed
**Then** no payout is created; only the status and `NgayRa` change

**Given** a settled detainee (`TrangThai` 2 or 3)
**When** anyone tries to post a receipt, payout, transfer or sale for them
**Then** the engine refuses it, and the detainee picker shows the red "Đã tất toán" label

**Given** any attempt to set `TrangThai ≠ 1` while `SoDuLuuKy > 0`
**When** it is saved by any path other than settlement
**Then** it is rejected ("Chỉ đóng hồ sơ khi số dư bằng 0")

**Given** a user without `LK-C.Them`
**When** the settlement service is called
**Then** it is refused and nothing changes
