# Đánh giá & Kế hoạch tích hợp Storefront FE với Backend v2

Phiên bản: 1.0 · Ngày: 04/06/2026
FE nguồn: `D:\MotorTeam\MoToSale-End\Frontend` (customer storefront `frontend-user-react`) → đã copy vào **`v2/frontend-store`**.
Mục tiêu: làm FE khách hàng chạy được đầy đủ với BE v2 (Gateway 5100 · Auth 5101 · API 5102).

---

## 1. Bối cảnh

- `frontend-store` là web **bán hàng cho khách** (Home, Sản phẩm, Giỏ, Thanh toán, Đơn của tôi, Tài khoản, Yêu thích, Voucher, Hệ thống cửa hàng), trước đây trỏ về **BE v1 (port 5000)** với schema field tiếng Việt.
- FE đã có sẵn lớp **adapter** (`api.js`, `productMappers.js`) đọc cả khóa tiếng Việt lẫn tiếng Anh → phần lớn **đọc dữ liệu English-schema của v2 vẫn hoạt động**.
- BE v2 hiện **thiên về quản trị (admin)**: nhiều endpoint storefront hoặc bị khóa quyền Admin/Staff, đổi tên/đổi method, hoặc **chưa có**.

Khác `v2/frontend-admin`: admin dùng token key `admin_token`; storefront dùng `token` → độc lập, cùng JWT từ AuthService, không xung đột.

---

## 2. Ma trận đối chiếu endpoint (FE cần ↔ BE v2)

Ký hiệu: ✅ dùng được · ⚠️ lệch nhẹ (sửa FE/alias) · 🔒 bị khóa quyền · ❌ thiếu hẳn.

| FE gọi (prefix `/api`) | Trạng thái BE v2 | Ghi chú |
|---|---|---|
| POST `/auth/login`, `/auth/register` | ✅ | có sẵn ở AuthService |
| GET/PUT `/users/me`, PUT `/users/me/password` | ✅ | có |
| GET/PUT `/users/me/address` | ⚠️ | v2 = `/users/me/addresses` (số nhiều): GET trả **list**, thêm là **POST**. Cần sửa FE |
| GET `/products`, `/products/{id}` | ✅ | **public**; cần khớp tham số query (search/category/sort/paging) & shape |
| GET `/products/filters` | ❌ | chưa có; FE dựng bộ lọc từ đây |
| GET `/products/{id}/reviews`, `/reviews/summary` | ❌ | v2 chỉ có `api/reviews` (Admin) — chưa có đọc review công khai |
| GET `/reviews/product/{id}/me`; POST `/products/{id}/reviews`; PATCH `/products/{id}/reviews/me` | ❌ | khách xem/gửi/sửa đánh giá — chưa có |
| GET `/categories` | ✅ | public (CatalogLookupController) |
| GET `/cart`; POST `/cart/items`; PUT/DELETE `/cart/items/{id}` | ✅ | yêu cầu đăng nhập (`[Authorize]`) |
| GET `/cart/count` | ❌ | thiếu — có thể suy từ `/cart` ở FE |
| DELETE `/cart/clear` | ❌ | thiếu — hoặc xóa lần lượt ở FE |
| GET `/orders` (đơn của tôi) | ⚠️🔒 | v2 `GET /orders` là **Admin/Staff**; đơn của khách = **`GET /orders/mine`**. Sửa FE gọi `/mine` |
| GET `/orders/{id}` | ✅ | có kiểm tra quyền sở hữu |
| POST `/orders` (đặt hàng) | ✅ | = `Checkout(CheckoutRequest)` cho user hiện tại; cần **map payload FE → CheckoutRequest** |
| PUT `/orders/{id}/cancel` | ⚠️ | v2 = **POST** `/orders/{id}/cancel`. Sửa method ở FE |
| GET `/payments/order/{id}`; POST `/payments`; POST `/payments/{id}/confirm-success` | 🔒❌ | payments v2 **chỉ Admin/Staff**; `confirm-success` chưa có. Cần quyết định mô hình thanh toán |
| GET `/vouchers` (công khai) | 🔒 | v2 list voucher là Admin/Staff |
| POST `/vouchers/validate` | ✅ | `[Authorize]` (mọi user đăng nhập) |
| POST `/vouchers/applicable`, `/vouchers/save`; GET `/vouchers/my`, `/vouchers/my/count`; GET `/content/vouchers/{code}` | ❌ | self-service voucher cho khách — chưa có |
| GET `/content/home-banners` | ✅ | public |
| GET `/content/blog-posts` | ⚠️🔒 | v2 = `/content/posts` và GET đang **Staff-only** |
| GET `/content/faqs` | ⚠️ | v2 = `/content/faq` (public) — chỉ lệch tên |
| POST `/content/contact-requests` | ❌ | v2 chưa có POST công khai (chỉ GET/`process` cho Staff) |
| GET `/favorites`; POST/DELETE `/favorites/{id}` | ❌ | **toàn bộ tính năng Yêu thích chưa có** ở v2 (không có entity/controller) |
| GET `/showrooms` | ❌ | mô hình **1 cửa hàng**, không có khái niệm showroom |

### Tổng kết mức độ phù hợp
- **Chạy được ngay / gần như ngay (lõi thương mại):** đăng nhập/đăng ký, danh mục, danh sách & chi tiết sản phẩm, giỏ hàng (thêm/sửa/xóa), **đặt hàng + xem đơn của tôi + chi tiết đơn**, hồ sơ người dùng, validate voucher.
- **Lệch nhẹ — sửa ở FE (nửa ngày):** đơn của tôi (`/mine`), hủy đơn (POST), địa chỉ (`addresses`), tên content (`posts`/`faq`), đếm/xóa giỏ.
- **Thiếu ở BE — cần bổ sung hoặc cắt tính năng:** Yêu thích, đánh giá công khai (đọc/gửi), voucher self-service, gửi liên hệ công khai, blog public, bộ lọc sản phẩm, showroom, luồng thanh toán khách.

→ **Khoảng 60–65%** storefront dùng lại được với chỉnh nhỏ; **35–40%** phụ thuộc bổ sung BE hoặc quyết định cắt giảm phạm vi.

---

## 3. Các quyết định cần chốt trước khi làm

1. **Thanh toán khi đặt hàng:** v2 không có cổng thanh toán; payments do Staff ghi nhận.
   - **(A – khuyến nghị)** Storefront đặt hàng theo **COD / thanh toán tại cửa hàng** → bỏ bước thanh toán online & `confirm-success`. Đơn tạo ở trạng thái *Chờ thanh toán*, nhân viên xử lý ở admin.
   - (B) Bổ sung endpoint cho khách tự ghi nhận đã chuyển khoản (giả lập) → thêm việc ở BE.
2. **Yêu thích (Favorites):** (A) Bổ sung entity + controller `api/favorites` · hay (B) **ẩn trang Yêu thích** ở storefront.
3. **Đánh giá sản phẩm (Reviews):** (A) Thêm endpoint public đọc + khách gửi/sửa · hay (B) chỉ hiển thị đánh giá tĩnh/ẩn.
4. **Voucher self-service & showroom:** (A) bổ sung BE · hay (B) đơn giản hóa (chỉ nhập mã + validate; trang "Hệ thống cửa hàng" hiển thị 1 cửa hàng từ Cấu hình).

> Khuyến nghị tổng: chọn **A cho Favorites + Reviews** (giá trị cao, chi phí vừa) và **COD + voucher-nhập-mã + 1 cửa hàng** để giảm phạm vi BE.

### ✅ Phạm vi đã chốt (04/06/2026)
1. **Thanh toán = COD / tại cửa hàng.** Bỏ thanh toán online + `confirm-success`; đơn tạo ở *Chờ thanh toán*, nhân viên xử lý ở admin.
2. **Favorites = bổ sung BE.** Thêm entity `Favorite` + migration + `FavoritesController`.
3. **Reviews = bổ sung BE public.** Đọc review công khai + khách gửi/sửa; chỉ hiển thị review đã duyệt (`Approved`).
4. **Voucher/Showroom = đơn giản hóa.** Chỉ nhập mã + `validate`; trang Hệ thống cửa hàng hiển thị **1 cửa hàng từ Settings**. Bỏ ví voucher (`applicable/save/my`) và `showrooms` đa điểm.

→ Pha 2 thu gọn còn: **Reviews public + Favorites + `POST /content/contact-requests` + `GET /content/posts` public + `/showrooms` (1 cửa hàng từ Settings)**; tiện ích `cart/count`,`cart/clear`,`products/filters` xử lý phía FE (không bắt buộc thêm BE).

---

## 4. Kế hoạch thực hiện (theo pha)

### Pha 0 — Wiring & chạy được khung (S)
- `vite.config.js`: đổi proxy `/api`,`/uploads` → **`http://localhost:5100`** (đang 5000); chọn port dev riêng (vd 5175) để không đụng admin (5176).
- Kiểm tra `.env.example`/`VITE_API_BASE_URL`; `npm install`; `npm run dev`; xác nhận trang Home gọi `/content/home-banners`, `/products` OK.
- Xác minh shape đăng nhập v2 (token + user) khớp `AuthContext`.

### Pha 1 — Sửa FE cho khớp đường dẫn/method/tên (M, chỉ FE)
- Đơn của tôi: `GET /orders` → **`GET /orders/mine`** (đọc `{items}`).
- Hủy đơn: `PUT /orders/{id}/cancel` → **`POST`**.
- Địa chỉ: `GET/PUT /users/me/address` → **`GET /users/me/addresses`** (lấy phần tử đầu) + **`POST`** khi lưu.
- Content: `/content/blog-posts` → `/content/posts`; `/content/faqs` → `/content/faq`.
- Giỏ: tự tính `count` từ `/cart`; `clear` = xóa lần lượt item (hoặc chờ Pha 2 nếu thêm BE).
- **Map checkout**: đối chiếu payload đặt hàng của FE với `CheckoutRequest` của v2 (người nhận, SĐT, email, địa chỉ, phương thức nhận, danh sách item theo `skuId`/`qty`, mã voucher) — sửa cho khớp.
- Bỏ `showroomId` khỏi payload (mô hình 1 kho).

### Pha 2 — Bổ sung BE v2 cho storefront (M–L, theo quyết định mục 3)
- **Reviews public** (nếu A): thêm vào `ProductsController`/`ReviewsController` các endpoint **không cần quyền** `GET /products/{id}/reviews`, `/reviews/summary`, và endpoint cho khách `GET /reviews/product/{id}/me`, `POST /products/{id}/reviews`, `PATCH /products/{id}/reviews/me` (chỉ duyệt hiển thị review `Approved`).
- **Favorites** (nếu A): entity `Favorite(UserId, ProductId)` + migration + `FavoritesController` (`GET /favorites`, `POST/DELETE /favorites/{productId}`).
- **Content công khai**: mở `GET /content/posts` (bỏ Staff) hoặc thêm `GET /content/posts/public`; thêm **`POST /content/contact-requests`** ẩn danh.
- **Voucher**: thêm `GET /content/vouchers/{code}` (xem theo mã) ; (tùy) `applicable/save/my` nếu giữ tính năng "ví voucher".
- **Giỏ**: thêm `GET /cart/count`, `DELETE /cart/clear` (tiện, không bắt buộc).
- **Products filters**: thêm `GET /products/filters` (trả danh mục/hãng/khoảng giá) — hoặc FE tự dựng từ `/categories` + `/brands`.
- **Showroom 1 cửa hàng**: `GET /showrooms` trả về 1 bản ghi từ **Settings** (tên/địa chỉ/SĐT/giờ mở) — hoặc ẩn trang.
- Thêm **route Gateway** nếu phát sinh prefix mới (mặc định `/api/*` đã chuyển 5102 nên thường không cần).

### Pha 3 — Kiểm thử luồng storefront E2E (S–M)
- Khách: đăng ký → đăng nhập → duyệt sản phẩm → thêm giỏ → đặt hàng (COD) → xem "Đơn của tôi" → chi tiết → hủy khi cho phép.
- Tài khoản: sửa hồ sơ, đổi mật khẩu, địa chỉ.
- (Nếu làm) Yêu thích, gửi đánh giá, nhập voucher.
- `npm run build` storefront; kiểm tra không lỗi console/4xx bất thường; đối chiếu dữ liệu với admin.

---

## 5. Ước lượng & rủi ro

| Pha | Phạm vi | Ước lượng |
|---|---|---|
| 0 | Wiring | ~0.5 buổi |
| 1 | Sửa FE path/method/map checkout | ~1 buổi |
| 2 | Bổ sung BE (Reviews+Favorites+Content+Voucher-code+Showroom) | ~2–3 buổi (tùy chọn A/B) |
| 3 | E2E + build | ~0.5–1 buổi |

**Rủi ro chính:** (1) shape `CheckoutRequest` lệch payload FE → lỗi đặt hàng (xử lý ở Pha 1); (2) cart yêu cầu `[Authorize]` → cần đăng nhập mới thêm giỏ (FE có thể cần guest-cart phía client nếu muốn mua không đăng nhập); (3) thêm entity Favorites/Review-public kéo theo migration DB.

---

## 6. Đề xuất bước kế tiếp
Chốt 4 quyết định ở **mục 3**. Sau đó mình chạy **Pha 0 + Pha 1** ngay (đưa luồng duyệt–giỏ–đặt hàng–xem đơn chạy được), rồi làm **Pha 2** theo phạm vi đã chốt.
