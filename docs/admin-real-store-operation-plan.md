# Admin Real Store Operation Plan

## Mục tiêu

Hoàn thiện Frontend Admin và Backend để có thể vận hành thực tế tại cửa hàng bán xe máy và phụ tùng, không chỉ dừng ở quản trị dữ liệu.

Hệ thống sau khi hoàn thiện phải hỗ trợ:

- Nhân viên vận hành theo quyền.
- Xử lý đơn hàng, thanh toán, giao nhận.
- Quản lý tồn kho bằng chứng từ.
- Audit log các thao tác quan trọng.
- In/xuất chứng từ phục vụ bán hàng và kho.
- Quản lý hồ sơ khách hàng.
- Báo cáo/export phục vụ đối soát thật.

## Rule Bắt Buộc

- Không làm big-bang.
- Làm theo từng phase, mỗi phase phải build/test xong mới sang phase tiếp theo.
- Không phá flow hiện tại: sản phẩm, đơn hàng, tồn kho, voucher, người dùng.
- Mọi nghiệp vụ thay đổi dữ liệu quan trọng phải có audit log.
- Staff không được có quyền nguy hiểm như xóa dữ liệu lõi, sửa admin, chỉnh cấu hình hệ thống.
- Mọi phiếu/chứng từ phải có mã chứng từ, người tạo, thời gian tạo, ghi chú.
- Mọi số tiền/tồn kho phải đối soát được từ lịch sử.
- Backend phải chặn quyền, không chỉ ẩn nút trên UI.
- Sau mỗi phase phải chạy build FE/BE liên quan.
- Sau mỗi phase phải test bằng UI thật với tài khoản Admin và Staff.

## Phase 1: Phân Quyền Admin/Staff

### Mục tiêu

Staff dùng được hệ thống để vận hành nhưng không thể phá dữ liệu quan trọng.

### Role chuẩn

- `Admin`
- `Staff`
- `Customer`

### Quyền đề xuất

Admin:

- Toàn quyền.
- Quản lý tài khoản admin/staff.
- Chỉnh cấu hình hệ thống.
- Xóa hoặc khôi phục dữ liệu lõi nếu có nghiệp vụ cho phép.

Staff:

- Xem/tạo/sửa sản phẩm nhưng không xóa cứng.
- Xem/cập nhật đơn hàng.
- Xác nhận thanh toán thủ công.
- Cập nhật vận chuyển.
- Xem/tạo phiếu kho.
- Xem khách hàng.
- Không sửa/xóa admin.
- Không chỉnh cấu hình hệ thống.
- Không xóa cứng dữ liệu lõi.

Customer:

- Chỉ dùng frontend khách hàng.

### Việc cần làm

- Rà backend authorization hiện tại.
- Chuẩn hóa middleware/policy role.
- Tạo ma trận quyền cho từng controller/action.
- FE ẩn hoặc disable nút không đủ quyền.
- Backend vẫn phải trả `403` nếu gọi API trái quyền.
- Test bằng tài khoản Admin và Staff.

## Phase 2: Audit Log Toàn Hệ Thống

### Mục tiêu

Biết ai đã sửa gì, sửa lúc nào, sửa từ giá trị nào sang giá trị nào.

### Đối tượng cần log

- Sản phẩm.
- Biến thể.
- Ảnh sản phẩm.
- Danh mục.
- Hãng xe/dòng xe.
- Voucher.
- Người dùng.
- Tồn kho.
- Đơn hàng.
- Thanh toán.
- Bài viết.
- FAQ.
- Liên hệ.

### Cấu trúc log đề xuất

- Mã log.
- Loại đối tượng.
- Mã đối tượng.
- Hành động: `Create`, `Update`, `Delete`, `StatusChange`, `Adjust`, `Confirm`.
- Giá trị trước.
- Giá trị sau.
- Người thực hiện.
- Thời gian.
- Ghi chú.
- IP hoặc user agent nếu lấy được.

### FE cần có

- Trang `Nhật ký hệ thống`.
- Filter theo:
  - Người thực hiện.
  - Loại đối tượng.
  - Hành động.
  - Khoảng thời gian.
- Xem chi tiết thay đổi.

## Phase 3: Phiếu Nhập/Xuất/Điều Chỉnh Kho

### Mục tiêu

Không sửa tồn kho trực tiếp mà qua chứng từ có thể đối soát.

### Loại phiếu

- Phiếu nhập kho.
- Phiếu xuất kho.
- Phiếu điều chỉnh tồn.

### Mỗi phiếu cần có

- Mã phiếu.
- Loại phiếu.
- Kho/showroom nếu có.
- Người tạo.
- Ngày tạo.
- Ghi chú.
- Danh sách sản phẩm/SKU.
- Số lượng trước.
- Số lượng thay đổi.
- Số lượng sau.
- Trạng thái phiếu.

### Nghiệp vụ

- Sau khi duyệt phiếu mới cập nhật tồn kho.
- Không cho sửa phiếu đã duyệt.
- Phiếu đã duyệt chỉ được hủy/đảo phiếu theo nghiệp vụ riêng.
- Mọi thay đổi tồn kho phải có audit log.

### FE cần có

- Danh sách phiếu kho.
- Tạo phiếu.
- Chi tiết phiếu.
- Duyệt/hủy phiếu.
- Export Excel.

## Phase 4: In/PDF Chứng Từ

### Mục tiêu

Cửa hàng có thể in giấy hoặc xuất PDF phục vụ bán hàng, giao nhận và kho.

### Chứng từ cần có

- Phiếu đơn hàng.
- Biên nhận thanh toán.
- Phiếu giao hàng.
- Phiếu xuất kho.
- Phiếu nhập kho.
- Phiếu bảo hành nếu làm Phase 6.

### Nội dung PDF

- Logo cửa hàng.
- Tên cửa hàng.
- Địa chỉ/hotline.
- Mã chứng từ.
- Thông tin khách.
- Danh sách sản phẩm.
- Số tiền.
- Trạng thái.
- Người lập.
- Ngày giờ.
- Khu vực ký nhận nếu cần.

### FE cần có

- Nút `In phiếu` trong chi tiết đơn.
- Nút `Xuất PDF`.
- Nút `In phiếu kho`.

## Phase 5: Quản Lý Khách Hàng Thật

### Mục tiêu

Trang người dùng không chỉ là tài khoản, mà hỗ trợ chăm sóc và đối soát khách hàng.

### Thông tin khách hàng

- Họ tên.
- SĐT.
- Email.
- Địa chỉ.
- Tổng đơn.
- Tổng chi tiêu.
- Đơn gần nhất.
- Số đơn hủy.
- Ghi chú chăm sóc.
- Lịch sử mua hàng.
- Lịch sử liên hệ.

### Nghiệp vụ

- Tìm khách theo SĐT/email/tên.
- Xem chi tiết khách.
- Ghi chú chăm sóc khách.
- Export danh sách khách.
- Staff chỉ được xem/sửa trong phạm vi quyền cho phép.

## Phase 6: Bảo Hành Xe Máy/Phụ Tùng

### Mục tiêu

Phù hợp nghiệp vụ cửa hàng xe máy và phụ tùng.

### Thông tin bảo hành

- Mã bảo hành.
- Khách hàng.
- Đơn hàng gốc.
- Sản phẩm/SKU.
- Số khung/số máy nếu là xe.
- Ngày mua.
- Thời hạn bảo hành.
- Lỗi khách báo.
- Trạng thái xử lý.
- Lịch sử xử lý.
- Chi phí nếu ngoài bảo hành.

### Trạng thái đề xuất

- Tiếp nhận.
- Đang xử lý.
- Chờ linh kiện.
- Hoàn tất.
- Từ chối.

### FE cần có

- Trang danh sách bảo hành.
- Tạo phiếu bảo hành từ đơn hàng.
- Chi tiết bảo hành.
- Cập nhật trạng thái.
- In phiếu tiếp nhận/trả hàng.

## Phase 7: Showroom/Kho

### Mục tiêu

Chuẩn bị cho vận hành nhiều cửa hàng/kho.

### Nghiệp vụ

- Quản lý showroom/kho.
- Nhân viên thuộc showroom.
- Tồn kho theo showroom.
- Đơn hàng thuộc showroom.
- Báo cáo theo showroom.
- Chuyển kho nếu có nhiều kho.

### Ghi chú

Nếu hiện tại chỉ có một cửa hàng, phase này có thể làm sau nhưng thiết kế DB/API không nên khóa chết vào một kho duy nhất.

## Phase 8: Cấu Hình Hệ Thống

### Mục tiêu

Không hard-code các chính sách vận hành.

### Cấu hình cần có

- Tên cửa hàng.
- Logo.
- Địa chỉ.
- Hotline.
- Ngưỡng tồn thấp mặc định.
- Chính sách đặt cọc.
- Chính sách hủy đơn.
- Chính sách bảo hành.
- Phí vận chuyển mặc định.
- Mẫu nội dung phiếu/in.

### Quyền

Chỉ Admin được sửa cấu hình hệ thống.

## Phase 9: Dashboard Vận Hành

### Mục tiêu

Dashboard không chỉ là báo cáo, mà là màn hình làm việc hằng ngày.

### Widget cần có

- Đơn chờ xác nhận.
- Đơn chưa thanh toán.
- Đơn đang giao.
- Sản phẩm hết hàng.
- Sản phẩm sắp hết.
- Liên hệ chưa xử lý.
- Voucher sắp hết hạn.
- Doanh thu hôm nay/tháng.
- Top sản phẩm bán chạy.
- Cảnh báo thao tác lỗi hoặc tồn âm nếu có.

## Acceptance Criteria

- Nhân viên đăng nhập và chỉ thấy quyền phù hợp.
- Backend chặn được API trái quyền.
- Mọi thao tác quan trọng có audit log.
- Tồn kho thay đổi qua phiếu/chứng từ.
- Đơn hàng có thể in phiếu/hóa đơn/giao hàng.
- Khách hàng có hồ sơ và lịch sử mua.
- Báo cáo/export phục vụ đối soát thật.
- Không có thao tác nguy hiểm không kiểm soát.
- Build FE/BE pass.
- Test bằng UI thật với Admin và Staff.

## Thứ Tự Làm Khuyến Nghị

1. Phân quyền Admin/Staff.
2. Audit log toàn hệ thống.
3. Phiếu kho.
4. In/PDF chứng từ đơn hàng.
5. Quản lý khách hàng.
6. Dashboard vận hành.
7. Bảo hành.
8. Showroom/kho.
9. Cấu hình hệ thống.
