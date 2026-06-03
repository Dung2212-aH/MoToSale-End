# V2 Admin Frontend Real World Full Test Plan

## Mục tiêu

Kiểm thử toàn bộ Frontend Admin v2 theo góc nhìn vận hành cửa hàng bán xe máy và phụ tùng: dữ liệu hiển thị phải đúng contract Backend, tất cả nút bấm phải hoạt động đúng, giao diện không vỡ, và các tình huống nghiệp vụ thực tế phải có phản hồi rõ ràng.

## Rule bắt buộc

- Không được tự ý dừng khi chưa test hết checklist trong file này.
- Mỗi route phải được mở bằng UI thật tại `http://localhost:5176`, không chỉ đọc code.
- Mỗi bảng phải chụp screenshot và đối chiếu ít nhất 3 dòng dữ liệu với API BE tương ứng.
- Mọi nút nhìn thấy trên UI phải được ấn ít nhất một lần. Nếu nút có tác động dữ liệu, dùng dữ liệu test có prefix `E2E-` và phải cleanup sau test.
- Mọi form phải thử đủ dữ liệu hợp lệ, thiếu field bắt buộc, dữ liệu dài, dữ liệu sai định dạng.
- Mọi modal phải test mở, đóng bằng nút X/Đóng/Hủy, submit hợp lệ, submit lỗi.
- Mọi select/filter/search/pagination phải test bằng thao tác thật và đối chiếu request/query với BE.
- Mọi file upload/export phải kiểm tra file thật: MIME, tên file, dung lượng, font tiếng Việt, dữ liệu cột.
- Sau mỗi nhóm test có mutation phải kiểm tra DB/audit log và cleanup dữ liệu test.
- Sau khi sửa lỗi, phải build lại `dotnet build MoToSale.slnx` và `npm run build`, sau đó test lại đúng case lỗi.

## Chuẩn bị môi trường

- BE v2 chạy qua gateway: `http://localhost:5100`.
- FE v2 chạy tại `http://localhost:5176`.
- Tài khoản admin: `admin@motosale.local / Admin@123`.
- Tài khoản staff: `staff@motosale.local / Staff@123`.
- Tài khoản customer: `customer@motosale.local / Customer@123`.
- Ghi log/screenshot vào `test-artifacts/v2-admin-real-world-full-test-YYYYMMDD/`.

## Ma trận kiểm tra chung cho mọi trang

| Hạng mục | Cách test | Kỳ vọng |
|---|---|---|
| Load lần đầu | Mở route trực tiếp | Không lỗi trắng, không console error, data load đúng |
| Reload | F5/reload route | Vẫn giữ đúng route, token, data |
| Chuyển trang rồi quay lại | Click sidebar sang trang khác rồi quay lại | Layout, filter, bảng không lệch |
| Sidebar | Toggle hamburger, hover, active route | Không che sai content, active đúng route |
| Footer | Trang ngắn và trang dài | Footer không phình, không che nội dung |
| Table | Screenshot header/cột/dòng/action | Cột text trái, số/tiền phải, status giữa, action giữa |
| Responsive | Desktop, tablet, mobile | Không overlap, bảng cuộn ngang khi cần |
| Error state | Tắt BE hoặc dùng token sai khi cần | UI báo lỗi rõ, không crash |

## Đối chiếu dữ liệu FE với BE

Với mỗi trang có bảng:

1. Gọi API BE tương ứng bằng token admin.
2. Lấy ít nhất 3 record đầu.
3. Đối chiếu từng cột FE:
   - ID/mã nghiệp vụ.
   - Tên hiển thị.
   - Trạng thái/badge.
   - Ngày giờ.
   - Số lượng/tồn kho.
   - Giá tiền/doanh thu.
   - Action button có đúng với trạng thái không.
4. Chụp screenshot bảng sau khi đối chiếu.
5. Nếu có format tiền/ngày, kiểm tra format tiếng Việt và timezone nhất quán.

## Login và phân quyền

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Admin login đúng | Nhập admin và submit | Điều hướng `/`, navbar hiện Quản trị viên |
| Sai mật khẩu | Nhập sai password | Báo lỗi có dấu, không lưu token |
| Staff login | Nhập staff | Vào được trang staff được phép, không thấy menu Admin-only |
| Customer login admin FE | Nhập customer nếu route cho phép | Không được truy cập admin hoặc bị chặn role |
| Logout | Mở user menu, bấm đăng xuất | Xóa token, về `/login` |
| Route protected | Mở `/orders` khi logout | Redirect `/login` |

## Dashboard `/`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Stat cards | So sánh sản phẩm, đơn hàng, user, doanh thu với API report/order/product | Số liệu đúng |
| Card links | Bấm từng card `Chi tiết` | Điều hướng đúng route |
| Charts | Kiểm tra doanh thu 7 ngày, order status | Không rỗng nếu API có data |
| Đơn mới nhất | So sánh 5 đơn với `/orders` | Mã, khách, tổng tiền, trạng thái, ngày đúng |
| Top sản phẩm | So sánh từ order lines | Tên, số lượng, doanh thu đúng |
| UI | Screenshot desktop/mobile | Không còn breadcrumb lẻ, không vỡ icon |

## Xe máy `/motorcycles`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Chỉ hiện product kind xe máy |
| Cột bảng | Đối chiếu mã, tên, danh mục, hãng, giá, tồn, trạng thái | Khớp BE |
| Search | Tìm mã/tên xe | Chỉ hiện item phù hợp |
| Filter danh mục/hãng/status | Chọn từng filter | Query đúng, bảng đúng |
| Thêm xe | Bấm thêm, nhập hợp lệ | Tạo product + SKU mặc định, upload ảnh nếu có |
| Validate form | Bỏ tên, bỏ giá, bỏ danh mục | Báo lỗi rõ |
| Sửa xe | Bấm edit, đổi tên/status | PUT đúng, list reload đúng |
| Biến thể | Mở modal biến thể, thêm/sửa/xóa SKU | Giá/SKU/status đúng |
| Ảnh | Upload file, set primary, xóa ảnh | Không nhập URL tay, ảnh persist sau reload |
| Xóa xe | Xóa product test | Xóa mềm, biến mất khỏi list, audit có log |
| Tồn kho | Không có field tồn kho trong form sản phẩm | Quản lý tồn qua trang tồn kho/phiếu kho |

## Phụ tùng `/parts`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Chỉ hiện product kind phụ tùng |
| Cột bảng | Đối chiếu mã, tên, danh mục, nhà sản xuất, giá, tồn, trạng thái | Khớp BE |
| Search/filter/status | Thao tác từng filter | Data đúng |
| Thêm/sửa phụ tùng | Nhập category phụ tùng, manufacturer | Không bắt hãng/dòng xe trực tiếp |
| Tương thích xe | Mở modal, thêm theo hãng/dòng/năm | Lưu được, list hiện rõ phạm vi |
| Tương thích toàn bộ | Chọn áp dụng tất cả nếu có | Form dễ hiểu, không fix cứng vô nghĩa |
| Xóa phụ tùng | Xóa data test | Xóa mềm, audit có log |

## Danh mục `/categories`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Tree/list | Mở trang | Có 2 root chính: Xe máy, Phụ tùng; con nằm đúng cha |
| Tạo danh mục con | Tạo E2E dưới root phù hợp | Kind/parent đúng |
| Sửa | Đổi tên/sort/status | Reload vẫn giữ |
| Xóa | Xóa danh mục test | Không ảnh hưởng danh mục có sản phẩm |
| UI | Kiểm tra dropdown cha | Không quá bé, không vỡ |

## Hãng xe và Dòng xe `/brands`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List hãng | Mở trang | Logo/name/status đúng |
| Upload logo | Import file logo | Logo persist sau reload |
| CRUD hãng | Tạo/sửa/xóa E2E brand | Data đúng, audit có log |
| List dòng xe | Chọn hãng | Dòng xe filter theo hãng |
| CRUD dòng xe | Tạo/sửa/xóa E2E model | Model không fix cứng, gắn đúng brand |
| UI logo | Kiểm tra kích thước logo | Đủ nhìn, không vỡ bảng |

## Đơn hàng `/orders`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Có mã đơn, khách hàng, tổng tiền, trạng thái đơn, thanh toán, vận chuyển, ngày |
| Filter orderStatus | Chọn từng status | BE trả đúng status |
| Filter paymentStatus | Chọn Unpaid/Paid | Data đúng |
| Filter fulfillmentStatus | Chọn Unallocated/Shipped/Fulfilled | Data đúng |
| Search | Tìm mã đơn | Data đúng |
| Export | Bấm xuất danh sách đơn | XLSX đúng font/cột/data |
| Chi tiết | Bấm chi tiết từng trạng thái | Route `/orders/:id`, data đúng |

## Chi tiết đơn hàng `/orders/:id`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Thông tin đơn | Đối chiếu với GET detail | Mã, tiền, khách, địa chỉ đúng |
| Lines | Kiểm tra sản phẩm/SKU/đơn giá/SL/thành tiền | Khớp BE lines |
| Timeline | Kiểm tra tạo đơn, payment, shipping, order status | Ngày giờ đúng, không lộn bố cục |
| Cập nhật trạng thái đơn | Chọn next status | OrderStatus đổi, FulfillmentStatus sync nếu cần |
| Cập nhật vận chuyển | Chọn fulfillment status | FulfillmentStatus đổi, OrderStatus sync nếu cần |
| Ghi thanh toán thủ công | Nhập số tiền/method/note | Tạo Payment, PaymentStatus cập nhật |
| Hủy payment | Qua trang payment hoặc API nếu UI có | PaymentStatus tính lại đúng |
| Hủy đơn | Nhập lý do | Đơn Cancelled, tồn/giữ chỗ xử lý đúng |
| In phiếu | Bấm in | Mở bản in đủ thông tin |
| Nút quay lại | Bấm quay lại | Về list, bố trí bên trái |

## Voucher `/vouchers`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Code, loại, giá trị, thời hạn, trạng thái đúng |
| Create | Tạo voucher % và fixed | Validate đúng |
| Scope | Chọn danh mục/sản phẩm/hãng bằng checkbox/multi-select | Không nhập tay ID, hiển thị tên rõ |
| Edit | Mở sửa voucher có scope | Form hiện đúng dữ liệu cũ |
| Validation | Sai ngày, giá trị âm, thiếu code | Báo lỗi |
| Delete/deactivate | Xóa voucher test | Không hiện hoặc status đúng |

## Tồn kho `/inventory`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Summary | Đối chiếu total SKU, hết hàng, sắp hết, giữ chỗ | Khớp BE |
| Table | Đối chiếu SKU/product/onHand/reserved/available/reorder/updatedAt | Khớp BE |
| Search/filter | Tìm SKU, status, lowStockOnly, hasHold | Data đúng |
| Sort | Tên, tồn khả dụng, tồn thực tế, đang giữ, ngày cập nhật | Order đúng |
| Holds | Bấm số đang giữ | Modal hiện đơn, SKU, số lượng, hạn |
| Threshold | Bấm cảnh báo, đổi ngưỡng test | Cập nhật đúng |
| Adjust | Nhập/xuất/điều chỉnh tồn test | StockMovement/audit đúng, không âm |
| Export | Xuất tồn kho | XLSX thật, tiếng Việt không lỗi font, status tiếng Việt |
| Sync | Bấm đồng bộ | Không crash, last sync cập nhật |

## Phiếu kho `/stock-documents`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Code/type/store/status/line count đúng |
| Create nhập kho | Tạo phiếu E2E với SKU | Draft/Pending đúng |
| Approve | Duyệt phiếu | Inventory tăng, movement/audit có |
| Cancel | Hủy phiếu chưa duyệt | Status cancelled |
| Validation | Không dòng, qty <= 0, SKU thiếu | Báo lỗi |
| Export | Xuất danh sách phiếu | XLSX đúng |

## Người dùng `/users`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Admin-only | Staff mở route | Bị chặn |
| List | Admin mở trang | User/customer/staff/admin đúng role |
| Chỉ admin chính | Kiểm tra tài khoản admin | Không thao tác làm mất admin cuối |
| Create staff/customer | Tạo user E2E với role | Role đúng ngay khi tạo |
| Edit | Đổi tên/phone/status/role | Persist đúng |
| Lock/unlock | Khóa user test | Login bị chặn nếu inactive |
| Validation | Email sai, trùng email, password yếu | Báo lỗi |

## Khách hàng `/customers`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List customer | Mở trang | Chỉ customer, thông tin liên hệ đúng |
| Search/filter | Tìm tên/sđt/email/status | Data đúng |
| Care note | Thêm/sửa ghi chú chăm sóc | Persist sau reload |
| Export | Xuất khách hàng | XLSX đúng dữ liệu |
| Detail nếu có | Mở lịch sử mua/bảo hành nếu có | Không lỗi |

## Bảo hành `/warranties`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Mã bảo hành, khách, sản phẩm, status đúng |
| Create | Tạo yêu cầu bảo hành test | Validate đúng |
| Update status | Nhận xử lý/chờ phụ tùng/hoàn tất | History đúng |
| Print/export | Bấm in/xuất nếu có | Đủ thông tin |
| Validation | Thiếu khách/sản phẩm/lý do | Báo lỗi |

## Đánh giá `/reviews`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Product/user/rating/title/status/date đúng |
| Filter status/rating | Chọn từng filter | Data đúng |
| Approve | Duyệt review test | Status Approved |
| Hide | Ẩn review test | Status Hidden |
| Delete | Xóa review test nếu cho phép | Data/audit đúng |
| UI | Sao, badge, ngày tạo | Không trống/không lệch |

## Bài viết `/posts`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Title/category/status/date/image đúng |
| Create | Nhập title/content/category/upload ảnh | Lưu đúng |
| Edit | Mở sửa bài có nội dung dài | Modal không tràn, data cũ đúng |
| Validation | Thiếu title/content, URL/file lỗi | Báo lỗi |
| Delete/deactivate | Xóa bài test | Data đúng |

## FAQ `/faq`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Question/category/sort/status đúng |
| Create/edit | Tạo/sửa FAQ test | Có dấu, status đúng |
| Hide/delete | Ẩn/xóa FAQ test | Bảng hiển thị đúng |
| Validation | Thiếu question/answer | Báo lỗi |

## Liên hệ `/contacts`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Name/phone/email/message/status/date đúng |
| Mark processed | Đánh dấu xử lý | Status đổi, audit có |
| Search/filter | Tìm email/status | Data đúng |
| Detail/read | Mở nội dung dài | Không tràn |

## Banner trang chủ `/home-banners`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| List | Mở trang | Image/title/link/position/status đúng |
| Upload image | Import file ảnh | Persist sau reload |
| Create/edit/delete | CRUD banner test | Data đúng |
| Validation | Thiếu ảnh, link sai nếu có validate | Báo lỗi |

## Báo cáo `/reports`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Date range | Chọn khoảng ngày | Data thay đổi đúng |
| Summary | Tổng doanh thu, tổng đơn, AOV | Khớp BE/order paid delivered/completed |
| Charts | Revenue/status/top product | Không rỗng khi có data |
| Top table | Đối chiếu order lines | Tên, qty, revenue đúng |
| Export revenue | Xuất XLSX | Có hướng dẫn, summary, order detail, top product |
| Empty date range | Chọn khoảng không có data | UI empty đúng, không crash |

## Audit logs `/audit-logs`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Admin-only | Staff mở route | Bị chặn |
| List | Mở trang | Entity/action/time/entityId/note đúng |
| Filter entity/action | Chọn Product/Order/Modified | Data đúng |
| Filter actor/date/keyword | Nhập từng field | Query BE đúng |
| Preview JSON | Kiểm tra ghi chú dài | Bảng không vỡ, tooltip giữ full text |
| Mutation audit | Sau CRUD test | Audit có record tương ứng |

## Cấu hình vận hành `/settings`

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Load | Mở trang | Setting hiện đúng |
| Edit setting | Thay đổi setting test | Persist sau reload |
| Validation | Giá trị sai kiểu | Báo lỗi |
| Permission | Staff nếu không được sửa | Bị chặn hoặc nút disabled |

## Quyền và bảo mật API qua FE

| Case | Thao tác | Kỳ vọng |
|---|---|---|
| Customer đọc đơn khác | Dùng customer token gọi/order UI nếu có | `404`/không lộ dữ liệu |
| Staff vào Admin-only | Mở `/users`, `/audit-logs` | Redirect/chặn |
| API tồn kho | Gọi không token | `401/403` |
| Token hết hạn | Xóa/đổi token localStorage | FE về login |

## Kiểm tra file export

Với từng export:

- Mở file bằng Excel hoặc parser XLSX.
- Kiểm tra MIME và extension `.xlsx`.
- Kiểm tra font tiếng Việt ở header và dữ liệu.
- Kiểm tra cột số/tiền/ngày có format đúng.
- Đối chiếu ít nhất 3 dòng với API BE.
- Kiểm tra file không bị rỗng khi có data và có empty state hợp lý khi không có data.

## Kiểm tra database sau test

Sau toàn bộ mutation:

- Không còn record test `E2E-*` ngoài những record cố tình giữ lại.
- Product test đã xóa mềm hoặc hard cleanup theo kế hoạch.
- StockMovement phản ánh đúng nhập/xuất/adjust.
- Payment test nếu hủy thì order payment status được tính lại.
- AuditLogs có record cho CRUD/status/update/upload.
- Không có InventoryItem orphan từ SKU/product đã cleanup.
- Không có file upload test mồ côi trong `wwwroot/uploads`.

## Acceptance criteria

- Toàn bộ route trong `v2/frontend-admin/src/App.jsx` đã được test.
- Toàn bộ nút hiển thị đã được bấm hoặc có lý do không bấm được ghi rõ.
- Toàn bộ bảng đã được chụp screenshot và đối chiếu dữ liệu với BE.
- Toàn bộ form chính đã test hợp lệ/lỗi/thiếu/sai định dạng.
- Không còn console error ở các route chính.
- Không còn trang trắng, bảng lệch cột, footer phình, sidebar che nội dung sai.
- `dotnet build MoToSale.slnx` pass.
- `npm run build` pass.
- Có báo cáo kết quả test kèm lỗi, file ảnh, API evidence, DB cleanup.

## Template ghi kết quả

| Route | Case | Status | Evidence | Lỗi nếu có | File liên quan | Kết quả sửa/retest |
|---|---|---|---|---|---|---|
| `/orders` | Filter order status | Pending |  |  |  |  |
| `/inventory` | Export XLSX | Pending |  |  |  |  |
