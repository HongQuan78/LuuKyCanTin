# Backlog nháp — Tiếp nhận tiền gửi lưu ký + căn tin (WinForms .NET 10)

Bản nháp rút ra từ phiên brainstorm ngày 29/09/2026 và danh sách 49 chức năng. Nguyên tắc chủ đạo là **ledger-first**: xây và test engine sổ cái `ChungTuLuuKy` (đánh số, ghi sổ có khoá dòng, đối chiếu) trước mọi form; biên nhận, phiếu chi, bán hàng, tất toán chỉ gọi vào engine này. Bước tiếp theo: tinh chỉnh bằng `bmad-create-epics-and-stories`.

**Quy ước:** ID dùng lại mã chức năng (HT-01, LK-T01…); story mới: `SP` spike, `NEN` nền tảng/cơ chế an toàn, `GAP` lỗ hổng spec, `DEC` quyết định, `TI` tiện ích. **(mới)** = không có trong spec gốc, bổ sung từ brainstorm. Size: S ≤ 2 ngày, M ≤ 1 tuần, L > 1 tuần.

---

## 1. Kế hoạch release

| Release | Mục tiêu | Phạm vi | Điều kiện ra release |
|---|---|---|---|
| **Sprint 0** | Nền tảng + gỡ rủi ro kỹ thuật | E0 (solution, CI, migration, interceptor nhật ký, IClock), SP-01..03, walking skeleton NEN-08; chốt các DEC chặn R0.5 | Walking skeleton chạy xuyên 4 lớp; CI xanh; DEC-02, DEC-05, DEC-09 đã chốt |
| **R0.5** | Chạy thực tế **chỉ lưu ký** (không căn tin) | HT, DM đối tượng/cán bộ, engine sổ cái, khung in + UI kit, LK-T, LK-C (trừ LK-C05), LK-BC, go-live lưu ký | Chạy song song sổ giấy 1 tháng, số dư cuối tháng khớp 100% (GAP-07) |
| **R1** | Căn tin | DM hàng hoá (DM-05..08), NH, BH, HH-BC, LK-C05, tồn đầu kỳ, khoá tồn đồng thời | Đối chiếu một chạm không chênh lệch cả số dư lẫn tồn kho; DEC-01, 03, 04, 06 đã chốt |
| **R2** | P2 + tiện ích | DM-03, LK-T04, BH-01, BH-03, các story TI, safeguard mở rộng | Theo từng story; DEC-07, DEC-08 đã chốt |

> Ghi chú ưu tiên: HT-06 (khoá sổ kỳ) và DM-09 (nhập Excel) là P2 trong spec nhưng **đề xuất nâng lên R0.5** — khoá sổ là cơ chế chống gian lận cốt lõi; nhập Excel cần để dry-run với số liệu thật.

---

## 2. Epic và story (theo thứ tự phụ thuộc)

### E0 — Nền tảng & spike
**Mục tiêu:** có khung solution chạy được, CI, và gỡ các rủi ro kỹ thuật (in PDF, cập nhật LAN) trước khi làm nghiệp vụ.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| NEN-01 | Khung solution + CI (mới) | 4 project + 3 project test, DI Host, Serilog, EF Core; CI build + test | P1 | Sprint 0 | — | M |
| NEN-02 | Migration đầu + seed data (mới) | Seed dùng cho dev/demo; bộ demo 200 đối tượng, 50 mặt hàng | P1 | Sprint 0 | NEN-01 | M |
| NEN-03 | Kiểm tra phiên bản CSDL khi khởi động (mới) | App từ chối chạy nếu CSDL chưa migrate đúng version | P1 | Sprint 0 | NEN-02 | S |
| NEN-04 | Test khớp enum Domain ↔ CHECK constraint (mới) | Test tự động quét mọi enum Domain, so với CHECK trong CSDL | P1 | Sprint 0 | NEN-02 | S |
| NEN-05 | Interceptor nhật ký thao tác (mới) | `SaveChanges` interceptor ghi `NhatKyThaoTac` (trước/sau) cho mọi bảng chứng từ | P1 | Sprint 0 | NEN-01 | M |
| NEN-06 | IClock (mới) | Mọi logic ngày dùng `IClock`; cho phép giả lập ngày khi test/tập huấn | P1 | Sprint 0 | NEN-01 | S |
| NEN-07 | Quy ước kiến trúc tầng Application (mới) | Service không phụ thuộc WinForms (không MessageBox); Presenter test bằng NSubstitute qua interface View | P1 | Sprint 0 | NEN-01 | S |
| NEN-08 | Walking skeleton (mới) | Đăng nhập → tạo 1 đối tượng → lập 1 biên nhận → ghi sổ → in PDF, xuyên 4 lớp | P1 | Sprint 0 | NEN-02, SP-01 | L |
| SP-01 | Spike QuestPDF + in (mới) | Giấy phép QuestPDF, font Unicode tiếng Việt, xem trước/in qua WebView2, khổ A4/A5 | P1 | Sprint 0 | — | M |
| SP-02 | Spike Velopack qua thư mục chia sẻ LAN (mới) | Cập nhật app từ share nội bộ, không Internet | P1 | Sprint 0 | NEN-01 | M |
| SP-03 | Spike tìm kiếm tiếng Việt (mới) | Collation `Vietnamese_CI_AI`, index, tìm tăng dần với hàng nghìn đối tượng | P1 | Sprint 0 | NEN-02 | S |

### E1 — Hệ thống (HT)
**Mục tiêu:** đăng nhập, phân quyền, đánh số, nhật ký, sao lưu — các cơ chế mọi nghiệp vụ dựa vào.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| HT-01 | Đăng nhập | Đăng nhập, đổi mật khẩu, đăng xuất; khoá sau 5 lần sai; mật khẩu băm, mật khẩu mạnh, buộc đổi lần đầu | P1 | R0.5 | NEN-01 | M |
| HT-02 | Người dùng và phân quyền | Tài khoản gắn cán bộ; vai trò; quyền chức năng × thao tác (xem/thêm/sửa/huỷ/in/duyệt); bảng Quyền sinh tự động (dạng `LK-C.Duyet`); kiểm tra quyền ở service | P1 | R0.5 | HT-01, DM-04 | L |
| NEN-09 | Tách nhiệm vụ (mới) | Người lập không được tự duyệt; ghi nhật ký mọi thao tác quản trị tài khoản | P1 | R0.5 | HT-02 | S |
| NEN-10 | Tự khoá phiên (mới) | Khoá màn hình khi rời máy, mở lại nhanh bằng mật khẩu | P1 | R0.5 | HT-01 | S |
| HT-03 | Thông tin đơn vị | Tên, địa chỉ đơn vị, in ở đầu mọi mẫu | P1 | R0.5 | NEN-02 | S |
| HT-04 | Cấu hình người ký | Mỗi mẫu in: danh sách chức danh ký, họ tên mặc định (`CauHinhKyTen`) | P1 | R0.5 | HT-03 | M |
| HT-05 | Đánh số chứng từ | `INumberingService` (`DemSoChungTu` + UPDLOCK) theo loại + năm; test 2 task song song trên LocalDB không trùng số | P1 | R0.5 | NEN-02 | M |
| GAP-03 | Chuyển năm số chứng từ (mới) | `DemSoChungTu` tự tạo dòng cho năm mới, không chỉ seed năm hiện tại; test qua 31/12 bằng IClock | P1 | R0.5 | HT-05, NEN-06 | S |
| HT-06 | Khoá sổ kỳ | Chứng từ trong kỳ đã khoá không sửa, không huỷ; kế toán khoá/mở kỳ. **Đề xuất nâng lên R0.5** | P2 (đề xuất P1) | R0.5 | HT-02 | M |
| NEN-11 | Chặn ngày chứng từ ngoài cửa sổ (mới) | Không cho lập/sửa lùi ngày về kỳ cũ trừ khi có quyền riêng | P1 | R0.5 | HT-06, NEN-06 | S |
| HT-07 | Nhật ký thao tác | Màn hình tra cứu: ai, lúc nào, thêm/sửa/huỷ/in gì, giá trị trước/sau; chỉ ghi thêm | P1 | R0.5 | NEN-05 | M |
| HT-08 | Sao lưu, phục hồi | Sao lưu thủ công và theo lịch hằng ngày; phục hồi từ file | P1 | R0.5 | NEN-02 | M |
| NEN-12 | Kiểm tra bản sao lưu (mới) | `RESTORE VERIFYONLY` định kỳ; cảnh báo trên màn hình quản trị khi backup cũ hơn 24h | P1 | R0.5 | HT-08 | S |

### E2 — Danh mục lưu ký
**Mục tiêu:** có danh mục đối tượng và cán bộ đủ để lập chứng từ lưu ký.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| DM-01 | Đối tượng | Mã số duy nhất, họ tên, loại, ngày vào, buồng/khu, quản giáo, trạng thái; tìm theo mã, tên (không dấu) | P1 | R0.5 | NEN-02, SP-03 | M |
| DM-02 | Chuyển loại đối tượng | Tạm giam → phạm nhân, lưu lịch sử; chứng từ giữ loại tại thời điểm lập | P1 | R0.5 | DM-01, NEN-15 | M |
| DM-04 | Cán bộ | Họ tên, chức vụ, vai trò (quản giáo, căn tin, chỉ huy, kế toán, lãnh đạo) | P1 | R0.5 | NEN-02 | S |
| DM-09 | Nhập từ Excel | Nạp đối tượng (R0.5), hàng hoá (R1); preview, báo lỗi từng dòng trước khi ghi, tải file mẫu; dry-run số liệu thật 1 tháng. **Đề xuất nâng ưu tiên** | P2 (đề xuất P1) | R0.5 / R1 | DM-01, DM-07 | M |
| DM-03 | Người thân | Họ tên, quan hệ, địa chỉ, số tài khoản; gợi ý người thân + số TK khi lập biên nhận | P2 | R2 | DM-01, LK-T01 | M |

### E3 — Engine sổ cái lưu ký (ledger-first)
**Mục tiêu:** một engine ghi sổ duy nhất, đúng tuyệt đối, chịu được nhiều máy cùng lúc; mọi nghiệp vụ tiền chỉ là caller.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| NEN-13 | Quy tắc số dư trong Domain — TDD (mới) | Số dư = tổng thu − tổng chi, không bao giờ âm; tiền là số nguyên đồng | P1 | R0.5 | NEN-01 | S |
| NEN-14 | State machine chứng từ chung (mới) | Nháp → Đã ghi sổ → Đã huỷ (có lý do); dùng cho biên nhận, phiếu chi, phiếu nhập, phiếu bán | P1 | R0.5 | NEN-13 | M |
| NEN-15 | Value object snapshot (mới) | Lưu họ tên, loại đối tượng, tên người mua lúc lập để in lại đúng bản gốc | P1 | R0.5 | NEN-14 | S |
| NEN-16 | `GhiSoLuuKyService` (mới) | Ghi `ChungTuLuuKy` + cập nhật `SoDuLuuKy` bằng UPDATE có điều kiện dưới khoá dòng; mỗi nghiệp vụ là 1 Unit of Work / 1 transaction | P1 | R0.5 | NEN-14, HT-05, DM-01 | L |
| NEN-17 | Test đồng thời chi trùng (mới) | 2 máy cùng chi cho 1 đối tượng → đúng 1 lệnh thành công | P1 | R0.5 | NEN-16 | S |
| NEN-18 | Chặn giao dịch theo trạng thái đối tượng (mới) | Chặn thu/chi/bán khi trạng thái ≠ đang quản lý | P1 | R0.5 | NEN-16 | S |
| NEN-19 | Đối chiếu sổ cái (mới) | So `SoDuLuuKy` với tổng `ChungTuLuuKy`; dùng làm assertion sau mọi integration test | P1 | R0.5 | NEN-16 | M |

### E4 — Khung in & UI kit
**Mục tiêu:** biến 12 mẫu in thành 1 story nền + 12 story nhỏ; các control nhập liệu dùng chung.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| NEN-20 | Khung mẫu in QuestPDF chung (mới) | Header từ `ThongTinDonVi`, khối chữ ký từ `CauHinhKyTen`, dấu "bản in lại"; mỗi mẫu chỉ viết phần thân | P1 | R0.5 | SP-01, HT-03, HT-04 | M |
| NEN-21 | Golden-file test mẫu in (mới) | Render PDF, so snapshot; duyệt đối chiếu với mẫu giấy đang dùng | P1 | R0.5 | NEN-20 | S |
| NEN-22 | Đếm số lần in (mới) | `SoLanIn`, ghi nhật ký in, watermark bản in lại kèm số lần | P1 | R0.5 | NEN-20, NEN-05 | S |
| NEN-23 | UI kit dùng chung (mới) | Ô nhập tiền; picker đối tượng (mã + buồng + năm sinh, nhãn đỏ nếu đã tất toán, tìm tăng dần); lưới chứng từ + chuột phải xuất Excel | P1 | R0.5 | DM-01, SP-03 | M |
| NEN-24 | Màn hình xác nhận trước ghi sổ (mới) | Hiện "số dư trước → số tiền → số dư sau" | P1 | R0.5 | NEN-16, NEN-23 | S |
| NEN-25 | Bộ lọc kỳ báo cáo dùng chung (mới) | Tháng/quý/năm/từ ngày–đến ngày; xem trước, in | P1 | R0.5 | NEN-20 | S |
| HH-BC04 | Xuất file | Xuất PDF, Excel cho mọi báo cáo (làm sớm vì LK-BC cần) | P1 | R0.5 | NEN-20, NEN-25 | M |

### E5 — Tăng tiền lưu ký (LK-T)
**Mục tiêu:** lập, in, huỷ biên nhận thu qua engine sổ cái.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| LK-T03 | Số tiền bằng chữ | Sinh chữ tiếng Việt, dùng chung mọi mẫu; bộ test biên: 0, 10, 15, 21, 101, 1.000.005, tỷ; "lẻ/linh", "mốt/một", "lăm/năm". Story làm quen quy trình đầu tiên | P1 | R0.5 | NEN-01 | S |
| LK-T01 | Lập biên nhận thu | 3 nguồn: mang theo khi vào trại, người thân gửi, nhận từ đối tượng khác; đủ trường mục A.I.4; gọi `GhiSoLuuKyService` | P1 | R0.5 | NEN-16, NEN-24, DEC-05 | M |
| LK-T02 | Hình thức nhận | Tiền mặt / chuyển khoản; chuyển khoản bắt buộc số tài khoản người gửi | P1 | R0.5 | LK-T01 | S |
| LK-T05 | In biên nhận | In Biên nhận thu tiền gửi lưu ký; in lại có dấu "bản in lại" | P1 | R0.5 | LK-T01, LK-T03, NEN-20, NEN-22 | S |
| LK-T06 | Sửa, huỷ biên nhận | Bắt buộc lý do; chỉ khi chưa khoá sổ và không làm số dư âm; cảnh báo khi huỷ chứng từ đã in | P1 | R0.5 | LK-T01, HT-06 | M |
| LK-T04 | Cảnh báo gửi tiền trong tháng | Người thân gửi > 1 lần/tháng cho cùng đối tượng → cảnh báo, không chặn | P2 | R2 | LK-T01, DM-03 | S |

### E6 — Giảm tiền lưu ký (LK-C)
**Mục tiêu:** chi, trích tiền cho nhau, tất toán — không bao giờ chi vượt số dư.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| LK-C01 | Lập phiếu chi | Loại: mua hàng, cho tiền, chuyển về người thân, chuyển trại, chấp hành xong án | P1 | R0.5 | NEN-16, NEN-24, DEC-02 | M |
| LK-C02 | Kiểm tra số dư | Không chi vượt số dư; khoá dòng khi ghi sổ để 2 máy không chi trùng | P1 | R0.5 | NEN-16, NEN-17 | S |
| LK-C03 | Trích tiền cho đối tượng khác | Đề nghị → BGT duyệt → tự sinh phiếu chi bên cho + biên nhận bên nhận, cùng số biên bản, 1 transaction; người lập không tự duyệt | P1 | R0.5 | LK-C01, LK-T01, NEN-09 | L |
| LK-C04 | Tất toán khi chuyển trại, ra trại | Nút một chạm: chi hết số dư, in phiếu chi, đổi trạng thái, chặn giao dịch mới; chỉ đóng hồ sơ khi số dư = 0 | P1 | R0.5 | LK-C01, NEN-18 | M |
| LK-C06 | In phiếu chi | Số dư kỳ trước, số chi, số còn được sử dụng, 4 chữ ký xác nhận | P1 | R0.5 | LK-C01, NEN-20 | S |
| LK-C05 | Chi mua hàng tự động | Sinh từ phiếu bán hàng, không lập tay | P1 | R1 | BH-05 | S |

### E7 — Báo cáo & đối chiếu lưu ký (LK-BC)
**Mục tiêu:** sổ theo dõi có sớm để kế toán kiểm; đối chiếu được mọi lúc.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| LK-BC04 | Tra cứu số dư | Số dư hiện tại + lịch sử giao dịch dạng timeline của một đối tượng | P1 | R0.5 | NEN-16, NEN-23 | M |
| LK-BC01 | Bảng kê theo dõi của một đối tượng | Đầu kỳ, từng chứng từ nhận/chi trong kỳ, số còn được sử dụng | P1 | R0.5 | NEN-16, NEN-25 | M |
| LK-BC02 | Sổ theo dõi toàn đơn vị | Mỗi đối tượng một dòng: đầu kỳ, nhận, chi, còn lại | P1 | R0.5 | NEN-16, NEN-25 | M |
| LK-BC03 | Bảng kê nộp tiền gửi lưu ký | Tự lọc biên nhận chưa nộp trong kỳ; người nộp; tổng bằng số và chữ; UQ `ChungTuLuuKyId` — mỗi biên nhận chỉ vào 1 bảng kê | P1 | R0.5 | LK-T01, LK-T03 | M |
| NEN-26 | Báo cáo chứng từ huỷ trong kỳ (mới) | Cho lãnh đạo: chứng từ huỷ, lý do, người huỷ, đã in hay chưa | P1 | R0.5 | LK-T06, NEN-22 | S |
| NEN-27 | Đối chiếu một chạm cho kế toán (mới) | Chênh lệch `SoDuLuuKy` vs sổ cái (R0.5); `SoLuongTon` vs `TheKho` (bổ sung ở R1) | P1 | R0.5 / R1 | NEN-19 | M |

### E8 — Go-live & vận hành
**Mục tiêu:** đủ điều kiện chạy thật: số dư đầu kỳ, hạ tầng, triển khai, chạy song song.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| GAP-01 | Số dư đầu kỳ khi go-live (mới) | Nghiệp vụ/chứng từ "số dư chuyển sang" qua engine, có duyệt | P1 | R0.5 | NEN-16, DEC-09 | M |
| GAP-04 | Checklist khởi tạo lần đầu (mới) | Admin đi qua: thông tin đơn vị, người ký, tài khoản, số dư đầu kỳ | P1 | R0.5 | HT-03, HT-04, HT-02, GAP-01 | S |
| GAP-05 | Hạ tầng vận hành (mới) | Cài SQL Express, IP tĩnh, firewall, chứng chỉ `Encrypt=True`; DENY xoá/sửa trực tiếp bảng chứng từ | P1 | R0.5 | NEN-02 | M |
| GAP-06 | Triển khai + cập nhật (mới) | Velopack đầy đủ từ thư mục chia sẻ LAN, kèm kiểm tra phiên bản CSDL | P1 | R0.5 | SP-02, NEN-03 | M |
| GAP-07 | Chạy song song sổ giấy 1 tháng (mới) | Đối chiếu số dư cuối tháng với sổ giấy trước khi bỏ giấy | P1 | R0.5 | Toàn bộ R0.5 | M |

### E9 — Danh mục căn tin
**Mục tiêu:** hàng hoá, nhà cung cấp, bảng giá bất biến cho R1.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| DM-05 | Nhà cung cấp | Mã, tên, địa chỉ cửa hàng bách hoá | P1 | R1 | NEN-02 | S |
| DM-06 | Đơn vị tính | Danh mục dùng chung cho hàng hoá | P1 | R1 | NEN-02 | S |
| DM-07 | Hàng hoá | Mã, tên, ĐVT, loại (sinh hoạt/thực phẩm), ghi chú, ngừng kinh doanh; in Danh mục hàng hoá | P1 | R1 | DM-06, NEN-20 | M |
| DM-08 | Bảng giá bán | Giá hiệu lực từ ngày, lịch sử bất biến; giá mới chỉ hiệu lực từ ngày mai; in Bảng niêm yết giá | P1 | R1 | DM-07, NEN-06 | M |

### E10 — Nhập hàng (NH)
**Mục tiêu:** nhập kho với thẻ kho lũy kế và giá vốn bình quân đúng đến từng đồng.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| NEN-28 | Thẻ kho lũy kế (mới) | `TheKho` lưu lũy kế sau mỗi dòng (NXT chỉ đọc dòng cuối kỳ); `decimal(18,4)`, quy tắc làm tròn rõ ràng; test cộng dồn hàng nghìn lần | P1 | R1 | DM-07, NEN-14, DEC-01 | M |
| NH-01 | Lập phiếu nhập | Nhiều dòng theo hoá đơn/bảng kê của từng NCC; bắt buộc số hoá đơn | P1 | R1 | DM-05, DM-07, HT-05 | M |
| NH-02 | Cập nhật tồn và giá vốn | Khi ghi phiếu: tăng tồn, tính lại đơn giá bình quân gia quyền | P1 | R1 | NH-01, NEN-28, DEC-01 | M |
| NH-03 | In phiếu nhập | Người giao, người nhận, chỉ huy đội, lãnh đạo ký | P1 | R1 | NH-01, NEN-20 | S |
| NEN-29 | Engine tính lại thẻ kho (mới) | Tính lại idempotent từ ngày X; test so với tính lại từ đầu | P1 | R1 | NEN-28 | L |
| NH-04 | Sửa, huỷ phiếu nhập | Bắt buộc lý do; tính lại giá vốn các phiếu xuất sau ngày đó | P1 | R1 | NH-02, NEN-29, HT-06 | M |
| GAP-02 | Tồn kho đầu kỳ khi go-live (mới) | Phiếu nhập khởi tạo có giá vốn, có duyệt | P1 | R1 | NH-02, DEC-09 | S |

### E11 — Bán hàng (BH)
**Mục tiêu:** bán hàng kiểu POS; phiếu bán + thẻ kho + chứng từ chi lưu ký trong 1 transaction.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| NEN-30 | Khoá tồn kho đồng thời (mới) | Trừ tồn `HangHoa` bằng UPDATE có điều kiện + khoá dòng; test 2 máy bán quá tồn → 1 lệnh bị chặn | P1 | R1 | NEN-28 | S |
| BH-04 | Giá và tồn kho | Lấy giá bán hiệu lực; chặn bán quá tồn | P1 | R1 | DM-08, NEN-30 | M |
| BH-02 | Lập phiếu bán | 4 loại người mua: đối tượng, người thân, CBCS, đơn vị đến công tác; màn hình POS: tìm theo mã/tên, Enter thêm dòng, tổng tiền hiện lớn, hỗ trợ máy quét mã vạch USB | P1 | R1 | BH-04, NEN-23, DEC-03, DEC-04 | L |
| BH-05 | Trừ tiền lưu ký | Bán cho đối tượng: trừ tiền trong cùng giao dịch qua `GhiSoLuuKyService`; chặn nếu thiếu số dư | P1 | R1 | BH-02, NEN-16, DEC-03, DEC-06 | M |
| BH-06 | Ghi giá vốn | Mỗi dòng lưu đơn giá vốn bình quân tại thời điểm xuất | P1 | R1 | BH-02, NH-02 | S |
| BH-07 | In phiếu mua hàng | Với đối tượng/người thân in thêm số dư trước, tiền mua, số còn lại; bắt buộc chữ ký đối tượng | P1 | R1 | BH-05, NEN-20 | S |
| BH-08 | Huỷ phiếu bán | Hoàn kho và hoàn tiền lưu ký trong cùng giao dịch | P1 | R1 | BH-05, BH-06, HT-06 | M |
| BH-01 | Đăng ký mua hàng | Quản giáo lập cho đối tượng mình quản lý; kiểm tra số dư lúc đăng ký | P2 | R2 | BH-02, DEC-08 | M |
| BH-03 | Chuyển đăng ký thành phiếu bán | Theo lô hoặc theo buồng, giữ liên kết với phiếu đăng ký | P2 | R2 | BH-01 | M |

### E12 — Báo cáo hàng hoá (HH-BC)
**Mục tiêu:** báo cáo căn tin khớp thẻ kho và sổ cái.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| HH-BC01 | Nhập – xuất – tồn | SL và thành tiền tồn đầu, nhập, xuất, tồn cuối; đơn giá xuất BQGQ; đọc dòng cuối `TheKho` trong kỳ | P1 | R1 | NEN-28, BH-06, NEN-25 | M |
| HH-BC02 | Doanh thu | Theo kỳ và loại người mua: doanh thu, giá vốn, lãi ròng từng dòng | P1 | R1 | BH-06, DEC-04 | M |
| HH-BC03 | Theo dõi mua hàng | Các lần mua của một người mua trong kỳ, kèm số dư nếu là đối tượng | P1 | R1 | BH-05 | S |

(HH-BC04 Xuất file đã đưa lên E4.)

### E13 — Tiện ích & safeguard mở rộng (R2)
**Mục tiêu:** tăng tốc thao tác, tăng giám sát, sau khi lõi đã chạy thật.

| ID | Tên story | Mô tả / quy tắc | Ưu tiên | Release | Phụ thuộc | Size |
|---|---|---|---|---|---|---|
| TI-01 | Màn hình "quầy" (mới) | Nhập mã đối tượng → tổng hợp số dư + thao tác nhanh thu/chi/bán | P2 | R2 | LK-BC04, BH-02 | M |
| TI-02 | Phím tắt (mới) | F2 thu, F3 chi, F4 bán… | P2 | R2 | TI-01 | S |
| TI-03 | Tìm kiếm toàn cục Ctrl+K (mới) | Theo số chứng từ, tên đối tượng, mã hàng | P2 | R2 | SP-03 | M |
| TI-04 | Dashboard chỉ huy (mới) | Đề nghị chờ duyệt, chứng từ nháp chưa ghi sổ, backup gần nhất | P2 | R2 | LK-C03, NEN-12 | M |
| TI-05 | Màn hình đầu ngày + nhắc việc cuối tháng (mới) | Kỳ khoá sổ, số chứng từ, giá hiệu lực hôm nay; nhắc khoá sổ, lập bảng kê nộp tiền | P2 | R2 | HT-06, DM-08 | S |
| TI-06 | Thanh trạng thái (mới) | Người dùng, máy trạm, kỳ hiện tại, kết nối CSDL, phiên bản, backup cuối | P2 | R2 | NEN-12 | S |
| TI-07 | Chế độ tập huấn (mới) | CSDL sandbox + giả lập ngày, không ảnh hưởng dữ liệu thật | P2 | R2 | NEN-06 | M |
| TI-08 | Tuỳ chọn in (mới) | In 2 liên A5 trên A4; tự in biên nhận sau ghi sổ; xem trước khi còn nháp (watermark NHÁP) | P2 | R2 | NEN-20 | S |
| TI-09 | Tự lưu PDF chứng từ đã in (mới) | Vào thư mục chia sẻ theo năm/tháng | P2 | R2 | NEN-22 | S |
| TI-10 | Huỷ và lập lại (mới) | Chứng từ thay thế liên kết với chứng từ bị huỷ | P2 | R2 | NEN-14 | M |
| TI-11 | QR mã chứng từ trên phiếu mua hàng (mới) | Tra cứu nhanh | P3 | R2 | BH-07 | S |
| TI-12 | Hạn mức chi mua hàng theo tháng (mới) | Cấu hình được, mặc định tắt | P2 | R2 | BH-05, DEC-06 | S |
| TI-13 | Kiểm kê kho & phiếu điều chỉnh (mới) | Hàng hỏng, hết hạn; đi qua thẻ kho | P2 | R2 | NEN-28, DEC-07 | M |
| TI-14 | Cảnh báo hàng (mới) | Sắp hết tồn / ngừng kinh doanh; tự in bảng niêm yết khi có giá mới | P2 | R2 | DM-08, NEN-28 | S |
| TI-15 | Nút mặt hàng bán chạy (mới) | Trên màn hình POS | P3 | R2 | BH-02 | S |
| TI-16 | Chuyển loại đối tượng hàng loạt (mới) | Từ danh sách bản án | P2 | R2 | DM-02 | S |
| TI-17 | Giám sát trích tiền (mới) | Cảnh báo tần suất; báo cáo cặp cho–nhận lặp lại | P2 | R2 | LK-C03 | S |
| TI-18 | Báo cáo bất thường căn tin (mới) | Đối tượng mua bất thường; lịch sử thay đổi giá | P2 | R2 | HH-BC03, DM-08 | M |
| TI-19 | Kiểm soát phiếu nhập (mới) | Đính kèm ảnh scan hoá đơn; chỉ huy duyệt | P2 | R2 | NH-01 | M |
| TI-20 | Hash chain `ChungTuLuuKy` (mới) | Phát hiện sửa dữ liệu ngoài app; tích hợp vào đối chiếu | P2 | R2 | NEN-19 | M |
| TI-21 | Xuất gói báo lỗi (mới) | Nén log thành zip cho quản trị, không cần Internet | P2 | R2 | NEN-01 | S |
| TI-22 | Hướng dẫn F1 theo form (mới) | | P3 | R2 | — | M |
| TI-23 | Biểu đồ doanh thu tháng (mới) | | P3 | R2 | HH-BC02 | S |
| TI-24 | Font lớn / zoom màn hình quầy; ảnh chân dung trên picker (mới) | | P3 | R2 | NEN-23 | M |

---

## 3. Decision / Spike (8 câu hỏi mở + 1 gap go-live)

Mỗi DEC cần người chịu trách nhiệm (nghiệp vụ + PO), chốt **trước sprint** của story bị chặn.

| ID | Câu hỏi cần chốt | Giả định hiện tại | Chặn story | Hạn chốt |
|---|---|---|---|---|
| DEC-01 | Đơn giá vốn BQGQ tính sau mỗi lần nhập (di động) hay một lần cuối kỳ? | Di động | NEN-28, NEN-29, NH-02, NH-04, BH-06, HH-BC01, HH-BC02 | Trước R1 |
| DEC-02 | "Chuyển tiền về cho người thân" (A.II.3) có là loại chi riêng không? | Có, loại chi riêng | LK-C01, LK-C06 | Sprint 0 |
| DEC-03 | Người thân mua hàng căn tin: trừ lưu ký của đối tượng hay trả tiền mặt? | Trừ lưu ký (mẫu phiếu in số dư) | BH-02, BH-05, BH-07 | Trước R1 |
| DEC-04 | Bán cho CBCS và đơn vị đến công tác: chỉ tiền mặt hay có ghi nợ? | Chỉ tiền mặt | BH-02, HH-BC02, HH-BC03 | Trước R1 |
| DEC-05 | "Phiếu gửi quà" có kèm tiền không, khác "phiếu gửi lưu ký" thế nào khi ghi sổ? | Chưa có | LK-T01 (nghiệp vụ 13) | Sprint 0 |
| DEC-06 | Có hạn mức chi mua hàng căn tin theo tháng cho mỗi đối tượng không? | Không (thiết kế sẵn cờ, mặc định tắt) | BH-05, TI-12 | Trước R1 |
| DEC-07 | Có cần kiểm kê, điều chỉnh kho cho hàng hỏng, hết hạn không? | Có, để R2 | TI-13, HH-BC01 (cột điều chỉnh) | Trước R2 |
| DEC-08 | Quản giáo nhập phiếu đăng ký trên phần mềm hay nộp giấy cho căn tin? | Chưa có | BH-01, BH-03, HT-02 (quyền vai trò quản giáo) | Trước R2 |
| DEC-09 (mới) | Số dư đầu kỳ và tồn đầu kỳ khi go-live nhập bằng chứng từ nào, ai duyệt? | Chứng từ "số dư chuyển sang" / phiếu nhập khởi tạo, lãnh đạo duyệt | GAP-01, GAP-02, GAP-04 | Sprint 0 |

---

## 4. Definition of Done

- [ ] Unit test cho quy tắc Domain liên quan (số dư, trạng thái, làm tròn).
- [ ] Integration test transaction trên LocalDB; đối chiếu sổ cái (NEN-19) pass sau test.
- [ ] Mẫu in (nếu có) đã duyệt đối chiếu với mẫu giấy; golden-file test cập nhật.
- [ ] Nhật ký thao tác ghi đúng (ai, lúc nào, trước/sau), kể cả in.
- [ ] Quyền được kiểm tra ở tầng service, không chỉ ẩn nút trên form.
- [ ] Service không phụ thuộc WinForms; Presenter có test qua interface View.
- [ ] Story ghi rõ: mã chức năng, vai trò, quy tắc, tiêu chí chấp nhận, mẫu in, bảng CSDL liên quan.

---

## 5. Kịch bản demo end-to-end

| Release | Kịch bản |
|---|---|
| Sprint 0 | Đăng nhập → tạo đối tượng Nguyễn Văn A → lập biên nhận 500.000đ → ghi sổ → xem trước và xuất PDF có header đơn vị, số tiền bằng chữ tiếng Việt đúng. |
| R0.5 | Kế toán nhập số dư đầu kỳ → người thân gửi 500.000đ chuyển khoản cho A (in biên nhận) → A đề nghị cho B 100.000đ, lãnh đạo duyệt, sinh phiếu chi + biên nhận cùng số biên bản → 2 máy cùng chi 450.000đ của A, chỉ 1 máy thành công → lập bảng kê nộp tiền → in bảng kê cá nhân A và sổ theo dõi đơn vị → đối chiếu một chạm: 0 chênh lệch → khoá sổ tháng, thử huỷ biên nhận bị chặn. |
| R1 | Nhập tồn đầu kỳ + phiếu nhập 2 mặt hàng từ NCC → A (số dư 400.000đ) mua 3 món trên màn hình POS, in phiếu mua hàng có số dư trước/sau → phiếu chi tự sinh (LK-C05) → huỷ 1 phiếu bán: hoàn kho, hoàn tiền → báo cáo NXT, doanh thu, bảng kê cá nhân A khớp; đối chiếu số dư + tồn kho: 0 chênh lệch. |
| R2 | Quản giáo lập đăng ký mua hàng cho buồng 3 → căn tin chuyển hàng loạt thành phiếu bán → người thân gửi tiền lần 2 trong tháng hiện cảnh báo → chỉ huy xem dashboard (đề nghị chờ duyệt, backup gần nhất) → kiểm kê điều chỉnh 1 mặt hàng hỏng, NXT cập nhật. |
