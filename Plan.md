# Plan: Manual UI Test cho Frontend (Customer) và FrontendAdmin

## Context

ShowroomDB gồm hai SPA: [Frontend](Frontend/) (khách hàng, port 5174) và [FrontendAdmin](FrontendAdmin/) (quản trị, port 5175), cùng gọi API Gateway tại `http://localhost:5000` (proxy tới 4 service: Auth 5001, Catalog 5002, Order 5003, Payment 5004). Trước khi release/demo cần một đợt rà soát tay toàn diện để phát hiện **lỗi logic** (sai luồng, sai dữ liệu, sai trạng thái sau thao tác) và **lỗi giao diện** (layout vỡ, text tràn, modal sai, console error, ảnh broken, responsive lỗi). Kế hoạch này mô tả cách dùng MCP `Claude_in_Chrome` để Claude tự đóng vai người dùng thật, đi qua mọi màn hình của cả hai SPA, ghi nhận lỗi, rồi tổng kết ra một file báo cáo duy nhất.

**Tiền đề:** Backend (5 service) đã chạy sẵn. Người dùng sẽ tự start hai Vite dev server (`npm run dev` trong `Frontend/` và `FrontendAdmin/`) trước khi bắt đầu thực thi plan, HOẶC plan sẽ start chúng background nếu chưa lên.

**Kết quả mong đợi:** Một file duy nhất `D:\ShowRoomDB\UI_TEST_REPORT.md` liệt kê toàn bộ lỗi đã phát hiện, phân loại theo SPA / màn hình / mức độ, kèm bằng chứng (screenshot, console log, API response).

## Phạm vi

- **Frontend** (khách): 17 route công khai/bảo vệ — Home, Products list/detail, Cart, Checkout (3 bước), Favorites, Orders list/detail, Account (Profile/Password/Address), Login/Register/Forgot, Vouchers, Contact, FAQ, 404.
- **FrontendAdmin**: 28 route — Dashboard, Motorcycles, Parts, Categories, Brands, Orders list/detail, Vouchers, Inventory, StockDocuments, AdvancedOperations, BusinessOperations, OperationalImports, Users, Customers, Warranties, Reviews, Posts, FAQ, Contacts, HomeBanners, Reports, AuditLogs, Settings/Payment.
- **Ngoài phạm vi:** sửa code, viết test tự động, kiểm thử backend qua Swagger, test bảo mật/penetration (chỉ ghi nhận nếu phát hiện tình cờ).

## Công cụ sử dụng

| Mục đích | Tool MCP |
|---|---|
| Mở/điều hướng trang | `mcp__Claude_in_Chrome__navigate`, `tabs_create_mcp`, `tabs_close_mcp` |
| Click, gõ form | `mcp__Claude_in_Chrome__computer` (click/type), `form_input`, `file_upload` |
| Đọc DOM/text | `mcp__Claude_in_Chrome__get_page_text`, `read_page`, `find` |
| Bắt lỗi runtime | `mcp__Claude_in_Chrome__read_console_messages` |
| Theo dõi API | `mcp__Claude_in_Chrome__read_network_requests` |
| Bằng chứng | `mcp__Claude_in_Chrome__gif_creator` hoặc screenshot qua `computer` |
| Test responsive | `mcp__Claude_in_Chrome__resize_window` (desktop 1440, tablet 768, mobile 390) |

## Tài khoản test

- **Admin có sẵn:** `admin123@gmail.com` / `dung123` (từ `FrontendAdmin/TEST_PLAN.md`).
- **Customer:** đăng ký mới `qa_customer_<timestamp>@test.local` ngay trong test case Auth; lưu credential để dùng tiếp ở các flow protected.
- Nếu seed thiếu sản phẩm/voucher để test, ghi nhận làm **Blocker** trong báo cáo, không tự tạo qua Admin để khỏi nhiễu (trừ khi cần unblock flow Customer).

## Quy trình thực hiện

### Bước 1 — Tiền kiểm tra (5 phút)

1. `mcp__Claude_in_Chrome__list_connected_browsers` để xác nhận có Chrome khả dụng.
2. Curl health gateway: `GET http://localhost:5000/health/auth | /catalog | /orders | /payments` → mọi endpoint phải `200`. Nếu fail, dừng và báo người dùng.
3. Truy cập `http://localhost:5174` và `http://localhost:5175/login` để xác nhận hai Vite dev server đã sống.
4. Tạo file rỗng `D:\ShowRoomDB\UI_TEST_REPORT.md` với khung mục lục (xem template ở mục "Định dạng báo cáo" bên dưới).

### Bước 2 — Test Frontend (khách hàng)

Thứ tự test bám theo journey thực tế của user.

**A. Auth & Account**
- Register account mới (validate password ≥6, phone `0XXXXXXXXX`, email format).
- Login với account vừa tạo; check redirect đúng nếu có `?redirect=`.
- Forgot password (kiểm tra UI flow, không yêu cầu gửi mail thật).
- `/account`: đổi tên / đổi password / thêm-sửa-xoá địa chỉ, set default.
- Logout, check protected route bị bật ngược về `/login`.

**B. Catalog browsing**
- Home: banner, danh mục nổi bật, deal/bestseller, broken image, click qua detail.
- `/products` với filter (category, brand, price range, sort), pagination, multi-category alias (scooter, sport bike, underbone, accessories).
- `/products/:id`: gallery, chọn version/color variant, kiểm tra stock, related products, reviews tab.
- Favorites: thêm/bỏ wishlist từ grid và detail; mở `/favorites` xác nhận đồng bộ.

**C. Cart & Checkout (3 nhánh thanh toán)**
- Thêm 2-3 sản phẩm vào cart, cập nhật số lượng, xoá item, tính subtotal.
- Checkout — **Full payment**: chọn Delivery + Bank/MoMo/VNPay, áp voucher hợp lệ + voucher hết hạn, ghi nhận shipping fee theo tỉnh.
- Checkout — **Deposit** (≥20%): kiểm tra UI tính số tiền cọc, order sau khi tạo có flag "pending deposit" ở Home.
- Checkout — **Installment** (≥30% down, kỳ 6/9/12 tháng): điền form personal + employment + CCCD 9-15 chữ số; check validation lỗi khi sai field.
- Pickup method: chọn appointment date, không yêu cầu address.
- Trang `/checkout/payment`: hiển thị QR/thông tin, polling chuyển sang `/checkout/success` (mock confirm qua Admin nếu cần).

**D. Post-purchase**
- `/orders` list, filter theo status, mở `/orders/:id` xem chi tiết, click "Pay Remaining" cho đơn deposit.
- Viết review cho item đã fulfilled (rating + text), check review xuất hiện ở product detail.

**E. Static / phụ trợ**
- `/vouchers`, `/contact` (submit form), `/faq`, route không tồn tại → `/404`.

**F. Cross-cutting** (chạy song song A-E)
- Sau mỗi màn, dump `read_console_messages` lọc `error|warning`, dump `read_network_requests` lọc status ≥400.
- Test responsive: lặp lại Home, Product detail, Checkout, Cart ở viewport 768 và 390.

### Bước 3 — Test FrontendAdmin

Đăng nhập `admin123@gmail.com / dung123`. Bám theo `FrontendAdmin/TEST_PLAN.md` như baseline rồi mở rộng workflow chain.

**A. Dashboard & Reports**
- Số liệu Dashboard (products/orders/revenue), revenue chart, top products, recent orders.
- `/reports` đổi date range, kiểm tra 3 chart (line, doughnut, bar) + summary boxes.

**B. Catalog CRUD**
- `/motorcycles`: tạo mới với variant + image + compatibility + promotion → save → reload → xác nhận persisted.
- `/parts`: CRUD tương tự, kiểm tra compatibility với motorcycle.
- `/categories`: tạo hierarchical parent-child, xoá category còn product → kiểm tra cảnh báo.
- `/brands`: tab Hãng xe / Dòng xe.
- `/home-banners`: upload ảnh, set order, xác nhận hiển thị ngược lại trên Frontend Home.

**C. Order lifecycle**
- `/orders`: filter theo status/date, search theo order ID.
- Mở đơn vừa tạo từ phía Customer ở Bước 2 → cập nhật trạng thái Pending → Confirmed → Shipped → Delivered, kiểm tra inventory hold giải phóng đúng.
- Cancel một đơn khác với lý do, check refund/restock.
- `/advanced-operations`: return/exchange, công nợ installment, phân ca.
- `/business-operations`: POS quick sale.

**D. Inventory & stock**
- `/inventory`: xem stock per variant, nút sync, hold quantity.
- `/stock-documents`: tạo phiếu nhập, phiếu xuất, kiểm tra số tồn cập nhật.

**E. Voucher**
- Tạo voucher %, voucher fixed, voucher scope theo brand/category/product, voucher limit per-customer.
- Quay lại Frontend áp dụng voucher đó ở checkout → kiểm tra usage history tăng.

**F. Content & user**
- `/users` (Admin-only): CRUD user, đổi role, soft delete.
- `/customers`, `/warranties`, `/reviews` (approve/hide), `/posts` CRUD, `/faq`, `/contacts` (mark processed).

**G. Audit & settings**
- `/audit-logs`: filter, kiểm tra log của action vừa làm ở các bước trên.
- `/settings`, `/settings/payment`: xem & save config; không xoá cấu hình có sẵn.
- `/operational-imports`: thử import CSV mẫu nhỏ (1-2 dòng), check báo lỗi với file sai schema.

**H. Cross-cutting**
- Console + network dump sau mỗi module.
- Phân quyền: logout Admin, login bằng Staff (nếu seed có) hoặc tạo Staff để verify Admin-only routes bị chặn.
- Responsive admin ở 1280 và 1024 (admin thường không cần mobile, nhưng vẫn check sidebar collapse).

### Bước 4 — Tổng hợp báo cáo

Ghi vào `D:\ShowRoomDB\UI_TEST_REPORT.md` theo template dưới. Mỗi finding cần đủ: **module**, **route**, **bước reproduce**, **expected vs actual**, **bằng chứng** (screenshot path / console snippet / network response), **mức độ** (Blocker / Major / Minor / Cosmetic), **phân loại** (Logic / UI / Performance / Accessibility).

## Định dạng `UI_TEST_REPORT.md`

```md
# UI Test Report — ShowroomDB
- Ngày test: 2026-06-10
- Tester: Claude (qua MCP Claude_in_Chrome)
- Backend gateway: http://localhost:5000 (OK)
- Frontend: http://localhost:5174 — FrontendAdmin: http://localhost:5175

## Tổng quan
| SPA | Tổng case | Pass | Fail | Blocker | Major | Minor | Cosmetic |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Frontend | … | … | … | … | … | … | … |
| FrontendAdmin | … | … | … | … | … | … | … |

## Frontend (Customer)

### [Severity] [Logic|UI] Module — tiêu đề lỗi
- Route: `/products/:id`
- Repro: 1) … 2) … 3) …
- Expected: …
- Actual: …
- Bằng chứng: `reports/screenshots/frontend-product-detail-broken.png`, console: `TypeError: cannot read property 'price' of undefined`
- API liên quan: `GET /api/products/123` trả 200 nhưng thiếu field `variants[].price`
- Ghi chú: (gợi ý hướng fix nếu rõ; không bắt buộc)

## FrontendAdmin
… (cùng cấu trúc)

## Phụ lục
- Console errors gộp theo route
- Network 4xx/5xx gộp theo endpoint
- Danh sách màn KHÔNG test được và lý do (thiếu data / endpoint 404 / blocker khác)
```

Screenshot lưu ở `D:\ShowRoomDB\reports\screenshots\` (tạo thư mục khi chạy). Trong báo cáo dùng đường dẫn tương đối.

## Verification (cách kiểm tra plan đã chạy đúng)

1. `D:\ShowRoomDB\UI_TEST_REPORT.md` tồn tại và có cả hai section Frontend + FrontendAdmin.
2. Mỗi route trong "Phạm vi" xuất hiện ít nhất một lần trong báo cáo (Pass hoặc Fail) — không bỏ sót.
3. Mỗi finding Fail có đủ 6 trường (module, route, repro, expected, actual, evidence).
4. Bảng tổng quan ở đầu file khớp số lượng finding bên dưới.
5. Mọi screenshot tham chiếu trong báo cáo phải tồn tại trên đĩa.

## Rủi ro & xử lý

- **Backend chết giữa chừng**: phát hiện qua healthcheck mỗi 30 phút; nếu chết, ghi nhận thời điểm + dừng test, báo người dùng restart.
- **Data nhiễu giữa hai SPA**: dùng prefix `qa_` cho mọi entity tự tạo để dễ dọn sau.
- **Test customer Checkout không tự confirm được payment thật**: dùng Admin để force-confirm đơn (nếu có chức năng) hoặc đánh dấu finding "không reproduce được past payment step" — không skip nhánh.
- **Token Admin hết hạn (401)**: re-login, không tự đổi config token.
- **Browser crash**: dùng `list_connected_browsers` + `select_browser` để gắn lại, screenshot trước đó không mất.