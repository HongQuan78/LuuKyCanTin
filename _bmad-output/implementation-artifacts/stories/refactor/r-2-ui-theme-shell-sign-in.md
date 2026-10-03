---
story: "R.2"
epic: refactor
title: UI theme, shared controls, shell and sign-in aligned with the prototypes
status: review
size: L
backlogItems: []
frsCovered: []
dependsOn: ["R.1", "2.2"]
---

# Story R.2: UI theme, shared controls, shell and sign-in aligned with the prototypes

Status: review

## Story

As a counter officer who works in the app all day,
I want the sign-in, the change-password dialog and the main window to look and behave like the approved UI prototypes,
So that the app feels calm and consistent, and every screen built after this one starts from the same theme, controls and shell.

## Acceptance Criteria

1. **Given** the tokens in `DESIGN.md` (colours, typography, spacing)
   **When** the story is finished
   **Then** a static `AppTheme` class in `src/Presentation/LuuKyCanTin.WinForms/Common/` exposes every token one-to-one (colours, fonts, sizes) and applies the shared styles (primary/secondary/disabled button, DataGridView, segmented control, banner)
   **And** when `SystemInformation.HighContrast` is on, `AppTheme` returns `SystemColors` instead of the brand colours

2. **Given** the components table in `DESIGN.md`
   **When** a screen needs a card or an input
   **Then** it uses the shared `CardPanel` (1px border, optional 44px header with `card-title`, body padding 16) and `InputFrame` (32px, borderless TextBox or ComboBox inside, optional leading glyph, states `input` / `input-focus` / `input-error` / `input-readonly`) from `Common/`
   **And** a field error is a red 9pt label with an error glyph under the field, which replaces `ErrorProvider`

3. **Given** any `.cs` file under `src/Presentation/LuuKyCanTin.WinForms/` except `AppTheme.cs`
   **When** the tests run
   **Then** a test fails if the file contains `Color.FromArgb`, a named `Color.<Name>` other than `Transparent`/`Empty`, `SystemColors.` or `new Font(`
   **And** a reusable test helper checks a form or UserControl for duplicate `&` mnemonics among its buttons and labels, and is applied to every screen this story touches

4. **Given** prototype `key-01` section A
   **When** the user opens the app
   **Then** `LoginForm` matches it: dark `side` left panel with the product name, white right panel with the "Đăng nhập" heading, labels above `InputFrame` fields, one primary "Đăng nhập" button
   **And** a failed sign-in or a locked account shows a `banner-error` above the fields, clears the password and focuses it; while signing in the button reads "Đang đăng nhập…", is disabled, and the cursor is `WaitCursor`

5. **Given** prototype `key-01` section B
   **When** the change-password dialog opens (forced at first sign-in or from the menu)
   **Then** it matches the edit-dialog layout (head with title and subtitle, labels above fields, footer band in `subtle` with buttons right-aligned and the primary last)
   **And** a live checklist shows each `PasswordPolicy` rule ticked green (✓) or grey (✕) as the user types, and "Lưu" is enabled only when every rule passes and the confirmation matches; a server-side failure maps to the field it concerns

6. **Given** prototype `key-01` section C
   **When** a user signs in
   **Then** `MainForm` has the sidebar (220px, Dock Left), the header bar (56px, page title and muted subtitle), the status bar (26px) and the content area (`surface`, padding 20), and the `MenuStrip` is gone
   **And** the sidebar shows Trang chủ, then the caps headers NGHIỆP VỤ and QUẢN LÝ with their groups, each item 36px (sub-items 32px, indented 47px), hover `side-hover`, active `side-active` with the 3px indicator, one group expanded at a time, and the user block (initials tile, name, role) at the bottom

7. **Given** the screens built so far
   **When** the user clicks a sidebar item
   **Then** the item opens the same screen as the old menu did (Cán bộ, Thêm đối tượng, Lập biên nhận thu, Vai trò, Đổi mật khẩu, Đăng xuất); a screen that is not yet a UserControl still opens in its own window until R.3 converts it
   **And** the shell can host a UserControl in the content area, created on first use and cached so its state survives switching screens

8. **Given** a signed-in user
   **When** the shell opens
   **Then** the content area shows Trang chủ: the greeting by time of day with the user's name ("Chào buổi sáng, …"), the date line `Thứ …, dd/MM/yyyy`, and one quick-action tile per **existing** screen in the prototype tile style (today only "Lập biên nhận thu", F2)
   **And** F2 opens that screen from anywhere in the shell

9. **Given** the status bar
   **When** the shell is open
   **Then** it shows the items whose data exists today, each with an 8px square dot: CSDL (server / database, green when connected), Máy (workstation name) and the app version
   **And** items whose feature isn't built yet (Kỳ, Sao lưu) are left out, not faked

10. **Given** every screen this story touches
    **When** it is used with the keyboard only
    **Then** the tab order follows the visual order, Enter triggers the AcceptButton, Esc the CancelButton, and every button has a unique `&` mnemonic
    **And** nothing is clipped at 1366×768, 100% DPI

11. **Given** the presenters and their tests
    **When** the views are redesigned
    **Then** presenters still talk only to view interfaces, the existing presenter tests pass (updated only where an interface member changes), and no business rule moves into a Form

12. **Given** the finished change
    **When** `dotnet build` and `dotnet test` run
    **Then** both pass, and the Dev Agent Record holds a 1366×768 screenshot of each touched screen next to its mockup section

## Tasks / Subtasks

- [x] **T1. `AppTheme`** (AC: 1)
  - [x] `Common/AppTheme.cs`: one property per `DESIGN.md` colour token, the eight fonts of the typography ramp, and the spacing constants. Names follow the token names in English PascalCase (`Accent`, `AccentSoft`, `Side`, `SideHover`, `Surface`, `Card`, `Subtle`, `Border`, `RowLine`, `InputBorder`, `Text`, `Text2`, `Label`, `Muted`, `Placeholder`, `Success`, `Danger`, `Warning` and the `-soft` variants; `BodyFont`, `LabelFont`, `SmallFont`, `CardTitleFont`, `DialogTitleFont`, `PageTitleFont`, `GreetingFont`, `AmountMediumFont`, `AmountLargeFont`, `IconFont(float size)`).
  - [x] Style helpers: `StylePrimary(Button)`, `StyleSecondary(Button)`, `StyleGrid(DataGridView)`, `StyleSegment(RadioButton)`. Disabled buttons switch to `button-disabled` through `EnabledChanged`.
  - [x] High-contrast branch (AC 1). Fonts are created once and reused; never dispose a shared font.
- [x] **T2. Shared controls** (AC: 2)
  - [x] `Common/CardPanel.cs`: Panel that paints a 1px `Border` rectangle, with an optional header text (44px, bottom border).
  - [x] `Common/InputFrame.cs`: Panel that hosts one borderless child (TextBox or ComboBox) and an optional glyph, paints 1px `InputBorder`, 2px `Accent` on focus, 2px `Danger` when `HasError`, `Subtle` background when `ReadOnly`. Expose `HasError`, `ReadOnly` and `Inner`.
  - [x] `Common/FieldError.cs` (or a method on `InputFrame`): the red message label under a field, hidden when empty.
  - [x] `Common/Banner.cs`: Panel with a 3px left bar, glyph and message; `Error` and `Warning` kinds.
- [x] **T3. Convention tests** (AC: 3)
  - [x] `WinForms.UnitTests/Common/ThemeUsageTests.cs`: scans the WinForms source tree for the banned patterns, excluding `AppTheme.cs`, `obj/` and `bin/`.
  - [x] `TestUtilities/MnemonicAssert.cs`: walks a control tree (STA thread) and fails on duplicate mnemonic letters. Use it for `LoginForm`, `ChangePasswordForm` and `MainForm`.
- [x] **T4. `LoginForm`** (AC: 4, 10)
  - [x] Rebuild the Designer layout after `key-01` A. Keep `ILoginView` as it is, and add a `IsBusy { set; }` member for the "Đang đăng nhập…" state if the presenter doesn't already expose one. Show errors in the banner instead of `lblError`.
- [x] **T5. `ChangePasswordForm`** (AC: 5, 10)
  - [x] Rebuild after `key-01` B. Add a pure `PasswordPolicy.Evaluate(string)` in Domain that returns each rule with pass/fail, if it doesn't exist yet, with unit tests; the dialog's checklist and the existing `ChangePasswordService` check use the same rules.
  - [x] Replace `ShowError(string)` with field-aware errors (e.g. `ShowFieldError(PasswordField, string)` plus a banner for the rest). Update `ChangePasswordPresenterTests`.
  - [x] Rename `IChangePasswordView.DisplayText()` to `ShowModal()`. `DisplayText` is an R.1 mistranslation of `HienThi`.
- [x] **T6. Shell** (AC: 6, 7, 9, 11)
  - [x] Replace the `MenuStrip` with `Shell/Sidebar` (a UserControl built from a plain nav model: group, caption, glyph, shortcut text, action), `Shell/HeaderBar` and `Shell/StatusBar`. The nav model is a static list in the shell for now; story 2.5 replaces its source with the permission-driven registry, so keep it a plain data model, not Designer controls.
  - [x] Content host: `INavigator` gains a way to show a cached UserControl in the content panel and set the header title. Existing `Open…` methods keep opening their Forms (AC 7).
  - [x] Move `MainForm.OnAddInmate` and `OnCreateDepositReceipt` into `Navigator` (`OpenAddInmate()`, `OpenDepositReceipt()`), so the shell view composes no screen itself.
  - [x] `IMainView` changes from one event per menu item to the nav model (e.g. `ShowNavigation(IReadOnlyList<NavGroup>)` plus `NavigationRequested`), so new screens don't add events. Update `MainPresenterTests`.
  - [x] Status-bar items: database from the connection string (server and catalog only, never credentials), workstation `Environment.MachineName`, version from the assembly. The dot is green when the startup schema check passed.
- [x] **T7. Trang chủ** (AC: 8)
  - [x] `Shell/HomePage` UserControl hosted by default after sign-in: greeting (`IClock` for the time of day: sáng before 11:00, chiều before 18:00, tối after), date line, quick-action tiles from the same nav model (only entries flagged as tiles).
  - [x] F2 through `MainForm.ProcessCmdKey`.
- [x] **T8. Verify** (AC: 10, 12)
  - [x] Build and test. Run the app at 1366×768, 100%: sign in (wrong password, locked account, forced change), open each sidebar item, sign out. Save the screenshots and list them in the Dev Agent Record.

## Dev Notes

- **Prototype patterns:** `LoginForm` → key-01 A; `ChangePasswordForm` → key-01 B (edit-dialog shape); `MainForm` + Trang chủ → key-01 C. Open each mockup in a browser at 100% next to the designer; 1 CSS px = 1 WinForms px.
- **Scope is visual and interaction only.** Don't add business behaviour that belongs to a later story:
  - Permission-driven hiding of sidebar items and tiles → 2.5. This story shows every item, as the old menu did.
  - Lock overlay, Ctrl+L and the lock button in the user block → 2.7 (it should build the overlay to key-01 D on top of this shell).
  - Quick search box and Ctrl+K, F3/F4/F5 tiles and shortcuts, the period and backup status items and the backup warning banner → their own stories (Epics 4–7, 13). Leave room for the search box in the header; don't draw an inert one.
- **Visible text:** keep existing captions, including the "Hủy" spelling (`EXPERIENCE.md` open question). The changes allowed are the required-field marker (` *` instead of `(*)`), removing colons after labels above fields, adding `&` mnemonics, and the new texts that come verbatim from the prototype (sidebar headers, greeting, tile descriptions, status labels).
- **Sidebar captions** come from `EXPERIENCE.md` › Information Architecture. Groups with no built screen yet (Căn tin, Báo cáo) are not shown.
- **Sidebar unit name:** the second line under "Lưu ký – Căn tin" is the unit name from `UnitInformation` (seeded empty in 1.8). Show nothing when it's empty.
- Icons are Segoe Fluent Icons / Segoe MDL2 Assets glyphs drawn as Label text (the codes are in the mockup HTML, e.g. `&#xE80F;` for Trang chủ). Don't add image files.
- `AutoScaleMode.Font`, body font set once on each Form/UserControl through `AppTheme.BodyFont`.
- Changing `IMainView` from per-item events to a nav model is the one structural change here. It's what makes 2.5's registry a drop-in, so do it now rather than adding more events.
- `docs/conventions/ui-prototype-conventions.md` names this refactor as the story that creates `AppTheme`, `CardPanel` and `InputFrame`. Its review checklist applies to every screen here.

### Follow-ups for other stories

- **2.5** builds its permission-driven "menu and toolbar" as the source of this story's sidebar nav model and the Trang chủ tiles.
- **2.7** builds the lock overlay (key-01 D) as a Dock Fill panel over this shell.
- **R.3** brings the remaining screens into line and moves them into the content area.

## References

- `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/DESIGN.md` (tokens, components, do's and don'ts)
- `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/EXPERIENCE.md` (information architecture, state patterns, shortcuts, accessibility floor)
- `_bmad-output/planning-artifacts/ux-designs/ux-TienGuiLuuKy-2026-10-02/mockups/key-01-dang-nhap-shell.html` (sections A, B, C)
- `docs/conventions/ui-prototype-conventions.md`, `docs/conventions/naming-conventions.md`
- Current code: `src/Presentation/LuuKyCanTin.WinForms/Shell/` (`LoginForm`, `ChangePasswordForm`, `MainForm`, `MainPresenter`, `Navigator`), `src/Libraries/LuuKyCanTin.Domain/Administration/PasswordPolicy.cs`

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (Amelia, bmad-agent-dev). The `bmad-build` workflow could not render (`uv` is not installed), so at the user's request the story was implemented directly from its tasks.

### Approach

- Dependency gate: R.1 and 2.2 are both `review` (developed, review pending).
- Prototype patterns: `LoginForm` → key-01 A; `ChangePasswordForm` → key-01 B (edit-dialog shape); `MainForm` + Trang chủ → key-01 C.
- Test-first per task. Domain: `PasswordPolicy.Evaluate` → `PasswordRule`/`PasswordRuleResult`; `Validate` now derives from it, so the checklist and `ChangePasswordService` share one rule set. Application: `PasswordRuleDisplayExtensions`, `ISignedInUserQuery`/`SignedInUserQuery` (user block, greeting name, unit name). WinForms: `AppTheme`, `Glyphs`, `CardPanel`, `InputFrame`, `FieldError`, `Banner`; shell `Sidebar`/`NavButton`, `HeaderBar`, `StatusBar`, `HomePage`; plain nav model (`NavItem`, `NavGroup`, `NavTile`, `NavigationModel`, `ShellNavigation`).
- `IMainView` moved from one event per menu item to `ShowNavigation(NavigationModel)` + `NavigationRequested`. The presenter runs the item's `Open` action, so 2.5 can swap `ShellNavigation.Create` for its registry.
- Content host: `MainForm` implements `IContentHost` (`ShowPage(key, title, create)`, created on first use and cached per session); `INavigator.ShowPage` forwards to it. `Navigator` is now created per shell session with the host (no longer a DI singleton) and owns `OpenAddInmate`/`OpenDepositReceipt`, so `MainForm` composes no screen.
- Status bar: `WorkstationInfo` (server / catalog only, `Environment.MachineName`, assembly version), registered as a singleton in `Program`.

### Decisions and deviations

- **Translations (naming convention):** sidebar → `Sidebar`, nav model → `NavigationModel`/`NavGroup`/`NavItem`, trang chủ → `HomePage`, unit name → `FacilityName` (glossary `Facility`).
- **`PasswordPolicy.Evaluate(newPassword, currentPassword)`** takes the current password too: the prototype's fifth rule ("Khác mật khẩu hiện tại") needs it.
- **Field-aware errors:** `IChangePasswordView.ShowError(string)` stays as the banner; `ShowFieldError(PasswordField, string)` was added. The presenter maps the service's message constants to the field: wrong current password → current; policy messages → new; mismatch → confirmation; locked/other → banner. The checklist is driven by the presenter (`InputChanged` → `ShowRuleResults`, `CanSave`), not by the form.
- **`AppTheme` extras beyond the DESIGN.md tokens:** `OnAccent` (the DESIGN.md "#FFFFFF" on accent/active), `BodySemiboldFont` (button-primary weight 600 at body size), `AmountInWordsFont` (DESIGN.md "Amounts in words: italic"), and a few layout constants (`CardHeaderHeight`, `NavSubIndent`, `LabelGap`, `TileHeight`, `ButtonPaddingHorizontal`). `AppThemeTests` checks every colour and typography token against the DESIGN.md front matter.
- **Theme scan (AC 3) covers the whole WinForms tree.** `RoleForm` and `DepositReceiptForm` used `Color.ForestGreen`/`Firebrick`/`DimGray` and one `new Font`; these were swapped one-for-one to `AppTheme.Success`/`Danger`/`Text2`/`AmountInWordsFont`. Their layout is untouched (R.3).
- **Mnemonics:** "Đăng &nhập" instead of the mockup's underlined Đ (Alt+Đ can't be typed on a US/Telex keyboard), and "Mật khẩu hiện &tại" / "Xác &nhận mật khẩu mới" so they don't clash with "Đăng &xuất" / "&Hủy". The sign-in window's ✕ (`btnClose`, the CancelButton/Esc) is a glyph button with no mnemonic, as the borderless window in the prototype requires.
- **User block name:** accounts aren't linked to staff yet (2.4), so the display name is the user name and the role is the role name(s). `SignedInUserDto.DisplayName` is where 2.4 plugs in the officer's full name.
- **Login side panel:** shows "Máy: … · v…"; the unit name is not shown before sign-in. It would need a DB read before sign-in and a new `ILoginView` member, and the story asked to keep `ILoginView` except `IsBusy`.
- **Change-password texts:** the existing "Lưu" and "Hủy" captions are kept; the forced secondary button reads "Đăng xuất" (prototype rule). The forced subtitle is the prototype text; the voluntary one keeps the old hint text.
- **Fix found while verifying:** Esc / ✕ on the sign-in window did nothing. The window is shown modeless, and a button's `DialogResult` only closes modal dialogs; the old "Hủy" button had the same bug. `btnClose` now closes the form explicitly (regression test `LoginForm_CancelButtonOnTheModelessForm_ClosesIt`).
- **Not done here (by the story's scope):** permission hiding (2.5), lock overlay/Ctrl+L/lock button (2.7), quick search, F3/F4/F5, period and backup status (their epics), screen conversion to UserControls (R.3), `IOfficerEditView.DisplayText` rename (R.3).

### Verification

- `dotnet build LuuKyCanTin.slnx`: 0 errors. The only warning is the existing MSB3277 in `spikes/SP-01-QuestPdf`.
- `dotnet test LuuKyCanTin.slnx`: Domain 81, Application 112, WinForms 186, Integration 221. All pass, none skipped (SQL Server 2022 in Docker/WSL).
- New WinForms tests: `ThemeUsageTests` (banned colour/font scan), `AppThemeTests` (tokens = DESIGN.md), `SharedControlTests`, `MnemonicAssert` (+ self-tests) applied to `LoginForm`, `ChangePasswordForm` (both modes) and `MainForm` with the nav model; `ShellNavigationTests`, `WorkstationInfoTests`, `HomeGreetingTests`, `UserInitialsTests`, `ShellScreenTests` (content-host caching, F2, busy state, Esc closes login), and updated `MainPresenterTests`, `LoginPresenterTests`, `ChangePasswordPresenterTests`.
- Live run against the dev database (`--migrate --seed-demo`), driven by keyboard only: wrong password (banner, password cleared and focused), lockout of the `cantin` demo account after 5 attempts, forced change for `admin` (live checklist, Lưu disabled, Đăng xuất returns to sign-in), sign-in as `luuky`, every sidebar group expanded with its mnemonic, F2 → Lập biên nhận thu, Cán bộ, Thêm đối tượng, Vai trò, Đổi mật khẩu (voluntary) each opened the same screen as the old menu, Trang chủ, Đăng xuất back to sign-in, Esc exits.
- The dev workstation runs at 150% (144 DPI). The live run shows the fixed-pixel layouts clip at that scale (sidebar sub-item text, tile description, status-bar text). This matches the existing screens, which don't scale either; 125%/150% zoom is epic 13 (EXPERIENCE.md › Accessibility Floor). The 100% screenshots below were rendered by a DPI-unaware harness at 96 DPI and show nothing clipped at 1366×768.

### Screenshots

Rendered at 96 DPI (100%); the shell at a 1366×737 client under a 31px title bar, i.e. 1366×768. Folder: `r-2-screenshots/`.

| Screen | Mockup section | Screenshot |
|---|---|---|
| Sign-in | key-01 A (normal) | `r2-01-login.png` |
| Sign-in, wrong password | key-01 A (error banner) | `r2-02-login-wrong-password.png` |
| Sign-in, account locked | key-01 A (banner variant) | `r2-03-login-locked.png` |
| Sign-in, signing in | key-01 A ("Đang đăng nhập…") | `r2-04-login-signing-in.png` |
| Change password, forced | key-01 B | `r2-05-change-password-forced-empty.png`, `r2-06-change-password-forced-typing.png` |
| Change password, server error under the field | key-01 B + EXPERIENCE edit-dialog | `r2-07-change-password-server-error.png` |
| Change password, voluntary | key-01 B (Hủy variant) | `r2-05-change-password-voluntary-empty.png` |
| Shell + Trang chủ | key-01 C | `r2-08-shell-home.png` |
| Sidebar groups expanded | key-01 C (sidebar rules) | `r2-09-shell-custody-expanded.png`, `r2-09-shell-master-data-expanded.png`, `r2-09-shell-administration-expanded.png` |

### File List

Added:
- `src/Libraries/LuuKyCanTin.Domain/Administration/PasswordRule.cs`, `PasswordRuleResult.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/PasswordRuleDisplayExtensions.cs`, `ISignedInUserQuery.cs`, `SignedInUserQuery.cs`, `SignedInUserDto.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Common/AppTheme.cs`, `Glyphs.cs`, `CardPanel.cs`, `InputFrame.cs`, `FieldError.cs`, `Banner.cs`, `BannerKind.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/Sidebar.cs`, `NavButton.cs`, `HeaderBar.cs`, `StatusBar.cs`, `HomePage.cs`, `IContentHost.cs`, `NavItem.cs`, `NavGroup.cs`, `NavTile.cs`, `NavigationModel.cs`, `ShellNavigation.cs`, `WorkstationInfo.cs`, `HomeGreeting.cs`, `UserInitials.cs`, `PasswordField.cs`
- `tests/LuuKyCanTin.Application.UnitTests/Administration/SignedInUserQueryTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Common/ThemeUsageTests.cs`, `AppThemeTests.cs`, `SharedControlTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellNavigationTests.cs`, `ShellScreenTests.cs`, `WorkstationInfoTests.cs`, `HomeGreetingTests.cs`, `UserInitialsTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/TestUtilities/MnemonicAssert.cs`, `MnemonicAssertTests.cs`, `StaThread.cs`, `RepositoryPaths.cs`
- `_bmad-output/implementation-artifacts/stories/refactor/r-2-screenshots/*.png`

Modified:
- `src/Libraries/LuuKyCanTin.Domain/Administration/PasswordPolicy.cs`
- `src/Libraries/LuuKyCanTin.Application/ApplicationServiceCollectionExtensions.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Program.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/LoginForm.cs`, `LoginForm.Designer.cs`, `ILoginView.cs`, `LoginPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/ChangePasswordForm.cs`, `ChangePasswordForm.Designer.cs`, `IChangePasswordView.cs`, `ChangePasswordPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/MainForm.cs`, `MainForm.Designer.cs`, `IMainView.cs`, `MainPresenter.cs`, `INavigator.cs`, `Navigator.cs`, `ShellApplicationContext.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/RoleForm.cs`, `Custody/DepositReceiptForm.Designer.cs` (theme tokens only)
- `tests/LuuKyCanTin.Domain.UnitTests/Administration/PasswordPolicyTests.cs`
- `tests/LuuKyCanTin.Application.UnitTests/Common/DisplayExtensionsTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/LuuKyCanTin.WinForms.UnitTests.csproj` (`UseWindowsForms`), `Shell/MainPresenterTests.cs`, `Shell/LoginPresenterTests.cs`, `Shell/ChangePasswordPresenterTests.cs`
- `_bmad-output/implementation-artifacts/stories/README.md` (status)

## Change Log

| Date | Change |
|---|---|
| 2026-10-03 | Story created (bmad-create-epics-and-stories): first half of the UI alignment refactor, split from R.3 at the user's request. |
| 2026-10-03 | Implemented (bmad-agent-dev, render step skipped at the user's request because `uv` is missing): theme, shared controls, convention tests, sign-in, change password, shell and Trang chủ. Fixed Esc/✕ not closing the sign-in window. Status → review. |
