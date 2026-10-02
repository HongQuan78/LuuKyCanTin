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
- Ở lần đăng nhập đầu tiên, hệ thống **bắt buộc đổi mật khẩu** (`PhaiDoiMatKhau = 1` trong migration `AddDangNhapBaoMat`). Sau khi đổi, mật khẩu mới phải có ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số.
- Hash khởi tạo nằm trong migration `AddWalkingSkeletonTables` (một chuỗi cố định, sinh một lần ngoại tuyến bằng `Pbkdf2MatKhauHasher` trong Infrastructure).

## Đăng nhập, khoá tài khoản và phân quyền

- Sai mật khẩu 5 lần liên tiếp thì tài khoản bị khoá. Thời gian khoá đọc từ `appsettings.json`:
  `"DangNhap": { "ThoiGianKhoaPhut": 15 }`. Giá trị `0` nghĩa là chỉ quản trị viên mở khoá (Story 2.4).
  Vì các máy trạm dùng chung một cơ sở dữ liệu, **mọi máy phải cấu hình cùng một giá trị**.
- `--seed-demo` tạo thêm một tài khoản cho mỗi vai trò chuẩn (mật khẩu chung `Demo@2026`, không buộc đổi):

  | Tên đăng nhập | Vai trò |
  |---|---|
  | `luuky` | Cán bộ theo dõi tiền lưu ký |
  | `cantin` | Cán bộ căn tin / bán hàng |
  | `quangiao` | Cán bộ quản giáo |
  | `lanhdao` | Chỉ huy phụ trách / Lãnh đạo đơn vị |
  | `ketoan` | Kế toán đơn vị |

- Màn hình **Hệ thống › Vai trò** cho phép quản trị viên bật/tắt quyền của từng vai trò; thay đổi được ghi vào nhật ký kèm danh sách quyền trước/sau.


## Điều kiện máy trạm

- **.NET Desktop Runtime**: chỉ cần khi bản phát hành không self-contained (bản self-contained qua Velopack, Story 1.6/7.4, không cần).
- **WebView2 Runtime**: bắt buộc để mở bản xem trước PDF, in và lưu tệp. Máy Windows 10/11 có sẵn Evergreen runtime thường tự cập nhật; máy không có Internet cần bộ cài ngoại tuyến `MicrosoftEdgeWebView2RuntimeInstallerX64.exe` (xem `docs/decisions/0001-pdf-engine-questpdf.md`).
