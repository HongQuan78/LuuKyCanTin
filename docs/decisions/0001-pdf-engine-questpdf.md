# ADR 0001: PDF engine — QuestPDF with embedded Tinos fonts and WebView2 preview

- **Date:** 2026-10-02 (licence terms checked on this date)
- **Status:** Proposed — the licence decision is **pending PO decision**; the technical findings
  below are validated by spike SP-01 (Story 1.5)
- **References:** Story 1.5 (SP-01), `epics.md` › NFR8, NFR9, NFR14, UX-DR6, FR42, FR43; tech
  stack PDF (QuestPDF row, WebView2 row, RDLC alternative); DB design PDF (`CauHinhKyTen.MaMauIn`
  e.g. `BIEN_NHAN_THU`, `PHIEU_CHI`)

## Context

Twelve print templates are coming (receipt, payout, per-detainee statement, unit ledger book,
remittance list, goods receipt note, purchase slip, stock movement, revenue, purchase history,
posted price list, goods catalogue), some with dense wide tables. Requirements:

- **Offline, always**: no CDN fonts, no online licence activation, no telemetry (site has no
  Internet).
- **NFR14**: free or permissively licensed libraries only — font licences count too.
- Vietnamese text on A4 and A5 (receipts are often A5 landscape), matching the legal paper forms
  (Times New Roman look).
- Preview + print + save inside WinForms without Internet.
- Golden-file snapshot tests (NEN-21) need deterministic PDF output.
- Reprints show "BẢN IN LẠI" + print count as a rotated watermark (UX-DR6, FR43).

## Licence verdict (QuestPDF Community v3.0, checked 2026-10-02)

- QuestPDF Community Licence **v3.0**, effective **2026-07-06**, governs the free tier. It is
  offered to *individual developers* and to companies under a revenue threshold, and it
  **explicitly excludes government / public-sector entities regardless of revenue**.
- (a) Does the unit qualify? **No.** The unit is a detention facility — a public-sector body,
  non-commercial, no revenue — exactly the category the Community licence excludes. The exclusion
  is by entity type, not by revenue or commerciality.
- (b) Does the development team / a contractor need its own licence? The paid QuestPDF licences
  (Professional/Enterprise) are per legal entity with unlimited developers; if the unit buys one,
  its developers and contractors working for it are covered. A contractor's own Community licence
  does **not** cover work delivered to a government customer.
- The spike itself is allowed under the Community licence ("learning and evaluation" category),
  so the spike sets `QuestPDF.Settings.License = LicenseType.Community;` and proceeds.

## Decision

**QuestPDF Community licence not applicable — pending PO decision:** purchase a paid
Professional/Enterprise licence (unlimited developers, covers the legal entity) **or** fall back
to RDLC via `ReportViewerCore.WinForms`. Escalate to the PO, because the alternative changes how
all 12 templates are built (the QuestPDF row in the tech-stack PDF already carries this caveat).

The spike proceeds with QuestPDF anyway to validate the technical approach. Because the product
will depend only on the `IReportRenderer` abstraction (below), swapping the engine later touches
one project (`Infrastructure/Reports`), not Application or Domain.

## Font choice

**Tinos** (Apache 2.0) — metric-compatible with Times New Roman, which matches Vietnamese
government paper forms. Alternatives considered: Noto Serif / Be Vietnam Pro (OFL) and others;
Tinos wins on the Times New Roman metric match (NFR14: Apache 2.0 is a permissive licence).

- Tinos Regular/Bold/Italic TTFs ship as **embedded resources** in the spike and are registered
  with `FontManager.RegisterFontFromStream(stream)` (QuestPDF 2026.9 API; `FontManager` lives in
  `QuestPDF.Drawing`). The product will never depend on fonts installed on the workstation (no
  CDN, no system-font fallback — `QuestPDF.Settings.UseSystemFonts` defaults to false).
- QuestPDF has no system-font fallback: with `QuestPDF.Settings.ThrowOnMissingTextGlyphs` (and
  `ThrowOnMissingFontFamilies`) enabled, a missing glyph stops generation with a
  `DocumentDrawingException`, so a "renders on the dev machine, broken on the workstation"
  situation cannot happen silently.
- The Apache 2.0 licence text ships next to the TTFs (`spikes/SP-01-QuestPdf/Fonts/LICENSE`).
- **Unicode normalization:** NFC and NFD input render identically (same glyphs); the product
  should normalize all text to **NFC (FormC)** before rendering, because text pasted from Word can
  arrive as NFD.

## WebView2 findings

- **Preview:** PDF bytes are written to `%LOCALAPPDATA%\LuuKyCanTin\preview\` and the control
  navigates to `file:///…`. Chosen over `SetVirtualHostNameToFolderMapping` /
  `WebResourceRequested` because it is the simplest path that works with Chromium's built-in PDF
  viewer on a single-user workstation.
- **Print:** programmatic `CoreWebView2.PrintAsync(settings)` with `ScaleFactor = 1.0` prints at
  100 % actual size (not "fit"). The PDF viewer's own toolbar print button also works. This feeds
  TI-08 "auto-print after posting" later. (Verify on target printers; print-driver behaviour is
  the one thing the spike cannot fully validate headlessly.)
- **Save:** programmatic `SaveFileDialog` writing the bytes — simpler and more reliable than the
  viewer's download button. Recommended for the product.
- **Offline:** preview, print and save are all local; no network is touched.
- **Runtime:** the WebView2 Evergreen runtime is delivered independently of Edge/Windows update
  cadence; machines without Edge updates still receive it (Windows Update / admin install). For
  the LAN rollout the **offline standalone installer**
  (`MicrosoftEdgeWebView2RuntimeInstallerX64.exe`) must be installed by the Velopack setup
  (Story 1.6) or the admin; fixed-version distribution is the fallback if Evergreen is refused.
- **User-data folder:** `%LOCALAPPDATA%\LuuKyCanTin\WebView2` — never next to the exe (Program
  Files is read-only).
- Stale preview files are deleted at startup and on close.

## `IReportRenderer` proposal (for Story 4.4)

Agreed shape — input model → PDF bytes, kept in Application so no engine type leaks:

```csharp
// Application/Abstractions
public interface IReportRenderer
{
    byte[] Render<TModel>(TModel model) where TModel : IReportModel;
}

// Application/BaoCao: one model per template, a plain DTO with snapshotted data
public interface IReportModel { string MaMauIn { get; } }  // e.g. "BIEN_NHAN_THU", matches CauHinhKyTen.MaMauIn
```

Infrastructure resolves `IReportTemplate<TModel>` (one QuestPDF class per template in
`Infrastructure/Reports/`). The shared frame — header from `ThongTinDonVi`, signature block from
`CauHinhKyTen`, reprint watermark — is a base component every template composes (built in Epic 4,
FR42/FR43). `byte[]` (not `Stream`) is right for 1–5 page documents; the spike showed no memory
concern.

## Determinism finding

QuestPDF embeds the current time in the PDF catalog (`CreationDate`) unless told otherwise. With
`DocumentMetadata.CreationDate` (and `ModifiedDate`) fixed, two renders of the same model are
**byte-identical — SHA-256 equal** — verified in the spike's `--render` determinism check
([`determinism.txt`](0001/assets/determinism.txt)). Consequences for the product:

- Every template must set `CreationDate` from `IClock.Now` (this is also what a real receipt
  shows) — determinism holds because the renderer never reads the wall clock itself.
- Golden-file snapshot tests (NEN-21) are feasible: render a model with a fixed clock and compare
  bytes. Document numbers and snapshot data must come from the model, not from the renderer.

## Evidence (spike output, checked 2026-10-02)

Sample documents (A4 portrait, A5 portrait, A5 landscape). A4 fits on one page; the A5 samples
flow onto a second page (the signature block lands on page 2) — QuestPDF paginates automatically,
and the shared frame must tolerate that:

- [mau-in-a4-portrait.pdf](0001/assets/mau-in-a4-portrait.pdf) ·
  [PNG preview](0001/assets/mau-in-a4-portrait-p1.png)
- [mau-in-a5-portrait.pdf](0001/assets/mau-in-a5-portrait.pdf) ·
  [PNG p.1](0001/assets/mau-in-a5-portrait-p1.png) · [PNG p.2](0001/assets/mau-in-a5-portrait-p2.png)
- [mau-in-a5-landscape.pdf](0001/assets/mau-in-a5-landscape.pdf) ·
  [PNG p.1](0001/assets/mau-in-a5-landscape-p1.png) · [PNG p.2](0001/assets/mau-in-a5-landscape-p2.png)

Each PDF embeds three subset TrueType fonts (`/FontFile2`: Tinos Regular/Bold/Italic); the
ToUnicode CMap maps Đ→U+0110, đ→U+0111, ấ→U+1EA5, and rendering succeeds with
`ThrowOnMissingTextGlyphs=true`, so every Vietnamese glyph is present in the embedded fonts. The
`--render` mode also writes one PNG per page (`-p<N>` suffix) and the determinism report; the
copies in `0001/assets/` are the exact spike output.

## Consequences

- All 12 templates are built against `IReportRenderer` + one model per template; swapping the
  engine (QuestPDF ↔ RDLC) is an Infrastructure-only change.
- The PO must decide the licence before production printing (blocking for Story 1.8's print path
  only if the decision is "RDLC").
- QuestPDF is not open source under a permissive licence — a paid licence is a cost item; this is
  the NFR14 tension the PO resolves.
- Print fidelity depends on the WebView2 runtime's PDF viewer and the installed printer driver;
  a clean-VM check (no extra fonts) is still to be done manually: open each sample PDF and
  confirm Properties › Fonts lists only **Tinos**.

## Open risks

1. **Licence decision** (PO) — QuestPDF paid licence vs RDLC fallback; RDLC via
   `ReportViewerCore.WinForms` is community-maintained, **not supported by Microsoft**.
2. **Clean-VM font check** not yet executed by a human (the spike machine has the fonts
   installed; the embedded-font path is proven by code + PDF bytes containing "Tinos").
3. **Printer scaling** on real drivers — verify 100 % actual size on the unit's printers.
4. **WebView2 Runtime rollout** — the offline installer must be part of the Velopack setup
   (Story 1.6) or an admin step on every workstation.

## Alternatives

1. **RDLC + ReportViewerCore.WinForms** — the fallback if the PO declines a paid licence;
   community-maintained, no Microsoft support, different layout model (RDL XML) so templates
   would be rebuilt; WinForms-native preview is a plus.
2. **QuestPDF paid licence (Professional/Enterprise)** — preferred if the PO accepts the cost:
   keeps the validated approach, per-legal-entity licence with unlimited developers.
3. Other engines (PdfSharpCore/MigraDoc, SkiaSharp-based) — not evaluated; QuestPDF was already
   the tech-stack candidate and its fluent layout model is a good fit for the shared frame.