# Kế hoạch kiểm thử toàn hệ thống 2 chiều — Storefront (Khách hàng) & Admin (Quản trị) — MoToSale v2

Phiên bản: 1.0 · Ngày: 04/06/2026
Kế thừa & mở rộng `V2_BTL_FULL_SYSTEM_TEST_PROCESS.md` (vốn chỉ có Admin) → bổ sung đầy đủ **Storefront** và **các luồng 2 chiều**.

---

## 1. Mục tiêu

Kiểm thử **toàn diện cả 2 ứng dụng** dùng chung backend v2:
- **Storefront (khách hàng)** — `http://localhost:5174` (web "EURO Moto").
- **Admin (quản trị)** — `http://localhost:5176`.

Không chỉ "mở được trang" mà phải kiểm: **từng trang, từng modal, từng nút, từng trường nhập, từng nghiệp vụ, các luồng liên hoàn 2 chiều (khách ↔ quản trị), phân quyền, và các trường hợp đặc biệt/biên**.

## 2. Quy tắc bắt buộc (giữ từ file cũ)
- Test bằng UI thật, không chỉ đọc code.
- Không bỏ sót modal/nút/trường nào.
- Mỗi modal: mở, đóng bằng X, đóng bằng Hủy, submit hợp lệ, submit lỗi.
- Mỗi trường: ≥ 4 nhóm dữ liệu — hợp lệ / thiếu bắt buộc / sai định dạng / dữ liệu dài + ký tự đặc biệt.
- Thao tác đổi DB: kiểm lại ở trang liên quan **và đối chiếu chéo phía bên kia** (khách ↔ admin).
- Thao tác nguy hiểm (xóa/hủy/duyệt/từ chối/hoàn tiền): phải có cảnh báo/xác nhận.
- Reload + rời trang rồi quay lại để xác nhận dữ liệu bền vững.
- Kết thúc: `npm run build` (cả 2 FE), `dotnet build`, `dotnet test`.
- Ghi lỗi đủ: trang, phần tử, dữ liệu nhập, kết quả mong muốn, thực tế, ảnh, mức độ.

## 3. Môi trường & tài khoản

### 3.1 Dịch vụ
| Thành phần | URL |
|---|---|
| API Gateway | http://localhost:5100 |
| AuthService / APIService | 5101 / 5102 |
| Storefront (khách) | http://localhost:5174 |
| Admin (quản trị) | http://localhost:5176 |

### 3.2 Tài khoản
- Admin: `admin@motosale.local / Admin@123`
- Nhân viên: `staff@motosale.local / Staff@123`
- Khách (seed): `customer@motosale.local`
- Khách test: tự đăng ký mới trên storefront.

### 3.3 Dữ liệu nền nên có (đối chiếu 2 chiều)
- Sản phẩm: xe (Vision/Winner X/Exciter…) nhiều SKU + phụ tùng (nhớt/lốp/má phanh) có hãng SX, có giá KM, còn hàng / sắp hết / hết hàng.
- Khách: có đơn ở mọi trạng thái (Chờ TT, đã giao, đã hủy, hoàn tất); có địa chỉ; có review.
- Voucher đang hiệu lực; bài viết Published; FAQ; banner; cấu hình cửa hàng (tên/địa chỉ/SĐT/giờ).

---

## 4. Chuẩn kiểm tra chung (áp cho MỌI trang/modal — cả 2 FE)

### 4.1 Trang
1. Vào từ menu/route; tiêu đề, breadcrumb, active menu đúng.
2. Không trắng trang, không lỗi console, không 4xx/5xx bất thường (mở DevTools → Network/Console).
3. Bảng/list: header đúng nghiệp vụ, căn lề hợp lý, **tiền VNĐ**, **ngày giờ kiểu VN**, **trạng thái tiếng Việt**, dữ liệu dài không tràn, empty-state rõ.
4. Filter/search/sort/phân trang (nếu có).
5. Mọi nút trên trang.
6. Reload + rời/quay lại trang.
7. Responsive: 1440 / 768 / 390 px (storefront bắt buộc test mobile).
8. Không mojibake (`Ã`, `áº`…), không lẫn tiếng Anh ở UI tiếng Việt.

### 4.2 Modal
- Mở đúng khi bấm; tiêu đề đúng ngữ cảnh; overlay không che nội dung; không tràn màn hình.
- Đóng bằng **X** và bằng **Hủy/Đóng**; submit hợp lệ → đóng + refresh; submit lỗi → thông báo rõ.
- Reload sau submit vẫn thấy dữ liệu mới.

---

# PHẦN A — STOREFRONT (KHÁCH HÀNG) — `:5174`

> Layout chung (`Header`, `Footer`, `FloatingActions`): kiểm ở **A0** rồi áp cho mọi trang.

## A0. Khung chung (Header/Footer/Layout)
**Header** — logo (về Home), menu điều hướng (Trang chủ, Sản phẩm, Hệ thống cửa hàng, Voucher), **ô tìm kiếm**, **badge giỏ hàng** (số lượng), **badge yêu thích**, **menu người dùng** (khi đăng nhập: Tài khoản, Đơn hàng, Đăng xuất; khi chưa: Đăng nhập/Đăng ký).
- TC-A0-1 Badge giỏ/yêu thích cập nhật realtime khi thêm/bớt (CartContext/FavoriteContext phát event).
- TC-A0-2 Menu người dùng đổi theo trạng thái đăng nhập; Đăng xuất xoá token, về trạng thái khách.
- TC-A0-3 Ô tìm kiếm → điều hướng `/products?...` đúng từ khoá.
- TC-A0-4 `FloatingActions` (nút nổi: lên đầu trang/liên hệ/zalo…) hoạt động.
- TC-A0-5 Footer: link, thông tin cửa hàng, không vỡ ở mobile.
- TC-A0-6 Toast (NotificationContext) hiện đúng cho add giỏ/yêu thích/lỗi.

## A1. Trang chủ `/` (HomePage)
- Banner (từ `/content/home-banners`), sản phẩm nổi bật/hot deal, danh mục, khối review/bài viết nếu có.
- TC-A1-1 Banner hiển thị + click → link đúng; empty khi không có banner.
- TC-A1-2 Sản phẩm nổi bật click → trang chi tiết.
- TC-A1-3 Loading/empty/error khi API chậm/lỗi.

## A2. Danh sách sản phẩm `/products` (ProductListPage + ProductFilters/ProductGrid/ProductCard)
- Bộ lọc (`ProductFilters`): danh mục, hãng, khoảng giá, trạng thái còn hàng; sắp xếp (giá ↑/↓, mới…); phân trang; từ khoá.
- ProductCard: ảnh, tên, giá (gốc + KM gạch), badge hot/giảm, nút **Yêu thích**, click → chi tiết.
- TC-A2-1 Lọc theo danh mục/hãng → kết quả đúng; kết hợp nhiều bộ lọc.
- TC-A2-2 Sắp xếp giá tăng/giảm đúng thứ tự.
- TC-A2-3 Khoảng giá min>max, min âm → xử lý hợp lý (không vỡ).
- TC-A2-4 Tìm từ khoá có/không kết quả → empty-state.
- TC-A2-5 Phân trang (đầu/cuối/giữa); đổi trang giữ bộ lọc.
- TC-A2-6 Bấm tim Yêu thích **khi chưa đăng nhập** → điều hướng login / nhắc đăng nhập; khi đã đăng nhập → toggle + badge tăng.
- TC-A2-7 `/products/filters` BE chưa có → FE trả bộ lọc rỗng (đã xử lý) → **không vỡ trang** (kiểm Network thấy 404 nhưng UI vẫn chạy).

## A3. Chi tiết sản phẩm `/products/:id` (ProductDetailPage + ProductImageGallery/ProductInfoBox/ProductTabs/ProductReviews/ReviewModal/RelatedProductSection)
- Gallery ảnh (đổi ảnh, ảnh theo biến thể), thông tin (tên/giá/kho), **chọn biến thể (màu/phiên bản)**, **QuantitySelector** (+/−, nhập số), nút **Thêm vào giỏ hàng**, **Mua ngay**, nút **Yêu thích**.
- Tabs: Mô tả / Thông số / Đánh giá; **RelatedProductSection** (sản phẩm liên quan/bán kèm).
- **ProductReviews**: danh sách review đã duyệt + điểm trung bình; nút **Đánh giá sản phẩm** (mở `ReviewModal`) hoặc "**Đăng nhập để đánh giá**".
- **ReviewModal**: chọn số sao (rating), tiêu đề, **Đánh giá của bạn** (comment), nút Gửi/Hủy.

Nghiệp vụ & TH:
- TC-A3-1 Chọn biến thể → giá/ảnh/kho đổi theo; biến thể hết hàng → chặn thêm giỏ.
- TC-A3-2 QuantitySelector: tăng/giảm, nhập 0/âm/vượt tồn → chặn hợp lý.
- TC-A3-3 **Thêm vào giỏ** (chưa đăng nhập) → nhắc/redirect login; (đã đăng nhập) → vào giỏ + toast + badge.
- TC-A3-4 **Mua ngay** → vào giỏ rồi chuyển checkout.
- TC-A3-5 Sản phẩm không có biến thể vẫn thêm được (skuId mặc định).
- TC-A3-6 Review: chưa đăng nhập → "Đăng nhập để đánh giá".
- TC-A3-7 Đăng nhập nhưng **chưa mua** → mở modal bị chặn / báo "cần mua trước khi đánh giá" (BE `canReview=false`, reason hiển thị).
- TC-A3-8 Đã mua & đơn **đã giao** → gửi review (1–5 sao + comment) → báo "chờ duyệt"; gửi lần 2 → chặn "đã đánh giá"; sửa review của mình → về trạng thái chờ duyệt.
- TC-A3-9 Review chỉ hiển thị công khai khi **admin duyệt Approved** (xem luồng X3).
- TC-A3-10 Rating để trống / comment quá dài / ký tự đặc biệt.
- TC-A3-11 Sản phẩm id không tồn tại → trang 404/empty hợp lý.

## A4. Giỏ hàng `/cart` (CartPage + CartItemRow/CartSummary/EmptyCart) — *cần đăng nhập*
- Danh sách item (ảnh, tên, biến thể, đơn giá, **QuantitySelector**, thành tiền, nút **Xóa**), **CartSummary** (tạm tính, phí ship, giảm giá, tổng), nút **Thanh toán**, **EmptyCart** khi rỗng.
- TC-A4-1 Vào `/cart` khi chưa đăng nhập → **ProtectedRoute redirect /login**, sau login quay lại giỏ.
- TC-A4-2 Tăng/giảm số lượng → thành tiền + tổng cập nhật (PUT `/cart/items/{id}` qty).
- TC-A4-3 Xóa 1 item → còn lại đúng, **không mất cả giỏ**; xóa hết → EmptyCart.
- TC-A4-4 Sửa qty vượt tồn → chặn/thông báo.
- TC-A4-5 Badge header khớp số item; reload giữ giỏ.
- TC-A4-6 Nút Thanh toán khi giỏ rỗng → chặn.

## A5. Thanh toán `/checkout` (CheckoutPage) — *cần đăng nhập*
- Form người nhận (họ tên, SĐT, email, địa chỉ: tỉnh/quận/phường/đường), **phương thức nhận** (Giao hàng/Nhận tại cửa hàng), ô **mã voucher** + Áp dụng, ghi chú, tóm tắt đơn, nút **Đặt hàng**, link **← Quay lại giỏ hàng**, nút **Xóa** (item).
- **COD/tại cửa hàng** (không thanh toán online).
- TC-A5-1 Điền đủ → **Đặt hàng** → tạo đơn (POST `/orders`, CheckoutRequest) → chuyển `/checkout/success`.
- TC-A5-2 Thiếu trường bắt buộc (tên/SĐT/địa chỉ) → chặn + báo lỗi từng trường.
- TC-A5-3 SĐT sai định dạng; email sai định dạng.
- TC-A5-4 Áp **voucher hợp lệ** → giảm đúng; **voucher sai/hết hạn/không đủ điều kiện** → báo lỗi, không giảm; tổng không âm.
- TC-A5-5 Chọn "Nhận tại cửa hàng" → ẩn/không bắt buộc địa chỉ giao.
- TC-A5-6 Đặt hàng khi giỏ rỗng / tồn vừa hết (mua ở tab khác) → chặn "tồn không đủ".
- TC-A5-7 Sau đặt hàng → **giỏ được làm rỗng**; badge về 0.

## A6. Đặt hàng thành công `/checkout/success` (CheckoutSuccessPage)
- TC-A6-1 Hiển thị mã đơn + tóm tắt; nút "Xem đơn hàng" → `/orders/:id`; "Tiếp tục mua" → `/products`.
- TC-A6-2 Vào trực tiếp khi không có đơn → xử lý hợp lý (redirect).

## A7. Đơn của tôi `/orders` (OrdersPage) — *cần đăng nhập*
- Danh sách đơn (mã, ngày, tổng, trạng thái đơn, trạng thái TT), nút **Xem chi tiết →**.
- TC-A7-1 Chỉ thấy **đơn của chính mình** (GET `/orders/mine`); KHÔNG thấy đơn người khác (kiểm bảo mật, xem X-SEC).
- TC-A7-2 Lọc theo trạng thái (nếu có); empty khi chưa có đơn.
- TC-A7-3 Mã/tổng/trạng thái hiển thị đúng (đối chiếu admin).

## A8. Chi tiết đơn `/orders/:id` (OrderDetailPage) — *cần đăng nhập*
- Thông tin người nhận, danh sách sản phẩm, tổng/giảm/đã trả/còn lại, **timeline trạng thái**, nút **Hủy đơn hàng** (điều kiện).
- TC-A8-1 Hiển thị đúng dòng hàng/tiền/trạng thái.
- TC-A8-2 **Hủy đơn hàng** khi đơn còn *Chờ thanh toán/Chưa giao* → POST `/orders/{id}/cancel` → Cancelled; có hộp xác nhận + lý do.
- TC-A8-3 Đơn **đã giao/hoàn tất** → nút Hủy ẩn/bị chặn.
- TC-A8-4 Mở đơn **không phải của mình** (sửa id trên URL) → **bị chặn 403/redirect** (X-SEC-1).
- TC-A8-5 Đơn đã giao → nút/đường dẫn đánh giá sản phẩm (nếu có).

## A9. Yêu thích `/favorites` (FavoritesPage) — *cần đăng nhập*
- Lưới sản phẩm đã thích, nút bỏ thích, click → chi tiết.
- TC-A9-1 Thêm ở trang khác → xuất hiện ở đây (kèm thông tin sản phẩm).
- TC-A9-2 Bỏ thích → biến mất + badge giảm; reload giữ trạng thái.
- TC-A9-3 Thích lại sản phẩm đã thích (idempotent, không nhân đôi).
- TC-A9-4 Empty-state khi chưa thích gì.

## A10. Voucher `/vouchers` (VouchersPage)
- Danh sách voucher (phạm vi **đơn giản hoá**: ví voucher applicable/save/my trả rỗng).
- TC-A10-1 Trang **không vỡ** dù BE trả rỗng (đã xử lý resilient); hiển thị empty/hướng dẫn nhập mã ở checkout.
- TC-A10-2 (Nếu có ô nhập mã) xem theo mã → validate.

## A11. Hệ thống cửa hàng `/he-thong-cua-hang` (StoreSystemPage + StoreFilters/StoreList/StoreMap/StoreStats)
- 1 cửa hàng (từ Settings qua `/showrooms`): tên, địa chỉ, SĐT, giờ mở; **bản đồ**; nút **Chỉ đường**; ô **Tìm cửa hàng**.
- TC-A11-1 Hiển thị đúng thông tin cửa hàng (đối chiếu Cấu hình admin).
- TC-A11-2 Bản đồ render; **Chỉ đường** mở Google Maps đúng địa chỉ/toạ độ.
- TC-A11-3 Tìm cửa hàng (1 cửa hàng) → lọc hợp lý.

## A12. Đăng nhập `/login` (LoginPage + AuthForm) — *chỉ khi chưa đăng nhập*
- Email, mật khẩu, (nhớ đăng nhập), nút **Đăng nhập**, link **Đăng ký**.
- TC-A12-1 Đăng nhập đúng → về trang trước/Home, header đổi.
- TC-A12-2 Sai mật khẩu / email không tồn tại → báo lỗi rõ.
- TC-A12-3 Bỏ trống; email sai định dạng.
- TC-A12-4 Đang đăng nhập mà vào `/login` → **PublicRoute redirect** về Home.
- TC-A12-5 "Nhớ đăng nhập" → token ở localStorage vs sessionStorage.

## A13. Đăng ký `/register` (RegisterPage + AuthForm)
- Họ tên, email, SĐT, mật khẩu (xác nhận), nút **Đăng ký**, link **Đăng nhập**.
- TC-A13-1 Đăng ký mới hợp lệ → tự đăng nhập/ò chuyển login.
- TC-A13-2 Email đã tồn tại → báo lỗi.
- TC-A13-3 Mật khẩu yếu/không khớp xác nhận; SĐT sai định dạng; bỏ trống.

## A14. Tài khoản `/account` (AccountPage) — *cần đăng nhập*
- Tab/khu: **Hồ sơ** (sửa họ tên/SĐT), **Đổi mật khẩu**, **Địa chỉ**, nút **Đăng xuất**.
- TC-A14-1 Sửa hồ sơ (PUT `/users/me` {fullName, phoneNumber}) → lưu + hiển thị lại đúng (lưu ý email không đổi qua API này).
- TC-A14-2 Đổi mật khẩu: sai mật khẩu hiện tại → chặn; đổi đúng → đăng nhập lại bằng mật khẩu mới.
- TC-A14-3 Địa chỉ: thêm/sửa (POST `/users/me/addresses`), đặt mặc định; GET lấy địa chỉ mặc định.
- TC-A14-4 Đăng xuất → xoá phiên, về Home.

## A15. 404 `*` (NotFoundPage)
- TC-A15-1 URL sai → trang 404 + nút về Home.

---

# PHẦN B — ADMIN (QUẢN TRỊ) — `:5176`

> Chi tiết đầy đủ theo **`V2_BTL_FULL_SYSTEM_TEST_PROCESS.md`** mục 6 (5 nhóm menu). Dưới đây là **danh mục bắt buộc + bổ sung** cho phiên bản hiện tại.

## B1. Bán hàng
- **POS** (`/pos`): tìm SKU (mã/tên/barcode), giỏ (sửa SL/giá, xóa 1 dòng), **khách lẻ vs khách quen (tra SĐT)**, voucher, **bán đứt / đặt cọc** (nhập cọc), phương thức TT, Tạo đơn, **In hóa đơn VAT**. TH lỗi: giỏ trống, SL≤0, cọc≤0, cọc≥tổng, hết hàng, voucher sai.
- **Đơn hàng**: danh sách + bộ lọc; chi tiết (tiền/công nợ/timeline); **Ghi nhận thanh toán** (loại theo trạng thái, không thu vượt nợ); **Giao hàng & xuất kho**; **Sửa đơn** (dòng hàng chỉ khi Chờ TT); Hủy; In VAT.
- **Khách hàng**: CRUD, ghi chú CSKH, lịch sử mua, tìm theo tên/SĐT.
- **Voucher**: CRUD, %/tiền, hạn mức/thời hạn/đơn tối thiểu, **chặn xóa khi đã dùng**.

## B2. Sản phẩm & Kho
- **Sản phẩm**: CRUD (xe/phụ tùng), **xóa mềm**, SKU/biến thể, ảnh, barcode, tương thích, bán kèm; lỗi: giá KM>gốc, trùng mã, ảnh sai định dạng.
- **Danh mục/Hãng xe/Dòng xe/Hãng SX**: CRUD, upload logo, **chặn xóa khi còn tham chiếu**, dòng xe lọc theo hãng.
- **Tồn kho**: tồn thực/giữ chỗ/khả dụng/ngưỡng, lọc trạng thái tồn, đặt ngưỡng, điều chỉnh, đồng bộ, lịch sử movements, **xuất Excel**.
- **Chứng từ kho**: nhập/xuất/điều chỉnh, nhiều dòng, lưu nháp, **duyệt** (mới tác động tồn), hủy; lỗi: phiếu rỗng, SL âm, duyệt quá tồn, duyệt lại.
- **Cung ứng**: NCC CRUD; đơn mua → duyệt → **nhận hàng (tồn +)** → **thanh toán NCC (chi quỹ)**; công nợ NCC.

## B3. Dịch vụ & Hậu mãi
- **Đổi trả & hoàn tiền**: tạo từ đơn đã giao, chọn tình trạng (bán lại/hư/bảo hành), **duyệt → hoàn tồn + sinh phiếu hoàn + ghi chi quỹ**, từ chối, **chặn sửa sau duyệt**.
- **Bảo hành**: tạo (số khung/máy, lỗi), **sửa khi mới tiếp nhận**, chuyển trạng thái, lịch sử, **chặn sửa sau xử lý**.
- **Sửa chữa**: tạo (kèm phụ tùng), luồng Nhận→Kiểm tra→Báo giá→**Sửa (xuất kho phụ tùng)**→Bàn giao, sửa khi mới tiếp nhận.
- **CSKH**: tạo/hoàn thành/hủy tương tác, lịch sử theo khách.
- **Đánh giá (Reviews)** *(MỚI)*: danh sách review, lọc theo trạng thái, **Duyệt/Từ chối/Ẩn**, xóa (Admin). Xem luồng X3.

## B4. Tài chính & Báo cáo (Admin)
- **Sổ quỹ**: phiếu thu/chi (phần lớn tự sinh), **đảo phiếu**; **Công nợ** khách.
- **Báo cáo**: doanh thu, **lãi gộp/COGS**, top SP, trạng thái đơn, thu chi, công nợ, cảnh báo tồn; lọc theo kỳ; **xuất Excel**; đơn hủy không tính doanh thu; hoàn tiền điều chỉnh tiền thực nhận.

## B5. Hệ thống (Admin)
- **Tài khoản & vai trò**: tạo/sửa/khóa Staff, không tự khóa-xóa, không xóa Admin cuối, **chặn xóa user đã có đơn**.
- **Phân ca / Chấm công**: xếp ca (chặn trùng giờ), check-in/out.
- **Cấu hình**: tên/MST/VAT/ngưỡng tồn… (ảnh hưởng hóa đơn + trang Hệ thống cửa hàng của storefront — xem X5).
- **Nhật ký kiểm toán**: có bản ghi sau mọi mutation.
- **Liên hệ (Contacts)** *(MỚI từ storefront)*: danh sách liên hệ khách gửi, **đánh dấu đã xử lý**. Xem luồng X4.
- **Bài viết/FAQ/Banner**: CRUD; bài viết **Published** mới hiện ở storefront. Xem X6.
- **Import dữ liệu**: file mẫu, import hợp lệ/sai cột/trùng mã/sai kiểu, báo số dòng OK/lỗi.

---

# PHẦN C — LUỒNG NGHIỆP VỤ 2 CHIỀU (KHÁCH ↔ QUẢN TRỊ)

> Đây là phần **trọng tâm của bản plan 2 chiều**: mỗi luồng thực hiện 1 phần ở storefront, 1 phần ở admin, rồi đối chiếu.

### X1. Đặt hàng online → xử lý ở admin → khách theo dõi
1. Khách: đăng nhập 5174 → thêm giỏ → checkout (COD) → đơn `Chờ thanh toán`.
2. Admin 5176 → **Đơn hàng**: thấy đơn mới (đúng khách/sản phẩm/tổng).
3. Admin: Ghi nhận thanh toán đủ → **Giao hàng & xuất kho** → đơn **Hoàn tất**, **tồn giảm**.
4. Khách: `/orders/:id` → trạng thái cập nhật theo (timeline).
- Đối chiếu: tồn (admin Tồn kho), doanh thu (Báo cáo), quỹ (thu tiền).

### X2. Đơn đặt cọc (nếu storefront cho chọn) / hoặc đối chiếu cọc tạo từ POS
1. Tạo đơn cọc (POS hoặc checkout orderType=Deposit) → giữ chỗ tồn.
2. Khách xem còn nợ; Admin thu phần còn lại → giao → Hoàn tất.
3. Hủy đơn cọc → nhả giữ chỗ, mất cọc.

### X3. Vòng đời đánh giá (Review) — chuỗi đầy đủ
1. Khách mua sản phẩm P, Admin giao đơn → `Delivered`.
2. Khách `/products/P` → **Đánh giá sản phẩm** (modal, 5 sao + comment) → gửi → "chờ duyệt".
3. Admin → **Đánh giá**: thấy review `Pending` → **Duyệt (Approved)**.
4. Khách/khách vãng lai xem `/products/P` → review hiển thị công khai + điểm trung bình cập nhật.
- TH: Admin **Từ chối/Ẩn** → review KHÔNG hiển thị công khai; khách sửa review → quay lại `Pending`.

### X4. Liên hệ/tư vấn
1. Khách (kể cả chưa đăng nhập) gửi form liên hệ (Home/Footer/Chi tiết SP) → `/content/contacts`.
2. Admin → **Liên hệ**: thấy yêu cầu mới (New) → **đánh dấu đã xử lý**.

### X5. Cấu hình cửa hàng đồng bộ
1. Admin sửa **Cấu hình** (tên/địa chỉ/SĐT/giờ mở).
2. Storefront `/he-thong-cua-hang` (`/showrooms`) phản ánh đúng (reload).

### X6. Nội dung (Blog/FAQ/Banner)
1. Admin tạo **Bài viết** Published / **FAQ** / **Banner**.
2. Storefront: Home (banner), trang blog (`/content/posts/public`), FAQ (`/content/faq`) hiển thị; bài **Draft** KHÔNG hiện.

### X7. Yêu thích / Khách hàng
1. Khách thêm Yêu thích, cập nhật hồ sơ/địa chỉ.
2. Admin → **Khách hàng**: thấy khách (đăng ký từ storefront) + thông tin cập nhật + lịch sử mua.

### X8. Đổi trả khởi tạo từ hậu mãi
1. Khách có đơn đã giao (từ X1).
2. Admin tạo phiếu trả → duyệt → hoàn tồn + hoàn tiền; đối chiếu khách thấy trạng thái/đơn liên quan (nếu storefront hiển thị).

---

# PHẦN D — TRƯỜNG HỢP ĐẶC BIỆT / BIÊN / BẢO MẬT

### Bảo mật & phân quyền (X-SEC)
- X-SEC-1 Khách A KHÔNG xem được đơn của khách B (đổi id trên `/orders/:id`) → 403/redirect.
- X-SEC-2 Khách gọi thẳng API admin (`/api/inventory`, `/api/vouchers` GET, `/api/reports`, `/api/users`) → **403**.
- X-SEC-3 Truy cập route protected khi chưa đăng nhập (`/cart`,`/orders`,`/account`,`/favorites`,`/checkout`) → redirect `/login`.
- X-SEC-4 Token hết hạn (480') / token rác → tự đăng xuất, không lỗi trắng trang.
- X-SEC-5 Staff đăng nhập admin: bị chặn endpoint Admin-only (tài chính/tài khoản/cung ứng/cấu hình/nhật ký/import); vẫn dùng POS/dịch vụ.
- X-SEC-6 Đăng nhập admin account trên storefront / khách trên admin → hành xử hợp lý (role không khớp UI).

### Đồng thời & tồn kho (X-RACE)
- X-RACE-1 2 khách cùng mua SKU cuối cùng → 1 thành công, 1 bị "tồn không đủ".
- X-RACE-2 Khách để sản phẩm trong giỏ, admin/đơn khác làm hết tồn → checkout chặn.
- X-RACE-3 Tạo nhiều đơn liên tiếp trong 1 giây → **không trùng mã** (đã sửa BUG-01, regression).

### Dữ liệu biên (X-EDGE)
- X-EDGE-1 Tên/ghi chú/địa chỉ rất dài, ký tự đặc biệt & tiếng Việt có dấu, emoji.
- X-EDGE-2 Số lượng/giá: 0, âm, rất lớn, số thập phân.
- X-EDGE-3 Giỏ nhiều dòng (≥ 20), đơn nhiều sản phẩm.
- X-EDGE-4 Voucher: vượt giá trị đơn (tổng không âm), đúng ngày bắt đầu/kết thúc (biên), đạt hạn mức lượt dùng.
- X-EDGE-5 Sản phẩm bị admin ẩn (Inactive) trong khi khách đang xem/đặt → xử lý hợp lý.
- X-EDGE-6 Ảnh/biến thể thiếu → placeholder, không vỡ layout.
- X-EDGE-7 Mạng chậm/đứt giữa chừng (DevTools throttling/offline) → loading/timeout/thông báo "Backend không khả dụng".

### Tương thích & hiển thị (X-UI)
- X-UI-1 Storefront mobile 390px: header thu gọn, menu hamburger, giỏ/checkout dùng được.
- X-UI-2 Không mojibake, tiền VNĐ, ngày VN, trạng thái tiếng Việt ở cả 2 FE.
- X-UI-3 In hóa đơn VAT (admin) bố cục đúng, số tiền bằng chữ.
- X-UI-4 Quay lại/tiến trình trình duyệt (back/forward) không vỡ trạng thái.

---

# PHẦN E — KIỂM TRA API/DB SAU UI (cả 2 chiều)
- Đơn khách tạo → đúng `Order/OrderLines/UserId`, tổng tiền, trạng thái.
- Tồn khớp sau mua/giao/trả; sổ cái `StockMovements` append-only.
- `Favorites`, `Reviews` (ReviewStatus), `ContactRequests` ghi đúng.
- `Refund/CashTransaction` phát sinh đúng khi hoàn tiền/thu/chi.
- Audit log có actor/thời gian/đối tượng cho mọi mutation (cả hành động khách: tạo đơn, gửi review?).
- Báo cáo lấy số liệu thật, không hardcode.

---

# PHẦN F — XUẤT FILE (Admin)
- Tồn kho / Báo cáo xuất `.xlsx`: mở Excel không lỗi font, header rõ, tiền là number, ngày đúng, dữ liệu khớp UI, tên file có ngày.

---

## G. Mẫu ghi nhận lỗi
| ID | FE | Trang/Modal/Nút/Trường | Dữ liệu test | Mong muốn | Thực tế | Mức độ | Ảnh | Trạng thái |
|---|---|---|---|---|---|---|---|---|
| BUG-001 | Storefront | Checkout / nút Đặt hàng | giỏ rỗng | Chặn + báo | Tạo đơn rỗng | High | … | Open |

Mức độ: **Critical** (mất dữ liệu, sai tiền/tồn, không đăng nhập) · **High** (nghiệp vụ chính lỗi) · **Medium** (có workaround) · **Low** (hiển thị/typo).

---

## H. Checklist hoàn tất
**Storefront**: [ ] A0 khung [ ] A1 Home [ ] A2 DS sản phẩm [ ] A3 Chi tiết+Review [ ] A4 Giỏ [ ] A5 Checkout [ ] A6 Success [ ] A7 Đơn của tôi [ ] A8 Chi tiết đơn+Hủy [ ] A9 Yêu thích [ ] A10 Voucher [ ] A11 Cửa hàng [ ] A12 Login [ ] A13 Register [ ] A14 Account [ ] A15 404
**Admin**: [ ] B1 Bán hàng [ ] B2 SP&Kho [ ] B3 Dịch vụ [ ] B4 Tài chính/BC [ ] B5 Hệ thống
**2 chiều**: [ ] X1 Đặt hàng [ ] X2 Cọc [ ] X3 Review [ ] X4 Liên hệ [ ] X5 Cấu hình [ ] X6 Nội dung [ ] X7 Khách/Yêu thích [ ] X8 Đổi trả
**Đặc biệt**: [ ] Bảo mật X-SEC [ ] Đồng thời X-RACE [ ] Biên X-EDGE [ ] UI X-UI
**Hệ thống**: [ ] API/DB [ ] Xuất Excel [ ] `npm run build` (2 FE) [ ] `dotnet build` [ ] `dotnet test`

## I. Tiêu chí đạt
- 2 FE chạy đủ luồng đầu→cuối; dữ liệu tiền/tồn/đơn/trả/báo cáo không sai logic; đồng bộ 2 chiều đúng.
- Phân quyền khách/Staff/Admin chặt; khách không chạm dữ liệu người khác/endpoint admin.
- Không lỗi giao diện nghiêm trọng, không mojibake; build FE/BE + unit test BE pass.
- Dữ liệu demo đủ phong phú để trình bày.
