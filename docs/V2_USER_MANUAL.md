# Hướng dẫn sử dụng — MoToSale v2 (Khu quản trị)

Phiên bản: 1.0 · Ngày: 04/06/2026 · Đối tượng: Quản lý cửa hàng (Admin) & Nhân viên (Staff).
Tài liệu tra cứu theo trang: `V2_ADMIN_PAGES_GUIDE.md`. Tài liệu này hướng dẫn **theo tác vụ** (làm thế nào để…).

---

## 1. Bắt đầu

### 1.1 Đăng nhập
1. Mở trình duyệt → địa chỉ hệ thống (dev: `http://localhost:5176`).
2. Nhập **email + mật khẩu** → **Đăng nhập**.
   - Admin: `admin@motosale.local` / `Admin@123`
   - Nhân viên: `staff@motosale.local` / `Staff@123`
3. Sau khi vào, **menu trái** chia **5 nhóm**: Bán hàng · Sản phẩm & Kho · Dịch vụ & Hậu mãi · Tài chính & Báo cáo · Hệ thống.

### 1.2 Đăng xuất / đổi mật khẩu
- Góc trên phải → menu tài khoản → **Đăng xuất** / **Đổi mật khẩu**.

### 1.3 Ai làm được gì
| Nhóm tác vụ | Admin | Nhân viên |
|---|:--:|:--:|
| Bán hàng, POS, đơn, khách, đổi trả, bảo hành, sửa chữa, CSKH, chấm công | ✔ | ✔ |
| Sản phẩm/danh mục, kho, chứng từ kho, cung ứng/nhà cung cấp | ✔ | ✖ |
| Tài chính (quỹ, công nợ, thanh toán NCC), báo cáo | ✔ | ✖ |
| Tài khoản & vai trò, phân ca, cấu hình, nhật ký, import | ✔ | ✖ |

---

## 2. Bán hàng tại quầy (POS) — tác vụ thường dùng nhất

**Đường đi:** Bán hàng → **Bán tại quầy (POS)**.

### 2.1 Bán đứt (thu đủ tiền)
1. **Tìm sản phẩm**: gõ mã SKU / tên / quét barcode → Enter hoặc bấm **Thêm**.
2. Chỉnh **số lượng**; sửa **đơn giá** nếu được phép. Thêm nhiều sản phẩm tùy ý.
3. **Khách hàng**:
   - Để trống = **Khách lẻ** (hệ thống tự gán).
   - Hoặc gõ **số điện thoại** để tra **khách quen** → chọn từ gợi ý (đơn sẽ gắn đúng khách).
4. (Tùy) nhập **mã voucher** để giảm giá.
5. Chọn hình thức **Bán đứt** → chọn **phương thức thanh toán** (Tiền mặt/Chuyển khoản).
6. Bấm **Tạo đơn**.
   → Đơn chuyển **Hoàn tất / Đã thanh toán / Đã giao**, **tồn kho trừ ngay**, **tự ghi thu quỹ**.
7. (Tùy) bấm **In hóa đơn GTGT** để in cho khách.

### 2.2 Đặt cọc (khách trả trước một phần)
1. Làm bước 1–4 như trên.
2. Chọn **Đặt cọc** → nhập **số tiền cọc** (lớn hơn 0 và nhỏ hơn tổng tiền).
3. **Tạo đơn** → đơn ở trạng thái **Đã xác nhận / Đã đặt cọc**, **giữ chỗ tồn**, hiển thị **còn nợ** = tổng − cọc.
4. Khi khách trả nốt & nhận hàng → xem mục **3.3 Tất toán & giao đơn cọc**.

### 2.3 Lỗi hay gặp ở POS
- *"Số lượng tồn khả dụng không đủ"*: SKU hết/không đủ → giảm SL hoặc nhập thêm kho.
- Giỏ trống / SL ≤ 0 / cọc ≤ 0 / cọc ≥ tổng tiền → hệ thống chặn, sửa lại.
- Voucher hết hạn/không đủ điều kiện → bỏ hoặc đổi mã.

---

## 3. Quản lý đơn hàng

**Đường đi:** Bán hàng → **Đơn hàng** → bấm 1 đơn để xem **Chi tiết đơn**.

### 3.1 Đọc chi tiết đơn
- Khối tóm tắt: tổng tiền, giảm giá, **đã thanh toán**, **còn lại**.
- **Dòng thời gian** ghi lại mọi lần đổi trạng thái.
- Trạng thái: **đơn** (Chờ thanh toán/Xác nhận/…/Hoàn tất/Hủy), **thanh toán** (Chưa/Cọc/Một phần/Đã thanh toán), **giao hàng**.

### 3.2 Ghi nhận thanh toán
1. Bấm **Ghi nhận thanh toán**.
2. Hệ thống **chỉ cho chọn loại phù hợp trạng thái** (đơn còn nợ → "Thu phần còn lại"; không cho thu vượt số nợ).
3. Nhập số tiền + phương thức → **Lưu**.
   → Cập nhật trạng thái thanh toán + công nợ, **tự ghi thu quỹ**. Thu đủ + đã giao → đơn **Hoàn tất**.

### 3.3 Tất toán & giao đơn cọc
1. Mở đơn cọc → **Ghi nhận thanh toán → Thu phần còn lại** → đơn **Đã thanh toán**.
2. Bấm **Giao hàng & xuất kho** → **trừ tồn thật**, nhả giữ chỗ, đơn **Hoàn tất**.

### 3.4 Sửa đơn
- **Thông tin người nhận/giao + ghi chú**: sửa được hầu hết thời điểm.
- **Sản phẩm trong đơn**: chỉ sửa khi đơn còn **Chờ thanh toán** (hệ thống tính lại tiền & giữ chỗ). Sau khi xác nhận → không cho sửa dòng hàng.

### 3.5 Hủy đơn
- Hủy được khi **chưa giao**. Đơn cọc bị hủy: nhả giữ chỗ; theo chính sách mặc định **khách mất cọc**.

---

## 4. Khách hàng & Voucher

### 4.1 Khách hàng (Bán hàng → Khách hàng)
- Thêm/sửa khách (tên, SĐT, email, địa chỉ, **ghi chú chăm sóc**).
- Bán POS bằng SĐT mới → khách tự xuất hiện ở danh sách.
- Tìm theo tên/SĐT; xem lịch sử mua.

### 4.2 Voucher (Bán hàng → Voucher)
- Tạo mã: chọn **giảm % hay giảm tiền**, đơn tối thiểu, hạn mức lượt dùng, thời hạn, phạm vi áp dụng.
- **Không xóa được** voucher đã có lượt dùng (chỉ nên ngừng/hết hạn). Sửa giá trị được.

---

## 5. Sản phẩm & Kho (Admin)

### 5.1 Thêm sản phẩm mới
**Sản phẩm & Kho → Sản phẩm → Thêm.**
1. Chọn loại: **Xe máy** (gắn Hãng xe/Dòng xe) hoặc **Phụ tùng** (gắn Hãng sản xuất).
2. Nhập mã, tên, danh mục, mô tả, giá.
3. Thêm **biến thể (SKU)**: mã SKU, màu/phiên bản, giá, barcode.
4. Tải **ảnh**; (phụ tùng) khai **tương thích xe** & **sản phẩm bán kèm**.
5. Lưu. Xóa sản phẩm là **xóa mềm** (chuyển Ngừng bán), không mất lịch sử.

### 5.2 Xem & điều chỉnh tồn kho
**Sản phẩm & Kho → Tồn kho.**
- Bảng: **Tồn thực / Đang giữ / Khả dụng / Ngưỡng**; lọc theo trạng thái tồn; xuất Excel.
- **Điều chỉnh tồn** hoặc lập **Chứng từ kho** (nhập/xuất/điều chỉnh) → **Duyệt** mới tác động tồn.

### 5.3 Mua hàng nhập kho
**Sản phẩm & Kho → Cung ứng/Nhà cung cấp.**
1. Tạo/chọn **Nhà cung cấp**.
2. Lập **Đơn mua** (chọn SKU, số lượng, đơn giá) → **Duyệt**.
3. **Nhận hàng** → **tồn tăng** theo số nhận.
4. **Thanh toán NCC** → **tự ghi chi quỹ**, giảm công nợ NCC.

---

## 6. Dịch vụ & Hậu mãi

### 6.1 Đổi trả & hoàn tiền
**Dịch vụ & Hậu mãi → Đổi trả & hoàn tiền.**
1. Tạo phiếu trả từ **đơn đã giao**; chọn sản phẩm, số lượng, **tình trạng** (Bán lại được / Hư hỏng / Bảo hành).
2. (Khi còn nháp) sửa được; **Duyệt** để xử lý.
   → Hàng "bán lại được" **nhập về kho**; hệ thống **sinh phiếu hoàn tiền** + **ghi chi quỹ**; công nợ điều chỉnh.
3. Có thể **Từ chối**. Phiếu đã duyệt/từ chối **không sửa được**.

### 6.2 Bảo hành
**Dịch vụ & Hậu mãi → Bảo hành.**
1. Tạo phiếu: chọn khách/sản phẩm, **số khung/số máy**, mô tả lỗi, chi phí dự kiến, số tháng BH.
2. **Sửa thông tin** chỉ khi phiếu **mới tiếp nhận**.
3. Cập nhật trạng thái theo tiến trình; xem **lịch sử xử lý**.

### 6.3 Sửa chữa
**Dịch vụ & Hậu mãi → Sửa chữa.**
1. Tạo phiếu (mô tả xe, lỗi, **phụ tùng** dự kiến).
2. Luồng: **Tiếp nhận → Kiểm tra → Báo giá → Sửa** (lúc sửa hệ thống **xuất kho phụ tùng**) → Bàn giao.
3. Sửa thông tin chỉ khi mới tiếp nhận.

### 6.4 Chăm sóc khách hàng (CSKH)
- Tạo lịch/tương tác (gọi, hẹn bảo dưỡng…), gán nhân viên, cập nhật & hoàn thành; xem lịch sử theo khách.

---

## 7. Tài chính & Báo cáo (Admin)

### 7.1 Sổ quỹ & công nợ
**Tài chính & Báo cáo → Sổ quỹ / Công nợ.**
- Xem phiếu thu/chi (phần lớn **tự sinh** khi bán/hoàn/mua). Lập phiếu thủ công khi cần; **hủy phiếu = đảo phiếu** (không xóa).
- **Công nợ**: danh sách đơn còn phải thu.

### 7.2 Báo cáo
**Tài chính & Báo cáo → Báo cáo.**
- Chọn **khoảng thời gian** (hôm nay / 7 ngày / tháng…).
- Xem: doanh thu, **lãi gộp & giá vốn (COGS)**, top sản phẩm, trạng thái đơn, thu chi, công nợ, cảnh báo tồn.
- **Xuất Excel (.xlsx)**. Lưu ý: đơn **đã hủy không tính doanh thu**; hoàn tiền điều chỉnh tiền thực nhận.

---

## 8. Hệ thống (Admin)

- **Tài khoản & vai trò**: tạo/sửa Staff, khóa/mở; không tự khóa-xóa Admin cuối cùng.
- **Phân ca / Chấm công**: Admin xếp ca; nhân viên **check-in/check-out**.
- **Cấu hình**: tên cửa hàng, MST, thuế VAT, ngưỡng tồn…
- **Nhật ký kiểm toán**: tra mọi thao tác (ai, lúc nào, đối tượng gì).
- **Import dữ liệu**: nhập sản phẩm hàng loạt từ Excel.

---

## 9. Mẹo & lưu ý nhanh
- **Khách lẻ vs khách quen**: bỏ trống = khách lẻ; gõ SĐT để gắn khách quen (tích điểm/lịch sử).
- **Tồn "Khả dụng"** mới là số bán được (đã trừ phần giữ chỗ cho đơn cọc).
- Thao tác **không tác động tồn/tiền cho tới khi Duyệt/Giao** (chứng từ kho, đơn mua, đổi trả).
- Nút hành động **chỉ hiện khi hợp lệ** — nếu không thấy nút, kiểm tra trạng thái hiện tại của phiếu/đơn.
- Mọi thay đổi đều **ghi nhật ký** — thao tác cẩn thận, đúng người đúng việc.
