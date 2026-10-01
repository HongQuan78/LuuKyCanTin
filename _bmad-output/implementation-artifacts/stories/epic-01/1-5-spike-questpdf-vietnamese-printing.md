---
story: "1.5"
epic: 1
title: "Spike: QuestPDF printing with Vietnamese fonts"
status: ready-for-dev
type: spike
size: M (time-boxed, 2 days suggested)
backlogItems: [SP-01]
frsCovered: []
nfrsTouched: [NFR8, NFR9, NFR14]
uxDrs: [UX-DR6]
dependsOn: ["1.1"]
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

- [ ] **T1. Licence check** (AC: 1)
  - [ ] Read the **current** QuestPDF licence page and EULA (questpdf.com/license). Record the version and date you checked them. The terms have changed between versions, so don't rely on memory or old blog posts.
  - [ ] Answer two questions explicitly: (a) does the unit qualify for Community (a public-sector body, internal non-commercial use, no revenue)? (b) Does the *development team* or a contractor need a licence of its own?
  - [ ] Write `docs/decisions/0001-pdf-engine-questpdf.md` (ADR style: Context, Decision, Consequences, Alternatives). If Community is doubtful, the decision may be "QuestPDF pending written confirmation". Escalate to the PO, because the alternative changes how all 12 templates are built.
- [ ] **T2. Throw-away spike project, outside the product code** (AC: 2, 3)
  - [ ] Create `spikes/SP-01-QuestPdf/` (a WinForms `net10.0-windows` project). **Don't** add it to `LuuKyCanTin.slnx`, and **don't** reference it from `src/`. That keeps the product source clean and the CI build unaffected. Add a one-line `spikes/README.md`: "Time-boxed experiments. Not production code. Outcomes live in docs/decisions/."
  - [ ] Packages: `QuestPDF` (latest stable), `Microsoft.Web.WebView2`. Set `QuestPDF.Settings.License = LicenseType.Community;` (or the value from T1).
- [ ] **T3. Vietnamese font embedding** (AC: 2)
  - [ ] Pick a font with full Vietnamese coverage and a licence that allows embedding and redistribution. Candidates: **Noto Sans / Noto Serif** (OFL), **Be Vietnam Pro** (OFL), or Times New Roman look-alikes such as **Tinos** (Apache 2.0). Government paper forms usually use Times New Roman, so a metric-compatible serif keeps layouts faithful.
  - [ ] Ship the `.ttf` as an embedded resource. Register it with `FontManager.RegisterFont(stream)`. **Never** depend on fonts installed on the workstation. Disable the system-font fallback if QuestPDF exposes the setting, so a missing glyph shows up in the spike instead of silently looking right on the dev's machine.
  - [ ] The sample text must cover every tone mark on every vowel, plus `đĐ`: e.g. "Nguyễn Thị Ánh Tuyết – Biên nhận thu tiền gửi lưu ký; ắ ằ ẳ ẵ ặ ấ ầ ẩ ẫ ậ ế ề ể ễ ệ ố ồ ổ ỗ ộ ớ ờ ở ỡ ợ ứ ừ ử ữ ự ỳ ỷ ỹ ỵ Đ đ". Include bold, italic and the amount-in-words line, and test Unicode **NFC** vs **NFD** input (text pasted from Word can be NFD). Recommend normalizing to NFC before rendering.
  - [ ] Render the same document to **A4 portrait** and **A5** (portrait and landscape: receipts are often A5 landscape, check the paper form). Produce one header block (unit name / address, centred, national motto lines) and a 3-column signature block. That's the shape every template will reuse (FR42).
  - [ ] Verify on a clean Windows VM or Sandbox with no extra fonts: open the PDF and check the PDF's font list (Properties › Fonts) shows only the **embedded** font.
- [ ] **T4. WebView2 preview / print / save** (AC: 3)
  - [ ] Form with a `WebView2` control. Write the PDF bytes to a temp file under `%LOCALAPPDATA%\LuuKyCanTin\preview\` and `Navigate` to `file:///…`. Alternatively serve from memory via `CoreWebView2.SetVirtualHostNameToFolderMapping`, or a `WebResourceRequested` handler for `https://report.local/`. Pick one and note why.
  - [ ] **Print**: test both the built-in PDF viewer toolbar's print button and a programmatic `CoreWebView2.PrintAsync(settings)` (silent print to the default printer, which feeds TI-08 "auto-print after posting" later). Note page-scaling issues: printing must be 100 % "actual size", not "fit".
  - [ ] **Save**: the viewer's download/save button, plus a programmatic Save (just write the bytes with a `SaveFileDialog`). The second is simpler and is the recommendation.
  - [ ] **Offline**: unplug the network and repeat. Set `CoreWebView2Environment` user-data folder to `%LOCALAPPDATA%\LuuKyCanTin\WebView2`, not next to the exe (Program Files is read-only).
  - [ ] **Runtime install**: confirm Windows 10/11 machines without Edge updates still get the Evergreen runtime. Document the **offline standalone installer** (`MicrosoftEdgeWebView2RuntimeInstallerX64.exe`) and how the Velopack setup (Story 1.6) or the admin installs it. Note the fixed-version option as a fallback.
  - [ ] Clean up temp preview files on close and at startup.
- [ ] **T5. `IReportRenderer` proposal** (AC: 4)
  - [ ] Write the proposed shape in the decision note, for example:

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
  - [ ] Keep `byte[]` (not `Stream`) unless the spike shows memory problems. The documents are 1–5 pages.
  - [ ] Note how golden-file snapshot tests (NEN-21) can work: QuestPDF output must be deterministic. Check whether creation timestamps or IDs vary between runs, and how to fix the metadata (`DocumentMetadata.CreationDate`).
- [ ] **T6. Outcome review** (AC: 1–4)
  - [ ] The decision note covers: licence verdict, chosen font plus its licence, A4/A5 screenshots, print/save results, offline result, WebView2 runtime requirement, `IReportRenderer` shape, determinism finding, open risks.
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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
