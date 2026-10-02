# SP-01 · Spike: QuestPDF printing with Vietnamese fonts

Time-boxed spike (Story 1.5, `SP-01`). Throw-away project — **not** in `LuuKyCanTin.slnx`, never
referenced from `src/`. The written outcome lives in `docs/decisions/0001-pdf-engine-questpdf.md`.

## What this proves

- Vietnamese text (all diacritics, `đĐ`, NFC and NFD) rendered offline with an **embedded** font —
  nothing depends on fonts installed on the workstation.
- A4 portrait, A5 portrait and A5 landscape of one receipt-shaped document (centred unit header
  block, amount in words, 3-column signature block, rotated semi-transparent "BẢN IN LẠI" watermark).
- Deterministic output: with a fixed `DocumentMetadata.CreationDate`, two renders are byte-identical
  (SHA-256 match) — the basis for golden-file snapshot tests (NEN-21).
- WebView2 preview / print / save fully offline.

## Run

```powershell
# Headless: renders 3 PDFs + PNG previews + determinism.txt into <outDir>, exits (verifiable without a UI)
dotnet run --project spikes/SP-01-QuestPdf -- --render C:\temp\sp01-out

# Interactive: WinForms form with WebView2 preview, Print (100 %), Save As…
dotnet run --project spikes/SP-01-QuestPdf
```

Exit code of `--render`: `0` = all three documents deterministic (hashes MATCH), `1` = mismatch,
`2` = render failed (or `--render` called without an outDir). PNG previews (one per page, 110 DPI,
`-p<N>` suffix) let the layout be reviewed without a PDF reader.

## Packages (versions declared directly — central package management is off for spikes)

| Package | Version |
|---|---|
| QuestPDF | 2026.9.1 (latest stable at spike time) |
| Microsoft.Web.WebView2 | 1.0.4258.31 |

## Layout

| Path | Purpose |
|---|---|
| `Program.cs` | Sets `QuestPDF.Settings.License = LicenseType.Community` (evaluation); dispatches `--render` vs the form |
| `SampleReportGenerator.cs` | Registers embedded fonts; composes the sample document (header/body/signature/watermark) |
| `HeadlessRenderer.cs` | `--render` mode: writes PDFs + `determinism.txt`, returns exit code |
| `PreviewForm.cs` | WebView2 preview (`file:///`), `CoreWebView2.PrintAsync` at scale 1.0, Save As… dialog |
| `Fonts/` | Tinos Regular/Bold/Italic TTFs (Apache 2.0) as `EmbeddedResource` + the Apache 2.0 `LICENSE` |

## Notes

- Fonts: **Tinos** (Apache 2.0), metric-compatible with Times New Roman — matches Vietnamese
  government paper forms. Downloaded from `https://github.com/google/fonts/raw/main/ofl/tinos/`.
  QuestPDF has no system-font fallback: a missing glyph throws `DocumentComposeException`, so
  "looks right only on the dev machine" cannot happen silently.
- Preview file path: `%LOCALAPPDATA%\LuuKyCanTin\preview\`; WebView2 user-data:
  `%LOCALAPPDATA%\LuuKyCanTin\WebView2`; stale preview PDFs deleted at startup and on close.
- Print: programmatic `CoreWebView2.PrintAsync` with `ScaleFactor = 1.0` (100 % actual size,
  not fit). The PDF viewer's own toolbar print button also works — verify on the target printer.
- Save: programmatic `SaveFileDialog` writing the bytes (recommended over the viewer's download
  button).
- Offline: everything is local. The only machine prerequisite is the **WebView2 Runtime**
  (Evergreen via Windows Update/admin, or the offline standalone installer
  `MicrosoftEdgeWebView2RuntimeInstallerX64.exe`; fixed-version distribution is the fallback).
- NFC vs NFD: both render identically; normalize input to NFC (`FormC`) before rendering.
- Licence: Community licence v3.0 does **not** cover production use by this unit (public sector) —
  the spike is evaluation only. See the ADR; the PO must decide (paid QuestPDF licence vs RDLC).
- Known benign build warning: MSB3277 "WindowsBase conflict" comes from the WebView2 package's
  WPF reference; it is a warning only, the spike builds and runs fine.
- QuestPDF 2026.9 API notes: `FontManager` moved to `QuestPDF.Drawing` (`RegisterFontFromStream`);
  metadata is set via `Document.WithMetadata(...)`; the watermark layer is `page.Foreground()`
  (the old `page.Layer()`/`Canvas` API is gone); multi-span text uses
  `text.Span("...").Style(TextStyle.Default.Bold())`.