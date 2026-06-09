# V2 Frontend Store - Full User Journey Test Report

- Test plan: `D:/MotorTeam/MoToSale-End/docs/V2_FRONTEND_STORE_FULL_USER_JOURNEY_TEST_SCENARIOS.md`
- Report time: 2026-06-04T18:17:23.620Z
- Run id: `20260604174344`
- Store FE: `http://localhost:5174`
- Admin FE used for cross-check: `http://localhost:5176`
- Backend gateway: `http://localhost:5100/api`

## 1. Executive Summary

Đã thực hiện vòng test đóng vai người dùng thật trên frontend-store, có dùng frontend-admin/API để chuẩn bị dữ liệu, đối chiếu đơn hàng, tồn kho, voucher và trạng thái thanh toán/giao hàng.

Kết quả tổng: **41 checks** = **33 PASS**, **5 WARN**, **3 FAIL**. Tổng lỗi ghi nhận: **13 findings** = **3 Critical**, **6 High**, **4 Medium**.

Kết luận: core flow mua hàng đã chạy được tới mức tạo đơn COD, tạo đơn chuyển khoản/đặt cọc, admin xác nhận thanh toán, ghi nhận phần còn lại, giao hàng và khách xem đơn. Tuy nhiên frontend-store **chưa nên coi là sẵn sàng production** vì còn lỗi critical về giá hiển thị, số lượng giỏ hàng và hợp đồng voucher/admin-voucher có thể làm sai tiền.

## 2. Test Data Created

- Customer mới: `store_e2e_20260604174344@motosale.local / Store@12345`; số điện thoại sau cập nhật: `0904174345`.
- Product test chính: product `10` - `Nhớt Motul 300V`; vehicle edge: product `14` - `Suzuki Raider R150`.
- Ảnh test upload cho product 10:
  - `/uploads/products/5d863b8120bc4b129d1676bcbe52f5f6.png`
  - `/uploads/products/95d1c40cb8db410c826cd9268c21d70c.png`
- Voucher đúng contract dùng retest: `STOREAMT20260604174344` active giảm 20.000đ; `STOREINA20260604174344` inactive sau PUT.
- Đơn COD: order `72`, code `DH20260604180447843`, đã hủy qua UI.
- Đơn chuyển khoản/đặt cọc: order `73`, code `DH20260604180539902`, cọc 100.000đ, admin xác nhận phần còn lại và giao hoàn tất.
- Edge tồn kho: SKU/product `14` đã export tạm về 0 để test hết hàng, sau đó import bù lại 2 để restore.

## 3. Coverage Completed

- Guest routes: trang chủ, sản phẩm, chi tiết sản phẩm, hệ thống cửa hàng, voucher, yêu thích, giỏ hàng, checkout, đơn hàng, tài khoản, login/register, route 404.
- Header/nav/mobile menu: đã bấm logo, Trang chủ, Sản phẩm, Hệ thống cửa hàng, Liên hệ, FAQ, menu mobile/tablet.
- Auth: login rỗng/sai/đúng, register rỗng/sai/đúng, customer mới login được, admin thấy customer mới.
- Product: search, filter/sort/reset, card actions, favorite, gallery, quantity edge, thêm giỏ, mua ngay, sản phẩm hết hàng.
- Cart: thêm nhiều dòng, tăng/giảm/nhập số lượng, xóa dòng, empty/protected route.
- Checkout/order: COD, chuyển khoản, đặt cọc, voucher hợp lệ/không hợp lệ, success page, cancel order, admin confirm/fulfill, order detail/history.
- Account: profile, address, password validation, reload/responsive.
- Responsive: desktop/tablet/mobile representative pages.
- Build: frontend-store build và backend APIService build đều pass.

## 4. Critical Findings

### C1. Store product price

- Issue: Giá chi tiết sản phẩm Motul hiển thị 360.000đ nhưng cart/BE tính 390.000đ.
- Reproduce: Mở /products/10 sau khi upload ảnh test, xem giá; thêm 2 sản phẩm vào giỏ; gọi GET /api/cart.
- Expected: Giá bán hiện tại trên detail/card/cart/order phải thống nhất.
- Actual: Detail hiển thị 360.000đ/390.000đ, cart API unitPrice=390000 lineTotal=780000.

### C2. Store cart quantity mapping

- Issue: Giỏ hàng đọc sai số lượng từ BE: Cart DTO trả qty nhưng UI CartItemRow dùng item.quantity nên selector hiển thị/submit sai.
- Reproduce: Thêm 2 Motul từ detail, mở /cart, bấm + hoặc nhập 0/9999.
- Expected: Selector hiển thị 2, bấm + lên 3, clamp theo tồn.
- Actual: Cart initial lineTotal=780000 và totalItems=2 nhưng selector value=1; sau thao tác cập nhật API về qty=1.

### C3. Voucher validation/checkout

- Issue: Voucher inactive vẫn được checkout áp dụng và giảm sai giá trị.
- Reproduce: Trong checkout nhập STOREOFF20260604174344 sau các voucher lỗi; hoặc POST /vouchers/validate subtotal=390000.
- Expected: Voucher status inactive phải invalid; discount phải đúng discountValue/maxDiscount.
- Actual: {   "STOREOK20260604174344": {     "valid": true,     "message": null,     "discountAmount": 390000,     "voucher": {       "id": 15,       "code": "STOREOK20260604174344",       "description": "Giảm 20.000đ cho đơn test store",       "discountType": "Percent",       "discountValue": 20000,       "maxDiscount": null,       "minOrderValue": 0,       "usageLimit": 50,       "perUserLimit": 10,       "usedCount": 0,    

## 5. High Findings

### H1. Store product filters

- Issue: Frontend gọi /products/filters nhưng BE trả 404, khiến select danh mục/hãng/loại xe không có options động.
- Reproduce: Mở /products, xem các select filter hoặc gọi GET /api/products/filters.
- Expected: Danh mục/hãng/loại xe có dữ liệu từ BE.
- Actual: API 404; FE fallback filters rỗng.

### H2. Store product filter UI

- Issue: Một số select filter chỉ có option “Tất cả” do /products/filters 404, chưa test được lọc danh mục/hãng bằng UI thực.
- Reproduce: Mở /products và xem select Danh mục/Hãng xe.
- Expected: Có danh mục/hãng/loại xe từ BE để chọn.
- Actual: [{"index":0,"options":[{"text":"Tất cả danh mục","value":""}],"value":""},{"index":1,"options":[{"text":"Tất cả hãng","value":""}],"value":""},{"index":2,"options":[{"text":"Tất cả mức giá","value":"-"},{"text":"Dưới 10.000.000đ","value":"0-10000000"},{"text":"10.000.000đ - 30.000.000đ","value":"10000000-30000000"},{"text":"30.000.000đ - 60.000.000đ","value":"30000000-60000000"},{"text":"Trên 60.000.000đ","value":"60

### H3. Store product review

- Issue: Khách không gửi được review từ order detail completed, có thể do order line thiếu productId và ReviewModal dùng line id làm product id.
- Reproduce: Mở /orders/73, bấm Đánh giá sản phẩm, nhập nội dung, gửi.
- Expected: Review gửi thành công cho ProductId=10 và xuất hiện pending/admin reviews.
- Actual: {"ordersUrl":"http://localhost:5174/orders","ordersText":"Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nĐơn hàng của tôi\n#DH2026060418053990218:05 04/06/2026 · 7 giờ trước\nHoàn tất\nHÌNH THỨC THANH TOÁN\nChưa cập nhật\nTRẠNG THÁI","ordersHasCompleted":true,"detailUrl":"http://localhost:5174/orders/73","detailText"

### H4. Store system page

- Issue: Trang hệ thống cửa hàng không có dữ liệu cửa hàng/tỉnh/thành, không có nút Xem bản đồ/Chỉ đường.
- Reproduce: Mở /he-thong-cua-hang.
- Expected: Có ít nhất cửa hàng/kho duy nhất, địa chỉ, hotline, bản đồ, chỉ đường.
- Actual: {"ui":{"initial":"Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nHệ thống cửa hàng\nEURO MOTO\nHệ thống cửa hàng\n\nTra cứu cửa hàng EURO Moto gần bạn, chọn tỉnh thành hoặc nhập tên cửa hàng ","selects":[{"idx":0,"options":["Chọn tỉnh thành"],"value":""}],"afterSearchHit":"Hệ thống cửa hàng\nstore_e2e_20260604174344@

### H5. Store account profile mapper

- Issue: AccountPage normalizeProfile không map fullName/phoneNumber từ BE nên UI không phản ánh đúng tên/SĐT sau load/save.
- Reproduce: Vào /account với customer mới, lưu name/phone rồi reload.
- Expected: Tên hiển thị = fullName, SĐT hiển thị = phoneNumber.
- Actual: {"id":26,"fullName":"Nguyễn Khách Test Updated","email":"store_e2e_20260604174344@motosale.local","phoneNumber":"0904174345","roles":["Customer"]}

### H6. Store account address mapper

- Issue: AccountPage normalizeAddress không map recipientName/phone/line từ BE, địa chỉ mặc định đã lưu nhưng UI/sidebar dễ hiện rỗng.
- Reproduce: Lưu địa chỉ ở /account rồi reload tab địa chỉ.
- Expected: UI hiển thị recipientName/phone/line/ward/province đã lưu.
- Actual: {"items":[{"userId":26,"recipientName":"Nguyễn Khách Nhận Hàng","phone":"0904174345","line":"88 Võ Văn Tần","ward":"Phường 6","district":null,"province":"TP.HCM","isDefault":true,"user":null,"id":15,"createdDate":"2026-06-04T18:09:31.5738542","updatedDate":null,"status":1}]}

## 6. Medium Findings

### M1. Store cart image

- Issue: Cart item không hiển thị ảnh sản phẩm dù product đã có ảnh primary.
- Reproduce: Upload ảnh cho product 10, thêm vào cart, mở /cart hoặc GET /api/cart.
- Expected: Cart item có imageUrl hoặc FE lấy ảnh sản phẩm để hiển thị.
- Actual: GET /api/cart trả imageUrl=null; UI fallback EURO Moto.

### M2. Store account profile

- Issue: UI cho sửa email nhưng BE /users/me chỉ cập nhật FullName/PhoneNumber, email không persist.
- Reproduce: Vào /account, đổi email rồi Lưu thông tin.
- Expected: Nếu email editable thì phải lưu; nếu không cho đổi thì input nên readonly/ẩn.
- Actual: {"attempt":"store_e2e_20260604174344_changed@motosale.local","api":{"id":26,"fullName":"Nguyễn Khách Test Updated","email":"store_e2e_20260604174344@motosale.local","phoneNumber":"0904174345","roles":["Customer"]}}

### M3. Store vouchers page

- Issue: /vouchers không được bảo vệ login và customer không lấy được danh sách/lưu voucher vì BE /vouchers chỉ admin/staff; page luôn rỗng.
- Reproduce: Mở /vouchers khi guest/customer.
- Expected: Có danh sách voucher khả dụng hoặc redirect/login nếu chỉ là “voucher của tôi”.
- Actual: {"url":"http://localhost:5174/vouchers","text":"Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nVoucher của tôi\nKho Voucher\n\nNhận voucher và sử dụng khi thanh toán để được giảm giá\n\nVoucher khả dụng\n\nHiện chưa có vouch","buttons":["Sản phẩm","Menu","Đăng ký"],"apiAsCustomer":{"error":"API GET /vouchers -> 403",

### M4. Store responsive layout

- Issue: Một số trang có scrollWidth lớn hơn viewport ở responsive representative test.
- Reproduce: Set viewport mobile/tablet và mở các trang chính.
- Expected: Không overflow ngang.
- Actual: [{"path":"/account","viewport":"mobile","bodyScrollWidth":615,"buttons":9,"innerWidth":390,"inputs":4,"scrollWidth":615,"text":"Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\n0\n0\nMenu\nTrang chủ\n/\nTài khoản cá nhân\nTÀI KHOẢN\nstore_e2e_20260604174344@motosale.local\nstore_e2e_20260604174344@motosale.local\nChưa có số điện thoại\nTải lại\nThông tin tài khoản\nĐổi mật khẩu\n","overflow":true}]

## 7. Screenshot Artifacts

- product-detail-price-mismatch: `D:/MotorTeam/MoToSale-End/docs/test-artifacts/store-user-journey-20260604174344/product-detail-price-mismatch.jpg` (84651 bytes)
- order-detail-completed-review: `D:/MotorTeam/MoToSale-End/docs/test-artifacts/store-user-journey-20260604174344/order-detail-completed-review.jpg` (66357 bytes)
- store-system-empty: `D:/MotorTeam/MoToSale-End/docs/test-artifacts/store-user-journey-20260604174344/store-system-empty.jpg` (95988 bytes)
- account-mobile-overflow: `D:/MotorTeam/MoToSale-End/docs/test-artifacts/store-user-journey-20260604174344/account-mobile-overflow.jpg` (42610 bytes)

## 8. Check Result Matrix

| # | Check | Status | Actual / Evidence |
|---:|---|---|---|
| 1 | SETUP-AUTH-ADMIN | PASS | Đăng nhập admin API thành công |
| 2 | SETUP-AUTH-CUSTOMER | PASS | Đăng nhập customer seed API thành công |
| 3 | SETUP-VOUCHERS | PASS | {   "valid": {     "ok": true,     "data": {       "id": 15     }   },   "highMin": {     "ok": true,     "data": {       "id": 16     }   },   "expired": {     "ok": true,     "data": {       "id": 17     }   },   "inactive": {     "ok": true,     "data": {       "id": 18     }   } } |
| 4 | SETUP-PRODUCTS | WARN | {   "partInStock": {     "id": 10,     "code": "PT-NHOT-MOTUL",     "name": "Nhớt Motul 300V",     "slug": "nhot-motul-300v",     "categoryId": 6,     "brandId": null,     "vehicleModelId": null,     "kind": 2,     "isFeatured": false,     "isHotDeal": true,     "listPrice": 420000,     "salePrice": 390000,     "mainImageUrl": null,     "manufacturerId": 2,     "manufacturerName": "Motul",     "stockTotal": 30,     " |
| 5 | SETUP-PRODUCTS-KIND | PASS | {   "partInStock": {     "id": 10,     "code": "PT-NHOT-MOTUL",     "name": "Nhớt Motul 300V",     "slug": "nhot-motul-300v",     "categoryId": 6,     "brandId": null,     "vehicleModelId": null,     "kind": 2,     "isFeatured": false,     "isHotDeal": true,     "listPrice": 420000,     "salePrice": 390000,     "mainImageUrl": null,     "manufacturerId": 2,     "manufacturerName": "Motul",     "stockTotal": 30,     " |
| 6 | GUEST-ROUTES | PASS | [   {     "path": "/",     "url": "http://localhost:5174/",     "snippet": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nDanh mục nổi bật\n↗\nDANH MỤC NỔI BẬT\nXe tay ga\n\nKhám phá bộ sưu tập được chọn lọc theo phong cách và nhu cầu sử dụng."   },   {     "path": "/products",     "url": "http://localhost:5174/products",     "snippet": "Hệ thống cửa |
| 7 | GUEST-HEADER-LINKS | PASS | [   {     "label": "Trang chủ",     "url": "http://localhost:5174/",     "snippet": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nDanh mục nổi bật\n↗\nDANH MỤC NỔI BẬT\nXe tay ga\n\nKhám phá "   },   {     "label": "Sản phẩm",     "url": "http://localhost:5174/",     "snippet": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\nTrang chủ\nSản phẩm\nHonda\nXe g |
| 8 | GUEST-RESPONSIVE-MENU | PASS | [   {     "vp": "tablet",     "urlBefore": "http://localhost:5174/",     "hasMenuText": true,     "menuButtons": 1,     "afterOpenSnippet": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\n0\n0\nMenu\nTrang chủ\nSản phẩm\nHonda\nXe ga\nXe côn tay\nXe số\nYamaha\nXe ga\nXe côn tay\nXe số\nSYM\nXe ga\nXe côn tay\nXe số\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\nDanh mục nổi bật\n↗\nDANH M"   },   {     "vp": "mobile",     |
| 9 | AUTH-LOGIN-INVALID | PASS | {   "loginEmptyValidation": "Please fill out this field.",   "loginWrong": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nĐăng nhập\nĐăng nhập tài khoản\n\nNhập email hoặc số điện thoại để tiếp tục mua hàng.\n\nEmail hoặc mật khẩu không đúng.\nEMAIL HOẶC SỐ ĐIỆN THOẠI\nMẬT" } |
| 10 | AUTH-LOGIN-CORRECT | PASS | {   "url": "http://localhost:5174/",   "snippet": "Hệ thống cửa hàng\nKhách hàng mẫu\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nDanh mục nổi bật\n↗\nDANH MỤC NỔI BẬT\nXe tay ga\n\nKhám phá bộ sưu tập được chọn lọc theo phong cách và nhu cầu sử dụng.\n\nXEM NGAY\n↗\nDANH MỤC NỔI BẬT\nXe số\n\nKhám p" } |
| 11 | AUTH-LOGOUT | PASS | {   "loginEmptyValidation": "Please fill out this field.",   "loginWrong": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nĐăng nhập\nĐăng nhập tài khoản\n\nNhập email hoặc số điện thoại để tiếp tục mua hàng.\n\nEmail hoặc mật khẩu không đúng.\nEMAIL HOẶC SỐ ĐIỆN THOẠI\nMẬT",   "loginCorrectUrl": "http://localhost:5174/",   "loginCorrect |
| 12 | AUTH-REGISTER | PASS | {   "newCustomer": {     "email": "store_e2e_20260604174344@motosale.local",     "password": "Store@12345",     "name": "Khách E2E 20260604174344",     "phone": "0904174344"   },   "register": {     "loginEmptyValidation": "Please fill out this field.",     "loginWrong": "Hệ thống cửa hàng\nĐăng nhập\nĐăng ký\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nĐăng nhập\nĐăng nhậ |
| 13 | AUTH-NEW-CUSTOMER-LOGIN | PASS | {   "url": "http://localhost:5174/",   "snippet": "Hệ thống cửa hàng\nNguyễn Khách Test\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nDanh mục nổi bật\n↗\nDANH MỤC NỔI BẬT\nXe tay ga\n\nKhám phá bộ sưu tập được chọn lọc theo phong cách và nhu cầu sử dụng.\n\nXEM NGAY\n↗\nDANH MỤC NỔI BẬT\nXe số\n\nKhá" } |
| 14 | AUTH-ADMIN-CUSTOMER-CHECK | WARN | {"items":[],"page":1,"pageSize":20,"totalItems":0,"totalPages":0} |
| 15 | AUTH-ADMIN-CUSTOMER-CHECK-RETRY | PASS | {   "/users?search=store_e2e_20260604174344%40motosale.local": {     "items": [],     "page": 1,     "pageSize": 20,     "totalItems": 0,     "totalPages": 0   },   "/users/customers?search=store_e2e_20260604174344%40motosale.local": {     "items": [       {         "id": 26,         "fullName": "Nguyễn Khách Test",         "email": "store_e2e_20260604174344@motosale.local",         "phoneNumber": "0904174344",       |
| 16 | PRODUCT-FILTERS-API | FAIL | {"status":404,"data":null} |
| 17 | SETUP-CART-CLEAR | PASS | Cleared 0 cart items for store_e2e_20260604174344@motosale.local |
| 18 | SETUP-PRODUCT-IMAGES | PASS | {   "productId": 10,   "images": [     {       "id": 2,       "skuId": null,       "url": "/uploads/products/5d863b8120bc4b129d1676bcbe52f5f6.png",       "alt": "Ảnh test Motul E2E",       "isPrimary": true,       "sortOrder": 1     },     {       "id": 3,       "skuId": null,       "url": "/uploads/products/95d1c40cb8db410c826cd9268c21d70c.png",       "alt": "Ảnh phụ Motul E2E",       "isPrimary": false,       "sort |
| 19 | PRODUCT-LIST-FILTERS | PASS | {   "initialUrl": "http://localhost:5174/products",   "initialSnippet": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nSản phẩm\n/\nTất cả sản phẩm\nBỘ LỌC\nChọn sản phẩm phù hợp\nTỪ KHÓA\nDANH MỤC\nTất cả danh mục\nHÃNG XE\nTất cả hãng\nKHOẢNG GIÁ\nTấ",   "searchMotulUrl": "http://localhost:5174/products?keyword=Mo |
| 20 | PRODUCT-CARD-FAVORITE | WARN | getByLabel requires a string or RegExp |
| 21 | PRODUCT-CARD-ADD | PASS | {   "url": "http://localhost:5174/products/10",   "snippet": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nSản phẩm bán chạy\n/\nNhớt Motul 300V\nEURO MOTO\n1 / 2\nNhớt Motul 300V\nGiá bán\nGiảm 7%\n360.000 ₫\n390.000 ₫\nMàu sắc\nĐang cập n" } |
| 22 | PRODUCT-CARD-FAVORITE | PASS | Hệ thống cửa hàng store_e2e_20260604174344@motosale.local Trang chủ Sản phẩm Liên hệ Hệ thống cửa hàng Câu hỏi thường gặp 1 0 Trang chủ / Sản phẩm / Tất cả sản phẩm BỘ LỌC Chọn sản phẩm phù hợp TỪ KHÓA DANH MỤC Tất cả danh mục HÃNG XE Tất cả hãng KHOẢNG GIÁ Tấ |
| 23 | PRODUCT-CARD-ADD | PASS | {   "url": "http://localhost:5174/products/10",   "snippet": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n1\n0\nTrang chủ\n/\nSản phẩm bán chạy\n/\nNhớt Motul 300V\nEURO MOTO\n1 / 2\nNhớt Motul 300V\nGiá bán\nGiảm 7%\n360.000 ₫\n390.000 ₫\nMàu sắc\nĐang cập n" } |
| 24 | PRODUCT-DETAIL-ADD-CART | PASS | {   "detail": {     "text": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n1\n0\nTrang chủ\n/\nSản phẩm bán chạy\n/\nNhớt Motul 300V\nEURO MOTO\n1 / 2\nNhớt Motul 300V\nGiá bán\nGiảm 7%\n360.000 ₫\n390.000 ₫\nMàu sắc\nĐang cập n",     "imgCount": 1,     "thumbCount": 2,     "buttonNames": [       "Sản phẩm",       "Menu",       "Ảnh tr |
| 25 | CART-QUANTITY | PASS | {   "initial": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n1\n2\nTrang chủ\n/\nGiỏ hàng\nEURO MOTO\nNhớt Motul 300V\nEURO Moto\nMã: N/A\n390.000 ₫\n-\n+\n780.000 ₫\nXóa\nThông tin đơn hàng\nTạm tính\n780.000 ₫\nPhí",   "plusCount": 1,   "qtyCount": 1,   "qtyAfterPlus": "1",   "qtyAfter0": "1",   "qtyAfter9999": "1",   "qtyAfter1": " |
| 26 | FAVORITE-PAGE | PASS | {   "initialUrl": "http://localhost:5174/favorites",   "initialText": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n1\n1\nTrang chủ\n/\nYêu thích\n-7%\nEURO Moto\nNhớt Motul 300V\n390.000 ₫\n420.000 ₫\nXem chi tiếtThêm vào giỏ\n\nHệ thống mua bán xe máy, phụ tùng và",   "detailLinkCount": 1,   "detailUrl": "http://localhost:5174/produ |
| 27 | CHECKOUT-VOUCHER-CASES | FAIL | {   "STOREOK20260604174344": {     "valid": true,     "message": null,     "discountAmount": 390000,     "voucher": {       "id": 15,       "code": "STOREOK20260604174344",       "description": "Giảm 20.000đ cho đơn test store",       "discountType": "Percent",       "discountValue": 20000,       "maxDiscount": null,       "minOrderValue": 0,       "usageLimit": 50,       "perUserLimit": 10,       "usedCount": 0,     |
| 28 | SETUP-VOUCHERS-PROPER | PASS | {   "properVouchers": {     "valid": {       "id": 19     },     "inactive": {       "id": 20     }   },   "properChecks": {     "STOREAMT20260604174344": {       "valid": true,       "message": null,       "discountAmount": 20000,       "voucher": {         "id": 19,         "code": "STOREAMT20260604174344",         "description": "Amount voucher STOREAMT20260604174344",         "discountType": "Amount",         "di |
| 29 | CHECKOUT-COD | PASS | {   "afterRemoveVoucherText": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n1\nTrang chủ\n/\nGiỏ hàng\n/\nThanh toán\nTHANH TOÁN\nThông tin giao hàng\nThông tin liên hệ\nHọ và tên *\nSố điện thoại *\nEmail\nPhương thức nhận ",   "inputsAfterRemove": [     {       "idx": 0,       "name": "shippingFullName",       "placeholder": "Ngu |
| 30 | ORDER-CANCEL-UI | PASS | {   "before": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nĐơn hàng\n/\n#DH20260604180447843\nĐƠN HÀNG\n#DH20260604180447843\n\nĐặt ngày 18:04 04/06/2026\n\nChờ thanh toán / xác nhận\nChưa t",   "after": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nC |
| 31 | CHECKOUT-BANK-DEPOSIT | PASS | {   "checkoutUrlAfterBuyNow": "http://localhost:5174/checkout",   "checkoutInitial": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n1\nTrang chủ\n/\nGiỏ hàng\n/\nThanh toán\nTHANH TOÁN\nThông tin giao hàng\nThông tin liên hệ\nHọ và tên *\nSố điện thoại *\nEmail\nPhương thức nhận ",   "depositZeroText": "Hệ thống cửa hàng\nstore_e2e_ |
| 32 | ADMIN-CONFIRM-FULFILL-FOR-REVIEW | PASS | {   "confirm": {     "message": "Đã xác nhận thanh toán."   },   "orderAfterConfirm": {     "id": 73,     "code": "DH20260604180539902",     "userId": 26,     "orderType": "Deposit",     "orderStatus": "Confirmed",     "paymentMethod": "BankTransfer",     "paymentStatus": "DepositPaid",     "fulfillmentStatus": "Unallocated",     "subtotal": 390000,     "discountTotal": 0,     "shippingFee": 0,     "grandTotal": 3900 |
| 33 | ORDER-REVIEW-UI | WARN | {   "ordersUrl": "http://localhost:5174/orders",   "ordersText": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nĐơn hàng của tôi\n#DH2026060418053990218:05 04/06/2026 · 7 giờ trước\nHoàn tất\nHÌNH THỨC THANH TOÁN\nChưa cập nhật\nTRẠNG THÁI",   "ordersHasCompleted": true,   "detailUrl": "http://localhost:5174/orders/ |
| 34 | ACCOUNT-PROFILE | PASS | {   "initial": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nTài khoản cá nhân\nTÀI KHOẢN\nstore_e2e_20260604174344@motosale.local\nstore_e2e_20260604174344@motosale.local\nChưa có số điệ",   "invalidProfileText": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cử |
| 35 | ACCOUNT-ADDRESS | PASS | {   "initial": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nTài khoản cá nhân\nTÀI KHOẢN\nstore_e2e_20260604174344@motosale.local\nstore_e2e_20260604174344@motosale.local\nChưa có số điệ",   "invalidProfileText": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cử |
| 36 | ACCOUNT-PASSWORD-VALIDATION | PASS | Hệ thống cửa hàng store_e2e_20260604174344@motosale.local Trang chủ Sản phẩm Liên hệ Hệ thống cửa hàng Câu hỏi thường gặp 0 0 Trang chủ / Tài khoản cá nhân TÀI KHOẢN store_e2e_20260604174344@motosale.local store_e2e_20260604174344@motosale.local Chưa có số điệ |
| 37 | VOUCHERS-PAGE | PASS | {   "url": "http://localhost:5174/vouchers",   "text": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nVoucher của tôi\nKho Voucher\n\nNhận voucher và sử dụng khi thanh toán để được giảm giá\n\nVoucher khả dụng\n\nHiện chưa có vouch",   "buttons": [     "Sản phẩm",     "Menu",     "Đăng ký"   ],   "apiAsCustomer": {  |
| 38 | STORE-SYSTEM | PASS | {   "initial": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\nTrang chủ\nSản phẩm\nLiên hệ\nHệ thống cửa hàng\nCâu hỏi thường gặp\n0\n0\nTrang chủ\n/\nHệ thống cửa hàng\nEURO MOTO\nHệ thống cửa hàng\n\nTra cứu cửa hàng EURO Moto gần bạn, chọn tỉnh thành hoặc nhập tên cửa hàng ",   "selects": [     {       "idx": 0,       "options": [         "Chọn tỉnh thành"       ],       "value": ""     }   ],   "aft |
| 39 | STORE-SYSTEM-DATA-CHECK | FAIL | {   "showrooms": [     {       "id": 1,       "name": "E2E Shop 401285",       "address": "",       "phoneNumber": "",       "email": "",       "openingHours": "08:00 - 21:00",       "bankName": "MB Bank",       "bankCode": "MB",       "bankAccountNo": "52968011042005",       "bankAccountName": "CUA HANG MOTOSALE",       "bankQrUrl": "",       "isActive": true     }   ],   "stores": {     "error": "API GET /stores -> |
| 40 | PRODUCT-OUT-OF-STOCK-EDGE | PASS | {   "before": {     "id": 7,     "code": "SP-RAIDER",     "name": "Suzuki Raider R150",     "slug": "suzuki-raider-r150",     "categoryId": 5,     "brandId": 3,     "vehicleModelId": 8,     "kind": 1,     "shortDescription": "Suzuki Raider R150 chính hãng, bảo hành theo tiêu chuẩn nhà sản xuất.",     "description": null,     "isFeatured": false,     "isHotDeal": false,     "manufacturerId": null,     "manufacturerNam |
| 41 | RESPONSIVE-REPRESENTATIVE | WARN | [   {     "path": "/products",     "viewport": "mobile",     "bodyScrollWidth": 375,     "buttons": 30,     "innerWidth": 390,     "inputs": 6,     "scrollWidth": 375,     "text": "Hệ thống cửa hàng\nstore_e2e_20260604174344@motosale.local\n0\n0\nMenu\nTrang chủ\n/\nSản phẩm\n/\nTất cả sản phẩm\nBỘ LỌC\nChọn sản phẩm phù hợp\nTỪ KHÓA\nDANH MỤC\nTất cả danh mục\nHÃNG XE\nTất cả hãng\nKHOẢNG GIÁ\nTất cả mức giá\nDưới 1 |

## 9. Build Verification

- `npm run build` trong `D:/MotorTeam/MoToSale-End/v2/frontend-store`: PASS.
- `dotnet build D:/MotorTeam/MoToSale-End/v2/backend/src/MoToSale.APIService/MoToSale.APIService.csproj`: PASS.

## 10. Recommended Fix Order

1. Sửa thống nhất giá store: detail/card/cart/order dùng cùng current price sau khuyến mại.
2. Sửa cart DTO/UI mapping: dùng `qty` hoặc normalize thành `quantity`, giữ đúng số lượng khi tăng/giảm/xóa 1 dòng.
3. Chuẩn hóa contract voucher Admin/BE/Store: `Amount`/`Percent`, status create/update, validate inactive/expired/min order/scope.
4. Sửa review completed order: order line phải có productId đúng, modal gửi review theo productId.
5. Bổ sung endpoint/data filter sản phẩm store hoặc đổi FE gọi đúng endpoint hiện có.
6. Sửa AccountPage mapper profile/address và mobile overflow.
7. Sửa trang hệ thống cửa hàng: lấy cấu hình cửa hàng duy nhất, hiển thị địa chỉ/hotline/map/chỉ đường.

## 11. Final Status

Plan đã được thực hiện đủ vòng chính theo vai người dùng thật và có đối chiếu BE/admin. Những lỗi còn lại đã được phân loại rõ để chuyển sang vòng sửa. Không có lỗi build, nhưng có lỗi nghiệp vụ tiền/giỏ/voucher nên chưa nên đánh dấu frontend-store là hoàn tất vận hành.
