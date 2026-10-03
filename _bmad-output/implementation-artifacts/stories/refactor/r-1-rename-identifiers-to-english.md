---
story: "R.1"
epic: refactor
title: Rename existing identifiers to English
status: review
size: L
backlogItems: []
frsCovered: []
dependsOn: ["1.4", "2.1", "2.2", "2.3"]
---

# Story R.1: Rename existing identifiers to English

Status: review

## Story

As a developer on the team,
I want every identifier in the codebase and database to be English, with only user-visible text in Vietnamese,
So that the code reads consistently and new stories don't have to mix two naming languages.

## Acceptance Criteria

1. **Given** the glossary in `docs/conventions/naming-conventions.md`
   **When** the refactor is finished
   **Then** every file, folder, namespace, type, member, parameter, variable, enum value and config key in `src/` and `tests/` follows the convention, and no Vietnamese word remains in an identifier
   **And** the product name `LuuKyCanTin` (solution, projects, root namespace) is unchanged

2. **Given** a database at the last migration before this story, with data in every table
   **When** `--migrate` applies the new migration `RenameIdentifiersToEnglish`
   **Then** tables, columns, indexes, keys and CHECK constraints are renamed in place (`RenameTable`, `RenameColumn`, `RenameIndex`), and every existing row survives unchanged
   **And** an integration test proves it by seeding data at the previous migration and reading it back after the upgrade

3. **Given** any screen, message or printed document
   **When** a user compares it with the build before this story
   **Then** every piece of visible Vietnamese text is identical, including the printed deposit receipt
   **And** enum display names come from `ToDisplayText()` extensions, never from the identifier

4. **Given** existing audit-log rows
   **When** the migration runs
   **Then** their stored table names and JSON property names are left as they were (the log is append-only)
   **And** the old → new name map is saved where story 2.10 (audit-log viewer) can use it to show historic rows

5. **Given** the finished change
   **When** `dotnet build` and `dotnet test` run
   **Then** both pass, including the architecture tests, the enum ↔ CHECK test and the schema-version check

6. **Given** `CLAUDE.md` and `docs/`
   **When** they mention a renamed identifier
   **Then** they use the new name

## Tasks / Subtasks

- [x] **T1. Rename map** (AC: 1, 4)
  - [x] List every Vietnamese identifier (types, members, enum values, folders, config keys, tables, columns) with its English name taken from the glossary. Add any missing term to the glossary first.
  - [x] Save the table/column part of the map as a static class in Application (e.g. `Administration/LegacyAuditNames.cs`) for AC 4, and the full map in the Dev Agent Record below.
- [x] **T2. Domain** (AC: 1)
  - [x] Rename entities, enums, value objects, interfaces and module folders (`LuuKy/` → `Custody/`, `HangHoa/` → `Inventory/`, `DanhMuc/` → `MasterData/`, `HeThong/` → `Administration/`, `BaoCao/` → `Reporting/`). Enum numeric values do not change.
- [x] **T3. Application** (AC: 1, 3)
  - [x] Rename services, requests, validators, DTOs, abstractions and exceptions. Message constants keep their Vietnamese values.
  - [x] Add a `<Enum>DisplayExtensions.ToDisplayText()` for every enum shown to users. Replace every place that displays an enum some other way.
- [x] **T4. Infrastructure and database** (AC: 2, 4)
  - [x] Rename EF configurations, stores, report templates and Serilog/config bindings.
  - [x] Generate the `RenameIdentifiersToEnglish` migration, then **edit it by hand** so every change is a rename, never a drop-and-create. Review the generated SQL (`dotnet ef migrations script`) before you commit.
  - [x] Write the upgrade integration test (`[SqlServerFact]`) described in AC 2.
- [x] **T5. WinForms** (AC: 1, 3)
  - [x] Rename forms, views, presenters, Designer files and control names. Don't change any `Text` property.
  - [x] Rename the `appsettings.json` keys and their options classes in the same commit.
- [x] **T6. Tests** (AC: 5)
  - [x] Rename test classes, files and methods to `<Method>_<Scenario>_<ExpectedResult>` in English.
- [x] **T7. Docs** (AC: 6)
  - [x] Update `CLAUDE.md` and `docs/`. Remove the "until R.1" interim section from `naming-conventions.md` and its pointer in `CLAUDE.md`.
- [x] **T8. Verify** (AC: 3, 5)
  - [x] Build and test the solution. Run the app on a migrated copy of a demo database, sign in, post and print one receipt, and compare the PDF with one from the previous build.

## Dev Notes

- Use the IDE's symbol rename (Rider or Visual Studio) rather than text search-and-replace, so references, `nameof`, XAML-free Designer code and test references move together. Rename one module per commit if the diff gets too large to review, but merge them all before any other story continues.
- This is a pure rename. Don't change behaviour, refactor logic or fix unrelated issues in the same change.
- Every workstation needs the admin to run `--migrate` after deploying this build, because the startup schema-version check refuses an older database.
- Seeded data codes (permission codes such as `HT.Xem`, voucher prefixes such as `BNT`) are data defined by the spec. Don't change them.
- Ready-for-dev stories (2.4–2.10 and later) still name identifiers in Vietnamese. Their developers translate them with the glossary; this story doesn't edit those files.

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (Amelia, bmad-agent-dev). The `bmad-build` workflow could not start (`uv` is not installed), so the user had the story implemented directly.

### Approach

No IDE was available, so the rename ran as a scripted, C#-aware token rewrite: code got the full map, comments got only compound names, and string literals kept their text except Designer control names and database names (every changed literal was logged and reviewed). Context-dependent names (enum values such as `TrangThaiChungTu.DaHuy` against the property `DaHuy`, and the permission-code constants) were renamed through qualified rules first, then finished by hand. The compiler, the full test suite and a byte-for-byte PDF comparison checked the result.

### Decisions and deviations

1. **Historic migrations stay as they are.** The six migrations before `RenameIdentifiersToEnglish` keep their class names (`AddNhatKyThaoTac`, `AddCanBo`, …) and the Vietnamese table and column strings in their bodies. Their IDs are stored in `__EFMigrationsHistory` and they rebuild the old schema step by step, so they are history, not identifiers to rename. None of them references a renamed type.
2. **Stored codes are data, not identifiers.** Permission codes (`HT.Xem`), role codes (`QUAN_TRI`), voucher prefixes (`BNT`), the receipt template code (`BIEN_NHAN_THU`) and the sign-in event codes in the audit payload (`DangNhap`, `DangNhapSai`, …) keep their values; only the constants holding them are English (`PermissionCodes.ActionView = "Xem"`, `SignInEvent.FailedSignIn = "DangNhapSai"`).
3. **The audit action keeps its stored codes (AC 4).** `AuditAction` members are English, but the `Action` column still stores `Them`/`Sua`/`Huy`/`In`/`Duyet`/`DangNhap` through `AuditActionCodes` (an EF value converter). Old and new rows agree, and nothing in the log is rewritten. `HasEnumCheck` now builds the CHECK from the property's converter, and the enum-check test (`EnumColumn.ToStoredText`) verifies those codes instead of the member names.
4. **New audit rows use the new names.** The interceptor writes the current table and column names, and the explicit payloads now use `Event` (was `SuKien`), `UserName` (was `TenDangNhap`) and `Permissions` (was `Quyen`). `LegacyAuditNames` (Application/Administration) maps every old table name, column or payload key, and JSON enum value name to its new name for story 2.10.
5. **The migration renames in place.** Tables, columns and indexes go through `RenameTable`/`RenameColumn`/`RenameIndex`, and primary and foreign keys through `sp_rename`. The CHECK constraints and the one filtered index are dropped and recreated, because their SQL text names columns; that touches no row. `Down` reverses everything. The scaffolded version had dropped and recreated every table.
6. **Display text comes from `ToDisplayText()` (AC 3).** `InmateType`, `PaymentMethod` and `TransactionType` are the enums shown to users, and each has an `<Enum>DisplayExtensions` in Application. The forms and the receipt template use them and produce the same text as before (`12 – Người thân gửi` is now `$"{(byte)v} – {v.ToDisplayText()}"`). An undefined value now throws instead of printing its number; the CHECK constraints make that unreachable.
7. **`User` is a T-SQL keyword.** EF quotes it; raw SQL in the tests writes `[User]`. This is now in the naming conventions.
8. **Small structural moves the rename forced:** `SignInStatus` moved out of `SignInResult.cs` into its own file (one type per file), `PermissionCodes.Ten(...)` became `BuildName(...)`, and `IMainView.Dong()` became `CloseShell()` so it doesn't clash with `Form.Close()`. Using directives were re-sorted after the folder renames. No behaviour changed.
9. **Config keys:** in `appsettings.json`, `"DangNhap": { "ThoiGianKhoaPhut" }` became `"SignIn": { "LockoutMinutes" }` and `"App": { "TieuDe" }` became `"App": { "Title" }`. `docs/install.md` tells admins who edited the file by hand what to change.
10. **Glossary additions:** `InCustody`, `AdmissionDate`/`ReleaseDate`/`BirthYear`, `Position`, `Leader`/`Accountant`/`Administrator`, `ParentAgencyName`/`Address`, `Sender`/`Relationship`, `SourceDocumentNumber`, `AccountNumber`, `Description`, `CancellationReason`/`PrintCount`, `Workstation`, `Prefix`, `CustodyIncrease`/`CustodyDecrease`, `ZeroTensStyle.Southern`/`Northern`, and the `WithoutDiacritics` suffix.

### Verification

- `dotnet build`: no errors. `dotnet ef migrations has-pending-model-changes`: none.
- `dotnet test`: 458 passed, 0 failed (Domain 72, Application 101, WinForms 63, Integration 222). That includes the architecture tests, the enum ↔ CHECK test and the schema-version checks. The baseline before the change was 436.
- AC 2: `RenameIdentifiersToEnglishMigrationTests` seeds a row in every table at `AddVaiTroQuyen` under the old names, upgrades, and reads every value back, including the untouched audit row (table name, JSON and stored code). It also migrates back down and finds every row under the old names.
- AC 3: the deposit receipt rendered from the same model before and after the change is **byte-identical** (cash and transfer variants). A diff of every string literal in `src` between `HEAD` and the change shows no Vietnamese text removed, apart from the labels that moved into the `ToDisplayText()` switches.
- T8: the app's `--migrate --seed-demo --environment Development` ran against a scratch database on the test server (all 7 migrations, 6 demo accounts, the audit rows), which was then dropped. Signing in and posting through the GUI was not exercised in this session.

### Rename map

#### Modules (folders and namespaces)

| Old | New |
|---|---|
| `LuuKy/` | `Custody/` |
| `HangHoa/` | `Inventory/` |
| `DanhMuc/` | `MasterData/` |
| `HeThong/` | `Administration/` |
| `BaoCao/` | `Reporting/` |

#### Tables

| Old | New |
|---|---|
| `CanBo` | `Officer` |
| `DoiTuong` | `Inmate` |
| `ChungTuLuuKy` | `CustodyVoucher` |
| `DemSoChungTu` | `VoucherCounter` |
| `NguoiDung` | `User` |
| `NguoiDungVaiTro` | `UserRole` |
| `NhatKyThaoTac` | `AuditLog` |
| `Quyen` | `Permission` |
| `ThongTinDonVi` | `FacilityInfo` |
| `VaiTro` | `Role` |
| `VaiTroQuyen` | `RolePermission` |

#### Columns (also the audit-log JSON property names)

| Old | New |
|---|---|
| `BanGhiId` | `RecordId` |
| `BuongGiam` | `Cell` |
| `ChucVu` | `Position` |
| `DangCongTac` | `IsActive` |
| `DangHoatDong` | `IsActive` |
| `DiaChi` | `Address` |
| `DoiTuongId` | `InmateId` |
| `DuLieuCu` | `OldValues` |
| `DuLieuMoi` | `NewValues` |
| `HanhDong` | `Action` |
| `HinhThuc` | `PaymentMethod` |
| `HoTen` | `FullName` |
| `HoTenDoiTuong` | `InmateFullName` |
| `KhoaDen` | `LockedUntil` |
| `LaQuanGiao` | `IsSupervisingOfficer` |
| `LoaiChungTu` | `VoucherTypeCode` |
| `LoaiDoiTuong` | `InmateType` |
| `LoaiPhieu` | `VoucherType` |
| `LyDoHuy` | `CancellationReason` |
| `Ma` | `Code` |
| `MaCanBo` | `OfficerCode` |
| `MaSo` | `InmateCode` |
| `MatKhauHash` | `PasswordHash` |
| `MayTram` | `Workstation` |
| `Nam` | `Year` |
| `NamSinh` | `BirthYear` |
| `NgayChungTu` | `VoucherDate` |
| `NgayHuy` | `CancelledAt` |
| `NgayNhan` | `ReceivedDate` |
| `NgayRa` | `ReleaseDate` |
| `NgaySua` | `ModifiedAt` |
| `NgayTao` | `CreatedAt` |
| `NgayVao` | `AdmissionDate` |
| `NghiepVu` | `TransactionType` |
| `NguoiDungId` | `UserId` |
| `NguoiGuiHoTen` | `SenderFullName` |
| `NguoiHuyId` | `CancelledById` |
| `NguoiSuaId` | `ModifiedById` |
| `NguoiTaoId` | `CreatedById` |
| `NoiDung` | `Description` |
| `PhaiDoiMatKhau` | `MustChangePassword` |
| `QuanHe` | `Relationship` |
| `QuyenId` | `PermissionId` |
| `SoChungTu` | `VoucherNumber` |
| `SoDuLuuKy` | `CustodyBalance` |
| `SoDuSau` | `BalanceAfter` |
| `SoDuTruoc` | `BalanceBefore` |
| `SoHienTai` | `CurrentNumber` |
| `SoLanIn` | `PrintCount` |
| `SoLanSai` | `FailedAttemptCount` |
| `SoPhieuGoc` | `SourceDocumentNumber` |
| `SoTaiKhoanNguoiGui` | `SenderAccountNumber` |
| `SoTien` | `Amount` |
| `SoTienBangChu` | `AmountInWords` |
| `Ten` | `Name` |
| `TenBang` | `TableName` |
| `TenCoQuanChuQuan` | `ParentAgencyName` |
| `TenDangNhap` | `UserName` |
| `TenDonVi` | `FacilityName` |
| `ThoiDiem` | `OccurredAt` |
| `TienTo` | `Prefix` |
| `TrangThai` | `Status` |
| `VaiTroId` | `RoleId` |

#### Qualified members (enum values, constants, nested classes)

| Old | New |
|---|---|
| `TrangThaiChungTu.Nhap` | `VoucherStatus.Draft` |
| `TrangThaiChungTu.DaGhiSo` | `VoucherStatus.Posted` |
| `TrangThaiChungTu.DaHuy` | `VoucherStatus.Cancelled` |
| `MauTrangThai.Nhap` | `SampleStatus.Draft` |
| `MauTrangThai.DaGhiSo` | `SampleStatus.Posted` |
| `MauTrangThai.DaHuy` | `SampleStatus.Cancelled` |
| `HanhDong.Them` | `AuditAction.Create` |
| `HanhDong.Sua` | `AuditAction.Update` |
| `HanhDong.Huy` | `AuditAction.Cancel` |
| `HanhDong.In` | `AuditAction.Print` |
| `HanhDong.Duyet` | `AuditAction.Approve` |
| `HanhDong.DangNhap` | `AuditAction.SignIn` |
| `LoaiPhieu.Thu` | `VoucherType.Receipt` |
| `LoaiPhieu.Chi` | `VoucherType.Payout` |
| `HinhThuc.TienMat` | `PaymentMethod.Cash` |
| `HinhThuc.ChuyenKhoan` | `PaymentMethod.BankTransfer` |
| `KieuDocLe.Le` | `ZeroTensStyle.Southern` |
| `KieuDocLe.Linh` | `ZeroTensStyle.Northern` |
| `TrangThaiDangNhap.ThanhCong` | `SignInStatus.Succeeded` |
| `TrangThaiDangNhap.PhaiDoiMatKhau` | `SignInStatus.PasswordChangeRequired` |
| `TrangThaiDangNhap.SaiThongTin` | `SignInStatus.InvalidCredentials` |
| `TrangThaiDangNhap.TaiKhoanBiKhoa` | `SignInStatus.AccountLocked` |
| `TrangThaiDangNhap.TaiKhoanNgungHoatDong` | `SignInStatus.AccountInactive` |
| `KetQuaDangNhap.DoiMatKhau` | `SignInResult.PasswordChangeRequired` |
| `SuKienDangNhap.DangNhapSai` | `SignInEvent.FailedSignIn` |
| `SuKienDangNhap.DangNhap` | `SignInEvent.SignIn` |
| `SuKienDangNhap.KhoaTaiKhoan` | `SignInEvent.AccountLocked` |
| `SuKienDangNhap.DangXuat` | `SignInEvent.SignOut` |
| `SuKienDangNhap.DoiMatKhau` | `SignInEvent.PasswordChanged` |
| `MaVaiTro.QuanTri` | `RoleCodes.Administrator` |
| `MaVaiTro.LuuKy` | `RoleCodes.CustodyOfficer` |
| `MaVaiTro.CanTin` | `RoleCodes.CanteenOfficer` |
| `MaVaiTro.QuanGiao` | `RoleCodes.SupervisingOfficer` |
| `MaVaiTro.LanhDao` | `RoleCodes.Leader` |
| `MaVaiTro.KeToan` | `RoleCodes.Accountant` |
| `MaQuyen.HT.` | `PermissionCodes.Administration.` |
| `MaQuyen.DM.` | `PermissionCodes.MasterData.` |
| `MaQuyen.LKT.` | `PermissionCodes.CustodyIncrease.` |
| `MaQuyen.LKC.` | `PermissionCodes.CustodyDecrease.` |
| `MaQuyen.LKBC.` | `PermissionCodes.CustodyReporting.` |
| `MaQuyen.NH.` | `PermissionCodes.GoodsReceipt.` |
| `MaQuyen.BH.` | `PermissionCodes.Sales.` |
| `MaQuyen.HHBC.` | `PermissionCodes.InventoryReporting.` |
| `KetQuaGhiSo.Loi` | `PostingResult.Fail` |
| `KetQuaThemDoiTuong.Loi` | `AddInmateResult.Fail` |
| `KetQuaDangNhap.Loi` | `SignInResult.Fail` |
| `SchemaVersionCheckResult.Tao` | `SchemaVersionCheckResult.Create` |
| `SchemaVersionCheckResult.TaoLoiKetNoi` | `SchemaVersionCheckResult.CreateConnectionFailed` |
| `XungDotDuLieuException.ThongBao` | `ConcurrencyConflictException.ConflictMessage` |
| `DemoSeedPolicy.KiemTra` | `DemoSeedPolicy.Decide` |
| `EnumCheckVerifier.KiemTra` | `EnumCheckVerifier.Verify` |
| `ProjectReferenceRules.KiemTra` | `ProjectReferenceRules.Check` |
| `ChinhSachMatKhau.KiemTra` | `PasswordPolicy.Validate` |
| `MaQuyen.<Module>.Xem/Them/Sua/Huy/In/Duyet` | `PermissionCodes.<Module>.View/Create/Update/Cancel/Print/Approve` |
| `HanhDong.Them/Sua/Huy/In/Duyet/DangNhap` (stored codes unchanged) | `AuditAction.Create/Update/Cancel/Print/Approve/SignIn` |

#### Types, members and constants

| Old | New |
|---|---|
| `ApDungMigrationAsync` | `MigrateAsync` |
| `BanGhiId` | `RecordId` |
| `BaoCao` | `Reporting` |
| `BaoLoiAsync` | `ReportFailureAsync` |
| `BatBuoc` | `IsForced` |
| `BatDau` | `Start` |
| `BayGio` | `Now` |
| `BienNhan` | `Receipt` |
| `BienNhanThuForm` | `DepositReceiptForm` |
| `BienNhanThuModel` | `DepositReceiptModel` |
| `BienNhanThuPresenter` | `DepositReceiptPresenter` |
| `BienNhanThuPresenterTests` | `DepositReceiptPresenterTests` |
| `BienNhanThuReport` | `DepositReceiptTemplate` |
| `BoDauNhay` | `StripQuotes` |
| `BoNhatKy` | `DetachAuditLogs` |
| `BuongGiam` | `Cell` |
| `CacHanhDong` | `Actions` |
| `CacModule` | `Modules` |
| `CaiDat` | `Install` |
| `CanBo` | `Officer` |
| `CanBoConfiguration` | `OfficerConfiguration` |
| `CanBoDangChon` | `SelectedOfficer` |
| `CanBoDto` | `OfficerDto` |
| `CanBoEditForm` | `OfficerEditForm` |
| `CanBoEditPresenter` | `OfficerEditPresenter` |
| `CanBoEditPresenterTests` | `OfficerEditPresenterTests` |
| `CanBoForm` | `OfficerForm` |
| `CanBoModelTests` | `OfficerModelTests` |
| `CanBoPresenter` | `OfficerPresenter` |
| `CanBoPresenterTests` | `OfficerPresenterTests` |
| `CanBoService` | `OfficerService` |
| `CanBoServiceTests` | `OfficerServiceTests` |
| `CanBoTests` | `OfficerTests` |
| `CapNhat` | `Update` |
| `CapNhatBangChu` | `UpdateAmountInWords` |
| `CapNhatQuyenAsync` | `UpdatePermissionsAsync` |
| `CapNhatSoDu` | `BalanceUpdate` |
| `CapNhatTaiKhoan` | `UpdateAccountNumberState` |
| `CapSoAsync` | `AllocateNumberAsync` |
| `CapSoRow` | `AllocatedNumberRow` |
| `CauHinhRieng` | `ConfigureEntity` |
| `ChapHanhXongAn` | `SentenceCompleted` |
| `ChayAsync` | `RunAsync` |
| `ChayLenhQuanTri` | `RunAdminCommand` |
| `ChayUngDung` | `RunApplication` |
| `ChinhSachMatKhau` | `PasswordPolicy` |
| `ChinhSachMatKhauTests` | `PasswordPolicyTests` |
| `ChoTien` | `GivenToOtherInmate` |
| `ChuanBi` | `Prepare` |
| `ChuanHoa` | `Normalize` |
| `ChucVu` | `Position` |
| `ChungTuLuuKy` | `CustodyVoucher` |
| `ChungTuLuuKyConfiguration` | `CustodyVoucherConfiguration` |
| `ChungTuLuuKyStore` | `CustodyVoucherStore` |
| `ChungTuLuuKyTests` | `CustodyVoucherTests` |
| `ChuyenKhoan` | `BankTransfer` |
| `ChuyenTrai` | `FacilityTransfer` |
| `ChuyenVeNguoiThan` | `ReturnedToRelative` |
| `CK_Test_TuChoiNhatKy` | `CK_Test_RejectAuditLog` |
| `CoApDungMigration` | `MustMigrate` |
| `CollationTimKiem` | `SearchCollation` |
| `CoNapDuLieuMau` | `MustSeedDemo` |
| `CongAsync` | `IncreaseAsync` |
| `CoNhatKyCanGhi` | `HasPendingChanges` |
| `CoSuKien` | `HasEvent` |
| `Cu` | `OldValues` |
| `DaCaiLocalDb` | `IsLocalDbInstalled` |
| `DaChapHanhXongAn` | `SentenceCompleted` |
| `DaChuyenTrai` | `FacilityTransferred` |
| `DaDangNhap` | `IsSignedIn` |
| `DaDangXuat` | `IsSignedOut` |
| `DaGhiSo` | `ShowPosted` (view method; the enum value is `VoucherStatus.Posted`) |
| `DaHuy` | `IsCancelled` (property; the enum value is `VoucherStatus.Cancelled`) |
| `DangBiKhoa` | `IsLocked` |
| `DangCo` | `Existing` |
| `DangCongTac` | `IsActive` |
| `DangHoatDong` | `IsActive` |
| `DangKy` | `Register` |
| `DangNhap` | `SignIn` |
| `DangNhapAdminAsync` | `SignInAdminAsync` |
| `DangNhapAsync` | `SignInAsync` |
| `DangNhapBam` | `SignInClicked` |
| `DangNhapOptions` | `SignInOptions` |
| `DangNhapService` | `SignInService` |
| `DangNhapServiceTests` | `SignInServiceTests` |
| `DangQuanLy` | `InCustody` |
| `DangXuat` | `SignOut` |
| `DangXuatAsync` | `SignOutAsync` |
| `DangXuatClicked` | `SignOutClicked` |
| `DanhMuc` | `MasterData` |
| `DanhMucCanBoClicked` | `OfficersClicked` |
| `DanhSachCotKhongGhi` | `BookkeepingColumns` |
| `DanhSachDoiTuong` | `Inmates` |
| `DemSoChungTu` | `VoucherCounter` |
| `DemSoChungTuConfiguration` | `VoucherCounterConfiguration` |
| `DemSoChungTuNumberingService` | `VoucherCounterNumberingService` |
| `DiaChi` | `Address` |
| `DienCotKiemToan` | `StampAuditColumns` |
| `DieuHuong` | `Navigator` |
| `DinhNghiaModule` | `ModuleDefinition` |
| `DinhNghiaQuyen` | `PermissionDefinition` |
| `DinhNghiaVaiTro` | `RoleDefinition` |
| `Doc` | `ToWords` |
| `DocJson` | `ReadJson` |
| `DoDaiChucVu` | `PositionMaxLength` |
| `DoDaiHash` | `HashLength` |
| `DoDaiHoTen` | `FullNameMaxLength` |
| `DoDaiMaCanBo` | `OfficerCodeMaxLength` |
| `DoDaiSalt` | `SaltLength` |
| `DoDaiToiThieu` | `MinLength` |
| `DoiMatKhau` | `ChangePassword` |
| `DoiMatKhauAsync` | `ChangePasswordAsync` |
| `DoiMatKhauClicked` | `ChangePasswordClicked` |
| `DoiMatKhauForm` | `ChangePasswordForm` |
| `DoiMatKhauPresenter` | `ChangePasswordPresenter` |
| `DoiMatKhauPresenterTests` | `ChangePasswordPresenterTests` |
| `DoiMatKhauService` | `ChangePasswordService` |
| `DoiMatKhauServiceTests` | `ChangePasswordServiceTests` |
| `DoiTuong` | `Inmate` |
| `DoiTuongChon` | `InmateOption` |
| `DoiTuongConfiguration` | `InmateConfiguration` |
| `DoiTuongId` | `InmateId` |
| `DoiTuongMau` | `SampleInmate` |
| `DoiTuongSoDuTests` | `InmateBalanceTests` |
| `DoiTuongStore` | `InmateStore` |
| `Dong` | `CloseShell` |
| `DongDaLuu` | `CloseAsSaved` |
| `DongVoiKetQua` | `CloseWithResult` |
| `DonVi` | `Unit` |
| `DuLieuCu` | `OldValues` |
| `DuLieuMoi` | `NewValues` |
| `DuocPhepChay` | `CanRun` |
| `DuocPhepGhiGiaTri` | `IsValueLoggable` |
| `DuocPhepMoAsync` | `CanOpenAsync` |
| `FillTaiKhoanDemoAsync` | `FillDemoAccountsAsync` |
| `GhiAsync` | `WriteAsync` |
| `GhiNhan` | `CaptureChange` |
| `GhiNhanAsync` | `RecordAsync` |
| `GhiNhanDangNhapDung` | `RecordSuccessfulSignIn` |
| `GhiNhanDangNhapSai` | `RecordFailedSignIn` |
| `GhiNhanDangNhapSaiService` | `FailedSignInService` |
| `GhiNhatKy` | `AuditLogWriter` |
| `GhiNhatKyTests` | `AuditLogWriterTests` |
| `GhiSoAsync` | `PostAsync` |
| `GhiSoBam` | `PostClicked` |
| `GhiSoBienNhanThuAsync` | `PostDepositReceiptAsync` |
| `GhiSoBienNhanThuRequest` | `PostDepositReceiptRequest` |
| `GhiSoBienNhanThuValidator` | `PostDepositReceiptRequestValidator` |
| `GhiSoLuuKyService` | `CustodyLedgerService` |
| `GhiSoLuuKyServiceTests` | `CustodyLedgerServiceTests` |
| `GiaoDichGia` | `FakeTransaction` |
| `GiaTri` | `Value` |
| `GiuNguyen` | `KeepOriginalValue` |
| `HangHoa` | `Inventory` |
| `HanhDong` | `AuditAction` (enum; the `AuditLog` property and column are `Action`) |
| `HanhDongDuyet` | `ActionApprove` |
| `HanhDongHuy` | `ActionCancel` |
| `HanhDongIn` | `ActionPrint` |
| `HanhDongNhatKy` | `AuditActionResolver` |
| `HanhDongNhatKyTests` | `AuditActionResolverTests` |
| `HanhDongSua` | `ActionUpdate` |
| `HanhDongThem` | `ActionCreate` |
| `HanhDongXem` | `ActionView` |
| `HashCu` | `OldHash` |
| `HashHopLe` | `ValidHash` |
| `HeThong` | `Administration` |
| `HienCaNguoiDaNghi` | `ShowInactive` |
| `HienDanhSach` | `ShowList` |
| `HienDanhSachVaiTro` | `ShowRoles` |
| `HienLoi` | `ShowError` |
| `HienQuyen` | `ShowPermissions` |
| `HienThi` | `DisplayText` |
| `HienThiBanIn` | `ShowPrintPreview` |
| `HienThongBao` | `ShowMessage` |
| `HienThongBaoLoi` | `ShowErrorMessage` |
| `HinhThuc` | `PaymentMethod` |
| `HinhThucItem` | `PaymentMethodItem` |
| `HoanTacTransaction` | `RollBackOwnTransaction` |
| `HoanTacTransactionAsync` | `RollBackOwnTransactionAsync` |
| `HoTen` | `FullName` |
| `HoTenDoiTuong` | `InmateFullName` |
| `HuyClicked` | `CancelClicked` |
| `HuyThayDoiClicked` | `DiscardClicked` |
| `IBienNhanThuView` | `IDepositReceiptView` |
| `ICanBoEditView` | `IOfficerEditView` |
| `ICanBoService` | `IOfficerService` |
| `ICanBoView` | `IOfficerView` |
| `IChungTuLuuKyStore` | `ICustodyVoucherStore` |
| `ICoTrangThaiHuy` | `ICancellable` |
| `IDieuHuong` | `INavigator` |
| `IDoiMatKhauView` | `IChangePasswordView` |
| `IDoiTuongStore` | `IInmateStore` |
| `IGhiNhatKy` | `IAuditLogWriter` |
| `IKiemTraQuyen` | `IPermissionChecker` |
| `IMatKhauHasher` | `IPasswordHasher` |
| `InAsync` | `PrintAsync` |
| `InBam` | `PrintClicked` |
| `INguoiDungStore` | `IUserStore` |
| `ISoDuLuuKyWriter` | `ICustodyBalanceWriter` |
| `IThemDoiTuongView` | `IAddInmateView` |
| `IThongTinDonViStore` | `IFacilityInfoStore` |
| `IVaiTroService` | `IRoleService` |
| `IVaiTroView` | `IRoleView` |
| `KetQuaDangNhap` | `SignInResult` |
| `KetQuaGhiSo` | `PostingResult` |
| `KetQuaThemDoiTuong` | `AddInmateResult` |
| `KetThuc` | `Reset` |
| `KhoaDen` | `LockedUntil` |
| `KhongCoNguoiDung` | `NoUser` |
| `KhongCoQuyenException` | `PermissionDeniedException` |
| `KhongGhiNhatKy` | `NotAudited` |
| `KhongGhiNhatKyAttribute` | `NotAuditedAttribute` |
| `KhongKhop` | `Mismatched` |
| `Khop` | `Matching` |
| `KiemTra` | `Validate` (`PasswordPolicy`); `Decide`, `Verify`, `Check` elsewhere, see the qualified table |
| `KiemTraAsync` | `CheckAsync` |
| `KiemTraHopLeAsync` | `ValidateAsync` |
| `KiemTraQuyen` | `PermissionChecker` |
| `KiemTraTrungMaAsync` | `EnsureCodeIsUniqueAsync` |
| `KieuDocLe` | `ZeroTensStyle` |
| `LaChiAnalyzer` | `IsAnalyzerOnly` |
| `LaGiongNhau` | `AreSame` |
| `LaKhop` | `IsMatch` |
| `LaLenhQuanTri` | `IsAdminCommand` |
| `LaLuuTheoTen` | `IsStoredAsText` |
| `LaPhienBanCoSoDuLieuKhop` | `IsDatabaseVersionCurrent` |
| `LaQuanGiao` | `IsSupervisingOfficer` |
| `LaQuyenDacBiet` | `IsSpecial` |
| `LaViPhamDuyNhat` | `IsUniqueViolation` |
| `LayAsync` | `GetAsync` |
| `LayAuditInterceptor` | `GetAuditInterceptor` |
| `LayBienNhanThuDeInQuery` | `DepositReceiptPrintQuery` |
| `LayCanBoDangCongTacAsync` | `GetActiveOfficersAsync` |
| `LayCheckConstraintAsync` | `GetCheckConstraintsAsync` |
| `LayCotEnum` | `GetEnumColumns` |
| `LayDangNhapOptions` | `BindSignInOptions` |
| `LayDangQuanLyAsync` | `GetInCustodyAsync` |
| `LayDanhSachAsync` | `GetAllAsync` |
| `LayDesignTimeModel` | `GetDesignTimeModel` |
| `LayDich` | `GetTarget` |
| `LayDoiTuongDangQuanLyQuery` | `InmatesInCustodyQuery` |
| `LayEntityType` | `GetEntityType` |
| `LayGiaTriAsync` | `GetScalarAsync` |
| `LayGiaTriChoPhep` | `GetAllowedValues` |
| `LayKhoa` | `GetKey` |
| `LayMayChuDaCauHinh` | `GetConfiguredServer` |
| `LayNhatKyAsync` | `GetAuditLogsAsync` |
| `LayPhanTu` | `GetElements` |
| `LayQuyenCuaVaiTroAsync` | `GetPermissionsAsync` |
| `LaySoDongNhatKyAsync` | `CountAuditLogsAsync` |
| `LayThongBaoChan` | `GetBlockingMessage` |
| `LayThongBaoTuChoi` | `GetRefusalMessage` |
| `LayThuocTinh` | `GetProperty` |
| `LayThuocTinhEnum` | `GetEnumProperties` |
| `LoaiChungTu` | `VoucherTypeCode` |
| `LoaiDoiTuong` | `InmateType` |
| `LoaiItem` | `InmateTypeItem` |
| `LoaiPhieu` | `VoucherType` |
| `LogCuaTaiKhoanAsync` | `GetAccountAuditLogsAsync` |
| `Loi` | `Fail` |
| `LoiChuaDangNhap` | `NotSignedInMessage` |
| `LoiKetNoi` | `ConnectionFailure` |
| `LoiKhiLuu` | `SaveFailure` |
| `LoiKhongKetNoiDuoc` | `ConnectionFailedMessage` |
| `LoiKhongMongMuon` | `UnexpectedErrorMessage` |
| `LoiKhongTimThay` | `NotFoundMessage` |
| `LoiKhongTimThayTaiKhoan` | `AccountNotFoundMessage` |
| `LoiKhongTimThayVaiTro` | `RoleNotFoundMessage` |
| `LoiMaQuyenKhongHopLe` | `InvalidPermissionCodeMessage` |
| `LoiNghiepVuException` | `BusinessRuleException` |
| `LoiPhienBanKhongKhop` | `VersionMismatchMessage` |
| `LoiQuaNgan` | `TooShortMessage` |
| `LoiTaiKhoanBiKhoa` | `AccountLockedMessage` |
| `LoiThieuChuHoa` | `MissingUpperCaseMessage` |
| `LoiThieuChuSo` | `MissingDigitMessage` |
| `LoiThieuChuThuong` | `MissingLowerCaseMessage` |
| `LoiTrungKhoa` | `DuplicateKeyErrors` |
| `LoiTrungMa` | `DuplicateCodeMessage` |
| `LoiTrungMatKhauCu` | `SameAsCurrentMessage` |
| `LoiXacNhanKhongKhop` | `ConfirmationMismatchMessage` |
| `LuuAsync` | `SaveAsync` |
| `LuuBam` | `SaveClicked` |
| `LuuCanBoRequest` | `SaveOfficerRequest` |
| `LuuCanBoRequestValidator` | `SaveOfficerRequestValidator` |
| `LuuCanBoRequestValidatorTests` | `SaveOfficerRequestValidatorTests` |
| `LuuClicked` | `SaveClicked` |
| `LuuKy` | `Custody` |
| `LyDoHuy` | `CancellationReason` |
| `Ma` | `Code` |
| `MaBiMat` | `Secret` |
| `MaCanBo` | `OfficerCode` |
| `MaLoaiChungTu` | `ReceiptVoucherTypeCode` |
| `MaMau` | `PrintTemplate` |
| `MaMauIn` | `TemplateCode` |
| `MangTheoKhiVao` | `BroughtOnAdmission` |
| `MaQuyen` | `PermissionCodes` (class); `PermissionDeniedException.PermissionCode` (property) |
| `MaQuyenTests` | `PermissionCodesTests` |
| `MaSo` | `InmateCode` |
| `MaSoDaTonTai` | `DuplicateCodeMessage` |
| `MaSoDaTonTaiAsync` | `CodeExistsAsync` |
| `MatKhau` | `Password` |
| `MatKhauAdmin` | `AdminPassword` |
| `MatKhauBanDau` | `InitialPassword` |
| `MatKhauDemo` | `DemoPassword` |
| `MatKhauGiaHash` | `DummyPasswordHash` |
| `MatKhauHash` | `PasswordHash` |
| `MatKhauHienTai` | `CurrentPassword` |
| `MatKhauMoi` | `NewPassword` |
| `MauChungTu` | `SampleVoucher` |
| `MauChungTuConfiguration` | `SampleVoucherConfiguration` |
| `MauTrangThai` | `SampleStatus` |
| `MauTrangThaiThieu` | `SampleStatusMissingValue` |
| `MauTrangThaiThua` | `SampleStatusExtraValue` |
| `MaVaiTro` | `RoleCodes` |
| `MayTram` | `Workstation` |
| `MoDangNhap` | `ShowLogin` |
| `MoDanhMucCanBo` | `OpenOfficers` |
| `MoDoiMatKhau` | `OpenChangePassword` |
| `MoDoiMatKhauBatBuoc` | `ShowForcedPasswordChange` |
| `ModuleBh` | `ModuleSales` |
| `ModuleDm` | `ModuleMasterData` |
| `ModuleHhbc` | `ModuleInventoryReporting` |
| `ModuleHt` | `ModuleAdministration` |
| `ModuleLkbc` | `ModuleCustodyReporting` |
| `ModuleLkc` | `ModuleCustodyDecrease` |
| `ModuleLkt` | `ModuleCustodyIncrease` |
| `ModuleNh` | `ModuleGoodsReceipt` |
| `MoHopThoaiAsync` | `OpenDialogAsync` |
| `Moi` | `NewValues` |
| `MoKetNoiKhongPoolAsync` | `OpenUnpooledConnectionAsync` |
| `MoMoShell` | `ShowShell` |
| `MoVaiTro` | `OpenRoles` |
| `MuaHang` | `CanteenPurchase` |
| `MuoiLamPhut` | `FifteenMinutes` |
| `Nam` | `Year` |
| `NamSinh` | `BirthYear` |
| `Nap` | `Load` |
| `NapDuLieuMauAsync` | `SeedAsync` |
| `NgayChu` | `FormatDate` |
| `NgayChungTu` | `VoucherDate` |
| `NgayHuy` | `CancelledAt` |
| `NgayNhan` | `ReceivedDate` |
| `NgayRa` | `ReleaseDate` |
| `NgaySua` | `ModifiedAt` |
| `NgayTao` | `CreatedAt` |
| `NgayTaoSeed` | `SeedCreatedAt` |
| `NgayVao` | `AdmissionDate` |
| `NghiepVu` | `TransactionType` |
| `NghiepVuItem` | `TransactionTypeItem` |
| `NguoiDung` | `User` |
| `NguoiDungConfiguration` | `UserConfiguration` |
| `NguoiDungDangNhapAsync` | `SignInUserAsync` |
| `NguoiDungId` | `UserId` |
| `NguoiDungStore` | `UserStore` |
| `NguoiDungTests` | `UserTests` |
| `NguoiDungVaiTro` | `UserRole` |
| `NguoiDungVaiTroConfiguration` | `UserRoleConfiguration` |
| `NguoiGuiHoTen` | `SenderFullName` |
| `NguoiHuyId` | `CancelledById` |
| `NguoiSuaId` | `ModifiedById` |
| `NguoiTaoId` | `CreatedById` |
| `NguoiThanGui` | `SentByRelative` |
| `NhanTuDoiTuongKhac` | `ReceivedFromOtherInmate` |
| `NhatKyCuaVaiTroAsync` | `GetRoleAuditLogsAsync` |
| `NhatKyFactory` | `AuditLogFactory` |
| `NhatKyThaoTac` | `AuditLog` |
| `NhatKyThaoTacConfiguration` | `AuditLogConfiguration` |
| `NhatKyThaoTacConfigurationTests` | `AuditLogConfigurationTests` |
| `NoiDung` | `Description` |
| `OnDangNhapBam` | `OnSignInClicked` |
| `OnDangXuatClicked` | `OnSignOutClicked` |
| `OnGhiSoBam` | `OnPostClicked` |
| `OnHinhThucThayDoi` | `OnPaymentMethodChanged` |
| `OnHuyClicked` | `OnCancelClicked` |
| `OnHuyThayDoiClicked` | `OnDiscardClicked` |
| `OnInBam` | `OnPrintClicked` |
| `OnLapBienNhanThu` | `OnCreateDepositReceipt` |
| `OnLuuBam` | `OnSaveClicked` |
| `OnLuuClicked` | `OnSaveClicked` |
| `OnSoTienThayDoi` | `OnAmountChanged` |
| `OnThemDoiTuong` | `OnAddInmate` |
| `Pbkdf2MatKhauHasher` | `Pbkdf2PasswordHasher` |
| `Pbkdf2MatKhauHasherTests` | `Pbkdf2PasswordHasherTests` |
| `PhaiDoiMatKhau` | `MustChangePassword` |
| `PhamNhan` | `Prisoner` |
| `PhanTich` | `Parse` |
| `Phien` | `Session` |
| `PhieuGuiQua` | `GiftSlip` |
| `QuanHe` | `Relationship` |
| `Quyen` | `Permission` |
| `QuyenConfiguration` | `PermissionConfiguration` |
| `QuyenCuaModule` | `ForModule` |
| `QuyenCuaVaiTroAsync` | `GetRolePermissionsAsync` |
| `QuyenDaChon` | `SelectedPermissions` |
| `QuyenId` | `PermissionId` |
| `SaiThongTin` | `InvalidCredentialsMessage` |
| `SangDto` | `ToDto` |
| `SangJson` | `ToJson` |
| `SapXep` | `SortForDisplay` |
| `ScopeFactoryGia` | `FakeScopeFactory` |
| `SeedMotVaiTro` | `SeedOneRole` |
| `SeedNguoiDung` | `SeedUser` |
| `SeedTaiKhoan` | `SeedAccount` |
| `SoChungTu` | `VoucherNumber` |
| `SoDuLuuKy` | `CustodyBalance` |
| `SoDuLuuKyWriter` | `CustodyBalanceWriter` |
| `SoDuLuuKyWriterUsageTests` | `CustodyBalanceWriterUsageTests` |
| `SoDuRow` | `BalanceRow` |
| `SoDuSau` | `BalanceAfter` |
| `SoDuTruoc` | `BalanceBefore` |
| `SoHienTai` | `CurrentNumber` |
| `SoLanIn` | `PrintCount` |
| `SoLanLuu` | `SaveCount` |
| `SoLanSai` | `FailedAttemptCount` |
| `SoLanSaiToiDa` | `MaxFailedAttempts` |
| `SoPhieuGoc` | `SourceDocumentNumber` |
| `SoQuyenChuan` | `StandardPermissionCount` |
| `SoTaiKhoanNguoiGui` | `SenderAccountNumber` |
| `SoTien` | `Amount` |
| `SoTienBangChu` | `AmountInWords` |
| `SoTienBangChuTests` | `AmountInWordsTests` |
| `SoTienThayDoi` | `AmountChanged` |
| `SoVongLap` | `Iterations` |
| `SoVongLapToiDa` | `MaxIterations` |
| `SuaAsync` | `UpdateAsync` |
| `SuaClicked` | `EditClicked` |
| `SuKien` | `Event` |
| `SuKienDangNhap` | `SignInEvent` |
| `Tai` | `LoadRequested` |
| `TaiAsync` | `LoadAsync` |
| `TaiDanhSachAsync` | `LoadRolesAsync` |
| `TaiKhoan` | `Account` |
| `TaiKhoanBiKhoa` | `AccountLocked` |
| `TaiKhoanDemo` | `DemoAccount` |
| `TaiKhoanMacDinh` | `DemoAccounts` |
| `TaiKhoanNgungHoatDong` | `AccountInactive` |
| `TaiLaiAsync` | `ReloadAsync` |
| `TaiQuyenAsync` | `LoadPermissionsAsync` |
| `TaiXongAsync` | `WaitForLoadAsync` |
| `TamGiuTamGiam` | `PreTrialDetainee` |
| `Tao` | `Create` |
| `TaoBienNhanThuDaGhiSo` | `CreatePostedDepositReceipt` |
| `TaoCheck` | `CreateCheck` |
| `TaoChungTu` | `CreateVoucher` |
| `TaoCot` | `CreateColumn` |
| `TaoCsproj` | `CreateCsproj` |
| `TaoDangNhapService` | `CreateSignInService` |
| `TaoDbContext` | `CreateDbContext` |
| `TaoDbContextCoNhatKy` | `CreateAuditedDbContext` |
| `TaoDoiMatKhauService` | `CreateChangePasswordService` |
| `TaoHost` | `CreateHost` |
| `TaoKiemTraQuyen` | `CreatePermissionChecker` |
| `TaoLogger` | `CreateLogger` |
| `TaoLoiKetNoi` | `CreateConnectionFailed` |
| `TaoLuoi` | `BuildGrid` |
| `TaoNguoiDungAsync` | `CreateUserAsync` |
| `TaoPresenter` | `CreatePresenter` |
| `TaoProvider` | `CreateProvider` |
| `TaoRongAsync` | `CreateEmptyAsync` |
| `TaoSeeder` | `CreateSeeder` |
| `TaoService` | `CreateService` |
| `TaoTaiKhoanAsync` | `CreateAccountAsync` |
| `TatCa` | `All` |
| `Ten` | `Name` |
| `TenBang` | `TableName` |
| `TenCoQuanChuQuan` | `ParentAgencyName` |
| `TenCoSoDuLieuXacNhan` | `ConfirmedDatabaseName` |
| `TenDangNhap` | `UserName` |
| `TenDonVi` | `FacilityName` |
| `TenHinhThuc` | `PaymentMethodText` |
| `TenLoaiDoiTuong` | `InmateTypeText` |
| `ThanhCong` | `Succeeded` |
| `ThanhDto` | `ToDtoExpression` |
| `ThayDoi` | `PendingChange` |
| `Them` | `Add` (`ICustodyVoucherStore.Add`) |
| `ThemAsync` | `AddAsync` |
| `ThemClicked` | `AddClicked` |
| `ThemDauNhay` | `Quote` |
| `ThemDoiTuongAsync` | `AddInmateAsync` |
| `ThemDoiTuongForm` | `AddInmateForm` |
| `ThemDoiTuongPresenter` | `AddInmatePresenter` |
| `ThemDoiTuongPresenterTests` | `AddInmatePresenterTests` |
| `ThemDoiTuongRequest` | `AddInmateRequest` |
| `ThemDoiTuongService` | `AddInmateService` |
| `ThemDoiTuongServiceTests` | `AddInmateServiceTests` |
| `ThemDoiTuongValidator` | `AddInmateRequestValidator` |
| `ThemNhatKy` | `AddAuditLogs` |
| `ThemVaLuuAsync` | `AddAndSaveAsync` |
| `ThoiDiem` | `OccurredAt` |
| `ThoiDiemIn` | `PrintedAt` |
| `ThoiGianKhoa` | `LockoutPeriod` |
| `ThoiGianKhoaPhut` | `LockoutMinutes` |
| `ThongBao` | `Message` (result records); `ConcurrencyConflictException.ConflictMessage` (constant) |
| `ThongTinDonVi` | `FacilityInfo` |
| `ThongTinDonViConfiguration` | `FacilityInfoConfiguration` |
| `ThongTinDonViStore` | `FacilityInfoStore` |
| `ThucThiAsync` | `ExecuteAsync` |
| `ThuGan` | `TryAttach` |
| `TienMat` | `Cash` |
| `TienTo` | `Prefix` |
| `TieuDe` | `Title` |
| `TieuDeLoi` | `ErrorTitle` |
| `TimAsync` | `SearchAsync` |
| `TimKiemThayDoi` | `SearchChanged` |
| `TimTheoDangNhapAsync` | `FindByUserNameAsync` |
| `TimTheoIdAsync` | `FindByIdAsync` |
| `TimThuMucGoc` | `FindRoot` |
| `TrangThai` | `Status` |
| `TrangThaiChungTu` | `VoucherStatus` |
| `TrangThaiDangNhap` | `SignInStatus` |
| `TrangThaiDoiTuong` | `InmateStatus` |
| `TrangThaiTheoTen` | `StatusAsText` |
| `TrangThaiTruoc` | `PreviousStatus` |
| `TrungGiaTriDuyNhatException` | `UniqueConstraintException` |
| `TuKhoa` | `Keyword` |
| `VaiTro` | `Role` |
| `VaiTroClicked` | `RolesClicked` |
| `VaiTroConfiguration` | `RoleConfiguration` |
| `VaiTroDangChon` | `SelectedRoleId` |
| `VaiTroDto` | `RoleDto` |
| `VaiTroForm` | `RoleForm` |
| `VaiTroId` | `RoleId` |
| `VaiTroMacDinh` | `DefaultRoles` |
| `VaiTroPresenter` | `RolePresenter` |
| `VaiTroPresenterTests` | `RolePresenterTests` |
| `VaiTroQuyen` | `RolePermission` |
| `VaiTroQuyenConfiguration` | `RolePermissionConfiguration` |
| `VaiTroService` | `RoleService` |
| `VaiTroServiceTests` | `RoleServiceTests` |
| `VaiTroThayDoi` | `RoleChanged` |
| `Vi` | `VietnameseCulture` |
| `VoThoiHan` | `NoExpiry` |
| `XacDinh` | `Resolve` |
| `XacNhan` | `Confirmation` |
| `XoaMatKhau` | `ClearPassword` |
| `XungDotDuLieuException` | `ConcurrencyConflictException` |
| `YeuCauAsync` | `RequireAsync` |

#### Parameters, locals and fields

| Old | New |
|---|---|
| `_batBuoc` | `_isForced` |
| `_canBo` | `_officer` |
| `_chungTuId` | `_voucherId` |
| `_chungTuStore` | `_voucherStore` |
| `_daGhiSo` | `_isPosted` |
| `_dangGhiNhatKy` | `_isWritingAuditLogs` |
| `_dangLuu` | `_isSaving` |
| `_dangXuat` | `_isSignedOut` |
| `_danhSach` | `_roles` |
| `_danhSachThayDoi` | `_pendingChanges` |
| `_danhSachThongBao` | `_messages` |
| `_dieuHuong` | `_navigator` |
| `_doiTuongStore` | `_inmateStore` |
| `_donViStore` | `_facilityInfoStore` |
| `_ghiNhatKy` | `_auditLog` |
| `_giaoDich` | `_transaction` |
| `_hanhDong` | `_actions` |
| `_lanTim` | `_searchVersion` |
| `_nguoiDung` | `_user` |
| `_phien` | `_session` |
| `_quyenDacBiet` | `_specialPermissions` |
| `_rowVerDangChon` | `_selectedRowVer` |
| `_soDuWriter` | `_balanceWriter` |
| `_taoHopThoai` | `_createDialog` |
| `_tenTep` | `_fileName` |
| `_transactionRieng` | `_ownTransaction` |
| `_vaiTro` | `_roleForm` |
| `banGhiId` | `recordId` |
| `baoGomDuyet` | `includeApprove` |
| `baoGomNgungCongTac` | `includeInactive` |
| `batBuoc` | `isForced` |
| `btnDangNhap` | `btnSignIn` |
| `btnDong` | `btnClose` |
| `btnGhiSo` | `btnPost` |
| `btnHuy` | `btnCancel` |
| `btnLuu` | `btnSave` |
| `btnSua` | `btnEdit` |
| `btnThem` | `btnAdd` |
| `canBo` | `officer` |
| `canTin` | `canteen` |
| `cham` | `slow` |
| `chiQuanGiao` | `supervisingOnly` |
| `chkDangCongTac` | `chkIsActive` |
| `chkHienCaNguoiDaNghi` | `chkShowInactive` |
| `chkLaQuanGiao` | `chkIsSupervisingOfficer` |
| `chon` | `option` |
| `chucDanh` | `title` |
| `chucVu` | `position` |
| `chungTu` | `voucher` |
| `chungTuDaLuu` | `savedVoucher` |
| `chungTuId` | `voucherId` |
| `chungTuSai` | `invalidVoucher` |
| `chungTuStore` | `voucherStore` |
| `chungTuTachRoi` | `detachedVoucher` |
| `chuSo` | `text` |
| `chuyenKhoan` | `isBankTransfer` |
| `cmbDoiTuong` | `cmbInmate` |
| `cmbHinhThuc` | `cmbPaymentMethod` |
| `cmbLoaiDoiTuong` | `cmbInmateType` |
| `cmbNghiepVu` | `cmbTransactionType` |
| `coApDungMigration` | `mustMigrate` |
| `colChucVu` | `colPosition` |
| `colDangCongTac` | `colIsActive` |
| `colHoTen` | `colFullName` |
| `colLaQuanGiao` | `colIsSupervisingOfficer` |
| `colMaCanBo` | `colOfficerCode` |
| `coNapDuLieuMau` | `mustSeedDemo` |
| `coQuyen` | `hasPermission` |
| `cu` | `oldValues` |
| `daApDung` | `applied` |
| `daBao` | `errorShown` |
| `daDangXuat` | `isSignedOut` |
| `daDong` | `closed` |
| `daHuySau` | `isCancelledAfter` |
| `daHuyTruoc` | `isCancelledBefore` |
| `daLuu` | `saved` |
| `dangChon` | `selectedId` |
| `dangCongTac` | `isActive` |
| `daNghi` | `formerOfficer` |
| `dangNhap` | `signIn` |
| `danhSach` | `items` |
| `danhSachChuaApDung` | `pending` |
| `danhSachCotDanhDau` | `flaggedColumns` |
| `danhSachCotDuocGhi` | `loggableColumns` |
| `danhSachCotThayDoi` | `changedColumns` |
| `danhSachDaApDung` | `applied` |
| `danhSachDong` | `lines` |
| `danhSachId` | `ids` |
| `danhSachMigrationCuaBan` | `buildMigrations` |
| `danhSachMigrationDaApDung` | `appliedMigrations` |
| `danhSachNhatKy` | `auditLogs` |
| `danhSachTruongHop` | `cases` |
| `daTai` | `loaded` |
| `daThem` | `added` |
| `dauHieu` | `marker` |
| `dbKiemTra` | `check` |
| `dich` | `target` |
| `dichVu` | `services` |
| `dichVu1` | `service` |
| `dieuHuong` | `navigator` |
| `doiMatKhau` | `changePassword` |
| `doiTuong` | `inmate` |
| `doiTuongId` | `inmateId` |
| `doiTuongStore` | `inmateStore` |
| `dong` | `rows` |
| `donVi` | `facility` |
| `dtpNgayChungTu` | `dtpVoucherDate` |
| `dtpNgayVao` | `dtpAdmissionDate` |
| `duLieuCu` | `oldValues` |
| `duLieuMoi` | `newValues` |
| `duongDan` | `path` |
| `ghiNhanSai` | `failedSignIns` |
| `ghiNhatKy` | `auditLog` |
| `ghiSo` | `ledger` |
| `giaoDich` | `transaction` |
| `giaoDien` | `contract` |
| `giaTri` | `value` |
| `giua` | `middle` |
| `gridCanBo` | `grdOfficers` |
| `gridQuyen` | `grdPermissions` |
| `hangSo` | `constants` |
| `hanhDong` | `action` |
| `hashMoi` | `newHash` |
| `hienLoi` | `showError` |
| `hienTai` | `current` |
| `hinhThuc` | `paymentMethod` |
| `homNay` | `today` |
| `hopThoai` | `dialog` |
| `hoTen` | `fullName` |
| `idDangSua` | `editedId` |
| `idTheoMa` | `idByCode` |
| `keToan` | `accountant` |
| `ketQua` | `result` |
| `ketQuaCuoi` | `lastResult` |
| `khoa` | `key` |
| `khoaDen` | `lockedUntil` |
| `khoangThoiGian` | `interval` |
| `kiemTraQuyen` | `permissionChecker` |
| `kieu` | `style` |
| `laKhop` | `isMatch` |
| `laMoiTruongPhatTrien` | `isDevelopment` |
| `lan` | `attempt` |
| `lanhDao` | `leader` |
| `lanNay` | `version` |
| `laQuanGiao` | `isSupervisingOfficer` |
| `layDeIn` | `printQuery` |
| `lblBangChu` | `lblAmountInWords` |
| `lblBuongGiam` | `lblCell` |
| `lblChucVu` | `lblPosition` |
| `lblDoiTuong` | `lblInmate` |
| `lblHinhThuc` | `lblPaymentMethod` |
| `lblHoTen` | `lblFullName` |
| `lblHuongDan` | `lblHint` |
| `lblLoaiDoiTuong` | `lblInmateType` |
| `lblLoi` | `lblError` |
| `lblMaCanBo` | `lblOfficerCode` |
| `lblMaSo` | `lblInmateCode` |
| `lblMatKhau` | `lblPassword` |
| `lblMatKhauHienTai` | `lblCurrentPassword` |
| `lblMatKhauMoi` | `lblNewPassword` |
| `lblNamSinh` | `lblBirthYear` |
| `lblNgayChungTu` | `lblVoucherDate` |
| `lblNgayVao` | `lblAdmissionDate` |
| `lblNghiepVu` | `lblTransactionType` |
| `lblNguoiGui` | `lblSender` |
| `lblNoiDung` | `lblDescription` |
| `lblQuanHe` | `lblRelationship` |
| `lblQuyenDacBiet` | `lblSpecialPermissions` |
| `lblSoTaiKhoan` | `lblAccountNumber` |
| `lblSoTien` | `lblAmount` |
| `lblTenDangNhap` | `lblUserName` |
| `lblThongBao` | `lblMessage` |
| `lblTieuDe` | `lblTitle` |
| `lblTrangThai` | `lblStatus` |
| `lblTuKhoa` | `lblKeyword` |
| `lblXacNhan` | `lblConfirmation` |
| `lech` | `unbalanced` |
| `lenh` | `command` |
| `loai` | `type` |
| `loaiChungTu` | `voucherTypeCode` |
| `loaiPhieu` | `voucherType` |
| `logTruoc` | `logsBefore` |
| `loi` | `error` |
| `lopModule` | `moduleClass` |
| `lstQuyenDacBiet` | `lstSpecialPermissions` |
| `lstVaiTro` | `lstRoles` |
| `luuKy` | `custody` |
| `ma` | `code` |
| `maCanBo` | `officerCode` |
| `maCu` | `oldCodes` |
| `maCuaModule` | `moduleCodes` |
| `maKhongHopLe` | `invalidCodes` |
| `maMoi` | `newCodes` |
| `maQuyen` | `permissionCode` |
| `maSo` | `inmateCode` |
| `maTheoId` | `codeById` |
| `matKhau` | `password` |
| `matKhauHasher` | `passwordHasher` |
| `matKhauHienTai` | `currentPassword` |
| `matKhauMoi` | `newPassword` |
| `maTrongDanhMuc` | `catalogueCodes` |
| `maVaiTro` | `roleCode` |
| `mnuCanBo` | `mnuOfficers` |
| `mnuDangXuat` | `mnuSignOut` |
| `mnuDanhMuc` | `mnuMasterData` |
| `mnuDoiMatKhau` | `mnuChangePassword` |
| `mnuHeThong` | `mnuAdministration` |
| `mnuLapBienNhanThu` | `mnuDepositReceipt` |
| `mnuLuuKy` | `mnuCustody` |
| `mnuThemDoiTuong` | `mnuAddInmate` |
| `mnuVaiTro` | `mnuRoles` |
| `moi` | `newValues` |
| `mongDoi` | `expected` |
| `nam` | `year` |
| `ngay` | `date` |
| `ngayChungTu` | `voucherDate` |
| `ngayNhan` | `receivedDate` |
| `ngayTao` | `createdAt` |
| `ngayVao` | `admissionDate` |
| `nghiepVu` | `transactionType` |
| `nguoiDung` | `user` |
| `nguoiDungId` | `userId` |
| `nguoiDungStore` | `userStore` |
| `nguoiDungVaiTro` | `userRole` |
| `nguoiGui` | `sender` |
| `nguoiGuiHoTen` | `senderFullName` |
| `nguoiThuHai` | `secondEdit` |
| `nguoiThuNhat` | `firstEdit` |
| `nguon` | `sourceDirectory` |
| `nhatKy` | `auditLog` |
| `nhatKyFactory` | `auditLogFactory` |
| `noiDung` | `description` |
| `numNamSinh` | `numBirthYear` |
| `phaiDoiMatKhau` | `mustChangePassword` |
| `phan` | `parts` |
| `phien` | `session` |
| `phut` | `minutes` |
| `quanGiao` | `supervising` |
| `quanHe` | `relationship` |
| `quanTri` | `administrator` |
| `quyen` | `permission` |
| `quyenTheoMa` | `idByCode` |
| `rowVerCu` | `staleRowVer` |
| `sau` | `after` |
| `sauLanHai` | `afterSecond` |
| `sauLanMot` | `afterFirst` |
| `soChungTu` | `voucherNumber` |
| `soDongNhatKy` | `auditLogCount` |
| `soDu` | `balance` |
| `soDuSau` | `balanceAfter` |
| `soDuTruoc` | `balanceBefore` |
| `soDuWriter` | `balanceWriter` |
| `soLanLuu` | `saveCount` |
| `soLanSai` | `failedAttemptCount` |
| `soPhieuGoc` | `sourceDocumentNumber` |
| `soTaiKhoan` | `accountNumber` |
| `soTaiKhoanNguoiGui` | `senderAccountNumber` |
| `soTien` | `amount` |
| `soTienBangChu` | `amountInWords` |
| `soVongLap` | `iterations` |
| `suKien` | `signInEvent` |
| `taiKhoan` | `account` |
| `taoHopThoai` | `createDialog` |
| `tap` | `granted` |
| `tenBang` | `tableName` |
| `tenCoSoDuLieuDich` | `targetDatabaseName` |
| `tenCoSoDuLieuXacNhan` | `confirmedDatabaseName` |
| `tenDangNhap` | `userName` |
| `tenTep` | `fileName` |
| `tep` | `file` |
| `thanhCong` | `succeeded` |
| `thayDoi` | `change` |
| `themDoiTuong` | `addInmate` |
| `themRow` | `createRow` |
| `theoMa` | `byCode` |
| `theoTen` | `byName` |
| `thoiDiem` | `now` |
| `thoiGian` | `period` |
| `thoiGianKhoa` | `lockPeriod` |
| `thongBao` | `message` |
| `thongDiep` | `failureMessage` |
| `thongTinDonViStore` | `facilityInfoStore` |
| `thuMuc` | `directory` |
| `tieuDe` | `title` |
| `timerTimKiem` | `tmrSearch` |
| `tinhDuoc` | `actual` |
| `trangThai` | `status` |
| `trongDb` | `inDatabase` |
| `truoc` | `before` |
| `tuChoi` | `rejectMarker` |
| `tuKhoa` | `keyword` |
| `txtBuongGiam` | `txtCell` |
| `txtChucVu` | `txtPosition` |
| `txtHoTen` | `txtFullName` |
| `txtMaCanBo` | `txtOfficerCode` |
| `txtMaSo` | `txtInmateCode` |
| `txtMatKhau` | `txtPassword` |
| `txtMatKhauHienTai` | `txtCurrentPassword` |
| `txtMatKhauMoi` | `txtNewPassword` |
| `txtNguoiGuiHoTen` | `txtSenderFullName` |
| `txtNoiDung` | `txtDescription` |
| `txtQuanHe` | `txtRelationship` |
| `txtSoTaiKhoan` | `txtAccountNumber` |
| `txtSoTien` | `txtAmount` |
| `txtTenDangNhap` | `txtUserName` |
| `txtTuKhoa` | `txtKeyword` |
| `txtXacNhan` | `txtConfirmation` |
| `vaiTro` | `role` |
| `vaiTroId` | `roleId` |
| `vaiTroMa` | `roleCode` |
| `vaiTroQuyen` | `rolePermission` |
| `viec` | `operation` |
| `viPham` | `violations` |
| `vuaKhoa` | `justLocked` |
| `xacNhan` | `confirmation` |
| `xemHt` | `administrationView` |

#### Test methods

| Old | New |
|---|---|
| `ApDungMigration_AlreadyCurrent_ReportsNothing` | `Migrate_AlreadyCurrent_ReportsNothing` |
| `ApDungMigration_DatabaseCreatedByAdmin_ChangesItsCollation` | `Migrate_DatabaseCreatedByAdmin_ChangesItsCollation` |
| `ApDungMigration_EmptyServer_CreatesDatabaseWithVietnameseCollation` | `Migrate_EmptyServer_CreatesDatabaseWithVietnameseCollation` |
| `ApDungMigration_NewDatabase_AppliesAndReportsEveryMigration` | `Migrate_NewDatabase_AppliesAndReportsEveryMigration` |
| `ApDungMigration_NewDatabase_AppliesEveryMigration` | `Migrate_NewDatabase_AppliesEveryMigration` |
| `CapNhat_NormalizesLikeTheConstructor` | `Update_NormalizesLikeTheConstructor` |
| `CapNhatQuyen_KhongCoQuyen_KhongGhiGiCa` | `UpdatePermissions_WithoutPermission_WritesNothing` |
| `CapNhatQuyen_LuuThayDoiVaLuuMotDongSuaVoiHaiDanhSachDaSapXep` | `UpdatePermissions_SavesTheChangeAndOneUpdateRowWithBothSortedLists` |
| `CapNhatQuyen_MaKhongCoTrongDanhMuc_BiTuChoi` | `UpdatePermissions_CodeNotInCatalogue_IsRejected` |
| `CapNhatQuyen_RowVerCu_XungDot` | `UpdatePermissions_StaleRowVer_Conflicts` |
| `Chay_MigrateAlreadyCurrent_SaysSo` | `Run_MigrateAlreadyCurrent_SaysSo` |
| `Chay_MigrateAndSeedDemo_RunsMigrationFirst` | `Run_MigrateAndSeedDemo_RunsMigrationFirst` |
| `Chay_MigrateFails_ReturnsFailureAndReportsError` | `Run_MigrateFails_ReturnsFailureAndReportsError` |
| `Chay_PendingMigrations_ReportsThemAndSucceeds` | `Run_PendingMigrations_ReportsThemAndSucceeds` |
| `Chay_SeedDemo_PassesEnvironmentAndConfirmation` | `Run_SeedDemo_PassesEnvironmentAndConfirmation` |
| `Chay_SeedDemoRefused_ReturnsFailureAndExplains` | `Run_SeedDemoRefused_ReturnsFailureAndExplains` |
| `Configure_AnyModel_MapsToTableNhatKyThaoTac` | `Configure_AnyModel_MapsToTableAuditLog` |
| `Configure_HanhDong_IsCheckedAgainstTheEnumNames` | `Configure_Action_IsCheckedAgainstTheStoredCodes` |
| `DaDangNhap_BeforeDangNhap_IsFalseWithNoUser` | `IsSignedIn_BeforeSignIn_IsFalseWithNoUser` |
| `DangBiKhoa_WhenTheLockExpired_IsOpenAgain` | `IsLocked_WhenTheLockExpired_IsOpenAgain` |
| `DangBiKhoa_WithoutALock_IsOpen` | `IsLocked_WithoutALock_IsOpen` |
| `DangCongTac_CanBeTurnedOffAndBackOn` | `IsActive_CanBeTurnedOffAndBackOn` |
| `DangNhap_AdminDaSeed_PhaiDoiMatKhau_RoiDangNhapLaiBinhThuong` | `SignIn_SeededAdmin_MustChangePasswordThenSignsInNormally` |
| `DangNhap_DungMatKhau_DatPhienVaLuuSuKien` | `SignIn_CorrectPassword_SetsSessionAndLogsEvent` |
| `DangNhap_HetThoiGianKhoa_DatVaoVaLamMoiSoLanSai` | `SignIn_LockExpired_SignsInAndResetsFailedAttempts` |
| `DangNhap_NamLanSai_KhoaTaiKhoanVaLuuSuKien` | `SignIn_FiveFailures_LocksAccountAndLogsEvent` |
| `DangNhap_TaiKhoanDangBiKhoa_DungMatKhauVanBiTuChoi` | `SignIn_LockedAccount_RejectsEvenTheCorrectPassword` |
| `DangNhap_TaiKhoanNgungHoatDong_BiTuChoiVoiCungThongBaoKhoa` | `SignIn_InactiveAccount_IsRejectedWithTheLockedMessage` |
| `DangNhap_ThenDangXuat_SetsAndClearsTheUser` | `SignIn_ThenSignOut_SetsAndClearsTheUser` |
| `DangXuat_WithoutASession_WritesNothing` | `SignOut_WithoutASession_WritesNothing` |
| `DangXuat_WritesTheEventThenClearsTheSession` | `SignOut_WritesTheEventThenClearsTheSession` |
| `DemoRun_TaiKhoanDangNhapDuocBangMatKhauDemo` | `DemoRun_AccountsSignInWithTheDemoPassword` |
| `DemoRun_TaoDuTaiKhoanVaiTro_ChayLaiKhongNhanDoi` | `DemoRun_CreatesEveryRoleAccount_RerunDoesNotDuplicate` |
| `Doc_Fractional_Throws` | `ToWords_Fractional_Throws` |
| `Doc_LinhStyle_ReadsLinhInsteadOfLe` | `ToWords_NorthernStyle_UsesTheNorthernZeroTensWord` |
| `Doc_MaxDecimal18_FitsTheStoredColumn` | `ToWords_MaxDecimal18_FitsTheStoredColumn` |
| `Doc_MaxDecimal18_ReadsEveryGroup` | `ToWords_MaxDecimal18_ReadsEveryGroup` |
| `Doc_Negative_Throws` | `ToWords_Negative_Throws` |
| `Doc_ReadsAmountInWords` | `ToWords_ReadsAmountInWords` |
| `Doc_TrailingZeroScale_IsStillWholeDong` | `ToWords_TrailingZeroScale_IsStillWholeDong` |
| `DoiMatKhau_KhongBaoGioGhiMatKhauHashVaoNhatKy` | `ChangePassword_NeverWritesThePasswordHashToTheAuditLog` |
| `DoiMatKhau_StoresTheNewHashAndClearsTheForcedChange` | `ChangePassword_StoresTheNewHashAndClearsTheForcedChange` |
| `DoiMatKhau_WithAMismatchedConfirmation_IsRejected` | `ChangePassword_WithAMismatchedConfirmation_IsRejected` |
| `DoiMatKhau_WithAValidNewPassword_StoresTheHashAndLogs` | `ChangePassword_WithAValidNewPassword_StoresTheHashAndLogs` |
| `DoiMatKhau_WithAWeakPassword_ReportsEveryBrokenRuleAndSavesNothing` | `ChangePassword_WithAWeakPassword_ReportsEveryBrokenRuleAndSavesNothing` |
| `DoiMatKhau_WithAWrongCurrentPassword_CountsTowardLockout` | `ChangePassword_WithAWrongCurrentPassword_CountsTowardLockout` |
| `DoiMatKhau_WithoutASignedInUser_IsRejected` | `ChangePassword_WithoutASignedInUser_IsRejected` |
| `DoiMatKhau_WithTheCurrentPassword_IsRejected` | `ChangePassword_WithTheCurrentPassword_IsRejected` |
| `DuocPhepMo_ConnectionFailed_ShowsConnectionMessageAndStops` | `CanOpen_ConnectionFailed_ShowsConnectionMessageAndStops` |
| `DuocPhepMo_Matches_ContinuesWithoutMessage` | `CanOpen_Matches_ContinuesWithoutMessage` |
| `DuocPhepMo_Mismatch_ShowsVersionMessageAndStops` | `CanOpen_Mismatch_ShowsVersionMessageAndStops` |
| `EveryConstantInTheNestedClasses_AppearsInTatCa` | `EveryConstantInTheNestedClasses_AppearsInAll` |
| `GhiNhanDangNhapDung_ResetsTheFailuresAndTheLock` | `RecordSuccessfulSignIn_ResetsTheFailuresAndTheLock` |
| `GhiNhanDangNhapSai_FifthAttempt_LocksForTheConfiguredPeriod` | `RecordFailedSignIn_FifthAttempt_LocksForTheConfiguredPeriod` |
| `GhiNhanDangNhapSai_FourthAttempt_DoesNotLock` | `RecordFailedSignIn_FourthAttempt_DoesNotLock` |
| `GhiNhanDangNhapSai_WithoutAPeriod_LocksUntilAnAdministratorUnlocks` | `RecordFailedSignIn_WithoutAPeriod_LocksUntilAnAdministratorUnlocks` |
| `HoTen_IsIndexedForSearch` | `FullName_IsIndexedForSearch` |
| `HoTen_UsesTheSearchCollation_SoNamesMatchWithoutAnyDiacritics` | `FullName_UsesTheSearchCollation_SoNamesMatchWithoutAnyDiacritics` |
| `KiemTra_AllowedValueMissingFromEnum_IsReported` | `Verify_AllowedValueMissingFromEnum_IsReported` |
| `KiemTra_APasswordBreakingOneRule_ReturnsThatViolation` | `Validate_APasswordBreakingOneRule_ReturnsThatViolation` |
| `KiemTra_ApplicationReferencingInfrastructure_IsReported` | `Check_ApplicationReferencingInfrastructure_IsReported` |
| `KiemTra_ApplicationWithAllowedPackage_Passes` | `Check_ApplicationWithAllowedPackage_Passes` |
| `KiemTra_ApplicationWithForbiddenPackage_IsReported` | `Check_ApplicationWithForbiddenPackage_IsReported` |
| `KiemTra_ApplicationWithFrameworkReference_IsReported` | `Check_ApplicationWithFrameworkReference_IsReported` |
| `KiemTra_ApplicationWithWindowsForms_IsReported` | `Check_ApplicationWithWindowsForms_IsReported` |
| `KiemTra_AStrongPassword_IsAccepted` | `Validate_AStrongPassword_IsAccepted` |
| `KiemTra_ColumnWithLongerName_ValuesAreNotMixedIn` | `Verify_ColumnWithLongerName_ValuesAreNotMixedIn` |
| `KiemTra_ColumnWithoutConstraint_IsReported` | `Verify_ColumnWithoutConstraint_IsReported` |
| `KiemTra_ConstraintOnAnotherTable_DoesNotCount` | `Verify_ConstraintOnAnotherTable_DoesNotCount` |
| `KiemTra_DatabaseDoesNotExist_IsMismatchWithNoActualVersion` | `Check_DatabaseDoesNotExist_IsMismatchWithNoActualVersion` |
| `KiemTra_DatabaseMigratedByNewerBuild_IsMismatch` | `Check_DatabaseMigratedByNewerBuild_IsMismatch` |
| `KiemTra_DeployedMismatchedConstraints_ReportsEachKind` | `Verify_DeployedMismatchedConstraints_ReportsEachKind` |
| `KiemTra_DevelopmentWithoutConfirmation_IsAllowed` | `Decide_DevelopmentWithoutConfirmation_IsAllowed` |
| `KiemTra_DomainWithAnalyzerOnlyPackage_Passes` | `Check_DomainWithAnalyzerOnlyPackage_Passes` |
| `KiemTra_DomainWithAnyPackage_IsReported` | `Check_DomainWithAnyPackage_IsReported` |
| `KiemTra_EmptyDatabaseWithoutHistoryTable_IsMismatchWithNoActualVersion` | `Check_EmptyDatabaseWithoutHistoryTable_IsMismatchWithNoActualVersion` |
| `KiemTra_EnumValueMissingFromConstraint_IsReported` | `Verify_EnumValueMissingFromConstraint_IsReported` |
| `KiemTra_EveryEnumColumnInModel_HasNoProblems` | `Verify_EveryEnumColumnInModel_HasNoProblems` |
| `KiemTra_EverySourceProject_HasARule` | `Check_EverySourceProject_HasARule` |
| `KiemTra_FullyMigratedDatabase_Matches` | `Check_FullyMigratedDatabase_Matches` |
| `KiemTra_InfrastructureMissingDomainReference_IsReported` | `Check_InfrastructureMissingDomainReference_IsReported` |
| `KiemTra_MatchingConstraint_HasNoProblems` | `Verify_MatchingConstraint_HasNoProblems` |
| `KiemTra_MiddleMigrationMissingFromHistory_IsMismatch` | `Check_MiddleMigrationMissingFromHistory_IsMismatch` |
| `KiemTra_NameStoredExtraNameInConstraint_IsReported` | `Verify_TextStoredExtraValueInConstraint_IsReported` |
| `KiemTra_NameStoredMatchingConstraint_HasNoProblems` | `Verify_TextStoredMatchingConstraint_HasNoProblems` |
| `KiemTra_NameStoredNameMissingFromConstraint_IsReported` | `Verify_TextStoredValueMissingFromConstraint_IsReported` |
| `KiemTra_NameStoredNumericConstraint_IsReported` | `Verify_TextStoredNumericConstraint_IsReported` |
| `KiemTra_Null_ReportsEveryViolation` | `Validate_Null_ReportsEveryViolation` |
| `KiemTra_OutsideDevelopmentMatchingDatabaseName_IsAllowed` | `Decide_OutsideDevelopmentMatchingDatabaseName_IsAllowed` |
| `KiemTra_OutsideDevelopmentWithoutConfirmation_IsNotDevelopment` | `Decide_OutsideDevelopmentWithoutConfirmation_IsNotDevelopment` |
| `KiemTra_OutsideDevelopmentWrongOrEmptyConfirmation_IsDatabaseNameMismatch` | `Decide_OutsideDevelopmentWrongOrEmptyConfirmation_IsDatabaseNameMismatch` |
| `KiemTra_SeveralConstraintsOnOneColumn_RequiresAllToAllowTheValue` | `Verify_SeveralConstraintsOnOneColumn_RequiresAllToAllowTheValue` |
| `KiemTra_SourceProject_FollowsTheDependencyRule` | `Check_SourceProject_FollowsTheDependencyRule` |
| `KiemTra_UnknownProject_IsReported` | `Check_UnknownProject_IsReported` |
| `KiemTra_UnreachableServer_IsConnectionFailedNotMismatch` | `Check_UnreachableServer_IsConnectionFailedNotMismatch` |
| `KiemTraQuyen_AdminCoQuyen_ChoPhep` | `Require_AdminHasPermission_Allows` |
| `KiemTraQuyen_KhongCoVaiTro_BiTuChoi` | `Require_WithoutRole_IsDenied` |
| `KiemTraQuyen_TaiKhoanNgungHoatDong_BiTuChoi` | `Require_InactiveAccount_IsDenied` |
| `KiemTraQuyen_ThuHoiVaiTro_MatQuyenNgay` | `Require_RoleRevoked_LosesPermissionImmediately` |
| `LaQuyenDacBiet_IsTrueOnlyOutsideTheStandardGrid` | `IsSpecial_IsTrueOnlyOutsideTheStandardGrid` |
| `LayCanBoDangCongTac_ReturnsActiveStaff_OptionallyWardensOnly` | `GetActiveOfficers_ReturnsActiveStaff_OptionallyWardensOnly` |
| `LayCotEnum_EveryEnumColumn_IsDeclaredByte` | `GetEnumColumns_EveryEnumColumn_IsDeclaredByte` |
| `LayCotEnum_NumericEnumColumn_IsTinyint` | `GetEnumColumns_NumericEnumColumn_IsTinyint` |
| `LayThongBaoChan_ConnectionFailed_ReturnsConnectionMessage` | `GetBlockingMessage_ConnectionFailed_ReturnsConnectionMessage` |
| `LayThongBaoChan_Matches_ReturnsNull` | `GetBlockingMessage_Matches_ReturnsNull` |
| `LayThongBaoChan_Mismatch_ReturnsVersionMessage` | `GetBlockingMessage_Mismatch_ReturnsVersionMessage` |
| `LayThuocTinhEnum_NullableAndNonNullableEnums_FindsBoth` | `GetEnumProperties_NullableAndNonNullableEnums_FindsBoth` |
| `MaCanBo_IsUnique` | `OfficerCode_IsUnique` |
| `Nap_MissingFile_DoesNothing` | `Load_MissingFile_DoesNothing` |
| `Nap_VariableAlreadySet_SetsOnlyMissingOnes` | `Load_VariableAlreadySet_SetsOnlyMissingOnes` |
| `NapDuLieuMau_OutsideDevelopmentConfirmedWithConnectedDatabaseName_IsAllowed` | `Seed_OutsideDevelopmentConfirmedWithConnectedDatabaseName_IsAllowed` |
| `NapDuLieuMau_OutsideDevelopmentNotConfirmed_IsRefusedAndWritesNothing` | `Seed_OutsideDevelopmentNotConfirmed_IsRefusedAndWritesNothing` |
| `NguoiDungVaiTro_AdminDuocGanQuanTri` | `UserRole_AdminIsAssignedTheAdministratorRole` |
| `OnLoaded_ViewLoaded_SetsTieuDeFromOptions` | `OnLoaded_ViewLoaded_SetsTitleFromOptions` |
| `OnlyGhiSoLuuKyService_DependsOnTheBalanceWriter` | `OnlyCustodyLedgerService_DependsOnTheBalanceWriter` |
| `PhanTich_BlankLinesAndComments_ReturnsOnlyKeyValuePairs` | `Parse_BlankLinesAndComments_ReturnsOnlyKeyValuePairs` |
| `PhanTich_ForceFlag_CarriesConfirmedDatabaseName` | `Parse_ForceFlag_CarriesConfirmedDatabaseName` |
| `PhanTich_MalformedLine_FailsWithItsLineNumber` | `Parse_MalformedLine_FailsWithItsLineNumber` |
| `PhanTich_MigrateAndSeedDemo_SetsBoth` | `Parse_MigrateAndSeedDemo_SetsBoth` |
| `PhanTich_MigrateVerb_IsAdminCommand` | `Parse_MigrateVerb_IsAdminCommand` |
| `PhanTich_MixedArguments_PassesOnlyNonAdminArgumentsToHost` | `Parse_MixedArguments_PassesOnlyNonAdminArgumentsToHost` |
| `PhanTich_NoArguments_IsNotAdminCommand` | `Parse_NoArguments_IsNotAdminCommand` |
| `PhanTich_QuotedValue_StripsOnlyMatchingQuotes` | `Parse_QuotedValue_StripsOnlyMatchingQuotes` |
| `PhanTich_ValueWithBackslashes_KeepsThemLiteral` | `Parse_ValueWithBackslashes_KeepsThemLiteral` |
| `PhanTich_VerbInUpperCase_IsRecognised` | `Parse_VerbInUpperCase_IsRecognised` |
| `Quyen_Seed_KhopHoanToanVoiDanhMuc` | `Permission_Seed_MatchesTheCatalogueExactly` |
| `QuyenCuaModule_CanLeaveDuyetOut` | `ForModule_CanLeaveApproveOut` |
| `SaveChanges_AlreadyCancelledVoucherEdited_WritesSuaRow` | `SaveChanges_AlreadyCancelledVoucherEdited_WritesUpdateRow` |
| `SaveChanges_CancelledVoucher_WritesHuyRowNotSua` | `SaveChanges_CancelledVoucher_WritesCancelRowNotUpdate` |
| `SaveChanges_ChangedVoucher_WritesSuaRowWithOnlyTheChangedColumns` | `SaveChanges_ChangedVoucher_WritesUpdateRowWithOnlyTheChangedColumns` |
| `SaveChanges_NewVoucher_WritesThemRowWithNewValues` | `SaveChanges_NewVoucher_WritesCreateRowWithNewValues` |
| `Sua_ConcurrencyConflict_PassesThroughAsBusinessError` | `Update_ConcurrencyConflict_PassesThroughAsBusinessError` |
| `Sua_KeepingItsOwnCode_Saves` | `Update_KeepingItsOwnCode_Saves` |
| `Sua_OpensTheSelectedStaffMember` | `Edit_OpensTheSelectedStaffMember` |
| `Sua_ToAnotherStaffMembersCode_IsRejected` | `Update_ToAnotherStaffMembersCode_IsRejected` |
| `Sua_UnknownStaffMember_IsRejected` | `Update_UnknownStaffMember_IsRejected` |
| `Sua_WithNothingSelected_DoesNothing` | `Edit_WithNothingSelected_DoesNothing` |
| `Sua_WithoutRowVersion_IsAProgrammingError` | `Update_WithoutRowVersion_IsAProgrammingError` |
| `Table_IsCanBo` | `Table_IsOfficer` |
| `Tao_DatabaseBehind_IsMismatch` | `Create_DatabaseBehind_IsMismatch` |
| `Tao_DatabaseMigratedByNewerBuild_IsMismatch` | `Create_DatabaseMigratedByNewerBuild_IsMismatch` |
| `Tao_MergedMigrationWithEarlierTimestampNotApplied_IsMismatch` | `Create_MergedMigrationWithEarlierTimestampNotApplied_IsMismatch` |
| `Tao_MigrationDiffersOnlyInCase_IsMismatch` | `Create_MigrationDiffersOnlyInCase_IsMismatch` |
| `Tao_NeverMigratedDatabase_IsMismatch` | `Create_NeverMigratedDatabase_IsMismatch` |
| `Tao_SameMigrations_Matches` | `Create_SameMigrations_Matches` |
| `TaoLoiKetNoi_AnyMigration_IsNeitherMatchNorMismatch` | `CreateConnectionFailed_AnyMigration_IsNeitherMatchNorMismatch` |
| `TatCa_CoversEveryModuleAndActionExactlyOnce` | `All_CoversEveryModuleAndActionExactlyOnce` |
| `TatCa_EveryCodeMatchesTheModuleActionPattern` | `All_EveryCodeMatchesTheModuleActionPattern` |
| `TatCa_HasThe48StandardEntries` | `All_HasThe48StandardEntries` |
| `TatCa_HasUniqueCodesAndIds` | `All_HasUniqueCodesAndIds` |
| `Them_DuplicateCode_IsRejected` | `Add_DuplicateCode_IsRejected` |
| `Them_DuplicateCode_IsRejected_AndNothingIsSaved` | `Add_DuplicateCode_IsRejected_AndNothingIsSaved` |
| `Them_InvalidRequest_IsRejectedWithEveryMessage_AndNothingIsSaved` | `Add_InvalidRequest_IsRejectedWithEveryMessage_AndNothingIsSaved` |
| `Them_IsSaved_AndFoundByNameWithoutDiacritics` | `Add_IsSaved_AndFoundByNameWithoutDiacritics` |
| `Them_OpensAnEmptyDialog_AndReloadsAfterASave` | `Add_OpensAnEmptyDialog_AndReloadsAfterASave` |
| `Them_SavesNormalizedStaffMember` | `Add_SavesNormalizedStaffMember` |
| `Them_SomeoneWhoHasAlreadyLeft_IsStoredInactive` | `Add_SomeoneWhoHasAlreadyLeft_IsStoredInactive` |
| `Them_UniqueIndexViolation_IsReportedAsDuplicateCode` | `Add_UniqueIndexViolation_IsReportedAsDuplicateCode` |
| `Tim_FoldsEveryVietnameseLetter_IncludingDAndHornedVowels` | `Search_FoldsEveryVietnameseLetter_IncludingDAndHornedVowels` |
| `Tim_HidesInactiveStaffUnlessAskedFor` | `Search_HidesInactiveStaffUnlessAskedFor` |
| `Tim_MatchesPartOfTheCode_InAnyCase` | `Search_MatchesPartOfTheCode_InAnyCase` |
| `Tim_TreatsLikeWildcardsInTheKeywordAsText` | `Search_TreatsLikeWildcardsInTheKeywordAsText` |
| `VaiTro_Seed_DuSauVaiTroVoiMaChuan` | `Role_Seed_HasTheSixStandardRolesWithTheirCodes` |
| `VaiTroQuyen_Seed_DungQuyenMacDinh` | `RolePermission_Seed_UsesTheDefaultPermissions` |
| `XacDinh_Deleted_ThrowsBecauseVouchersAreCancelledNotDeleted` | `Resolve_Deleted_ThrowsBecauseVouchersAreCancelledNotDeleted` |
| `XacDinh_EntryStateAndCancellation_MapsToHanhDong` | `Resolve_EntryStateAndCancellation_MapsToAuditAction` |
| `XacDinh_StateWithoutAChange_Throws` | `Resolve_StateWithoutAChange_Throws` |

### File List

Every source and test file under `src/` and `tests/` except the historic migrations was renamed or edited. File names follow the type renames above, and the module folders `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/` became `Custody/`, `Inventory/`, `MasterData/`, `Administration/`, `Reporting/`. New files:

- `src/Libraries/LuuKyCanTin.Application/Administration/LegacyAuditNames.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/SignInStatus.cs`
- `src/Libraries/LuuKyCanTin.Application/MasterData/InmateTypeDisplayExtensions.cs`
- `src/Libraries/LuuKyCanTin.Application/Custody/PaymentMethodDisplayExtensions.cs`
- `src/Libraries/LuuKyCanTin.Application/Custody/TransactionTypeDisplayExtensions.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/Administration/AuditActionCodes.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/20261003120750_RenameIdentifiersToEnglish.cs` (+ `.Designer.cs` and the updated `AppDbContextModelSnapshot.cs`)
- `tests/LuuKyCanTin.Application.UnitTests/Common/DisplayExtensionsTests.cs`
- `tests/LuuKyCanTin.IntegrationTests/Persistence/RenameIdentifiersToEnglishMigrationTests.cs`
- `tests/LuuKyCanTin.IntegrationTests/Persistence/LegacyAuditNamesTests.cs`

Docs: `CLAUDE.md`, `docs/conventions/naming-conventions.md`, `docs/conventions/ui-prototype-conventions.md`, `docs/decisions/0001-pdf-engine-questpdf.md`, `docs/decisions/0003-vietnamese-incremental-search.md`, `docs/install.md`.

## Change Log

| Date | Change |
|---|---|
| 2026-10-03 | Story created after the naming convention changed to English identifiers with Vietnamese display text. |
| 2026-10-03 | Implemented: every identifier renamed to English, `RenameIdentifiersToEnglish` migration, `ToDisplayText()` extensions, `LegacyAuditNames`; status → review. |
