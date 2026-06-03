# V2 Admin UI - Backend Full Coverage Test Plan

## 1. Mục tiêu

Kiểm thử toàn bộ Frontend Admin v2 trên UI thật và đối chiếu với backend v2 đã xây dựng.

Plan phải trả lời đủ các câu hỏi:

- Mọi trang admin có tải đúng dữ liệu từ backend không?
- Mọi nút đang hiển thị có bấm được và tạo đúng kết quả không?
- Mọi ô trong bảng có hiển thị đúng giá trị, đúng cột, đúng định dạng và đúng căn lề không?
- Mọi form có gửi đúng payload, validate hợp lý và lưu đúng DB không?
- Reload trang hoặc chuyển trang rồi quay lại có làm mất dữ liệu không?
- Backend có endpoint nào cần cho vận hành nhưng chưa có nút hoặc màn hình tương ứng trên admin không?
- Frontend có nút hoặc service nào gọi endpoint không tồn tại trên backend không?
- Phân quyền `Admin`, `Staff`, `Customer` có đúng ở cả UI và API không?

## 2. Rule bắt buộc

- [ ] Không được chỉ đọc code hoặc chỉ chạy build; phải thao tác trên UI thật.
- [ ] Không được tự ý dừng khi chưa hoàn thành toàn bộ checklist hoặc ghi rõ `Blocked`.
- [ ] Mọi nút nhìn thấy trên trang, bảng, modal, dropdown và pagination phải được bấm ít nhất một lần.
- [ ] Mọi field nhập liệu phải test: hợp lệ, rỗng, thiếu bắt buộc, sai định dạng, giá trị biên và chuỗi dài.
- [ ] Mọi bảng phải được chụp screenshot sau khi tải dữ liệu.
- [ ] Với mỗi screenshot bảng, phải đối chiếu từng cột với response API và DB, không chỉ kiểm tra bố cục.
- [ ] Mỗi thao tác ghi dữ liệu phải kiểm tra đủ bốn lớp: UI sau thao tác, network response, UI sau reload, DB hoặc audit log.
- [ ] Mỗi trang phải test: tải lần đầu, reload, chuyển sang trang khác rồi quay lại, dữ liệu rỗng, dữ liệu dài và lỗi API.
- [ ] Mọi modal phải test: mở, nút đóng `x`, nút hủy, click thao tác chính, lỗi validation, submit thành công và mở lại dữ liệu vừa lưu.
- [ ] Mọi filter phải thử độc lập và kết hợp; phân trang phải thử trang đầu, trang giữa, trang cuối, nút trước và nút sau.
- [ ] Giá tiền phải đối chiếu số gốc API/DB; ngày giờ phải kiểm tra múi giờ `Asia/Ho_Chi_Minh`.
- [ ] Badge trạng thái phải đối chiếu cả mã backend và nhãn tiếng Việt trên UI.
- [ ] Sau mỗi nhóm trang phải chạy `npm run build`.
- [ ] Khi test backend phải dùng Swagger/network log và truy vấn SQL Server khi cần.
- [ ] Không sửa dữ liệu production; chỉ dùng DB test hoặc bản backup.

## 3. Quy ước trạng thái task

Mỗi task chỉ dùng một trạng thái:

- `Pending`: chưa test.
- `In Progress`: đang test.
- `Done`: đã test đủ evidence và pass.
- `Failed`: đã xác nhận lỗi, cần sửa rồi test lại.
- `Blocked`: không thể tiếp tục; phải ghi nguyên nhân, file hoặc API liên quan và hướng unblock.

## 4. Evidence bắt buộc cho mỗi lỗi

Ghi vào report:

| Trường | Nội dung bắt buộc |
|---|---|
| Mã lỗi | Ví dụ `V2-ADMIN-ORD-001` |
| Trang | Route UI |
| Bước tái hiện | Các thao tác đã bấm |
| Dữ liệu nhập | Payload hoặc giá trị field |
| Expected | Kết quả đúng |
| Actual | Kết quả thực tế |
| Screenshot | Đường dẫn file ảnh |
| Network | Method, URL, status code, response body |
| DB | Query và kết quả đối chiếu nếu có ghi dữ liệu |
| Nghiệp vụ | Ảnh hưởng vận hành |
| Trạng thái | `Failed`, `Fixed`, `Retested` |

## 5. Chuẩn bị môi trường

### 5.1 Host và tài khoản

- [ ] `Pending` Chạy Gateway tại `http://localhost:5100`.
- [ ] `Pending` Chạy AuthService tại `http://localhost:5101`.
- [ ] `Pending` Chạy APIService tại `http://localhost:5102`.
- [ ] `Pending` Chạy Frontend Admin tại `http://127.0.0.1:5175`.
- [ ] `Pending` Restart APIService một lần để nạp seed mới.
- [ ] `Pending` Đăng nhập admin: `admin@motosale.local` / `Admin@123`.
- [ ] `Pending` Đăng nhập staff: `staff@motosale.local` / `Staff@123`.
- [ ] `Pending` Xác nhận seed có đúng `10` user, trong đó chỉ có `1` admin và `1` staff.

### 5.2 Baseline kỹ thuật

- [ ] `Pending` Chạy `dotnet build` backend v2.
- [ ] `Pending` Chạy `npm run build` trong `v2/frontend-admin`.
- [ ] `Pending` Mở Swagger APIService và AuthService.
- [ ] `Pending` Lưu danh sách endpoint Swagger làm baseline.
- [ ] `Pending` Mở DevTools Network và Console.
- [ ] `Pending` Chụp screenshot desktop `1440x900`, tablet `1024x768`, mobile `390x844`.

## 6. Checklist dùng chung cho mọi bảng

Áp dụng cho tất cả bảng trong các phần bên dưới.

- [ ] `Pending` Header đúng tên nghiệp vụ, không thiếu hoặc thừa cột.
- [ ] `Pending` Mỗi giá trị nằm đúng cột; không lệch khi có dữ liệu dài hoặc ảnh.
- [ ] `Pending` Cột văn bản căn trái.
- [ ] `Pending` Cột số lượng, ID ngắn và trạng thái căn giữa khi phù hợp.
- [ ] `Pending` Cột tiền căn phải và định dạng `vi-VN`.
- [ ] `Pending` Cột ngày giờ nhất quán, không sai timezone.
- [ ] `Pending` Badge trạng thái đúng nhãn tiếng Việt, màu và mã backend.
- [ ] `Pending` Không hiển thị `undefined`, `[object Object]`, `NaN`, mã enum thô hoặc dấu `-` khi API có dữ liệu.
- [ ] `Pending` Text dài không phá layout; có wrap, ellipsis hoặc xem chi tiết hợp lý.
- [ ] `Pending` Nút thao tác có icon, tooltip và trạng thái disabled đúng nghiệp vụ.
- [ ] `Pending` Empty state đúng khi API trả mảng rỗng.
- [ ] `Pending` Loading state và error state hiển thị đúng.
- [ ] `Pending` Pagination không đổi sai dữ liệu khi chuyển trang.
- [ ] `Pending` Screenshot bảng được đối chiếu với API response và ít nhất một query DB.

## 7. Checklist layout dùng chung

- [ ] `Pending` Sidebar hamburger mở/đóng đúng; content dịch chuyển hợp lý.
- [ ] `Pending` Sidebar hover không che content sai như layout cũ.
- [ ] `Pending` Active menu đúng route.
- [ ] `Pending` Footer không phình cao ở trang ngắn và không che nội dung ở trang dài.
- [ ] `Pending` Modal không tràn màn hình; phần footer modal luôn truy cập được.
- [ ] `Pending` Bảng dài cuộn ngang trong vùng bảng, không làm vỡ toàn trang.
- [ ] `Pending` Reload và điều hướng qua lại không làm mất CSS, logo hoặc ảnh.
- [ ] `Pending` Responsive desktop, tablet, mobile không overlap.

## 8. Ma trận kiểm thử theo trang

### 8.1 Đăng nhập và phân quyền

Route: `/login`

- [ ] `Pending` Test đăng nhập admin đúng mật khẩu.
- [ ] `Pending` Test đăng nhập staff đúng mật khẩu.
- [ ] `Pending` Test email sai, password sai, bỏ trống, khoảng trắng và chuỗi dài.
- [ ] `Pending` Kiểm tra thông báo lỗi có dấu tiếng Việt.
- [ ] `Pending` Kiểm tra token lưu và gọi `/api/users/me`.
- [ ] `Pending` Logout rồi truy cập route bảo vệ.
- [ ] `Pending` Staff không thấy `/users` và `/audit-logs`.
- [ ] `Pending` Staff gọi trực tiếp API chỉ-admin phải nhận `403`.
- [ ] `Pending` Customer gọi API admin phải nhận `403`.
- [ ] `Pending` Không token phải nhận `401`.

### 8.2 Tổng quan

Route: `/`

- [ ] `Pending` Đối chiếu mọi thẻ thống kê với API nguồn và DB.
- [ ] `Pending` Đối chiếu doanh thu chỉ tính đúng đơn đã thanh toán và đã giao/hoàn tất.
- [ ] `Pending` Đối chiếu đơn chưa thanh toán, đơn cần xử lý và cảnh báo tồn kho.
- [ ] `Pending` Chụp screenshot biểu đồ doanh thu, trạng thái đơn, top sản phẩm và đơn gần đây.
- [ ] `Pending` Bấm link từ từng stat card và xác minh route đích.
- [ ] `Pending` Kiểm tra empty data và lỗi từng API con.
- [ ] `Pending` Kiểm tra không còn dòng thừa hoặc breadcrumb lỗi.

### 8.3 Xe máy

Route: `/motorcycles`

API chính: `/api/products?kind=1`

Bảng phải kiểm tra: mã xe, tên xe, danh mục, hãng xe, giá gốc, giá khuyến mại, tồn kho, trạng thái, thao tác.

- [ ] `Pending` Đối chiếu toàn bộ dòng chỉ có `kind=Motorcycle`.
- [ ] `Pending` Test tìm kiếm, danh mục, hãng xe, trạng thái và pagination.
- [ ] `Pending` Bấm `Thêm xe máy`, `Sửa`, `Biến thể`, `Ảnh`, `Xóa` nếu đang hiển thị.
- [ ] `Pending` Form xe máy: mã, tên, slug, danh mục xe, hãng, dòng xe, trạng thái, nổi bật, hot deal, mô tả, giá và ảnh file.
- [ ] `Pending` Dòng xe lọc đúng theo hãng đã chọn.
- [ ] `Pending` Reload sau upload ảnh; đổi trang rồi quay lại; ảnh vẫn tồn tại.
- [ ] `Pending` Đối chiếu tồn kho hiển thị với SKU và bảng inventory.
- [ ] `Pending` Xác minh nghiệp vụ xóa sản phẩm: hard delete, soft delete hay không cho xóa khi đã phát sinh dữ liệu.

### 8.4 Phụ tùng và tương thích xe

Route: `/parts`

API chính: `/api/products?kind=2`

- [ ] `Pending` Đối chiếu toàn bộ dòng chỉ có `kind=Part`.
- [ ] `Pending` Xác minh `Dầu nhớt` nằm dưới danh mục cha `Phụ tùng`.
- [ ] `Pending` Test CRUD phụ tùng và mọi field form.
- [ ] `Pending` Bấm `Tương thích xe`.
- [ ] `Pending` Test phạm vi: tất cả xe, theo hãng, theo dòng xe.
- [ ] `Pending` Test chọn hãng, dòng xe, từ năm, đến năm, trạng thái và ghi chú.
- [ ] `Pending` Test thêm, sửa, xóa cấu hình tương thích.
- [ ] `Pending` Đối chiếu bảng tương thích với `/api/products/{id}/compatibilities`.
- [ ] `Pending` Test `Từ năm <= Đến năm`, giá trị rỗng và giá trị biên.

### 8.5 Biến thể SKU

Modal từ `/motorcycles` và `/parts`

- [ ] `Pending` Đối chiếu bảng: tên biến thể, SKU, phiên bản, màu sắc, giá niêm yết, giá khuyến mại, trạng thái, thao tác.
- [ ] `Pending` Test thêm, sửa, xóa SKU.
- [ ] `Pending` Test SKU trùng, thiếu tên, giá âm, giá khuyến mại lớn hơn giá gốc, chuỗi dài.
- [ ] `Pending` Xác minh SKU không chỉnh tồn kho trực tiếp; tồn kho thuộc nghiệp vụ inventory.
- [ ] `Pending` Xác minh typography của form đồng đều.

### 8.6 Ảnh sản phẩm và logo

Modal ảnh sản phẩm, modal hãng xe và banner/bài viết.

- [ ] `Pending` Test upload file ảnh hợp lệ.
- [ ] `Pending` Test file không phải ảnh, file quá lớn, nhiều file và tên file dài.
- [ ] `Pending` Test gắn ảnh chung và gắn ảnh theo SKU.
- [ ] `Pending` Test đặt ảnh chính và xóa ảnh.
- [ ] `Pending` Reload và điều hướng qua lại; ảnh/logo không mất.
- [ ] `Pending` Kiểm tra ảnh chính trong form sản phẩm nhất quán với ảnh chính trong modal quản lý ảnh.
- [ ] `Pending` Không còn field bắt nhập URL ảnh thủ công nếu nghiệp vụ đã chuẩn hóa dùng file upload.

### 8.7 Danh mục

Route: `/categories`

- [ ] `Pending` Đối chiếu bảng và cây cha-con với `/api/categories`.
- [ ] `Pending` Xác minh hai root `Xe máy`, `Phụ tùng`; các danh mục con xổ đúng nhóm.
- [ ] `Pending` Test thêm, sửa, xóa danh mục.
- [ ] `Pending` Test tên, slug, danh mục cha, loại sản phẩm, thứ tự và trạng thái.
- [ ] `Pending` Không cho tạo vòng lặp cha-con hoặc gắn danh mục xe vào phụ tùng sai loại.
- [ ] `Pending` Xóa danh mục đang được sản phẩm sử dụng phải trả lỗi nghiệp vụ rõ ràng.

### 8.8 Hãng xe và dòng xe

Route: `/brands`

- [ ] `Pending` Tab hãng xe: kiểm tra ID, tên, slug, logo, trạng thái, thao tác.
- [ ] `Pending` Bấm tab `Hãng xe`, tab `Dòng xe`, thêm, sửa, xóa và pagination.
- [ ] `Pending` Test upload logo file; reload và điều hướng qua lại.
- [ ] `Pending` Logo đủ lớn để nhận diện nhưng không phá hàng bảng.
- [ ] `Pending` Tab dòng xe: hãng xe, tên dòng, slug, trạng thái, thao tác.
- [ ] `Pending` Test filter dòng xe theo hãng.
- [ ] `Pending` Test thêm/sửa dòng xe với hãng bắt buộc.

### 8.9 Tồn kho

Route: `/inventory`

- [ ] `Pending` Đối chiếu bảng: kho, SKU, sản phẩm, tồn thực tế, đang giữ, khả dụng, ngưỡng cảnh báo và trạng thái tồn.
- [ ] `Pending` Kiểm tra công thức `khả dụng = tồn thực tế - đang giữ`.
- [ ] `Pending` Test search, filter kho, hết hàng, sắp hết hàng và chỉ giữ chỗ.
- [ ] `Pending` Bấm đồng bộ tồn và đối chiếu ledger.
- [ ] `Pending` Bấm xem giữ chỗ; đối chiếu đơn hàng và thời hạn.
- [ ] `Pending` Bấm lịch sử điều chỉnh.
- [ ] `Pending` Bấm cập nhật ngưỡng cảnh báo.
- [ ] `Pending` Bấm điều chỉnh tồn; test tăng, giảm, âm, bằng `0`, lý do rỗng và lý do dài.
- [ ] `Pending` Bấm export; mở file và kiểm tra tiếng Việt, header, số liệu, trạng thái.
- [ ] `Pending` Kiểm tra ngày cập nhật và lần đồng bộ cuối nếu UI hiển thị.

### 8.10 Phiếu kho

Route: `/stock-documents`

- [ ] `Pending` Đối chiếu bảng: mã phiếu, loại, số dòng, tổng số lượng, trạng thái, ngày tạo, ngày duyệt, ghi chú, thao tác.
- [ ] `Pending` Test filter loại, trạng thái và pagination.
- [ ] `Pending` Bấm tạo phiếu nhập, phiếu xuất và phiếu điều chỉnh.
- [ ] `Pending` Test thêm/xóa dòng hàng, chọn kho, SKU, số lượng và ghi chú.
- [ ] `Pending` Test số lượng rỗng, `0`, âm, chữ, xuất vượt tồn.
- [ ] `Pending` Bấm xem chi tiết, duyệt, hủy và in phiếu.
- [ ] `Pending` Duyệt phiếu phải cập nhật inventory và stock movement đúng một lần.
- [ ] `Pending` Phiếu đã duyệt không được duyệt hoặc hủy lần hai.

### 8.11 Đơn hàng

Routes: `/orders`, `/orders/{id}`

- [ ] `Pending` Đối chiếu bảng: mã đơn, khách hàng, tổng tiền, trạng thái đơn, thanh toán, vận chuyển, ngày tạo, thao tác.
- [ ] `Pending` Không được hiển thị `-` ở khách hàng nếu chi tiết đơn có dữ liệu.
- [ ] `Pending` Test tìm kiếm, trạng thái đơn, thanh toán, vận chuyển và pagination.
- [ ] `Pending` Bấm xuất Excel; mở file và đối chiếu dữ liệu.
- [ ] `Pending` Bấm xem chi tiết và quay lại.
- [ ] `Pending` Đối chiếu thông tin khách, dòng hàng, voucher, thanh toán, giữ chỗ và tổng tiền.
- [ ] `Pending` Test cập nhật trạng thái đơn theo luồng hợp lệ.
- [ ] `Pending` Test cập nhật vận chuyển thủ công.
- [ ] `Pending` Test ghi nhận thanh toán thủ công, đặt cọc, đủ tiền và hoàn tiền nếu hỗ trợ.
- [ ] `Pending` Test hủy đơn với lý do bắt buộc.
- [ ] `Pending` Kiểm tra trạng thái đơn và vận chuyển đồng bộ theo rule nghiệp vụ.
- [ ] `Pending` Kiểm tra timeline có log ngay sau mỗi thay đổi, đúng thứ tự và đúng timezone.
- [ ] `Pending` Bấm in phiếu đơn hàng.
- [ ] `Pending` Xác minh backend allocation: lấy gợi ý kho và phân phối kho cho đơn.

### 8.12 Voucher

Route: `/vouchers`

- [ ] `Pending` Đối chiếu bảng: mã, loại giảm, giá trị, phạm vi, thời hạn, giới hạn dùng, trạng thái, thao tác.
- [ ] `Pending` Test tạo, sửa, xóa voucher.
- [ ] `Pending` Test phần trăm, số tiền cố định, giới hạn giảm, đơn tối thiểu, thời gian và số lượt.
- [ ] `Pending` Test phạm vi toàn bộ, danh mục, sản phẩm cụ thể và hãng xe bằng checkbox.
- [ ] `Pending` Mở lại voucher đã lưu; mọi checkbox và field phải giữ đúng.
- [ ] `Pending` Test backend `/api/vouchers/validate` với đơn hợp lệ và không hợp lệ.

### 8.13 Người dùng

Route: `/users`, chỉ `Admin`

- [ ] `Pending` Đối chiếu bảng: họ tên, email, SĐT, vai trò, trạng thái, ngày tạo, thao tác.
- [ ] `Pending` Xác minh chỉ có một admin vận hành theo seed.
- [ ] `Pending` Test thêm user với role `Customer` và `Staff`.
- [ ] `Pending` Test sửa user, đổi role, khóa/mở khóa và xóa.
- [ ] `Pending` Test email trùng, email sai, SĐT sai, password rỗng và chuỗi dài.
- [ ] `Pending` Không cho staff vào route hoặc gọi API.
- [ ] `Pending` Xác minh có chặn tự xóa admin cuối cùng nếu đây là rule nghiệp vụ được chọn.

### 8.14 Khách hàng

Route: `/customers`

- [ ] `Pending` Đối chiếu bảng: khách hàng, liên hệ, tổng đơn, tổng chi tiêu, đơn hủy, đơn gần nhất, ghi chú chăm sóc, thao tác.
- [ ] `Pending` Đối chiếu tổng đơn và chi tiêu với bảng orders.
- [ ] `Pending` Bấm ghi chú chăm sóc; test rỗng và chuỗi dài.
- [ ] `Pending` Reload; ghi chú vẫn còn.
- [ ] `Pending` Bấm xuất Excel; đối chiếu file.

### 8.15 Bảo hành

Route: `/warranties`

- [ ] `Pending` Đối chiếu bảng: mã phiếu, khách hàng, sản phẩm/SKU, serial, thời hạn, trạng thái, thao tác.
- [ ] `Pending` Test tạo phiếu bảo hành từ đơn đã giao.
- [ ] `Pending` Test cập nhật trạng thái theo luồng.
- [ ] `Pending` Test serial trùng, thiếu dòng đơn, thời hạn sai và ghi chú dài.
- [ ] `Pending` Bấm xem chi tiết và in phiếu.

### 8.16 Đánh giá

Route: `/reviews`

- [ ] `Pending` Đối chiếu bảng: khách hàng, sản phẩm, số sao, nội dung, trạng thái, ngày tạo, thao tác.
- [ ] `Pending` Test duyệt, ẩn và xóa.
- [ ] `Pending` Reload; trạng thái giữ đúng.
- [ ] `Pending` Staff không được hard delete nếu backend chỉ cho admin.

### 8.17 Bài viết

Route: `/posts`

- [ ] `Pending` Đối chiếu bảng và trạng thái bài viết.
- [ ] `Pending` Test thêm, sửa, xóa.
- [ ] `Pending` Test tiêu đề, slug, tóm tắt, nội dung dài, danh mục, trạng thái và ảnh file.
- [ ] `Pending` Modal không tràn; nút hủy và cập nhật luôn truy cập được.
- [ ] `Pending` Staff không được hard delete nếu backend chỉ cho admin.

### 8.18 FAQ

Route: `/faq`

- [ ] `Pending` Đối chiếu bảng câu hỏi, câu trả lời, danh mục, thứ tự và trạng thái.
- [ ] `Pending` Test thêm, sửa, xóa và mọi field.
- [ ] `Pending` FAQ đang hoạt động phải hiển thị badge hoạt động, không được ghi `Ẩn`.
- [ ] `Pending` Staff không được hard delete nếu backend chỉ cho admin.

### 8.19 Liên hệ

Route: `/contacts`

- [ ] `Pending` Đối chiếu bảng: họ tên, SĐT, email, loại yêu cầu, trạng thái, ngày tạo, thao tác.
- [ ] `Pending` Test filter trạng thái và pagination.
- [ ] `Pending` Bấm xem chi tiết, đóng modal và đánh dấu đã xử lý.
- [ ] `Pending` Reload; trạng thái vẫn đúng.

### 8.20 Banner trang chủ

Route: `/home-banners`

- [ ] `Pending` Đối chiếu bảng: ảnh, vị trí, tiêu đề, liên kết, thứ tự, trạng thái, thao tác.
- [ ] `Pending` Test thêm, sửa, xóa banner.
- [ ] `Pending` Test upload file, preview, liên kết, thứ tự và toggle hoạt động.
- [ ] `Pending` Reload; ảnh banner không mất.

### 8.21 Báo cáo và thống kê

Route: `/reports`

- [ ] `Pending` Test từ ngày, đến ngày, khoảng ngày rỗng và khoảng ngày ngược.
- [ ] `Pending` Đối chiếu tổng doanh thu, tổng đơn, số đơn có doanh thu và giá trị đơn trung bình với DB.
- [ ] `Pending` Đối chiếu biểu đồ doanh thu theo ngày.
- [ ] `Pending` Đối chiếu biểu đồ nhóm trạng thái đơn.
- [ ] `Pending` Đối chiếu top sản phẩm và bảng chi tiết.
- [ ] `Pending` Bấm xuất XLSX thật; mở từng sheet và kiểm tra tiếng Việt, header, số liệu, định dạng số.
- [ ] `Pending` Xác minh báo cáo hiện tổng hợp ở client; đánh giá có cần endpoint báo cáo backend riêng để tránh sai lệch và tải quá nhiều dữ liệu.

### 8.22 Nhật ký hệ thống

Route: `/audit-logs`, chỉ `Admin`

- [ ] `Pending` Đối chiếu bảng: mã, thời gian, đối tượng, mã đối tượng, hành động, người thực hiện, ghi chú.
- [ ] `Pending` Test filter đối tượng, hành động, người thực hiện, từ khóa, từ ngày, đến ngày.
- [ ] `Pending` Bấm lọc, đặt lại, trang trước và trang sau.
- [ ] `Pending` Tạo dữ liệu ở từng module quan trọng và kiểm tra audit log phát sinh.
- [ ] `Pending` Staff truy cập phải nhận `403`.

### 8.23 Cấu hình vận hành

Route: `/settings`

- [ ] `Pending` Đối chiếu danh sách kho/showroom và settings với backend.
- [ ] `Pending` Test thêm kho/showroom.
- [ ] `Pending` Test tên, mã, loại kho, địa chỉ, hotline và trạng thái.
- [ ] `Pending` Test lưu settings và mở lại.
- [ ] `Pending` Staff chỉ xem; gọi lưu kho hoặc settings phải nhận `403` nếu backend chỉ-admin.

## 9. Ma trận gap FE - BE phải xác minh

Các mục dưới đây là ứng viên gap phát hiện bằng rà code. Khi thực thi phải kiểm tra UI/network thật và chuyển sang `Done`, `Failed` hoặc `Blocked`.

| Mã | Phát hiện tĩnh | Phải kiểm tra |
|---|---|---|
| `GAP-001` | FE có nút và service `DELETE /api/products/{id}`, BE v2 chưa có endpoint xóa sản phẩm | Xác định thêm soft delete/hard delete hoặc bỏ nút |
| `GAP-002` | BE có `GET /api/orders/{id}/allocation-suggestion` và `POST /api/orders/{id}/allocate`, FE chưa thấy lời gọi | Bổ sung UI phân phối kho nếu nghiệp vụ cần |
| `GAP-003` | BE có `POST /api/payments` để ghi nhận thanh toán thủ công; FE chi tiết đơn đang cập nhật payment qua `PUT /api/orders/{id}/status` | Chốt một luồng nghiệp vụ và kiểm tra ledger thanh toán |
| `GAP-004` | `PaymentList.jsx` tồn tại nhưng không có route/menu; service còn gọi `GET /payments/{id}` và `PATCH /payments/{id}/confirm` không tồn tại ở BE | Quyết định bỏ component cũ hoặc nối lại theo contract mới |
| `GAP-005` | Báo cáo tổng hợp ở client từ products/orders/payments/users, chưa có endpoint báo cáo backend chuyên dụng | Kiểm tra độ đúng, hiệu năng và phân quyền |
| `GAP-006` | FE service còn khai báo `GET /brands/{id}`, `GET /categories/{id}`, `GET /content/faq/{id}`, `GET /content/contacts/{id}`, `GET /reviews/{id}` nhưng BE không có các route này | Xác minh method chết hay UI đang gọi; dọn hoặc bổ sung API |
| `GAP-007` | BE có `GET /api/inventory/movements`, FE dùng `/inventory/adjustments` cho lịch sử | Kiểm tra tên route và mục đích nghiệp vụ có thống nhất |
| `GAP-008` | FE form sản phẩm còn field `Tồn kho ban đầu` trong khi tồn kho đã tách module inventory | Xác minh payload và bỏ field nếu sai nghiệp vụ |
| `GAP-009` | BE cho `GET /api/orders/{id}` và cancel với `[Authorize]` chung | Kiểm tra customer không xem hoặc hủy đơn của người khác |
| `GAP-010` | BE user delete chưa thấy rule chặn xóa admin cuối cùng | Chốt rule một admin duy nhất và bổ sung guard nếu cần |
| `GAP-011` | Một số GET lookup catalog/content đang public | Rà đúng chủ đích public storefront và không lộ dữ liệu quản trị |

## 10. Đối chiếu ngược từ endpoint BE lên UI

### 10.1 Endpoint admin phải có UI hoặc quyết định rõ

- [ ] `Pending` Products CRUD, SKU CRUD, ảnh CRUD, tương thích CRUD.
- [ ] `Pending` Categories CRUD.
- [ ] `Pending` Brands CRUD, upload logo, models CRUD.
- [ ] `Pending` Inventory list, movement, documents, approve, cancel, holds, adjust, threshold, sync, export.
- [ ] `Pending` Orders search, detail, cancel, allocation suggestion, allocate, update status.
- [ ] `Pending` Payments record, list, order payments, cancel.
- [ ] `Pending` Vouchers CRUD và validate.
- [ ] `Pending` Reviews list, status và delete.
- [ ] `Pending` Warranties list, detail, create và status.
- [ ] `Pending` Posts CRUD và upload image.
- [ ] `Pending` FAQ CRUD.
- [ ] `Pending` Contacts list và process.
- [ ] `Pending` Home banners CRUD và upload image.
- [ ] `Pending` Operations warehouses và settings.
- [ ] `Pending` Audit logs list.
- [ ] `Pending` Users CRUD, status, customers và care note.

### 10.2 Endpoint storefront không bắt buộc có UI admin

- [ ] `Pending` Ghi chú rõ lý do loại khỏi admin test: cart, register, profile cá nhân và địa chỉ cá nhân.
- [ ] `Pending` Vẫn test authorization để đảm bảo không ảnh hưởng admin hoặc lộ dữ liệu.

## 11. Kiểm tra DB sau regression

- [ ] `Pending` Users và roles: chỉ một admin seed, một staff seed, customer đúng role.
- [ ] `Pending` Categories: cây cha-con và `ProductKind` đúng.
- [ ] `Pending` Products, SKUs, images và compatibilities không mồ côi FK.
- [ ] `Pending` Inventory item khớp stock movement; available không âm.
- [ ] `Pending` Stock documents duyệt đúng một lần.
- [ ] `Pending` Orders, order lines, timeline, voucher, holds và payments nhất quán.
- [ ] `Pending` Reviews và warranties gắn đúng order/customer/product.
- [ ] `Pending` Nội dung, FAQ, contacts và banners lưu UTF-8 đúng.
- [ ] `Pending` Audit logs có actor, action, entity và thời gian hợp lý.
- [ ] `Pending` Không có dữ liệu test rác ngoài bộ seed hoặc dữ liệu test đã ghi rõ.

## 12. Kết thúc regression

- [ ] `Pending` Chạy `dotnet build` backend v2.
- [ ] `Pending` Chạy `npm run build` frontend admin v2.
- [ ] `Pending` Chạy lại các test từng `Failed` sau khi sửa.
- [ ] `Pending` Tạo report kết quả riêng, liên kết screenshot và query DB.
- [ ] `Pending` Lập danh sách gap còn lại theo mức độ `Critical`, `High`, `Medium`, `Low`.
- [ ] `Pending` Chỉ kết luận sẵn sàng vận hành khi toàn bộ task quan trọng là `Done`, không còn lỗi `Critical` hoặc `High`.
