# Báo cáo rà soát FrontendAdmin so với v2/frontend-admin

Ngày rà soát: 2026-06-09

## Kết luận

`ShowRoomDB/FrontendAdmin` **chưa giống** `ShowRoomDB/v2/frontend-admin`.

Hai thư mục có cấu trúc `src` gần giống nhau, nhưng nội dung nhiều màn hình, service, route, CSS và cấu hình build đang lệch. Nếu `v2/frontend-admin` là bản chuẩn cần bám theo, `FrontendAdmin` còn thiếu nhiều phần mapping dữ liệu/API v2 và cũng có một số chức năng dư không tồn tại ở v2.

## Phạm vi so sánh

- So sánh chính: `FrontendAdmin/src` với `v2/frontend-admin/src`.
- Có đối chiếu thêm: `package.json`, `vite.config.js`.
- Bỏ qua khi kết luận: `node_modules`, `dist`, file `.backup`, tài liệu kiểm thử cũ.

## Sai khác route và menu



### FrontendAdmin thiếu/sai so với Sidebar v2

- V2 có `displayName = user.fullName || user.hoTen || user.name || ...`; `FrontendAdmin` chỉ dùng `hoTen/name/Admin`, nên hiển thị sai tên nếu API v2 trả `fullName` hoặc role Staff.
- Menu "Phiếu kho" ở `FrontendAdmin` vẫn dùng nhãn cũ, trong v2 là "Chứng từ kho".
- Thứ tự menu nội dung có lệch: v2 đưa "Banner trang chủ" lên trước "Bài viết/FAQ/Liên hệ".
- Link `/settings` trong v2 active theo group `/settings`; `FrontendAdmin` chỉ active chính xác `/settings` và tách thêm `/settings/payment`.

## Sai khác cấu hình hằng số nghiệp vụ

File: `FrontendAdmin/src/utils/constants.js`

So với v2, `FrontendAdmin` đang dùng mô hình đơn hàng đã đơn giản hóa còn 3 trạng thái chính:

- `AwaitingPayment`
- `Confirmed`
- `Cancelled`

Trong khi v2 có đầy đủ luồng:

- `AwaitingPayment`
- `Confirmed`
- `Allocated`
- `Shipping`
- `Delivered`
- `Completed`
- `Cancelled`

Các điểm sai/thiếu:

- Thiếu trạng thái `Allocated`, `Shipping`, `Delivered`, `Completed` trong options cập nhật đơn.
- `ORDER_NEXT_STATUS` không khớp luồng v2: v2 đi `Confirmed -> Allocated -> Shipping -> Delivered -> Completed`.
- `SHIPPING_STATUS` của `FrontendAdmin` dùng `Preparing/Shipping/Delivered`; v2 dùng `Unallocated/Allocated/Shipped/Fulfilled`.
- `PAYMENT_STATUS_OPTIONS` của `FrontendAdmin` loại bỏ `Pending` và `Failed`; v2 vẫn hiển thị hai trạng thái này.
- `PAYMENT_METHODS` của `FrontendAdmin` thiếu `Cash`.

Ảnh hưởng: các màn hình đơn hàng, báo cáo, dashboard và cập nhật vận chuyển/thanh toán có thể gửi sai trạng thái hoặc không hiển thị đúng dữ liệu API v2.

## Sai khác service/API

### Auth

File: `FrontendAdmin/src/services/authService.js`

- `FrontendAdmin` login bằng payload `{ Email, MatKhau }`.
- V2 login bằng payload `{ email, password }`.

Nếu backend v2 yêu cầu camelCase, login của `FrontendAdmin` sẽ lỗi.

### Product service

File: `FrontendAdmin/src/services/productService.js`

`FrontendAdmin` còn thiếu gần như toàn bộ lớp normalize/map payload của v2:

- Thiếu `normalizeProduct`, `normalizeSku`, `normalizeImage`.
- Thiếu `mapProductSearchParams`, nên filter `loaiSanPham`, `maDanhMuc`, `maHangXe`, `trangThaiSanPham` chưa map sang `kind`, `categoryId`, `brandId`, `status`.
- Update sản phẩm dùng `PATCH /products/:id`; v2 dùng `PUT /products/:id`.
- Variant dùng endpoint `/products/:id/variants`; v2 dùng `/products/:id/skus`.
- Thiếu `setPrimaryImage(productId, imageId)`.
- Tên hàm promotions/related lệch:
  - `FrontendAdmin`: `getApplicableVouchers`, `getRelatedProducts`
  - v2: `getPromotions`, `getRelatedItems`
- Upload image chưa normalize `isMain -> isPrimary`, `maBienSanPham -> skuId`.

Ảnh hưởng: danh sách sản phẩm, biến thể/SKU, ảnh sản phẩm, khuyến mại, bán kèm và barcode có thể không hoạt động đúng với API v2.

### Brand/Model service

File: `FrontendAdmin/src/services/brandService.js`

Thiếu normalize/map payload của v2:

- Brand chưa map `maHangXe/name/logo/status`.
- Model chưa map `maDongXe/brandId/name/status`.
- Payload tạo/sửa brand/model chưa chuyển sang schema v2 (`name`, `slug`, `logoUrl`, `status`, `brandId`).

### Category service

File: `FrontendAdmin/src/services/categoryService.js`

Thiếu normalize/map payload của v2:

- Chưa normalize `id/maDanhMuc`, `parentId/danhMucChaId/maDanhMucCha`, `name/tenDanhMuc`, `sortOrder/thuTu`, `status/dangHoatDong`.
- Vẫn có `getById` và `uploadImage`, trong khi v2 service không dùng các hàm này.
- Payload tạo/sửa chưa map sang `parentId`, `name`, `slug`, `kind`, `sortOrder`, `status`.

### Order/Payment service

Files:

- `FrontendAdmin/src/services/orderService.js`
- `FrontendAdmin/src/services/paymentService.js`

`FrontendAdmin` thiếu các API v2:

- `orderService.updateFulfillmentStatus`
- `orderService.getAllocationSuggestion`
- `orderService.allocate`
- `paymentService.getByOrder`
- `paymentService.record`

Các API lệch:

- `cancel` của `FrontendAdmin`: `PUT /orders/:id/cancel`
- `cancel` của v2: `POST /orders/:id/cancel`
- `FrontendAdmin` còn dùng API riêng:
  - `/orders/:id/payment-info`
  - `/orders/:id/confirm-payment`
  - `/orders/:id/refunds/:refundId/confirm`

Các API riêng này không có trong v2 service.

### Inventory service

File: `FrontendAdmin/src/services/inventoryService.js`

Thiếu API v2:

- `getGoodsReceipts`
- `getGoodsReceiptById`
- `getStores`
- `getSkus`

Ảnh hưởng: màn `StockDocumentList` của `FrontendAdmin` không thể hiển thị luồng "Chứng từ kho" hợp nhất giữa phiếu kho thủ công và nhận hàng từ nhà cung cấp như v2.

### User/Customer service

File: `FrontendAdmin/src/services/userService.js`

Thiếu:

- `getCustomerProfile(id) -> GET /customers/:id/profile`

Ảnh hưởng: màn khách hàng thiếu hồ sơ khách hàng 360 và tạo lịch chăm sóc.

### Content/FAQ/Post/Voucher/Warranty/Review/Report services

Nhiều service trong `FrontendAdmin` còn trả dữ liệu raw, trong khi v2 có normalize/map payload:

- `faqService`
- `postService`
- `voucherService`
- `warrantyService`
- `reviewService`
- `reportService`

Ảnh hưởng: UI có thể không đọc đúng các field camelCase của API v2 như `items/data`, `id`, `status`, `createdDate`, `startAt/endAt`, `grandTotal`, v.v.

## Sai khác từng màn hình chính

### Dashboard

File: `FrontendAdmin/src/pages/Dashboard.jsx`

Thiếu so với v2:

- State/data:
  - `inventoryWarnings`
  - `crmTasks`
  - `operations`
- Các chỉ số vận hành:
  - doanh thu hôm nay
  - còn phải thu
  - cần trả nhà cung cấp
  - CSKH cần xử lý
  - pending purchases
  - open repairs
  - paid/refunded totals
- Bảng "Cảnh báo tồn kho".
- Bảng "CSKH cần xử lý".
- Fallback field v2: `dashboard.orders`, `order.code`, `grandTotal`, `placedAt`, `endAt`, `warrantyStatus`.
- Link sản phẩm tổng quan ở `FrontendAdmin` trỏ `/products`; v2 trỏ `/motorcycles`.

### Danh sách sản phẩm

File: `FrontendAdmin/src/pages/products/ProductList.jsx`

Thiếu so với v2:

- Import/export Excel cho danh sách sản phẩm.
- Link "Nhập nhanh/XLSX" tới `/operational-imports`.
- Bộ lọc:
  - `stockStatus`
  - `hasPromotion`
  - `minPrice`
  - `maxPrice`
- Fallback field v2:
  - `categoryId`
  - `brandId`
  - `salePrice`
  - `listPrice`
- Hiển thị giá khuyến mại dạng "Không" nếu không có sale price.
- Reset thêm các modal barcode/promotions/related/aging khi đổi loại sản phẩm.
- Một số màu nút và tooltip action khác v2.

### Product service và các modal sản phẩm

Files:

- `FrontendAdmin/src/pages/products/ImageManager.jsx`
- `FrontendAdmin/src/pages/products/VariantManager.jsx`
- `FrontendAdmin/src/pages/products/ProductForm.jsx`
- `FrontendAdmin/src/pages/products/ProductPromotionsModal.jsx`
- `FrontendAdmin/src/pages/products/ProductRelatedManager.jsx`

Sai khác lớn:

- `ImageManager` của `FrontendAdmin` vẫn theo luồng upload/preview cũ và CSS riêng; v2 dựa vào image API mới có set primary image và normalize field.
- `VariantManager` của `FrontendAdmin` dùng variant schema cũ; v2 dùng SKU schema (`skuCode`, `variantName`, `listPrice`, `salePrice`, `barcode`, `status`).
- `ProductForm` của v2 map field theo backend v2; `FrontendAdmin` còn gửi trực tiếp nhiều field cũ.

### Chi tiết đơn hàng

File: `FrontendAdmin/src/pages/orders/OrderDetail.jsx`

`FrontendAdmin` dư chức năng không có trong v2:

- Xác nhận thanh toán chuyển khoản bằng `orderService.confirmPayment`.
- Hiển thị/xác nhận kỳ trả góp.
- In hồ sơ trả góp bằng `printInstallmentApplication`.
- Hiển thị và xác nhận yêu cầu hoàn tiền từ khách.

`FrontendAdmin` thiếu/sai so với v2:

- Không dùng `paymentService.record` để ghi nhận thanh toán thủ công.
- Không dùng `orderService.updateFulfillmentStatus` cho vận chuyển.
- Không hỗ trợ các trạng thái/order field v2: `orderStatus`, `fulfillmentStatus`, `lines`, `grandTotal`, `shippingRecipient`, `shippingAddress`, `shippingPhone`, `shippingEmail`, `placedAt`, `skuCode`, `qty`, `lineTotal`.
- Modal thanh toán của `FrontendAdmin` cập nhật trạng thái thanh toán; v2 ghi nhận số tiền, phương thức và note.
- Lịch sử thanh toán của `FrontendAdmin` là bảng nhiều giao dịch cũ; v2 hiển thị thông tin payment theo schema mới.

### Danh sách đơn hàng

File: `FrontendAdmin/src/pages/orders/OrderList.jsx`

Sai khác chính:

- V2 có nhiều fallback field API mới hơn cho mã đơn, khách hàng, tổng tiền, ngày tạo, trạng thái.
- V2 đi theo lifecycle `Allocated/Shipping/Delivered/Completed`; `FrontendAdmin` đi theo lifecycle rút gọn.

### Thanh toán

File: `FrontendAdmin/src/pages/payments/PaymentList.jsx`

Sai khác chính:

- V2 dùng payment service mới có `record` và `cancel` bằng `POST /payments/:id/cancel`.
- `FrontendAdmin` vẫn dùng `confirm`/`cancel` kiểu cũ.

### Khách hàng

File: `FrontendAdmin/src/pages/customers/CustomerList.jsx`

Thiếu so với v2:

- Hồ sơ khách hàng 360.
- API `userService.getCustomerProfile`.
- Tạo lịch chăm sóc bằng `businessOperationsService.createInteraction`.
- Fallback field v2: `fullName`, `phoneNumber`, `careNote`, `orderStatus`, `grandTotal`, `placedAt`.
- Filter trạng thái theo numeric status `1/0/-1`.
- Modal profile gồm:
  - tổng đơn
  - tổng mua
  - bảo hành
  - CSKH mở
  - timeline khách hàng
  - sửa chữa
  - bảo hành

### Chứng từ kho / phiếu kho

File: `FrontendAdmin/src/pages/inventory/StockDocumentList.jsx`

Thiếu so với v2:

- Hợp nhất `inventory/documents` với `inventory/goods-receipts`.
- Load lookup từ `/stores` và `/skus`.
- Tạo phiếu kho theo `type`, `storeId`, `toStoreId`, `reason`, `lines[{skuId, qty, note}]`.
- Các loại chứng từ v2:
  - `1`: Nhập kho khác
  - `2`: Phiếu xuất kho
  - `3`: Phiếu điều chỉnh tồn
  - `4`: Phiếu kiểm kê
  - `5`: Phiếu chuyển kho
  - `PurchaseReceipt`: Nhận hàng từ NCC
- Phân quyền: Staff không được tạo loại nhập kho khác (`type=1`).
- Bảng danh sách có thêm:
  - nguồn chứng từ
  - kho áp dụng
  - kho nhận
  - line count
- Chi tiết/print/export theo schema v2 (`code`, `sourceLabel`, `storeName`, `toStoreName`, `createdDate`, `approvedAt`, `lines`).

`FrontendAdmin` vẫn theo schema cũ: `loaiPhieu`, `maSanPham`, `maBienSanPham`, `soLuong`, `ghiChu`.

### Tồn kho

File: `FrontendAdmin/src/pages/inventory/InventoryView.jsx`

Sai khác chính:

- V2 có nhiều fallback field mới hơn cho store/SKU/product và trạng thái cảnh báo.
- `FrontendAdmin` dễ thiếu dữ liệu nếu API trả theo `storeName`, `skuCode`, `productName`, `available`, `warningStatus`.

### Cấu hình vận hành

File: `FrontendAdmin/src/pages/settings/OperationsSettings.jsx`

Thiếu so với v2:

- Quản lý `Showroom/Kho`.
- API `operationsService.getWarehouses` và `saveWarehouse`.
- Form kho/showroom:
  - tên kho
  - loại kho
  - địa chỉ
  - hotline
  - trạng thái
- Các setting cơ bản của v2:
  - `StoreName`
  - `Hotline`
  - `Address`

`FrontendAdmin` lại có thêm các setting trả góp/thanh toán:

- `InstallmentAnnualRate`
- `InstallmentMinDownPaymentPercent`
- `InstallmentAllowedTerms`
- `PaymentHoldMinutes`
- `DepositMinPercent`

Các setting này không nằm trong `OperationsSettings` của v2.

### Báo cáo

File: `FrontendAdmin/src/pages/reports/ReportsPage.jsx`

Thiếu so với v2:

- Tabs báo cáo:
  - Bán hàng
  - Mua hàng
  - Thu chi/Công nợ
  - Dịch vụ
  - Kho
- Các dataset:
  - `purchaseReports`
  - `cashReports`
  - `receivableReports`
  - `serviceReports.repairs`
  - `serviceReports.warranties`
  - `inventoryWarnings`
  - `crmTasks`
- Export Excel thêm sheet:
  - `MuaHang`
  - `ThuChi`
  - `CongNo`
  - `DichVuSuaChua`
  - `BaoHanh`
  - `CanhBaoTonKho`
- Bảng cảnh báo tồn kho và công nợ khách hàng.

### Cấu hình thanh toán và trả góp

Các file chỉ có ở `FrontendAdmin`, không có ở v2:

- `FrontendAdmin/src/pages/settings/PaymentSettings.jsx`
- `FrontendAdmin/src/pages/installments/InstallmentTermList.jsx`
- `FrontendAdmin/src/utils/printInstallmentApplication.js`

Nếu mục tiêu là giống v2, đây là phần dư/lệch cần quyết định:

- Xóa khỏi `FrontendAdmin`, hoặc
- Giữ lại như chức năng riêng, nhưng không thể nói hai frontend đã giống nhau.

## Sai khác CSS/giao diện

File: `FrontendAdmin/src/index.css`

V2 có bổ sung CSS responsive:

- `@media (max-width: 575.98px)` cho `.card-header`, `.card-tools`, `.card-tools .btn`.
- `.small-box.bg-light { color: #1f2933; }`

`FrontendAdmin` còn có nhiều CSS riêng không còn trong v2:

- `.image-upload-panel`
- `.image-dropzone`
- `.image-preview-strip`
- `.image-preview-tile`
- `.variant-card-list`
- `.variant-image-card`
- `.variant-image-strip`
- `.product-image-thumb`
- `.image-card-overlay`

Ảnh hưởng: giao diện upload ảnh/variant có thể khác v2; mobile card tools có thể chưa responsive giống v2.

## Sai khác package/build/test

### package.json

`FrontendAdmin/package.json` thiếu so với v2:

- Script `test:ui`: `playwright test`
- Dependency dev `@playwright/test`
- `overrides.uuid`
- Phiên bản mới hơn của:
  - `@vitejs/plugin-react`
  - `vite`

### vite.config.js

`FrontendAdmin/vite.config.js` thiếu so với v2:

- `build.chunkSizeWarningLimit = 1000`
- `rollupOptions.output.manualChunks`:
  - `excel`
  - `vendor`
- Port dev khác:
  - `FrontendAdmin`: `5175`
  - v2: `5176`
- Message proxy lỗi vẫn ghi backend chung, v2 ghi rõ API Gateway v2.

### playwright.config.js

`v2/frontend-admin` có `playwright.config.js`; `FrontendAdmin` không có.

## Danh sách ưu tiên sửa nếu muốn FrontendAdmin giống v2

1. Đồng bộ route/menu theo v2: bỏ `/installments`, `/settings/payment` nếu không còn là nghiệp vụ riêng.
2. Đồng bộ `constants.js` theo lifecycle v2 cho đơn hàng, thanh toán và vận chuyển.
3. Đồng bộ toàn bộ service layer theo v2, ưu tiên:
   - `authService`
   - `productService`
   - `orderService`
   - `paymentService`
   - `inventoryService`
   - `userService`
   - `categoryService`
   - `brandService`
4. Đồng bộ các màn có blast radius lớn:
   - `Dashboard`
   - `ProductList`, `ProductForm`, `VariantManager`, `ImageManager`
   - `OrderDetail`, `OrderList`
   - `StockDocumentList`, `InventoryView`
   - `CustomerList`
   - `ReportsPage`
   - `OperationsSettings`
5. Đồng bộ CSS responsive và loại bỏ CSS cũ nếu màn image/variant dùng theo v2.
6. Đồng bộ `package.json`, `vite.config.js`, thêm `playwright.config.js`.
7. Chạy build và UI test sau khi đồng bộ:
   - `npm run build`
   - `npm run test:ui` nếu đã thêm Playwright và test sẵn sàng.

## Ghi chú

Một số phần `FrontendAdmin` có thể là chức năng mở rộng sau v2, đặc biệt trả góp, cấu hình thanh toán, xác nhận hoàn tiền. Nếu đây là nghiệp vụ cần giữ, không nên xóa máy móc; thay vào đó cần port chúng lên schema/API v2 và ghi rõ là extension riêng ngoài baseline `v2/frontend-admin`.
