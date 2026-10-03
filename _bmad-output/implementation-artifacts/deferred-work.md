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
