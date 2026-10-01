---
epic: 1
title: Foundation & first printed receipt
release: Sprint 0
frsCovered: [FR25]
backlogItems: [NEN-01, NEN-02, NEN-03, NEN-04, NEN-05, NEN-06, NEN-07, LK-T03, SP-01, SP-02, SP-03, NEN-08]
dependsOn: []
---

## Epic 1: Foundation & first printed receipt

The team proves the whole stack before building features. An administrator signs in, registers one detainee, records one custodial receipt through the real ledger engine, and prints it as a PDF with the unit header and the amount in Vietnamese words. Along the way the technical risks are retired: printing, LAN updates and Vietnamese search.

**Exit criteria (Sprint 0 gate):** the walking skeleton runs through all 4 layers; CI is green; the spike outcomes are written down; DEC-02, DEC-05 and DEC-09 are decided by the PO.

**Applies to every story in this epic:** Clean Architecture references point inward only; Domain and Application are unit-tested without UI or DB; tests use xUnit + NSubstitute + Shouldly.

---

### Story 1.1: Solution skeleton, host and CI

`NEN-01, NEN-07` · Size M · Depends on: —

As a developer,
I want a buildable solution with the agreed layers, a Generic Host and a CI pipeline,
So that every later story starts from the same structure and is checked automatically.

**Acceptance Criteria:**

**Given** a clean clone of the repository
**When** I run `dotnet build LuuKyCanTin.slnx` and `dotnet test LuuKyCanTin.slnx`
**Then** both succeed with the projects `LuuKyCanTin.Domain`, `.Application`, `.Infrastructure` (under `src/Libraries`), `.WinForms` (under `src/Presentation`), and the test projects `Domain.UnitTests`, `Application.UnitTests`, `IntegrationTests`
**And** each source project has the module folders `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/`, `Common/` (Application also has `Abstractions/`)

**Given** the project references
**When** an architecture test inspects the assemblies
**Then** Domain references no project and no NuGet package; Application references only Domain (plus FluentValidation); Infrastructure references Application and Domain; WinForms references Application and Infrastructure
**And** the test fails if Application references EF Core SqlServer, QuestPDF, ClosedXML or System.Windows.Forms

**Given** the WinForms app starts
**When** `Program.cs` runs
**Then** it builds a Generic Host with DI, `appsettings.json` configuration and Serilog file logging to `%ProgramData%\LuuKyCanTin\logs` (rolling daily, 30 files kept)
**And** a global handler catches unhandled UI-thread and background exceptions, logs them, and shows a friendly Vietnamese message without a stack trace (UX-DR10)

**Given** a sample Presenter with a View interface
**When** its unit test runs
**Then** the Presenter is tested with an NSubstitute fake View and no Form, which documents the MVP convention (Views are passive, Presenters call Application services, services never reference `MessageBox`)

**Given** a push to `master`
**When** the CI pipeline runs on a Windows agent
**Then** it restores, builds and runs all tests, including LocalDB integration tests, and fails the build on any failure

---

### Story 1.2: Database migrations, schema-version check and enum ↔ CHECK test

`NEN-02, NEN-03, NEN-04` · Size M · Depends on: 1.1

As an administrator,
I want the database created and upgraded only through versioned migrations, and workstations that refuse to run against the wrong schema version,
So that every machine always works on a consistent database.

**Acceptance Criteria:**

**Given** an empty SQL Server (LocalDB in tests)
**When** the migrations are applied
**Then** the database is created with the default collation `Vietnamese_CI_AI` and the shared conventions: `int IDENTITY` keys, `decimal(18,0)` money, `date` document dates, `datetime2(0)` timestamps, and audit columns `NgayTao, NguoiTaoId, NgaySua, NguoiSuaId, RowVer` applied through a base configuration
**And** this story creates only the tables it needs; later stories add their own tables in their own migrations

**Given** reference data (roles, permissions, counters, units of measure, signer configuration)
**When** a later story needs it
**Then** it is seeded inside an EF migration (`HasData` or SQL in the migration), so a fresh install and an upgrade produce identical data
**And** dev/demo data is seeded by a separate command that never runs in production

**Given** a workstation whose build expects migration `X`
**When** it starts against a database whose last applied migration is not `X`
**Then** the app shows "Database version does not match, contact the administrator", logs both versions, and exits without opening the main form
**And** workstations never apply migrations themselves; migrations run only from the admin tool or command (`--migrate`)

**Given** every Domain enum mapped to a `tinyint` column
**When** the enum-consistency integration test runs against the migrated database
**Then** it reads each CHECK constraint from `sys.check_constraints` and fails if any enum value is missing from the constraint or any allowed value is missing from the enum

---

### Story 1.3: IClock and automatic audit-log interceptor

`NEN-05, NEN-06` · Size M · Depends on: 1.2

As a unit leader,
I want every change to a voucher automatically recorded with who, when, from which workstation, and the before/after values,
So that every operation can be traced and nobody can alter records silently.

**Acceptance Criteria:**

**Given** the Application abstraction `IClock` (`Today`, `Now`)
**When** any Domain or Application code needs the current date
**Then** it uses `IClock`; a test or analyzer rule fails the build if `DateTime.Now`, `DateTime.Today` or `DateTime.UtcNow` appears in Domain, Application or Infrastructure (except the `SystemClock` implementation)
**And** tests can substitute a fixed or simulated clock

**Given** the `NhatKyThaoTac` table (`Id bigint`, `ThoiDiem`, `NguoiDungId`, `MayTram`, `HanhDong` [Them, Sua, Huy, In, Duyet, DangNhap], `TenBang`, `BanGhiId`, `DuLieuCu`, `DuLieuMoi` as JSON)
**When** an entity marked as auditable (a marker interface) is added, modified or cancelled and `SaveChanges` runs
**Then** a `SaveChanges` interceptor inserts one audit row per affected record in the same transaction, with before/after JSON of the changed columns, the user from `ICurrentUser`, the machine name and `IClock.Now`
**And** a cancellation (`TrangThai` → cancelled) is logged as `Huy`, not `Sua`

**Given** an explicit business event that does not change a voucher row (login, print, approve)
**When** a service calls the audit-log writer
**Then** the row is written through the same append-only path

**Given** the application code
**When** anything tries to update or delete an `NhatKyThaoTac` row through EF
**Then** an exception is thrown (the log is append-only); the DB-level `DENY UPDATE, DELETE` is applied in Epic 7

---

### Story 1.4: Amount in Vietnamese words

`LK-T03` · Size S · Depends on: 1.1 · FR25

As a custodial officer,
I want every printed amount also written out correctly in Vietnamese words,
So that receipts and reports match the legal paper forms and cannot be misread.

**Acceptance Criteria:**

**Given** the Domain class `SoTienBangChu`
**When** it converts an integer number of đồng
**Then** the result starts with a capital letter and ends with "đồng" (e.g. 500000 → "Năm trăm nghìn đồng")
**And** it has no dependency outside Domain

**Given** the boundary table test
**When** it runs
**Then** these cases pass: 0 → "Không đồng"; 10 → "Mười đồng"; 15 → "Mười lăm đồng"; 21 → "Hai mươi mốt đồng"; 101 → "Một trăm lẻ một đồng"; 105 → "Một trăm lẻ năm đồng"; 1,000,005 → "Một triệu không trăm lẻ năm đồng"; 25 → "Hai mươi lăm đồng"; 11 → "Mười một đồng"; 1,000,000,000 → "Một tỷ đồng"; and amounts above one billion with groups of zeros

**Given** the style choice "lẻ" vs "linh"
**When** the converter is configured
**Then** "lẻ" is the default and the alternative is one setting, covered by a test

**Given** a negative amount
**When** it is converted
**Then** an `ArgumentOutOfRangeException` is thrown (money is never negative)

---

### Story 1.5: Spike: QuestPDF printing with Vietnamese fonts

`SP-01` · Size M (time-boxed) · Depends on: 1.1

As a developer,
I want to prove that PDF generation, preview and printing work offline with Vietnamese text on A4 and A5,
So that all 12 print templates rest on a validated approach.

**Acceptance Criteria:**

**Given** the QuestPDF Community licence terms
**When** the spike checks them against the unit (a government body, non-commercial use)
**Then** a short decision note in `docs/decisions/` records whether the Community licence applies, or falls back to the alternative (RDLC via ReportViewerCore.WinForms)

**Given** a sample document rendered with an embedded Unicode font (all Vietnamese diacritics, e.g. "Nguyễn Thị Ánh Tuyết – Biên nhận thu tiền gửi lưu ký")
**When** it is rendered to A4 portrait and A5
**Then** every glyph displays correctly on a machine with no extra fonts installed

**Given** the generated PDF
**When** it opens in an embedded WebView2 window in WinForms
**Then** the user can preview, print to the default printer, and save the PDF without Internet
**And** the note records the WebView2 Runtime offline-install requirement

**Given** the spike result
**When** it is reviewed
**Then** the `IReportRenderer` abstraction shape (input model → PDF bytes) is agreed for Story 4.4

---

### Story 1.6: Spike: Velopack updates from a LAN shared folder

`SP-02` · Size M (time-boxed) · Depends on: 1.1

As an administrator,
I want to confirm that workstations install and update the app from a shared folder on the LAN without Internet,
So that rolling out new versions takes one step and needs no visit to each machine.

**Acceptance Criteria:**

**Given** a self-contained build packaged with Velopack (`Setup.exe` plus a release feed)
**When** the release is placed in a UNC shared folder (e.g. `\\server\LuuKyCanTin\releases`)
**Then** a test workstation installs it from that folder with no .NET runtime pre-installed

**Given** a newer version published to the same folder
**When** the installed app starts
**Then** it detects the update, applies it, and restarts on the new version

**Given** the spike result
**When** it is reviewed
**Then** a note records the folder layout, the share permissions needed (read for workstations, write for the admin) and how the DB-version check (Story 1.2) interacts with updates; the full rollout is Story 7.4

---

### Story 1.7: Spike: Vietnamese diacritic-insensitive incremental search

`SP-03` · Size S (time-boxed) · Depends on: 1.2

As a custodial officer,
I want to type part of a name without diacritics and see matches immediately,
So that I can find a detainee among thousands in a second or two.

**Acceptance Criteria:**

**Given** a test table with 10,000 Vietnamese names under collation `Vietnamese_CI_AI` and an index on the name
**When** searching "nguyen van a" or "NGUYỄN VĂN A"
**Then** both return the same rows as "Nguyễn Văn A"

**Given** incremental search (results refresh while typing, with a ~300 ms debounce)
**When** the user types successive characters
**Then** each query returns the top 50 matches in under 300 ms on target hardware (the measurement is recorded)

**Given** the spike result
**When** it is reviewed
**Then** a note records the chosen query pattern (prefix vs contains, code vs name), the index definitions, and how stale queries are cancelled, for use in Story 3.2

---

### Story 1.8: Walking skeleton: sign in, register a detainee, post and print one receipt

`NEN-08` · Size L · Depends on: 1.2, 1.3, 1.4, 1.5

As an administrator,
I want to sign in, add one detainee, record a 500,000 đ receipt and print it,
So that the team sees one real business flow working through every layer before building out features.

**Acceptance Criteria:**

**Given** the seeded `admin` account (password hashed with salted PBKDF2 via `Rfc2898DeriveBytes.Pbkdf2`) and the tables `NguoiDung`, `ThongTinDonVi` (single row, `CHECK (Id = 1)`)
**When** the admin enters the correct password on the login form
**Then** the main shell opens and `ICurrentUser` is populated; a wrong password shows an error (full login rules come in Epic 2)

**Given** the `DoiTuong` table with its full spec columns (`MaSo` UQ, `HoTen`, `NamSinh`, `LoaiDoiTuong`, `NgayVao`, `BuongGiam`, `TrangThai`, `NgayRa`, `SoDuLuuKy decimal(18,0) DEFAULT 0 CHECK (>= 0)`)
**When** the admin adds the detainee "Nguyễn Văn A" through a minimal form
**Then** the detainee is saved with balance 0

**Given** the `ChungTuLuuKy` and `DemSoChungTu` tables and a thin `GhiSoLuuKyService` that supports receipts only
**When** the admin records a cash receipt of 500,000 đ from a relative and posts it
**Then** in one transaction the document number is allocated (`BNT`, current year), the `ChungTuLuuKy` row is inserted with posted status, the amount in words and a snapshot of the detainee name and type, and `DoiTuong.SoDuLuuKy` is increased by a conditional `UPDATE … WITH (UPDLOCK, ROWLOCK) OUTPUT deleted/inserted`
**And** the posting writes `SoDuTruoc = 0` and `SoDuSau = 500000` on the document and an audit row through the interceptor
**And** no code outside `GhiSoLuuKyService` writes `SoDuLuuKy`

**Given** the posted receipt
**When** the admin clicks Print
**Then** a PDF preview opens in WebView2 with the unit name and address from `ThongTinDonVi`, the document number, "Nguyễn Văn A", "500.000" and "Năm trăm nghìn đồng"

**Given** the integration test of this flow on LocalDB
**When** it finishes
**Then** a reconciliation assertion checks `SoDuLuuKy` = total posted receipts − total posted payouts (the first version of NEN-19)
