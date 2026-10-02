# Cài đặt và chạy thử (walking skeleton)

## Cấu hình kết nối

Sao chép `.env.example` cạnh tệp thực thi thành `.env` và đặt chuỗi kết nối:

```
ConnectionStrings__LuuKyCanTin=Server=.\SQLEXPRESS;Database=LuuKyCanTin;Integrated Security=true;TrustServerCertificate=true
```

Ứng dụng không đọc chuỗi kết nối từ `appsettings.json`; biến môi trường thật luôn thắng tệp `.env`.

## Tạo cơ sở dữ liệu (chỉ quản trị viên)

```
LuuKyCanTin.WinForms.exe --migrate
LuuKyCanTin.WinForms.exe --migrate --seed-demo --environment Development
```

`--seed-demo` chỉ chạy trong môi trường Development, hoặc khi xác nhận bằng `--force=<tên cơ sở dữ liệu>`.

## Tài khoản quản trị ban đầu

| Tên đăng nhập | Mật khẩu ban đầu |
|---|---|
| `admin` | `LuuKy@2026` |

- Mật khẩu được băm bằng PBKDF2-SHA256 (600.000 vòng, salt ngẫu nhiên 16 byte); không lưu dạng rõ.
- **Chưa có cách đổi mật khẩu.** Chức năng bắt buộc đổi ở lần đăng nhập đầu tiên và màn hình quản trị tài khoản chỉ có ở Epic 2 (FR1); ở bản walking skeleton này không có công cụ đổi mật khẩu nào. Vì vậy chỉ dùng tài khoản này ngoài môi trường phát triển khi đã chấp nhận rủi ro, hoặc sửa hash bằng tay trong cơ sở dữ liệu qua quản trị viên.
- Hash khởi tạo nằm trong migration `AddWalkingSkeletonTables` (một chuỗi cố định, sinh một lần ngoại tuyến bằng `Pbkdf2MatKhauHasher` trong Infrastructure).

## Điều kiện máy trạm

- **.NET Desktop Runtime**: chỉ cần khi bản phát hành không self-contained (bản self-contained qua Velopack, Story 1.6/7.4, không cần).
- **WebView2 Runtime**: bắt buộc để mở bản xem trước PDF, in và lưu tệp. Máy Windows 10/11 có sẵn Evergreen runtime thường tự cập nhật; máy không có Internet cần bộ cài ngoại tuyến `MicrosoftEdgeWebView2RuntimeInstallerX64.exe` (xem `docs/decisions/0001-pdf-engine-questpdf.md`).
