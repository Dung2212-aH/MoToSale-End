# Báo cáo kiểm thử toàn hệ thống MoToSale V2 (lần 2 — toàn diện)

Ngày test: **04/06/2026** · Thay thế/bổ sung cho report 03/06 (đã lạc hậu sau nhiều thay đổi).
Môi trường: Gateway `http://localhost:5100` · Auth `5101` · API `5102` · DB `MoToSaleV2` (SQL Server LocalDB, mô hình 1 kho).

## 1. Mục tiêu & điểm khác plan cũ
Lần này kiểm thử **sâu hơn** plan 03/06: không chỉ "mở được trang" mà chạy **E2E mức API qua Gateway**, **assert từng giá trị** (trạng thái đơn, tiền, tồn kho, công nợ), phủ **toàn bộ tính năng mới** thêm sau 03/06, kiểm **ràng buộc nghiệp vụ** (chặn xóa/sửa, giới hạn chuyển trạng thái) và **phân quyền Admin/Staff**.

So với plan cũ, lần này bổ sung kiểm thử cho:
- POS bán đứt / **đặt cọc** / bán chịu, **khách quen** (gắn `customerId`), **voucher áp mã**.
- **Giao hàng & xuất kho** (chốt đơn cọc), tự **Hoàn tất** khi thu đủ.
- **Sửa đơn** (thông tin + dòng hàng khi Chờ thanh toán) và **chặn sửa** sau xác nhận.
- **Sửa bảo hành / sửa chữa** khi mới tiếp nhận và **chặn sửa** sau đó.
- **Chặn xóa** voucher/SKU/user khi đã phát sinh giao dịch.
- Vòng đời mua hàng đầy đủ: NCC → đơn mua → duyệt → **nhận hàng (tồn +)** → thanh toán NCC (chi quỹ).
- Báo cáo **lãi gộp/COGS**; phân quyền Staff bị chặn endpoint Admin.

## 2. Phương pháp
- Script PowerShell gọi API thật qua Gateway bằng JWT (Admin & Staff), mỗi bước có hàm `Assert` → đánh dấu `[PASS]/[FAIL]` kèm lý do; đối chiếu lại DB qua chính API (tồn kho, đơn, công nợ, refund, audit) sau mỗi mutation.
- Chia 4 phần để cô lập lỗi. Dữ liệu test mang tiền tố `SMOKE/SMK/Smoke`.

## 3. Kết quả tổng quan

| Nhóm kiểm thử | Kết quả |
|---|---:|
| Phần 1 — Xác thực + Danh mục/Sản phẩm | **15/15 PASS** |
| Phần 2 — Bán hàng/POS/Đơn/Voucher | **12/12 PASS** |
| Phần 3A — Kho/Đổi trả/Bảo hành/Sửa chữa/CSKH/Chấm công | **16/16 PASS** |
| Phần 3B — Cung ứng/Tài chính/Báo cáo/Nhật ký/Phân quyền | **16/16 PASS** |
| **Tổng E2E** | **59/59 PASS** |
| FE build (`npm run build`) | **PASS** |
| BE build (`dotnet build`) | **PASS — 0 warning, 0 error** |
| BE unit/integration test | **PASS — 20/20** |

> 1 **lỗi thật** được phát hiện và **đã sửa** trong quá trình test (xem mục 5).

## 4. Chi tiết theo domain (đã PASS)

### 4.1 Xác thực & phân quyền
- Đăng nhập Admin OK; đăng nhập sai mật khẩu bị từ chối.
- Staff đăng nhập OK; **bị chặn (403)** khi tạo Nhà cung cấp (Admin-only); **dùng được POS** (Staff được phép).

### 4.2 Danh mục & sản phẩm
- CRUD: Danh mục (tạo/sửa, **chặn xóa khi còn sản phẩm**), Hãng xe, Dòng xe, Hãng SX.
- Sản phẩm: tạo (xe/phụ tùng), sửa, **xóa mềm (Inactive)**; biến thể (SKU), ảnh, tương thích, sản phẩm bán kèm.

### 4.3 Bán hàng / POS / Đơn
- POS **bán đứt** thu đủ → **Hoàn tất/Đã thanh toán/Đã giao**, **trừ kho −1**.
- POS **khách quen** → đơn gắn đúng `customerId`.
- POS **áp voucher 10%** → giảm **20.000**, tổng **180.000**.
- POS **đặt cọc** → **Đã xác nhận/Đã đặt cọc**, còn nợ **300.000**; **tất toán** → **Đã thanh toán**; **Giao hàng & xuất kho** → **Hoàn tất** + **trừ kho −2**.
- Đơn online: cart → checkout → **Chờ thanh toán**; **sửa đơn** (người nhận + dòng hàng) → **tính lại tiền = 300.000**; **chặn sửa dòng** trên đơn đã xác nhận; **hủy đơn** → Đã hủy (nhả giữ chỗ).

### 4.4 Voucher
- Tạo, **áp dụng** (usedCount tăng), **chặn xóa khi đã dùng**, sửa giá trị 10→15%.

### 4.5 Kho
- Điều chỉnh tồn **+5**; đặt ngưỡng cảnh báo; đồng bộ sổ cái; **chứng từ kho** tạo + duyệt; xem lịch sử movements.

### 4.6 Đổi trả & hoàn tiền
- Tạo phiếu trả (hàng bán lại được) → **duyệt** → **hoàn tồn +1** + **sinh phiếu hoàn tiền 150.000** + **ghi chi quỹ**.

### 4.7 Bảo hành
- Tạo; **sửa được khi mới tiếp nhận** (đổi SP/khách/số tháng); chuyển trạng thái; **chặn sửa sau khi xử lý**.

### 4.8 Sửa chữa / CSKH / Chấm công
- Sửa chữa: tạo (kèm phụ tùng); **sửa khi mới tiếp nhận**; luồng trạng thái Nhận → Kiểm tra → Báo giá → **Sửa (xuất kho phụ tùng)**; **chặn sửa sau đó**.
- CSKH: tạo + hoàn thành. Chấm công: check-in + check-out (nhân viên hợp lệ); hệ thống **từ chối đúng** nếu không phải Staff.

### 4.9 Cung ứng & Tài chính
- NCC tạo/sửa; Đơn mua tạo → duyệt → **nhận hàng (tồn +10)** → **thanh toán NCC (chi quỹ)**.
- Quỹ: lập phiếu thu + **đảo phiếu**; danh sách **công nợ** khách.

### 4.10 Báo cáo & Nhật ký
- Dashboard có **`cogs` + `grossProfit`**; báo cáo theo kỳ đủ mảng (doanh thu, top SP, thu chi, công nợ).
- Nhật ký kiểm toán có bản ghi sau các mutation.

## 5. Lỗi phát hiện trong khi test (đã sửa)

**BUG-01 (Cao) — Trùng mã đơn khi tạo nhiều đơn trong cùng 1 giây.**
- Hiện tượng: tạo 2 đơn POS liên tiếp → `SqlException 2601: duplicate key 'IX_Orders_Code'` (mã `POS20260604011030`).
- Nguyên nhân: mã đơn dùng timestamp **giây** `POS{yyyyMMddHHmmss}` / `DH{yyyyMMddHHmmss}`.
- Sửa: thêm **mili-giây** `…HHmmssfff` (đồng bộ với mã phiếu thu/chi/hoàn vốn đã dùng `fff`).
- Đã re-test: tạo liên tiếp nhiều đơn POS/đơn online → không còn trùng.

## 6. Ghi chú test-data (không phải lỗi sản phẩm)
- Lần chạy đầu Part 2/3A có vài `[FAIL]` do **script test**, không phải hệ thống: API tạo voucher chỉ trả `{id}` (script đọc nhầm `code`); SKU bị rút cạn tồn giữa các bước; chấm công thử với `staffUserId=1` (Admin) bị từ chối **đúng**. Đã chỉnh script và chạy lại **PASS**.

## 7. Phạm vi chưa test bằng API (kiểm thủ công/qua build)
- **Hóa đơn GTGT (VAT)** và **In phiếu**: là chức năng **in phía trình duyệt** (`window.print`), không có endpoint API → kiểm trực quan; logic số tiền/đọc chữ nằm ở `utils/vatInvoice.js`.
- **Render giao diện** các trang: đảm bảo qua **FE build PASS** (mọi component biên dịch + đóng gói) và kiểm trực quan ở các phiên trước (timeline, dropdown khách, menu 5 nhóm).

## 8. Kết luận
- **59/59 kiểm thử nghiệp vụ E2E PASS**, build FE/BE sạch, **20/20** test BE.
- Phát hiện & sửa **1 lỗi thật** (trùng mã đơn) — minh chứng độ phủ của lần test này.
- Toàn bộ **luồng nghiệp vụ lõi + tính năng mới + ràng buộc + phân quyền** hoạt động đúng và nhất quán dữ liệu (tiền/tồn/công nợ/kiểm toán).
- **Sẵn sàng demo/nộp BTL.** Lưu ý vận hành thật: dọn dữ liệu test (`SMOKE/SMK/BTL-E2E`), cấu hình HTTPS + SQL Server thật + secrets ra biến môi trường.
