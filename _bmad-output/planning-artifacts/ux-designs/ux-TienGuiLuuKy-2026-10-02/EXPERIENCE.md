---
name: LuuKyCanTin
status: draft
updated: 2026-10-02
sources:
  - _bmad-output/planning-artifacts/epics.md
  - _bmad-output/planning-artifacts/epics/epic-02-secure-access-unit-setup.md
  - _bmad-output/planning-artifacts/epics/epic-03-detainee-register.md
  - _bmad-output/planning-artifacts/epics/epic-10-canteen-sales.md
  - _bmad-output/planning-artifacts/epics/epic-13-faster-counter-work.md
  - _bmad-output/implementation-artifacts/stories/epic-02/
  - document/ (feature list, DB design, process)
---

# LuuKyCanTin – EXPERIENCE

This file covers how the app behaves. How it looks is in [DESIGN.md](DESIGN.md), and tokens are referenced as `{path}`. The prototypes in `mockups/` are binding for developers (see `docs/conventions/ui-prototype-conventions.md`). When a prototype and the spines disagree, the spines win.

## Foundation

- **Form factor:** a Windows 10/11 desktop app, WinForms .NET 10, on a few LAN workstations (counter, canteen, office). Offline, keyboard-first. A USB keyboard-wedge barcode scanner is used at the canteen.
- **UI system:** WinForms controls with the modern flat look in DESIGN.md: stock controls restyled, plus two shared controls (CardPanel, InputFrame) and a central `AppTheme`. MVP: passive Views behind interfaces, Presenters call Application services.
- **Shell:** `MainForm` has a sidebar, a header bar, a status bar and a content area. Each module screen is a UserControl hosted in the content area. It is created on first use and cached, so its state survives switching screens and locking. Edit and confirmation forms are modal dialogs [ASSUMPTION].

## Information Architecture

The sidebar navigation tree [ASSUMPTION on grouping; captions from the stories where defined]. Trang chủ is first. The groups sit under two caps headers, NGHIỆP VỤ (Lưu ký, Căn tin, Báo cáo) and QUẢN LÝ (Danh mục, Hệ thống). Only one group is expanded at a time, and Khoá máy / Đăng xuất also live in the user block at the bottom.

| Group | Items |
|---|---|
| Hệ thống | Tài khoản · Vai trò · Thông tin đơn vị · Cấu hình người ký · Nhật ký thao tác · Đổi mật khẩu · Khoá máy (Ctrl+L) · Đăng xuất |
| Danh mục | Cán bộ · Đối tượng · Hàng hoá · Bảng giá |
| Lưu ký | Lập biên nhận thu… (F2) · Lập phiếu chi… (F3) · Màn hình quầy (F5) · Danh sách chứng từ lưu ký · Tất toán… · Tìm nhanh… (Ctrl+K) |
| Căn tin | Nhập hàng · Bán hàng (F4) · Danh sách phiếu bán hàng |
| Báo cáo | Báo cáo lưu ký › … · Báo cáo hàng hoá › … |
| Trang chủ | Greeting, quick-action tiles F2 / F3 / F4 / F5 (only those permitted), system warning banners |
| (F1) | Hướng dẫn: opened by F1, no sidebar entry |

- A sidebar item is **hidden** when the user lacks its `<Module>.Xem` permission, and a group left with no items is hidden too. Trang chủ tiles follow the same rule.
- When the user has Xem but not the write permission, the screen opens read-only: write buttons are disabled and dialogs open read-only.
- Services re-check permissions in every case, and a refusal shows "Bạn không có quyền thực hiện thao tác này".

| Surface | Prototype |
|---|---|
| DangNhapForm, DoiMatKhauForm, MainForm + Trang chủ, lock overlay | [key-01](mockups/key-01-dang-nhap-shell.html) |
| DoiTuongForm / DoiTuongEditForm and every other list + edit pair | [key-02](mockups/key-02-doi-tuong.html) |
| BanHangForm and every other voucher-entry screen | [key-03](mockups/key-03-ban-hang.html) |
| Reports, import wizard, counter screen (F5), quick search (Ctrl+K) | spine-only for now; no prototype |

## Voice and Tone

- **Vietnamese, administrative, short.** Labels are nouns above the field, with no colon; required fields end with " *" (`Họ và tên *`). Inline filter labels keep a colon (`Loại: Tất cả`). Buttons are verbs with a mnemonic (`&Lưu`, `&Ghi sổ`).
- **Domain terms are copied verbatim from the spec:** đối tượng, biên nhận thu, phiếu chi, ghi sổ, huỷ, số dư lưu ký.
- **Messages name the problem and the next step, with no blame and no technical detail.** Examples: "Tên đăng nhập hoặc mật khẩu không đúng", "Không đủ số dư lưu ký", "Mã số đã tồn tại".
- **No stack traces.** Unexpected errors show a friendly message and are logged.
- **Spelling:** the spec spells "huỷ" and "khoá", but the existing button says "Hủy". Keep each existing caption as it is and use the spec spelling for new text [OPEN QUESTION: unify?].

## Component Patterns

| Pattern | Behaviour |
|---|---|
| **List screen** (key-02 A) | Search box: accent-insensitive, 300 ms debounce, no Search button. Filters apply on change. One primary action button (Thêm …). Enter or double-click opens Sửa, Insert opens Thêm. Rarer actions (Chuyển loại…, Xuất Excel) are in the row context menu. The card footer shows the result count and keyboard hints. No delete for detainees. |
| **Edit dialog** (key-02 B) | Modal. Validation runs on Lưu: the invalid field gets a red border and a red message under it, and focus moves to the first invalid field. Server errors map to the same field. Esc = Hủy. A concurrency conflict asks the user to reload. |
| **Detainee picker** | Code or name, accent-insensitive, top 50, an exact code match first. Rows show MaSo, HoTen, NamSinh, BuongGiam and Loại. ↑/↓ and Enter select, Esc clears. Inactive detainees are labelled in red and refused on money screens. After a pick, the detainee card fills and focus moves on. |
| **Item lookup** (key-03 A) | Scanner or typing. The suggestion list shows mã, tên, ĐVT, giá, tồn. Enter adds quantity 1, and scanning the same item again adds 1 more. Out-of-stock items are refused. The box clears after each add. |
| **Voucher entry** (key-03 A) | Header → lines → total. The total and amount in words update live. The posting shortcut is F9 [ASSUMPTION]. Enter never posts. |
| **Posting confirmation** (key-03 C) | Shows Số dư trước → Số tiền → Số dư sau with the amount in words, and an optional print checkbox. Default button: Ghi sổ. Esc = Quay lại. |
| **Lock overlay** (key-01 D) | Password only, Mở khoá / Đăng xuất link. Đăng xuất asks for confirmation. Underlying screens keep their state. |
| **Buyer type** (key-03 A) | Segmented control: Đối tượng, Người thân, CBCS, Đơn vị đến công tác. It switches the buyer fields and the payment method, and hides the balance rows for cash buyers. |
| **Change password** (key-01 B) | A live policy checklist (✓ green / ✕ grey). Lưu is enabled only when every rule passes. |

## State Patterns

| State | Treatment |
|---|---|
| Sign-in failure or lockout | Red banner above the fields (`banner-error`). The password is cleared and focused (key-01 A). |
| Signing in | The button reads "Đang đăng nhập…" and is disabled. WaitCursor. |
| Field invalid | `input-error` border plus a red message under the field. |
| Read-only by permission | Write buttons disabled, fields ReadOnly. |
| Inactive detainee | Trạng thái cell with a red dot and red text in lists. A red label in the picker. Refused on money screens. |
| Positive or zero balance | `{colors.success}` semibold when positive, plain when zero. |
| Insufficient balance | The Thanh toán card shows a red "Còn thiếu N" row, the total box turns `total-box-shortfall`, and a banner explains "Không đủ số dư lưu ký". Ghi sổ is disabled (key-03 B). If the server's re-check fails, a MessageBox shows the same text. |
| Out of stock | The suggestion is greyed with a red "Hết hàng". At posting, the line turns red and the message names it. |
| Voucher states | Nháp → Đã ghi sổ → Đã huỷ (a reason is required). The draft preview carries a "NHÁP" watermark, and a reprint carries "BẢN IN LẠI". |
| Degraded connection or backup | The status-bar item gets a red dot, red semibold text and a tooltip, refreshed every 60 s. Trang chủ also shows a warning banner. |
| After sign-in | Trang chủ (greeting + quick-action tiles), not an empty area. |

## Interaction Primitives

- **Global shortcuts:**
  - F2 Biên nhận thu, F3 Phiếu chi, F4 Bán hàng, F5 Màn hình quầy
  - Ctrl+K Tìm nhanh, Ctrl+L Khoá máy, F1 Trợ giúp
- **In lists:** Insert = Thêm, Enter = Sửa.
- **In dialogs:** Enter = AcceptButton, Esc = CancelButton.
- **On voucher screens:** F9 = Ghi sổ, Delete = xoá dòng, Esc = làm mới [ASSUMPTION].
- **Mnemonics:** every button has an `&` mnemonic, and no two share a letter on the same form.
- **Tab order:** follows the visual order (top-to-bottom, left-to-right) and is set explicitly in the designer.
- **Mouse:** no action may require the mouse.

## Accessibility Floor

- Everything is keyboard-operable (see Interaction Primitives).
- All colours come from `AppTheme`. When Windows high contrast is on (`SystemInformation.HighContrast`), `AppTheme` returns `SystemColors` instead [ASSUMPTION].
- Colour is never the only signal: an inactive status also has its text, and a shortfall also says "Không đủ số dư lưu ký".
- No clipping at 1366×768 at 100%. Zoom to 125% and 150% arrives in epic 13.
- Every input has a visible label, and the label's `&` mnemonic focuses its input (the label precedes the input in tab order).

## Key Flows

[ASSUMPTION: protagonists and details are illustrative. Please replace them with a real session.]

**1. Lan starts her shift at the deposit counter.**
1. Lan, the officer who tracks custodial money, opens the app on QUAY-01 and types `lan.nt` and her password.
2. Her first sign-in forces Đổi mật khẩu. The checklist ticks green as she types, and Lưu mật khẩu lights up once every rule passes.
3. The shell opens on Trang chủ: "Chào buổi sáng, Nguyễn Thị Lan", four quick-action tiles, and a status bar showing Kỳ 10/2026 · đang mở and a backup taken 3 giờ trước.
4. She steps away. After 5 minutes the lock overlay covers the screen. She comes back, types her password and clicks Mở khoá.
5. **Climax:** her half-filled receipt is still there exactly as she left it.

**2. Hùng sells at the canteen counter.**
1. Hùng, the canteen officer, presses F4. Bán hàng opens with Loại người mua = Đối tượng and focus in Đối tượng.
2. He types `DT-2026-0012`. The card shows NGUYỄN VĂN AN, Buồng A2-05, and the Thanh toán card shows 1.250.000.
3. He scans 10 packs of Hảo Hảo (scanning the same code adds 1 each time), then types "sua" and presses Enter on Sữa tươi Vinamilk. He edits the quantity to 6.
4. The total reads 186.000 đ, Số dư sau khi mua 1.064.000 in green.
5. **Climax:** he presses F9. The confirmation shows 1.250.000 → 186.000 → 1.064.000 with the amount in words. He presses Enter, the slip prints, and the screen clears for the next buyer.
6. Variant: the next buyer has 85.000. The card turns red ("Còn thiếu 101.000") and Ghi sổ is disabled, so Hùng removes items until it fits.

**3. Lan looks up a namesake.**
1. In Danh mục › Đối tượng, Lan types "nguyen van". Eight rows appear, and two of them are NGUYỄN VĂN AN.
2. Năm sinh and Buồng/khu tell them apart. Former detainees show their status in red.
3. She presses Enter on the right one and Sửa opens with the Lịch sử chuyển loại history.

## Open Questions

- Should the spellings "Hủy" and "Huỷ" be unified?
- Is the sidebar + single content area shell confirmed, or are tabs for several open screens wanted?
- Is the sidebar grouping (NGHIỆP VỤ / QUẢN LÝ) confirmed?
- Is F9 the right posting key?
- Is the blue accent (#2563EB) right, or does the unit have a preferred colour?
- Is the Trang chủ page with quick-action tiles wanted?
