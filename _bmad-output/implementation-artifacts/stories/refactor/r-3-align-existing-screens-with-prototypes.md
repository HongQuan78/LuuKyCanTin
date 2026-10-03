---
story: "R.3"
epic: refactor
title: Bring the existing screens into line with the UI prototypes
status: ready-for-dev
size: L
backlogItems: []
frsCovered: []
dependsOn: ["R.2", "1.8", "2.1", "2.3"]
---

# Story R.3: Bring the existing screens into line with the UI prototypes

Status: ready-for-dev

## Story

As a counter officer,
I want the staff register, the detainee dialog, the deposit receipt, the role screen and the print preview to follow the same prototypes as the new shell,
So that every screen I use looks and works the same way, and new catalogue and voucher screens have a correct example to copy.

## Acceptance Criteria

1. **Given** the staff register (`OfficerForm`)
   **When** the user opens "Danh mục › Cán bộ"
   **Then** it is a UserControl hosted in the shell's content area, laid out as the list screen of `key-02` A: toolbar row (search `InputFrame` 300px with a search glyph, the inline-label filter "Trạng thái:" with "Đang công tác" / "Tất cả", spacer, secondary "&Sửa", primary "&Thêm cán bộ" last), 16px gap, then a `CardPanel` with the grid and a 44px footer (result count left, keyboard hints right)
   **And** the grid is styled by `AppTheme` (38px header and rows, name column semibold and the only Fill column, status shown as a square dot plus text), Insert opens Thêm, Enter or double-click opens Sửa, and search keeps its debounce with no Search button

2. **Given** the staff edit dialog (`OfficerEditForm`) and the add-detainee dialog (`AddInmateForm`)
   **When** either opens
   **Then** it follows the edit dialog of `key-02` B: `FixedDialog`, `CenterParent`, width 560 (480 if the fields fit), head with the title (the record's name on Sửa) and a muted subtitle, body as a 2-column `TableLayoutPanel` (16px column gap, 14px rows, padding 20) with labels above `InputFrame` fields, long fields (Họ tên) spanning both columns, and a `subtle` footer band with buttons right-aligned, primary "&Lưu" last
   **And** validation on Lưu marks each invalid field with the red border and message under it, moves focus to the first invalid field and keeps the dialog open; a server error that names a field ("Mã số đã tồn tại", "Mã cán bộ đã tồn tại") maps to that field, and any other error shows in a `banner-error` in the dialog instead of a MessageBox

3. **Given** the deposit receipt (`DepositReceiptForm`)
   **When** the user opens "Lưu ký › Lập biên nhận thu" (or presses F2)
   **Then** it is a UserControl hosted in the content area, laid out as the voucher-entry screen of `key-03` A: left column with a "Đối tượng" card (detainee, Nghiệp vụ) on top and a "Thông tin nộp tiền" card (Người gửi, Quan hệ, Hình thức, Số tài khoản, Ngày chứng từ, Nội dung) below; right column a 340px "Số tiền" card with the amount input and a `total-box` showing the amount (`amount-large`, `#,##0`) and the amount in words under it
   **And** "&Ghi sổ" is the one 44px primary button in the right card, Enter never posts (no `AcceptButton`), after posting the card shows the voucher number and the new balance (`success` when positive) and "&In biên nhận" becomes enabled, and a secondary "Làm &mới" clears the screen for the next receipt with focus back on Đối tượng

4. **Given** the deposit receipt is half filled
   **When** the user switches to another screen and back
   **Then** everything typed is still there (the UserControl is cached, as R.2 provides)
   **And** each operation still creates its own DI scope; the cached view never holds a scope or a `DbContext`

5. **Given** the role screen (`RoleForm`)
   **When** the user opens "Hệ thống › Vai trò"
   **Then** it is a UserControl hosted in the content area, with the role list and the permission matrix each in a `CardPanel`, the permission grid styled by `AppTheme`, one primary "&Lưu" and a secondary "Huỷ thay đổi", and success or failure messages shown in a banner (warning or error) instead of coloured label text
   **And** its layout was confirmed by the user before it was built (see T1)

6. **Given** the print preview (`PdfPreviewForm`)
   **When** a receipt is previewed
   **Then** its toolbar follows `AppTheme` (white bar with a bottom border, secondary buttons, "&In" as the one primary button, glyph icons from the icon font) and its status line uses the status-bar style
   **And** its print and save failures keep their current MessageBox texts (they are operation failures, not field errors)

7. **Given** every screen in this story
   **When** it is compared with the build before this story
   **Then** the only visible-text changes are the required-field marker (` *` instead of `(*)`), no colon after a label above its field, added `&` mnemonics, and texts taken verbatim from the prototypes; every other caption, message and printed output is identical (the printed deposit receipt is byte-identical)

8. **Given** every screen in this story
   **When** it is used with the keyboard only
   **Then** the tab order follows the visual order, Enter and Esc behave as `EXPERIENCE.md` says for its pattern (list, dialog, voucher), every button has a unique `&` mnemonic (checked by R.2's mnemonic test helper), and nothing is clipped at 1366×768, 100% DPI

9. **Given** the finished change
   **When** `dotnet build` and `dotnet test` run
   **Then** both pass, including R.2's theme-usage test, which now covers these screens without exceptions
   **And** the Dev Agent Record holds a 1366×768 screenshot of each screen next to its mockup section

## Tasks / Subtasks

- [ ] **T1. Confirm the role-screen layout first** (AC: 5)
  - [ ] The role screen is master-detail (list left, matrix right). No prototype has that layout, so `ui-prototype-conventions.md` makes it a UX decision. Before coding T6, show the user a sketch (two `CardPanel`s side by side, role list 260px, matrix filling the rest) and get a yes, or have it added to the prototypes through `bmad-ux` (Update mode). Record the answer in the Dev Agent Record. Do T2–T5 meanwhile.
- [ ] **T2. Staff register as a hosted list screen** (AC: 1, 8)
  - [ ] Convert `OfficerForm` to a UserControl and register it with R.2's content host instead of `Navigator.OpenOfficers()` opening a window. Keep the class names of converted screens (`OfficerForm`, `DepositReceiptForm`, `RoleForm`): `naming-conventions.md` defines `<Noun>Form` for list forms and no separate suffix for hosted screens.
  - [ ] Replace the "Hiện cả người đã nghỉ" checkbox with the inline-label `Trạng thái:` ComboBox; `IOfficerView.ShowInactive` keeps its meaning ("Tất cả" = true).
  - [ ] Status column: `CellPainting` draws an 8px square (`Success` for Đang công tác, `Danger` otherwise) before the text. Quản giáo stays a check column.
  - [ ] Footer: "N cán bộ" left; "Insert thêm · Enter sửa" right in `muted`.
- [ ] **T3. Edit dialogs** (AC: 2, 7, 8)
  - [ ] Rebuild `OfficerEditForm` and `AddInmateForm` after `key-02` B with `InputFrame`s and the 2-column grid.
  - [ ] Replace `ShowError(string)` on `IOfficerEditView` and `IAddInmateView` with field-aware errors (a field enum per view plus a banner for errors that name no field). The presenters map FluentValidation `PropertyName`s and the known duplicate-code messages to fields. Update `OfficerEditPresenterTests` and `AddInmatePresenterTests`.
  - [ ] Rename `IOfficerEditView.DisplayText()` to `ShowModal()` (an R.1 mistranslation of `HienThi`, like the one fixed on the change-password view in R.2).
- [ ] **T4. Deposit receipt as a hosted voucher screen** (AC: 3, 4, 7, 8)
  - [ ] Convert `DepositReceiptForm` to a UserControl and host it; F2 and the Trang chủ tile from R.2 open it in the content area.
  - [ ] Lay out the two columns and three cards. Money input: `InputFrame`, right-aligned, `#,##0` vi-VN while typing if the current control already formats; otherwise keep today's parsing (the shared money control is UX-DR1, a later story).
  - [ ] Replace `btnClose`/"Đóng" with "Làm &mới" (navigation replaces closing). Add `IDepositReceiptView.Reset()` and a `ResetClicked` event; the presenter reloads the detainee list. Update `DepositReceiptPresenterTests`.
  - [ ] Posting failure keeps its MessageBox ("Không ghi sổ được"), as `EXPERIENCE.md` allows for server re-check failures; validation errors go to their fields.
- [ ] **T5. Print preview** (AC: 6)
  - [ ] Restyle the toolbar and status line of `PdfPreviewForm` through `AppTheme`. It stays a separate window (WebView2).
- [ ] **T6. Role screen** (AC: 5) — only after T1
  - [ ] Convert to a hosted UserControl in two `CardPanel`s; replace `lblMessage` colours with a `Banner`. `IRoleView.ShowMessage`/`ShowError` keep their signatures.
- [ ] **T7. Shell wiring** (AC: 1, 3, 5)
  - [ ] Point the R.2 nav model entries for Cán bộ, Lập biên nhận thu and Vai trò at the content host. Remove the window-opening code they replace from `Navigator`.
- [ ] **T8. Verify** (AC: 7, 8, 9)
  - [ ] Build and test. Run the app at 1366×768, 100%: add and edit a staff member (including a duplicate code), add a detainee (including a duplicate code), post and print a receipt, switch away from a half-filled receipt and back, save a role. Compare the printed receipt with one from the previous build. Save screenshots and list them in the Dev Agent Record.

## Dev Notes

- **Prototype patterns:** `OfficerForm` → key-02 A (list screen); `OfficerEditForm`, `AddInmateForm` → key-02 B (edit dialog); `DepositReceiptForm` → key-03 A (voucher entry, reusing rules 1, 4, 5, 6 of its notes); `RoleForm` → no exact prototype, see T1; `PdfPreviewForm` → no prototype, restyle only.
- **Scope is visual and interaction only** (decided with the user on 2026-10-03). Don't build, even though the prototypes show them:
  - the posting confirmation dialog (UX-DR4), the balance-before row and the F9 shortcut → Epic 4;
  - the detainee picker with incremental search and the detainee card (UX-DR2) → Epics 3–4; the detainee ComboBox stays, inside an `InputFrame`;
  - the shared money input (UX-DR1), the grid's "Export to Excel" context menu (UX-DR3) → their stories;
  - permission-driven read-only mode → 2.5.
- The detainee list screen doesn't exist yet (Epic 3). "Thêm đối tượng" stays a dialog opened from the sidebar.
- `AddInmateForm` currently says "Hủy" and `RoleForm` says "Huỷ thay đổi". Keep both spellings (`EXPERIENCE.md` open question).
- Presenter tests are the safety net: change a view interface only where this story's AC needs it (field errors, `Reset`, `ShowModal`), and update the matching tests in the same commit.
- Converting a Form to a UserControl: move `Text` to the header title the content host shows, drop `AcceptButton`/`CancelButton` from hosted screens (they belong to dialogs), and handle `Load` through `VisibleChanged`/`HandleCreated` once, since a cached control loads only the first time.

## References

- `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/DESIGN.md`, `EXPERIENCE.md`
- `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/mockups/key-02-doi-tuong.html` (A, B and their rules), `key-03-ban-hang.html` (A and its rules)
- `docs/conventions/ui-prototype-conventions.md` (review checklist), `docs/conventions/naming-conventions.md`
- Story R.2 (`AppTheme`, `CardPanel`, `InputFrame`, `Banner`, content host, mnemonic and theme-usage tests)
- Current code: `src/Presentation/LuuKyCanTin.WinForms/MasterData/`, `Custody/`, `Administration/RoleForm*`, `Common/PdfPreviewForm.cs`

## Dev Agent Record

### Agent Model Used

### Approach

### Decisions and deviations

### Verification

### Screenshots

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-03 | Story created (bmad-create-epics-and-stories): second half of the UI alignment refactor, after R.2. |
