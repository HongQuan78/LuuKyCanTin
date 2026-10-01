---
story: "1.8"
epic: 1
title: "Walking skeleton: sign in, register a detainee, post and print one receipt"
status: ready-for-dev
size: L
backlogItems: [NEN-08]
frsCovered: []
frsTouchedThin: [FR1, FR7, FR14, FR23, FR27]
nfrsTouched: [NFR1, NFR2, NFR3, NFR5, NFR6, NFR8, NFR11]
dependsOn: ["1.2", "1.3", "1.4", "1.5"]
---

# Story 1.8: Walking skeleton: sign in, register a detainee, post and print one receipt

Status: ready-for-dev

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

- [ ] **T0. Resolve the Application ↔ EF Core decision** (prerequisite, see Story 1.1 Dev Notes)
  - [ ] Confirm with the PO/team whether Application may reference `Microsoft.EntityFrameworkCore` (core, no provider) so that `IAppDbContext` exposes `DbSet<T>`. Update the Story 1.1 architecture test to match. The tasks below assume **yes**. If the answer is no, replace the `DbSet` usage with narrow ports (`IDoiTuongStore`, `IChungTuLuuKyStore`) and keep everything else.
- [ ] **T1. Domain: enums and entities** (AC: 2, 3)
  - [ ] Domain `DanhMuc/`: `LoaiDoiTuong : byte { TamGiuTamGiam = 1, PhamNhan = 2 }`, `TrangThaiDoiTuong : byte { DangQuanLy = 1, DaChuyenTrai = 2, DaChapHanhXongAn = 3 }`, and the entity `DoiTuong : AuditableEntity`. `SoDuLuuKy` has a **private setter** with no public mutator. Only the ledger engine changes it, through SQL.
  - [ ] Domain `LuuKy/`: `LoaiPhieu : byte { Thu = 1, Chi = 2 }`, `NghiepVu : byte { MangTheoKhiVao = 11, NguoiThanGui = 12, PhieuGuiQua = 13, NhanTuDoiTuongKhac = 14, MuaHang = 21, ChoTien = 22, ChuyenVeNguoiThan = 23, ChuyenTrai = 24, ChapHanhXongAn = 25 }`, `HinhThuc : byte { TienMat = 1, ChuyenKhoan = 2 }`. In `Common/`: `TrangThaiChungTu : byte { Nhap = 1, DaGhiSo = 2, DaHuy = 3 }`.
  - [ ] `ChungTuLuuKy : AuditableEntity, IAuditable, ICoTrangThaiHuy` (Story 1.3 markers). Add a factory `ChungTuLuuKy.TaoBienNhanThuDaGhiSo(...)` that enforces the invariants in Domain: `SoTien > 0`; `NghiepVu / 10 == (int)LoaiPhieu`; `HinhThuc == ChuyenKhoan` ⇒ `SoTaiKhoanNguoiGui` is required. The full state machine (NEN-14) is Epic 4. Here a receipt is created directly as posted.
  - [ ] Domain `HeThong/`: `NguoiDung` (`TenDangNhap`, `MatKhauHash`, `DangHoatDong`, `SoLanSai`, `KhoaDen`) and `ThongTinDonVi` (`TenCoQuanChuQuan?`, `TenDonVi`, `DiaChi`).
  - [ ] Domain unit tests for the `ChungTuLuuKy` factory invariants.
- [ ] **T2. Persistence: configurations and migration** (AC: 1, 2, 3)
  - [ ] `NguoiDung`: `TenDangNhap nvarchar(50)` UQ NOT NULL; `MatKhauHash varchar(200)` NOT NULL; `DangHoatDong bit` default 1; `SoLanSai tinyint` default 0; `KhoaDen datetime2(0)` NULL. **Mark `MatKhauHash` as excluded from audit JSON** (Story 1.3 T6). `CanBoId` is **deferred to Epic 2** (the `CanBo` table doesn't exist yet).
  - [ ] `ThongTinDonVi`: `Id int` **not** identity (`ValueGeneratedNever`), `CHECK (Id = 1)`; `TenCoQuanChuQuan nvarchar(200)` NULL; `TenDonVi nvarchar(200)` NOT NULL; `DiaChi nvarchar(300)` NOT NULL. Seed one row `Id = 1` with empty strings via `HasData` (DB design: "an empty row for the admin to fill").
  - [ ] `DoiTuong`: `MaSo varchar(30)` UQ NOT NULL; `HoTen nvarchar(100)` NOT NULL + index `IX_DoiTuong_HoTen`; `NamSinh smallint` NULL; `LoaiDoiTuong tinyint` NOT NULL; `NgayVao date` NOT NULL; `BuongGiam nvarchar(50)`; `TrangThai tinyint` NOT NULL default 1; `NgayRa date` NULL with `CHECK (TrangThai = 1 OR NgayRa IS NOT NULL)`; `SoDuLuuKy decimal(18,0)` NOT NULL default 0 with `CHECK (SoDuLuuKy >= 0)`.
    - Configure `SoDuLuuKy` with `PropertySaveBehavior.Ignore` for both before- and after-save. EF then **never** writes it (it's only read back), which enforces AC 3's "no code outside `GhiSoLuuKyService` writes `SoDuLuuKy`" at the persistence level.
    - `CanBoQuanGiaoId` is **deferred to Epic 3**.
  - [ ] `ChungTuLuuKy`: `Id bigint IDENTITY`; `SoChungTu varchar(20)` UQ NOT NULL; `NgayChungTu date` NOT NULL; `LoaiPhieu`, `NghiepVu`, `HinhThuc`, `TrangThai`, `LoaiDoiTuong` as `tinyint`, each with an enum CHECK via the Story 1.2 helper, plus `CHECK (NghiepVu / 10 = LoaiPhieu)`; `DoiTuongId int` FK NOT NULL (`OnDelete Restrict`).
    - Snapshot columns `HoTenDoiTuong nvarchar(100)`, `LoaiDoiTuong`. Sender columns `NguoiGuiHoTen nvarchar(100)`, `QuanHe nvarchar(50)`, `SoPhieuGoc varchar(30)`.
    - `SoTaiKhoanNguoiGui varchar(30)` with `CHECK (HinhThuc <> 2 OR SoTaiKhoanNguoiGui IS NOT NULL)`; `NgayNhan date` NULL; `NoiDung nvarchar(500)`.
    - `SoTien decimal(18,0)` with `CHECK (SoTien > 0)`; `SoTienBangChu nvarchar(300)` NOT NULL; `SoDuTruoc`, `SoDuSau decimal(18,0)`.
    - Cancellation columns `LyDoHuy nvarchar(300)`, `NgayHuy datetime2(0)`, `NguoiHuyId int`, with `CHECK (TrangThai <> 3 OR (LyDoHuy IS NOT NULL AND NgayHuy IS NOT NULL AND NguoiHuyId IS NOT NULL))`; `SoLanIn smallint` default 0.
    - Indexes: `(DoiTuongId, NgayChungTu, Id) INCLUDE (LoaiPhieu, SoTien, TrangThai)` and `(NgayChungTu) WHERE TrangThai = 2`.
    - **Deferred columns**: `NguoiThanId` (Epic 12), `PhieuBanHangId` (Epic 10), `DeNghiChoTienId` (Epic 5), `CanBoLapId` (Epic 2/4). Each later story adds its column and FK in its own migration.
  - [ ] `DemSoChungTu`: PK `(LoaiChungTu varchar(10), Nam smallint)`, `TienTo varchar(10)`, `SoHienTai int` NOT NULL default 0. Seed **only `BNT`** for the current install year (2026) via `HasData`. Each later document type seeds its own counter, and year rollover is FR8 (Epic 4).
  - [ ] Migration `AddWalkingSkeletonTables`. Seed the `admin` user in the same migration (see T3). Re-run Story 1.2's enum test: it now covers 6 real enums.
- [ ] **T3. Password hashing and seeded admin** (AC: 1)
  - [ ] Application `Abstractions/IMatKhauHasher.cs`: `string Hash(string matKhau)`, `bool Verify(string matKhau, string hash)`.
  - [ ] Infrastructure `HeThong/Pbkdf2MatKhauHasher.cs`: `Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32)`, 16-byte random salt from `RandomNumberGenerator`, self-describing format `PBKDF2-SHA256$<iterations>$<saltB64>$<hashB64>` (about 90 chars, fits `varchar(200)`). Compare with `CryptographicOperations.FixedTimeEquals`. Iterations: 600,000 (current OWASP guidance for PBKDF2-SHA256). Store them in the hash string so they can be raised later.
  - [ ] Seed admin: generate the hash **once** offline with the hasher and paste the literal string into the migration. Never compute it inside the migration, because migrations must be deterministic. Document the initial password in the install notes (`docs/`), not in code comments. Epic 2 (FR1) forces a change at first login.
  - [ ] Unit tests: hash then verify true; wrong password false; two hashes of the same password differ (salt); a tampered hash string returns false, not an exception.
- [ ] **T4. Login (thin)** (AC: 1)
  - [ ] Application `HeThong/DangNhapService.cs`: load `NguoiDung` by `TenDangNhap` (inactive accounts fail), verify the hash, set `ICurrentUser` (the Story 1.3 session object), and write `HanhDong.DangNhap` via `IGhiNhatKy`. It returns a result object (success / "Tên đăng nhập hoặc mật khẩu không đúng"). **No exceptions for a wrong password**, and never say which part was wrong. Lockout and the password policy come in Epic 2.
  - [ ] WinForms `Shell/`: `ILoginView` + `LoginPresenter` + `LoginForm`. In `Program`, show `LoginForm` modally first. Open `MainForm` only on success, and exit if cancelled.
  - [ ] Presenter test (NSubstitute view + service): success closes the view as OK; failure shows the error and keeps the form open.
- [ ] **T5. Minimal detainee form** (AC: 2)
  - [ ] Application `DanhMuc/ThemDoiTuongService.cs` (+ FluentValidation: `MaSo`/`HoTen` required, `NgayVao` required and ≤ `IClock.Today`). It creates the entity with `TrangThai = DangQuanLy` and the balance at its default of 0, and saves. Map a duplicate `MaSo` to a friendly "Mã số đã tồn tại". The full detainee module is Epic 3.
  - [ ] WinForms `DanhMuc/`: `IThemDoiTuongView` + presenter + form (MaSo, HoTen, NamSinh, LoaiDoiTuong combo, NgayVao, BuongGiam). Each Save creates a **new DI scope** (`IServiceScopeFactory.CreateAsyncScope()`). The form never holds a DbContext.
- [ ] **T6. Thin ledger engine** (AC: 3)
  - [ ] Application `Abstractions/INumberingService.cs`: `Task<string> CapSoAsync(string loaiChungTu, int nam, CancellationToken ct)`. The Infrastructure implementation runs `UPDATE DemSoChungTu WITH (UPDLOCK, ROWLOCK) SET SoHienTai = SoHienTai + 1 OUTPUT inserted.TienTo, inserted.SoHienTai WHERE LoaiChungTu = @loai AND Nam = @nam` **on the current DbContext connection and transaction**. Format the result as `BNT-2026-00001` (PO open question 1, Story 4.2). A missing counter row throws a clear `InvalidOperationException` ("no counter for BNT/2027"). Automatic rollover is Epic 4.
  - [ ] Balance update port: Application `LuuKy/ISoDuLuuKyWriter.cs` with `Task<(decimal SoDuTruoc, decimal SoDuSau)?> CongAsync(int doiTuongId, decimal soTien, CancellationToken ct)`. Infrastructure implements it with `UPDATE DoiTuong WITH (UPDLOCK, ROWLOCK) SET SoDuLuuKy = SoDuLuuKy + @SoTien OUTPUT deleted.SoDuLuuKy, inserted.SoDuLuuKy WHERE Id = @Id AND TrangThai = 1`. Zero rows returns `null`, meaning the detainee doesn't exist or isn't managed. Only the receipt (add) direction is needed now. Epic 5 adds the subtract direction with `AND SoDuLuuKy >= @SoTien`.
  - [ ] Application `LuuKy/GhiSoLuuKyService.cs` with `GhiSoBienNhanThuAsync(GhiSoBienNhanThuRequest, ct)`, validated by FluentValidation. In **one transaction**:
    1. Begin the transaction (`IAppDbContext.BeginTransactionAsync`).
    2. Load the detainee for the snapshot (name, type) and fail with a business error if it isn't managed.
    3. `CapSoAsync("BNT", clock.Today.Year)`.
    4. `ISoDuLuuKyWriter.CongAsync`. A `null` result means business error, then roll back.
    5. Build `ChungTuLuuKy` via the Domain factory with `TrangThai = DaGhiSo`, `SoTienBangChu = SoTienBangChu.Doc(soTien)`, `SoDuTruoc`/`SoDuSau` from the OUTPUT.
    6. `SaveChangesAsync`. The Story 1.3 interceptor writes the `Them` audit row and joins the open transaction.
    7. Commit.
    It returns `(long Id, string SoChungTu, decimal SoDuSau)`. It throws/returns business errors and never shows UI.
  - [ ] **Guard rail for AC 3**: an architecture test asserts that `ISoDuLuuKyWriter` is used only by `GhiSoLuuKyService` (reflection over constructor parameters in Application). A second check confirms that the string `SoDuLuuKy =` (an update) appears only in the one Infrastructure writer file, via a source scan.
  - [ ] Unit tests (NSubstitute for `INumberingService`, `ISoDuLuuKyWriter`, `IClock`): the happy path builds the document with the right snapshot, words and balances; a `null` writer result produces a business error and no document is saved; validation failures stop before numbering.
- [ ] **T7. Minimal receipt form** (AC: 3)
  - [ ] WinForms `LuuKy/`: `IBienNhanThuView` + presenter + form. It has a detainee selector (a simple combo or MaSo lookup is enough; the real picker comes in Story 3.2), NghiepVu restricted to receipts (12 "Người thân gửi" for this flow; disable 13 per alignment A4), NguoiGuiHoTen, QuanHe, HinhThuc (cash/transfer, with the account required for transfer), SoTien (an integer textbox is fine; the real money control UX-DR1 comes in Epic 4), and NoiDung.
  - [ ] It shows the amount in words live as a preview (using `SoTienBangChu`). The Post button calls the service in a new scope. Show the business error message on failure (UX-DR10). The pre-posting confirmation dialog (UX-DR4) is Epic 4.
  - [ ] After a successful post, enable **Print**.
- [ ] **T8. Print the receipt** (AC: 4)
  - [ ] Implement the `IReportRenderer` shape agreed in Story 1.5 (Application `Abstractions/`). Application `BaoCao/` (or `LuuKy/`): `BienNhanThuModel` (unit header, `SoChungTu`, `NgayChungTu`, snapshot name/type, sender, `SoTien`, `SoTienBangChu`) and a query `LayBienNhanThuDeInQuery` that reads the **stored snapshot** columns plus `ThongTinDonVi`.
  - [ ] Infrastructure `Reports/BienNhanThuReport.cs` (QuestPDF, the embedded Vietnamese font from Story 1.5). The layout follows process A.I.5 loosely. The shared frame, signer configuration, print counting and reprint mark are **Epic 4** (FR42/FR43/FR27), so don't build them here.
  - [ ] Format money with `vi-VN` culture (`soTien.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))` gives `500.000`).
  - [ ] WinForms `Common/PdfPreviewForm` (WebView2, from the spike's findings): preview, print and save.
  - [ ] Dev convenience: `--seed-demo` (Story 1.2) fills `ThongTinDonVi` with demo values, so the printed header isn't blank on a dev machine.
- [ ] **T9. Integration test of the whole flow + reconciliation** (AC: 3, 5)
  - [ ] `IntegrationTests/LuuKy/WalkingSkeletonTests.cs` on the Story 1.2 LocalDB fixture, with a `FakeClock` at 2026-10-01 and a fake `ICurrentUser` = admin:
    - log in with the seeded admin;
    - add "Nguyễn Văn A";
    - post 500,000 cash from a relative;
    - assert `SoChungTu = "BNT-2026-00001"`, `TrangThai = 2`, `SoDuTruoc = 0`, `SoDuSau = 500000`, `SoTienBangChu = "Năm trăm nghìn đồng"`, the snapshot name, `DoiTuong.SoDuLuuKy = 500000`, and one `NhatKyThaoTac` row (`Them`, `ChungTuLuuKy`, the new Id) plus one `DangNhap` row.
  - [ ] Rollback test: force a failure after numbering (e.g. a substitute writer that returns `null`, or a detainee with `TrangThai = 2`). Assert that no document, no audit row, no balance change and **no consumed number** remain (the counter is back to 0), all inside the same transaction.
  - [ ] Render test: call `IReportRenderer` for the posted receipt and assert that the bytes start with `%PDF`. Optionally extract the text with **PdfPig** (`UglyToad.PdfPig`, Apache-2.0, test project only) and assert it contains the unit name, `BNT-2026-00001`, "Nguyễn Văn A", "500.000" and "Năm trăm nghìn đồng".
  - [ ] `IntegrationTests/Infrastructure/LedgerReconciliation.cs`: one SQL query returns every detainee where `SoDuLuuKy <> ISNULL(SUM(CASE LoaiPhieu WHEN 1 THEN SoTien ELSE -SoTien END) over posted (TrangThai = 2) documents, 0)`. `AssertBalanced()` must find zero rows. Call it at the end of **every** integration test that touches the ledger. A base class or fixture `DisposeAsync` hook is the simplest way (NEN-19, NFR11).
- [ ] **T10. Manual end-to-end check** (AC: 1–4)
  - [ ] Run `--migrate` against a fresh LocalDB, then `--seed-demo`, then start the app, log in as admin, add Nguyễn Văn A, post 500,000, print, and check the preview. Record a screenshot in the Completion Notes. That screenshot is the Sprint 0 gate evidence.

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
