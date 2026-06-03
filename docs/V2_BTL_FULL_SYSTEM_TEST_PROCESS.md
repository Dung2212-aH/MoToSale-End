# Quy trình test tổng thể hệ thống quản trị MoToSale V2 cho BTL

## 1. Mục tiêu

Quy trình này dùng để kiểm tra toàn bộ hệ thống quản trị BTL theo mạch nghiệp vụ lõi:

1. Sản phẩm
2. Bán hàng
3. Kho và cung ứng
4. Hậu mãi và dịch vụ
5. Tài chính, hệ thống và báo cáo

Mục tiêu không chỉ là kiểm tra trang có mở được hay không, mà phải kiểm tra:

- Dữ liệu hiển thị đúng từ backend.
- Từng bảng, từng cột, từng badge trạng thái, từng giá trị tiền/số lượng/ngày giờ.
- Từng nút bấm trên trang.
- Từng modal con.
- Từng field trong modal.
- Submit dữ liệu hợp lệ, thiếu dữ liệu, sai định dạng, dữ liệu dài.
- Luồng nghiệp vụ liên hoàn từ sản phẩm -> bán hàng -> kho -> trả hàng/bảo hành -> tài chính/báo cáo.
- Quyền Admin/Staff.
- Giao diện dễ nhìn, không vỡ layout, không lỗi tiếng Việt, không lỗi footer/sidebar/modal.

## 2. Quy tắc bắt buộc khi test

- Không chỉ đọc code, phải test bằng UI thật.
- Không bỏ qua modal nào, nút nào, field nào.
- Mỗi bảng phải chụp screenshot để đối chiếu header, giá trị, badge, action button.
- Mỗi modal phải test mở, đóng bằng nút X, đóng bằng nút Đóng/Hủy, submit thành công, submit lỗi.
- Mỗi field phải test ít nhất 4 nhóm dữ liệu: hợp lệ, thiếu bắt buộc, sai định dạng, dữ liệu dài.
- Với dữ liệu thay đổi DB, phải kiểm tra lại ở trang liên quan và kiểm tra tác động nghiệp vụ.
- Với thao tác nguy hiểm như xóa, hủy, duyệt, từ chối, hoàn tiền, phải xác nhận hệ thống có cảnh báo hoặc luồng xử lý rõ ràng.
- Sau mỗi nhóm chức năng phải reload trang, chuyển sang trang khác rồi quay lại để kiểm tra dữ liệu còn đúng.
- Sau khi hoàn tất phải chạy `npm run build`, `dotnet build`, `dotnet test`.
- Nếu phát hiện lỗi, ghi rõ: trang, nút/modal/field, dữ liệu nhập, kết quả mong muốn, kết quả thực tế, ảnh màn hình, mức độ lỗi.

## 3. Môi trường test

### 3.1. Dịch vụ cần chạy

- Auth Service: `http://localhost:5101`
- API Service: `http://localhost:5102`
- API Gateway: `http://localhost:5100`
- Frontend Admin: `http://localhost:5176`

### 3.2. Tài khoản test

- Admin: `admin@motosale.local / Admin@123`
- Staff: dùng tài khoản Staff seed hoặc tạo mới trong trang Tài khoản hệ thống.

### 3.3. Dữ liệu sát thực tế nên có

- Xe máy:
  - Honda Vision 2024, Honda Winner X 2024, Yamaha Exciter 155 VVA, Yamaha Janus.
  - Có nhiều phiên bản/SKU, có giá gốc, có giá khuyến mãi, có barcode.
- Phụ tùng:
  - Nhớt Motul 300V, Lốp Michelin Pilot Street 2, Má phanh Honda, Ắc quy GS, Mũ bảo hiểm.
  - Có hãng sản xuất phụ tùng, có sản phẩm tương thích xe.
- Khách hàng:
  - Khách lẻ.
  - Khách có số điện thoại.
  - Khách có nhiều đơn hàng.
- Đơn hàng:
  - Đơn online chờ xác nhận.
  - Đơn POS bán đứt.
  - Đơn đặt cọc.
  - Đơn đã giao.
  - Đơn đã hủy.
- Kho:
  - SKU còn hàng.
  - SKU hết hàng.
  - SKU sắp hết hàng.
  - Phiếu nhập, phiếu xuất, phiếu điều chỉnh.
- Hậu mãi:
  - Phiếu trả hàng chờ duyệt.
  - Phiếu trả hàng đã duyệt.
  - Bảo hành đang xử lý.
  - Lịch hẹn CSKH.
- Tài chính:
  - Thu tiền bán hàng.
  - Chi nhập hàng.
  - Hoàn tiền trả hàng.
  - Công nợ khách còn thiếu.

## 4. Chuẩn kiểm tra cho mọi trang

Mỗi trang phải chạy theo thứ tự sau:

1. Mở trang từ menu.
2. Kiểm tra tiêu đề trang, breadcrumb nếu có, active menu.
3. Kiểm tra trang không trắng, không lỗi console, không lỗi API 4xx/5xx bất thường.
4. Kiểm tra bảng/list:
   - Header cột đúng nghiệp vụ.
   - Căn trái/căn giữa/căn phải hợp lý.
   - Tiền tệ hiển thị VNĐ.
   - Ngày giờ đúng định dạng Việt Nam.
   - Trạng thái hiển thị tiếng Việt.
   - Dữ liệu dài không tràn.
   - Empty state rõ ràng khi không có dữ liệu.
5. Kiểm tra filter/search/sort/pagination nếu có.
6. Kiểm tra toàn bộ nút trên trang.
7. Kiểm tra từng modal theo quy trình tại mục 5.
8. Reload trang.
9. Chuyển sang trang khác rồi quay lại.
10. Kiểm tra responsive:
    - Desktop 1440px.
    - Tablet 768px.
    - Mobile 390px nếu trang có thể dùng trên mobile.
11. Chụp screenshot trước và sau thao tác chính.

## 5. Chuẩn kiểm tra cho mọi modal

Mỗi modal phải test theo cấu trúc:

### 5.1. Tổng thể modal

- Modal mở đúng khi bấm nút.
- Tiêu đề modal đúng ngữ cảnh: Thêm, Sửa, Chi tiết, Duyệt, Từ chối, Hủy.
- Overlay không che mất nội dung cần thao tác.
- Modal không tràn màn hình.
- Nút X đóng được.
- Nút Đóng/Hủy đóng được.
- Bấm submit khi dữ liệu hợp lệ hoạt động đúng.
- Bấm submit khi dữ liệu lỗi hiển thị thông báo rõ ràng.
- Sau submit thành công, modal đóng hoặc chuyển trạng thái hợp lý.
- Dữ liệu trong bảng được refresh.
- Reload trang vẫn thấy dữ liệu mới.

### 5.2. Từng field trong modal

Với từng field nhập liệu:

- Nhập hợp lệ.
- Bỏ trống nếu là field bắt buộc.
- Nhập dữ liệu dài.
- Nhập ký tự đặc biệt.
- Nhập sai kiểu dữ liệu.
- Copy/paste dữ liệu.
- Kiểm tra placeholder, label, thông báo lỗi.

Với select/checkbox/radio:

- Chọn từng option.
- Chọn option đầu/cuối.
- Bỏ chọn nếu cho phép.
- Chọn nhiều nếu là multi-select.
- Kiểm tra dữ liệu phụ thuộc, ví dụ chọn hãng xe thì danh sách dòng xe đổi theo.

Với upload file:

- Upload file hợp lệ `.jpg/.png/.webp`.
- Upload file quá lớn.
- Upload sai định dạng.
- Kiểm tra preview.
- Lưu xong reload trang ảnh vẫn còn.

### 5.3. Từng nút trong modal

- Submit/Lưu/Cập nhật.
- Hủy/Đóng.
- Thêm dòng con.
- Xóa dòng con.
- Upload/Đổi ảnh.
- Xóa ảnh.
- Duyệt/Từ chối/Xác nhận.
- In/Xuất file nếu có.

## 6. Kiểm tra theo nhóm menu lõi

## 6.1. Tổng quan

### Trang Dashboard

Kiểm tra:

- Tổng doanh thu.
- Tổng đơn hàng.
- Đơn chờ xử lý.
- Sản phẩm sắp hết hàng.
- Bảng đơn gần đây.
- Bảng sản phẩm bán chạy.
- Chart doanh thu/đơn hàng.
- Trạng thái dữ liệu rỗng.
- Dữ liệu có thay đổi sau khi tạo đơn POS mới.

Kịch bản:

1. Ghi nhận số liệu ban đầu.
2. Tạo đơn POS bán đứt.
3. Quay lại Dashboard.
4. Kiểm tra doanh thu, đơn hàng, đơn gần đây có cập nhật.
5. Hủy hoặc trả hàng một đơn.
6. Kiểm tra báo cáo không tính sai doanh thu thực nhận.

## 6.2. Kinh doanh và sản phẩm

### Xe máy

Test bảng:

- Mã xe.
- Tên xe.
- Danh mục.
- Hãng xe.
- Dòng xe.
- Giá gốc.
- Giá khuyến mãi.
- Tồn kho.
- Trạng thái bán/ngừng bán.
- Action: xem/sửa/xóa/ảnh/SKU/barcode/tương thích/khuyến mại nếu có.

Test modal/form:

- Thêm xe máy.
- Sửa xe máy.
- Upload ảnh.
- Thêm/sửa/xóa SKU/biến thể.
- In mã vạch.
- Xem chương trình khuyến mại áp dụng.
- Xem thời gian tồn kho.

Dữ liệu test:

- Xe có đầy đủ hãng/dòng xe.
- Xe chưa có giá khuyến mãi.
- Xe có giá khuyến mãi thấp hơn giá gốc.
- Xe có nhiều biến thể màu/phiên bản.
- Xe đang bán và ngừng bán.

Kịch bản lỗi:

- Giá khuyến mãi lớn hơn giá gốc.
- Bỏ trống mã/tên.
- Chọn danh mục sai.
- Upload ảnh sai định dạng.
- In mã vạch không được ra nhiều trang thừa.

### Phụ tùng

Test bảng:

- Mã phụ tùng.
- Tên phụ tùng.
- Danh mục.
- Hãng sản xuất phụ tùng.
- Giá gốc.
- Giá khuyến mãi.
- Tồn kho.
- Trạng thái.
- Action.

Test modal/form:

- Thêm phụ tùng.
- Sửa phụ tùng.
- Upload ảnh.
- SKU/barcode.
- Tương thích xe.
- Phụ kiện/sản phẩm liên quan.
- Chương trình khuyến mại.

Kịch bản:

- Phụ tùng không cần hãng xe trực tiếp.
- Phụ tùng có hãng sản xuất phụ tùng.
- Phụ tùng tương thích nhiều dòng xe.
- Phụ tùng thay đổi tương thích rồi reload vẫn còn.

### Danh mục

Test:

- Danh mục cha: Xe máy, Phụ tùng.
- Danh mục con xổ đúng dưới nhóm cha.
- Thêm/sửa/xóa/ẩn danh mục.
- Không cho xóa danh mục đang có sản phẩm nếu backend chặn.
- Tên danh mục không trùng trong cùng cấp nếu có rule.

### Hãng xe và dòng xe

Test:

- CRUD hãng xe.
- Upload logo hãng xe.
- CRUD dòng xe.
- Chọn hãng xe thì dòng xe lọc đúng.
- Logo không quá nhỏ, không méo.
- Reload sau upload logo vẫn còn.

### Hãng sản xuất phụ tùng

Test:

- CRUD hãng sản xuất.
- Upload logo nếu có.
- Dùng hãng này trong form phụ tùng.
- Không cho xóa hãng đang có sản phẩm nếu backend chặn.

## 6.3. Bán hàng

### Đơn hàng

Test bảng:

- Mã đơn.
- Khách hàng.
- Tổng tiền.
- Trạng thái đơn.
- Trạng thái thanh toán.
- Ngày tạo.
- Action xem chi tiết/cập nhật/hủy.

Test chi tiết đơn:

- Thông tin khách.
- Danh sách sản phẩm.
- Tổng tiền, giảm giá, đã thanh toán, còn lại.
- Timeline lịch sử đơn hàng.
- Cập nhật trạng thái: chờ xác nhận, đang giao, đã giao, đã hủy.
- Xác nhận thanh toán thủ công nếu có.
- Nút quay lại ở bên trái.

Kịch bản:

- Tạo đơn POS rồi xem trong danh sách đơn.
- Cập nhật từ chờ xác nhận -> đang giao -> đã giao.
- Hủy đơn khi chưa giao.
- Không cho hủy đơn đã giao nếu rule nghiệp vụ chặn.
- Timeline ghi nhận từng lần cập nhật.

### Bán tại quầy POS

Test:

- Tìm bằng SKU.
- Tìm bằng tên sản phẩm.
- Tìm bằng barcode.
- Enter thêm nhanh sản phẩm đầu tiên.
- Bấm nút thêm trên dòng kết quả.
- Thêm 2 sản phẩm khác nhau.
- Tăng/giảm số lượng.
- Sửa đơn giá nếu cho phép.
- Xóa 1 sản phẩm không làm mất toàn bộ giỏ.
- Chọn khách lẻ.
- Nhập khách có tên/số điện thoại.
- Chọn bán đứt.
- Chọn đặt cọc.
- Nhập tiền cọc.
- Chọn phương thức thanh toán.
- Áp voucher.
- Tạo đơn.

Kịch bản lỗi:

- Giỏ trống mà bấm tạo đơn.
- Số lượng 0 hoặc âm.
- Tiền cọc <= 0.
- Tiền cọc >= tổng tiền.
- Voucher sai/hết hạn/không áp dụng.
- Sản phẩm hết hàng.

### Voucher

Test:

- Thêm/sửa/xóa/ẩn voucher.
- Loại giảm tiền.
- Loại giảm phần trăm.
- Giới hạn ngày bắt đầu/kết thúc.
- Giới hạn lượt dùng.
- Đơn tối thiểu.
- Phạm vi áp dụng: toàn bộ, danh mục, sản phẩm, hãng xe/hãng sản xuất.
- Chọn phạm vi bằng checkbox/select rõ ràng, không nhập tay ID.

Kịch bản:

- Voucher giảm trên giá bán hiện tại sau khuyến mãi.
- Sản phẩm không có giá khuyến mãi thì dùng giá gốc.
- Voucher hết hạn không áp dụng.
- Voucher vượt giá trị đơn không làm âm đơn.

### Khách hàng

Test:

- Bảng khách hàng.
- Thêm/sửa khách hàng.
- Số điện thoại.
- Email.
- Địa chỉ.
- Trạng thái.
- Ghi chú chăm sóc.
- Lịch sử mua hàng nếu có.

Kịch bản:

- Tạo đơn POS với số điện thoại mới.
- Kiểm tra khách xuất hiện ở trang Khách hàng.
- Cập nhật ghi chú CSKH.
- Tìm theo tên/số điện thoại.

## 6.4. Kho và cung ứng

### Tồn kho

Test bảng:

- Mã SKU.
- Tên sản phẩm.
- Tên biến thể.
- Tồn thực tế.
- Đang giữ chỗ.
- Tồn khả dụng.
- Ngưỡng thấp.
- Trạng thái tồn: còn hàng, sắp hết, hết hàng.
- Ngày cập nhật.

Test chức năng:

- Search theo SKU/tên sản phẩm.
- Filter trạng thái tồn.
- Chỉ sản phẩm đang giữ chỗ.
- Cập nhật ngưỡng thấp nếu có.
- Điều chỉnh tồn nếu có.
- Xem lịch sử điều chỉnh tồn.
- Xuất Excel/CSV.

Kịch bản:

- Nhập kho một SKU rồi tồn tăng.
- Bán POS một SKU rồi tồn giảm.
- Trả hàng Resellable rồi tồn tăng.
- Trả hàng Damaged không tăng tồn bán được.

### Chứng từ kho

Test:

- Tạo phiếu nhập.
- Tạo phiếu xuất.
- Tạo phiếu điều chỉnh.
- Thêm nhiều dòng sản phẩm.
- Xóa một dòng không mất toàn phiếu.
- Lưu nháp nếu có.
- Duyệt phiếu.
- Hủy phiếu.
- Xem chi tiết.
- Kiểm tra tồn kho thay đổi sau duyệt.

Kịch bản lỗi:

- Phiếu không có dòng.
- Số lượng âm/0.
- SKU không tồn tại.
- Duyệt phiếu xuất quá tồn.
- Duyệt lại phiếu đã duyệt.

### Cung ứng và mua hàng

Test:

- Nhà cung cấp.
- Tạo đơn mua hàng.
- Thêm dòng sản phẩm mua.
- Duyệt đơn mua.
- Nhận hàng.
- Ghi nhận thanh toán nhà cung cấp.
- Hủy đơn mua.
- Kiểm tra phiếu nhập/tồn kho/công nợ nhà cung cấp sau nhận hàng.

Kịch bản:

- Tạo mua 10 chai nhớt từ Motul.
- Duyệt đơn mua.
- Nhận đủ hàng.
- Kiểm tra tồn kho tăng 10.
- Ghi nhận chi tiền.
- Kiểm tra tài chính có khoản chi.

## 6.5. Hậu mãi và dịch vụ

### Đổi trả và hoàn tiền

Test:

- Tạo phiếu trả hàng từ đơn đã giao.
- Chọn sản phẩm trả.
- Nhập số lượng trả.
- Chọn tình trạng: bán lại, hư hỏng, bảo hành.
- Sửa phiếu trả hàng khi còn Draft.
- Duyệt phiếu trả hàng.
- Từ chối phiếu trả hàng.
- Kiểm tra hoàn tiền.
- Kiểm tra tồn kho sau trả hàng.
- Kiểm tra không cho sửa phiếu đã duyệt/từ chối.

Kịch bản:

- Trả 1 lốp Michelin còn bán lại -> tồn tăng.
- Trả 1 sản phẩm hư hỏng -> không tăng tồn bán được.
- Hoàn tiền bằng tiền mặt.
- Công nợ/doanh thu điều chỉnh đúng.

### Bảo hành

Test:

- Tạo phiếu bảo hành.
- Tìm đơn/sản phẩm.
- Chọn khách hàng.
- Nhập lỗi bảo hành.
- Cập nhật trạng thái tiếp nhận, đang xử lý, hoàn tất, từ chối.
- Lịch sử xử lý bảo hành.
- Ghi chú kỹ thuật.

Kịch bản:

- Xe đã bán phát sinh bảo hành.
- Phụ tùng đã bán phát sinh bảo hành.
- Không cho bảo hành sản phẩm không thuộc đơn đã giao nếu có rule.

### Dịch vụ và CSKH

Test:

- Tạo lịch hẹn chăm sóc.
- Tạo yêu cầu dịch vụ/sửa chữa.
- Gán khách hàng.
- Gán nhân viên nếu có.
- Cập nhật trạng thái.
- Hoàn tất/hủy lịch.
- Kiểm tra lịch sử tương tác khách hàng.

Kịch bản:

- Khách mua xe được hẹn bảo dưỡng sau 30 ngày.
- Khách gọi hỏi tình trạng bảo hành.
- Nhân viên ghi chú cuộc gọi.

## 6.6. Tài chính, hệ thống và báo cáo

### Tài chính

Test:

- Thu tiền bán hàng.
- Chi tiền nhập hàng.
- Hoàn tiền trả hàng.
- Công nợ khách hàng.
- Công nợ nhà cung cấp nếu có.
- Lọc theo ngày.
- Lọc theo loại giao dịch.
- Tổng thu, tổng chi, tồn quỹ.
- Xuất Excel.

Kịch bản:

- Tạo đơn POS tiền mặt -> phát sinh thu.
- Tạo đơn mua hàng, thanh toán nhà cung cấp -> phát sinh chi.
- Duyệt trả hàng hoàn tiền -> phát sinh chi hoàn tiền.
- Báo cáo tài chính khớp với các giao dịch.

### Báo cáo

Test:

- Báo cáo doanh thu.
- Báo cáo đơn hàng.
- Báo cáo kho.
- Báo cáo sản phẩm bán chạy.
- Báo cáo dịch vụ/bảo hành nếu có.
- Date range.
- Export Excel thật `.xlsx`.
- Kiểm tra số liệu tổng và bảng chi tiết.

Kịch bản:

- Chọn hôm nay.
- Chọn 7 ngày gần nhất.
- Chọn tháng hiện tại.
- Kiểm tra đơn đã hủy không tính doanh thu.
- Kiểm tra hoàn tiền làm giảm doanh thu/tiền thực nhận nếu nghiệp vụ định nghĩa như vậy.

### Tài khoản hệ thống

Test với Admin:

- Xem danh sách Admin/Staff.
- Tạo Staff.
- Sửa Staff.
- Khóa/mở Staff.
- Không xóa hoặc khóa Admin duy nhất.
- Staff không thấy các trang Admin-only.

Test với Staff:

- Đăng nhập Staff.
- Không truy cập được Tài chính Admin, Tài khoản, Nhật ký, Import nếu bị giới hạn.
- Vẫn thao tác được trang bán hàng/kho/dịch vụ theo quyền.

### Nhật ký hệ thống

Test:

- Sau tạo/sửa/xóa/duyệt/hủy nghiệp vụ, nhật ký có dòng tương ứng.
- Có actor.
- Có thời gian.
- Có entity/action.
- Lọc theo loại thao tác nếu có.

### Cấu hình vận hành

Test:

- Xem cấu hình.
- Sửa cấu hình.
- Lưu.
- Reload vẫn còn.
- Sai định dạng bị chặn.
- Cấu hình ảnh hưởng đúng trang liên quan nếu có.

### Import dữ liệu

Test:

- Tải file mẫu.
- Import sản phẩm hợp lệ.
- Import khách hàng hợp lệ.
- Import tồn kho hợp lệ.
- Import file sai cột.
- Import dữ liệu trùng mã.
- Import dữ liệu sai kiểu.
- Kiểm tra kết quả import có báo số dòng thành công/thất bại.
- Kiểm tra dữ liệu đã import xuất hiện ở trang tương ứng.

## 7. Luồng test nghiệp vụ liên hoàn

### Luồng 1: Nhập phụ tùng rồi bán tại quầy

1. Tạo hãng sản xuất phụ tùng.
2. Tạo danh mục phụ tùng.
3. Tạo phụ tùng mới.
4. Tạo SKU/barcode.
5. Tạo phiếu nhập kho.
6. Duyệt phiếu nhập.
7. Kiểm tra tồn kho tăng.
8. Vào POS tìm SKU.
9. Bán 2 sản phẩm.
10. Kiểm tra đơn hàng được tạo.
11. Kiểm tra tồn kho giảm.
12. Kiểm tra tài chính phát sinh thu.
13. Kiểm tra báo cáo doanh thu tăng.

### Luồng 2: Bán xe máy có voucher

1. Tạo xe máy có giá gốc và giá khuyến mãi.
2. Tạo voucher áp dụng cho sản phẩm hoặc danh mục xe máy.
3. Nhập kho xe.
4. Vào POS bán xe.
5. Áp voucher.
6. Kiểm tra voucher giảm trên giá bán hiện tại sau giá khuyến mãi.
7. Tạo đơn.
8. Kiểm tra báo cáo doanh thu.
9. Kiểm tra tồn kho.

### Luồng 3: Đơn hàng giao xong rồi trả hàng

1. Tạo đơn hàng.
2. Cập nhật đơn đến trạng thái đã giao.
3. Tạo phiếu trả hàng.
4. Sửa phiếu trả khi còn Draft.
5. Duyệt phiếu trả hàng.
6. Kiểm tra hoàn tiền.
7. Kiểm tra tồn kho tăng nếu hàng bán lại.
8. Kiểm tra tài chính phát sinh chi hoàn tiền.
9. Kiểm tra báo cáo doanh thu/tiền thực nhận.
10. Thử sửa lại phiếu đã duyệt, hệ thống phải chặn.

### Luồng 4: Mua hàng từ nhà cung cấp

1. Tạo nhà cung cấp.
2. Tạo đơn mua hàng.
3. Duyệt đơn mua.
4. Nhận hàng.
5. Kiểm tra tồn kho tăng.
6. Thanh toán cho nhà cung cấp.
7. Kiểm tra tài chính phát sinh chi.
8. Kiểm tra báo cáo kho.

### Luồng 5: Bảo hành và CSKH

1. Tạo đơn bán xe/phụ tùng.
2. Cập nhật đã giao.
3. Tạo phiếu bảo hành.
4. Cập nhật trạng thái bảo hành.
5. Tạo lịch CSKH.
6. Hoàn tất lịch CSKH.
7. Kiểm tra lịch sử khách hàng.

## 8. Test giao diện

Kiểm tra trên mọi trang:

- Sidebar active đúng.
- Sidebar collapse/hover không che nội dung sai.
- Footer không phình to ở trang ngắn.
- Trang dài cuộn đúng.
- Modal không bị tràn.
- Button không bị mất chữ.
- Icon đúng nghĩa.
- Không có text mojibake như `Ä`, `áº`, `Ã`.
- Không có trạng thái tiếng Anh nếu UI đang dùng tiếng Việt.
- Không có bảng lệch cột.
- Không có text dài tràn khỏi ô.
- Logo/ảnh hiển thị đúng kích thước.
- Loading/empty/error state rõ ràng.

## 9. Test API và database sau UI

Sau các luồng chính, kiểm tra DB/API:

- Sản phẩm tạo mới có đúng Product/SKU/Image.
- Tồn kho khớp sau nhập/bán/trả.
- Đơn hàng có đúng dòng sản phẩm, tổng tiền, thanh toán.
- Phiếu trả hàng có đúng trạng thái.
- Refund/CashTransaction phát sinh đúng.
- Audit log có đủ thao tác.
- Báo cáo lấy số liệu từ dữ liệu thật, không hardcode.

## 10. Test quyền

### Admin

- Thấy toàn bộ trang lõi.
- Thao tác được các trang Admin-only.
- Tạo/sửa Staff.
- Xem nhật ký.
- Import dữ liệu.
- Xem tài chính.

### Staff

- Không thấy hoặc không truy cập được trang Admin-only.
- Vẫn bán hàng POS được nếu nghiệp vụ cho phép.
- Vẫn xử lý đơn/kho/dịch vụ nếu được phân quyền.
- Truy cập URL trực tiếp vào trang Admin-only phải bị chặn.

## 11. Test xuất file

Với từng trang có xuất file:

- Xuất Excel `.xlsx`.
- Mở bằng Excel.
- Tiếng Việt không lỗi font.
- Header cột rõ nghĩa.
- Số tiền là number/currency.
- Ngày là date hoặc text đúng định dạng.
- Dữ liệu khớp bảng UI.
- File có tên rõ: ví dụ `bao-cao-doanh-thu-2026-06-03.xlsx`.

## 12. Mẫu ghi nhận lỗi

| ID | Trang | Modal/Nút/Field | Dữ liệu test | Kết quả mong muốn | Kết quả thực tế | Mức độ | Ảnh | Trạng thái |
|---|---|---|---|---|---|---|---|---|
| BUG-001 | POS | Xóa dòng giỏ | 2 sản phẩm | Xóa 1 còn 1 | Xóa hết giỏ | High | screenshot | Fixed |

Mức độ:

- Critical: Không đăng nhập được, mất dữ liệu, sai tiền/tồn kho nghiêm trọng.
- High: Nghiệp vụ chính lỗi, không thể hoàn thành luồng.
- Medium: Một phần thao tác lỗi nhưng có workaround.
- Low: Lỗi hiển thị nhỏ, typo, spacing.

## 13. Checklist hoàn tất

- [ ] Đăng nhập Admin thành công.
- [ ] Đăng nhập Staff và kiểm tra quyền.
- [ ] Test Dashboard.
- [ ] Test Kinh doanh và sản phẩm.
- [ ] Test Bán hàng.
- [ ] Test Kho và cung ứng.
- [ ] Test Hậu mãi và dịch vụ.
- [ ] Test Tài chính, hệ thống và báo cáo.
- [ ] Test toàn bộ modal.
- [ ] Test toàn bộ field.
- [ ] Test toàn bộ nút.
- [ ] Test dữ liệu lỗi.
- [ ] Test dữ liệu dài.
- [ ] Test reload và quay lại trang.
- [ ] Test responsive.
- [ ] Test export Excel.
- [ ] Test database/API sau luồng nghiệp vụ.
- [ ] Chạy `npm run build`.
- [ ] Chạy `dotnet build`.
- [ ] Chạy `dotnet test`.
- [ ] Lập báo cáo lỗi cuối cùng.
- [ ] Xác nhận hệ thống đạt yêu cầu demo BTL.

## 14. Tiêu chí đạt BTL

Hệ thống được xem là đạt để demo BTL khi:

- Menu lõi gọn đúng 5 nhóm nghiệp vụ.
- Các luồng chính chạy được từ đầu đến cuối.
- Dữ liệu tiền, tồn kho, đơn hàng, trả hàng, báo cáo không sai logic.
- Admin/Staff có phân quyền cơ bản.
- Không còn lỗi giao diện nghiêm trọng.
- Không còn text lỗi font ở các trang lõi.
- Build frontend/backend pass.
- Unit test backend pass.
- Có dữ liệu demo đủ phong phú để thuyết trình.
