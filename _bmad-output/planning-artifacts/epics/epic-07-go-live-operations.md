---
epic: 7
title: Go-live & safe operations
release: R0.5
frsCovered: [FR12, FR13, FR47, FR48]
backlogItems: [HT-08, NEN-12, GAP-05, GAP-06, GAP-01, GAP-04, GAP-07]
dependsOn: [1, 2, 3, 4, 5, 6]
---

## Epic 7: Go-live & safe operations

The unit can run the custodial system for real. The server is installed and hardened, workstations install and update from the LAN, data is backed up every day and the backups are verified, opening balances are carried over from the paper ledger with approval, and a one-month parallel run proves the figures before paper is retired.

**Exit criteria (R0.5 release gate):** one month in parallel with the paper ledger with 100% matching month-end balances (Story 7.7); the latest backup is verified; the first-run checklist is complete.

**Alignment note A12:** opening balances need a new `NghiepVu` code **15 "Số dư chuyển sang"** (receipt, `LoaiPhieu = 1`). This changes the DB design: the enum and the CHECK constraint are extended together, and the enum-consistency test (Story 1.2) must pass.

---

### Story 7.1: Manual backup and restore

`HT-08` (manual part) · Size M · Depends on: 2.5 · FR12

As a system administrator,
I want to back up the database on demand and restore it from a file,
So that I can protect the data before risky operations and recover from failures.

**Acceptance Criteria:**

**Given** a user with `HT.SaoLuu` and a configured backup folder on the server
**When** they click "Sao lưu ngay"
**Then** `BACKUP DATABASE … WITH CHECKSUM, COMPRESSION` (compression only where the edition supports it) writes `LuuKyCanTin_yyyyMMdd_HHmmss.bak` to that folder, the result and size are shown, and the action is audit-logged

**Given** a user with `HT.PhucHoi` and a chosen `.bak` file
**When** they start a restore
**Then** the app shows the backup's date and database name, requires a typed confirmation, takes a safety backup of the current DB first, switches the DB to single-user mode, restores, and returns it to multi-user mode
**And** other workstations lose their connection and show "Cơ sở dữ liệu đang được phục hồi"; after the restore, the DB-version check (Story 1.2) runs before anyone continues

**Given** a restore that fails
**When** the error occurs
**Then** the DB is returned to multi-user mode, the error is logged, and the safety backup's path is shown

**Given** a user without these permissions
**When** the backup or restore service is called
**Then** it is refused

---

### Story 7.2: Scheduled daily backup, verification and age warning

`HT-08, NEN-12` · Size M · Depends on: 7.1 · FR13

As a system administrator,
I want an automatic daily backup that is copied to another drive and verified, with a warning when backups stop,
So that we never discover a missing backup on the day we need it.

**Acceptance Criteria:**

**Given** SQL Server Express (no SQL Agent)
**When** the admin runs the provided setup script
**Then** a Windows Task Scheduler job runs `sqlcmd` every day at a configurable time, backs up with `CHECKSUM`, copies the file to a second drive or share, and deletes files older than the retention setting (default 30 days)

**Given** the latest backup file
**When** the verification job runs (daily, after the backup)
**Then** it runs `RESTORE VERIFYONLY … WITH CHECKSUM` and records the result in a backup-status table with time, file, size and success

**Given** the admin opens the main screen
**When** the latest successful backup is older than 24 hours, or the latest verification failed
**Then** a red warning banner shows the age or failure and links to the backup screen

**Given** the backup screen
**When** the admin views history
**Then** the last 30 backups are listed with verification status

---

### Story 7.3: Server setup and least-privilege database login

`GAP-05` · Size M · Depends on: 1.2, 1.3

As a system administrator,
I want a documented, repeatable server setup and an application login that cannot erase records,
So that the data is protected even from someone who obtains the app's credentials.

**Acceptance Criteria:**

**Given** the install checklist in `docs/operations/server-setup.md`
**When** the admin follows it on a clean Windows server
**Then** SQL Server 2022 Express is installed with TCP enabled on a fixed port, the server has a static IP, the Windows firewall allows only that port from the LAN, and a server certificate is installed so connections use `Encrypt=True` without `TrustServerCertificate`

**Given** a migration or setup script that creates the app SQL login and role
**When** it runs
**Then** the login gets only the permissions the app needs, plus `DENY UPDATE, DELETE ON NhatKyThaoTac` and `DENY DELETE` on every voucher table (`ChungTuLuuKy`, `DeNghiChoTien`, `BangKeNop`, `BangKeNopChiTiet`, and later `PhieuNhap*`, `PhieuBanHang*`, `TheKho`)
**And** voucher tables are **not** denied UPDATE, because the app updates status and cancellation fields (alignment note A3)
**And** migrations run under a separate admin login

**Given** the integration test connected as the app login
**When** it tries `DELETE FROM ChungTuLuuKy` or `UPDATE NhatKyThaoTac`
**Then** both fail with a permission error

**Given** `appsettings.json` on a workstation
**When** the admin sets the connection string through the admin tool
**Then** it is stored encrypted with DPAPI (machine scope), decrypted only at startup, and never written to the log

---

### Story 7.4: Installer and one-touch LAN updates

`GAP-06` · Size M · Depends on: 1.6, 7.3

As a system administrator,
I want to install the app once on each workstation and then publish updates to one shared folder,
So that every workstation upgrades itself without me visiting it and without Internet.

**Acceptance Criteria:**

**Given** the release pipeline from the Velopack spike (Story 1.6)
**When** a release is built
**Then** it produces a self-contained `Setup.exe` and update packages, and the installer bundle includes the WebView2 Runtime offline installer, which runs if WebView2 is missing

**Given** the admin publishes a new version to `\\server\LuuKyCanTin\releases`
**When** a workstation starts the app
**Then** it checks the share, downloads and applies the update, and restarts on the new version; if the share is unreachable it starts the installed version and logs a warning

**Given** a version that needs a DB migration
**When** the admin runs the admin tool with `--migrate` on the admin machine
**Then** migrations are applied once, and workstations still on the old version are refused by the DB-version check (Story 1.2) until they update

**Given** the release notes
**When** a release is published
**Then** a step-by-step publish procedure (`docs/operations/release.md`) lists the order: back up → migrate → publish → verify one workstation

---

### Story 7.5: Opening balances carried forward at go-live

`GAP-01, DEC-09` · Size M · Depends on: 2.6, 3.4, 4.3 · FR47

As an accountant,
I want to enter each detainee's balance from the paper ledger as an approved "carried forward" document,
So that the system starts from the true balances and the opening entries can be traced like any other.

**Acceptance Criteria:**

**Given** a migration that adds `NghiepVu = 15` "Số dư chuyển sang" (receipt, `LoaiPhieu = 1`) to the Domain enum and to the `ChungTuLuuKy` CHECK constraint
**When** the enum-consistency test runs
**Then** it passes (alignment note A12)

**Given** the go-live date setting
**When** the accountant enters opening balances per detainee, or bulk-imports them from Excel (code, amount; with preview and per-row errors, reusing the Story 3.4 import pattern)
**Then** the entries are saved as drafts dated on the go-live date, with "Số dư chuyển sang từ sổ giấy" as the description

**Given** draft opening entries
**When** a leader with `LK-T.Duyet` approves them (the creator cannot approve, per the Story 2.6 policy)
**Then** each entry is posted through `GhiSoLuuKyService` with its own `BNT` number, and the approval is audit-logged as `Duyet`

**Given** a detainee who already has a posted opening entry
**When** another opening entry is created for them
**Then** it is refused; an opening entry is also refused after the go-live date

**Given** the ledger reports and reconciliation
**When** they cover the go-live date
**Then** code 15 counts toward the balance, is shown as "Số dư chuyển sang", and the reconciliation (Story 6.7) shows 0 difference

---

### Story 7.6: First-run setup checklist

`GAP-04` · Size S · Depends on: 2.8, 2.9, 2.4, 3.4, 7.5 · FR48

As a system administrator,
I want a checklist that walks me through everything that must be set up before go-live,
So that nothing is missed when the system starts real use.

**Acceptance Criteria:**

**Given** a new installation
**When** the admin signs in
**Then** a "Khởi tạo hệ thống" checklist shows each item with done/not done: unit information (2.8), signers for every template (2.9), staff and user accounts with roles (2.1, 2.4), detainee import (3.4), opening balances approved (7.5), first verified backup (7.2)

**Given** an item
**When** the admin clicks it
**Then** the matching screen opens; on return the status is re-evaluated from the data (e.g. `ThongTinDonVi.TenDonVi` not empty, at least one signer per template)

**Given** an incomplete checklist
**When** any admin signs in
**Then** a reminder banner shows until every item is done; the checklist can be reopened from the System menu

---

### Story 7.7: One-month parallel run with the paper ledger

`GAP-07` · Size M · Depends on: 6.4, 7.5

As a unit leader,
I want to run the system alongside the paper ledger for one month and compare the month-end balances,
So that we retire paper only when the system has proven it is exact.

**Acceptance Criteria:**

**Given** the month-end unit ledger book (Story 6.4)
**When** the accountant exports the comparison sheet
**Then** an Excel file lists every detainee with code, name and system closing balance, plus empty columns "Số dư sổ giấy" and "Chênh lệch" (formula), and a summary count of mismatches

**Given** the filled-in sheet
**When** every row shows a difference of 0 and the reconciliation (Story 6.7) is green
**Then** the parallel run passes; any mismatch is investigated and documented, and the run repeats for the following month

**Given** a passed parallel run
**When** the accountant and unit leader sign the sign-off record (template in `docs/operations/parallel-run-signoff.md`)
**Then** the R0.5 release gate is met, and the paper ledger can be retired
