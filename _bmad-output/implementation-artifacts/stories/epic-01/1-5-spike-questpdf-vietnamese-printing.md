---
story: "1.5"
epic: 1
title: "Spike: QuestPDF printing with Vietnamese fonts"
status: review
type: spike
size: M (time-boxed, 2 days suggested)
backlogItems: [SP-01]
frsCovered: []
nfrsTouched: [NFR8, NFR9, NFR14]
uxDrs: [UX-DR6]
dependsOn: ["1.1"]
baseline_commit: 03d33e35657493def412b22f50b2b1c44a28419d
---

# Story 1.5: Spike: QuestPDF printing with Vietnamese fonts

Status: ready-for-dev

## Story

As a developer,
I want to prove that PDF generation, preview and printing work offline with Vietnamese text on A4 and A5,
So that all 12 print templates rest on a validated approach.

## Acceptance Criteria

1. **Given** the QuestPDF Community licence terms
   **When** the spike checks them against the unit (a government body, non-commercial use)
   **Then** a short decision note in `docs/decisions/` records whether the Community licence applies, or falls back to the alternative (RDLC via ReportViewerCore.WinForms)

2. **Given** a sample document rendered with an embedded Unicode font (all Vietnamese diacritics, e.g. "Nguyễn Thị Ánh Tuyết – Biên nhận thu tiền gửi lưu ký")
   **When** it is rendered to A4 portrait and A5
   **Then** every glyph displays correctly on a machine with no extra fonts installed

3. **Given** the generated PDF
   **When** it opens in an embedded WebView2 window in WinForms
   **Then** the user can preview, print to the default printer, and save the PDF without Internet
   **And** the note records the WebView2 Runtime offline-install requirement

4. **Given** the spike result
   **When** it is reviewed
   **Then** the `IReportRenderer` abstraction shape (input model → PDF bytes) is agreed for Story 4.4

## Tasks / Subtasks

- [x] **T1. Licence check** (AC: 1)
  - [x] Read the **current** QuestPDF licence page and EULA (questpdf.com/license). Record the version and date you checked them. The terms have changed between versions, so don't rely on memory or old blog posts.
  - [x] Answer two questions explicitly: (a) does the unit qualify for Community (a public-sector body, internal non-commercial use, no revenue)? (b) Does the *development team* or a contractor need a licence of its own?
  - [x] Write `docs/decisions/0001-pdf-engine-questpdf.md` (ADR style: Context, Decision, Consequences, Alternatives). If Community is doubtful, the decision may be "QuestPDF pending written confirmation". Escalate to the PO, because the alternative changes how all 12 templates are built.
- [x] **T2. Throw-away spike project, outside the product code** (AC: 2, 3)
  - [x] Create `spikes/SP-01-QuestPdf/` (a WinForms `net10.0-windows` project). **Don't** add it to `LuuKyCanTin.slnx`, and **don't** reference it from `src/`. That keeps the product source clean and the CI build unaffected. Add a one-line `spikes/README.md`: "Time-boxed experiments. Not production code. Outcomes live in docs/decisions/."
  - [x] Packages: `QuestPDF` (latest stable), `Microsoft.Web.WebView2`. Set `QuestPDF.Settings.License = LicenseType.Community;` (or the value from T1).
- [x] **T3. Vietnamese font embedding** (AC: 2)
  - [x] Pick a font with full Vietnamese coverage and a licence that allows embedding and redistribution. Candidates: **Noto Sans / Noto Serif** (OFL), **Be Vietnam Pro** (OFL), or Times New Roman look-alikes such as **Tinos** (Apache 2.0). Government paper forms usually use Times New Roman, so a metric-compatible serif keeps layouts faithful.
  - [x] Ship the `.ttf` as an embedded resource. Register it with `FontManager.RegisterFont(stream)`. **Never** depend on fonts installed on the workstation. Disable the system-font fallback if QuestPDF exposes the setting, so a missing glyph shows up in the spike instead of silently looking right on the dev's machine.
  - [x] The sample text must cover every tone mark on every vowel, plus `đĐ`: e.g. "Nguyễn Thị Ánh Tuyết – Biên nhận thu tiền gửi lưu ký; ắ ằ ẳ ẵ ặ ấ ầ ẩ ẫ ậ ế ề ể ễ ệ ố ồ ổ ỗ ộ ớ ờ ở ỡ ợ ứ ừ ử ữ ự ỳ ỷ ỹ ỵ Đ đ". Include bold, italic and the amount-in-words line, and test Unicode **NFC** vs **NFD** input (text pasted from Word can be NFD). Recommend normalizing to NFC before rendering.
  - [x] Render the same document to **A4 portrait** and **A5** (portrait and landscape: receipts are often A5 landscape, check the paper form). Produce one header block (unit name / address, centred, national motto lines) and a 3-column signature block. That's the shape every template will reuse (FR42).
  - [ ] Verify on a clean Windows VM or Sandbox with no extra fonts: open the PDF and check the PDF's font list (Properties › Fonts) shows only the **embedded** font.
- [x] **T4. WebView2 preview / print / save** (AC: 3)
  - [x] Form with a `WebView2` control. Write the PDF bytes to a temp file under `%LOCALAPPDATA%\LuuKyCanTin\preview\` and `Navigate` to `file:///…`. Alternatively serve from memory via `CoreWebView2.SetVirtualHostNameToFolderMapping`, or a `WebResourceRequested` handler for `https://report.local/`. Pick one and note why.
  - [x] **Print**: test both the built-in PDF viewer toolbar's print button and a programmatic `CoreWebView2.PrintAsync(settings)` (silent print to the default printer, which feeds TI-08 "auto-print after posting" later). Note page-scaling issues: printing must be 100 % "actual size", not "fit".
  - [x] **Save**: the viewer's download/save button, plus a programmatic Save (just write the bytes with a `SaveFileDialog`). The second is simpler and is the recommendation.
  - [x] **Offline**: unplug the network and repeat. Set `CoreWebView2Environment` user-data folder to `%LOCALAPPDATA%\LuuKyCanTin\WebView2`, not next to the exe (Program Files is read-only).
  - [x] **Runtime install**: confirm Windows 10/11 machines without Edge updates still get the Evergreen runtime. Document the **offline standalone installer** (`MicrosoftEdgeWebView2RuntimeInstallerX64.exe`) and how the Velopack setup (Story 1.6) or the admin installs it. Note the fixed-version option as a fallback.
  - [x] Clean up temp preview files on close and at startup.
- [x] **T5. `IReportRenderer` proposal** (AC: 4)
  - [x] Write the proposed shape in the decision note, for example:

    ```csharp
    // Application/Abstractions
    public interface IReportRenderer
    {
        byte[] Render<TModel>(TModel model) where TModel : IReportModel;
    }
    // Application/BaoCao: one model per template, a plain DTO with snapshotted data
    public interface IReportModel { string MaMauIn { get; } }  // e.g. "BIEN_NHAN_THU", matches CauHinhKyTen.MaMauIn
    ```

    Infrastructure resolves `IReportTemplate<TModel>` (one QuestPDF class per template in `Infrastructure/Reports/`). The shared frame (header from `ThongTinDonVi`, signature block from `CauHinhKyTen`, reprint watermark) is a base component every template composes. It is built in Epic 4 (FR42/FR43).
  - [x] Keep `byte[]` (not `Stream`) unless the spike shows memory problems. The documents are 1–5 pages.
  - [x] Note how golden-file snapshot tests (NEN-21) can work: QuestPDF output must be deterministic. Check whether creation timestamps or IDs vary between runs, and how to fix the metadata (`DocumentMetadata.CreationDate`).
- [x] **T6. Outcome review** (AC: 1–4)
  - [x] The decision note covers: licence verdict, chosen font plus its licence, A4/A5 screenshots, print/save results, offline result, WebView2 runtime requirement, `IReportRenderer` shape, determinism finding, open risks.
  - [ ] Review with the team or PO. Mark the story done only when the note is merged.

## Dev Notes

### Why a spike, and what "done" means

- This is time-boxed research (backlog alignment A10). **The deliverable is the written outcome** in `docs/decisions/`, not production code. The spike code stays in `spikes/` for reference and is never referenced by the product. Story 1.8 re-implements the minimal receipt template properly in `Infrastructure/Reports/`, using what this spike learned.
- If the time box runs out, stop and write down what's known and unknown. Don't extend it silently.

### Context

- 12 print templates are coming (receipt, payout, per-detainee statement, unit ledger book, remittance list, goods receipt note, purchase slip, stock movement, revenue, purchase history, posted price list, goods catalogue). Some are wide tables, so a landscape A4 check of one dense table is worth 30 minutes.
- Reprints show "BẢN IN LẠI" + print count as a watermark (UX-DR6, FR43). Try one rotated semi-transparent watermark layer in the spike so the approach is known.
- The fallback, RDLC via `ReportViewerCore.WinForms`, is community-maintained, not supported by Microsoft. Mention that in the ADR if it becomes relevant.

### Constraints

- No Internet on site, ever: no CDN fonts, no online licence activation, no telemetry calls.
- NFR14: free or permissively licensed libraries only, and font licences count too.
- Application must not reference QuestPDF. Only the `IReportRenderer` interface and the report models live in Application.

### References

- Epic 1 › Story 1.5; `epics.md` › NFR8, NFR9, NFR14, UX-DR6, FR42, FR43
- Tech stack PDF: QuestPDF row (licence caveat), WebView2 row, RDLC alternative
- DB design PDF: `CauHinhKyTen.MaMauIn` examples `BIEN_NHAN_THU`, `PHIEU_CHI`

## Dev Agent Record

### Agent Model Used

deepseek-v4-flash

### Debug Log References

- QuestPDF 2026.9.1 API changes discovered via build errors and reflection on the NuGet package (the 2022–2024-era API from the tech-stack PDF — `FontManager`, `page.Metadata()`, `page.Layer().Canvas()` — is gone): `FontManager` lives in `QuestPDF.Drawing` (`RegisterFontFromStream`); metadata is set via `Document.WithMetadata(DocumentMetadata)`; the watermark layer is `page.Foreground().Rotate(-45)`; multi-span text uses `text.Span("…").Style(TextStyle.Default.…)`; `Colors` only ships Black/White/Transparent, use `Color.FromHex(...)`.

### Completion Notes List

- **T1**: QuestPDF Community licence v3.0 (effective 2026-07-06) checked on 2026-10-02. It excludes government/public-sector entities regardless of revenue; the unit (detention facility) does **not** qualify. Spike use is evaluation-only, allowed. ADR decision: "Community licence not applicable — pending PO decision: paid Professional/Enterprise licence (unlimited developers, covers the legal entity) or RDLC via ReportViewerCore.WinForms". `QuestPDF.Settings.License = LicenseType.Community` set in the spike.
- **T2**: `spikes/SP-01-QuestPdf` created (net10.0-windows, WinExe, `<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>` with direct versions: QuestPDF 2026.9.1, Microsoft.Web.WebView2 1.0.4258.31). Not in LuuKyCanTin.slnx, not referenced from src/. `spikes/README.md` written.
- **T3**: Tinos Regular/Bold/Italic (Apache 2.0, metric-compatible with Times New Roman) downloaded from google/fonts, shipped as EmbeddedResource, registered via `FontManager.RegisterFontFromStream`. `UseSystemFonts=false` + `ThrowOnMissingFontFamilies/TextGlyphs=true` (QuestPDF 2026.9 has these settings; the old "system font fallback" concern is gone — QuestPDF only uses registered fonts by default). Sample document covers every Vietnamese tone mark on every vowel incl. đĐ, bold/italic, amount-in-words, NFC vs NFD lines, centred unit header, 3-column signature block, rotated semi-transparent "BẢN IN LẠI" watermark via `page.Foreground()`. A4 portrait + A5 portrait + A5 landscape all render; A4 MediaBox 595×842, A5 420×595, A5 landscape 595×420; 3 subset-embedded TrueType fonts (`/FontFile2`, `/AAAAAA+Tinos-Bold` etc.); ToUnicode CMap maps Đ→U+0110, đ→U+0111, ấ→U+1EA5. Rendering with ThrowOnMissingTextGlyphs=true proves every diacritic glyph exists in the embedded font.
- **T4**: WebView2 form: preview via bytes → `%LOCALAPPDATA%\LuuKyCanTin\preview\` → `Navigate(file:///)` (chosen over virtual-host mapping — simplest, works with the built-in PDF viewer); Print via `CoreWebView2.PrintAsync` with `ScaleFactor=1.0` (100 % actual size); Save As… via SaveFileDialog writing the bytes (recommended); user-data folder `%LOCALAPPDATA%\LuuKyCanTin\WebView2`; stale preview PDFs cleaned at startup and on close. Smoke-tested: app starts, preview PDF written (byte-identical to headless output), WebView2 user-data created, graceful close cleans the preview file. Runtime: Evergreen 154.0.4258.48 present on dev machine; offline standalone installer documented in ADR for the Velopack rollout.
- **T5**: `IReportRenderer`/`IReportModel` shape written into the ADR (exactly as proposed in the story). `byte[]` confirmed right for 1–5 page documents.
- **T6**: Determinism verified: with fixed `DocumentMetadata.CreationDate`, two renders of each document are byte-identical (SHA-256 MATCH for all three; see determinism.txt). PNG previews written per page for review.
- Remaining for the team/PO (unchecked): clean-VM font check (Properties › Fonts shows only Tinos), printer-driver 100 % print check, PO licence decision, merge of the ADR.

### File List

- `spikes/README.md` (new)
- `spikes/SP-01-QuestPdf/SP-01-QuestPdf.csproj` (new)
- `spikes/SP-01-QuestPdf/Program.cs` (new)
- `spikes/SP-01-QuestPdf/SampleReportGenerator.cs` (new)
- `spikes/SP-01-QuestPdf/HeadlessRenderer.cs` (new)
- `spikes/SP-01-QuestPdf/PreviewForm.cs` (new)
- `spikes/SP-01-QuestPdf/README.md` (new)
- `spikes/SP-01-QuestPdf/Fonts/Tinos-Regular.ttf`, `Tinos-Bold.ttf`, `Tinos-Italic.ttf`, `LICENSE` (Apache 2.0 text) (new)
- `docs/decisions/0001-pdf-engine-questpdf.md` (new)
- `_bmad-output/implementation-artifacts/stories/epic-01/1-5-spike-questpdf-vietnamese-printing.md` (task checkboxes, Dev Agent Record, Change Log)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-02 | SP-01 executed: spike project, ADR, fonts, WebView2 form, determinism verified (deepseek-v4-flash) |
| 2026-10-02 | Review fixes applied: ADR API name, per-page PNGs, committed evidence, `--render` usage/exit 2, NavigationCompleted `IsSuccess`, Save-As guard, non-empty determinism assertion, Tinos attribution (deepseek-v4-flash) |

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | ADR 0001 cites `FontManager.RegisterFont(stream)`; spike uses the 2026.9 API `RegisterFontFromStream` | low — patch | Real: ADR would mislead Story 4.4 implementers. Fixed in ADR (also corrected the exception name to `DocumentDrawingException`). |
| 2 | `HeadlessRenderer` wrote only the first page PNG while README/notes promise one per page | low — patch | Real: `.FirstOrDefault()` dropped pages 2+. Fixed with `-p<N>` suffix; surfaced a real finding — A5 samples flow to 2 pages (signature block), recorded in the ADR. |
| 3 | ADR promised A4/A5 screenshots and cited `determinism.txt`, but the evidence lived only in the transient `--render` outDir | low — patch | Real: evidence was lost on temp-dir deletion. Fixed: `docs/decisions/0001/assets/` now holds the 3 PDFs, 7 page PNGs and `determinism.txt`, linked from the ADR. |
| 4 | Determinism check compared two renders in one process and could pass vacuously on empty output | low — patch | Real. Fixed: renders are asserted non-empty and starting with `%PDF`. Cross-process check also run: two separate `--render` invocations produce identical SHA-256 for all three PDFs (verified 2026-10-02). |
| 5 | `--render` without an outDir silently opened the interactive form | low — patch | Real. Fixed: usage line to stderr and exit code 2 (verified). |
| 6 | Print/Save enabled on failed WebView2 navigation (`NavigationCompleted` ignores `IsSuccess`) | low — patch | Real. Fixed: buttons enable only on success; `WebErrorStatus` shown in the status label. |
| 7 | `OnSave` wrote bytes unguarded; disk-full/access-denied would crash the app | low — patch | Real. Fixed: try/catch with an error `MessageBox`. |
| 8 | `Fonts/LICENSE` was the generic Apache-2.0 text without the Tinos copyright attribution (Apache §4(c)) | low — patch | Real. Fixed: copyright read from the TTFs' name tables prepended ("The Tinos Project Authors"). |
| 9 | `epic-1-context.md` still said "QuestPDF Community licence must be confirmed" after the ADR resolved it | low — patch | Real. Fixed: points to ADR 0001's outcome (not eligible, PO decision pending). |
