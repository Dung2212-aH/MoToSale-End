# Admin New Features Test Plan

## Phạm vi

Test các phần mới vừa triển khai cho Frontend Admin và backend liên quan:

- Phân quyền Admin/Staff.
- Nhật ký hệ thống.
- Phiếu kho.
- In chứng từ đơn hàng/phiếu kho/bảo hành.
- Khách hàng và ghi chú chăm sóc.
- Bảo hành.
- Showroom/kho.
- Cấu hình vận hành.
- Dashboard vận hành.

## Rule bắt buộc

- Không chỉ đọc code, phải test bằng UI thật.
- Mỗi trang phải reload, chuyển trang khác rồi quay lại, sau đó kiểm tra lại dữ liệu.
- Mọi nút hiển thị trên trang/modal phải được bấm ít nhất một lần.
- Mọi field nhập liệu phải test dữ liệu hợp lệ, thiếu bắt buộc, sai định dạng, dữ liệu dài.
- Mỗi bảng phải chụp màn hình và đối chiếu header, cột, giá trị, căn lề, badge, nút thao tác.
- Với thao tác ghi dữ liệu, phải kiểm tra lại API trả về, UI sau reload và DB/audit log nếu có.
- Không kết luận pass nếu chỉ build pass.
- Nếu gặp lỗi, ghi rõ: trang, bước test, dữ liệu nhập, expected, actual, ảnh chụp, log console/network nếu có.

## Chuẩn bị

- Chạy BE: AuthService, CatalogService, OrderService, ApiGateway.
- Chạy FE Admin.
- Đăng nhập bằng tài khoản Admin.
- Tạo/đăng nhập thêm tài khoản Staff để kiểm tra phân quyền.
- Mở DevTools Network/Console hoặc dùng browser automation để bắt lỗi.
- Backup DB hoặc dùng dữ liệu test riêng.

## 1. Phân quyền Admin/Staff

### Admin

- Vào các trang có thao tác nguy hiểm:
  - Sản phẩm.
  - Danh mục.
  - Hãng xe & Dòng xe.
  - Voucher.
  - Bài viết.
  - FAQ.
  - Đánh giá.
  - Người dùng.
  - Nhật ký hệ thống.
  - Cấu hình vận hành.
- Kiểm tra Admin thấy nút xóa/sửa/cấu hình tương ứng.
- Gọi thao tác xóa/hủy/duyệt trên UI, xác nhận backend trả đúng.
- Kiểm tra audit log phát sinh.

### Staff

- Đăng nhập Staff.
- Kiểm tra Staff không thấy:
  - Trang Người dùng nếu route yêu cầu Admin.
  - Nút hard delete.
  - Nút chỉnh cấu hình hệ thống.
  - Trang Nhật ký hệ thống nếu chỉ Admin.
- Thử gọi trực tiếp URL/API cấm bằng UI hoặc request:
  - Expected: `403`.
- Kiểm tra Staff vẫn thao tác được:
  - Cập nhật đơn hàng.
  - Xem/tạo phiếu kho.
  - Xem tồn kho.
  - Xem khách hàng.
  - Tạo/cập nhật bảo hành.

## 2. Nhật ký hệ thống

### API/dữ liệu

- Tạo/sửa một sản phẩm hoặc danh mục.
- Cập nhật trạng thái đơn hàng.
- Tạo phiếu kho.
- Cập nhật ghi chú khách hàng.
- Cập nhật bảo hành.
- Mở API `/api/audit-logs`.
- Đối chiếu bản ghi có:
  - Mã log.
  - Loại đối tượng.
  - Mã đối tượng.
  - Hành động.
  - Giá trị trước/sau.
  - Người thực hiện.
  - Thời gian.
  - Ghi chú.

### UI

- Vào `Nhật ký hệ thống`.
- Test filter:
  - Đối tượng.
  - Hành động.
  - Mã người thực hiện.
  - Từ khóa.
  - Từ ngày/đến ngày.
- Bấm `Lọc nhật ký`.
- Bấm `Đặt lại`.
- Kiểm tra phân trang nếu nhiều log.
- Chụp bảng, đối chiếu giá trị từng cột với API/DB.

## 3. Phiếu kho

### Danh sách

- Vào `Phiếu kho`.
- Kiểm tra bảng:
  - Mã phiếu.
  - Loại phiếu.
  - Số dòng.
  - Tổng số lượng.
  - Trạng thái.
  - Ngày tạo.
  - Ngày duyệt.
  - Ghi chú.
  - Thao tác.
- Test filter loại phiếu.
- Test filter trạng thái.
- Chụp màn hình bảng và đối chiếu API `/api/inventory/documents`.

### Tạo phiếu nháp

- Bấm `Tạo phiếu kho`.
- Test các loại:
  - Phiếu nhập kho.
  - Phiếu xuất kho.
  - Phiếu điều chỉnh tồn.
- Test field:
  - Không chọn sản phẩm.
  - Số lượng rỗng/0/âm/chữ.
  - Ghi chú dài.
  - Nhiều dòng hàng.
  - Xóa dòng hàng.
- Lưu phiếu nháp.
- Reload trang và kiểm tra phiếu vẫn còn.
- Kiểm tra DB bảng `TONKHO_PHIEU`, `TONKHO_PHIEU_CHITIET`.
- Kiểm tra audit log `InventoryDocument/Create`.

### Chi tiết/duyệt/hủy

- Bấm xem chi tiết phiếu.
- Kiểm tra dòng hàng:
  - Mã SP.
  - SKU.
  - Sản phẩm.
  - Tồn trước.
  - Thay đổi.
  - Tồn sau.
  - Ghi chú.
- Bấm `Duyệt phiếu`.
  - Expected: tồn kho cập nhật.
  - Trạng thái thành `Đã duyệt`.
  - Không còn được hủy như phiếu nháp.
  - Có log điều chỉnh tồn.
  - Có audit log approve.
- Tạo phiếu nháp khác, bấm `Hủy phiếu`.
  - Expected: trạng thái `Đã hủy`.
  - Tồn kho không đổi.
  - Có audit log cancel.
- Test xuất Excel.
- Test `In phiếu`.

## 4. In chứng từ

### Đơn hàng

- Vào chi tiết đơn hàng.
- Bấm `In phiếu đơn hàng`.
- Kiểm tra cửa sổ in có:
  - Tên cửa hàng.
  - Mã đơn.
  - Ngày in.
  - Thông tin khách hàng.
  - Trạng thái đơn/thanh toán/vận chuyển.
  - Danh sách sản phẩm.
  - Tổng tiền.
  - Khu vực ký nhận.

### Phiếu kho

- Vào chi tiết phiếu kho.
- Bấm `In phiếu`.
- Kiểm tra:
  - Mã phiếu.
  - Loại phiếu.
  - Trạng thái.
  - Người tạo/ngày tạo.
  - Dòng hàng.
  - Tồn trước/thay đổi/tồn sau.
  - Khu vực ký nhận.

### Bảo hành

- Vào chi tiết bảo hành.
- Bấm `In phiếu`.
- Kiểm tra:
  - Mã phiếu bảo hành.
  - Khách hàng/SĐT.
  - Sản phẩm/SKU.
  - Số khung/số máy.
  - Ngày mua/hết hạn.
  - Lỗi khách báo.
  - Ghi chú xử lý.
  - Khu vực ký nhận.

## 5. Khách hàng

### Danh sách

- Vào `Khách hàng`.
- Kiểm tra bảng:
  - Khách hàng.
  - Liên hệ.
  - Tổng đơn.
  - Tổng chi tiêu.
  - Đơn hủy.
  - Đơn gần nhất.
  - Ghi chú chăm sóc.
  - Thao tác.
- Đối chiếu tổng đơn/tổng chi tiêu với danh sách đơn hàng theo SĐT/email.
- Test tìm kiếm theo:
  - Tên.
  - SĐT.
  - Email.
- Test filter trạng thái.
- Chụp bảng và đối chiếu API `/api/users/customers`.

### Ghi chú chăm sóc

- Bấm nút ghi chú.
- Nhập ghi chú hợp lệ.
- Nhập ghi chú dài.
- Lưu.
- Reload trang.
- Kiểm tra ghi chú vẫn còn.
- Kiểm tra DB `KHACHHANG_GHICHU_CHAMSOC`.
- Kiểm tra audit log `Customer/UpdateCareNote`.
- Test Staff có thể ghi chú nếu nghiệp vụ cho phép.

## 6. Bảo hành

### Tạo phiếu

- Vào `Bảo hành`.
- Bấm `Tạo phiếu bảo hành`.
- Test bắt buộc:
  - Khách hàng.
  - SĐT.
  - Sản phẩm.
  - Lỗi khách báo.
- Test optional:
  - Mã đơn hàng.
  - Mã khách hàng.
  - Mã sản phẩm.
  - Mã biến thể.
  - SKU.
  - Số khung.
  - Số máy.
  - Ngày mua.
  - Hết hạn bảo hành.
  - Chi phí dự kiến.
  - Ghi chú.
- Lưu phiếu.
- Reload trang.
- Kiểm tra phiếu vẫn tồn tại.
- Kiểm tra DB `BAOHANH_PHIEU`, `BAOHANH_LICHSU`.
- Kiểm tra audit log `Warranty/Create`.

### Cập nhật trạng thái

- Mở chi tiết phiếu.
- Chuyển lần lượt:
  - Tiếp nhận.
  - Đang xử lý.
  - Chờ linh kiện.
  - Hoàn tất.
  - Từ chối.
- Nhập ghi chú xử lý.
- Nhập chi phí thực tế.
- Kiểm tra lịch sử xử lý cập nhật theo từng lần.
- Reload và kiểm tra lịch sử vẫn đúng.
- Kiểm tra audit log `Warranty/UpdateStatus`.

## 7. Showroom/kho

- Vào `Cấu hình vận hành`.
- Kiểm tra danh sách kho/showroom.
- Admin tạo mới:
  - Cửa hàng kiêm kho.
  - Showroom.
  - Kho.
- Test thiếu tên kho.
- Test địa chỉ dài.
- Test hotline sai/dài.
- Sửa kho đã có.
- Reload và kiểm tra dữ liệu còn.
- Kiểm tra DB `CUAHANG_KHO`.
- Staff vào trang:
  - Expected: chỉ xem, không chỉnh.
- Kiểm tra audit log `Warehouse/Create`, `Warehouse/Update`.

## 8. Cấu hình vận hành

- Kiểm tra các cấu hình:
  - Tên cửa hàng.
  - Hotline.
  - Địa chỉ.
  - Ngưỡng tồn thấp mặc định.
  - Chính sách đặt cọc.
  - Chính sách hủy đơn.
  - Chính sách bảo hành.
  - Phí vận chuyển mặc định.
- Admin sửa từng field.
- Test dữ liệu dài ở chính sách.
- Lưu.
- Reload.
- Kiểm tra DB `HETHONG_CAUHINH`.
- Kiểm tra audit log `SystemSettings/Update`.
- Staff vào trang:
  - Expected: chỉ xem, không lưu được.

## 9. Dashboard vận hành

- Vào `Tổng quan`.
- Kiểm tra các thẻ:
  - Tổng sản phẩm.
  - Tổng đơn hàng.
  - Người dùng nếu Admin.
  - Doanh thu tháng.
  - Đơn cần xử lý.
  - Chưa thanh toán.
  - Đang giao/chuẩn bị.
  - Hết hàng.
  - Sắp hết hàng.
  - Liên hệ mới.
  - Voucher sắp hết hạn.
  - Bảo hành đang xử lý.
- Đối chiếu từng số với API:
  - `/api/orders`.
  - `/api/inventory`.
  - `/api/content/contacts`.
  - `/api/vouchers`.
  - `/api/warranties`.
- Bấm từng thẻ và kiểm tra điều hướng đúng.
- Reload dashboard.
- Chụp màn hình kiểm tra layout, icon, số liệu, text không vỡ.

## 10. Regression layout

- Test desktop 1920x1080.
- Test laptop 1366x768.
- Test tablet width khoảng 768.
- Test mobile width khoảng 390.
- Kiểm tra:
  - Sidebar mở/đóng.
  - Sidebar hover không che sai nội dung.
  - Footer không phình ở trang ngắn.
  - Bảng không lệch cột.
  - Modal không tràn màn hình.
  - Nút không mất chữ.
  - Text tiếng Việt không lỗi font.

## 11. Build và log

- Chạy:
  - `dotnet build Backend/AuthService/AuthService.csproj`
  - `dotnet build Backend/CatalogService/CatalogService.csproj`
  - `dotnet build Backend/OrderService/OrderService.csproj`
  - `npm run build` trong `FrontendAdmin`
- Kiểm tra console browser không có lỗi runtime.
- Kiểm tra network không có API trả `500`.
- Kiểm tra các warning build nếu có, phân loại:
  - Chấp nhận được.
  - Cần sửa trước khi dùng thật.

## Acceptance Criteria

- Tất cả trang mới mở được, reload không mất dữ liệu.
- Tất cả nút bấm có phản hồi đúng.
- Tất cả bảng hiển thị đúng giá trị, đúng cột, không lệch layout.
- Phân quyền Admin/Staff đúng cả UI và backend.
- Các thao tác ghi dữ liệu có audit log.
- Phiếu kho cập nhật tồn chỉ khi duyệt.
- Bảo hành có lịch sử xử lý đầy đủ.
- Khách hàng có ghi chú chăm sóc lưu bền vững.
- Cấu hình hệ thống chỉ Admin sửa được.
- Dashboard số liệu đối chiếu được với API.
- Build FE/BE pass.
