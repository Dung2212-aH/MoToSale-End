# V2 Admin Basic Operations Completion Plan

## Implementation Status - 2026-06-02

- Phase 1 - Dashboard quan tri thuc te: Done
  - Mo rong API report/dashboard, them KPI van hanh, canh bao ton kho, CRM can xu ly.
  - Frontend dashboard hien KPI, bang canh bao ton kho va CSKH.
- Phase 2 - Bao cao quan tri toi thieu: Done
  - Mo rong report response cho mua hang, thu chi, cong no, sua chua, bao hanh, canh bao ton kho.
  - Frontend reports co tabs va export XLSX nhieu sheet.
- Phase 3 - Don hang: Done
  - Bo sung filter keyword, ngay bat dau/ngay ket thuc.
  - Ghi audit cho huy don, cap nhat trang thai, cap nhat giao nhan, phan bo hang.
- Phase 4 - Kho: Done
  - Chuan hoa phieu kho theo DTO V2: type/store/lines, ho tro nhap, xuat, dieu chinh, kiem ke, chuyen kho.
  - Them audit cho tao/duyet/huy phieu kho, dieu chinh ton, nguong ton, dong bo ton.
- Phase 5 - Ke toan co ban va cong no: Done
  - Trang Van hanh cua hang co thu chi, mua hang, thanh toan NCC, in phieu, export XLSX.
  - Dashboard/report co phai thu/phai tra va thu chi.
- Phase 6 - Ho so khach hang 360: Done
  - Them API `/api/customers/{id}/profile`.
  - Frontend Khach hang co modal ho so 360, timeline, don hang, bao hanh, sua chua, tao lich CSKH.
- Phase 7 - Bao hanh va sua chua: Done
  - Trang Van hanh cua hang tao phieu sua chua co phu tung su dung, cap nhat luong Received -> Repairing -> Ready -> Delivered, in phieu.
  - Bao hanh co san trang quan ly va report.
- Phase 8 - Cham soc khach hang admin: Done
  - CRM co tao lich, loc trang thai/tu khoa, hoan thanh lich, lien ket tu ho so khach hang 360.
- Phase 9 - Phan quyen va audit: Done
  - API admin/staff chinh co Authorize.
  - Audit log duoc ghi cho don hang, ton kho, phieu kho, mua hang, thu chi, NCC, sua chua, CSKH, cham cong.
- Phase 10 - Full regression va nghiem thu: Done
  - `dotnet build` pass.
  - `dotnet test` pass 19/19.
  - `npm run build` pass.
  - UI smoke test pass cac trang: dashboard, reports, stock-documents, customers, business-operations, audit-logs.
  - Screenshot luu tai `D:/MotorTeam/MoToSale-End/docs/ui-smoke-20260602-final`.

## Mục tiêu
Hoàn thiện giao diện quản trị V2 ở mức cơ bản có thể dùng cho cửa hàng bán xe máy và phụ tùng: admin/staff có thể quản lý, theo dõi, can thiệp, in phiếu, xuất báo cáo và kiểm tra trạng thái vận hành mà không cần truy vấn DB thủ công.

Phạm vi chỉ là **Admin UI + API phục vụ Admin UI**, không thay thế các app riêng như POS bán tại quầy, web khách hàng, app kỹ thuật viên hoặc app kho chuyên dụng.

## Rule bắt buộc khi triển khai
- Không được tự ý dừng khi chưa hoàn thành toàn bộ plan hoặc chưa ghi rõ phần bị Blocked.
- Sau mỗi phase phải chạy:
  - `dotnet build v2/backend/src/MoToSale.APIService/MoToSale.APIService.csproj`
  - `dotnet test v2/backend/tests/MoToSale.Backend.Tests/MoToSale.Backend.Tests.csproj`
  - `npm run build` trong `v2/frontend-admin`
- Sau mỗi màn hình mới/sửa phải test UI thật qua browser: tải trang, reload, chuyển trang rồi quay lại, bấm toàn bộ nút chính.
- Mọi nghiệp vụ ghi dữ liệu phải có kiểm tra DB/API sau thao tác: tạo, sửa, duyệt, hủy, xóa mềm nếu có.
- Các thao tác nhạy cảm phải có phân quyền và audit/log: sửa tồn, duyệt/hủy đơn, hoàn tiền, công nợ, thanh toán NCC, xóa dữ liệu.
- Không hard-code dữ liệu nghiệp vụ trong FE nếu BE đã có dữ liệu tương ứng.
- Tất cả bảng quản trị phải có cột căn chỉnh hợp lý: text trái, số/tiền phải, trạng thái/action giữa.
- Tất cả báo cáo phải export được Excel `.xlsx` thật, font tiếng Việt không lỗi.
- Nếu task bị `Blocked`, phải ghi rõ: nguyên nhân, file liên quan, hướng xử lý, trạng thái hiện tại.

## Phase 1 - Dashboard quản trị thực tế
Trạng thái: Pending

### Backend
- Bổ sung endpoint dashboard tổng hợp:
  - Doanh thu hôm nay, tháng này.
  - Số đơn theo trạng thái.
  - Thanh toán đã thu, còn phải thu.
  - Công nợ NCC cần trả.
  - Tồn kho hết hàng, sắp hết hàng, tồn lâu.
  - Phiếu sửa chữa/bảo hành/CSKH cần xử lý hôm nay.
- Chuẩn hóa DTO dashboard theo cấu trúc rõ ràng: sales, orders, inventory, debts, services, crm.

### Frontend
- Sửa Dashboard thành màn hình quản trị thật:
  - KPI cards.
  - Biểu đồ doanh thu theo ngày.
  - Bảng đơn cần xử lý.
  - Bảng tồn kho cảnh báo.
  - Bảng công việc CSKH/sửa chữa/bảo hành.
- Thêm trạng thái empty/loading/error rõ ràng.

### Test
- DB có dữ liệu: số liệu phải khớp API/DB.
- DB ít dữ liệu/rỗng: không vỡ UI, không hiện NaN.
- Reload/chuyển trang không mất layout.

## Phase 2 - Báo cáo quản trị tối thiểu
Trạng thái: Pending

### Backend
- Bổ sung endpoint báo cáo:
  - Bán hàng theo ngày/tháng.
  - Sản phẩm bán chạy/chậm bán.
  - Tồn kho và tuổi tồn.
  - Nhập hàng/mua hàng.
  - Thu chi/công nợ.
  - Bảo hành/sửa chữa.
- Các endpoint đều nhận `fromDate`, `toDate`, `storeId`, `categoryId`, `brandId` nếu phù hợp.

### Frontend
- Nâng cấp trang Báo cáo:
  - Tabs theo nhóm báo cáo.
  - Bộ lọc khoảng ngày, kho/cửa hàng, nhóm sản phẩm.
  - Bảng số liệu + biểu đồ.
  - Export Excel từng tab.
- Mỗi báo cáo có mô tả ngắn: dùng để xem gì, số liệu lấy từ đâu.

### Test
- Đối chiếu ít nhất 3 chỉ số với DB/API.
- Export `.xlsx` mở bằng Excel không lỗi font, đúng cột, đúng tiền/ngày.

## Phase 3 - Đơn hàng: lọc mạnh, timeline, in phiếu
Trạng thái: Pending

### Backend
- Mở rộng filter đơn hàng:
  - Trạng thái đơn.
  - Trạng thái thanh toán.
  - Trạng thái giao/nhận.
  - Khách hàng, số điện thoại.
  - Khoảng ngày.
  - Kho/cửa hàng nếu có.
- Chuẩn hóa timeline đơn hàng:
  - Tạo đơn.
  - Xác nhận.
  - Ghi nhận thanh toán/cọc.
  - Phân bổ/giữ hàng.
  - Giao hàng.
  - Hoàn tất/hủy/trả hàng/hoàn tiền.
- Bổ sung ghi chú nội bộ đơn hàng.

### Frontend
- Trang danh sách đơn:
  - Bộ lọc nâng cao.
  - Export Excel theo filter.
  - Cột khách hàng không được hiện `-` nếu chi tiết có dữ liệu.
- Trang chi tiết đơn:
  - Timeline rõ ràng, giờ đúng timezone hiển thị.
  - Nút in phiếu đơn/hóa đơn bán hàng.
  - Khu vực ghi chú nội bộ.
  - Trạng thái đơn, thanh toán, vận chuyển đồng bộ nghiệp vụ.

### Test
- Tạo/cập nhật đơn qua API/UI mẫu.
- Cập nhật từng trạng thái, kiểm tra timeline sinh log ngay.
- In phiếu không vỡ layout.

## Phase 4 - Kho: kiểm kê, cảnh báo, lịch sử tồn
Trạng thái: Pending

### Backend
- Bổ sung nghiệp vụ kiểm kê:
  - Tạo phiếu kiểm kê.
  - Nhập số lượng thực tế.
  - Tính chênh lệch.
  - Duyệt để tự tạo stock movement điều chỉnh.
- Bổ sung endpoint lịch sử tồn theo SKU/sản phẩm.
- Bổ sung cảnh báo tồn thấp theo ngưỡng SKU.

### Frontend
- Trang tồn kho:
  - Bộ lọc tồn thấp/hết hàng/tồn lâu/đang giữ.
  - Link xem lịch sử tồn từng SKU.
  - Nút tạo phiếu kiểm kê.
- Trang phiếu kho:
  - Tab phiếu kiểm kê.
  - In phiếu kiểm kê/phiếu điều chỉnh.
- In mã vạch hàng loạt theo filter.

### Test
- Tạo phiếu kiểm kê, duyệt, kiểm tra tồn thay đổi đúng.
- Lịch sử tồn khớp stock movements.
- Không cho duyệt phiếu sai dữ liệu.

## Phase 5 - Kế toán cơ bản và công nợ
Trạng thái: Pending

### Backend
- Chuẩn hóa sổ thu chi:
  - Phiếu thu.
  - Phiếu chi.
  - Liên kết đơn hàng, đơn mua, hoàn tiền nếu có.
- Bổ sung công nợ:
  - Công nợ khách hàng.
  - Công nợ nhà cung cấp.
  - Lịch sử thanh toán.
- Bổ sung đối soát đơn hàng:
  - Tổng đơn.
  - Đã thu.
  - Đã hoàn.
  - Còn phải thu.

### Frontend
- Trang Vận hành cửa hàng:
  - Tách tab Thu chi rõ hơn.
  - Bảng công nợ khách hàng/NCC.
  - Nút ghi nhận thanh toán.
  - Nút in phiếu thu/chi.
- Báo cáo công nợ có export Excel.

### Test
- Ghi nhận thanh toán đơn, kiểm tra công nợ giảm.
- Thanh toán NCC, kiểm tra công nợ NCC giảm.
- Hoàn tiền, kiểm tra đã hoàn và còn phải thu đúng.

## Phase 6 - Hồ sơ khách hàng 360 độ
Trạng thái: Pending

### Backend
- Bổ sung endpoint customer profile:
  - Thông tin khách.
  - Đơn đã mua.
  - Xe/sản phẩm đã mua.
  - Bảo hành.
  - Sửa chữa.
  - Liên hệ.
  - CSKH/follow-up.
  - Ghi chú nội bộ.

### Frontend
- Trang khách hàng:
  - Nút xem hồ sơ 360.
  - Timeline hoạt động khách.
  - Lịch sử mua hàng/sửa chữa/bảo hành.
  - Tạo lịch chăm sóc ngay từ hồ sơ.

### Test
- Khách có nhiều đơn/phiếu sửa/bảo hành hiển thị gom đúng.
- Khách mới không có dữ liệu vẫn hiển thị sạch.

## Phase 7 - Bảo hành và sửa chữa chi tiết hơn
Trạng thái: Pending

### Backend
- Mở rộng phiếu sửa chữa:
  - Checklist tiếp nhận.
  - Báo giá sửa chữa.
  - Phụ tùng sử dụng.
  - Công sửa chữa.
  - Timeline trạng thái.
  - Lịch hẹn trả xe.
- Mở rộng bảo hành:
  - Điều kiện bảo hành.
  - Lịch sử xử lý.
  - Liên kết đơn/sản phẩm/SKU/khách.

### Frontend
- Trang bảo hành:
  - Bộ lọc còn hạn/hết hạn/sắp hết hạn.
  - Timeline xử lý bảo hành.
  - In phiếu bảo hành.
- Trang sửa chữa trong vận hành:
  - Form tiếp nhận rõ hơn.
  - Thêm phụ tùng sử dụng.
  - Báo giá/in phiếu sửa chữa.
  - Cập nhật trạng thái theo luồng.

### Test
- Tạo phiếu sửa có phụ tùng, chuyển sang Repairing phải trừ tồn đúng.
- In phiếu sửa/bảo hành không vỡ.
- Timeline cập nhật từng trạng thái.

## Phase 8 - Chăm sóc khách hàng admin
Trạng thái: Pending

### Backend
- Bổ sung phân loại khách hàng:
  - Khách mới.
  - Khách mua xe.
  - Khách mua phụ tùng.
  - Khách cần chăm sóc lại.
- Bổ sung follow-up:
  - Ngày hẹn.
  - Người phụ trách.
  - Trạng thái.
  - Kết quả xử lý.

### Frontend
- Trang CSKH hoặc mở rộng tab CRM:
  - Lịch hẹn hôm nay/quá hạn.
  - Bộ lọc nhân viên/trạng thái/khoảng ngày.
  - Hoàn thành/hẹn lại.
  - Link về hồ sơ khách hàng.

### Test
- Tạo lịch hẹn, hoàn thành, hẹn lại.
- Dashboard hiển thị đúng việc quá hạn/hôm nay.

## Phase 9 - Phân quyền và audit hoàn chỉnh
Trạng thái: Pending

### Backend
- Rà tất cả API admin/staff:
  - Admin toàn quyền.
  - Staff không xóa dữ liệu quan trọng.
  - Staff không sửa cấu hình hệ thống.
- Ghi audit log cho:
  - Sửa đơn hàng.
  - Thanh toán/hoàn tiền.
  - Điều chỉnh tồn.
  - Duyệt đơn mua/trả hàng.
  - Sửa giá/sản phẩm/voucher.

### Frontend
- Ẩn/disable nút theo quyền.
- Trang audit log có filter:
  - Người thao tác.
  - Loại hành động.
  - Đối tượng.
  - Khoảng ngày.

### Test
- Login Admin và Staff, kiểm tra nút/API đúng quyền.
- Dùng API gọi vượt quyền phải trả 403.
- Audit log sinh đúng sau thao tác nhạy cảm.

## Phase 10 - Full regression và nghiệm thu
Trạng thái: Pending

### UI Test bắt buộc
- Dashboard.
- Xe máy.
- Phụ tùng.
- Danh mục.
- Hãng/dòng xe.
- Đơn hàng.
- Voucher.
- Tồn kho.
- Phiếu kho.
- Vận hành cửa hàng.
- Vận hành nâng cao.
- Khách hàng.
- Bảo hành.
- Đánh giá.
- Liên hệ.
- FAQ.
- Bài viết/banner.
- Báo cáo.
- Audit log.

### Checklist test từng trang
- Tải trang lần đầu.
- Reload.
- Chuyển trang khác rồi quay lại.
- Bấm toàn bộ nút chính.
- Mở/đóng modal.
- Submit dữ liệu hợp lệ.
- Submit thiếu field bắt buộc.
- Search/filter/pagination.
- Export Excel nếu có.
- In phiếu nếu có.
- Kiểm tra giá trị cột bảng bằng screenshot/UI thật.
- Kiểm tra responsive desktop/tablet/mobile ở các màn quan trọng.

### Acceptance Criteria
- Admin UI có đủ màn giám sát và can thiệp cơ bản cho cửa hàng.
- Không có lỗi build/test.
- Không có lỗi font tiếng Việt trong UI/export.
- Không có bảng lệch cột, modal tràn, footer/sidebar lỗi.
- Các số liệu dashboard/report khớp backend.
- Tất cả thao tác nhạy cảm có phân quyền và audit.
- Checklist phase 1-10 đều `Done`.
