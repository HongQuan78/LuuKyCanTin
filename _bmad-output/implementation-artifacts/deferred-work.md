# Deferred Work

- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-8-walking-skeleton-first-printed-receipt.md`
  summary: The real WebView2 PDF preview/print/save window is not covered by any automated test.
  evidence: The repo has no WinForms/WebView2 rendering-test infrastructure (`PdfPreviewForm` is only reached through `ShowDialog`); `WalkingSkeletonTests` covers the renderer bytes/text, and the presenter tests substitute the view. The story's T10 manual walk is the planned check.
- source_spec: `_bmad-output/implementation-artifacts/stories/epic-01/1-8-walking-skeleton-first-printed-receipt.md`
  summary: The sign-in gate that opens the shell only on `DialogResult.OK` is not covered by any test.
  evidence: `Program.RunApplication` is composition-root UI code the repo does not unit-test; `LoginPresenterTests` cover the presenter with a substituted view only. The story's T10 interactive walk is where it is observed.
