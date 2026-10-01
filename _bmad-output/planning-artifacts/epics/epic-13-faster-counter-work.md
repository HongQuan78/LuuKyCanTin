---
epic: 13
title: Faster counter work
release: R2
frsCovered: [FR65, FR66, FR67, FR69, FR70, FR72, FR79, FR86, FR88]
backlogItems: [TI-01, TI-02, TI-03, TI-05, TI-06, TI-08, TI-15, TI-22, TI-24]
dependsOn: [1, 2, 3, 4, 5, 6, 7, 8, 10]
---

## Epic 13: Faster counter work

Daily users serve each detainee faster once the core runs in production. One counter screen shows a detainee's balance and offers every action. Shortcuts and a global search replace menu hunting. The start-of-day screen and status bar show the state of the system at a glance, and the print, POS, help and zoom options remove small frictions. Every story here is a convenience over existing services. None adds a new way to change a balance or stock.

**Exit criteria (R2 gate for this epic):** per story; no regression in the R0.5/R1 integration suites and reconciliation.

**Applies to every story in this epic:** actions opened from a shortcut or quick button go through the same Presenters and services as the menu, so the service-level permission check (2.5) still applies; hidden or disabled UI is never the only control.

---

### Story 13.1: Counter screen for one detainee

`TI-01` · Size M · Depends on: 3.2, 4.5, 5.1, 5.4, 6.2, 10.3 · FR65

As a custodial or canteen officer,
I want to type or scan a detainee code and see their balance and recent activity with quick-action buttons,
So that I can serve the detainee in front of me from one screen.

**Acceptance Criteria:**

**Given** the counter screen
**When** the officer enters or scans a detainee code (or picks one with the picker 3.2)
**Then** it shows code, name, type, cell, status (red label if not managed), current `SoDuLuuKy`, the last 10 posted transactions, and pending transfer requests where the detainee is giver or receiver

**Given** the detainee is managed
**When** the officer clicks Receipt, Payout or Sale
**Then** the existing form (receipt 4.5, payout 5.1, POS 10.2/10.3) opens with the detainee pre-filled, and after posting the counter screen refreshes its balance and history

**Given** a user lacking permission for an action, or a detainee whose status ≠ managed
**When** the screen loads
**Then** that action button is disabled, and the service still refuses if the form is reached another way

---

### Story 13.2: Keyboard shortcuts

`TI-02` · Size S · Depends on: 13.1 · FR66

As a frequent user,
I want function-key shortcuts for the common actions,
So that I can work without the mouse.

**Acceptance Criteria:**

**Given** the main shell
**When** the user presses F2, F3, F4 or F5
**Then** these open respectively: new receipt, new payout, new sale (POS) and the counter screen, each only if the user holds that permission; otherwise nothing opens and the status bar says why

**Given** any menu item that has a shortcut
**When** the menu is displayed
**Then** the shortcut is shown next to the item, and a "Keyboard shortcuts" help list shows the full map

**Given** a data-entry form that is already open
**When** a global shortcut is pressed
**Then** it never discards unsaved input silently; the user is asked to confirm or the new form opens alongside it

---

### Story 13.3: Global search (Ctrl+K)

`TI-03` · Size M · Depends on: 1.7, 4.7, 8.3 · FR67

As any user,
I want one search box that finds documents, detainees and goods,
So that I can jump to any record without knowing which menu it lives under.

**Acceptance Criteria:**

**Given** the user presses Ctrl+K anywhere in the shell
**When** they type at least 2 characters
**Then** results are grouped as Documents (by `SoChungTu`/`SoPhieu`), Detainees (by `MaSo` or name, diacritic-insensitive via `Vietnamese_CI_AI`) and Goods (by `MaHang` or name), top 10 per group, refreshed incrementally using the pattern from spike 1.7

**Given** a result is selected with Enter or a click
**When** it opens
**Then** the matching view form opens (document detail, detainee record or counter screen, goods record)

**Given** a user without view permission for a module
**When** they search
**Then** results from that module are not returned by the query service

---

### Story 13.4: Start-of-day screen and month-end reminders

`TI-05` · Size S · Depends on: 6.5, 6.8, 8.4 · FR69

As a custodial officer or accountant,
I want a start-of-day summary and reminders as the month ends,
So that I start work knowing the state of the system and don't forget the month-end close.

**Acceptance Criteria:**

**Given** a user signs in for the first time on a calendar day (`IClock.Today`)
**When** the start-of-day screen shows (it can be turned off per user)
**Then** it lists: whether the current period is locked and the last locked period, the next document number per type for this year, goods whose effective price changes today, and the count of unposted drafts

**Given** the date is within the last 3 days of the month or the first 5 days of the next
**When** a user with the relevant permission signs in
**Then** reminders show: "Lock period MM/YYYY" (if not yet locked, for the accountant) and "Create the deposit remittance list" (if posted receipts in the period are not yet on a list, for the custodial officer)

**Given** a reminder is acted on (period locked, list created)
**When** the screen is shown again
**Then** that reminder disappears

---

### Story 13.5: Status bar

`TI-06` · Size S · Depends on: 7.2, 6.8 · FR70

As any user or administrator,
I want a status bar with who I am, where I am and the system's health,
So that I notice problems (no connection, old backup, wrong period) before they cause errors.

**Acceptance Criteria:**

**Given** the main shell is open
**When** it renders
**Then** the status bar shows signed-in user and role, workstation name, current period with open/locked state, DB connection state (server and database name), app version, and age of the last successful backup (from 7.2)

**Given** the DB connection drops or the last backup is older than 24 hours
**When** the status bar refreshes (every 60 s and after each operation)
**Then** the relevant item turns red with a tooltip explaining the issue

---

### Story 13.6: Print options

`TI-08` · Size S · Depends on: 4.4, 4.5 · FR72

As a custodial officer,
I want to print two A5 copies on one A4 sheet, auto-print after posting, and preview drafts,
So that printing matches how we file paper copies and fewer clicks are needed.

**Acceptance Criteria:**

**Given** an A5 template (e.g. the receipt) and the per-workstation setting "2 copies A5 on A4"
**When** the document is printed
**Then** the PDF has one A4 landscape page with two identical A5 copies, and `SoLanIn` increases by one per print action, not per copy

**Given** the per-user setting "auto-print receipt after posting" is on
**When** a receipt is posted
**Then** the print preview (or a direct print, per setting) opens immediately and the print is counted and audit-logged like a manual print

**Given** a draft (unposted) document
**When** the user previews it
**Then** the PDF carries a diagonal "NHÁP" watermark and has no document number if none is allocated yet, printing a draft never changes `SoLanIn`, and no print audit row is written

---

### Story 13.7: POS best-seller buttons

`TI-15` · Size S · P3 · Depends on: 10.2 · FR79

As a canteen cashier,
I want buttons for the best-selling items on the POS screen,
So that I can add the most common items with one click.

**Acceptance Criteria:**

**Given** posted sale lines in the last 30 days (`IClock.Today`)
**When** the POS screen opens
**Then** up to N buttons (default 12, configurable) show the top items by quantity sold, excluding discontinued items and items with no effective price

**Given** a best-seller button
**When** it is clicked
**Then** it adds the item as if scanned (quantity 1, or increments an existing line), with the same price and stock checks as 10.2

---

### Story 13.8: Per-form F1 help

`TI-22` · Size M · P3 · Depends on: 1.1 · FR86

As a new user,
I want to press F1 on any form and read how to use it,
So that I can learn the system without Internet access or calling the administrator.

**Acceptance Criteria:**

**Given** help pages are shipped with the app as local files (HTML or Markdown rendered in WebView2), one per form keyed by the form name
**When** the user presses F1 on a form
**Then** that form's help opens offline; if none exists, the help index opens

**Given** a new release
**When** help content changes
**Then** it is updated through the normal Velopack update, with no separate installation

---

### Story 13.9: Zoom on the counter screen and portrait photo in the picker

`TI-24` · Size M · P3 · Depends on: 13.1, 3.2 · FR88 · UX-DR12

As a counter officer,
I want a large-font mode on the counter screen and the detainee's photo in the picker,
So that I can read easily and confirm I am serving the right person.

**Acceptance Criteria:**

**Given** the counter screen
**When** the user switches zoom (100% / 125% / 150%, remembered per user)
**Then** fonts and grid rows scale without clipping or horizontal scrolling at 1366×768

**Given** the proposal to store photos as files on the LAN share (e.g. `\\server\LuuKyCanTin\photos\<MaSo>.jpg`) with only a relative path column (`DoiTuong.AnhChanDung nvarchar(260)` NULL) in the DB, not a blob (to stay well inside the Express 10 GB limit), confirmed with the PO before implementation
**When** a user with `DM-01.Sua` attaches a photo to a detainee
**Then** the image is resized (max 600 px), saved to the share, the path is stored, and the change is audit-logged

**Given** a detainee with a photo
**When** they appear in the picker or on the counter screen
**Then** the thumbnail shows; if the file is missing or the share is unreachable, a placeholder shows and nothing fails
