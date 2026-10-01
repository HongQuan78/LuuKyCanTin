---
story: "1.1"
epic: 1
title: Solution skeleton, host and CI
status: review
size: M
backlogItems: [NEN-01, NEN-07]
frsCovered: []
nfrsTouched: [NFR9, NFR11, NFR12, NFR14]
uxDrs: [UX-DR10]
dependsOn: []
---

# Story 1.1: Solution skeleton, host and CI

Status: review

## Story

As a developer,
I want a buildable solution with the agreed layers, a Generic Host and a CI pipeline,
So that every later story starts from the same structure and is checked automatically.

## Acceptance Criteria

1. **Given** a clean clone of the repository
   **When** I run `dotnet build LuuKyCanTin.slnx` and `dotnet test LuuKyCanTin.slnx`
   **Then** both succeed with the projects `LuuKyCanTin.Domain`, `.Application`, `.Infrastructure` (under `src/Libraries`), `.WinForms` (under `src/Presentation`), and the test projects `Domain.UnitTests`, `Application.UnitTests`, `IntegrationTests`
   **And** each source project has the module folders `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/`, `Common/` (Application also has `Abstractions/`)

2. **Given** the project references
   **When** an architecture test inspects the assemblies
   **Then** Domain references no project and no NuGet package; Application references only Domain (plus FluentValidation); Infrastructure references Application and Domain; WinForms references Application and Infrastructure
   **And** the test fails if Application references EF Core SqlServer, QuestPDF, ClosedXML or System.Windows.Forms

3. **Given** the WinForms app starts
   **When** `Program.cs` runs
   **Then** it builds a Generic Host with DI, `appsettings.json` configuration and Serilog file logging to `%ProgramData%\LuuKyCanTin\logs` (rolling daily, 30 files kept)
   **And** a global handler catches unhandled UI-thread and background exceptions, logs them, and shows a friendly Vietnamese message without a stack trace (UX-DR10)

4. **Given** a sample Presenter with a View interface
   **When** its unit test runs
   **Then** the Presenter is tested with an NSubstitute fake View and no Form, which documents the MVP convention (Views are passive, Presenters call Application services, services never reference `MessageBox`)

5. **Given** a push to `master`
   **When** the CI pipeline runs on a Windows agent
   **Then** it restores, builds and runs all tests, including LocalDB integration tests, and fails the build on any failure

## Tasks / Subtasks

- [x] **T1. Shared build settings** (AC: 1)
  - [x] Add `Directory.Build.props` at the repo root: `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `LangVersion=latest`. Remove the duplicated properties from each `.csproj`.
  - [x] Add `Directory.Packages.props` (Central Package Management) and move the test package versions out of the two existing test `.csproj` files.
  - [x] Add `global.json` pinning SDK `10.0.x` with `rollForward: latestFeature` (the local SDK is 10.0.201).
- [x] **T2. Module folders** (AC: 1)
  - [x] Create `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/`, `Common/` in Domain, Application, Infrastructure and WinForms. Add `Abstractions/` in Application. Use a `.gitkeep` in folders that stay empty, so git keeps them.
  - [x] Infrastructure also gets `Persistence/`, `Reports/`, `Excel/` and `Backup/` (from the tech stack doc). WinForms gets `Shell/` and `Controls/`.
- [x] **T3. Test projects** (AC: 1, 4)
  - [x] Create `tests/LuuKyCanTin.IntegrationTests` (xUnit + Shouldly, net10.0). It references Domain, Application and Infrastructure. Add it to `LuuKyCanTin.slnx` under `/tests/`.
  - [x] Create `tests/LuuKyCanTin.WinForms.UnitTests` (net10.0-windows) for Presenter tests. See the Dev Notes on why a 4th test project is needed.
  - [x] Add one placeholder test to IntegrationTests that opens a LocalDB connection (`(localdb)\MSSQLLocalDB`) and runs `SELECT 1`. This proves that CI can reach LocalDB before Story 1.2 needs it.
- [x] **T4. Architecture tests** (AC: 2)
  - [x] In `IntegrationTests/Architecture/`, add `ProjectReferenceTests`. It parses each `src/**/*.csproj` (XML) for `ProjectReference` and `PackageReference`, and asserts the allowed graph from AC 2.
  - [x] Also assert on `typeof(X).Assembly.GetReferencedAssemblies()` for the forbidden names (`Microsoft.EntityFrameworkCore.SqlServer`, `QuestPDF`, `ClosedXML`, `System.Windows.Forms`) in Domain and Application.
  - [x] Ignore analyzer-only packages (`PrivateAssets="all"`) when checking "Domain has no NuGet package". Story 1.3 may add an analyzer.
  - [x] Add a negative check: the test helper fails on a fixture csproj string that contains a forbidden reference, so nobody can quietly turn the test into a no-op.
- [x] **T5. Generic Host in `Program.cs`** (AC: 3)
  - [x] Delete `Form1.cs` / `Form1.Designer.cs`. Add `Shell/MainForm` (an empty shell window titled "Lưu ký – Căn tin").
  - [x] `Program.Main`: `ApplicationConfiguration.Initialize()`, then `Host.CreateApplicationBuilder(args)`, then `appsettings.json` (copied to output), then Serilog, then the DI registrations (`AddApplication()` and `AddInfrastructure(config)` extension methods, empty for now), then build, then resolve `MainForm`, then `Application.Run`.
  - [x] Serilog: `Serilog.Extensions.Hosting` + `Serilog.Sinks.File` + `Serilog.Settings.Configuration`. Path `%ProgramData%\LuuKyCanTin\logs\log-.txt`, `rollingInterval: Day`, `retainedFileCountLimit: 30`. Expand `%ProgramData%` with `Environment.GetFolderPath(SpecialFolder.CommonApplicationData)`. Don't rely on Serilog to expand it.
  - [x] Call `Log.CloseAndFlush()` in `finally`.
- [x] **T6. Global exception handler** (AC: 3)
  - [x] Call `Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)` **before** any form is created. Subscribe to `Application.ThreadException`, `AppDomain.CurrentDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException`.
  - [x] Each handler logs the full exception at `Fatal`/`Error`. Then it shows `MessageBox` with a friendly Vietnamese text, for example: "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại hoặc liên hệ quản trị viên. Chi tiết đã được ghi vào nhật ký." Never show the stack trace.
  - [x] Put this code in `WinForms/Common/GlobalExceptionHandler.cs`. It's the **only** place outside Forms that may call `MessageBox`.
- [x] **T7. Sample MVP Presenter** (AC: 4)
  - [x] In WinForms `Shell/`: `IMainView` (an event `Loaded`, a property `string TieuDe { set; }`), and `MainPresenter`, which sets the title from an injected `IOptions<AppOptions>` or a tiny Application service.
  - [x] `MainForm : Form, IMainView` only raises events and sets controls. It has no logic.
  - [x] Test in `WinForms.UnitTests/Shell/MainPresenterTests.cs`: `Substitute.For<IMainView>()`, raise `Loaded`, assert that `TieuDe` was set. The test creates no Form.
- [x] **T8. CI pipeline** (AC: 5)
  - [x] Add `.github/workflows/ci.yml`: trigger on `push` to `master` and on `pull_request`, `runs-on: windows-latest`, `actions/setup-dotnet` with `global-json-file: global.json`.
  - [x] Steps: start LocalDB (`sqllocaldb start MSSQLLocalDB`), `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet test --no-build -c Release --logger trx`. Upload the test results as an artifact.
- [x] **T9. Verify** (AC: 1–5)
  - [x] Run `dotnet build LuuKyCanTin.slnx` and `dotnet test LuuKyCanTin.slnx` locally and confirm both pass. Run the app and check that a log file appears under `%ProgramData%\LuuKyCanTin\logs`.
  - [x] Temporarily throw from a button and check the friendly message plus the log entry, then remove the throw.

## Dev Notes

### Current codebase state (verified 2026-10-01)

- `LuuKyCanTin.slnx` already contains Domain, Application and Infrastructure (`src/Libraries`), WinForms (`src/Presentation`), and the test projects `Domain.UnitTests` and `Application.UnitTests`. **IntegrationTests is missing.**
- Every project is a bare template. WinForms has `Form1` and a template `Program.cs`. There are no module folders, no packages in the source projects, and no `.github/`.
- The existing project references already match AC 2: Application→Domain; Infrastructure→Application, Domain; WinForms→Application, Infrastructure.
- Test packages already in use: xunit 2.9.3, xunit.runner.visualstudio 3.1.4, NSubstitute 6.2.0, Shouldly 4.3.0, Microsoft.NET.Test.Sdk 17.14.1, coverlet.collector 6.0.4. Keep these versions when you move them to central package management.
- Git remote: GitHub (`HongQuan78/LuuKyCanTin`), so CI uses **GitHub Actions**. The team commits directly to `master`.

### Decisions and deviations to know about

- **⚠ Application and EF Core, a conflict to resolve before Story 1.8.** AC 2 says that Application references *only* Domain plus FluentValidation. CLAUDE.md, however, puts `IAppDbContext` in Application, and that interface normally exposes `DbSet<T>`, which needs the `Microsoft.EntityFrameworkCore` package (core, not the SqlServer provider). The recommendation is to allow `Microsoft.EntityFrameworkCore` (core only) in Application and keep the SqlServer provider forbidden. That matches CLAUDE.md ("no EF-provider"). Encode this in the architecture test, and ask the PO to amend the wording of AC 2. If the PO keeps the strict wording, Application has to talk to persistence only through narrow ports, which changes Stories 1.3 and 1.8. Implement this story strictly for now, because Application has no EF need yet, but write the allow-list as one constant so the change is one line.
- **4th test project.** Presenters live in the WinForms project (`net10.0-windows`). A `net10.0` test project can't reference it. AC 1 lists the minimum test projects, and adding `LuuKyCanTin.WinForms.UnitTests` doesn't break it.
- **Where the architecture test lives.** IntegrationTests already references all three library projects, so the test goes there. No separate project is needed (KISS).

### Architecture constraints (CLAUDE.md)

- Dependencies point inward only. WinForms references Infrastructure **only** for DI composition, so Forms and Presenters must never use Infrastructure types.
- Services never use `MessageBox`. Forms never hold a `DbContext`. Each operation creates a new DI scope. That last rule matters from Story 1.8, but register Forms and Presenters as `Transient` now.
- Tests use xUnit + NSubstitute + Shouldly. **Never use FluentAssertions** (commercial licence, NFR14).

### Gotchas

- `Application` is ambiguous in WinForms: it's both `System.Windows.Forms.Application` and the namespace `LuuKyCanTin.Application`. Use a `using WinFormsApp = System.Windows.Forms.Application;` alias in `Program.cs`, or fully qualify it.
- `SetUnhandledExceptionMode` throws if a window already exists. Call it first.
- `appsettings.json` must have `CopyToOutputDirectory=PreserveNewest`, and the host must set `ContentRootPath = AppContext.BaseDirectory`, so the app works when started from a shortcut whose working directory is elsewhere.
- Writing to `%ProgramData%` works for standard users once the folder exists, but create it with `Directory.CreateDirectory` at startup.
- The `windows-latest` GitHub runner has SQL Server LocalDB installed. Start the instance explicitly before the tests run.

### Out of scope

- Database, EF Core and migrations (Story 1.2). `IClock` and audit (1.3). Login (1.8). Velopack packaging (spike 1.6). Real menus (Epic 2).

### Testing

- `WinForms.UnitTests`: Presenter test with an NSubstitute View.
- `IntegrationTests/Architecture`: reference-graph tests, plus the LocalDB smoke test.
- The Domain and Application test projects may contain only a trivial test until later stories add real ones. `dotnet test` must not fail with "no tests found".

### References

- Epic: `_bmad-output/planning-artifacts/epics/epic-01-foundation-walking-skeleton.md` (Story 1.1)
- Requirements: `_bmad-output/planning-artifacts/epics.md`: Additional Requirements › Starter / solution setup; NFR11, NFR12, NFR14; UX-DR10
- Tech stack: `document/Tiếp nhận tiền gửi lưu ký — Tech Stack (WinForms .NET 10).pdf` (solution structure, logging, packages)
- `CLAUDE.md`: Architecture, Engineering rules

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5[1m]). Implemented directly, without the `bmad-build` workflow, because `uv` is not installed locally.

### Debug Log References

- Build failed with `WFO1000` (WinForms analyzer, as error under `TreatWarningsAsErrors`) on the write-only `MainForm.TieuDe`. Fixed with `[DesignerSerializationVisibility(Hidden)]`.
- T9 manual check: the app started from `%TEMP%` and showed the title "Lưu ký – Căn tin". `C:\ProgramData\LuuKyCanTin\logs\log-20261001.txt` was created. A temporary timer throw produced an `[ERR] Unhandled exception on the UI thread` entry with the full stack trace, plus a "Lỗi" dialog with the friendly text. The app kept running. The throw was then removed.

### Completion Notes List

- **AC 1–4 verified locally.** Debug and Release builds: 0 warnings, 0 errors. Tests: 23 passed, 1 skipped (the LocalDB smoke test).
- **AC 5 is not yet verified.** It needs the first push to `master` so `.github/workflows/ci.yml` runs on `windows-latest`.
- **LocalDB smoke test.** LocalDB is not installed on the dev machine. `[LocalDbFact]` skips the test locally when LocalDB is absent, and never skips when the `CI` env var is set. GitHub Actions sets `CI`, so on the runner a missing or broken LocalDB fails the build.
- **⚠ Deviation from AC 2, for the PO to confirm.** Application references `Microsoft.Extensions.DependencyInjection.Abstractions` so that `AddApplication()` can be an `IServiceCollection` extension, as T5 asks. FluentValidation's DI package pulls this in transitively anyway. The allow-list is one constant: `ProjectReferenceRules.ApplicationAllowedPackages`. The EF Core core decision from the Dev Notes goes in the same place.
- **Unobserved task exceptions** are raised on the finalizer thread. The handler logs them, marks them observed, and posts the friendly message to the open form's UI thread. It does not block the finalizer.
- **Placeholder tests.** Domain.UnitTests and Application.UnitTests each have one placeholder test, so `dotnet test` doesn't report "No test is available".
- Forms and Presenters are created per use. `MainForm` is registered `Transient`. `MainPresenter` is created with `ActivatorUtilities`, binding the form as its view.

### File List

- `global.json` (new)
- `Directory.Build.props` (new)
- `Directory.Packages.props` (new)
- `.github/workflows/ci.yml` (new)
- `LuuKyCanTin.slnx` (modified)
- `src/Libraries/LuuKyCanTin.Domain/LuuKyCanTin.Domain.csproj` (modified)
- `src/Libraries/LuuKyCanTin.Application/LuuKyCanTin.Application.csproj` (modified)
- `src/Libraries/LuuKyCanTin.Application/DependencyInjection.cs` (new)
- `src/Libraries/LuuKyCanTin.Infrastructure/LuuKyCanTin.Infrastructure.csproj` (modified)
- `src/Libraries/LuuKyCanTin.Infrastructure/DependencyInjection.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/LuuKyCanTin.WinForms.csproj` (modified)
- `src/Presentation/LuuKyCanTin.WinForms/Program.cs` (modified)
- `src/Presentation/LuuKyCanTin.WinForms/appsettings.json` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Common/AppOptions.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Common/GlobalExceptionHandler.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Shell/IMainView.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Shell/MainPresenter.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Shell/MainForm.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Shell/MainForm.Designer.cs` (new)
- `src/Presentation/LuuKyCanTin.WinForms/Form1.cs` and `Form1.Designer.cs` (deleted)
- `src/**/{LuuKy,HangHoa,DanhMuc,HeThong,BaoCao,Common,Abstractions,Persistence,Reports,Excel,Backup,Controls}/.gitkeep` (new)
- `tests/LuuKyCanTin.Domain.UnitTests/LuuKyCanTin.Domain.UnitTests.csproj` (modified)
- `tests/LuuKyCanTin.Domain.UnitTests/DomainAssemblyTests.cs` (new)
- `tests/LuuKyCanTin.Application.UnitTests/LuuKyCanTin.Application.UnitTests.csproj` (modified)
- `tests/LuuKyCanTin.Application.UnitTests/DependencyInjectionTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/LuuKyCanTin.IntegrationTests.csproj` (new)
- `tests/LuuKyCanTin.IntegrationTests/Architecture/ProjectReferenceRules.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Architecture/ProjectReferenceTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Architecture/ProjectReferenceRulesTests.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Common/RepositoryPaths.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Common/LocalDbFactAttribute.cs` (new)
- `tests/LuuKyCanTin.IntegrationTests/Persistence/LocalDbSmokeTests.cs` (new)
- `tests/LuuKyCanTin.WinForms.UnitTests/LuuKyCanTin.WinForms.UnitTests.csproj` (new)
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/MainPresenterTests.cs` (new)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-01 | Implemented T1–T9; status → review (AC 5 pending first CI run) |
