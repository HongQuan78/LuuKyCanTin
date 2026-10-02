---
stepsCompleted: [1, 2, 3, 4]
outputFormat: one-file-per-epic
inputDocuments:
  - _bmad-output/brainstorming/brainstorm-tien-gui-luu-ky-desktop-app-2026-09-29/backlog-draft.md
  - document/QUY TRÌNH TIẾP NHẬN TIỀN GỬI LƯU KÝ.docx
  - document/Tiếp nhận tiền gửi lưu ký — Danh sách chức năng.pdf
  - document/Tiếp nhận tiền gửi lưu ký — Tech Stack (WinForms .NET 10).pdf
  - document/Tiếp nhận tiền gửi lưu ký — Thiết kế Database.pdf
  - CLAUDE.md
---

# TienGuiLuuKy (LuuKyCanTin) - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for TienGuiLuuKy (LuuKyCanTin), decomposing the requirements from the PRD, UX Design if it exists, and Architecture requirements into implementable stories.

> **Source note:** there is no formal PRD, Architecture or UX document. Requirements are taken from the process doc (*Quy trình*), the 49-function feature list (*Danh sách chức năng*, the PRD stand-in), the Tech Stack and Database Design PDFs (the architecture stand-in), and the brainstorm backlog draft (release plan, extra safeguards, decisions). Each FR carries its source code (HT-01, NEN-16, GAP-01…) so it stays traceable to the backlog. Release tags: **S0** = Sprint 0, **R0.5** = custodial-only go-live, **R1** = canteen, **R2** = P2 + utilities.

## Requirements Inventory

### Functional Requirements

**HT — System**

- FR1 (HT-01, R0.5): Users log in, change password and log out. The account locks after 5 failed attempts. Passwords are hashed with salted PBKDF2, a strong-password policy applies, and a password change is forced at first login.
- FR2 (HT-02, R0.5): User accounts link to a staff member (`CanBo`; NULL for admin). Roles (RBAC, 6 seeded roles) grant permissions per function × action (view/add/edit/cancel/print/approve). The `Quyen` table is generated automatically with codes like `LK-C.Duyet`. Menus and buttons hide by permission, and services re-check permission before every write.
- FR3 (NEN-09, R0.5): Segregation of duties: the creator of a request or voucher cannot approve it. Every account-administration action is audit-logged.
- FR4 (NEN-10, R0.5): The session auto-locks when the workstation is left idle and unlocks quickly with the user's password.
- FR5 (HT-03, R0.5): The administrator maintains the unit information (parent agency name optional, unit name, address; a single row) printed at the head of every template.
- FR6 (HT-04, R0.5): Signatory configuration per print template (`CauHinhKyTen`): ordered signer titles, left to right, with an optional default staff name.
- FR7 (HT-05, R0.5): Document numbers are auto-generated per document type + year (BNT, PC, BKN, PN, DK, PB, with a printed prefix). Numbers never collide when several workstations create documents at the same time.
- FR8 (GAP-03, R0.5): Numbering rolls over to a new year automatically: a `DemSoChungTu` row is created on first use in a new year rather than only seeded for the install year.
- FR9 (HT-06, R0.5, raised from P2): The accountant locks and unlocks accounting periods. Documents dated inside a locked period cannot be added, edited or cancelled.
- FR10 (NEN-11, R0.5): Documents cannot be created or edited with a back-date into an earlier period unless the user holds a dedicated permission.
- FR11 (HT-07, R0.5): Audit-log screen: search who did what and when (add/edit/cancel/print/approve/login) with before/after values. The log is append-only.
- FR12 (HT-08, R0.5): Manual backup, a scheduled daily backup, and restore from a backup file.
- FR13 (NEN-12, R0.5): Periodic `RESTORE VERIFYONLY` of backups. The admin screen warns when the latest backup is older than 24h.

**DM — Master data**

- FR14 (DM-01, R0.5): Detainee master (`DoiTuong`): unique code, full name, birth year, type (temporary detention or convicted prisoner), entry date, cell/zone, assigned warden, status (managed / transferred / sentence completed; exit date required when status ≠ managed). Search by code and by name, ignoring diacritics.
- FR15 (DM-02, R0.5): Change detainee type (detention → prisoner) with history (`LichSuLoaiDoiTuong`, e.g. sentence number). Documents keep the type in force when they were created.
- FR16 (DM-03, R2): Relatives: name, relationship, address, bank account and bank. Suggest the relative and account when creating a receipt.
- FR17 (DM-04, R0.5): Staff (`CanBo`): code, name, position, warden flag, active flag. Roles: warden, canteen, commander, accountant, leadership.
- FR18 (DM-05, R1): Suppliers: code, name, address, phone, active flag.
- FR19 (DM-06, R1): Unit-of-measure master shared by goods.
- FR20 (DM-07, R1): Goods: code, name, UoM, category (daily-use / food), note, discontinued flag (history kept). Print the *Danh mục hàng hoá* (goods catalogue).
- FR21 (DM-08, R1): Sale price list with an effective-from date and immutable history. A new price takes effect from tomorrow. Print the *Bảng niêm yết giá* (posted price list).
- FR22 (DM-09, R0.5 detainees / R1 goods, raised from P2): Excel import of initial detainees and goods. Downloadable template, preview with per-row errors before writing, used for a one-month dry-run with real data.

**LK-T — Custodial receipts**

- FR23 (LK-T01, R0.5): Create a custodial receipt (*Biên nhận thu*) from 3 sources: brought in on arrival, sent by a relative, received from another detainee. Fields per process A.I.4: number, date, sender name, relationship, recipient detainee (name, code, type), source document number and type (custodial deposit slip / gift slip / give-receive minutes), receipt method, sender account, receipt date, description, amount in figures and words. Posts via `GhiSoLuuKyService`.
- FR24 (LK-T02, R0.5): Receipt method is cash or bank transfer. Transfer requires the sender's account number.
- FR25 (LK-T03, R0.5): Amount-in-words in Vietnamese, shared by every template. Edge cases: 0, 10, 15, 21, 101, 1,000,005, billions; "lẻ/linh", "mốt/một", "lăm/năm".
- FR26 (LK-T04, R2): Warn, without blocking, when a relative sends money more than once a month to the same detainee.
- FR27 (LK-T05, R0.5): Print the custodial receipt (fields per process A.I.5; signers: sender, recipient, unit leader). A reprint carries a "reprint" mark.
- FR28 (LK-T06, R0.5): Edit or cancel a receipt with a mandatory reason, only when its period is not locked and the change would not make the balance negative. Warn when cancelling a document that has already been printed.

**LK-C — Custodial payouts**

- FR29 (LK-C01, R0.5): Create a payout voucher (*Phiếu chi*). Types: canteen purchase, give money, send to relative, transfer to another facility, sentence completed. Fields per process A.II.5.
- FR30 (LK-C02, R0.5): A payout never exceeds the balance. The balance check-and-update runs under a row lock so two workstations cannot double-spend.
- FR31 (LK-C03, R0.5): Transfer to another detainee: request → Board of Wardens approval → automatically generates the giver's payout and the receiver's receipt with the same minutes number, in one transaction. The creator cannot approve their own request.
- FR32 (LK-C04, R0.5): One-click settlement on transfer or release: pay out the full balance, print the payout, change the detainee status, block new transactions. The file closes only when the balance = 0.
- FR33 (LK-C05, R1): Canteen-purchase payouts are generated automatically from a sale invoice and are never created by hand.
- FR34 (LK-C06, R0.5): Print the payout voucher: prior balance, amount paid out, remaining usable balance, 4 confirmation signatures (custodial officer, detainee, warden, unit leader).
- FR35 (NEN-18, R0.5): Block receipts, payouts and sales when the detainee status ≠ managed.

**LK-BC — Custodial reports and reconciliation**

- FR36 (LK-BC01, R0.5): Per-detainee ledger statement for a period: opening balance, each receipt and payout in the period, closing usable balance. Signers: canteen officer, warden, detainee, head of unit.
- FR37 (LK-BC02, R0.5): Unit-wide custodial ledger book: one row per detainee with opening, received, paid out and closing amounts. Signers: canteen officer, commander, accountant, head of unit.
- FR38 (LK-BC03, R0.5): Deposit remittance list (*Bảng kê nộp tiền*): auto-select posted receipts not yet remitted in the period, depositor name and address, total in figures and words. Each receipt can appear on only one list. Signers: depositor, commander, head of unit.
- FR39 (LK-BC04, R0.5): Balance lookup: current balance and transaction-history timeline for one detainee.
- FR40 (NEN-26, R0.5): Report of documents cancelled in a period, for leadership: reason, who cancelled, and whether it had been printed.
- FR41 (NEN-27, R0.5 / R1): One-click reconciliation for the accountant. Lists differences between `SoDuLuuKy` and the ledger (R0.5) and between `SoLuongTon` and `TheKho` (added in R1).

**Shared printing and posting UX (functional parts)**

- FR42 (NEN-20, R0.5): Every print template shares a frame: header from `ThongTinDonVi`, signature block from `CauHinhKyTen`, a reprint mark. Each template implements only its body. A4/A5 paper, preview before printing.
- FR43 (NEN-22, R0.5): Count prints (`SoLanIn`), audit-log each print, and watermark reprints with the print count.
- FR44 (NEN-24, R0.5): Before posting, a confirmation screen shows "balance before → amount → balance after".
- FR45 (NEN-25, R0.5): Shared report-period filter: month / quarter / year / from–to, with preview and print.
- FR46 (HH-BC04, R0.5): Export every report to PDF and Excel. Built early because LK-BC needs it.

**Go-live**

- FR47 (GAP-01, R0.5): Opening custodial balances at go-live are entered as an approved "balance carried forward" document posted through the ledger engine (pending DEC-09).
- FR48 (GAP-04, R0.5): First-run checklist that walks the admin through unit info, signatories, accounts and opening balances.

**NH — Goods receipt**

- FR49 (NH-01, R1): Goods-receipt note (*Phiếu nhập*) with many lines per supplier invoice or purchase list. The invoice number is mandatory. Fields per process B.I.2: invoice number and date, receipt date, supplier (code, name, address), lines (goods code and name, UoM, quantity, unit price, line amount).
- FR50 (NH-02, R1): Posting a receipt increases stock and recalculates the weighted-average cost.
- FR51 (NH-03, R1): Print the goods-receipt note. Signers: deliverer, receiver, team commander, unit leader.
- FR52 (NH-04, R1): Edit or cancel a goods receipt with a mandatory reason. Recalculate the cost of every issue after that date. Block it if stock would go negative at any later point.
- FR53 (GAP-02, R1): Opening stock at go-live via an approved initial goods-receipt note with cost.

**BH — Sales**

- FR54 (BH-01, R2): The warden creates a purchase registration for detainees they manage. The balance is checked at registration.
- FR55 (BH-02, R1): Create a sale invoice for 4 buyer types: detainee, relative, staff member, visiting unit. POS-style screen.
- FR56 (BH-03, R2): Convert registrations into sale invoices by batch or by cell, keeping the link to the registration.
- FR57 (BH-04, R1): Use the effective sale price (latest `BangGia` row ≤ sale date). Block sales above available stock.
- FR58 (BH-05, R1): A sale to a detainee debits the custodial balance in the same transaction via `GhiSoLuuKyService`. Block the sale if the balance is insufficient.
- FR59 (BH-06, R1): Each sale line stores the weighted-average unit cost at the time of issue and the line cost (rounded quantity × unit cost).
- FR60 (BH-07, R1): Print the purchase slip (*Phiếu mua hàng*). For a detainee or relative buyer it also shows prior balance, purchase amount and remaining balance, and the detainee's signature is mandatory. Signers: buyer, canteen officer, unit leader.
- FR61 (BH-08, R1): Cancelling a sale returns the stock and refunds the custodial balance in the same transaction.

**HH-BC — Goods reports**

- FR62 (HH-BC01, R1): Stock movement report (opening / in / out / closing, quantity and value). Issue price uses weighted-average cost, read from the last `TheKho` row in the period.
- FR63 (HH-BC02, R1): Revenue report by period and buyer type: invoice number, date, item, quantity, sale price, revenue, unit cost, cost, net profit per line.
- FR64 (HH-BC03, R1): Purchase history of one buyer in a period, with balances when the buyer is a detainee or relative.

**TI — Utilities and extended safeguards (R2)**

- FR65 (TI-01): "Counter" screen: enter a detainee code → balance summary plus quick receipt/payout/sale actions.
- FR66 (TI-02): Keyboard shortcuts (F2 receipt, F3 payout, F4 sale…).
- FR67 (TI-03): Global search (Ctrl+K) by document number, detainee name, goods code.
- FR68 (TI-04): Commander dashboard: requests awaiting approval, unposted draft documents, latest backup.
- FR69 (TI-05): Start-of-day screen (locked period, document counters, today's prices) and month-end reminders (lock the period, create the remittance list).
- FR70 (TI-06): Status bar: user, workstation, current period, DB connection, version, last backup.
- FR71 (TI-07): Training mode: sandbox database plus simulated date, isolated from live data.
- FR72 (TI-08): Print options: two A5 copies on A4, auto-print the receipt after posting, preview drafts with a "DRAFT" watermark.
- FR73 (TI-09): Auto-save a PDF of every printed document to a shared folder by year/month.
- FR74 (TI-10): Cancel-and-replace: the replacement document links to the cancelled one.
- FR75 (TI-11, P3): QR code of the document number on the purchase slip for quick lookup.
- FR76 (TI-12): Configurable monthly canteen spending limit per detainee, off by default.
- FR77 (TI-13): Stock count and adjustment notes (damaged, expired items) posted through `TheKho`.
- FR78 (TI-14): Goods alerts (low stock, discontinued); auto-print the posted price list when a new price is set.
- FR79 (TI-15, P3): Best-seller quick buttons on the POS screen.
- FR80 (TI-16): Bulk detainee type change from a sentence list.
- FR81 (TI-17): Transfer monitoring: frequency alerts and a report of repeated giver–receiver pairs.
- FR82 (TI-18): Canteen anomaly report: unusual detainee purchases, price-change history.
- FR83 (TI-19): Goods-receipt control: attach a scanned invoice, commander approval.
- FR84 (TI-20): Hash chain over `ChungTuLuuKy` to detect edits made outside the app, integrated into reconciliation.
- FR85 (TI-21): Export a support bundle (zipped logs) for the admin, no Internet needed.
- FR86 (TI-22, P3): Per-form F1 help.
- FR87 (TI-23, P3): Monthly revenue chart.
- FR88 (TI-24, P3): Large font / zoom on the counter screen; portrait photo in the detainee picker.

### NonFunctional Requirements

- NFR1 (Accuracy): Money is integer đồng (`decimal(18,0)`, never float). Quantity allows up to 3 decimals (`decimal(18,3)`). Weighted-average unit cost uses `decimal(18,4)` with explicit rounding to đồng for amounts, and repeated accumulation must not drift (tested over thousands of postings).
- NFR2 (Accuracy): A custodial balance always equals total posted receipts − total posted payouts and is never negative (DB CHECK plus a conditional UPDATE).
- NFR3 (Integrity): Every posting, cancellation and sale runs in one transaction. Balance and stock checks happen in the same statement as the update, under a row lock (`UPDLOCK, ROWLOCK`), never as a read-then-write.
- NFR4 (Concurrency): Several LAN workstations work at once. No duplicate document numbers, no double-spend, no overselling. Optimistic concurrency via `rowversion` on editable records.
- NFR5 (Traceability): Documents are never hard-deleted. Cancellation needs a reason, date and user. The audit log is append-only (the app's SQL login is DENIED UPDATE/DELETE on `NhatKyThaoTac`) and records before/after JSON, the user and the workstation.
- NFR6 (Security): Salted PBKDF2 password hashing; per-action authorization re-checked in services; session auto-lock; least-privilege SQL login; connection string encrypted with DPAPI; `Encrypt=True` with the server's certificate.
- NFR7 (Operations): Runs fully offline on the LAN with no Internet. Daily automatic backup (Windows Task Scheduler + sqlcmd, since Express has no Agent), copies kept on a separate drive.
- NFR8 (Printing): A4/A5, Unicode Vietnamese fonts, preview before printing, layout faithful to the paper forms in use. Snapshotted data means a reprint matches the original.
- NFR9 (Platform): Workstations run Windows 10/11 64-bit, self-contained .NET 10 (no runtime install), WebView2 Runtime bundled offline. The server runs SQL Server 2022 Express (10 GB/DB limit; upgrading to Standard needs no code change) on a static IP.
- NFR10 (Performance/usability): Incremental, diacritic-insensitive search stays responsive over thousands of detainees (`Vietnamese_CI_AI` collation plus indexes).
- NFR11 (Maintainability): Domain and Application are unit-testable without a UI or DB. Integration tests run on SQL Server LocalDB, and ledger reconciliation is asserted after every integration test.
- NFR12 (Logging): Serilog daily file logs in `%ProgramData%\LuuKyCanTin\logs`, kept 30 days. A global handler catches every unhandled exception and shows a friendly message.
- NFR13 (Deployment): The installer and one-touch updates run from a LAN shared folder (Velopack). EF migrations run once from the admin machine, and a workstation refuses to start when the DB version doesn't match.
- NFR14 (Licensing): Only free or permissively licensed libraries. QuestPDF Community licence terms must be confirmed. No FluentAssertions.

### Additional Requirements

**Starter / solution setup (affects Epic 1 Story 1)**

- No external starter template. The greenfield solution `LuuKyCanTin.slnx` has 4 source projects (`src/Libraries/LuuKyCanTin.Domain`, `LuuKyCanTin.Application`, `LuuKyCanTin.Infrastructure`, `src/Presentation/LuuKyCanTin.WinForms`) and 3 test projects (Domain, Application, Integration), with folders per business module (`LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/`, `Common/`, `Abstractions/`). CI builds and runs the tests (NEN-01).
- Clean Architecture with inward-only references. Domain has no packages. Application has no EF-provider, QuestPDF, ClosedXML or WinForms dependencies. WinForms references Infrastructure only for DI composition.
- Generic Host in `Program.cs` with DI, configuration (`appsettings.json`), Serilog and a global exception handler.
- WinForms MVP: passive Views behind interfaces; Presenters are tested with NSubstitute (NEN-07). Services never use `MessageBox`.
- Forms never hold a `DbContext`. Each operation creates a new DI scope.
- Abstractions in Application: `IAppDbContext`, `IReportRenderer`, `ICurrentUser`, `IClock`, `INumberingService`.
- `IClock` for all date logic; dates can be simulated for tests and training (NEN-06).
- Tests use xUnit + NSubstitute + Shouldly, plus LocalDB for integration tests.

**Persistence and data**

- EF Core 10 code-first migrations. Seed data lives in migrations so a fresh install and an upgrade end up identical (NEN-02). Seed: 6 roles plus permissions per function code; one `admin` user who must change their password; `CauHinhKyTen` for the 12 templates; `DemSoChungTu` for BNT, PC, BKN, PN, DK, PB; UoMs (cái, gói, hộp, chai, kg, thùng); an empty `ThongTinDonVi` row. Separate dev/demo seed with 200 detainees and 50 goods.
- The workstation checks the DB schema version at startup and refuses to run on a mismatch (NEN-03).
- An automated test enforces that every Domain enum matches its DB CHECK constraint (NEN-04).
- A `SaveChanges` interceptor writes `NhatKyThaoTac` (before/after JSON) automatically for every voucher table (NEN-05).
- DB conventions: 29 tables in 4 groups; Vietnamese names without diacritics in PascalCase; `int IDENTITY` PKs (`bigint` for `ChungTuLuuKy`, `TheKho` and detail tables); `date` for document dates and `datetime2(0)` for timestamps; `nvarchar` with `Vietnamese_CI_AI`; enums as `tinyint` + CHECK; common audit columns (`NgayTao`, `NguoiTaoId`, `NgaySua`, `NguoiSuaId`, `RowVer`); soft cancel (`TrangThai`=Cancelled + `LyDoHuy`, `NgayHuy`, `NguoiHuyId`); snapshot columns (names, type, buyer).
- Key enums: `LoaiPhieu` 1 receipt / 2 payout; `NghiepVu` 11 brought in, 12 relative, 13 gift slip, 14 from another detainee, 21 purchase, 22 give money, 23 send to relative, 24 transfer, 25 sentence completed, with CHECK (`NghiepVu / 10 = LoaiPhieu`); `HinhThuc` 1 cash / 2 transfer; `TrangThai` 1 draft / 2 posted / 3 cancelled; `LoaiDoiTuong` 1 detention / 2 prisoner; detainee `TrangThai` 1 managed / 2 transferred / 3 completed; `LoaiNguoiMua` 1–4; `HinhThucThanhToan` 1 custodial debit / 2 cash; `LoaiBienDong` 1 in / 2 out / 3 cancelled receipt / 4 cancelled issue; `DeNghiChoTien.TrangThai` 1 pending / 2 approved / 3 rejected; `DangKyMuaHang.TrangThai` 1 new / 2 converted / 3 cancelled.
- Constraints: `ChungTuLuuKy.PhieuBanHangId` filtered UQ (one sale → exactly one purchase payout); `BangKeNopChiTiet.ChungTuLuuKyId` UQ; `DeNghiChoTien` CHECK giver ≠ receiver; `BangGia` UQ (HangHoaId, ApDungTuNgay); `KyKhoaSo` with no overlapping periods; in `TheKho` exactly one of the receipt-line or sale-line FKs is set.
- Indexes: `ChungTuLuuKy (DoiTuongId, NgayChungTu, Id) INCLUDE (LoaiPhieu, SoTien, TrangThai)`; `ChungTuLuuKy (NgayChungTu) WHERE TrangThai=2`; `TheKho (HangHoaId, NgayChungTu, Id)`; `PhieuBanHang (NgayXuat, LoaiNguoiMua)` and `(DoiTuongId, NgayXuat)`; `DoiTuong (HoTen)`; `HangHoa (TenHang)`.
- Complex reports are written as SQL (EF `SqlQuery` or Dapper), not complex LINQ.

**Ledger engine and domain rules**

- Ledger-first: `GhiSoLuuKyService` is the only writer of `ChungTuLuuKy`/`SoDuLuuKy`, using `UPDATE DoiTuong WITH (UPDLOCK, ROWLOCK) … OUTPUT deleted/inserted … WHERE Id=@Id AND TrangThai=1 AND SoDuLuuKy >= @SoTien`. Receipts, payouts, transfers, settlement, sales and opening balances all call it (NEN-16).
- Balance rule in Domain, built test-first (NEN-13). Shared document state machine Draft → Posted → Cancelled (with reason) for receipts, payouts, goods receipts and sales (NEN-14). Snapshot value object for names and types (NEN-15).
- Concurrency tests: two parallel payouts → exactly one succeeds (NEN-17); two parallel numbering calls → no duplicates (HT-05); two workstations overselling → one blocked (NEN-30).
- A ledger reconciliation routine compares `SoDuLuuKy` with the `ChungTuLuuKy` totals and runs as the assertion after every integration test (NEN-19).
- `TheKho` stores running totals after each line (quantity, value, average cost after), so stock reports read only the last row in the period (NEN-28). An idempotent stock-card recalculation engine runs from date X and is tested against a full recalculation (NEN-29). Back-dated documents and cancelled goods receipts trigger the recalculation.
- Conditional UPDATE + row lock for stock deduction on `HangHoa` (NEN-30).

**Printing, Excel, infrastructure**

- QuestPDF spike: licence, Vietnamese Unicode font, preview/print via WebView2, A4/A5 (SP-01). One class per template in `Infrastructure/Reports`. Golden-file snapshot tests of rendered PDFs (NEN-21).
- 12 print templates: Receipt, Payout, Per-detainee statement, Unit ledger book, Remittance list, Goods receipt note, Purchase slip, Stock movement, Revenue, Purchase history, Posted price list, Goods catalogue.
- ClosedXML for Excel import/export (`Infrastructure/Excel`).
- Velopack spike and full rollout via the LAN share (SP-02, GAP-06).
- Vietnamese search spike: `Vietnamese_CI_AI`, indexes, incremental search over thousands of rows (SP-03).
- Walking skeleton: login → create detainee → create receipt → post → print PDF, through all 4 layers (NEN-08).
- Operations infrastructure: SQL Express install, static IP, firewall, certificate for `Encrypt=True`, a least-privilege SQL login with DENY on direct edits and deletes of voucher and audit tables (GAP-05).
- Backup via Task Scheduler + sqlcmd, plus in-app manual backup and restore (`Infrastructure/Backup`).

**Process, release and decision constraints**

- Release gates. Sprint 0: walking skeleton through 4 layers, CI green, DEC-02/05/09 decided. R0.5: one month in parallel with the paper ledger, month-end balances 100% match (GAP-07). R1: one-click reconciliation shows zero difference in both balances and stock, DEC-01/03/04/06 decided. R2: per story, DEC-07/08 decided.
- Open decisions (current assumptions):
  - DEC-01 Weighted-average cost is moving (recalculated after each receipt).
  - DEC-02 "Send to relative" is its own payout type.
  - DEC-03 A relative's canteen purchase debits the detainee's custodial balance.
  - DEC-04 Staff and visiting units pay cash only.
  - DEC-05 Gift-slip money handling is undecided.
  - DEC-06 No monthly limit (flag designed in, off by default).
  - DEC-07 Stock count/adjustment deferred to R2.
  - DEC-08 How the warden's purchase registration is entered is undecided.
  - DEC-09 Opening balances and stock use a "carried forward" document / initial goods receipt approved by leadership.
- Definition of Done:
  - Domain unit tests.
  - LocalDB transaction integration tests with reconciliation passing.
  - Print templates checked against the paper forms, golden files updated.
  - Audit log correct, including prints.
  - Permissions enforced in services.
  - Presenter tests through View interfaces.
  - Each story states its function code, role, rules, AC, template and tables.

### UX Design Requirements

> No UX design contract exists. The UI requirements below come from the backlog and the spec and are specific enough to drive stories.

- UX-DR1 (NEN-23): Shared money input control. Integer đồng only, thousand separators while typing, no decimals or negatives.
- UX-DR2 (NEN-23): Shared detainee picker with incremental, diacritic-insensitive search by code or name. Rows show code, cell and birth year to tell namesakes apart, with a red label when the detainee is settled or inactive.
- UX-DR3 (NEN-23): Shared voucher grid (DataGridView) with a right-click "Export to Excel".
- UX-DR4 (NEN-24): Pre-posting confirmation dialog showing "balance before → amount → balance after" (and the amount in words) before any posting that changes a balance.
- UX-DR5 (NEN-25): Shared report-period filter (month / quarter / year / from–to) with Preview, Print and Export buttons, reused by every report.
- UX-DR6: Integrated WebView2 PDF preview and print window for all 12 templates. Reprints show the "BẢN IN LẠI" watermark with the print count.
- UX-DR7 (HT-02): Main shell menu and toolbar built from the user's permissions. Hidden or disabled items are cosmetic only, and services still re-check.
- UX-DR8 (BH-02): POS sale screen. Search goods by code or name, Enter adds a line, a large total display, USB barcode-scanner input (keyboard wedge). The buyer-type selector switches between the detainee picker and a free-text buyer.
- UX-DR9 (DM-09): Excel import wizard: download template → choose file → preview grid with per-row error messages → commit only valid files.
- UX-DR10 (NFR12): Friendly error message for unhandled exceptions (no stack trace to the user; details go to the log), and a clear business-rule message when a validation or posting fails (e.g. insufficient balance, period locked).
- UX-DR11 (NEN-10): Session-lock screen that unlocks with the current user's password without losing open work.
- UX-DR12 (TI-24, R2): Zoom/large-font mode on the counter screen and a portrait photo in the picker.

### Backlog Alignment Notes

These are the changes made to the backlog draft so it follows story best practice and matches the spec documents. Each epic file applies them.

| # | Backlog item | Issue | Resolution |
|---|---|---|---|
| A1 | E0, E3, E4 (technical epics) | Epics organized by technical layer deliver no user value | Folded into value epics. The ledger engine ships with its first caller (receipts, Epic 4), the print frame with the first printed document, the UI kit with the first form that uses it |
| A2 | NEN-04, NEN-06, NEN-07, NEN-21 | Engineering conventions, not stories | Become acceptance criteria of the foundation stories and part of the Definition of Done |
| A3 | GAP-05 "DENY edit/delete on voucher tables" | Conflicts with the spec: the app must UPDATE vouchers (status, cancel) and balances | Aligned with the Tech Stack doc: DENY UPDATE, DELETE on `NhatKyThaoTac`; DENY DELETE on voucher tables; no DENY UPDATE on vouchers |
| A4 | LK-T01 "3 sources" | The DB defines a 4th `NghiepVu` (13 gift slip) | Code 13 exists in the enum/CHECK, but the UI option stays disabled until DEC-05 is decided |
| A5 | LK-T06 depends on HT-06 (lock screen) | Would make R0.5 stories depend on a later screen | The period-lock **rule** is enforced in the posting engine from Epic 4; the accountant's lock/unlock **screen** comes in Epic 6 |
| A6 | LK-BC03 "auto-filter" vs spec "select receipts" | Wording differs | Proposes all unremitted posted receipts in the period; the user may deselect; UQ guarantees one list per receipt |
| A7 | HT-02 depends on DM-04 | Staff master is needed before accounts | DM-04 moves into the access epic (Epic 2) |
| A8 | DM-09, NEN-27 | Each spans two releases | Split: detainee import (Epic 3) / goods import (Epic 8); balance reconciliation (Epic 6) / stock reconciliation (Epic 11) |
| A9 | NEN-08 walking skeleton | Crosses features built later | Kept as the last story of Epic 1, built on a thin real ledger engine (receipt only). Epic 4 completes the engine. Nothing bypasses the engine, even in the skeleton |
| A10 | Sprint 0 spikes SP-01..03 | Time-boxed research | Kept as spike stories in Epic 1, each with a written outcome as the AC |
| A11 | LK-C01 lists all 5 payout types for manual entry | Types 21/22/24/25 have their own source flows in the spec (sale, transfer approval, settlement); in R0.5 the canteen still runs on paper | Manual payout form: type 23 always; type 21 only while the canteen module is off (disabled by Story 10.3). Type 22 comes only from transfer approval (5.5); types 24/25 only from settlement (5.6) |
| A12 | GAP-01 opening balance | The DB design has no `NghiepVu` for "balance carried forward" | Add `NghiepVu` 15 "Số dư chuyển sang" (receipt) to the enum and CHECK, a DB design change (Story 7.5) |
| A13 | Cancelling a payout | Not stated explicitly in the spec | Follows the shared state machine: cancel with a reason, reversed through the engine (Story 5.3). Generated payouts are cancelled only through their source document |
| A14 | Stock adjustment (TI-13) | `LoaiBienDong` only has codes 1–4 | Add adjustment codes (e.g. 5 increase, 6 decrease) when DEC-07 is decided (Story 14.7) |
| A15 | DM-04 "staff role" | `CanBo` has only `ChucVu` + `LaQuanGiao` | Business role kept in `ChucVu`, wardens flagged by `LaQuanGiao`; system access comes from account roles (2.1); no new column |
| A16 | LK-T06 "edit receipt" | Editing a posted document conflicts with "never hard-delete, cancel with reason" | Only drafts are edited; a posted document is corrected by cancel + new document. Numbers are allocated at draft save (`SoChungTu` NOT NULL UQ), so gaps are visible and audited (4.2, 4.7) |
| A17 | `DeNghiChoTien.NguoiDuyetId` → `CanBo` | The approver is a logged-in user | The approving account must be linked to a staff member; an admin account without `CanBoId` cannot approve (5.5) |
| A18 | Permissions | The spec gives only module × action | Standard `<Module>.<Action>` codes plus a registry of special permissions (`HT.KhoaSo`, `LK.LuiNgay`, `HT.SaoLuu`, `HT.PhucHoi`, `HT.CauHinh`, `HT.QuanTri`), each seeded by the story that needs it (2.3) |
| A19 | `PhieuNhapChiTiet.DonGia decimal(18,2)` | Exception to "money is integer đồng" | Kept as in the DB design (supplier unit prices can have decimals); `ThanhTien` rounded to đồng (9.2) |
| A20 | DB design additions | Columns/tables not in the 29-table design | Backup status table (7.2); `CanTinDaKichHoat` setting (5.1); `NghiepVu` 15 (7.5); `PhieuNhap` opening-stock flag, approval, supplier snapshot, `SoLanIn` (9.2, 9.6); `ChungTuThayTheId` (14.4); `TheKho` 3rd FK + `LoaiBienDong` 5/6 + `DCK` numbering (14.7); goods-receipt "awaiting approval" status (14.12); photo path on `DoiTuong` (13.9); `ChuoiHash` (14.13). The DB design document should be updated as each lands |
| A21 | GAP-05 DENY script | R1 voucher tables are created after Story 7.3 | Each R1 migration that adds a voucher table extends the DENY DELETE script (9.2, 10.2) |
| A22 | HT-07 audit log for leadership vs the role table | The feature-list role table gives leadership no `HT` permission, but FR11/HT-07 makes the audit log a leadership tool (Story 2.10's user is a unit leader) | The *Chỉ huy phụ trách / Lãnh đạo đơn vị* role is also granted `HT.Xem` (2.3). Writes stay re-checked by services, so read-only access to the HT screens is harmless |

### Open Questions for the PO

These are proposals the stories assume; confirm them before the story's sprint.

1. Document number format `BNT-2026-00001` (4.2).
2. Back-date window default of 3 days (6.9).
3. Cancelling an approved transfer cancels both generated documents together through the request (5.5).
4. Cost rounding: stock value is the source of truth, `MidpointRounding.AwayFromZero`, and the last issue takes the remaining value when stock reaches 0 (9.1).
5. A stock recalculation that would change cost inside a locked period is refused (9.4).
6. Portrait photos are stored as files on the LAN share, not in the DB (13.9).
7. The hash chain serializes postings on one lock row, which affects throughput (14.13).
8. The open decisions DEC-01..09 (see Additional Requirements).

### FR Coverage Map

| FR | Epic | Summary |
|---|---|---|
| FR1 | 2 | Login, password policy, lockout |
| FR2 | 2 | Users, roles, permissions |
| FR3 | 2 | Segregation of duties |
| FR4 | 2 | Session auto-lock |
| FR5 | 2 | Unit information |
| FR6 | 2 | Signatory configuration |
| FR7 | 4 | Document numbering |
| FR8 | 4 | Year rollover of numbering |
| FR9 | 6 | Period lock/unlock |
| FR10 | 6 | Back-date guard |
| FR11 | 2 | Audit-log screen |
| FR12 | 7 | Backup and restore |
| FR13 | 7 | Backup verification and age warning |
| FR14 | 3 | Detainee master + search |
| FR15 | 3 | Detainee type change + history |
| FR16 | 12 | Relatives |
| FR17 | 2 | Staff master |
| FR18 | 8 | Suppliers |
| FR19 | 8 | Units of measure |
| FR20 | 8 | Goods + catalogue print |
| FR21 | 8 | Price list + posted price print |
| FR22 | 3, 8 | Excel import (detainees / goods) |
| FR23 | 4 | Create receipt |
| FR24 | 4 | Cash / transfer method |
| FR25 | 1 | Amount in words |
| FR26 | 12 | Monthly repeat-deposit warning |
| FR27 | 4 | Print receipt |
| FR28 | 4 | Edit / cancel receipt |
| FR29 | 5 | Create payout |
| FR30 | 5 | No overspend, row lock |
| FR31 | 5 | Detainee-to-detainee transfer with approval |
| FR32 | 5 | One-click settlement |
| FR33 | 10 | Auto purchase payout from sale |
| FR34 | 5 | Print payout |
| FR35 | 4 | Block inactive detainees (engine rule) |
| FR36 | 6 | Per-detainee statement |
| FR37 | 6 | Unit ledger book |
| FR38 | 6 | Deposit remittance list |
| FR39 | 6 | Balance lookup + timeline |
| FR40 | 6 | Cancelled-documents report |
| FR41 | 6, 11 | One-click reconciliation (balance / stock) |
| FR42 | 4 | Shared print frame |
| FR43 | 4 | Print counting + reprint watermark |
| FR44 | 4 | Pre-posting confirmation |
| FR45 | 6 | Shared period filter |
| FR46 | 6 | PDF / Excel export of reports |
| FR47 | 7 | Opening balances |
| FR48 | 7 | First-run checklist |
| FR49 | 9 | Goods receipt note |
| FR50 | 9 | Stock + moving average cost |
| FR51 | 9 | Print goods receipt note |
| FR52 | 9 | Edit / cancel goods receipt + recalculation |
| FR53 | 9 | Opening stock |
| FR54 | 12 | Warden purchase registration |
| FR55 | 10 | POS sale invoice |
| FR56 | 12 | Registration → sale conversion |
| FR57 | 10 | Effective price + stock block |
| FR58 | 10 | Custodial debit on sale |
| FR59 | 10 | Cost of goods per line |
| FR60 | 10 | Print purchase slip |
| FR61 | 10 | Cancel sale (restock + refund) |
| FR62 | 11 | Stock movement report |
| FR63 | 11 | Revenue report |
| FR64 | 11 | Purchase history report |
| FR65 | 13 | Counter screen |
| FR66 | 13 | Keyboard shortcuts |
| FR67 | 13 | Global search Ctrl+K |
| FR68 | 14 | Commander dashboard |
| FR69 | 13 | Start-of-day + month-end reminders |
| FR70 | 13 | Status bar |
| FR71 | 14 | Training mode |
| FR72 | 13 | Print options |
| FR73 | 14 | Auto-archive printed PDFs |
| FR74 | 14 | Cancel-and-replace |
| FR75 | 14 | QR on purchase slip |
| FR76 | 14 | Monthly spending limit |
| FR77 | 14 | Stock count + adjustment |
| FR78 | 14 | Goods alerts |
| FR79 | 13 | POS best-seller buttons |
| FR80 | 14 | Bulk type change |
| FR81 | 14 | Transfer monitoring |
| FR82 | 14 | Canteen anomaly report |
| FR83 | 14 | Goods-receipt control |
| FR84 | 14 | Hash chain on ledger |
| FR85 | 14 | Support bundle export |
| FR86 | 13 | F1 help |
| FR87 | 14 | Revenue chart |
| FR88 | 13 | Zoom + portrait in picker |

## Epic List

Each epic has its own file in `epics/`. Release order: Epic 1 = Sprint 0; Epics 2–7 = **R0.5** (custodial go-live); Epics 8–11 = **R1** (canteen); Epics 12–14 = **R2**.

| # | Epic | Release | User outcome | FRs | File |
|---|---|---|---|---|---|
| 1 | Foundation & first printed receipt | S0 | The team proves the whole stack: an admin logs in, records one receipt for one detainee, and prints it as a PDF | FR25 (+ technical setup) | `epics/epic-01-foundation-walking-skeleton.md` |
| 2 | Secure access & unit setup | R0.5 | The admin sets up the unit, staff, signatories, accounts and roles; users sign in securely and every admin action is audited | FR1–6, FR11, FR17 | `epics/epic-02-secure-access-unit-setup.md` |
| 3 | Detainee register | R0.5 | Staff keep an accurate detainee register, find anyone fast without diacritics, and bulk-load the initial list from Excel | FR14, FR15, FR22 (detainees) | `epics/epic-03-detainee-register.md` |
| 4 | Receive custodial money | R0.5 | The custodial officer records, prints, edits and cancels receipts through one correct ledger engine with no duplicate numbers | FR7, FR8, FR23, FR24, FR27, FR28, FR35, FR42–44 | `epics/epic-04-receive-custodial-money.md` |
| 5 | Pay out custodial money | R0.5 | The officer pays out, transfers between detainees with Board approval, and settles accounts on transfer or release, never overspending | FR29–32, FR34 | `epics/epic-05-pay-out-custodial-money.md` |
| 6 | Custodial reports, reconciliation & period close | R0.5 | The accountant and leadership print every custodial report, reconcile in one click, and lock the month | FR9, FR10, FR36–41 (balance), FR45, FR46 | `epics/epic-06-custodial-reports-period-close.md` |
| 7 | Go-live & safe operations | R0.5 | The unit installs, updates and backs up the system on the LAN, enters opening balances, and runs one month in parallel with paper | FR12, FR13, FR47, FR48 | `epics/epic-07-go-live-operations.md` |
| 8 | Canteen catalogue & pricing | R1 | Canteen staff maintain suppliers, goods and immutable dated prices, and print the catalogue and posted price list | FR18–21, FR22 (goods) | `epics/epic-08-canteen-catalogue-pricing.md` |
| 9 | Goods receiving & stock costing | R1 | Canteen staff receive goods from suppliers with an exact running stock card and moving-average cost, including opening stock | FR49–53 | `epics/epic-09-goods-receiving-stock-costing.md` |
| 10 | Canteen sales against custodial balance | R1 | The cashier sells to the 4 buyer types on a POS screen; a detainee's purchase debits their balance, records cost and prints the slip in one transaction | FR33, FR55, FR57–61 | `epics/epic-10-canteen-sales.md` |
| 11 | Canteen reports & stock reconciliation | R1 | Leadership sees stock movement, revenue/profit and purchase history that match the stock card and ledger | FR41 (stock), FR62–64 | `epics/epic-11-canteen-reports.md` |
| 12 | Warden purchase registration & relatives | R2 | Wardens register purchases for their detainees, canteen converts them in batches, and relatives are suggested on receipts | FR16, FR26, FR54, FR56 | `epics/epic-12-registration-relatives.md` |
| 13 | Faster counter work | R2 | Daily users work faster: counter screen, shortcuts, global search, status bar, print options, POS quick buttons, help, zoom | FR65–67, FR69, FR70, FR72, FR79, FR86, FR88 | `epics/epic-13-faster-counter-work.md` |
| 14 | Oversight & extended controls | R2 | Leadership gets dashboards, anomaly and transfer monitoring, tamper detection and stricter controls; admins get training mode and support tools | FR68, FR71, FR73–78, FR80–85, FR87 | `epics/epic-14-oversight-controls.md` |

**Dependency flow:** each epic uses only earlier epics. 1 → 2 → 3 → 4 → 5 → 6 → 7 completes R0.5. Epics 8 → 9 → 10 → 11 need only Epics 1–7. R2 epics build on everything before them, and none is required by an earlier epic.
