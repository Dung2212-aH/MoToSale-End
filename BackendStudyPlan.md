# Backend Study Plan

## Mục tiêu
Tài liệu này giúp bạn học hệ thống backend theo luồng từ cơ bản tới cặn kẽ, với các phase rõ ràng và các file cần mở/chép tay.

---

## Phase 0 — Môi trường & chạy local
1. Mở `Backend/ApiGateway/Program.cs` và `Backend/ApiGateway/ocelot.json`
2. Xem các port và service đang cấu hình:
   - Gateway: `http://localhost:5000`
   - AuthService: `http://localhost:5001`
   - CatalogService: `http://localhost:5002`
   - OrderService: `http://localhost:5003`
   - PaymentService: `http://localhost:5004`
3. Kiểm tra cấu hình JWT và connection string trong từng `appsettings*.json` của các service.
4. Chạy từng project nếu cần để xác nhận health endpoints.

---

## Phase 1 — Deep-dive ApiGateway
1. `Backend/ApiGateway/Program.cs`
   - Mục đích: entry point của gateway, load Ocelot, cấu hình CORS, Swagger.
   - Nội dung chính: `builder.Configuration.AddJsonFile("ocelot.json")`, `builder.Services.AddOcelot()`, `app.UseOcelot()`.
2. `Backend/ApiGateway/ocelot.json`
   - Mục đích: định tuyến request từ frontend đến service tương ứng.
   - Nội dung chính:
     - `/api/auth/*`, `/api/users/*` -> AuthService
     - `/api/products/*`, `/api/categories/*`, `/api/favorites/*`, `/uploads/*`, `/api/reviews/*`, `/api/brands/*`, `/api/models/*`, `/api/inventory/*`, `/api/content/*` -> CatalogService
     - `/api/cart/*`, `/api/orders/*`, `/api/vouchers/*` -> OrderService
     - `/api/payments/*` -> PaymentService
     - health endpoints `/health/auth`, `/health/catalog`, `/health/orders`, `/health/payments`
3. Viết tay: định nghĩa route từ upstream tới downstream.

---

## Phase 2 — Deep-dive AuthService
1. `Backend/AuthService/Program.cs`
   - Mục đích: cấu hình EF Core, JWT authentication, DI và controller mapping.
2. `Backend/AuthService/Controllers/AuthController.cs`
   - Mục đích: register/login/token, endpoint `api/auth/register`, `api/auth/login`, `api/auth/me`, `api/auth/logout`.
3. `Backend/AuthService/Controllers/UsersController.cs`
   - Mục đích: quản lý người dùng, profile, address, thay đổi mật khẩu, quản trị người dùng.
4. Tiếp tục mở và chép tay:
   - `Backend/AuthService/Data/AuthDbContext.cs`
   - `Backend/AuthService/Services/*`
   - `Backend/AuthService/Repositories/*`
   - `Backend/AuthService/Entities/*.cs` và `DTOs/*`

---

## Phase 3 — Deep-dive CatalogService
1. `Backend/CatalogService/Program.cs`
   - Mục đích: cấu hình CatalogDbContext, JWT, DI cho repository/service, phục vụ static files.
2. `Backend/CatalogService/Controllers/ProductsController.cs`
   - Mục đích: product CRUD, variants, images, filter, review summary.
3. `Backend/CatalogService/Controllers/CategoriesController.cs`
   - Mục đích: category CRUD.
4. `Backend/CatalogService/Controllers/BrandsController.cs`
   - Mục đích: brand CRUD.
5. `Backend/CatalogService/Controllers/ReviewsController.cs`
   - Mục đích: review CRUD và tổng hợp đánh giá.
6. Các file hỗ trợ:
   - `Backend/CatalogService/Data/CatalogDbContext.cs`
   - `Backend/CatalogService/Repositories/*`
   - `Backend/CatalogService/Services/*`
   - `Backend/CatalogService/Entities/*.cs`, `DTOs/*.cs`

---

## Phase 4 — Deep-dive OrderService
1. `Backend/OrderService/Program.cs`
   - Mục đích: cấu hình OrderDbContext, JWT, DI cho order service.
2. `Backend/OrderService/Controllers/CartController.cs`
   - Mục đích: quản lý giỏ hàng.
3. `Backend/OrderService/Controllers/OrdersController.cs`
   - Mục đích: tạo đơn, lấy đơn, hủy đơn, cập nhật trạng thái.
4. `Backend/OrderService/Controllers/VouchersController.cs`
   - Mục đích: quản lý voucher.
5. Hỗ trợ:
   - `Backend/OrderService/Data/OrderDbContext.cs`
   - `Backend/OrderService/Services/*`
   - `Backend/OrderService/Repositories/*`

---

## Phase 5 — Deep-dive PaymentService
1. `Backend/PaymentService/Program.cs`
   - Mục đích: cấu hình PaymentDbContext, JWT, DI cho payment service.
2. `Backend/PaymentService/Controllers/PaymentsController.cs`
   - Mục đích: quản lý payment, confirm, cancel, tổng hợp thanh toán theo order.
3. Hỗ trợ:
   - `Backend/PaymentService/Data/PaymentDbContext.cs`
   - `Backend/PaymentService/Services/*`
   - `Backend/PaymentService/Repositories/*`

---

## Phase 6 — DB schema & DbBootstrap
1. `Backend/DbBootstrap/Program.cs`
   - Mục đích: tạo schema bằng EF Core cho Auth/Catalog/Order/Payment.
   - Nội dung chính: `EnsureCreated`, generate create script, drop partial order tables.
2. `Backend/database/ShowroomDB.sql`
   - Mục đích: schema canonical cho các bảng chính.
3. `Backend/database/20260518_ProductReviews_Upgrade.sql`
   - Mục đích: nâng cấp schema liên quan reviews.
4. `Backend/database/20260521_OrderPaymentStatus_Cancelled.sql`
   - Mục đích: cập nhật trạng thái thanh toán/hủy đơn.

---

## Phase 7 — Luồng tích hợp (flow)
1. User login và lấy JWT:
   - `POST /api/auth/login` -> AuthService -> token
2. Load dữ liệu sản phẩm:
   - `GET /api/products` -> CatalogService
3. Thêm vào giỏ và tạo đơn:
   - `POST /api/cart`/`PUT /api/cart/...` -> OrderService
   - `POST /api/orders` -> OrderService
4. Thanh toán:
   - `POST /api/payments` -> PaymentService
   - `PATCH /api/payments/{id}/confirm` -> PaymentService
5. Health check:
   - `/health/auth`, `/health/catalog`, `/health/orders`, `/health/payments`

---

## Phase 8 — Hands-on
1. Mở và chép tay từng file theo phase.
2. Sau đó thực hành một luồng:
   - login, lấy token
   - lấy products
   - tạo order
   - tạo payment
3. Nếu muốn, thêm comment trực tiếp trong mã khi chép tay.

---

## Lộ trình đề xuất để chép tay
1. `Backend/ApiGateway/Program.cs`
2. `Backend/ApiGateway/ocelot.json`
3. `Backend/AuthService/Program.cs`
4. `Backend/AuthService/Controllers/AuthController.cs`
5. `Backend/AuthService/Controllers/UsersController.cs`
6. `Backend/CatalogService/Program.cs`
7. `Backend/CatalogService/Controllers/ProductsController.cs`
8. `Backend/OrderService/Program.cs`
9. `Backend/OrderService/Controllers/OrdersController.cs`
10. `Backend/PaymentService/Program.cs`
11. `Backend/PaymentService/Controllers/PaymentsController.cs`
12. `Backend/DbBootstrap/Program.cs`
13. `Backend/database/ShowroomDB.sql`

---

## Ghi chú
- Nếu bạn muốn, tôi có thể tiếp tục bằng cách tạo thêm: `Backend/StudyChecklist.md` với checklist cho mỗi file.
- Hoặc tôi có thể tạo thêm `Backend/LearningFlow.md` với sơ đồ đường đi cụ thể cho một request từ frontend.
