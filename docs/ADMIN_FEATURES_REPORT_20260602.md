# Báo cáo tính năng Frontend Admin MoToSale V2

Ngày lập: 02/06/2026
Phạm vi: giao diện quản trị V2 và các API phục vụ vận hành admin.

## 1. Tổng quan

Frontend Admin MoToSale V2 là hệ thống quản trị dành cho cửa hàng bán xe máy và phụ tùng. Hệ thống tập trung vào các nghiệp vụ quản lý danh mục, sản phẩm, đơn hàng, tồn kho, mua hàng, thu chi, khách hàng, bảo hành, sửa chữa, chăm sóc khách hàng, nội dung và báo cáo.

Giao diện hiện hỗ trợ hai nhóm quyền chính:

- Admin: toàn quyền, bao gồm người dùng, import dữ liệu và nhật ký hệ thống.
- Staff: thao tác nghiệp vụ vận hành cơ bản, không truy cập các phần quản trị nhạy cảm như user/audit/import tổng.

## 2. Xác thực và phân quyền

Hệ thống có màn hình đăng nhập riêng cho admin/staff. Sau đăng nhập, token được dùng để gọi API bảo vệ bằng quyền `Admin` hoặc `Staff`.

Các chức năng chính:

- Đăng nhập quản trị.
- Tự chuyển hướng người chưa đăng nhập về `/login`.
- Ẩn hoặc chặn route theo quyền.
- Admin được truy cập người dùng, import dữ liệu vận hành và nhật ký hệ thống.
- Staff được truy cập các nghiệp vụ vận hành thường ngày.

## 3. Tổng quan vận hành

Trang Tổng quan cung cấp màn hình giám sát nhanh tình hình cửa hàng.

Các nhóm chỉ số:

- Tổng sản phẩm.
- Tổng đơn hàng.
- Người dùng/khách hàng.
- Doanh thu.
- Doanh thu hôm nay.
- Còn phải thu từ khách.
- Cần trả nhà cung cấp.
- Công việc chăm sóc khách hàng cần xử lý.
- Cảnh báo tồn kho.
- Đơn hàng gần đây.
- Top sản phẩm bán chạy.
- Biểu đồ doanh thu và trạng thái đơn hàng.

Ý nghĩa nghiệp vụ: giúp quản trị viên nhìn nhanh tình trạng bán hàng, tồn kho, tiền và việc cần xử lý trong ngày.

## 4. Quản lý xe máy

Trang Xe máy quản lý nhóm sản phẩm là xe máy.

Chức năng:

- Xem danh sách xe máy.
- Tìm kiếm, lọc, phân trang.
- Thêm, sửa, xóa/ngừng bán sản phẩm.
- Quản lý SKU/biến thể.
- Quản lý ảnh sản phẩm bằng upload file.
- Đặt ảnh chính.
- Quản lý mã vạch.
- Xem chương trình khuyến mại áp dụng cho sản phẩm.
- Xem thời gian tồn kho/tồn lâu.

Nghiệp vụ: xe máy là sản phẩm chính, có thể có biến thể theo màu/phiên bản/SKU và liên kết hãng xe, dòng xe.

## 5. Quản lý phụ tùng

Trang Phụ tùng quản lý nhóm sản phẩm là phụ tùng.

Chức năng:

- CRUD phụ tùng.
- Quản lý SKU/biến thể.
- Upload ảnh.
- In mã vạch.
- Xem chương trình khuyến mại.
- Xem tồn kho/tồn lâu.
- Quản lý sản phẩm liên quan/phụ kiện bán kèm.
- Quản lý cấu hình tương thích xe cho phụ tùng.

Nghiệp vụ: phụ tùng không gắn trực tiếp vào một hãng/dòng xe trong form sản phẩm chính; phần phù hợp xe được tách thành nghiệp vụ tương thích để một phụ tùng có thể áp dụng cho nhiều hãng/dòng xe/năm sản xuất.

## 6. Danh mục

Trang Danh mục quản lý cây danh mục sản phẩm.

Chức năng:

- Tạo, sửa, xóa danh mục.
- Hỗ trợ danh mục cha - con.
- Tách nhóm danh mục xe máy và phụ tùng.
- Mở/thu gọn danh mục con.
- Quản lý trạng thái hoạt động.
- Quản lý thứ tự hiển thị.

Nghiệp vụ: giúp phân loại sản phẩm để lọc, báo cáo và áp dụng voucher/khuyến mại.

## 7. Hãng xe và dòng xe

Trang Hãng xe & Dòng xe quản lý dữ liệu hãng xe và các dòng xe tương ứng.

Chức năng:

- CRUD hãng xe.
- Upload logo hãng.
- CRUD dòng xe.
- Lọc dòng xe theo hãng.
- Quản lý trạng thái hãng/dòng xe.

Nghiệp vụ: dùng cho xe máy, phụ tùng tương thích xe và bộ lọc sản phẩm.

## 8. Đơn hàng

Trang Đơn hàng gồm danh sách và chi tiết đơn hàng.

Chức năng danh sách:

- Xem danh sách đơn.
- Tìm theo mã đơn, khách hàng, email, số điện thoại.
- Lọc trạng thái đơn.
- Lọc trạng thái thanh toán.
- Lọc trạng thái vận chuyển/giao nhận.
- Lọc theo khoảng ngày.
- Phân trang.
- Xuất dữ liệu theo bộ lọc.

Chức năng chi tiết:

- Xem thông tin khách hàng, địa chỉ, dòng hàng.
- Xem thanh toán, đặt cọc/còn phải thu.
- Cập nhật trạng thái đơn.
- Cập nhật trạng thái giao nhận.
- Hủy đơn.
- Phân bổ/giữ hàng theo kho.
- Ghi nhận thanh toán thủ công.
- In phiếu/hóa đơn.
- Xem lịch sử đơn hàng theo timeline.

Nghiệp vụ: trạng thái đơn và trạng thái giao nhận được quản lý thủ công theo thực tế cửa hàng; admin xác nhận thanh toán và vận chuyển bằng thao tác quản trị.

## 9. Voucher

Trang Voucher quản lý mã giảm giá.

Chức năng:

- CRUD voucher.
- Thiết lập mã voucher.
- Loại giảm giá: phần trăm hoặc số tiền.
- Giá trị giảm.
- Thời gian hiệu lực.
- Giới hạn sử dụng.
- Điều kiện giá trị đơn.
- Phạm vi áp dụng.
- Chọn danh mục, sản phẩm, hãng xe bằng UI chọn dữ liệu thay vì nhập tay.

Nghiệp vụ: voucher được dùng cho chính sách khuyến mại bán hàng, có thể áp dụng toàn hệ thống hoặc giới hạn theo nhóm hàng cụ thể.

## 10. Tồn kho

Trang Tồn kho dùng để giám sát tồn thực tế, tồn giữ chỗ và tồn khả dụng.

Chức năng:

- Xem tồn kho theo SKU.
- Hiển thị mã sản phẩm, SKU, tên sản phẩm, biến thể.
- Tồn thực tế.
- Số lượng đang giữ chỗ.
- Tồn khả dụng.
- Ngưỡng cảnh báo tồn thấp.
- Trạng thái tồn: còn hàng, sắp hết, hết hàng.
- Lọc tồn thấp/hết hàng/đang giữ chỗ.
- Xem chi tiết giữ chỗ thuộc đơn nào.
- Xem lịch sử điều chỉnh tồn.
- Cập nhật ngưỡng tồn thấp.
- Điều chỉnh tồn có lý do.
- Đồng bộ tồn theo sổ cái.
- Xuất Excel tồn kho.

Nghiệp vụ: tồn kho được quản lý theo SKU và kho/cửa hàng; mọi điều chỉnh nhạy cảm có audit log.

## 11. Phiếu kho

Trang Phiếu kho quản lý chứng từ kho.

Loại phiếu:

- Phiếu nhập kho.
- Phiếu xuất kho.
- Phiếu điều chỉnh tồn.
- Phiếu kiểm kê.
- Phiếu chuyển kho.

Chức năng:

- Tạo phiếu nháp.
- Chọn kho áp dụng/kho xuất.
- Chọn kho nhận khi chuyển kho.
- Chọn SKU trực tiếp từ dữ liệu backend.
- Nhập số lượng hoặc tồn thực tế sau kiểm kê.
- Duyệt phiếu để cập nhật tồn.
- Hủy phiếu nháp.
- Xem chi tiết phiếu.
- In phiếu.
- Xuất Excel.
- Audit thao tác tạo, duyệt, hủy.

Nghiệp vụ: phiếu kiểm kê/điều chỉnh dùng số lượng thực tế sau kiểm đếm; khi duyệt, hệ thống tự tính chênh lệch và ghi stock movement.

## 12. Vận hành nâng cao

Trang Vận hành nâng cao quản lý các nghiệp vụ sau bán và vận hành mở rộng.

Chức năng chính:

- Trả hàng/hoàn tiền.
- Theo dõi giao dịch hoàn tiền.
- Quản lý công nợ/cọc.
- Quản lý ca làm/chấm công.
- Các nghiệp vụ vận hành bổ sung phục vụ cửa hàng.

Nghiệp vụ: đây là khu vực dành cho các thao tác có tính phát sinh, không nằm trong luồng bán hàng cơ bản.

## 13. Vận hành cửa hàng

Trang Vận hành cửa hàng gom các nghiệp vụ cửa hàng dùng hằng ngày.

Nhóm Nhà cung cấp:

- Xem danh sách nhà cung cấp.
- Thêm/sửa nhà cung cấp.
- Import nhà cung cấp từ XLSX.
- Export XLSX.

Nhóm Mua hàng:

- Tạo đơn mua hàng.
- Chọn nhà cung cấp.
- Chọn kho nhận.
- Nhập nhiều dòng SKU, số lượng, giá nhập.
- Duyệt đơn mua.
- Nhận hàng còn lại.
- Thanh toán nhà cung cấp.
- In phiếu mua.
- Export XLSX.

Nhóm Thu chi:

- Tạo phiếu thu.
- Tạo phiếu chi.
- Nhập nhóm thu chi.
- Nhập số tiền, hình thức, ghi chú.
- In phiếu thu/chi.
- Export XLSX.

Nhóm Sửa chữa:

- Tạo phiếu sửa chữa.
- Chọn khách hàng, cửa hàng, nhân viên phụ trách.
- Nhập thông tin xe.
- Nhập lỗi ghi nhận.
- Nhập công sửa chữa.
- Thêm phụ tùng sử dụng.
- Cập nhật trạng thái: tiếp nhận, đang sửa, sửa xong, bàn giao.
- Trừ tồn phụ tùng khi bắt đầu sửa.
- In phiếu sửa chữa.
- Export XLSX.

Nhóm Chăm sóc khách hàng:

- Tạo lịch chăm sóc.
- Chọn khách hàng, nhân viên phụ trách.
- Nhập nội dung, thời gian nhắc lại, ghi chú.
- Lọc theo trạng thái/từ khóa.
- Hoàn thành lịch chăm sóc.
- Export XLSX.

Nhóm Chấm công:

- Check-in nhân viên.
- Check-out.
- Theo dõi cửa hàng, thời gian, ghi chú.
- Export XLSX.

## 14. Import dữ liệu vận hành

Trang Import dữ liệu dành cho Admin.

Chức năng:

- Import sản phẩm/SKU bằng file XLSX.
- Nhập nhanh sản phẩm/SKU bằng text nhiều dòng.
- Import tồn đầu kỳ.
- Đọc, kiểm tra, preview dữ liệu trước khi gửi.
- Gửi dữ liệu vào backend.

Nghiệp vụ: phục vụ nhập dữ liệu ban đầu hoặc cập nhật hàng loạt, giảm thao tác thủ công.

## 15. Người dùng

Trang Người dùng dành cho Admin.

Chức năng:

- Xem danh sách user.
- Thêm user.
- Gán vai trò.
- Sửa thông tin user.
- Khóa/mở user.
- Xóa hoặc vô hiệu hóa user theo nghiệp vụ.
- Phân biệt Admin/Staff/Customer.

Nghiệp vụ: hệ thống hướng tới chỉ cần một Admin chính và các Staff vận hành, tránh nhiều admin gây rủi ro quyền hạn.

## 16. Khách hàng

Trang Khách hàng quản lý hồ sơ khách mua hàng.

Chức năng:

- Xem danh sách khách hàng.
- Tìm kiếm theo tên, SĐT, email.
- Lọc trạng thái.
- Xem tổng đơn, tổng chi tiêu, đơn hủy, đơn gần nhất.
- Ghi chú chăm sóc.
- Xuất Excel.
- Xem hồ sơ khách hàng 360.

Hồ sơ khách hàng 360 gồm:

- Thông tin khách.
- Tổng đơn hàng.
- Tổng giá trị đã mua.
- Công nợ/còn phải thu.
- Bảo hành.
- Sửa chữa.
- CSKH đang mở.
- Timeline hoạt động.
- Tạo lịch chăm sóc ngay từ hồ sơ.

## 17. Bảo hành

Trang Bảo hành quản lý phiếu bảo hành.

Chức năng:

- Tạo phiếu bảo hành.
- Liên kết đơn hàng/SKU/khách hàng nếu có.
- Nhập sản phẩm, serial, số khung, số máy.
- Nhập lỗi khách báo.
- Nhập chi phí dự kiến/thực tế.
- Cập nhật trạng thái bảo hành.
- Xem lịch sử xử lý.
- In phiếu bảo hành.

Nghiệp vụ: phục vụ theo dõi sau bán, bảo hành xe/phụ tùng và lịch sử xử lý.

## 18. Đánh giá

Trang Đánh giá quản lý review sản phẩm.

Chức năng:

- Xem danh sách đánh giá.
- Lọc/tìm kiếm.
- Duyệt/ẩn đánh giá.
- Xóa nếu cần.
- Hiển thị sản phẩm, khách hàng, sao, nội dung, trạng thái.

Nghiệp vụ: kiểm soát nội dung khách hàng hiển thị trên frontend khách.

## 19. Banner trang chủ

Trang Banner trang chủ quản lý nội dung banner.

Chức năng:

- Thêm/sửa/xóa banner.
- Upload ảnh.
- Thiết lập tiêu đề, liên kết, thứ tự, trạng thái.

Nghiệp vụ: phục vụ quản trị nội dung marketing trên trang chủ.

## 20. Bài viết

Trang Bài viết quản lý nội dung tin tức/tư vấn.

Chức năng:

- CRUD bài viết.
- Tiêu đề, slug, tóm tắt, nội dung.
- Upload/chọn ảnh đại diện.
- Danh mục bài viết.
- Trạng thái bản nháp/đã xuất bản.
- Phân trang.

Nghiệp vụ: phục vụ nội dung SEO, tư vấn mua xe, bảo dưỡng, tin cửa hàng.

## 21. FAQ

Trang FAQ quản lý câu hỏi thường gặp.

Chức năng:

- CRUD câu hỏi.
- Câu trả lời.
- Trạng thái hiển thị/ẩn.
- Sắp xếp thứ tự.

Nghiệp vụ: giúp giảm tải chăm sóc khách hàng bằng thông tin tự phục vụ.

## 22. Liên hệ

Trang Liên hệ quản lý yêu cầu liên hệ từ khách.

Chức năng:

- Xem danh sách yêu cầu.
- Xem chi tiết.
- Lọc trạng thái.
- Đánh dấu đã xử lý.

Nghiệp vụ: tiếp nhận lead, phản hồi khách và lưu dấu xử lý.

## 23. Báo cáo và thống kê

Trang Báo cáo & Thống kê gom các báo cáo quản trị.

Nhóm báo cáo:

- Bán hàng.
- Mua hàng.
- Thu chi/công nợ.
- Dịch vụ sửa chữa/bảo hành.
- Tồn kho/cảnh báo.

Chức năng:

- Lọc khoảng ngày.
- Xem KPI.
- Xem bảng số liệu.
- Xem biểu đồ doanh thu/trạng thái/top sản phẩm.
- Export XLSX nhiều sheet.
- Mô tả mục đích và nguồn dữ liệu báo cáo.

Nghiệp vụ: phục vụ đánh giá hoạt động mua - bán, tồn kho, dòng tiền, dịch vụ và chăm sóc khách hàng.

## 24. Nhật ký hệ thống

Trang Nhật ký hệ thống dành cho Admin.

Chức năng:

- Xem audit log.
- Lọc theo đối tượng.
- Lọc theo hành động.
- Lọc theo người thực hiện.
- Lọc theo từ khóa.
- Lọc theo khoảng ngày.

Các hành động đã ghi audit:

- Đơn hàng: hủy, phân bổ, đổi trạng thái đơn, đổi trạng thái giao nhận.
- Tồn kho: điều chỉnh, đồng bộ, cập nhật ngưỡng tồn.
- Phiếu kho: tạo, duyệt, hủy.
- Mua hàng: tạo, duyệt, hủy, nhận hàng, thanh toán NCC.
- Thu chi: tạo phiếu.
- Nhà cung cấp: tạo/sửa.
- Sửa chữa: tạo phiếu, đổi trạng thái.
- Chăm sóc khách hàng: tạo, hoàn thành.
- Chấm công: check-in/check-out.

Nghiệp vụ: đảm bảo truy vết các thao tác nhạy cảm.

## 25. Cấu hình vận hành

Trang Cấu hình vận hành quản lý các thiết lập nghiệp vụ.

Chức năng:

- Xem/cập nhật setting vận hành.
- Cấu hình chính sách hoặc tham số dùng chung.

Nghiệp vụ: tách các tham số vận hành khỏi code để admin có thể điều chỉnh.

## 26. Khả năng xuất/nhập dữ liệu

Hệ thống hỗ trợ:

- Export XLSX cho báo cáo.
- Export XLSX cho tồn kho.
- Export XLSX cho khách hàng.
- Export XLSX cho vận hành cửa hàng: NCC, mua hàng, thu chi, sửa chữa, CRM, chấm công.
- Import XLSX nhà cung cấp.
- Import XLSX sản phẩm/SKU.
- Import tồn đầu kỳ.
- Nhập nhanh sản phẩm/SKU bằng text.

## 27. Đánh giá mức sẵn sàng

Admin V2 đã bao phủ các nghiệp vụ cơ bản để cửa hàng có thể vận hành ở mức quản trị:

- Quản lý sản phẩm xe máy/phụ tùng.
- Quản lý danh mục, hãng, dòng xe.
- Quản lý bán hàng và đơn hàng.
- Quản lý tồn kho và phiếu kho.
- Quản lý mua hàng.
- Quản lý thu chi và công nợ cơ bản.
- Quản lý khách hàng và CSKH.
- Quản lý bảo hành/sửa chữa.
- Quản lý nội dung.
- Báo cáo, biểu đồ, export dữ liệu.
- Phân quyền và audit.

Các nghiệp vụ nâng cao có thể tiếp tục mở rộng sau:

- Kế toán chuyên sâu theo chuẩn sổ sách.
- POS bán tại quầy riêng.
- App kho/nhân viên kỹ thuật chuyên dụng.
- Báo cáo quản trị nâng cao theo chi nhánh/nhân viên/biên lợi nhuận.
- Tự động hóa nhắc lịch CSKH/bảo dưỡng.
