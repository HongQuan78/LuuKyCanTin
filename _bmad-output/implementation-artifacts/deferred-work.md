# Deferred Work

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-8-walking-skeleton-first-printed-receipt.md`
  summary: The real WebView2 PDF preview/print/save window is not covered by any automated test.
  evidence: The repo has no WinForms/WebView2 rendering-test infrastructure (`PdfPreviewForm` is only reached through `ShowDialog`); `WalkingSkeletonTests` covers the renderer bytes/text, and the presenter tests substitute the view. The story's T10 manual walk is the planned check.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-8-walking-skeleton-first-printed-receipt.md`
  summary: The sign-in gate that opens the shell only on `DialogResult.OK` is not covered by any test.
  evidence: `Program.RunApplication` is composition-root UI code the repo does not unit-test; `LoginPresenterTests` cover the presenter with a substituted view only. The story's T10 interactive walk is where it is observed.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-2-secure-sign-in-password-policy-lockout.md`
  summary: The interactive login → forced change → shell → sign-out loop is not covered by any test.
  evidence: `ShellApplicationContext` swaps `LoginForm`, `DoiMatKhauForm` and a fresh `MainForm`; `DoiMatKhauPresenterTests` and `MainPresenterTests` cover the presenters with substituted views, and the integration tests cover the services. No test drives the actual forms.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-3-roles-permission-catalogue.md`
  summary: The "Vai trò" form (module × action checkbox grid) is not covered by any UI test.
  evidence: `VaiTroPresenterTests` substitute `IVaiTroView`; building the actual `DataGridView` rows/columns and reading ticked cells is only exercised by hand.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-4-user-accounts-role-assignment.md`
  summary: The real account forms (grid selection, checked role lists, clipboard copy) are covered only by presenter tests with substituted views.
  evidence: `AccountPresenterTests`, `CreateAccountPresenterTests` and `AccountRolesPresenterTests` substitute `IAccountView`, `ICreateAccountView` and `IAccountRolesView`; `AccountForm`, `CreateAccountForm`, `AccountRolesForm` and `TemporaryPasswordForm` are only exercised by hand.


## Deferred from: code review of 1-1-solution-skeleton-host-ci (2026-10-02)

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-1-solution-skeleton-host-ci.md`
  summary: The architecture test reads raw `.csproj` only, so references injected through `Directory.Build.props`/`Directory.Packages.props` (`GlobalPackageReference`), or any package marked `PrivateAssets="all"`, get past the "Domain has no NuGet package" rule.
  evidence: `ProjectReferenceRules.Check` parses the XML text it is given; `IsAnalyzerOnly` trusts `PrivateAssets="all"`. Nothing violates the rule today: the only shared reference is the BannedApiAnalyzers analyzer in `src/Directory.Build.props`.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-1-solution-skeleton-host-ci.md`
  summary: Nothing tests the composition root (the `App` section binds to `AppOptions`, the presenter attaches to the form).
  evidence: `Program.CreateHost` is untested; removing `Configure<AppOptions>` leaves every test green because the Designer text, the `AppOptions` default and `appsettings.json` all give the same title.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-1-solution-skeleton-host-ci.md`
  summary: The host is built but never started, so a future `IHostedService` (for example scheduled backup) would silently never run.
  evidence: `Program.Main` calls `CreateHost` and then `WinFormsApp.Run`, with no `host.Start()`/`StopAsync()`. No hosted service is registered yet.

## Deferred from: code review of 1-2-database-migrations-schema-version-enum-check (2026-10-03)

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-2-database-migrations-schema-version-enum-check.md`
  summary: A login or permission failure (SQL 18456/4060) may be reported as a schema-version mismatch instead of "cannot connect to the database" (unverified, medium if real).
  evidence: `SchemaVersionChecker` relies on `GetAppliedMigrationsAsync`, and EF's `SqlServerDatabaseCreator.Exists` treats some of these errors as "database does not exist", which yields an empty list and so a mismatch. To settle it, start a workstation with a wrong password and with a login lacking DB access, and see which message appears.

## Deferred from: code review of 1-3-iclock-audit-log-interceptor (2026-10-03)

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-3-iclock-audit-log-interceptor.md`
  summary: The "flagged but unchanged" value-comparer filter in `AuditInterceptor.GhiNhan` is never exercised by a test.
  evidence: `UpdateWithoutRealChange_WritesNoRow` assigns an equal value to a tracked entity, so `DetectChanges` leaves it `Unchanged` and `GhiNhan` never runs. No caller flags properties without changing them yet. Settle together with the detached-update decision.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-3-iclock-audit-log-interceptor.md`
  summary: No model test enforces the `IAuditable` invariants: a single integer key (otherwise `BanGhiId` is null) and `ICoTrangThaiHuy` on every voucher (otherwise a cancellation is logged as `Sua`).
  evidence: Every current `IAuditable` (`CanBo`, `NguoiDung`, `VaiTro`, `ChungTuLuuKy`) has an int key, and `ChungTuLuuKy` implements `ICoTrangThaiHuy`. Add the test when Epic 3 adds more vouchers.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-3-iclock-audit-log-interceptor.md`
  summary: CLAUDE.md doesn't mention the text-stored enum exception (`HanhDong` as `varchar` names; `HasConversion<string>()` must come before `HasEnumCheck`).
  evidence: CLAUDE.md says every enum is `: byte` with `HasEnumCheck`, which implies tinyint; a later developer could "fix" `HanhDong` back to tinyint.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-3-iclock-audit-log-interceptor.md`
  summary: `ExecuteUpdate`/`ExecuteDelete` on `NhatKyThaoTac` bypass the `SaveChanges` append-only guard, although AC 4 says "through EF".
  evidence: Accepted by a review decision until Epic 7 applies `DENY UPDATE, DELETE` on the table. No code calls them on the log today. Close this item when the DENY lands.

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-4-user-accounts-role-assignment.md`
  summary: The new 2.4 terms (TemporaryPassword, LastAdministratorGuard, TransactionIsolation, AccountRow) are not in the naming-conventions glossary.
  evidence: The convention requires new domain terms to be added in the same change; the review workflow defers findings whose fix edits a rules file.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-4-user-accounts-role-assignment.md`
  summary: The UniqueConstraintException to BusinessRuleException mapping in AccountService.CreateAsync and ReactivateAsync has no deterministic test.
  evidence: Forcing the database, rather than the service pre-check, to reject needs a timing-dependent concurrent create; the filtered index itself is covered by a raw-context test. A deterministic two-session harness would settle it.

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-5-permission-driven-shell-service-authorization.md`
  summary: A refused permission caught by a presenter is not logged, and the top-level refusal log has no service identity.
  evidence: T4 asks Serilog to record user, permission code and service, but permission denials are caught in the presenters and the global handler cannot know the calling service. A logging seam (decorator or checker callback) would settle it.

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-6-segregation-of-duties-policy.md`
  summary: The naming-conventions table has no Policy category or SeparationOfDuties glossary entry.
  evidence: The story names the class a policy while the convention only defines Service/Policy-less patterns; the fix edits a rules file.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-02/2-6-segregation-of-duties-policy.md`
  summary: GlobalExceptionHandler keeps a hand-maintained list of Domain business exceptions instead of a Domain marker interface.
  evidence: BusinessRuleException lives in Application, so Domain exceptions cannot derive from it; both Domain exceptions are recognized by an explicit type list whose only tie is a comment. A Domain marker interface would make a future business exception show correctly by construction.
