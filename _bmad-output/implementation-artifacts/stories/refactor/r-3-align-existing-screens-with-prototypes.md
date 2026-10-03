---
story: "R.3"
epic: refactor
title: Bring the existing screens into line with the UI prototypes
status: review
size: L
backlogItems: []
frsCovered: []
dependsOn: ["R.2", "1.8", "2.1", "2.3"]
---

# Story R.3: Bring the existing screens into line with the UI prototypes

Status: review

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

- [x] **T1. Confirm the role-screen layout first** (AC: 5)
  - [x] The role screen is master-detail (list left, matrix right). No prototype has that layout, so `ui-prototype-conventions.md` makes it a UX decision. Before coding T6, show the user a sketch (two `CardPanel`s side by side, role list 260px, matrix filling the rest) and get a yes, or have it added to the prototypes through `bmad-ux` (Update mode). Record the answer in the Dev Agent Record. Do T2–T5 meanwhile.
- [x] **T2. Staff register as a hosted list screen** (AC: 1, 8)
  - [x] Convert `OfficerForm` to a UserControl and register it with R.2's content host instead of `Navigator.OpenOfficers()` opening a window. Keep the class names of converted screens (`OfficerForm`, `DepositReceiptForm`, `RoleForm`): `naming-conventions.md` defines `<Noun>Form` for list forms and no separate suffix for hosted screens.
  - [x] Replace the "Hiện cả người đã nghỉ" checkbox with the inline-label `Trạng thái:` ComboBox; `IOfficerView.ShowInactive` keeps its meaning ("Tất cả" = true).
  - [x] Status column: `CellPainting` draws an 8px square (`Success` for Đang công tác, `Danger` otherwise) before the text. Quản giáo stays a check column.
  - [x] Footer: "N cán bộ" left; "Insert thêm · Enter sửa" right in `muted`.
- [x] **T3. Edit dialogs** (AC: 2, 7, 8)
  - [x] Rebuild `OfficerEditForm` and `AddInmateForm` after `key-02` B with `InputFrame`s and the 2-column grid.
  - [x] Replace `ShowError(string)` on `IOfficerEditView` and `IAddInmateView` with field-aware errors (a field enum per view plus a banner for errors that name no field). The presenters map FluentValidation `PropertyName`s and the known duplicate-code messages to fields. Update `OfficerEditPresenterTests` and `AddInmatePresenterTests`.
  - [x] Rename `IOfficerEditView.DisplayText()` to `ShowModal()` (an R.1 mistranslation of `HienThi`, like the one fixed on the change-password view in R.2).
- [x] **T4. Deposit receipt as a hosted voucher screen** (AC: 3, 4, 7, 8)
  - [x] Convert `DepositReceiptForm` to a UserControl and host it; F2 and the Trang chủ tile from R.2 open it in the content area.
  - [x] Lay out the two columns and three cards. Money input: `InputFrame`, right-aligned, `#,##0` vi-VN while typing if the current control already formats; otherwise keep today's parsing (the shared money control is UX-DR1, a later story).
  - [x] Replace `btnClose`/"Đóng" with "Làm &mới" (navigation replaces closing). Add `IDepositReceiptView.Reset()` and a `ResetClicked` event; the presenter reloads the detainee list. Update `DepositReceiptPresenterTests`.
  - [x] Posting failure keeps its MessageBox ("Không ghi sổ được"), as `EXPERIENCE.md` allows for server re-check failures; validation errors go to their fields.
- [x] **T5. Print preview** (AC: 6)
  - [x] Restyle the toolbar and status line of `PdfPreviewForm` through `AppTheme`. It stays a separate window (WebView2).
- [x] **T6. Role screen** (AC: 5) — only after T1
  - [x] Convert to a hosted UserControl in two `CardPanel`s; replace `lblMessage` colours with a `Banner`. `IRoleView.ShowMessage`/`ShowError` keep their signatures.
- [x] **T7. Shell wiring** (AC: 1, 3, 5)
  - [x] Point the R.2 nav model entries for Cán bộ, Lập biên nhận thu and Vai trò at the content host. Remove the window-opening code they replace from `Navigator`.
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

Claude Opus 5.5 (Amelia, bmad-agent-dev). The `bmad-build` workflow could not render (`uv` is not installed), so at the user's request the story was implemented directly from its tasks.

### Approach

- Dependency gate: R.2, 2.1 and 2.3 are `review`, 1.8 is `done`.
- **T1:** the role-screen sketch (two `CardPanel`s side by side, role list 260px, matrix filling the rest) was put to the user, who answered "just implement". That was taken as a yes to the proposed sketch. No prototype was added through `bmad-ux`.
- Prototype patterns: `OfficerForm` → key-02 A; `OfficerEditForm`, `AddInmateForm` → key-02 B; `DepositReceiptForm` → key-03 A (rules 1, 4, 5, 6); `RoleForm` → the T1 sketch; `PdfPreviewForm` → restyle only.
- **Field-aware errors need property names**, which Application only returned as the first message or as one joined string. Application tests were written first (red), then:
  - `AddInmateResult.Errors` and `PostingResult.Errors` carry every FluentValidation failure. `Message` is unchanged, so existing callers and tests still work.
  - `OfficerService` throws `RequestValidationException : BusinessRuleException`, which carries the failures. Its message is the same joined text as before.
- WinForms `Common/`:
  - `FieldMessage<TField>`, plus `FieldMessages.Split`, which maps validation failures to a form's field enum and sends the rest to the banner.
  - `FieldErrorDisplay<TField>`: marks each field red with its message, focuses the first field on screen (the enum is in visual order), clears a field's error when its value changes, and re-enables input.
  - `EditDialogLayout`: the key-02 B anatomy (head, 2-column body, subtle footer, primary last), shared by both dialogs and meant to be copied for new catalogue dialogs.
  - `InputFrame.InlineLabel` for list filters; `InputFrame` also hosts `NumericUpDown` and `DateTimePicker`.
  - `AppTheme.SetGlyph` (an icon-font glyph drawn before button text in the button's text colour) and `AppTheme.PaintStatusCell` (the 8px status square plus text).
- Field enums: `OfficerField`, `InmateField`, `DepositReceiptField`. The presenters map `PropertyName` and the known duplicate-code constants (`OfficerService.DuplicateCodeMessage`, `AddInmateService.DuplicateCodeMessage`) to fields. Anything else goes to the dialog's `Banner`, or on the receipt to the existing "Không ghi sổ được" MessageBox.
- Hosted screens: `OfficerForm`, `DepositReceiptForm` and `RoleForm` are now UserControls. `Navigator.OpenOfficers`, `OpenDepositReceipt` and `OpenRoles` call `IContentHost.ShowPage` with the `ShellNavigation` keys, so F2, the Trang chủ tile and the sidebar all land in the content area. The window-opening code and the `DepositReceiptForm` DI registration are gone. Each screen loads once through `OnLoad`, and the presenters get `IServiceScopeFactory` only, never a scope.
- Keys:
  - List: Insert → Thêm, and Enter on the grid → Sửa (taken in `ProcessCmdKey` before the grid moves down a row).
  - Voucher: no AcceptButton, so Enter never posts; Esc → Làm mới unless a drop-down is open.
  - Dialogs: AcceptButton Lưu, CancelButton Hủy.

### Decisions and deviations

- **Visible-text changes** (AC 7). Besides " *", the dropped colons and the new `&` mnemonics:
  - Officer list:
    - the "Hiện cả người đã nghỉ" checkbox became the "Trạng thái:" filter ("Đang công tác" / "Tất cả", as the story asks);
    - the check column "Đang công tác" became the status column "Trạng thái", which shows "Đang công tác" / "Đã nghỉ" ("Đã nghỉ" is new, taken from the old checkbox wording);
    - the footer "N cán bộ · Insert thêm · Enter sửa" is from the story;
    - "&Thêm" became "&Thêm cán bộ", the prototype wording ("Thêm đối tượng").
  - Dialog subtitles: "Nhập thông tin đối tượng mới tiếp nhận." is verbatim from the prototype. "Nhập thông tin cán bộ mới." is that sentence adapted to staff. On Sửa the subtitle is "code · position".
  - Receipt:
    - card titles "Đối tượng", "Thông tin nộp tiền" and "Số tiền" come from the story;
    - "TỔNG TIỀN" is from the prototype;
    - the total shows "#,##0 đ";
    - "Đóng" became "Làm &mới".
  - Required-field markers added where the old form had none: "Mã cán bộ *", "Họ tên *".
  - Unchanged: every message, "Viết bằng chữ: …", "Đã ghi sổ … · Số dư mới: … đồng", and the "Hủy" / "Huỷ thay đổi" spellings.
- **Mnemonics:**
  - Receipt: G, I and M are fixed by AC 3, so the labels use "Nghiệ&p vụ", "&Người gửi", "Số tài &khoản", "Ngày &chứng từ", "Nội &dung" and "&Số tiền".
  - Role screen: "&Huỷ thay đổi" (AC 8 needs a mnemonic on every button).
  - Print preview: "&In ra máy in…" and "&Lưu thành tệp PDF…". The old captions were kept with a mnemonic added, rather than shortened to "&In".
- **Role screen messages** use the banner as AC 5 specifies: success in `Warning`, failure in `Error`. A success-green banner would read better, but DESIGN.md defines no `banner-success`, so this is left as a UX question. The matrix card's header is the selected role's name. The role list is a one-column, header-less grid styled by `AppTheme`, not a ListBox, so its selection uses the theme with no owner drawing. It raises `RoleChanged` only when the selected role actually changes, not on every rebind.
- **Dates** show as `dd/MM/yyyy` (DESIGN.md › Typography) instead of the Windows short date.
- **Receipt reset keeps Ngày chứng từ.** The next receipt is usually the same day, and resetting the date would need `IClock` in the view. "Số tài khoản" is a read-only `InputFrame` unless Chuyển khoản is chosen, and switching back to Tiền mặt clears its error.
- **Print preview** is 1100×720 (was 780 high), so it fits a 1366×768 screen with the taskbar.
- **Not done, per the story's scope:** the posting confirmation, balance-before row and F9 (Epic 4); the detainee picker and card (Epics 3–4); the shared money input (UX-DR1); the grid's Excel context menu (UX-DR3); permission-driven read-only (2.5). The permission matrix column headers still show the action codes (Xem, Them, …), as before.
- **Translations:** none needed; the story already used English identifiers.

### Verification

- `dotnet build LuuKyCanTin.slnx`: 0 errors. The only warning is the existing MSB3277 in `spikes/SP-01-QuestPdf`.
- `dotnet test LuuKyCanTin.slnx`: Domain 81, Application 117, WinForms 222 and Integration 221 all pass, none skipped (SQL Server 2022 in Docker/WSL). The first full run hit a stopped container (WSL had idled), and the integration suite was re-run once it was healthy.
- R.2's `ThemeUsageTests` scans the whole WinForms tree with only `AppTheme.cs` excluded, and it passes over every screen in this story.
- New tests:
  - `MasterDataScreenTests`: mnemonics, dialog pattern (width 480/560, FixedDialog, CenterParent, Lưu/Hủy), Insert → Thêm, the status filter, field marking and focus on the first field, clearing on edit, banner, heading.
  - `DepositReceiptScreenTests`: mnemonics, no AcceptButton when hosted, Esc → reset, total box, posted state, reset, account-number read-only and error clearing.
  - `RoleScreenTests`: mnemonics, banner kinds, selection and header; print-preview toolbar with one primary button.
  - `ShellScreenTests`: the `Navigator` hosts each module screen as a UserControl under its key.
  - Updated presenter tests: `OfficerEditPresenterTests`, `AddInmatePresenterTests`, `DepositReceiptPresenterTests` (reset, field errors), `OfficerPresenterTests` (`ShowModal`).
  - Application: `OfficerServiceTests`, `AddInmateServiceTests`, `CustodyLedgerServiceTests` (property names of failures).
- Printed receipt: nothing in Infrastructure, the print query or the template changed (`git diff` has no file there), so the PDF is produced by the same code from the same model. A byte comparison with a print from the previous build was **not** done.
- **Still open (T8):** the live keyboard-only run against the dev database, and the printed-receipt comparison. This workstation runs at 150%, and the screenshots below come from a DPI-unaware harness, not from driving the real app. Known harness limit: `DrawToBitmap` ignores window regions, so the screenshots still show the date picker's own inner border. On screen, `InputFrame` clips that border (it sets the picker's region), and the live run should confirm it.

### Screenshots

Rendered at 96 DPI (100%) by a DPI-unaware harness. Each shows the client area: the shell at 1366×737 under a 31px title bar, i.e. 1366×768. Dialogs show their client area. Folder: `r-3-screenshots/`.

| Screen | Mockup section | Screenshot |
|---|---|---|
| Staff register, "Tất cả", inactive rows | key-02 A | `r3-01-officers.png` |
| Thêm cán bộ, duplicate code under the field | key-02 B (Thêm) | `r3-02-officer-add-duplicate-code.png` |
| Sửa cán bộ, name heading, field error + banner | key-02 B (Sửa) | `r3-03-officer-edit-errors.png` |
| Thêm đối tượng, duplicate code under the field | key-02 B (Thêm) | `r3-04-add-inmate-duplicate-code.png` |
| Lập biên nhận thu, empty | key-03 A | `r3-05-receipt-empty.png` |
| Lập biên nhận thu, filled (total box, words) | key-03 A (Thanh toán rules) | `r3-06-receipt-filled.png` |
| Same receipt after Trang chủ and back (cached) | EXPERIENCE › Shell | `r3-07-receipt-after-switching-back.png` |
| Receipt validation under the fields | key-02 B rule 4 / EXPERIENCE › Field invalid | `r3-08-receipt-field-errors.png` |
| Receipt posted (number, new balance in success, In enabled) | key-03 A | `r3-09-receipt-posted.png` |
| Vai trò, saved (banner) | T1 sketch | `r3-10-roles-saved.png` |
| Vai trò, refused (error banner) | T1 sketch | `r3-11-roles-error.png` |
| Print preview chrome (WebView2 content does not render to a bitmap) | DESIGN › Button / Status bar | `r3-12-print-preview-chrome.png` |

### File List

Added:
- `src/Libraries/LuuKyCanTin.Application/Common/RequestValidationException.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Common/EditDialogLayout.cs`, `FieldErrorDisplay.cs`, `FieldMessage.cs`, `FieldMessages.cs`
- `src/Presentation/LuuKyCanTin.WinForms/MasterData/OfficerField.cs`, `InmateField.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Custody/DepositReceiptField.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/MasterData/MasterDataScreenTests.cs`, `Custody/DepositReceiptScreenTests.cs`, `Administration/RoleScreenTests.cs`
- `_bmad-output/implementation-artifacts/stories/refactor/r-3-screenshots/*.png`

Modified:
- `src/Libraries/LuuKyCanTin.Application/Custody/CustodyLedgerService.cs`, `PostingResult.cs`
- `src/Libraries/LuuKyCanTin.Application/MasterData/AddInmateResult.cs`, `AddInmateService.cs`, `OfficerService.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Common/AppTheme.cs`, `Glyphs.cs`, `InputFrame.cs`, `PdfPreviewForm.cs`
- `src/Presentation/LuuKyCanTin.WinForms/MasterData/OfficerForm.cs`, `OfficerForm.Designer.cs`, `IOfficerView.cs`, `OfficerPresenter.cs`, `OfficerEditForm.cs`, `OfficerEditForm.Designer.cs`, `IOfficerEditView.cs`, `OfficerEditPresenter.cs`, `AddInmateForm.cs`, `AddInmateForm.Designer.cs`, `IAddInmateView.cs`, `AddInmatePresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Custody/DepositReceiptForm.cs`, `DepositReceiptForm.Designer.cs`, `IDepositReceiptView.cs`, `DepositReceiptPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/RoleForm.cs`, `RoleForm.Designer.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/Navigator.cs`, `INavigator.cs`, `Program.cs`
- `tests/LuuKyCanTin.Application.UnitTests/Custody/CustodyLedgerServiceTests.cs`, `MasterData/AddInmateServiceTests.cs`, `MasterData/OfficerServiceTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/MasterData/OfficerEditPresenterTests.cs`, `OfficerPresenterTests.cs`, `AddInmatePresenterTests.cs`, `Custody/DepositReceiptPresenterTests.cs`, `Shell/ShellScreenTests.cs`
- `_bmad-output/implementation-artifacts/stories/README.md` (status)

## Change Log

| Date | Change |
|---|---|
| 2026-10-03 | Story created (bmad-create-epics-and-stories): second half of the UI alignment refactor, after R.2. |
| 2026-10-03 | Implemented (bmad-agent-dev, render step skipped at the user's request because `uv` is missing): hosted staff register, receipt and role screens; key-02 B dialogs with field-aware errors; print-preview restyle; property-aware validation results in Application. T1 taken as approved from "just implement". Status → review; T8's live run and print comparison remain open. |
