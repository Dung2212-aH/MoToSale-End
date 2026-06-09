# V2 Frontend Store - Full User Journey Test Scenarios

> Mục tiêu: đóng vai người dùng thật để quét toàn bộ `frontend-store`, truy cập đủ trang, dùng đủ trường nhập, đủ nút bấm, đủ trạng thái nghiệp vụ phổ biến và các ca đặc biệt/hi hữu. Được phép dùng `frontend-admin` để chuẩn bị dữ liệu, thay đổi trạng thái đơn, tồn kho, voucher, đánh giá và đối chiếu kết quả.

## 1. Phạm vi bắt buộc

### 1.1. Storefront phải truy cập đủ

- `/` - Trang chủ.
- `/products` - Danh sách sản phẩm.
- `/products/:id` - Chi tiết sản phẩm.
- `/he-thong-cua-hang` - Hệ thống cửa hàng.
- `/vouchers` - Voucher của tôi.
- `/favorites` - Yêu thích.
- `/cart` - Giỏ hàng.
- `/checkout` - Thanh toán.
- `/checkout/success` - Thành công đặt hàng.
- `/orders` - Đơn hàng của tôi.
- `/orders/:id` - Chi tiết đơn hàng.
- `/account` - Tài khoản cá nhân.
- `/login` - Đăng nhập.
- `/register` - Đăng ký.
- Route không tồn tại, ví dụ `/abc-not-found-404`.
- Header/footer/mobile menu/social/floating actions.
- Các link nav đang trỏ về `/` như `Liên hệ`, `Câu hỏi thường gặp` vẫn phải bấm để ghi nhận hành vi hiện tại.

### 1.2. Admin được dùng để hỗ trợ test

- `/motorcycles`, `/parts`: tạo/sửa sản phẩm, biến thể, ảnh, trạng thái bán/ngừng bán.
- `/inventory`, `/stock-documents`: chỉnh tồn, làm hết hàng, tạo/duyệt chứng từ kho nếu cần.
- `/vouchers`: tạo voucher hợp lệ, hết hạn, giới hạn lượt, min order, scope sản phẩm/danh mục/hãng.
- `/orders`, `/orders/:id`: xác nhận đơn, ghi nhận thanh toán, phân bổ/xuất kho/giao hàng, hủy đơn.
- `/customers`: đối chiếu khách mới đăng ký và lịch sử đơn.
- `/reviews`: duyệt/ẩn đánh giá sau khi khách gửi.
- `/settings`, `/reports`: đối chiếu cấu hình cửa hàng và số liệu sau mua.

## 2. Rule test bắt buộc

- Không đánh dấu `PASS` nếu chỉ nhìn UI mà chưa bấm nút hoặc chưa nhập field liên quan.
- Mỗi trang phải test: tải lần đầu, reload, đi trang khác quay lại, responsive desktop/tablet/mobile.
- Mỗi form phải nhập: dữ liệu hợp lệ, thiếu field bắt buộc, sai định dạng, dữ liệu dài, ký tự đặc biệt có dấu.
- Mỗi bảng/list/card phải đối chiếu ít nhất 1 giá trị với BE hoặc admin: tên, giá, tồn, trạng thái, tổng tiền, mã đơn.
- Với thao tác tạo dữ liệu, phải kiểm tra cả hai chiều: Storefront tạo -> Admin thấy; Admin cập nhật -> Storefront phản ánh.
- Với lỗi/edge case, phải ghi rõ: bước tái hiện, dữ liệu nhập, expected, actual, ảnh chụp, log network nếu có.
- Sau mỗi luồng mua hàng phải kiểm tra: giỏ hàng, tồn kho, đơn hàng, trạng thái thanh toán, trạng thái giao hàng, lịch sử đơn.

## 3. Tài khoản và dữ liệu nền

- Admin: `admin@motosale.local / Admin@123`.
- Staff: `staff@motosale.local / Staff@123`.
- Customer seed: `customer@motosale.local / Customer@123` nếu DB đã seed theo bộ hiện tại.
- Customer mới: tạo trong từng lần test bằng email dạng `store_e2e_<timestamp>@motosale.local`.
- SĐT hợp lệ: `0900000003`.
- SĐT sai: `12345`, `abcdefghij`, `090000000000000`.
- Địa chỉ test: `120 Nguyễn Trãi, Phường Bến Thành, TP.HCM`.

## 4. Chuẩn bị dữ liệu bằng admin

### SETUP-01. Sản phẩm bình thường còn tồn

- Admin tạo hoặc chọn một xe máy còn tồn, có ảnh chính, có ít nhất 2 biến thể/màu.
- Admin tạo hoặc chọn một phụ tùng còn tồn, có SKU rõ ràng, giá bán hiện tại.
- Storefront phải thấy sản phẩm ở `/products` và `/products/:id`.

### SETUP-02. Sản phẩm hết hàng

- Admin chỉnh tồn một SKU về `0`.
- Storefront chi tiết sản phẩm phải hiển thị hết hàng hoặc chặn thêm giỏ/mua ngay.

### SETUP-03. Sản phẩm bị ẩn/ngừng bán

- Admin đổi trạng thái sản phẩm sang ngừng bán.
- Storefront reload danh sách và detail phải không cho mua hoặc không hiển thị tùy nghiệp vụ hiện tại.

### SETUP-04. Voucher đa dạng

- Voucher hợp lệ toàn đơn, min order thấp.
- Voucher min order cao hơn giỏ hiện tại.
- Voucher hết hạn/chưa tới ngày.
- Voucher giới hạn lượt dùng/per user.
- Voucher scope theo sản phẩm/danh mục/hãng.
- Storefront `/vouchers` và checkout phải phản ánh đúng từng loại.

### SETUP-05. Đơn hàng cho review

- Storefront tạo đơn.
- Admin xác nhận thanh toán và chuyển trạng thái giao hàng/hoàn tất.
- Storefront `/orders/:id` phải mở được nút đánh giá sản phẩm.

## 5. Nhóm kịch bản Storefront

### GUEST-01. Khách vãng lai duyệt toàn site

1. Mở `/`.
2. Bấm logo, Trang chủ, Sản phẩm, Hệ thống cửa hàng, Liên hệ, Câu hỏi thường gặp.
3. Hover/click menu Sản phẩm desktop, bấm từng nhóm Honda/Yamaha/SYM.
4. Mở mobile viewport, bấm `Menu`, mở nhóm Sản phẩm, bấm từng link con.
5. Bấm icon Yêu thích, Giỏ hàng khi chưa đăng nhập.
6. Expected:
   - Không lỗi layout/header/footer.
   - Link protected redirect về `/login?redirect=...`.
   - Link đang trỏ `/` phải quay về trang chủ, không vỡ UI.

### AUTH-01. Đăng nhập sai, đúng, remember

1. Mở `/login`.
2. Submit rỗng.
3. Nhập email sai + password sai.
4. Tick `Ghi nhớ tôi`, nhập customer đúng.
5. Expected:
   - Validate HTML/FE hoạt động.
   - Sai credentials có thông báo tiếng Việt đúng dấu.
   - Đăng nhập thành công quay về redirect nếu có, hoặc `/`.
   - Header đổi sang tên khách, có icon voucher/orders.

### AUTH-02. Đăng ký khách mới

1. Mở `/register`.
2. Submit rỗng.
3. Nhập email sai, SĐT sai, mật khẩu ngắn.
4. Nhập đầy đủ: Họ, Tên, Email mới, SĐT, Mật khẩu.
5. Đăng nhập bằng tài khoản vừa tạo.
6. Admin `/customers` kiểm tra khách mới xuất hiện.
7. Expected:
   - Không tạo trùng email.
   - Customer mới có role customer, không vào được admin.

### PRODUCT-01. Danh sách sản phẩm và filter

1. Mở `/products`.
2. Test search: từ khóa có kết quả, không kết quả, tiếng Việt có dấu/không dấu, ký tự đặc biệt.
3. Test danh mục: tất cả, xe máy, phụ tùng, danh mục con.
4. Test loại xe/phụ tùng nếu select xuất hiện.
5. Test hãng: Honda/Yamaha/SYM và hãng không có sản phẩm nếu có.
6. Test mức giá: dưới 10 triệu, 10-30 triệu, 30-60 triệu, trên 60 triệu.
7. Test sort: mặc định, giá thấp-cao, cao-thấp, tên A-Z, Z-A, hàng mới.
8. Test nút reset filter.
9. Test pagination: trang kế, trang trước, trang cuối nếu có.
10. Expected:
    - Query URL cập nhật đúng.
    - Sản phẩm hiển thị đúng giá/tên/ảnh/tồn.
    - Không lệch card, không tràn text.

### PRODUCT-02. Card sản phẩm

1. Bấm ảnh/tên sản phẩm -> detail.
2. Bấm thêm giỏ từ card khi chưa đăng nhập -> redirect login.
3. Sau đăng nhập, bấm thêm giỏ từ card sản phẩm không có biến thể -> thêm được.
4. Bấm thêm giỏ từ card sản phẩm có biến thể -> nếu yêu cầu chọn biến thể thì redirect detail hoặc báo chọn biến thể.
5. Bấm tim yêu thích: thêm, bấm lại bỏ.
6. Expected:
   - Badge yêu thích/giỏ trên header tăng giảm đúng.
   - Không thêm được sản phẩm hết hàng.

### PRODUCT-03. Chi tiết sản phẩm đầy đủ

1. Mở `/products/:id` của xe máy có nhiều biến thể.
2. Bấm tất cả thumbnail ảnh.
3. Chọn từng phiên bản/màu.
4. Đổi số lượng bằng nút `-`, `+`, nhập trực tiếp: `0`, `-1`, `9999`, chữ, số thập phân, số hợp lệ.
5. Bấm `Thêm vào giỏ hàng`.
6. Bấm `Mua ngay`.
7. Bấm tim yêu thích.
8. Bấm các tab/section: mô tả, thông số, đánh giá, sản phẩm liên quan, phụ kiện mua cùng nếu có.
9. Expected:
   - Ảnh đổi theo biến thể/màu.
   - Giá/tồn/SKU đổi theo biến thể.
   - Quantity bị clamp đúng min/max.
   - `Mua ngay` tạo item và chuyển `/checkout`.

### PRODUCT-04. Phụ tùng tương thích xe

1. Mở phụ tùng có cấu hình tương thích xe.
2. Kiểm tra phần tương thích hiển thị hãng/dòng/năm/ghi chú.
3. Admin sửa tương thích, reload detail.
4. Expected:
   - Storefront phản ánh cấu hình mới.
   - Phụ tùng không có tương thích phải có trạng thái rỗng dễ hiểu.

### STORE-01. Hệ thống cửa hàng

1. Mở `/he-thong-cua-hang`.
2. Chọn tỉnh/thành.
3. Chọn quận/huyện nếu select hiện.
4. Nhập tìm kiếm tên cửa hàng, địa chỉ, từ khóa không có kết quả.
5. Bấm `Xem bản đồ`.
6. Bấm `Chỉ đường`.
7. Bấm hotline/tel link.
8. Reload và quay lại.
9. Expected:
   - Danh sách và số lượng cửa hàng lọc đúng.
   - Map đổi theo cửa hàng.
   - Trạng thái không kết quả không vỡ layout.

### VOUCHER-01. Voucher của tôi

1. Mở `/vouchers` khi chưa đăng nhập -> redirect login.
2. Đăng nhập, mở lại `/vouchers`.
3. Bấm lưu voucher hợp lệ.
4. Bấm lưu lại voucher đã lưu.
5. Admin tắt/hết hạn voucher, reload.
6. Expected:
   - Voucher đã lưu không bị lưu trùng.
   - Số voucher ở header cập nhật.
   - Voucher hết hạn/không active không dùng được ở checkout.

### FAVORITE-01. Yêu thích

1. Từ danh sách/detail thêm 2 sản phẩm vào yêu thích.
2. Mở `/favorites`.
3. Bấm xem chi tiết từng sản phẩm.
4. Bấm bỏ yêu thích.
5. Reload.
6. Expected:
   - Danh sách yêu thích giữ đúng sau reload.
   - Header count đúng.
   - Empty state đúng khi xóa hết.

### CART-01. Giỏ hàng cơ bản

1. Thêm xe máy và phụ tùng vào giỏ.
2. Mở `/cart`.
3. Tăng/giảm số lượng từng dòng.
4. Nhập trực tiếp số lượng hợp lệ.
5. Xóa 1 dòng.
6. Xóa dòng cuối cùng.
7. Bấm tiếp tục mua sắm.
8. Expected:
   - Xóa 1 dòng không xóa toàn bộ giỏ.
   - Tổng tiền, số lượng, header count cập nhật đúng.
   - Giỏ rỗng có empty state và không cho checkout.

### CART-02. Giỏ hàng edge

1. Admin giảm tồn sản phẩm trong giỏ xuống thấp hơn quantity.
2. Storefront tăng quantity vượt tồn.
3. Storefront checkout với item đã hết tồn.
4. Expected:
   - Update quantity hoặc checkout bị chặn rõ ràng.
   - Không tạo đơn âm tồn.

### CHECKOUT-01. Mua bình thường COD giao hàng

1. Từ detail bấm `Mua ngay`.
2. Chọn `Giao hàng tận nơi`.
3. Nhập Họ tên, SĐT, Email, Địa chỉ, Tỉnh/Thành, Phường/Xã, Ghi chú.
4. Chọn `Thanh toán toàn bộ`.
5. Chọn `Thanh toán khi nhận hàng (COD)`.
6. Nhập voucher hợp lệ hoặc chọn voucher gợi ý.
7. Bấm `Đặt hàng`.
8. Mở `/checkout/success`, bấm `Xem đơn hàng`.
9. Admin `/orders` đối chiếu đơn mới.
10. Expected:
    - Đơn tạo thành công, giỏ về 0.
    - Chi tiết đơn hiển thị đúng người nhận, sản phẩm, tổng tiền, COD.
    - Admin thấy đơn ở trạng thái chờ xác nhận/chưa thanh toán/chưa giao.

### CHECKOUT-02. Chuyển khoản ngân hàng

1. Checkout sản phẩm còn tồn.
2. Chọn `Chuyển khoản ngân hàng`.
3. Bấm `Đặt hàng`.
4. Ở màn QR, kiểm tra số tiền, nội dung chuyển khoản, thông tin ngân hàng.
5. Bấm `Tôi đã chuyển khoản`.
6. Admin vào đơn, xác nhận payment claim.
7. Storefront reload `/orders/:id`.
8. Expected:
   - Đơn có trạng thái chờ xác nhận thanh toán trước khi admin xác nhận.
   - Sau admin xác nhận, storefront hiển thị đã thanh toán.

### CHECKOUT-03. Chuyển khoản nhưng trả sau

1. Chọn `Chuyển khoản ngân hàng`.
2. Sau khi hiện QR, bấm `Tôi sẽ thanh toán sau`.
3. Mở chi tiết đơn.
4. Bấm `Tôi đã chuyển khoản` từ chi tiết đơn.
5. Expected:
   - Đơn vẫn được tạo.
   - Chi tiết đơn còn hiển thị khối chuyển khoản cho đơn BankTransfer chưa paid.

### CHECKOUT-04. Nhận tại showroom

1. Chọn `Nhận tại showroom`.
2. Không nhập địa chỉ giao hàng.
3. Nhập `Ngày hẹn nhận`, `Ghi chú giao nhận`.
4. Chọn COD hoặc chuyển khoản.
5. Đặt hàng.
6. Expected:
   - Không bắt buộc địa chỉ giao hàng khi pickup.
   - Chi tiết đơn hiển thị phương thức nhận tại showroom.

### CHECKOUT-05. Đặt cọc

1. Chọn `Đặt cọc trước`.
2. Nhập cọc rỗng, `0`, âm, lớn hơn tổng tiền, bằng tổng tiền.
3. Nhập cọc hợp lệ.
4. Chọn COD/chuyển khoản.
5. Đặt hàng.
6. Admin ghi nhận thanh toán cọc nếu cần.
7. Expected:
   - FE validate cọc sai.
   - Chi tiết đơn hiển thị đặt cọc/còn lại đúng.

### CHECKOUT-06. Voucher edge

1. Nhập voucher sai mã.
2. Nhập voucher min order không đủ.
3. Nhập voucher hết hạn.
4. Nhập voucher đã dùng hết lượt/per user.
5. Nhập voucher đúng scope.
6. Xóa voucher đã áp.
7. Đổi sản phẩm/quantity làm voucher không còn hợp lệ.
8. Expected:
   - Thông báo rõ.
   - Tổng tiền cập nhật đúng sau áp/xóa.
   - Voucher giảm trên giá bán hiện tại sau giá khuyến mại.

### CHECKOUT-07. Validate form

1. Submit rỗng.
2. SĐT sai.
3. Email sai.
4. Tên quá dài, địa chỉ quá dài, ghi chú nhiều dòng.
5. Ký tự đặc biệt/emoji trong ghi chú.
6. Double-click `Đặt hàng` nhanh.
7. Expected:
   - Không tạo đơn trùng.
   - Các field lỗi hiển thị gần field.
   - Dữ liệu dài không làm vỡ layout.

### ORDER-01. Danh sách đơn của tôi

1. Mở `/orders`.
2. Kiểm tra empty state với user mới chưa có đơn.
3. Với user có đơn, kiểm tra list đơn.
4. Bấm từng đơn sang detail.
5. Admin đổi trạng thái đơn, reload list.
6. Expected:
   - Chỉ thấy đơn của chính user.
   - Mã đơn, tổng tiền, trạng thái đúng với admin.

### ORDER-02. Chi tiết đơn và hủy đơn

1. Mở đơn mới chờ xác nhận.
2. Bấm `Hủy đơn hàng`.
3. Đóng modal.
4. Mở lại modal, nhập lý do ngắn, xác nhận hủy.
5. Admin kiểm tra đơn bị hủy.
6. Storefront reload.
7. Expected:
   - Đơn hủy không còn cho hủy lại.
   - Tồn giữ chỗ được giải phóng nếu BE hỗ trợ.
   - Lý do/lịch sử hủy hiển thị hợp lý ở admin.

### ORDER-03. Theo dõi trạng thái giao hàng

1. Tạo đơn mới.
2. Admin chuyển lần lượt: xác nhận -> soạn hàng/phân bổ -> đang giao -> đã giao/hoàn tất.
3. Storefront reload `/orders/:id` sau mỗi bước.
4. Expected:
   - Badge trạng thái đơn/payment/fulfillment đồng bộ.
   - Timeline không hiện `Không xác định`.
   - Ngày giờ hiển thị đúng timezone mong muốn.

### REVIEW-01. Đánh giá sau mua

1. Tạo đơn và admin chuyển hoàn tất.
2. Storefront mở `/orders/:id`, bấm `Đánh giá sản phẩm`.
3. Chọn 1 sao, 3 sao, 5 sao.
4. Nhập tiêu đề, nội dung rỗng, nội dung dài, nội dung hợp lệ.
5. Upload ảnh hợp lệ nhỏ hơn 5MB.
6. Upload file quá 5MB.
7. Xóa ảnh preview.
8. Submit.
9. Admin `/reviews` duyệt đánh giá.
10. Storefront detail sản phẩm kiểm tra đánh giá công khai.
11. Expected:
    - Chưa mua/chưa hoàn tất không được đánh giá.
    - Review pending không công khai nếu nghiệp vụ yêu cầu duyệt.
    - Review approved hiển thị đúng rating, nội dung, ảnh.

### ACCOUNT-01. Hồ sơ cá nhân

1. Mở `/account`.
2. Tab `Thông tin tài khoản`: đổi Họ tên, Email, SĐT.
3. Submit thiếu tên, email sai, phone sai.
4. Submit hợp lệ.
5. Bấm refresh/tải lại thông tin.
6. Admin `/customers` đối chiếu thông tin.
7. Expected:
   - Header name cập nhật.
   - Không cho trùng email.

### ACCOUNT-02. Đổi mật khẩu

1. Tab `Đổi mật khẩu`.
2. Submit rỗng.
3. Nhập mật khẩu hiện tại sai.
4. Nhập mật khẩu mới ngắn.
5. Nhập confirm không khớp.
6. Đổi hợp lệ.
7. Logout, login bằng password cũ và mới.
8. Expected:
   - Password cũ không dùng được, password mới dùng được.
   - Không để lộ password trong UI/log.

### ACCOUNT-03. Địa chỉ nhận hàng

1. Tab `Địa chỉ nhận hàng`.
2. Nhập Tên người nhận, SĐT, Địa chỉ, Phường/Xã, Tỉnh/Thành, Ghi chú.
3. Submit thiếu field bắt buộc.
4. Submit SĐT sai.
5. Submit hợp lệ.
6. Vào checkout kiểm tra có prefill nếu hệ thống hỗ trợ.
7. Expected:
   - Lưu địa chỉ đúng.
   - Không vỡ layout với địa chỉ dài.

### ACCOUNT-04. Đăng xuất

1. Bấm đăng xuất.
2. Vào `/cart`, `/orders`, `/account`, `/favorites`.
3. Expected:
   - Token bị xóa.
   - Protected route redirect login.

### 404-01. Route không tồn tại

1. Mở `/abc-not-found-404`.
2. Bấm nút quay về trang chủ/sản phẩm nếu có.
3. Expected:
   - Không crash.
   - Header/footer vẫn ổn.

## 6. Kịch bản đặc biệt/hi hữu

### EDGE-01. Race condition tồn kho

1. Customer A thêm SKU quantity 2 vào giỏ.
2. Admin giảm tồn SKU xuống 1 hoặc đơn khác mua hết.
3. Customer A checkout.
4. Expected: bị chặn tại update cart hoặc đặt hàng, không âm tồn.

### EDGE-02. Race condition voucher

1. Customer mở checkout, thấy voucher còn hiệu lực.
2. Admin tắt voucher hoặc dùng hết lượt.
3. Customer bấm áp dụng/đặt hàng.
4. Expected: voucher bị từ chối, tổng tiền không giảm sai.

### EDGE-03. Sản phẩm bị xóa/ngừng bán khi đang checkout

1. Customer thêm sản phẩm vào giỏ.
2. Admin xóa/ngừng bán sản phẩm hoặc SKU.
3. Customer checkout.
4. Expected: không tạo đơn lỗi dữ liệu; UI báo sản phẩm không còn bán/không đủ tồn.

### EDGE-04. Khách mở nhiều tab

1. Tab A mở cart.
2. Tab B xóa item hoặc đặt hàng.
3. Tab A reload/checkout.
4. Expected: cart đồng bộ, không checkout giỏ cũ.

### EDGE-05. Double submit

1. Ở checkout, double-click `Đặt hàng` thật nhanh.
2. Kiểm tra admin `/orders`.
3. Expected: chỉ tạo 1 đơn hoặc có cơ chế chống trùng.

### EDGE-06. Token hết hạn

1. Đăng nhập.
2. Xóa token hoặc dùng token hết hạn trong storage.
3. Bấm thêm giỏ/checkout/orders/account.
4. Expected: redirect login, không crash.

### EDGE-07. Dữ liệu dài/cực đoan

1. Tên khách 150 ký tự.
2. Địa chỉ 500 ký tự.
3. Note nhiều dòng, ký tự đặc biệt, tiếng Việt có dấu.
4. Expected: lưu hợp lý hoặc chặn rõ; UI không tràn.

### EDGE-08. Mạng chậm/API lỗi

1. Tắt API hoặc giả lập 500 khi load products/cart/orders.
2. Bấm `Thử lại` ở error state.
3. Expected: loading/error/retry rõ ràng.

## 7. Đối chiếu admin sau mỗi luồng

### CROSS-01. Đơn online

- Storefront order detail phải khớp admin order detail:
  - Mã đơn.
  - Customer.
  - Lines: SKU, tên, số lượng, đơn giá, thành tiền.
  - Subtotal, discount, shipping fee, grand total.
  - Payment method/status.
  - Fulfillment status.
  - Note, địa chỉ nhận hàng.

### CROSS-02. Tồn kho

- Sau checkout, admin `/inventory` phải phản ánh giữ chỗ hoặc giảm tồn theo nghiệp vụ.
- Sau hủy đơn, giữ chỗ phải giải phóng.
- Sau fulfill, tồn thực phải giảm.

### CROSS-03. Voucher

- Sau dùng voucher, admin voucher used count tăng đúng.
- Per-user limit hoạt động.
- Scope voucher áp đúng sản phẩm/danh mục/hãng.

### CROSS-04. Customer

- Customer đăng ký ngoài store xuất hiện trong admin `/customers`.
- Hồ sơ/care note/lịch sử đơn không bị mất.

### CROSS-05. Review

- Review gửi từ store xuất hiện pending trong admin.
- Admin approve -> store product detail thấy review.
- Admin hide/reject -> store không thấy review.

## 8. Checklist nút bấm bắt buộc

- Header: logo, Trang chủ, Sản phẩm menu, từng link dropdown, Liên hệ, Hệ thống cửa hàng, FAQ, đăng nhập, đăng ký, tài khoản, yêu thích, voucher, đơn hàng, giỏ hàng, mobile Menu.
- Home: banner/link chính, category cards, product cards, service sections, footer links, social links, floating actions.
- Product list: search, category, type, brand, price, sort, reset, pagination, product detail, add cart, favorite.
- Product detail: thumbnail, variant/version, color, quantity minus/plus/input, add cart, buy now, favorite, related products, tabs/reviews.
- Store system: city select, district select, search, xem bản đồ, chỉ đường, hotline.
- Voucher: lưu voucher, trạng thái đã lưu, dùng ở checkout.
- Cart: quantity minus/plus/input, xóa item, quay lại mua sắm, checkout.
- Checkout: all radio pills, all fields, voucher apply/remove/suggested, submit, QR transfer buttons, back cart.
- Success: xem đơn hàng, trang chủ, tiếp tục mua sắm, xem tất cả đơn hàng.
- Orders: xem detail, empty state CTA.
- Order detail: hủy đơn modal, đóng modal, xác nhận hủy, tôi đã chuyển khoản, đánh giá sản phẩm, quay lại orders, tiếp tục mua sắm.
- Account: tab profile/password/address, submit từng tab, reload info, logout.
- Auth: submit login/register, remember me, link qua lại login/register.

## 9. Acceptance criteria

- Không bỏ sót route storefront nào.
- Không bỏ sót field nhập nào trên login/register/product/cart/checkout/account/review/store filters.
- Không bỏ sót nút bấm chính nào.
- Ít nhất 1 đơn COD giao hàng pass end-to-end.
- Ít nhất 1 đơn chuyển khoản pass end-to-end.
- Ít nhất 1 đơn pickup pass end-to-end.
- Ít nhất 1 đơn đặt cọc pass end-to-end.
- Ít nhất 1 luồng hủy đơn pass.
- Ít nhất 1 luồng admin xác nhận thanh toán/giao hàng và khách xem trạng thái pass.
- Ít nhất 1 luồng review sau mua, admin duyệt, store hiển thị pass.
- Các ca hi hữu không gây crash, không tạo dữ liệu sai, không âm tồn, không tạo đơn trùng.
- Sau test, đối chiếu admin và DB cho đơn/tồn/voucher/customer/review.

## 10. Mẫu ghi kết quả

| ID | Vai | Trang | Dữ liệu | Expected | Actual | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| CHECKOUT-01 | Customer | `/checkout` | COD, giao hàng | Tạo 1 đơn, giỏ về 0 |  | Pending | screenshot/network/admin |

## 11. Thứ tự chạy đề xuất

1. SETUP bằng admin.
2. Guest browsing + auth/register.
3. Product list/detail/favorite/cart.
4. Checkout COD.
5. Checkout chuyển khoản.
6. Checkout pickup.
7. Checkout đặt cọc.
8. Orders/cancel/status tracking.
9. Review after purchase.
10. Account profile/password/address/logout.
11. Edge cases race/voucher/stock/multi-tab/token/API error.
12. Admin cross-check + cleanup dữ liệu test.

## 12. Bổ sung sau rà soát plan

> Mục này là phần bổ sung bắt buộc sau khi đối chiếu test plan với route, field, button và component thật của `v2/frontend-store`. Khi chạy test, không được bỏ qua các checklist dưới đây dù các mục trước đã pass.

### 12.1. Điều chỉnh kỳ vọng route và trạng thái public/protected

- `/vouchers` hiện là route public: khách vãng lai phải xem được danh sách voucher khả dụng nếu BE trả dữ liệu.
- Với `/vouchers`, chỉ thao tác `Nhận` voucher mới yêu cầu đăng nhập; không được đánh fail vì guest không bị redirect login khi vừa mở trang.
- `/cart`, `/favorites`, `/checkout`, `/checkout/success`, `/orders`, `/orders/:id`, `/account` là protected routes: guest phải bị redirect login đúng redirect URL.
- `/login` và `/register` là public-only routes: user đã đăng nhập mở các route này phải bị đưa về `/`.
- Route 404 nằm ngoài `MainLayout`: phải kiểm tra rõ trạng thái hiển thị header/footer theo thiết kế thật, không mặc định kỳ vọng giống các page thường.
- Header/footer có một số link đang trỏ `/` hoặc `#`: phải bấm và ghi nhận hành vi thật, không tự giả định có trang `/contact` hoặc `/faq`.

### 12.2. Bổ sung field bắt buộc phải test

- Checkout: nút `Dùng thông tin tài khoản`, kiểm tra prefill họ tên, số điện thoại, email, địa chỉ nếu profile/address có dữ liệu.
- Checkout: phân biệt `fulfillmentNote` (ghi chú giao nhận) và `note` (ghi chú đơn hàng); nhập cả hai, tạo đơn, đối chiếu admin/order detail.
- Checkout pickup: `pickupAppointmentAt` phải test ngày hợp lệ, ngày quá khứ, ngày quá xa, bỏ trống, format lỗi nếu có thể nhập thủ công.
- Checkout deposit: `depositAmount` phải test rỗng, `0`, âm, chữ, số thập phân, bằng tổng tiền, lớn hơn tổng tiền, hợp lệ nhỏ hơn tổng tiền.
- Product filters: khi chọn danh mục `Phụ tùng`, phải test field `compatibleCarModelId` / `Loại xe tương thích`.
- Product filters: khi chọn `Xe máy`, phải test field `vehicleTypeCategoryId` / `Loại xe`.
- Account profile: email đang `readOnly`; test đúng là không sửa được email, không test đổi email/trùng email qua store.
- Account address: scope hiện tại là một địa chỉ mặc định; test lưu/ghi đè một địa chỉ, không giả định CRUD nhiều địa chỉ nếu UI chưa có.
- Footer newsletter: test input email nhận tin khuyến mãi với rỗng, sai định dạng, hợp lệ, submit.
- Review product detail form: ngoài review modal từ order detail, phải test form đánh giá trong tab/section đánh giá ở trang chi tiết sản phẩm.

### 12.3. Bổ sung nút và tương tác còn thiếu

- Product gallery: bấm nút ảnh trước, ảnh sau, từng thumbnail, và ảnh fallback khi không có ảnh.
- Product detail: bấm từng button phiên bản, từng button màu, favorite, `Thêm vào giỏ hàng`, `Mua ngay`, từng tab `Mô tả`, `Thông số`, `Đánh giá sản phẩm`.
- Product related: bấm card sản phẩm liên quan, favorite, thêm giỏ từ card liên quan nếu có.
- Store system: bấm từng card cửa hàng trong danh sách, kiểm tra active store và map đổi đúng.
- Store system: bấm `Thử tải lại` khi giả lập API lỗi.
- Store system: bấm `Xem bản đồ`, `Chỉ đường`, hotline/tel link và kiểm tra URL mở ra.
- Product list pagination: UI hiện chỉ có nút số trang; test từng nút số trang đang có, không kỳ vọng prev/next/last nếu UI chưa render.
- Cart: test nút `THANH TOÁN` khi giỏ rỗng trong trạng thái đã đăng nhập.
- Cart: test `Tiếp tục mua sắm` hoặc CTA empty cart nếu có.
- Order list: test nút `Thử lại` khi API lỗi, `Mua sắm ngay` ở empty state, và click từng order card.
- Order detail: test nút `Thử lại` khi load đơn lỗi.
- Order detail: test nút đóng `X` hoặc `Đóng` của modal review/cancel nếu modal có cả hai.
- Review modal: test đóng bằng `X`, đổi sao, upload ảnh, xóa ảnh preview, submit.
- Floating actions: bấm hotline và Messenger, kiểm tra href/URL không làm vỡ trang.

### 12.4. Bổ sung kịch bản nghiệp vụ cần có

- Voucher guest: guest xem danh sách voucher khả dụng, bấm `Nhận` thì được yêu cầu đăng nhập.
- Voucher saved: user bấm `Nhận`, reload, quay trang khác rồi quay lại, voucher vẫn ở trạng thái đã nhận và header count đúng.
- Voucher amount vs percent: test voucher giảm tiền cố định, giảm phần trăm, max discount, min order.
- Voucher trên giá sau khuyến mại: tạo sản phẩm có giá gốc và giá bán hiện tại; voucher phải giảm trên giá bán hiện tại sau khuyến mại.
- Voucher đổi cart sau apply: apply voucher xong tăng/giảm/xóa item làm điều kiện voucher không còn hợp lệ, tổng tiền phải cập nhật đúng.
- Product image by variant: test sản phẩm có ảnh theo biến thể và sản phẩm chỉ có ảnh chung.
- Product without variant image: chọn biến thể không có ảnh riêng thì gallery fallback hợp lý.
- Product part manufacturer: phụ tùng có hãng sản xuất phụ tùng nhưng không có hãng xe trực tiếp phải hiển thị dễ hiểu.
- Product hidden/out-of-stock during checkout: admin ngừng bán hoặc đưa tồn về 0 khi khách đang ở cart/checkout, store phải chặn tạo đơn sai.
- Checkout shipping fee: nếu BE/FE có phí ship khác 0, phải test tổng tiền, order detail và admin đều khớp.
- Checkout back navigation: từ checkout quay lại cart rồi trở lại checkout, dữ liệu giỏ/voucher/form không sai.
- Checkout success direct access: mở thẳng `/checkout/success` khi không có order vừa tạo, UI phải xử lý hợp lý.
- Bank transfer QR: kiểm tra số tiền QR, nội dung chuyển khoản, bank code/account name/account number khớp cấu hình cửa hàng.
- Order history/status: sau mỗi bước admin xác nhận, thanh toán, giao hàng, khách reload thấy badge và lịch sử/timeline đúng.
- Review repeat: sau khi đã review một sản phẩm, user không tạo trùng review; nếu UI cho sửa review ở product detail thì update phải về pending lại.
- Review moderation: pending không public; approve thì public; hide/reject thì không public.
- Multi-user isolation: user A không xem được đơn, cart, favorite, voucher đã nhận của user B.
- Auth storage: remember me lưu đúng storage, logout xóa token, token hết hạn redirect login.

### 12.5. Bổ sung kiểm tra dữ liệu, responsive và cleanup

- Với mọi bảng/list/card có số tiền, phải đối chiếu `unitPrice`, `lineTotal`, `subtotal`, `discount`, `shippingFee`, `grandTotal` với API/admin.
- Với mọi sản phẩm trong cart/order, phải đối chiếu `productId`, `skuId`, `skuCode`, tên sản phẩm, số lượng và ảnh.
- Responsive bắt buộc thêm kiểm tra horizontal overflow ở `/account`, `/cart`, `/checkout`, `/orders/:id`, `/products/:id`.
- Dữ liệu dài bắt buộc test email dài, tên dài, địa chỉ dài, note nhiều dòng, tiếng Việt có dấu và ký tự đặc biệt.
- Sau test mutation phải cleanup: xóa review test, khôi phục tồn kho, đưa voucher test về trạng thái ban đầu hoặc ghi rõ voucher đã tăng usage, hủy/đánh dấu đơn test, xóa ảnh/upload test nếu cần.
- Sau cleanup phải chạy smoke API/admin đối chiếu lại: cart không còn dữ liệu rác, tồn kho không âm, review test không public, voucher không còn sai trạng thái.
