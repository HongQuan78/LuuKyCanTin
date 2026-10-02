---
epic: 2
title: Secure access & unit setup
release: R0.5
frsCovered: [FR1, FR2, FR3, FR4, FR5, FR6, FR11, FR17]
backlogItems: [DM-04, HT-01, HT-02, NEN-09, NEN-10, HT-03, HT-04, HT-07]
dependsOn: [1]
---

## Epic 2: Secure access & unit setup

The administrator sets up the unit: unit information, staff, signatories, user accounts and roles. Every user then signs in with their own account, sees only the functions they are allowed to use, and cannot get around permissions by any path, because services re-check every write. The workstation locks itself when left unattended, and leadership can look up who did what and when in the audit log.

**Exit criteria:** every R0.5 role can sign in and sees only its own menus; a service rejects an unauthorized call even when the UI is bypassed; the unit header and the signers for all 12 templates are configured.

**Applies to every story in this epic:** every account-administration and configuration change is written to `NhatKyThaoTac` (Story 1.3); services check permission through `ICurrentUser` before writing; passwords and hashes never appear in logs or audit JSON.

---

### Story 2.1: Staff register

`DM-04` · Size S · Depends on: 1.2, 1.3 · FR17

As an administrator,
I want to maintain the list of staff (cán bộ) with their position and whether they are a warden,
So that accounts, signatories, wardens of detainees and document creators all refer to real people.

**Acceptance Criteria:**

**Given** the `CanBo` table (`Id`, `MaCanBo varchar(20)` UQ, `HoTen nvarchar(100)` NOT NULL, `ChucVu nvarchar(100)`, `LaQuanGiao bit`, `DangCongTac bit` default 1, plus audit columns)
**When** the administrator adds a staff member with code, name, position and the warden flag
**Then** the record is saved and appears in the staff list, searchable by code or name without diacritics

**Given** an existing staff code
**When** another staff member is saved with the same `MaCanBo`
**Then** the save is rejected with "Mã cán bộ đã tồn tại" (code already exists)

**Given** a staff member who has left
**When** the administrator sets `DangCongTac = 0`
**Then** the person is hidden from every selection list (wardens, signers, accounts) but stays on historical documents and in reports
**And** staff are never hard-deleted

**Given** the business roles from the spec (warden, canteen, commander, accountant, leadership)
**When** a staff member is recorded
**Then** the role shows in `ChucVu` and `LaQuanGiao` marks wardens; system permissions come from account roles (Story 2.3), not from this record

**Given** two users edit the same staff record
**When** the second one saves
**Then** the `RowVer` conflict is detected and the user sees "Dữ liệu đã bị người khác thay đổi, vui lòng tải lại" (data changed by someone else, please reload)

---

### Story 2.2: Secure sign-in, password policy and lockout

`HT-01` · Size M · Depends on: 1.8, 2.1 · FR1

As a user,
I want to sign in with my own account, change my password and sign out, with the account protected against guessing,
So that nobody can act under my name.

**Acceptance Criteria:**

**Given** the `NguoiDung` table extended with `SoLanSai tinyint`, `KhoaDen datetime2(0) NULL`, `DangHoatDong bit` and `PhaiDoiMatKhau bit`
**When** a user signs in with the correct password
**Then** `SoLanSai` resets to 0, `ICurrentUser` is populated, and a `DangNhap` audit row is written with user, workstation and `IClock.Now`

**Given** a wrong password
**When** the user fails for the 5th consecutive time
**Then** the account is locked (`KhoaDen` set), the message says the account is locked, and an audit row is written
**And** the error message is the same for an unknown user and a wrong password (no user enumeration)

**Given** a locked or inactive account
**When** the user tries to sign in, even with the correct password
**Then** sign-in is refused until an administrator unlocks the account (Story 2.4) or the lock period configured in `appsettings.json` expires

**Given** an account with `PhaiDoiMatKhau = 1` (the seeded `admin` and any newly created or reset account)
**When** the user signs in
**Then** they must set a new password before reaching the main shell

**Given** a new password
**When** it is validated
**Then** it must have at least 8 characters, including an upper-case letter, a lower-case letter and a digit, and must differ from the current one; the password is stored only as a salted PBKDF2 hash (`Rfc2898DeriveBytes.Pbkdf2`, SHA-256, ≥ 100,000 iterations, per-user random salt)

**Given** a signed-in user
**When** they choose Change password (current + new) or Sign out
**Then** the change requires the current password; sign-out returns to the login form and clears `ICurrentUser`

---

### Story 2.3: Roles and permission catalogue

`HT-02` · Size M · Depends on: 2.2 · FR2 (part)

As an administrator,
I want a fixed catalogue of permissions (module × action) and the 6 standard roles seeded,
So that I grant access by role instead of configuring every user by hand.

**Acceptance Criteria:**

**Given** the tables `VaiTro` (`Ma varchar(30)` UQ, `Ten`), `Quyen` (`Ma varchar(50)` UQ, `Ten`, `Module varchar(10)`) and `VaiTroQuyen` (composite PK)
**When** the migration runs
**Then** `Quyen` holds one row per module × action in the form `<Module>.<Action>` (for example `LK-C.Duyet`, `BH.Huy`), for modules `HT, DM, LK-T, LK-C, LK-BC, NH, BH, HH-BC` and actions `Xem, Them, Sua, Huy, In, Duyet`

**Given** permission codes used in code
**When** they are referenced
**Then** they come from one static catalogue class in Application (no string literals scattered in services)
**And** a unit test fails if a catalogue entry has no seeded `Quyen` row, or if a seeded row has no catalogue entry
**And** later stories may add special permissions to the catalogue with their own seed rows and default role grants. The registry of special permissions, each seeded by the story that needs it, is: `HT.KhoaSo` (lock/unlock period, 6.8, Kế toán), `LK.LuiNgay` (back-dating, 6.9), `HT.SaoLuu` / `HT.PhucHoi` (backup/restore, 7.1, Quản trị), `HT.CauHinh` (business settings, e.g. 14.6, Quản trị), `HT.QuanTri` (training mode, support bundle, 14.2/14.14, Quản trị)

**Given** the seed
**When** the database is created
**Then** these 6 roles exist with their default permissions from the feature list:
- *Quản trị hệ thống*: all of `HT`, all of `DM`.
- *Cán bộ theo dõi tiền lưu ký*: `LK-T`, `LK-C`, `LK-BC` (all actions except `Duyet`), `DM.Xem`.
- *Cán bộ căn tin / bán hàng*: `NH`, `BH`, `HH-BC`, `LK-BC.Xem`.
- *Cán bộ quản giáo*: `LK-BC.Xem` (the purchase-registration permission and the "own detainees only" scope are added in Epic 12).
- *Chỉ huy phụ trách / Lãnh đạo đơn vị*: `Duyet` on `LK-T` (opening balances, 7.5), `LK-C` (transfers, 5.5) and `NH` (opening stock, 9.6), plus `Xem` on every report module, plus `HT.Xem` (audit-log viewer, 2.10).
- *Kế toán đơn vị*: `Xem` on `LK-BC` and `HH-BC` (the period-lock permission `HT.KhoaSo` is granted when Story 6.8 seeds it).

**Given** the administrator opens Roles
**When** they tick or untick permissions for a role and save
**Then** `VaiTroQuyen` is updated and an audit row records the before/after permission list

---

### Story 2.4: User accounts linked to staff and role assignment

`HT-02, NEN-09` · Size M · Depends on: 2.1, 2.3 · FR2

As an administrator,
I want to create accounts for staff, assign roles, deactivate accounts and reset passwords,
So that every person works under their own account with exactly the access their job needs.

**Acceptance Criteria:**

**Given** `NguoiDung.CanBoId` (FK → `CanBo`, NULL only for the built-in admin) and `NguoiDungVaiTro` (composite PK)
**When** the administrator creates an account with a unique `TenDangNhap`, an active staff member and one or more roles
**Then** the account is saved with a temporary password and `PhaiDoiMatKhau = 1`

**Given** a staff member who already has an active account
**When** a second active account is created for the same staff member
**Then** it is rejected (one active account per person)

**Given** an existing account
**When** the administrator deactivates it, unlocks it, resets its password or changes its roles
**Then** the change takes effect at the user's next sign-in or permission check, and one audit row per action records who did it, when, and the before/after values (never the password or hash)

**Given** the last active account holding the `HT` administration permissions
**When** someone tries to deactivate it or remove its admin role
**Then** the action is refused so the system can never be left without an administrator

**Given** any user without `HT.Sua`
**When** they call the account service directly
**Then** an authorization error is raised and nothing is written

---

### Story 2.5: Permission-driven shell and service-level authorization

`HT-02` · Size M · Depends on: 2.3, 2.4 · FR2 · UX-DR7

As a unit leader,
I want each user to see only the functions they may use, and every write to be authorized again by the service,
So that hiding a button is never the only protection.

**Acceptance Criteria:**

**Given** `ICurrentUser` exposing `HasPermission(string code)` loaded from the user's roles at sign-in
**When** the main shell builds its menu and toolbar
**Then** each item declares the permission it needs; items the user lacks are hidden; a module with no visible items is hidden

**Given** an Application service method that writes data
**When** the current user lacks the required permission
**Then** the service throws a domain `KhongCoQuyenException` (no permission) before opening a transaction, and the UI shows "Bạn không có quyền thực hiện thao tác này" (you are not allowed to do this)

**Given** an integration test that calls a service directly, bypassing the hidden menu
**When** the caller has only `Xem` rights
**Then** the write is rejected and no row (data or audit) is written

**Given** an administrator who changes a user's roles
**When** that user next opens a function
**Then** permissions are re-read from the DB at sign-in, and services check the current DB permission on writes (a revoked permission stops working immediately)

---

### Story 2.6: Segregation-of-duties policy

`NEN-09` · Size S · Depends on: 2.5 · FR3

As a unit leader,
I want a rule that whoever created a request or document can never approve it,
So that a single person cannot both originate and authorize a movement of money.

**Acceptance Criteria:**

**Given** a reusable Application policy `KiemTraTachNhiemVu` that takes the creator's user id and the approver's user id
**When** they are the same person (same user, or two accounts linked to the same `CanBoId`)
**Then** it throws `ViPhamTachNhiemVuException` with "Người lập không được tự duyệt" (the creator cannot approve their own document)

**Given** unit tests
**When** they run
**Then** they cover same user, different user with the same staff member, different staff member, and the admin account with no staff member

**Given** the policy
**When** later approval flows are built (transfer approval in Story 5.5, opening-balance approval in Story 7.5)
**Then** they call this policy instead of re-implementing the check, and every refused attempt writes an audit row

---

### Story 2.7: Session auto-lock

`NEN-10` · Size S · Depends on: 2.2 · FR4 · UX-DR11

As a custodial officer,
I want my session to lock itself when I step away and unlock quickly with my password,
So that nobody can use my account at an unattended workstation.

**Acceptance Criteria:**

**Given** an idle timeout read from `appsettings.json` (default 5 minutes)
**When** there is no keyboard or mouse input in the app for that long
**Then** a full-window lock screen covers the shell, showing the signed-in user's name and a password box

**Given** the lock screen
**When** the same user enters the correct password
**Then** the session resumes with every open form and unsaved input intact
**And** wrong attempts count toward the lockout of Story 2.2

**Given** the lock screen
**When** another person chooses "Đăng xuất" (sign out)
**Then** the app warns that unsaved work will be lost, then closes the session and returns to the login form; it never unlocks into the other user's session

**Given** the user clicks "Khoá máy" (lock) or presses Ctrl+L
**When** the shortcut fires
**Then** the session locks immediately; the lock and unlock events are written to the audit log

---

### Story 2.8: Unit information

`HT-03` · Size S · Depends on: 2.5 · FR5

As an administrator,
I want to enter the unit's parent agency, name and address once,
So that every printed form carries the correct heading.

**Acceptance Criteria:**

**Given** the `ThongTinDonVi` table from Story 1.8 (`Id = 1` only, `TenCoQuanChuQuan nvarchar(200)` optional, `TenDonVi nvarchar(200)` NOT NULL, `DiaChi nvarchar(300)` NOT NULL)
**When** an administrator with `HT.Sua` edits and saves the unit information
**Then** the single row is updated, audited with before/after values, and used by every print from then on

**Given** an empty unit name or address
**When** the user saves
**Then** validation shows which field is required and nothing is saved

**Given** a user without `HT.Sua`
**When** they open the screen
**Then** it is read-only, and the service rejects any save attempt

---

### Story 2.9: Signatory configuration per print template

`HT-04` · Size M · Depends on: 2.1, 2.8 · FR6

As an administrator,
I want to configure the signature columns of each print template, with an optional default signer name,
So that printed documents show the right titles and names without retyping them.

**Acceptance Criteria:**

**Given** the `CauHinhKyTen` table (`MaMauIn varchar(30)`, `ThuTu tinyint`, UQ (`MaMauIn`, `ThuTu`), `ChucDanh nvarchar(100)`, `CanBoId` NULL = leave the name blank)
**When** the migration runs
**Then** the signers of the 12 templates are seeded left to right exactly as in the process doc:
- `BIEN_NHAN_THU` (receipt): Người gửi · Người nhận · Lãnh đạo đơn vị.
- `PHIEU_CHI` (payout): Cán bộ theo dõi tiền lưu ký · Người bị tạm giữ, tạm giam/phạm nhân xác nhận · Cán bộ quản giáo xác nhận · Lãnh đạo đơn vị xác nhận.
- `BANG_KE_CA_NHAN` (per-detainee statement): Cán bộ căn tin · Cán bộ quản giáo · Người bị tạm giữ, tạm giam/phạm nhân · Thủ trưởng đơn vị.
- `SO_THEO_DOI` (unit ledger book): Cán bộ căn tin · Chỉ huy phụ trách · Kế toán đơn vị · Thủ trưởng đơn vị.
- `BANG_KE_NOP` (remittance list): Người nộp · Chỉ huy phụ trách · Thủ trưởng đơn vị.
- `PHIEU_NHAP` (goods receipt): Người giao · Người nhận · Chỉ huy đội · Lãnh đạo đơn vị.
- `PHIEU_MUA_HANG` (purchase slip): Người mua hàng · Cán bộ căn tin · Lãnh đạo đơn vị.
- `BAO_CAO_NXT` (stock movement): Cán bộ bán hàng · Chỉ huy phụ trách · Lãnh đạo đơn vị.
- `BAO_CAO_DOANH_THU` (revenue): Cán bộ căn tin · Chỉ huy phụ trách · Lãnh đạo đơn vị.
- `BANG_NIEM_YET_GIA` (posted price list): Cán bộ căn tin · Lãnh đạo đơn vị.
- `THEO_DOI_MUA_HANG` (purchase history): Cán bộ căn tin · Chỉ huy phụ trách · Lãnh đạo đơn vị.
- `DANH_MUC_HANG_HOA` (goods catalogue): Cán bộ căn tin · Chỉ huy phụ trách · Lãnh đạo đơn vị.

**Given** the signatory screen
**When** the administrator selects a template, then edits titles, reorders columns or picks a default staff member (active staff only)
**Then** the change is saved and audited; signer roles that are not staff (sender, detainee, buyer, deliverer) keep `CanBoId` NULL

**Given** a template with no signer rows
**When** it is saved
**Then** validation requires at least one signer column

---

### Story 2.10: Audit-log viewer

`HT-07` · Size M · Depends on: 1.3, 2.5 · FR11

As a unit leader,
I want to look up who added, changed, cancelled, printed or approved what, and when, with the old and new values,
So that I can investigate any discrepancy.

**Acceptance Criteria:**

**Given** a user with `HT.Xem`
**When** they open the audit log
**Then** they can filter by date range (default today), user, workstation, action (`Them, Sua, Huy, In, Duyet, DangNhap`), table and record id, and results are paged (newest first)

**Given** a selected audit row
**When** the user opens its detail
**Then** a side-by-side view shows each changed field with old → new values parsed from `DuLieuCu` / `DuLieuMoi`

**Given** the audit-log screen
**When** it is used
**Then** it offers no edit or delete action anywhere, and the service exposes only read methods

**Given** 1 million audit rows
**When** filtering by a one-day range
**Then** results appear in under 2 seconds (an index on `ThoiDiem` plus `TenBang, BanGhiId` is added in this story's migration)
