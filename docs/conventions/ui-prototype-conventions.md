# UI prototype conventions

**Status: mandatory for every new or changed WinForms screen, whether a person or an AI agent builds it.**

## The rule

Every screen must **strictly follow the style of the UI prototypes**. Developers copy the prototype. They do not reinterpret it.

The source of truth lives in `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/`:

| File | Owns |
|---|---|
| `DESIGN.md` | Visual identity: colours, fonts, spacing grid, component specs, do's and don'ts |
| `EXPERIENCE.md` | Behaviour: menu tree, component patterns, states, shortcuts, accessibility |
| `mockups/key-01-dang-nhap-shell.html` | Sign-in, change password, shell (sidebar, header, status bar), Trang chủ, lock overlay |
| `mockups/key-02-doi-tuong.html` | **List form + edit dialog**: the template for every catalogue screen |
| `mockups/key-03-ban-hang.html` | **Voucher entry**: the template for POS, biên nhận thu, phiếu chi, phiếu nhập; also the balance panel and posting confirmation |

The mockups render 1:1 at 96 DPI, so 1 CSS px equals 1 WinForms px. Open them in a browser next to the Visual Studio designer.

## What "strictly follow" means

1. **Pick the pattern first.** Before building a screen, name the prototype it follows: list form, edit dialog, voucher entry, or shell. Record it in the story's Dev Notes.
2. **Same layout.** Use the same shell regions (sidebar 220, header 56, status 26, content padding 20), the same card arrangement, labels above inputs in a 2-column dialog grid, the same control sizes (32 px inputs and buttons, 44 px large buttons, 38 px grid rows) and the same button placement as the prototype.
3. **Same controls.** Use stock WinForms controls styled as `DESIGN.md` describes, plus the shared controls in `src/Presentation/LuuKyCanTin.WinForms/Common/` (`CardPanel`, `InputFrame`). Don't add third-party control suites, skins, rounded corners, shadows or new owner-drawn controls. Icons are glyphs of the Segoe MDL2 / Fluent icon font, never image files.
4. **Same colours and fonts.** Take every colour and font from `AppTheme` (`Common/`), which mirrors the `DESIGN.md` tokens one-to-one. No hex literals, no `Color.FromArgb` and no `new Font(...)` outside `AppTheme`. Body text is Segoe UI 10pt. Keep one primary (accent) button per screen or dialog.
5. **Same behaviour.** Follow the prototype's focus order, AcceptButton/CancelButton, shortcuts, mnemonics, inline validation (red border + message under the field), banners, read-only and disabled rules, and number/date formats, as `EXPERIENCE.md` specifies.
6. **Same wording style.** Vietnamese labels above the field (required fields end with " *") and verb buttons with an `&` mnemonic. Take message texts verbatim from the spec or the prototype.

## When a screen has no exact prototype

- Use the closest pattern and change only what the data forces, such as fields, columns and filters.
- A **new layout or a new kind of control** (a tree, a chart, a tab strip, a wizard) is a UX decision, not a coding decision. Stop and ask the user. Then add it to the prototypes through `bmad-ux` (Update mode) before building it.

## Conflicts

- `DESIGN.md` and `EXPERIENCE.md` win over a mockup. If the two disagree, report it and don't guess.
- Existing screens built before these prototypes (for example `MainForm` and `OfficerForm`) are brought into line in a dedicated refactor story, not as a side effect of another story. That story also creates `AppTheme`, `CardPanel` and `InputFrame`.

## Review checklist (code review must check)

- [ ] The prototype pattern is named in the story.
- [ ] A side-by-side comparison with the mockup shows the same layout, spacing, sizes and placement.
- [ ] No hard-coded colours or fonts; everything comes from `AppTheme`. Only one primary button per screen or dialog.
- [ ] Keyboard-only operation works: tab order, mnemonics, shortcuts, Enter/Esc.
- [ ] States match `EXPERIENCE.md` (errors, read-only, inactive, empty).
- [ ] Nothing is clipped at 1366×768 and 100% DPI.
