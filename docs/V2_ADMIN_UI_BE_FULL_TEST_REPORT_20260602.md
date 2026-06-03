# V2 Admin UI - Backend Full Coverage Test Report

Ngày kiểm thử: `2026-06-02`
Plan nguồn: `docs/V2_ADMIN_UI_BE_FULL_TEST_PLAN.md`
Evidence: `test-artifacts/v2-admin-regression-20260602`

## 1. Kết luận

Trạng thái tổng thể: **Failed - chưa thể đưa vào vận hành thực tế**.

Backend v2 build thành công và nhiều CRUD lõi hoạt động tốt khi gọi đúng contract. Tuy nhiên, FE admin hiện hành và backend v2 chưa đồng bộ contract ở các nghiệp vụ quan trọng: đơn hàng, tồn kho tổng hợp, danh sách sản phẩm, review và logo hãng xe. Có một lỗi phân quyền nghiêm trọng: customer đọc được chi tiết đơn hàng của customer khác.

## 2. Môi trường

| Thành phần | Kết quả |
|---|---|
| Gateway `http://localhost:5100` | Pass |
| AuthService `http://localhost:5101` | Pass |
| APIService `http://localhost:5102` | Pass |
| FE đang được mở `http://localhost:5175` | Chạy từ `FrontendAdmin`, không phải `v2/frontend-admin` |
| FE v2 đối chiếu `http://localhost:5176` | Build được, nhưng đăng nhập UI bị lỗi |
| Backend build | Pass, `0 warning`, `0 error` |
| FE v2 build | Pass, còn cảnh báo chunk lớn |
| Swagger JSON | Blocked: route Swagger không được expose |
| SQL CLI | Blocked: `sqlcmd` không kết nối được `(localdb)\MSSQLLocalDB` dù service vẫn đọc/ghi DB |

## 3. Phạm vi đã thực hiện

- Smoke test và screenshot toàn bộ `20` route admin.
- Mở/đóng các modal tạo mới, xem chi tiết, biến thể, ảnh sản phẩm, tương thích phụ tùng, ghi chú khách hàng.
- Kiểm tra responsive desktop, tablet và mobile; không thấy horizontal overflow ở viewport đã đo.
- Đối chiếu API read matrix, nested endpoint, endpoint gap và quyền `Admin`, `Staff`, `Customer`.
- Chạy mutation có kiểm soát với tiền tố `E2E-20260602`, đọc lại dữ liệu và cleanup.
- Upload file ảnh thật, kiểm tra read-back và cleanup file upload.
- Export tồn kho và kiểm tra BOM.
- Chạy build cuối vòng cho backend và FE v2.

## 4. Mutation đã test

| Nghiệp vụ | Kết quả |
|---|---|
| Danh mục create, update, delete | Pass |
| Hãng xe create, update, delete | Pass |
| Dòng xe create, update, delete | Pass |
| Voucher create, update, delete | Pass |
| FAQ create, update, delete | Pass |
| Bài viết create, update, delete | Pass |
| Banner create, update, delete | Pass |
| User create Staff, update, khóa, delete | Pass |
| Ghi chú chăm sóc khách hàng | Pass; restore về chuỗi rỗng vì BE từ chối `null` |
| Review `Rejected -> Approved` | Pass |
| Review `Hidden` | Failed: BE không chấp nhận enum FE gửi |
| Ngưỡng tồn kho update, restore | Pass |
| Điều chỉnh tồn kho `+1 -> -1` | Pass; tồn cuối giữ nguyên |
| Phiếu kho nháp create, cancel | Pass |
| Thanh toán thủ công create, cancel | Pass |
| Liên hệ đánh dấu đã xử lý | Pass |
| Biến thể SKU create, update, delete | Pass |
| Tương thích phụ tùng create, update, delete | Pass |
| Ảnh sản phẩm upload, read-back, delete | Pass |
| Upload ảnh post, banner | Pass |
| Upload logo hãng xe | Failed: file lưu nhưng URL không được persist vào hãng |

## 5. Lỗi bắt buộc sửa

### `V2-ADMIN-SEC-001` - Customer đọc được đơn hàng của customer khác

- Mức độ: **Critical**
- API: `GET /api/orders/2`
- Actual: token của `customer@motosale.local` nhận `200`.
- Nguyên nhân: controller chỉ dùng `[Authorize]`; service lấy đơn theo `id` mà không kiểm tra ownership.
- Ảnh hưởng: rò rỉ thông tin đơn hàng và có nguy cơ hủy đơn của người khác qua endpoint dùng chung.
- Liên quan: `OrdersController.cs`, `OrderService.cs`.

### `V2-ADMIN-ORD-001` - FE cập nhật trạng thái đơn sai contract BE

- Mức độ: **High**
- BE nhận: `{ toStatus, note }`.
- FE gửi: `{ trangThaiDonHang, trangThaiVanChuyen, lyDoHuyDon }`, `{ trangThaiThanhToan, ghiChuThanhToan }`, hoặc `{ trangThaiVanChuyen, ghiChuGiaoNhan }`.
- Ảnh hưởng: modal nhìn hợp lệ nhưng submit không cập nhật đúng. Thanh toán và fulfillment cần endpoint nghiệp vụ riêng.
- Liên quan: `OrderingDtos.cs`, `OrderService.cs`, `pages/orders/OrderDetail.jsx`.

### `V2-ADMIN-ORD-002` - Filter đơn hàng không hoạt động

- Mức độ: **High**
- Bước tái hiện: tại `/orders`, chọn `Đã hủy`, `Đã thanh toán` hoặc `Đã giao`.
- Actual: URL đổi query nhưng bảng vẫn giữ đủ `6` đơn.
- Expected: bảng lọc theo điều kiện.
- Evidence: `order-filter-ui.json`, `orders.png`.

### `V2-ADMIN-ORD-003` - Danh sách đơn thiếu khách hàng và fulfillment hiển thị enum thô

- Mức độ: **High**
- Actual: cột khách hàng là `-`; fulfillment hiển thị `Unallocated`, `Shipped`, `Fulfilled`.
- API list không trả tên khách hàng.
- Evidence: `orders.png`, `order-detail-4-api.json`.

### `V2-ADMIN-ORD-004` - Chi tiết đơn thiếu timeline thực tế

- Mức độ: **High**
- Actual: đơn đã giao vẫn chỉ có log tạo đơn; thời gian timeline và thông tin đơn lệch múi giờ.
- API detail không trả histories hoặc payment ledger.
- Evidence: `order-detail-4.png`, `order-detail-4-api.json`.

### `V2-ADMIN-SEC-002` - Nút xóa sản phẩm gọi endpoint không tồn tại

- Mức độ: **High**
- FE gọi: `DELETE /api/products/{id}`.
- BE trả: `405`.
- Ảnh hưởng: sản phẩm không thể xóa từ UI; cần quyết định nghiệp vụ soft-delete hoặc hard-delete và đồng bộ FE/BE.

### `V2-ADMIN-CAT-001` - Danh sách sản phẩm hiển thị tồn kho bằng `0`

- Mức độ: **High**
- Actual: mọi xe máy hiển thị tồn `0`.
- API product list không trả tồn tổng hợp; tồn thực tế có dữ liệu trong inventory.
- Evidence: `motorcycles.png`, `api-read-matrix.json`.

### `V2-ADMIN-INV-001` - Dashboard tồn kho luôn bằng `0`

- Mức độ: **High**
- Actual: `Tổng SKU`, `Hết hàng`, `Sắp hết`, `Đang giữ chỗ` đều là `0`, trong khi API inventory có `100` dòng.
- FE chờ `summary` và `lastSyncAt`, BE chỉ trả paging list.
- Evidence: `inventory.png`.

### `V2-ADMIN-INV-002` - Filter tồn kho FE vượt contract BE

- Mức độ: **Medium**
- FE gửi `stockStatus`, `hasHold`, `sortBy`, `sortDirection`.
- BE DTO chỉ có `StoreId`, `LowStockOnly` và `Keyword` kế thừa.
- Actual: chọn trạng thái tồn không lọc dữ liệu.

### `V2-ADMIN-REV-001` - Nút ẩn review dùng enum không hợp lệ

- Mức độ: **Medium**
- FE gửi `Hidden`.
- BE chỉ nhận `Pending`, `Approved`, `Rejected`.
- Mutation API xác nhận `Hidden` trả `400`, `Rejected -> Approved` pass.

### `V2-ADMIN-BRAND-001` - Upload logo hãng xe không persist

- Mức độ: **High**
- API upload trả URL `/uploads/brands/...png`.
- Read-back hãng vừa upload vẫn có `logoUrl: null`.
- Reload trang sẽ mất logo.
- Evidence: `upload-probes.json`.

### `V2-ADMIN-RPT-001` - Dashboard và báo cáo top sản phẩm hiển thị `0`

- Mức độ: **Medium**
- Actual: top sản phẩm có doanh số và doanh thu `0`.
- FE tổng hợp client-side từ order list, nhưng DTO list không có lines hoặc sold quantity.
- Evidence: `dashboard.png`, `reports.png`.

### `V2-ADMIN-FE-001` - FE được chạy sai thư mục

- Mức độ: **High**
- Port `5175` chạy từ `D:\MotorTeam\MoToSale-End\FrontendAdmin`.
- Plan yêu cầu `D:\MotorTeam\MoToSale-End\v2\frontend-admin`.
- Port `5176` đã bật để đối chiếu FE v2, nhưng login bằng UI báo lỗi dù gọi API login qua proxy trả `200`.

### `V2-ADMIN-AUD-001` - Audit log chưa ghi nhận mutation

- Mức độ: **Medium**
- Sau nhiều thao tác ghi, `GET /api/audit-logs` vẫn trả `0` dòng.
- Ảnh hưởng: thiếu truy vết vận hành.

### `V2-ADMIN-EXP-001` - Export tồn kho chưa phải XLSX thật

- Mức độ: **Low**
- API trả TSV UTF-16 có BOM `FF FE`, dùng đuôi `.xls`.
- Font tiếng Việt ổn hơn CSV cũ, nhưng không phải workbook `.xlsx`.
- Evidence: `inventory-export.xls`.

## 6. Lỗi và thiếu bổ sung

- Dashboard có breadcrumb lẻ `Dashboard`.
- Dashboard dùng link `/products`; app redirect về `/motorcycles`, chưa thể hiện lựa chọn rõ giữa xe máy và phụ tùng.
- Staff bị hiển thị tên mặc định `Admin` trên navbar và sidebar.
- Product form vẫn có trường tồn kho ban đầu trong khi tồn kho đã tách thành module nghiệp vụ riêng.
- Backend có API allocation suggestion và allocate nhưng chưa thấy luồng UI vận hành tương ứng.
- Category seed có `Xe côn tay` và `Xe số` cùng `sortOrder = 2`.
- Brand page từng xuất hiện alert tải lỗi trong khi bảng vẫn có dữ liệu; cần retest sau khi đồng bộ FE v2.
- Nhiều chuỗi source trong FE/BE đang bị mojibake; cần chuẩn hóa UTF-8 toàn repo.
- Inventory export dùng header không dấu; cần workbook XLSX chuẩn nếu dùng vận hành.
- FE còn service stale như brand/category/review/FAQ detail GET không tồn tại hoặc trả `405`.

## 7. Quyền đã đối chiếu

| Ca test | Kết quả |
|---|---|
| Không token gọi inventory | `401` Pass |
| Customer gọi inventory | `403` Pass |
| Staff gọi users all | `403` Pass |
| Customer gọi users all | `403` Pass |
| Staff gọi audit log | `403` Pass |
| Customer gọi order search admin | `403` Pass |
| Staff đọc cấu hình vận hành | `200` Pass |
| Customer đọc foreign order detail | `200` **Fail Critical** |

## 8. Evidence chính

- `route-smoke.json`
- `route-controls.json`
- `api-read-matrix.json`
- `authorization-api-probes.json`
- `endpoint-gap-probes.json`
- `nested-api-probes.json`
- `modal-open-close-results.json`
- `controlled-mutations.json`
- `secondary-mutations.json`
- `sku-compatibility-mutations.json`
- `upload-probes.json`
- `post-cleanup-state.json`
- `inventory-export.xls`
- Screenshot từng route và modal trong cùng thư mục evidence.

## 9. Cleanup

Đã xóa toàn bộ category, brand, model, voucher, FAQ, post, banner, user, SKU, compatibility và product image tạm. Đã xóa `4` file upload thử.

Giữ lại có chủ đích:

- Một `StockDocument` trạng thái `Cancelled`.
- Một `Payment` trạng thái `Cancelled`, `transactionRef = E2E-20260602`.
- Hai `StockMovement` `+1` và `-1`, tồn cuối không đổi.
- Một contact seed đã được đánh dấu `Processed` để xác nhận action.

## 10. Hướng sửa ưu tiên

1. Vá ownership cho `GET /orders/{id}` và `POST /orders/{id}/cancel`.
2. Thiết kế lại contract cập nhật đơn: order status, fulfillment, manual payment, allocation và timeline.
3. Đồng bộ FE v2 làm bản chạy chính; bỏ FE cũ khỏi quy trình host.
4. Bổ sung customer name, inventory aggregate, summary và report DTO từ BE.
5. Sửa persist logo hãng và enum review.
6. Chốt nghiệp vụ xóa sản phẩm theo soft-delete hoặc hard-delete.
7. Bổ sung audit log thật và export XLSX thật.
# Retest sau sửa lỗi - 2026-06-02

## Kết quả

- `Done` Security đơn hàng: customer truy cập đơn không thuộc sở hữu nhận `404`.
- `Done` Order list/detail: trả đúng khách hàng, lines, payment, history; filter `orderStatus`, `paymentStatus`, `fulfillmentStatus` hoạt động.
- `Done` Đồng bộ trạng thái: `Delivered -> Fulfilled`, hoàn tác `Shipping -> Shipped`; timeline ghi cả trạng thái đơn và vận chuyển.
- `Done` Thanh toán thủ công: ghi phiếu thu làm đơn thành `PartiallyPaid`, hủy phiếu trả về `Unpaid`; timeline ghi đủ hai sự kiện.
- `Done` Product: list trả tồn kho thật; DELETE xóa mềm và sản phẩm biến mất khỏi list; DTO list/detail trả `status`.
- `Done` Brand logo: upload file persist vào DB; đã hoàn tác logo test sau khi xác nhận.
- `Done` Review: trạng thái `Hidden` hợp lệ; FE normalize đúng `reviewStatus` và `createdDate`.
- `Done` Inventory: API có auth, summary/filter/last sync đúng; FE đọc `skuId`, `onHand`, `reserved`, `available`, `reorderPoint`, `updatedAt`; export trả XLSX thật.
- `Done` Dashboard/report: bỏ breadcrumb lẻ; bảng chi tiết và top sản phẩm dùng order lines thật.
- `Done` Port FE v2: chuẩn hóa Vite mặc định sang `5176`, tránh nhầm với admin cũ ở `5175`.
- `Done` Audit: tự ghi mutation; UI map đúng entity/action, có preview JSON gọn và filter BE.
- `Done` Swagger: `/swagger/v1/swagger.json` trả `200`, upload file không còn làm lỗi generate schema.
- `Done` Build: `dotnet build MoToSale.slnx` và `npm run build` đều thành công.

## Bằng chứng smoke test

- Order count: `6`; customer name và lines có dữ liệu.
- Inventory: `100` dòng, không còn dữ liệu `E2E`, tổng tồn thực tế có dữ liệu.
- Report UI: doanh thu `76.145.000 ₫`, top sản phẩm gồm Air Blade, Wave Alpha và nhớt Honda.
- Audit filter `Order + Modified + Shipping`: trả `4` dòng.
- Screenshot: `test-artifacts/v2-admin-fixes-20260602/screenshots/`.
