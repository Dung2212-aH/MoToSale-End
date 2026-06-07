# Plan: Implement v2 Backend for Customer Frontend (ShowRoomDB/frontend)
<!-- STATUS: HOÀN THÀNH ✅ — Phases 1–6 đã triển khai & verify end-to-end (2026-06-07). Build sạch, migration AddCustomerFeatures đã áp dụng, smoke-test qua gateway port 5000 OK. -->

## Context

`ShowRoomDB/frontend/` là customer-facing SPA (React + Vite) hiện đang proxy `/api` đến `http://localhost:5000` — tức là OLD Backend (microservices). Mục tiêu là làm cho frontend này chạy hoàn toàn với **v2 backend** mà không thay đổi bất kỳ file nào trong `frontend/`.

`v2/backend/src` đã có kiến trúc monolith rõ ràng:
- **MoToSale.ApiGateway** (Ocelot, port 5100 → đổi thành 5000) → route tới AuthService + APIService
- **MoToSale.AuthService** (port 5101) → xác thực, user management
- **MoToSale.APIService** (port 5102) → catalog, orders, inventory, content, operations
- **Shared DB:** `MoToSaleV2` (LocalDB), 37 migration files

v2 backend đã có đủ entities, repositories, services, và 20 controllers — nhưng phần lớn hướng về **admin** (v2/frontend-admin). Cần bổ sung endpoints **customer-facing** và đổi port gateway về 5000.

---

## Tổng quan thay đổi

| Phạm vi | Loại thay đổi | Status |
|---------|--------------|--------|
| ApiGateway appsettings.json | Đổi port 5100 → 5000 | ✅ Done |
| ApiGateway ocelot.json | Đổi BaseUrl → 5000 | ✅ Done |
| AuthService DTOs | Thêm ForgotPasswordRequest/Response, ResetPasswordRequest | ✅ Done |
| IAuthService | Thêm ForgotPasswordAsync, ResetPasswordAsync | ✅ Done |
| AuthService.cs | Implement forgot/reset password logic | ✅ Done |
| AuthController.cs | Thêm forgot-password, reset-password endpoints | ✅ Done |
| AuthService Program.cs | Đăng ký IRepository<> generic | ✅ Done |
| UsersController.cs | Thêm address CRUD (PUT/DELETE/set-default/get-default) | 🔄 In progress |
| APIService | Thêm customer-facing reviews, favorites, voucher user ops, content aliases | ⏳ Pending |
| Business logic | Shipping quote, inventory hold, refund request | ⏳ Pending |
| Database | Tạo migration cho Favorites, UserVouchers, PasswordResetTokens | ⏳ Pending |

---

## Phase 1: Gateway Port Fix ✅

**File:** `v2/backend/src/MoToSale.ApiGateway/appsettings.json`
- Đổi `"Urls": "http://localhost:5100"` → `"http://localhost:5000"`

**File:** `v2/backend/src/MoToSale.ApiGateway/ocelot.json`
- Đổi `"BaseUrl": "http://localhost:5100"` → `"http://localhost:5000"`

Ocelot routing đã đúng:
- `/api/auth/{everything}` → AuthService (5101)
- `/api/users/{everything}` → AuthService (5101)
- `/uploads/{everything}` → APIService (5102)
- `/api/{everything}` → APIService (5102)

---

## Phase 2: AuthService — Thêm Endpoints 🔄

### Đã hoàn thành
- **`MoToSale.DTO/Auth/AuthDtos.cs`**: Thêm `ForgotPasswordRequest`, `ForgotPasswordResponse`, `ResetPasswordRequest`
- **`MoToSale.Services/Identity/IAuthService.cs`**: Thêm `ForgotPasswordAsync`, `ResetPasswordAsync`
- **`MoToSale.Services/Identity/AuthService.cs`**: Implement forgot/reset password (dev mode trả token trực tiếp, production cần email service)
- **`MoToSale.AuthService/Controllers/AuthController.cs`**: Thêm `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`
- **`MoToSale.AuthService/Program.cs`**: Đăng ký `IRepository<>` generic cho `PasswordResetToken`

### Còn lại — UsersController.cs
**File:** `v2/backend/src/MoToSale.AuthService/Controllers/UsersController.cs`

Cần thêm:
```
GET  /api/users/me/address              → lấy địa chỉ mặc định (legacy, cho customer frontend)
PUT  /api/users/me/addresses/{id}       → cập nhật địa chỉ
PUT  /api/users/me/addresses/{id}/default → đặt làm mặc định
DELETE /api/users/me/addresses/{id}     → xóa địa chỉ
```

---

## Phase 3: APIService — Customer-Facing Endpoints ⏳

### 3.1 Catalog — đã có sẵn
`CatalogLookupController.cs` đã expose public `GET /api/categories`, `GET /api/brands`, `GET /api/models` — không cần thêm.

### 3.2 Products
**File:** `v2/backend/src/MoToSale.APIService/Controllers/ProductsController.cs`

Cần thêm:
```
GET /api/products/filters        → public, filter options (categories, brands, models)
GET /api/products/{id}/reviews   → public, danh sách approved reviews
GET /api/products/{id}/reviews/summary → public, avg rating + breakdown
POST /api/products/{id}/reviews  → auth, tạo review mới (multipart/form-data)
PATCH /api/products/{id}/reviews/me → auth, cập nhật review của mình
```

**File:** `v2/backend/src/MoToSale.APIService/Controllers/ReviewsController.cs`

Cần thêm:
```
GET /api/reviews/product/{productId}/me → auth, lấy review của user hiện tại
```

**Files cần cập nhật:**
- `MoToSale.Services/Catalog/IReviewService.cs` — thêm customer methods
- `MoToSale.Services/Catalog/ReviewService.cs` — implement
- `MoToSale.Repository/Catalog/IReviewRepository.cs` — thêm query methods
- `MoToSale.Repository/Catalog/ReviewRepository.cs` — implement
- `MoToSale.DTO/Catalog/CatalogDtos.cs` — thêm CustomerReviewDto, CreateReviewRequest, UpdateMyReviewRequest, ProductFiltersDto, ReviewSummaryDto

### 3.3 Favorites
**New file:** `v2/backend/src/MoToSale.APIService/Controllers/FavoritesController.cs`
```
GET    /api/favorites              → auth
POST   /api/favorites/{productId}  → auth
DELETE /api/favorites/{productId}  → auth
```

**Files cần cập nhật:**
- `MoToSale.Services/Catalog/ICatalogService.cs` — thêm favorite methods
- `MoToSale.Services/Catalog/CatalogService.cs` — implement

### 3.4 Vouchers (Customer)
**File:** `v2/backend/src/MoToSale.APIService/Controllers/VouchersController.cs`

Cần thêm/sửa:
```
GET  /api/vouchers          → sửa: cho phép customer xem (public vouchers); admin/staff xem tất cả
POST /api/vouchers/applicable → auth, danh sách voucher áp dụng được
POST /api/vouchers/save     → auth, user lưu voucher
GET  /api/vouchers/my       → auth, danh sách voucher đã lưu
GET  /api/vouchers/my/count → auth, số lượng voucher
```
*(Lưu ý: `POST /api/vouchers/validate` đã tồn tại)*

**Files cần cập nhật:**
- `MoToSale.Services/Ordering/IVoucherService.cs` — thêm user voucher methods
- `MoToSale.Services/Ordering/VoucherService.cs` — implement
- `MoToSale.Repository/Ordering/IVoucherRepository.cs` — thêm UserVoucher queries
- `MoToSale.Repository/Ordering/VoucherRepository.cs` — implement
- `MoToSale.DTO/Ordering/OrderingDtos.cs` — thêm UserVoucherDto, ApplicableVouchersRequest

### 3.5 Orders & Payments (Bổ sung)
**File:** `v2/backend/src/MoToSale.APIService/Controllers/OrdersController.cs`

Cần thêm/sửa:
```
GET  /api/orders             → role-aware: customer = own orders, staff = search all
GET  /api/orders/{id}/payment-info → auth, thông tin thanh toán (bank transfer/QR)
POST /api/orders/shipping-quote    → auth, tính phí vận chuyển
POST /api/orders/{id}/request-refund → auth, yêu cầu hoàn tiền
PUT  /api/orders/{id}/cancel       → alias cho POST cancel (frontend dùng PUT)
```

**File:** `v2/backend/src/MoToSale.APIService/Controllers/PaymentsController.cs`

Cần thêm/sửa:
```
POST /api/payments           → mở cho customer (bỏ admin-only restriction)
POST /api/payments/{id}/confirm-success → auth, customer xác nhận đã chuyển khoản
```

**Files cần cập nhật:**
- `MoToSale.Services/Ordering/IOrderService.cs`
- `MoToSale.Services/Ordering/OrderService.cs`
- `MoToSale.Services/Payments/IPaymentService.cs`
- `MoToSale.Services/Payments/PaymentService.cs`
- `MoToSale.DTO/Ordering/OrderingDtos.cs`
- `MoToSale.DTO/Payments/PaymentDtos.cs`

### 3.6 Cart (Bổ sung)
**File:** `v2/backend/src/MoToSale.APIService/Controllers/CartController.cs`

Cần thêm:
```
GET    /api/cart/count  → auth, số lượng item trong giỏ
DELETE /api/cart/clear  → auth, xóa toàn bộ giỏ hàng
```

### 3.7 Content (URL Aliases)
**File:** `v2/backend/src/MoToSale.APIService/Controllers/ContentController.cs`

Cần thêm:
```
GET  /api/content/blog-posts       → public, alias cho /content/posts (admin) 
GET  /api/content/faqs             → alias thêm cho /content/faq (đã có)
POST /api/content/contact-requests → public, gửi form liên hệ
GET  /api/content/vouchers/{code}  → public, tra cứu voucher theo code
```

---

## Phase 4: Business Logic ⏳

### Shipping Quote
**v2 target:** `MoToSale.Services/Ordering/OrderService.cs`
- Phí cố định 30.000đ nếu giao hàng tại nhà
- 0đ nếu nhận tại cửa hàng (ReceivingMethod = "AtStore")
- Có thể mở rộng theo tỉnh/thành sau

### Inventory Reservation
Đã có trong v2 OrderService (`CheckoutAsync` dùng `IReservationRepository`). Cần xác nhận logic release hold khi hủy đơn.

### Refund Request
**v2 target:** `MoToSale.Services/Ordering/OrderService.cs`
- Tạo `RefundRequest` entity khi customer yêu cầu
- Order phải ở trạng thái phù hợp (Confirmed/Delivered)

### Payment Info
**v2 target:** `MoToSale.Services/Payments/PaymentService.cs` hoặc `OrderService.cs`
- Trả về thông tin chuyển khoản (STK, ngân hàng, nội dung CK)
- Lấy từ `Settings` table hoặc cấu hình cứng trong `appsettings.json`

---

## Phase 5: Database Migration ⏳

### Entities chưa có trong migration (cần tạo mới)
1. **`Favorites`** — `(UserId, ProductId)`, unique index
2. **`UserVouchers`** — `(UserId, VoucherId, VoucherStatus, SavedAt)`
3. **`PasswordResetTokens`** — `(UserId, TokenHash, ExpiresAt, UsedAt)`

**Lệnh tạo migration:**
```bash
cd v2/backend/src
dotnet ef migrations add AddCustomerFeatures --project MoToSale.Repository --startup-project MoToSale.APIService
dotnet ef database update --project MoToSale.Repository --startup-project MoToSale.APIService
```

> Lưu ý: AuthService cũng dùng cùng AppDbContext và DB, nên migration chạy 1 lần là đủ.

---

## Phase 6: Configuration ⏳

**`v2/backend/src/MoToSale.APIService/appsettings.json`**
- Kiểm tra `AllowedOrigins` có bao gồm `http://localhost:5174` (customer frontend port)

**`v2/backend/src/MoToSale.AuthService/appsettings.json`**
- Kiểm tra CORS config

---

## Verification

### Chạy v2 backend
```bash
# Terminal 1 — API Gateway (port 5000)
cd v2/backend/src/MoToSale.ApiGateway && dotnet run

# Terminal 2 — Auth Service (port 5101)
cd v2/backend/src/MoToSale.AuthService && dotnet run

# Terminal 3 — API Service (port 5102)
cd v2/backend/src/MoToSale.APIService && dotnet run
```

### Chạy customer frontend
```bash
cd frontend && npm run dev   # port 5174, proxy /api → localhost:5000
```

### Test end-to-end
1. `GET /api/categories` → danh sách categories
2. `GET /api/products` → danh sách sản phẩm
3. `POST /api/auth/register` → đăng ký tài khoản
4. `POST /api/auth/login` → nhận JWT
5. `POST /api/cart/items` → thêm vào giỏ
6. `GET /api/cart/count` → đếm số item
7. `POST /api/orders/shipping-quote` → tính phí ship
8. `POST /api/orders` → tạo đơn hàng
9. `POST /api/payments` → tạo payment
10. `POST /api/payments/{id}/confirm-success` → customer xác nhận đã chuyển khoản

### Checklist endpoint coverage  (✅ tất cả đã triển khai & build/smoke-test OK 2026-06-07)
- [x] Auth: login, register
- [x] Auth: forgot-password, reset-password (PasswordResetTokens đã có trong migration AddCustomerFeatures)
- [x] Products: list, detail
- [x] Products: filters, reviews, review-summary
- [x] Categories: public list (via CatalogLookupController)
- [x] Cart: add, update, remove
- [x] Cart: count, clear
- [x] Orders: create (checkout), detail, cancel (POST), search (admin)
- [x] Orders: list-mine as `GET /orders`, payment-info, shipping-quote, refund-request, cancel (PUT)
- [x] Payments: get-by-order
- [x] Payments: create (for customer), confirm-success
- [x] Vouchers: validate, admin CRUD
- [x] Vouchers: public list, applicable, save, my-list, my-count
- [x] Favorites: list, add, remove
- [x] Reviews: by-product, summary, my-review, create, update-mine
- [x] Users: me, update-profile, password, add-address, list-addresses
- [x] Users: update-address, delete-address, set-default-address, get-default-address
- [x] Content: faq, home-banners
- [x] Content: blog-posts (public), contact-requests (public), faqs alias, voucher lookup

### Ghi chú phiên 2026-06-07 (tiếp tục triển khai)
- **Sửa lỗi build:** `CatalogService.MapListItem`/`MapDetail` chưa cập nhật theo record `ProductListItem`/`ProductDetail` đã mở rộng các trường tương thích frontend cũ (`productCode`, `categoryName`, `brandName`, `carModelName`, `productType`, `basePrice`, `stockQuantity`, `averageRating`, `totalReviews`, `variants`). Đã bổ sung helper `NameMapsAsync` (lookup tên category/brand/model) và `ReviewAggAsync` (điểm TB + số lượng review đã duyệt), và map biến thể (SKU) kèm tồn kho.
- **Sửa lỗi seed:** `SeedConfiguration.AddMissingOperationalDataAsync` tham chiếu SKU `SP-VISION-DEFAULT` không tồn tại → đổi thành `SP-VISION-TC` (biến thể tiêu chuẩn). Trước đó lỗi này làm APIService crash khi khởi động.
- **Verify:** build toàn solution sạch (0 warning/error), migration đã áp dụng (DB up-to-date), chạy 3 service (Gateway 5000 / Auth 5101 / API 5102) và smoke-test qua gateway: categories/products/brands/blog-posts/faqs (200), login → cart/count, orders, favorites, vouchers/my (200), shipping-quote Delivery=30.000đ & AtStore=0đ, product detail/reviews/summary (200).
- **Còn lại (không chặn):** 2 cảnh báo EF precision decimal (`RefundRequest.Amount`, `InstallmentTerm.TotalAmount`) — chỉ là warning, có thể thêm `HasPrecision` nếu cần độ chính xác tiền tệ.
