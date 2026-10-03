---
name: LuuKyCanTin
description: Modern flat visual identity for the LuuKyCanTin WinForms .NET 10 desktop app (custodial deposits and canteen), built only from what WinForms can render: Panels, flat Buttons, Labels, restyled DataGridView, icon-font glyphs.
status: draft
updated: 2026-10-02
colors:
  accent: '#2563EB'          # primary buttons, focus border, selected segment, links
  accent-hover: '#1D4ED8'
  accent-soft: '#DBEAFE'     # grid selection, total box, shortcut chips
  side: '#1E293B'            # sidebar, sign-in left panel, lock overlay
  side-hover: '#273449'
  side-active: '#334155'
  side-indicator: '#60A5FA'  # 3px bar on the active nav item
  side-text: '#CBD5E1'
  side-muted: '#7C8BA1'
  surface: '#F1F5F9'         # content area background
  card: '#FFFFFF'
  subtle: '#F8FAFC'          # grid header, read-only input, dialog footer band
  border: '#E2E8F0'          # card and header borders
  row-line: '#EEF2F6'        # DataGridView.GridColor
  input-border: '#CBD5E1'
  text: '#0F172A'
  text-2: '#334155'
  label: '#475569'           # field labels, grid header text
  muted: '#64748B'           # subtitles, hints, footers
  placeholder: '#94A3B8'
  success: '#15803D'         # positive money, Đang quản lý, OK status
  success-soft: '#DCFCE7'
  danger: '#B91C1C'          # errors, inactive status, shortfall, degraded status
  danger-soft: '#FEE2E2'
  warning: '#B45309'
  warning-soft: '#FEF3C7'
typography:
  body:     { fontFamily: 'Segoe UI', fontSize: 10pt, fontWeight: '400', note: 'Set once on every Form/UserControl; inherited.' }
  label:    { fontFamily: 'Segoe UI', fontSize: 9pt,  fontWeight: '600', note: 'Field labels above inputs, grid headers. Colour {colors.label}.' }
  small:    { fontFamily: 'Segoe UI', fontSize: 9pt,  fontWeight: '400', note: 'Hints, footers, status bar, meta lines. Colour {colors.muted}.' }
  card-title:   { fontFamily: 'Segoe UI', fontSize: 11pt, fontWeight: '600' }
  dialog-title: { fontFamily: 'Segoe UI', fontSize: 13pt, fontWeight: '600' }
  page-title:   { fontFamily: 'Segoe UI', fontSize: 13.5pt, fontWeight: '600', note: 'Header bar title.' }
  greeting:     { fontFamily: 'Segoe UI', fontSize: 15pt, fontWeight: '600', note: 'Trang chủ and sign-in heading.' }
  amount-medium: { fontFamily: 'Segoe UI', fontSize: 14pt, fontWeight: '600', note: 'Balance rows, confirmation amounts. Right-aligned.' }
  amount-large:  { fontFamily: 'Segoe UI', fontSize: 24pt, fontWeight: '700', note: 'Running total. One per screen.' }
  icon: { fontFamily: 'Segoe Fluent Icons, Segoe MDL2 Assets', note: 'Glyph icons drawn as Label text; 12pt in nav, 10.5pt in buttons, 18pt in tiles. Built into Windows 10/11, no image files.' }
rounded:
  DEFAULT: 0px
spacing:
  page: 20px            # content area padding
  gap: 16px             # between cards, between dialog columns
  gap-sm: 8px           # between toolbar items and buttons
  card-pad: 16px
  dialog-pad: 20px
  sidebar-width: 220px
  header-height: 56px
  status-height: 26px
  input-height: 32px
  input-height-lg: 40px # item lookup
  button-height: 32px
  button-height-lg: 44px
  grid-header-height: 38px
  grid-row-height: 38px
  nav-item-height: 36px
  nav-sub-height: 32px
components:
  button-primary:   { background: '{colors.accent}', foreground: '#FFFFFF', border: '{colors.accent}', height: '{spacing.button-height}', fontWeight: '600' }
  button-secondary: { background: '{colors.card}', foreground: '{colors.text}', border: '{colors.input-border}', height: '{spacing.button-height}' }
  button-disabled:  { background: '{colors.border}', foreground: '{colors.placeholder}' }
  input:          { height: '{spacing.input-height}', background: '{colors.card}', border: '{colors.input-border}' }
  input-focus:    { border: '2px {colors.accent}' }
  input-error:    { border: '2px {colors.danger}' }
  input-readonly: { background: '{colors.subtle}', foreground: '{colors.text-2}' }
  card:           { background: '{colors.card}', border: '1px {colors.border}' }
  data-grid:      { header-background: '{colors.subtle}', header-foreground: '{colors.label}', grid-line: '{colors.row-line}', selection: '{colors.accent-soft}', selection-foreground: '{colors.text}' }
  sidebar-item-active: { background: '{colors.side-active}', indicator: '{colors.side-indicator}', foreground: '#FFFFFF' }
  segmented-on:   { background: '{colors.accent}', foreground: '#FFFFFF' }
  banner-error:   { background: '{colors.danger-soft}', bar: '{colors.danger}' }
  banner-warning: { background: '{colors.warning-soft}', bar: '{colors.warning}' }
  total-box:      { background: '{colors.accent-soft}', typography: '{typography.amount-large}' }
  total-box-shortfall: { background: '{colors.danger-soft}' }
---

# LuuKyCanTin – DESIGN

The prototypes are binding. Every screen must match the prototype for its pattern, as `docs/conventions/ui-prototype-conventions.md` requires. When a prototype and this file disagree, **this file wins**. Report the conflict so the prototype gets fixed.

| Prototype | Covers |
|---|---|
| [key-01-dang-nhap-shell.html](mockups/key-01-dang-nhap-shell.html) | Sign-in, change password, shell (sidebar, header, status bar), Trang chủ, lock overlay |
| [key-02-doi-tuong.html](mockups/key-02-doi-tuong.html) | List screen + edit dialog: the template for every catalogue screen |
| [key-03-ban-hang.html](mockups/key-03-ban-hang.html) | POS: the template for every voucher-entry screen, the payment summary and the posting confirmation |

## Brand & Style

This is an internal tool for a detention facility. Staff use it all day at a counter, so it should feel **calm, modern and trustworthy**, not like a 2005 Windows form. The style is **modern flat**:
- a dark slate sidebar
- a light grey work surface with white cards
- one blue accent for the main action
- generous spacing and clear hierarchy (title → card → field)

It stays honest to WinForms. Every element is a Panel, a flat Button, a Label, a TextBox, a ComboBox or a restyled DataGridView, and every icon is a glyph from the Segoe MDL2 / Fluent icon font that ships with Windows. There are no rounded corners, gradients, shadows or transparency, because WinForms can't draw them without fragile custom painting.

## Colors

All colours live in one static class, `AppTheme` (WinForms `Common/`), which mirrors the tokens above. Code never writes a hex literal or uses `SystemColors` for app chrome.

- **Accent `{colors.accent}`** is the action colour: the single primary button per screen or dialog, focus borders, the checked segment, links and quick-action glyphs. `{colors.accent-soft}` is used for selection and the total box.
- **Sidebar family** (`{colors.side}` and its variants) is used for navigation chrome only: the sidebar, the sign-in left panel and the lock overlay.
- **Neutrals** set the depth with flat colour instead of shadows: page → card → field (`surface` → `card` → `subtle`), with borders `{colors.border}` and inputs `{colors.input-border}`.
- **Semantic colours** each carry one meaning:
  - Success green: positive money, Đang quản lý, healthy status.
  - Danger red: errors, inactive detainees, shortfall, degraded status.
  - Warning amber: system warnings such as the backup age.

  Each is paired with its `-soft` background for banners and boxes.
- Colour is never the only signal. A status always has text, and an error always has a message.

## Typography

Segoe UI throughout. Body text is **10pt** (`{typography.body}`). Labels sit above inputs in 9pt semibold `{colors.label}`. The ramp is small on purpose:

| Role | Used for |
|---|---|
| `page-title` | The title in the header bar |
| `dialog-title` | The heading of a dialog |
| `card-title` | A card's header |
| `amount-medium` | Balance rows |
| `amount-large` | The total |
| `greeting` | Trang chủ and the sign-in heading |

- **Numbers:** money is `#,##0` vi-VN with a "." thousands separator, no decimals, and right-aligned.
- **Dates:** `dd/MM/yyyy`.
- **Amounts in words:** italic `{colors.text-2}`, directly under the amount they spell out.
- **Detainee names:** upper case as stored and semibold in grids.

## Layout & Spacing

- **Shell:** sidebar `{spacing.sidebar-width}` (Dock Left), header bar `{spacing.header-height}` (Dock Top, white, bottom border), status bar `{spacing.status-height}` (Dock Bottom), and the content area (Dock Fill, `{colors.surface}`, padding `{spacing.page}`). The minimum target is 1366×768 at 100% with nothing clipped.
- **List screen:** a toolbar row (search 300px, inline-label filters, spacer, secondary buttons, one primary button), then 16px space, then a card filling the rest. The card holds the grid and a 44px footer (count left, keyboard hints right).
- **Voucher screen:** two columns 16px apart. The left fills: the "Người mua" card on top and the "Mặt hàng" card filling the rest. The right is the "Thanh toán" card, fixed at 340px and full height.
- **Dialog:** width 560 (480 for small ones). The parts, top to bottom:
  - title bar
  - head with title and subtitle, padding 20
  - body: a 2-column TableLayoutPanel, 16px column gap, 14px row gap
  - footer band in `{colors.subtle}`, buttons right-aligned, primary last
- **Fields:** the label sits above the input with a 5px gap. Inputs fill their column, and long fields span both columns.
- `AutoScaleMode.Font`; sizes in this file are at 96 DPI.

## Elevation & Depth

Depth comes from flat tone steps (`surface` → `card` → `subtle`) and 1px borders, not shadows. Dialogs are ordinary windows; Windows draws their shadow.

## Shapes

Square corners everywhere (`{rounded.DEFAULT}`). Avatars and dots are squares too.

## Components

| Component | WinForms build | Spec |
|---|---|---|
| Sidebar | Panel + flat Buttons (`FlatAppearance.BorderSize = 0`, `TextAlign = MiddleLeft`) with a glyph Label | Items are `{spacing.nav-item-height}`, sub-items `{spacing.nav-sub-height}` indented 47px. Hover uses `{colors.side-hover}`; active uses `sidebar-item-active` with a 3px indicator Panel. Groups have a muted caps header ("NGHIỆP VỤ", "QUẢN LÝ"). The user block sits at the bottom. |
| Header bar | Panel | Title (`page-title`), a muted subtitle/breadcrumb, and quick search (300px, Ctrl+K chip) on the right. |
| Status bar | Panel or StatusStrip with flat renderer | Items separated by 1px `{colors.border}`, each with an 8px square dot (success or danger). A degraded item's text is danger semibold with a tooltip. |
| Card | **CardPanel** (shared control: Panel that paints a 1px `{colors.border}` border) | An optional 44px header (`card-title`, bottom border) and a body with padding `{spacing.card-pad}`. |
| Input | **InputFrame** (shared control: Panel that paints a 1/2px border and hosts a borderless TextBox or ComboBox plus an optional leading glyph) | Height `{spacing.input-height}`. States: `input`, `input-focus`, `input-error`, `input-readonly`. |
| Button | Button `FlatStyle.Flat` | `button-primary` / `button-secondary` / `button-disabled`, height `{spacing.button-height}` (large `{spacing.button-height-lg}`), 14px horizontal padding, glyph + text with an `&` mnemonic. One primary per screen or dialog. |
| Segmented control | RadioButtons, `Appearance = Button`, `FlatStyle.Flat` | 28px high. The checked segment uses `segmented-on`. |
| DataGridView | Stock, restyled | `BorderStyle.None`, `EnableHeadersVisualStyles = false`, `CellBorderStyle.SingleHorizontal`, header `{spacing.grid-header-height}`, rows `{spacing.grid-row-height}`, colours from `data-grid`, `RowHeadersVisible = false`, `FullRowSelect`. Status cells paint a square dot in `CellPainting`. The editable cell gets a 2px accent outline. |
| Banner | Panel + glyph Label + Label | `banner-error` / `banner-warning` with a 3px left bar. Used for form-level errors and system warnings. |
| Field error | Label under the field | Danger 9pt with an error glyph, plus `input-error` on the field. Replaces ErrorProvider. |
| Detainee card | Panel `{colors.subtle}` | Initials tile 40px (accent), name semibold, muted meta line, status dot right. |
| Total box | Panel | `total-box` / `total-box-shortfall`: caps "TỔNG TIỀN" label, amount, amount in words. |
| Quick-action tile | CardPanel | 124px high. Accent glyph 18pt, title semibold, muted description, shortcut chip (`accent-soft`). |
| Confirmation dialog | Form | Icon tile 44px (`accent-soft`), question in `dialog-title`, subject muted, a before → amount → after table, footer with an option checkbox left and buttons right. |
| Lock overlay | Panel Dock Fill `{colors.side}` | Centred white card 380px with initials, name, password field, primary "Mở khoá", and a "Đăng xuất" link. |

## Do's and Don'ts

| Do | Don't |
|---|---|
| Take every colour and font from `AppTheme` | Write hex literals, use `SystemColors` for chrome, or set ad-hoc fonts |
| Use one primary (accent) button per screen or dialog | Make several buttons blue, or colour buttons by meaning |
| Put labels above inputs, in a 2-column dialog grid | Put labels on the left with ragged input columns (old style) |
| Use CardPanel and InputFrame from `Common/` | Write new owner-drawn controls, add third-party suites, or use rounded corners or shadows |
| Use icon-font glyphs (Segoe MDL2 / Fluent) | Add bitmap icon sets or emoji |
| Use red only for errors, inactive records, shortfall and degraded state | Use colour as decoration or as the only signal |
| Right-align money with "." separators | Show decimals on money |
