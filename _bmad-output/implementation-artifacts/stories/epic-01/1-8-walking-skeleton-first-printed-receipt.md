---
story: "1.8"
epic: 1
title: "Walking skeleton: sign in, register a detainee, post and print one receipt"
status: done
size: L
backlogItems: [NEN-08]
frsCovered: []
frsTouchedThin: [FR1, FR7, FR14, FR23, FR27]
nfrsTouched: [NFR1, NFR2, NFR3, NFR5, NFR6, NFR8, NFR11]
dependsOn: ["1.2", "1.3", "1.4", "1.5"]
baseline_commit: 404f0b5a6168ee6163dd894f2870a089d53a6361
---

# Story 1.8: Walking skeleton: sign in, register a detainee, post and print one receipt

Status: done

## Story

As an administrator,
I want to sign in, add one detainee, record a 500,000 đ receipt and print it,
So that the team sees one real business flow working through every layer before building out features.

## Acceptance Criteria

1. **Given** the seeded `admin` account (password hashed with salted PBKDF2 via `Rfc2898DeriveBytes.Pbkdf2`) and the tables `NguoiDung`, `ThongTinDonVi` (single row, `CHECK (Id = 1)`)
   **When** the admin enters the correct password on the login form
   **Then** the main shell opens and `ICurrentUser` is populated; a wrong password shows an error (full login rules come in Epic 2)

2. **Given** the `DoiTuong` table with its full spec columns (`MaSo` UQ, `HoTen`, `NamSinh`, `LoaiDoiTuong`, `NgayVao`, `BuongGiam`, `TrangThai`, `NgayRa`, `SoDuLuuKy decimal(18,0) DEFAULT 0 CHECK (>= 0)`)
   **When** the admin adds the detainee "Nguyễn Văn A" through a minimal form
   **Then** the detainee is saved with balance 0

3. **Given** the `ChungTuLuuKy` and `DemSoChungTu` tables and a thin `GhiSoLuuKyService` that supports receipts only
   **When** the admin records a cash receipt of 500,000 đ from a relative and posts it
   **Then** in one transaction the document number is allocated (`BNT`, current year), the `ChungTuLuuKy` row is inserted with posted status, the amount in words and a snapshot of the detainee name and type, and `DoiTuong.SoDuLuuKy` is increased by a conditional `UPDATE … WITH (UPDLOCK, ROWLOCK) OUTPUT deleted/inserted`
   **And** the posting writes `SoDuTruoc = 0` and `SoDuSau = 500000` on the document and an audit row through the interceptor
   **And** no code outside `GhiSoLuuKyService` writes `SoDuLuuKy`

4. **Given** the posted receipt
   **When** the admin clicks Print
   **Then** a PDF preview opens in WebView2 with the unit name and address from `ThongTinDonVi`, the document number, "Nguyễn Văn A", "500.000" and "Năm trăm nghìn đồng"

5. **Given** the integration test of this flow on LocalDB
   **When** it finishes
   **Then** a reconciliation assertion checks `SoDuLuuKy` = total posted receipts − total posted payouts (the first version of NEN-19)

## Tasks / Subtasks

- [x] **T0. Resolve the Application ↔ EF Core decision** (prerequisite, see Story 1.1 Dev Notes)
  - [x] **Resolved as "no"** at implementation time (no PO available; Stories 1.2/1.3 already settled Application on EF-free ports and the architecture test forbade EF in Application). Implemented the fallback: `IAppDbContext` exposed only `SaveChangesAsync` + `BeginTransactionAsync` (returning the Application-level `IAppTransaction`); entity access uses narrow ports (`IDoiTuongStore`, `IChungTuLuuKyStore`, `INguoiDungStore`, `IThongTinDonViStore`). **Superseded by the master merge (Epic 2.1): the team's answer is "yes"** — Application references `Microsoft.EntityFrameworkCore` core (no provider), `IAppDbContext` exposes `DbSet`/`Entry`, and the narrow stores plus `BeginTransactionAsync` stay on top of it.
- [x] **T1. Domain: enums and entities** (AC: 2, 3)
  - [x] Domain `DanhMuc/`: `LoaiDoiTuong : byte { TamGiuTamGiam = 1, PhamNhan = 2 }`, `TrangThaiDoiTuong : byte { DangQuanLy = 1, DaChuyenTrai = 2, DaChapHanhXongAn = 3 }`, and the entity `DoiTuong : AuditableEntity`. `SoDuLuuKy` has a **private setter** with no public mutator. Only the ledger engine changes it, through SQL.
  - [x] Domain `LuuKy/`: `LoaiPhieu : byte { Thu = 1, Chi = 2 }`, `NghiepVu : byte { MangTheoKhiVao = 11, NguoiThanGui = 12, PhieuGuiQua = 13, NhanTuDoiTuongKhac = 14, MuaHang = 21, ChoTien = 22, ChuyenVeNguoiThan = 23, ChuyenTrai = 24, ChapHanhXongAn = 25 }`, `HinhThuc : byte { TienMat = 1, ChuyenKhoan = 2 }`. In `Common/`: `TrangThaiChungTu : byte { Nhap = 1, DaGhiSo = 2, DaHuy = 3 }`.
  - [x] `ChungTuLuuKy : AuditableEntity, IAuditable, ICoTrangThaiHuy` (Story 1.3 markers). Add a factory `ChungTuLuuKy.TaoBienNhanThuDaGhiSo(...)` that enforces the invariants in Domain: `SoTien > 0`; `NghiepVu / 10 == (int)LoaiPhieu`; `HinhThuc == ChuyenKhoan` ⇒ `SoTaiKhoanNguoiGui` is required. The full state machine (NEN-14) is Epic 4. Here a receipt is created directly as posted.
  - [x] Domain `HeThong/`: `NguoiDung` (`TenDangNhap`, `MatKhauHash`, `DangHoatDong`, `SoLanSai`, `KhoaDen`) and `ThongTinDonVi` (`TenCoQuanChuQuan?`, `TenDonVi`, `DiaChi`).
  - [x] Domain unit tests for the `ChungTuLuuKy` factory invariants.
- [x] **T2. Persistence: configurations and migration** (AC: 1, 2, 3)
  - [x] `NguoiDung`: `TenDangNhap nvarchar(50)` UQ NOT NULL; `MatKhauHash varchar(200)` NOT NULL; `DangHoatDong bit` default 1; `SoLanSai tinyint` default 0; `KhoaDen datetime2(0)` NULL. **Mark `MatKhauHash` as excluded from audit JSON** (Story 1.3 T6). `CanBoId` is **deferred to Epic 2** (the `CanBo` table doesn't exist yet).
  - [x] `ThongTinDonVi`: `Id int` **not** identity (`ValueGeneratedNever`), `CHECK (Id = 1)`; `TenCoQuanChuQuan nvarchar(200)` NULL; `TenDonVi nvarchar(200)` NOT NULL; `DiaChi nvarchar(300)` NOT NULL. Seed one row `Id = 1` with empty strings via `HasData` (DB design: "an empty row for the admin to fill").
  - [x] `DoiTuong`: `MaSo varchar(30)` UQ NOT NULL; `HoTen nvarchar(100)` NOT NULL + index `IX_DoiTuong_HoTen`; `NamSinh smallint` NULL; `LoaiDoiTuong tinyint` NOT NULL; `NgayVao date` NOT NULL; `BuongGiam nvarchar(50)`; `TrangThai tinyint` NOT NULL default 1; `NgayRa date` NULL with `CHECK (TrangThai = 1 OR NgayRa IS NOT NULL)`; `SoDuLuuKy decimal(18,0)` NOT NULL default 0 with `CHECK (SoDuLuuKy >= 0)`.
    - Configure `SoDuLuuKy` with `PropertySaveBehavior.Ignore` for both before- and after-save. EF then **never** writes it (it's only read back), which enforces AC 3's "no code outside `GhiSoLuuKyService` writes `SoDuLuuKy`" at the persistence level.
    - `CanBoQuanGiaoId` is **deferred to Epic 3**.
  - [x] `ChungTuLuuKy`: `Id bigint IDENTITY`; `SoChungTu varchar(20)` UQ NOT NULL; `NgayChungTu date` NOT NULL; `LoaiPhieu`, `NghiepVu`, `HinhThuc`, `TrangThai`, `LoaiDoiTuong` as `tinyint`, each with an enum CHECK via the Story 1.2 helper, plus `CHECK (NghiepVu / 10 = LoaiPhieu)`; `DoiTuongId int` FK NOT NULL (`OnDelete Restrict`).
    - Snapshot columns `HoTenDoiTuong nvarchar(100)`, `LoaiDoiTuong`. Sender columns `NguoiGuiHoTen nvarchar(100)`, `QuanHe nvarchar(50)`, `SoPhieuGoc varchar(30)`.
    - `SoTaiKhoanNguoiGui varchar(30)` with `CHECK (HinhThuc <> 2 OR SoTaiKhoanNguoiGui IS NOT NULL)`; `NgayNhan date` NULL; `NoiDung nvarchar(500)`.
    - `SoTien decimal(18,0)` with `CHECK (SoTien > 0)`; `SoTienBangChu nvarchar(300)` NOT NULL; `SoDuTruoc`, `SoDuSau decimal(18,0)`.
    - Cancellation columns `LyDoHuy nvarchar(300)`, `NgayHuy datetime2(0)`, `NguoiHuyId int`, with `CHECK (TrangThai <> 3 OR (LyDoHuy IS NOT NULL AND NgayHuy IS NOT NULL AND NguoiHuyId IS NOT NULL))`; `SoLanIn smallint` default 0.
    - Indexes: `(DoiTuongId, NgayChungTu, Id) INCLUDE (LoaiPhieu, SoTien, TrangThai)` and `(NgayChungTu) WHERE TrangThai = 2`.
    - **Deferred columns**: `NguoiThanId` (Epic 12), `PhieuBanHangId` (Epic 10), `DeNghiChoTienId` (Epic 5), `CanBoLapId` (Epic 2/4). Each later story adds its column and FK in its own migration.
  - [x] `DemSoChungTu`: PK `(LoaiChungTu varchar(10), Nam smallint)`, `TienTo varchar(10)`, `SoHienTai int` NOT NULL default 0. Seed **only `BNT`** for the current install year (2026) via `HasData`. Each later document type seeds its own counter, and year rollover is FR8 (Epic 4).
  - [x] Migration `AddWalkingSkeletonTables`. Seed the `admin` user in the same migration (see T3). Re-run Story 1.2's enum test: it now covers 6 real enums.
- [x] **T3. Password hashing and seeded admin** (AC: 1)
  - [x] Application `Abstractions/IMatKhauHasher.cs`: `string Hash(string matKhau)`, `bool Verify(string matKhau, string hash)`.
  - [x] Infrastructure `HeThong/Pbkdf2MatKhauHasher.cs`: `Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32)`, 16-byte random salt from `RandomNumberGenerator`, self-describing format `PBKDF2-SHA256$<iterations>$<saltB64>$<hashB64>` (about 90 chars, fits `varchar(200)`). Compare with `CryptographicOperations.FixedTimeEquals`. Iterations: 600,000 (current OWASP guidance for PBKDF2-SHA256). Store them in the hash string so they can be raised later.
  - [x] Seed admin: generate the hash **once** offline with the hasher and paste the literal string into the migration. Never compute it inside the migration, because migrations must be deterministic. Document the initial password in the install notes (`docs/`), not in code comments. Epic 2 (FR1) forces a change at first login.
  - [x] Unit tests: hash then verify true; wrong password false; two hashes of the same password differ (salt); a tampered hash string returns false, not an exception.
- [x] **T4. Login (thin)** (AC: 1)
  - [x] Application `HeThong/DangNhapService.cs`: load `NguoiDung` by `TenDangNhap` (inactive accounts fail), verify the hash, set `ICurrentUser` (the Story 1.3 session object), and write `HanhDong.DangNhap` via `IGhiNhatKy`. It returns a result object (success / "Tên đăng nhập hoặc mật khẩu không đúng"). **No exceptions for a wrong password**, and never say which part was wrong. Lockout and the password policy come in Epic 2.
  - [x] WinForms `Shell/`: `ILoginView` + `LoginPresenter` + `LoginForm`. In `Program`, show `LoginForm` modally first. Open `MainForm` only on success, and exit if cancelled.
  - [x] Presenter test (NSubstitute view + service): success closes the view as OK; failure shows the error and keeps the form open.
- [x] **T5. Minimal detainee form** (AC: 2)
  - [x] Application `DanhMuc/ThemDoiTuongService.cs` (+ FluentValidation: `MaSo`/`HoTen` required, `NgayVao` required and ≤ `IClock.Today`). It creates the entity with `TrangThai = DangQuanLy` and the balance at its default of 0, and saves. Map a duplicate `MaSo` to a friendly "Mã số đã tồn tại". The full detainee module is Epic 3.
  - [x] WinForms `DanhMuc/`: `IThemDoiTuongView` + presenter + form (MaSo, HoTen, NamSinh, LoaiDoiTuong combo, NgayVao, BuongGiam). Each Save creates a **new DI scope** (`IServiceScopeFactory.CreateAsyncScope()`). The form never holds a DbContext.
- [x] **T6. Thin ledger engine** (AC: 3)
  - [x] Application `Abstractions/INumberingService.cs`: `Task<string> CapSoAsync(string loaiChungTu, int nam, CancellationToken ct)`. The Infrastructure implementation runs `UPDATE DemSoChungTu WITH (UPDLOCK, ROWLOCK) SET SoHienTai = SoHienTai + 1 OUTPUT inserted.TienTo, inserted.SoHienTai WHERE LoaiChungTu = @loai AND Nam = @nam` **on the current DbContext connection and transaction**. Format the result as `BNT-2026-00001` (PO open question 1, Story 4.2). A missing counter row throws a clear `InvalidOperationException` ("no counter for BNT/2027"). Automatic rollover is Epic 4.
  - [x] Balance update port: Application `LuuKy/ISoDuLuuKyWriter.cs` with `Task<(decimal SoDuTruoc, decimal SoDuSau)?> CongAsync(int doiTuongId, decimal soTien, CancellationToken ct)`. Infrastructure implements it with `UPDATE DoiTuong WITH (UPDLOCK, ROWLOCK) SET SoDuLuuKy = SoDuLuuKy + @SoTien OUTPUT deleted.SoDuLuuKy, inserted.SoDuLuuKy WHERE Id = @Id AND TrangThai = 1`. Zero rows returns `null`, meaning the detainee doesn't exist or isn't managed. Only the receipt (add) direction is needed now. Epic 5 adds the subtract direction with `AND SoDuLuuKy >= @SoTien`.
  - [x] Application `LuuKy/GhiSoLuuKyService.cs` with `GhiSoBienNhanThuAsync(GhiSoBienNhanThuRequest, ct)`, validated by FluentValidation. In **one transaction**:
    1. Begin the transaction (`IAppDbContext.BeginTransactionAsync`).
    2. Load the detainee for the snapshot (name, type) and fail with a business error if it isn't managed.
    3. `CapSoAsync("BNT", clock.Today.Year)`.
    4. `ISoDuLuuKyWriter.CongAsync`. A `null` result means business error, then roll back.
    5. Build `ChungTuLuuKy` via the Domain factory with `TrangThai = DaGhiSo`, `SoTienBangChu = SoTienBangChu.Doc(soTien)`, `SoDuTruoc`/`SoDuSau` from the OUTPUT.
    6. `SaveChangesAsync`. The Story 1.3 interceptor writes the `Them` audit row and joins the open transaction.
    7. Commit.
    It returns `(long Id, string SoChungTu, decimal SoDuSau)`. It throws/returns business errors and never shows UI.
  - [x] **Guard rail for AC 3**: an architecture test asserts that `ISoDuLuuKyWriter` is used only by `GhiSoLuuKyService` (reflection over constructor parameters in Application). A second check confirms that the string `SoDuLuuKy =` (an update) appears only in the one Infrastructure writer file, via a source scan.
  - [x] Unit tests (NSubstitute for `INumberingService`, `ISoDuLuuKyWriter`, `IClock`): the happy path builds the document with the right snapshot, words and balances; a `null` writer result produces a business error and no document is saved; validation failures stop before numbering.
- [x] **T7. Minimal receipt form** (AC: 3)
  - [x] WinForms `LuuKy/`: `IBienNhanThuView` + presenter + form. It has a detainee selector (a simple combo or MaSo lookup is enough; the real picker comes in Story 3.2), NghiepVu restricted to receipts (12 "Người thân gửi" for this flow; disable 13 per alignment A4), NguoiGuiHoTen, QuanHe, HinhThuc (cash/transfer, with the account required for transfer), SoTien (an integer textbox is fine; the real money control UX-DR1 comes in Epic 4), and NoiDung.
  - [x] It shows the amount in words live as a preview (using `SoTienBangChu`). The Post button calls the service in a new scope. Show the business error message on failure (UX-DR10). The pre-posting confirmation dialog (UX-DR4) is Epic 4.
  - [x] After a successful post, enable **Print**.
- [x] **T8. Print the receipt** (AC: 4)
  - [x] Implement the `IReportRenderer` shape agreed in Story 1.5 (Application `Abstractions/`). Application `BaoCao/` (or `LuuKy/`): `BienNhanThuModel` (unit header, `SoChungTu`, `NgayChungTu`, snapshot name/type, sender, `SoTien`, `SoTienBangChu`) and a query `LayBienNhanThuDeInQuery` that reads the **stored snapshot** columns plus `ThongTinDonVi`.
  - [x] Infrastructure `Reports/BienNhanThuReport.cs` (QuestPDF, the embedded Vietnamese font from Story 1.5). The layout follows process A.I.5 loosely. The shared frame, signer configuration, print counting and reprint mark are **Epic 4** (FR42/FR43/FR27), so don't build them here.
  - [x] Format money with `vi-VN` culture (`soTien.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))` gives `500.000`).
  - [x] WinForms `Common/PdfPreviewForm` (WebView2, from the spike's findings): preview, print and save.
  - [x] Dev convenience: `--seed-demo` (Story 1.2) fills `ThongTinDonVi` with demo values, so the printed header isn't blank on a dev machine.
- [x] **T9. Integration test of the whole flow + reconciliation** (AC: 3, 5)
  - [x] `IntegrationTests/LuuKy/WalkingSkeletonTests.cs` on the Story 1.2 LocalDB fixture, with a `FakeClock` at 2026-10-01 and a fake `ICurrentUser` = admin:
    - log in with the seeded admin;
    - add "Nguyễn Văn A";
    - post 500,000 cash from a relative;
    - assert `SoChungTu = "BNT-2026-00001"`, `TrangThai = 2`, `SoDuTruoc = 0`, `SoDuSau = 500000`, `SoTienBangChu = "Năm trăm nghìn đồng"`, the snapshot name, `DoiTuong.SoDuLuuKy = 500000`, and one `NhatKyThaoTac` row (`Them`, `ChungTuLuuKy`, the new Id) plus one `DangNhap` row.
  - [x] Rollback test: force a failure after numbering (e.g. a substitute writer that returns `null`, or a detainee with `TrangThai = 2`). Assert that no document, no audit row, no balance change and **no consumed number** remain (the counter is back to 0), all inside the same transaction.
  - [x] Render test: call `IReportRenderer` for the posted receipt and assert that the bytes start with `%PDF`. Optionally extract the text with **PdfPig** (`UglyToad.PdfPig`, Apache-2.0, test project only) and assert it contains the unit name, `BNT-2026-00001`, "Nguyễn Văn A", "500.000" and "Năm trăm nghìn đồng".
  - [x] `IntegrationTests/Infrastructure/LedgerReconciliation.cs`: one SQL query returns every detainee where `SoDuLuuKy <> ISNULL(SUM(CASE LoaiPhieu WHEN 1 THEN SoTien ELSE -SoTien END) over posted (TrangThai = 2) documents, 0)`. `AssertBalanced()` must find zero rows. Call it at the end of **every** integration test that touches the ledger. A base class or fixture `DisposeAsync` hook is the simplest way (NEN-19, NFR11).
- [x] **T10. Manual end-to-end check** (AC: 1–4)
  - [ ] Ran `--migrate` against LocalDB (3 migrations applied, exit 0), `--seed-demo --environment Development` (exit 0), verified `admin`, the demo `ThongTinDonVi` values and the `BNT/2026` counter in SQL, started the app and confirmed the login shell stays up with a clean log. **The interactive GUI walk (typing the password, using the forms, viewing the preview) and the gate screenshot were still not done** — the same flow is proven end to end by `WalkingSkeletonTests`; a human still owes the Sprint 0 screenshot before this task is complete.

## Dev Notes

### Current codebase state (after Stories 1.1–1.5)

- Host, Serilog, global handler and MVP sample (1.1). `AppDbContext`, conventions, `AuditableEntity`, `HasEnumCheck`, `InitialCreate`, LocalDB fixture, enum test, `--migrate` / `--seed-demo`, schema-version check (1.2). `IClock`/`FakeClock`, `ICurrentUser`, `NhatKyThaoTac`, `AuditInterceptor`, `IGhiNhatKy`, `IAuditable`/`ICoTrangThaiHuy` (1.3). `SoTienBangChu` (1.4). The QuestPDF/WebView2 decision note and the `IReportRenderer` shape in `docs/decisions/0001-*` (1.5). **Read those decision notes first.**

### Why this story is shaped this way (backlog alignment A9)

- It's the last story of Epic 1 and crosses features that are fully built later (login → Epic 2, detainees → Epic 3, receipts/engine/print frame → Epic 4). It deliberately builds **thin but real** versions: the same engine, the same tables, the same transaction rule. **Nothing bypasses the engine, even in the skeleton.** Epic 4 grows `GhiSoLuuKyService` instead of replacing it, so name and place things as their final versions.

### Ledger-first rules (CLAUDE.md, non-negotiable)

- `GhiSoLuuKyService` is the **only** writer of `ChungTuLuuKy` and `SoDuLuuKy`. The balance changes via the conditional `UPDATE … WITH (UPDLOCK, ROWLOCK) OUTPUT deleted/inserted` in the same statement, never read-then-write (NFR3).
- One posting = one service method = one transaction: number, balance, document and audit row commit or roll back together.
- Money is `decimal(18,0)`, integer đồng. The balance never goes negative (DB `CHECK` + conditional update).
- Snapshot the name and type at posting, and print from the snapshot (NFR8).
- Document numbers come only from `INumberingService` with UPDLOCK, per type and year.
- Dates come only from `IClock` (`NgayChungTu = clock.Today`).
- Services never touch WinForms. Forms never hold a DbContext, and each operation creates a fresh DI scope.

### Gotchas

- **`SqlQuery` + `UPDATE … OUTPUT`**: EF wraps a raw query in a sub-select when you **compose** LINQ on it (`FirstAsync`, `SingleAsync`, `Where`). That fails for an `UPDATE`. Materialize first with `ToListAsync()`, then use `.Single()` in memory. Use a small unmapped result type (EF 8+ supports `SqlQuery<T>` for unmapped types; columns must match the property names).
- The raw SQL must run **inside the service's transaction**. Use the same scoped `AppDbContext` (whose `Database.CurrentTransaction` is set), not a new connection.
- `SoDuLuuKy` is ignored on save, so after the raw update the tracked `DoiTuong` holds a stale balance. Don't read it back from the tracked entity in the same scope. Use the OUTPUT values.
- Raw SQL bypasses the audit interceptor (Story 1.3 note). That's fine for `DoiTuong.SoDuLuuKy`, because the voucher row carries `SoDuTruoc`/`SoDuSau` and is audited.
- **`rowversion` on `DoiTuong`** changes on the raw update. A concurrently open detainee edit form gets a concurrency conflict on save. That's correct behaviour, but Epic 3 has to handle it with a friendly message.
- **Numbers and rollback**: the counter `UPDATE` is inside the transaction, so a rollback returns the number and leaves no gap. Gaps only appear when a *committed* draft is later cancelled (alignment A16, Epic 4).
- `ThongTinDonVi.Id` must not be IDENTITY (`CHECK (Id = 1)`, seeded row).
- `vi-VN` uses `.` as the thousands separator. Never format with the current UI culture, because workstations may run en-US Windows.
- The WinForms `Application` name clash again (Story 1.1 gotcha).

### Out of scope (later epics, don't build)

- Lockout, password policy, forced password change, roles/permissions, session lock (Epic 2). Staff `CanBo` and the `CanBoId` columns (Epic 2). Detainee search, picker, edit, type history (Epic 3). Drafts, the state machine, edit/cancel, the pre-posting confirmation, the money input control, the print frame with signers, print counting, the reprint mark, year rollover, concurrency tests (Epic 4). Payouts (Epic 5). Period lock (Epic 6).

### Testing summary

- Domain: `ChungTuLuuKy` factory invariants. `SoTienBangChu` already exists.
- Application: `GhiSoLuuKyService`, `DangNhapService`, `ThemDoiTuongService` with NSubstitute fakes and `FakeClock`.
- WinForms: `LoginPresenter`, `BienNhanThuPresenter` (NSubstitute views).
- Integration (LocalDB): the walking-skeleton flow, the rollback test, the PDF render test, the enum test (now meaningful), and reconciliation after each test.
- Architecture: the `ISoDuLuuKyWriter` usage guard.

### References

- Epic 1 › Story 1.8; `epics.md` › Additional Requirements › Ledger engine and domain rules (NEN-16, NEN-19), Persistence and data (seed, enums, constraints, indexes); Backlog Alignment A4, A9, A16; Open Questions #1
- DB design PDF: `ThongTinDonVi`, `NguoiDung` (p.4), `DemSoChungTu` (p.5), `DoiTuong` (p.6–7), `ChungTuLuuKy` (p.9–10), conditional UPDATE + indexes (p.14), seed data (p.15)
- Process doc `document/QUY TRÌNH TIẾP NHẬN TIỀN GỬI LƯU KÝ.docx`: A.I.4 (receipt fields), A.I.5 (printed receipt)
- `docs/decisions/0001-pdf-engine-questpdf.md` (Story 1.5 outcome)
- `CLAUDE.md`: Key design rules

## Dev Agent Record

### Agent Model Used

deepseek-v4.1-flash (opencode). Implemented directly from this story file.

### Debug Log References

1. **T0 resolved as "no EF in Application"** (see T0 note). Narrow ports keep the Story 1.1 architecture test untouched and follow Story 1.2's precedent (`ISchemaVersionChecker`, `IDatabaseMigrator` are plain ports).
2. **`SqlQuery` + `UPDATE … OUTPUT`**: `SoDuLuuKyWriter` and `DemSoChungTuNumberingService` materialize with `ToListAsync()` before any LINQ (the Dev Notes gotcha); tests prove both statements release/return state correctly. Both run on the scoped `AppDbContext`, so they join the service's transaction.
3. **Enum ↔ CHECK verifier widened**: business CHECKs that merely mention an enum column (`[TrangThai] = 1 OR [NgayRa] IS NOT NULL`, `[NghiepVu] / 10 = [LoaiPhieu]`, `[HinhThuc] <> 2 OR …`, `[TrangThai] <> 3 OR …`) were being intersected into the allowed-value set and made the enum test fail. `EnumCheckVerifier.ParseAllowedValues` now ignores a constraint as soon as it references any other column; only a constraint about the column alone defines its enum set. `EnumCheckVerifierTests` and the deployed-DB self-test still pass.
4. **EF sentinel warning** (`TrangThaiDoiTuong`/`DangHoatDong` database defaults): silenced with `HasSentinel(...)` set to the default value; `dotnet ef migrations has-pending-model-changes` reports no model change, so no new migration.
5. **NSubstitute + concrete services**: presenters resolve concrete Application services from a scope, so presenter tests build the real service with substituted ports (a substitute cannot intercept a non-virtual method).
6. **PdfPig** package id is `PdfPig` (namespace `UglyToad.PdfPig`), not `UglyToad.PdfPig`; the walking-skeleton render test extracts the PDF text and finds the unit name, number, name, `500.000` and `Năm trăm nghìn đồng`.

### Completion Notes List

- **Status: implemented, all tests green** — after the review fixes: 270 tests (49 Domain + 32 Application + 31 WinForms + 158 Integration), 0 failed, 0 skipped with LocalDB running. `dotnet build LuuKyCanTin.slnx` warning-free.
- **Ledger-first holds in the skeleton.** `GhiSoLuuKyService` is the only Application type that touches `ISoDuLuuKyWriter`; the source scan finds the balance `UPDATE` only in `SoDuLuuKyWriter.cs`; `DoiTuong.SoDuLuuKy` is `PropertySaveBehavior.Ignore` in EF, so no other save can write it. Both guard tests are in `Architecture/SoDuLuuKyWriterUsageTests.cs`.
- **One transaction per posting.** Number, conditional balance UPDATE, voucher and interceptor audit row commit or roll back together. The rollback test forces the writer to return `null` after numbering and asserts the counter is back to 0, the balance is unchanged, and no document or audit row remains.
- **Seeded admin.** `admin` / `LuuKy@2026` (PBKDF2-SHA256, 600,000 iterations, random salt, 90-char self-describing string pasted into `AddWalkingSkeletonTables`). Documented in `docs/install.md`; Epic 2 forces the change at first sign-in.
- **T0 deviation** (documented above): no `DbSet<T>` in Application. `IAppDbContext` = save + transaction; stores are per-aggregate.
- **T10 gap**: the agent verified migrate/seed/app-start and DB state, but could not drive the GUI or take the Sprint 0 screenshot. The integration test walks the same four layers (login → detainee → posting → render).
- **Known thin spots for later stories**: `DangNhapService` ignores `KhoaDen`/`SoLanSai` (Epic 2); no password-rotation tool exists before Epic 2; the print frame, signers, print counting and reprint watermark are deliberately absent (Epic 4); `QuestPDF.Settings.License = Community` is the spike's evaluation setting pending the PO decision in ADR 0001. The duplicate-`MaSo` race was closed in review (the store maps SQL 2601/2627 to the friendly message).

### File List

**Domain**
- `src/Libraries/LuuKyCanTin.Domain/Common/TrangThaiChungTu.cs` (new)
- `src/Libraries/LuuKyCanTin.Domain/DanhMuc/LoaiDoiTuong.cs`, `TrangThaiDoiTuong.cs`, `DoiTuong.cs` (new)
- `src/Libraries/LuuKyCanTin.Domain/LuuKy/LoaiPhieu.cs`, `NghiepVu.cs`, `HinhThuc.cs`, `ChungTuLuuKy.cs` (new)
- `src/Libraries/LuuKyCanTin.Domain/HeThong/NguoiDung.cs`, `ThongTinDonVi.cs`, `DemSoChungTu.cs` (new)
- `tests/LuuKyCanTin.Domain.UnitTests/LuuKy/ChungTuLuuKyTests.cs` (new)

**Application**
- `src/Libraries/LuuKyCanTin.Application/Abstractions/IAppDbContext.cs`, `IAppTransaction.cs`, `INumberingService.cs`, `IMatKhauHasher.cs`, `ICurrentUserSession.cs`, `IReportRenderer.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/DanhMuc/IDoiTuongStore.cs`, `ThemDoiTuongRequest.cs`, `ThemDoiTuongValidator.cs`, `ThemDoiTuongService.cs`, `KetQuaThemDoiTuong.cs`, `DoiTuongChon.cs`, `LayDoiTuongDangQuanLyQuery.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/HeThong/DangNhapService.cs`, `KetQuaDangNhap.cs`, `INguoiDungStore.cs`, `IThongTinDonViStore.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/LuuKy/ISoDuLuuKyWriter.cs`, `IChungTuLuuKyStore.cs`, `GhiSoBienNhanThuRequest.cs`, `GhiSoBienNhanThuValidator.cs`, `GhiSoLuuKyService.cs`, `KetQuaGhiSo.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/BaoCao/IReportModel.cs`, `BienNhanThuModel.cs`, `LayBienNhanThuDeInQuery.cs` (new)
- `src/Libraries/LuuKyCanTin.Application/DependencyInjection.cs`, `LuuKyCanTin.Application.csproj` (modified)
- `tests/LuuKyCanTin.Application.UnitTests/LuuKy/GhiSoLuuKyServiceTests.cs`, `HeThong/DangNhapServiceTests.cs`, `DanhMuc/ThemDoiTuongServiceTests.cs`, `TestUtilities/FakeClock.cs` (new)

**Infrastructure**
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/HeThong/NguoiDungConfiguration.cs`, `ThongTinDonViConfiguration.cs`, `DemSoChungTuConfiguration.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/DanhMuc/DoiTuongConfiguration.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/LuuKy/ChungTuLuuKyConfiguration.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/AppDbContext.cs`, `AppTransaction.cs`, `Seed/Demo/DemoDataSeeder.cs` (modified/new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/20261001221409_AddWalkingSkeletonTables.cs` (+ `.Designer.cs`, snapshot) (new/modified)
- `src/Libraries/LuuKyCanTin.Infrastructure/HeThong/Pbkdf2MatKhauHasher.cs`, `NguoiDungStore.cs`, `ThongTinDonViStore.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/DanhMuc/DoiTuongStore.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/LuuKy/DemSoChungTuNumberingService.cs`, `SoDuLuuKyWriter.cs`, `ChungTuLuuKyStore.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/Reports/IReportTemplate.cs`, `QuestPdfReportRenderer.cs`, `ReportFonts.cs`, `BienNhanThuReport.cs`, `Fonts/*` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/HeThong/CurrentUserSession.cs`, `DependencyInjection.cs`, `LuuKyCanTin.Infrastructure.csproj` (modified)

**WinForms**
- `src/Presentation/LuuKyCanTin.WinForms/Shell/ILoginView.cs`, `LoginPresenter.cs`, `LoginForm.cs`, `LoginForm.Designer.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/DanhMuc/IThemDoiTuongView.cs`, `ThemDoiTuongPresenter.cs`, `ThemDoiTuongForm.cs`, `ThemDoiTuongForm.Designer.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/LuuKy/IBienNhanThuView.cs`, `BienNhanThuPresenter.cs`, `BienNhanThuForm.cs`, `BienNhanThuForm.Designer.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Common/PdfPreviewForm.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Shell/MainForm.cs`, `MainForm.Designer.cs`, `Program.cs`, `LuuKyCanTin.WinForms.csproj` (modified)
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/LoginPresenterTests.cs`, `DanhMuc/ThemDoiTuongPresenterTests.cs`, `LuuKy/BienNhanThuPresenterTests.cs`, `TestUtilities/ScopeFactoryGia.cs`, `TestUtilities/FakeClock.cs` (new)

**Tests / docs / root**
- `tests/LuuKyCanTin.IntegrationTests/LuuKy/WalkingSkeletonTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Infrastructure/LedgerReconciliation.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Architecture/SoDuLuuKyWriterUsageTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/HeThong/Pbkdf2MatKhauHasherTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/EnumChecks/EnumCheckVerifier.cs`, `EnumModelTests.cs`, `EnumCheckVerifierTests.cs` (modified)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/DoiTuongSoDuTests.cs`, `Persistence/Seed/DemoDataSeederTests.cs`, `LuuKyCanTin.IntegrationTests.csproj` (new/modified)
- `_bmad-output/implementation-artifacts/deferred-work.md` (new: UI preview and login-gate coverage deferrals)
- `Directory.Packages.props` (modified: FluentValidation 12.1.1, QuestPDF 2026.9.1, Microsoft.Web.WebView2 1.0.4258.31, PdfPig 0.1.16)
- `docs/install.md` (new: connection setup, admin commands, initial credentials)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-02 | Implemented T0–T9 and the T10 admin/startup checks (deepseek-v4.1-flash); interactive GUI screenshot still pending; status → review |
| 2026-10-02 | Review fixes applied: presenters surface unexpected errors, submit buttons guard against double posts, the `MaSo` race maps to the friendly message, validator bounds/`IsInEnum` + tests, vi-VN money parsing, PBKDF2 iteration cap, wider balance guard + `PropertySaveBehavior.Ignore` test, demo-seeder guard + test, install.md corrections, enum-verifier test, WebView2 WPF reference removed (warning-free build); T10 marker corrected; UI preview/login-gate deferrals recorded (deepseek-v4.1-flash) |
| 2026-10-02 | Merged `master` (Epic 2.1 staff register): T0 settled as **yes** — Application references EF Core core (no provider); `IAppDbContext` = CanBo `DbSet`/`Entry`/`SaveChangesAsync` + `BeginTransactionAsync`; DI/Program/MainForm/snapshot union-merged; 350 tests green, snapshot reports no pending model changes |

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | `docs/install.md` tells the admin to change the seeded password with an admin tool that does not exist (blind hunter) | low — patch | Verified: no user-management surface exists and no rotation tool is referenced; Epic 2 owns password change. Doc corrected to say the skeleton has no rotation path yet. |
| 2 | `docs/install.md` omits the WebView2 Runtime prerequisite that the preview/print path needs (blind hunter, both runs) | low — patch | Verified: `PdfPreviewForm.InitializeAsync` fails with a runtime dialog without WebView2 Runtime, and install.md never lists it. Prerequisite added to install.md. |
| 3 | Duplicate `MaSo` race: the pre-check loses to a concurrent insert and the unique-index `DbUpdateException` is not mapped to the friendly message (blind hunter, edge-case hunter twice) | low — patch | Verified: `ThemDoiTuongService` checks then saves; the race surfaces through the global handler. Patched: the store saves with the duplicate translated to a `false` result, so the service returns `MaSoDaTonTai`. Application stays EF-free. |
| 4 | Presenter event handlers are fire-and-forget (`_ = XxxAsync()`), so any exception is unobserved and the user sees nothing (blind hunter, edge-case hunter twice) | medium — patch | Verified in `LoginPresenter`, `ThemDoiTuongPresenter`, `BienNhanThuPresenter`; a missing counter, SQL or render failure is silently swallowed. Patched: handlers await inside try/catch and show the error through the view. |
| 5 | No submit re-entrancy: the "Ghi sổ"/"Lưu" button stays enabled while the operation runs, so a double-click posts or saves twice (blind hunter twice, edge-case hunter) | medium — patch | Verified: the buttons are disabled only after success; a second click starts a second scope+transaction and can double a deposit. Patched: the view disables the button on submit and re-enables it on failure. |
| 6 | T10 is marked `[x]` although its own note admits the interactive GUI walk and the Sprint 0 screenshot were never done (blind hunter) | low — patch | Verified against the Dev Agent Record. Task marker corrected to unchecked with the pending manual step named. |
| 7 | `ICurrentUserSession.DangXuat` is declared but never called and the shell has no sign-out action (blind hunter) | false | Sign-out is not in this story's ACs (thin login only; Epic 2 owns the session rules). The wider session contract is used by `DangNhapService`; the unused method is not a defect of this change. |
| 8 | `LayBienNhanThuDeInQuery` renders any voucher id with the receipt template; no `TrangThai`/`LoaiPhieu` guard (blind hunter, edge-case hunter) | false | Unreachable in this skeleton: only posted receipts exist (no drafts, cancellations or payouts until Epics 4/5), and the only caller passes the id it just posted. Epic 4 adds the state guard when those states exist. |
| 9 | `PdfPreviewForm.FormClosing` disposes WebView2 while `Load` initialization may still be pending; `CleanupPreviewFiles` deletes every `*.pdf` (blind hunter) | low — reject | Initialization failures are caught and shown; a file open in another instance's Chromium viewer is locked, so `File.Delete` fails and is deliberately ignored (`PdfPreviewForm.cs:167-170`), and one modal preview exists per process. Fixing the theoretical race adds lifecycle guards without a demonstrated outcome. |
| 10 | `GhiSoBienNhanThuValidator` only requires `NgayChungTu` non-empty; numbering uses `clock.Today.Year` while the document date can differ (blind hunter) | false | The working-year rule is an explicit, tested design decision (`Numbering_UsesTheWorkingYearNotTheDocumentDate`), and back-date rules are Epic 4/6 (period lock); AC3 only requires the number in the current year. `IClock` drives numbering and audit. |
| 11 | `SoTien` has no upper bound; an amount over `decimal(18,0)` passes validation and fails inside `SaveChangesAsync` (blind hunter twice, edge-case hunter) | low — patch | Verified: only `GreaterThan(0)` and integer checks exist. Patched: `.LessThanOrEqualTo(999_999_999_999_999_999m)` so the failure is a business message. |
| 12 | `Pbkdf2MatKhauHasher.Verify` accepts any positive iteration count, so a tampered hash can force an arbitrarily expensive computation (blind hunter, edge-case hunter) | low — patch | Verified: the parsed iterations are used unchecked. Patched: cap the accepted range (e.g. 1…1,000,000); malformed/tampered values return false. |
| 13 | `ChungTuLuuKy.TaoBienNhanThuDaGhiSo` does not check `SoDuSau == SoDuTruoc + SoTien` or reject an undefined `HinhThuc` (blind hunter) | false | The story defines the factory's three invariants and they are implemented; the balances come only from the writer's OUTPUT (equal by construction) and the enum is enforced by `CK_ChungTuLuuKy_HinhThuc`. No reachable call builds an inconsistent voucher. |
| 14 | `MainForm`'s parameterless designer constructor passes `null` and the menu handlers dereference `_scopeFactory!` (blind hunter) | false | The parameterless constructor exists for the WinForms designer only; the app always resolves the `IServiceScopeFactory` constructor (`Program.cs:76`), and a designer-created form is never run. |
| 15 | The new mixed-column branch in `EnumCheckVerifier.ParseAllowedValues` ships with no test (blind hunter) | low — patch | Verified: `EnumCheckVerifierTests` covers pure enum constraints only. Patched: a test for a definition that mentions the column plus another (returns no allowed set). |
| 16 | `Pbkdf2MatKhauHasher.Verify(null, hash)` throws `NullReferenceException` (edge-case hunter) | false | `NguoiDung.MatKhauHash` is a required, non-null column (`varchar(200) NOT NULL`) and the only caller passes the loaded entity; no path supplies null. The nullability contract is enforced by the schema. |
| 17 | `GhiSoBienNhanThuValidator` lacks `IsInEnum` for `HinhThuc`, and the business rules (blank sender, payout `NghiepVu`, transfer without account) have no service-level tests (edge-case hunter, verification-gap layer) | low — patch | Verified: `NghiepVu` has `IsInEnum` but `HinhThuc` does not, and the only validation test is `soTien = 0`. Patched: rule added and theory cases assert business errors, no numbering and no save. |
| 18 | `BienNhanThuForm` strips `.`, `,` and spaces before `NumberStyles.None`, so `1.234,5` posts as 12345 and `12.5` as 125 (edge-case hunter, verification-gap layer) | low — patch | Verified in the getter. Patched: parse with `vi-VN` + `NumberStyles.AllowThousands`, so `500.000` works and malformed/decimal input is rejected by the existing amount validation. |
| 19 | `DemoDataSeeder.FillUnitInfoAsync` overwrites a row that only has `TenCoQuanChuQuan` filled, and no test runs the real seeder (edge-case hunter, verification-gap layer) | low — patch | Verified: the guard only checks `TenDonVi`/`DiaChi`. Patched: treat any non-empty field as "already filled" and add an integration test that an empty row is filled and a filled row is untouched. |
| 20 | The balance-writer guard rail is a literal source scan (`"SoDuLuuKy ="`) and the reflection check only looks at constructors, so a second writer using `[SoDuLuuKy]=`/`ExecuteUpdate` evades both (verification-gap layer) | medium — patch | Verified: `SoDuLuuKyWriterUsageTests` is exactly as described. Patched: widen the scan to a whitespace/bracket-insensitive pattern and add an integration assertion that saving a `DoiTuong` with a changed non-balance field leaves `SoDuLuuKy` untouched. |
| 21 | The real PDF preview/print window is never exercised and the planned manual check is outstanding (verification-gap layer) | low — defer | The repo has no WinForms/WebView2 rendering-test infrastructure (pre-existing), and the story's own T10 manual walk is the realistic check; recorded in `deferred-work.md`. |
| 22 | The sign-in gate that opens the shell only on success is untested (verification-gap layer) | low — defer | Composition-root UI code the repo does not test by design; the T10 interactive walk covers it. Recorded in `deferred-work.md`. |
