---
epic: 4
title: Receive custodial money
release: R0.5
frsCovered: [FR7, FR8, FR23, FR24, FR27, FR28, FR35, FR42, FR43, FR44]
backlogItems: [NEN-13, NEN-14, NEN-15, HT-05, GAP-03, NEN-16, NEN-17, NEN-18, NEN-19, NEN-20, NEN-21, NEN-22, NEN-23, NEN-24, LK-T01, LK-T02, LK-T05, LK-T06]
dependsOn: [1, 2, 3]
---

## Epic 4: Receive custodial money

The custodial officer records money that detainees bring in on arrival or that relatives send. They print the official receipt (*Biên nhận thu tiền gửi lưu ký*) and cancel a wrong receipt with a reason. Behind the form is the single ledger engine `GhiSoLuuKyService`, built here to its full form. It numbers documents without collisions across workstations, updates balances under a row lock, never lets a balance go negative, refuses inactive detainees and locked periods, and can always be reconciled against the ledger. Every later money flow (payouts, transfers, settlement, canteen sales, opening balances) calls this engine.

**Exit criteria:** a receipt can be created, posted, printed, reprinted (with a reprint mark) and cancelled; the concurrency tests for numbering and double-spend pass on LocalDB; reconciliation shows 0 difference after every integration test.

**Applies to every story in this epic:** money is `long`/`decimal(18,0)` integer đồng; every posting operation is one service method in one transaction; only `GhiSoLuuKyService` writes `ChungTuLuuKy` and `DoiTuong.SoDuLuuKy`; services check permission before writing; the Definition of Done reconciliation assertion runs after every integration test.

---

### Story 4.1: Ledger domain rules: balance, voucher state machine, snapshot

`NEN-13, NEN-14, NEN-15` · Size M · Depends on: 1.1

As a custodial officer,
I want the core money rules fixed in one tested place,
So that a balance can never go negative and a document can never change state illegally, whichever screen uses it.

**Acceptance Criteria:**

**Given** the Domain balance rule, written test-first
**When** a receipt of `x > 0` is applied to balance `b`
**Then** the new balance is `b + x`; a payout of `x` gives `b − x` only if `b ≥ x`, otherwise it throws `SoDuKhongDuException` (insufficient balance)
**And** amounts ≤ 0 throw, and the balance always equals total posted receipts − total posted payouts in a property-based test of 1,000 random sequences

**Given** the shared voucher state machine `TrangThaiChungTu` (1 Nháp/draft, 2 Đã ghi sổ/posted, 3 Đã huỷ/cancelled)
**When** a transition is requested
**Then** only Draft → Posted, Draft → Cancelled and Posted → Cancelled are allowed; every other transition throws
**And** cancelling requires a non-empty reason (max 300 characters) and records `LyDoHuy`, `NgayHuy` (`IClock.Now`) and `NguoiHuyId`
**And** the same state machine is reused by payouts, goods receipts and sale invoices

**Given** the snapshot value object `ThongTinDoiTuongSnapshot` (`HoTen`, `MaSo`, `LoaiDoiTuong`)
**When** a document is created
**Then** it copies the detainee's current values, and later changes to the detainee never alter the document

**Given** the Domain project
**When** these rules are implemented
**Then** they have no dependency on EF, UI or `DateTime.Now`

---

### Story 4.2: Concurrency-safe document numbering with year rollover

`HT-05, GAP-03` · Size M · Depends on: 1.2, 1.3 · FR7, FR8

As a custodial officer,
I want every document to get the next number for its type and year automatically,
So that numbers are never duplicated or skipped by mistake, even when several workstations save at the same moment.

**Acceptance Criteria:**

**Given** the `DemSoChungTu` table (PK `LoaiChungTu varchar(10)` + `Nam smallint`, `TienTo varchar(10)`, `SoHienTai int`) seeded for the current year with types `BNT` (receipt), `PC` (payout), `BKN` (remittance list), `PN` (goods receipt), `DK` (purchase registration), `PB` (sale invoice)
**When** `INumberingService.CapSoAsync(loai)` is called inside the caller's transaction
**Then** it runs `UPDATE … WITH (UPDLOCK) SET SoHienTai = SoHienTai + 1 OUTPUT inserted.SoHienTai` for the type and the year of the document date, and returns the formatted number
**And** the proposed format is `<TienTo>-<yyyy>-<5-digit sequence>`, e.g. `BNT-2026-00001` (fits `SoChungTu varchar(20)`; confirm with the PO)

**Given** the first document of a new year
**When** no `DemSoChungTu` row exists for that year
**Then** the service inserts it safely (no duplicate under concurrency, e.g. `MERGE … WITH (HOLDLOCK)` or a retry on a unique-key violation), copying `TienTo` from the previous year, and starts at 1

**Given** an integration test on LocalDB with 2 parallel tasks each allocating 100 `BNT` numbers
**When** both finish
**Then** there are exactly 200 distinct numbers with no duplicates

**Given** a simulated `IClock` at 31/12/2026 23:59 and then 01/01/2027
**When** a number is allocated at each time
**Then** they are `BNT-2026-n` and `BNT-2027-00001`

**Given** a caller's transaction that rolls back
**When** the number was already allocated
**Then** the counter rolls back too (the allocation shares the caller's transaction)

---

### Story 4.3: Complete ledger posting engine

`NEN-16, NEN-17, NEN-18, NEN-19` · Size L · Depends on: 4.1, 4.2, 3.1 · FR35

As a custodial officer,
I want every receipt and payout posted by one engine that checks the balance and the detainee's status in the same locked statement,
So that two workstations can never spend the same money and the balance always matches the ledger.

**Acceptance Criteria:**

**Given** `ChungTuLuuKy` completed to the spec: `LoaiPhieu` (1 thu/receipt, 2 chi/payout), `NghiepVu` with CHECK (`NghiepVu / 10 = LoaiPhieu`), receipt codes 11 mang theo khi vào trại, 12 người thân gửi, 13 phiếu gửi quà, 14 nhận từ đối tượng khác, payout codes 21 mua hàng, 22 cho tiền, 23 chuyển về người thân, 24 chuyển trại, 25 chấp hành xong án; `SoTien > 0`; `SoDuTruoc`, `SoDuSau`; `TrangThai`; cancel columns; `SoLanIn`; `CanBoLapId`; the index (`DoiTuongId`, `NgayChungTu`, `Id`) INCLUDE (`LoaiPhieu`, `SoTien`, `TrangThai`) and the filtered index on `NgayChungTu` WHERE `TrangThai = 2`
**When** `GhiSoLuuKyService.GhiSoAsync(chungTuId)` posts a draft
**Then** in one transaction it updates the balance with `UPDATE DoiTuong WITH (UPDLOCK, ROWLOCK) SET SoDuLuuKy = SoDuLuuKy ± @SoTien OUTPUT deleted.SoDuLuuKy, inserted.SoDuLuuKy WHERE Id = @Id AND TrangThai = 1 [AND SoDuLuuKy >= @SoTien for payouts]`, stores `SoDuTruoc` / `SoDuSau` and sets `TrangThai = 2`

**Given** the conditional UPDATE affects 0 rows
**When** the engine inspects the reason
**Then** it throws `DoiTuongKhongConQuanLyException` (detainee not active) if `TrangThai ≠ 1`, otherwise `SoDuKhongDuException` (insufficient balance), and the whole transaction rolls back

**Given** the `KyKhoaSo` table (`TuNgay`, `DenNgay`, CHECK `TuNgay <= DenNgay`, no overlapping periods, `DaKhoa bit`, `NguoiKhoaId`, `NgayKhoa`)
**When** a document dated inside a period with `DaKhoa = 1` is posted or cancelled
**Then** the engine throws `KyDaKhoaSoException` (period locked); the lock/unlock screen comes in Epic 6 (alignment note A5)

**Given** `GhiSoLuuKyService.HuyAsync(chungTuId, lyDo)` on a posted document
**When** it cancels
**Then** in one transaction it reverses the balance effect with the same conditional UPDATE (cancelling a receipt requires `SoDuLuuKy >= SoTien`, otherwise `SoDuKhongDuException` "cancelling would make the balance negative"), sets `TrangThai = 3` with the reason, and the interceptor logs `Huy`
**And** cancelling a draft only changes its state (no balance effect)

**Given** an integration test where detainee A has 500,000 đ and two parallel tasks each post a 450,000 đ payout
**When** both complete
**Then** exactly one succeeds, the other gets `SoDuKhongDuException`, and A's balance is 50,000 đ

**Given** the reconciliation routine `DoiChieuSoCaiService` (SQL: for each detainee, `SoDuLuuKy` vs Σ posted receipts − Σ posted payouts)
**When** it runs
**Then** it returns the list of detainees with a difference (empty when correct)
**And** a shared integration-test fixture calls it after every integration test and fails the test on any difference

**Given** the codebase
**When** an architecture test scans it
**Then** no type other than `GhiSoLuuKyService` writes `DoiTuong.SoDuLuuKy` or changes `ChungTuLuuKy.TrangThai` to posted or cancelled

---

### Story 4.4: Shared print frame and print counting

`NEN-20, NEN-21, NEN-22` · Size M · Depends on: 1.5, 2.8, 2.9, 4.3 · FR42, FR43 · UX-DR6

As a custodial officer,
I want every form to print with the same heading, signature block and reprint mark,
So that printed documents look official and a copy can never pass as the original.

**Acceptance Criteria:**

**Given** the Application abstraction `IReportRenderer` (report model → PDF bytes) and a QuestPDF base template in `Infrastructure/Reports`
**When** any template is rendered
**Then** the header shows `TenCoQuanChuQuan` (if set), `TenDonVi` and `DiaChi` from `ThongTinDonVi`; the footer holds the signature block built from `CauHinhKyTen` for that `MaMauIn` (titles left to right, default names if set); each concrete template implements only its body
**And** A4 and A5 page sizes are supported, using the embedded Vietnamese font from SP-01

**Given** a shared WebView2 preview window
**When** a user previews any document
**Then** they can zoom, print and save the PDF from that window, which every later template reuses

**Given** a posted document
**When** the user prints it or saves the PDF (preview alone does not count)
**Then** `SoLanIn` is incremented and one `In` audit row is written (not a `Sua` row)
**And** when `SoLanIn > 1` the page shows the watermark "BẢN IN LẠI (lần n)" (reprint, copy n)

**Given** a draft or cancelled document
**When** the user tries to print it
**Then** printing is refused (draft preview with a "NHÁP" watermark is TI-08 in R2)

**Given** the golden-file test harness
**When** a template is rendered with fixed data and a fixed `IClock`
**Then** the output is compared with an approved snapshot (text plus layout), and any change fails the test until the snapshot is re-approved against the paper form

---

### Story 4.5: Create and post a custodial receipt

`LK-T01, LK-T02, NEN-23 (part), NEN-24` · Size M · Depends on: 3.2, 4.3 · FR23, FR24, FR44 · UX-DR1, UX-DR4

As a custodial officer,
I want to record money that a detainee brings in or that a relative sends, and post it after checking the new balance,
So that the money is in the detainee's custodial account immediately and correctly.

**Acceptance Criteria:**

**Given** a user with `LK-T.Them` opens "Lập biên nhận thu" (new receipt)
**When** the form loads
**Then** it shows the fields of process A.I.4: document number (allocated on save), document date (default `IClock.Today`), detainee (shared picker, active only) with code and type shown, sender name, relationship, source document number (`SoPhieuGoc`), source type, receipt method, sender account, receipt date (`NgayNhan`), description, amount in figures, amount in words (generated live)

**Given** the source type list
**When** it is shown
**Then** manual entry offers 11 "Mang theo khi vào trại" (brought in on arrival) and 12 "Người thân gửi" (sent by a relative); 14 "Nhận từ đối tượng khác" (from another detainee) is created only by transfer approval (Story 5.5); 13 "Phiếu gửi quà" (gift slip) is visible but disabled until DEC-05 is decided (alignment note A4)
**And** for source 11 the sender defaults to the detainee and the relationship to "Bản thân" (self)

**Given** the shared money input control
**When** the user types an amount
**Then** it accepts digits only, shows thousand separators (500.000), rejects 0, negatives and decimals

**Given** the method "Chuyển khoản" (bank transfer, `HinhThuc = 2`)
**When** the account number is empty
**Then** the FluentValidation validator blocks saving (matching the DB CHECK `HinhThuc <> 2 OR SoTaiKhoanNguoiGui IS NOT NULL`); for cash (`HinhThuc = 1`) the account field is hidden

**Given** a valid form
**When** the user clicks "Lưu nháp" (save draft)
**Then** a draft `ChungTuLuuKy` is saved with an allocated `BNT` number, the detainee snapshot, `CanBoLapId` = the current user's staff record, and `SoTienBangChu`

**Given** a valid form or draft
**When** the user clicks "Ghi sổ" (post)
**Then** a confirmation dialog shows "Số dư trước → Số tiền → Số dư sau" (balance before → amount → balance after), e.g. 0 → +500.000 → 500.000, plus the amount in words; confirming calls `GhiSoLuuKyService` and the form shows the posted state with a Print button
**And** an engine error (inactive detainee, locked period) is shown as a clear business message and nothing is saved

---

### Story 4.6: Print the custodial receipt

`LK-T05` · Size S · Depends on: 4.4, 4.5 · FR27

As a custodial officer,
I want to print the official receipt for the sender and the detainee,
So that both have legal proof of the deposit.

**Acceptance Criteria:**

**Given** the template `BIEN_NHAN_THU` built on the shared frame (A5 by default, A4 selectable)
**When** a posted receipt is printed
**Then** it shows every field of process A.I.5: unit name and address, title "BIÊN NHẬN THU TIỀN GỬI LƯU KÝ", document number and date, sender name, relationship, recipient name, code, type (tạm giữ/tạm giam or phạm nhân), source document number and type, receipt method, sender account (if transfer), description, amount in figures (with thousand separators) and amount in words
**And** the signature block shows Người gửi · Người nhận · Lãnh đạo đơn vị

**Given** a detainee whose name or type changed after the receipt was posted
**When** the receipt is reprinted
**Then** it shows the snapshot values stored on the document, identical to the first print except for the reprint watermark

**Given** a golden-file test for this template
**When** it runs
**Then** it matches the snapshot approved against the paper receipt currently in use

---

### Story 4.7: Receipt list, edit draft and cancel a posted receipt

`LK-T06, NEN-23 (part)` · Size M · Depends on: 4.5, 4.6 · FR28 · UX-DR3

As a custodial officer,
I want to find receipts, correct drafts and cancel a wrong posted receipt with a reason,
So that mistakes are fixed openly and always leave a trace.

**Acceptance Criteria:**

**Given** the shared voucher grid control (DataGridView with filter bar)
**When** the user opens the receipt list
**Then** they can filter by date range, detainee, status and source type, see number, date, detainee, amount, status and print count, and right-click "Xuất Excel" (export to Excel) to export the visible rows

**Given** a draft receipt
**When** a user with `LK-T.Sua` edits and saves it
**Then** the changes are saved and audited; the number stays the same

**Given** a posted receipt
**When** a user tries to edit it
**Then** editing is not offered; the correction is cancel + new receipt (alignment: posted documents are immutable, matching the spec's "no hard delete, cancel with reason" rule)

**Given** a posted receipt and a user with `LK-T.Huy`
**When** they choose "Huỷ" (cancel)
**Then** a reason is mandatory; if `SoLanIn > 0` a warning says "Chứng từ đã được in n lần, hãy thu hồi bản in" (already printed n times, recover the printed copies) before confirming
**And** the service calls `GhiSoLuuKyService.HuyAsync`, which refuses the cancel when the period is locked or the balance would become negative, showing the reason

**Given** a cancelled receipt
**When** it is shown in the list or detail
**Then** it is marked cancelled with `LyDoHuy`, `NgayHuy` and the cancelling user, and it can no longer be printed or edited
