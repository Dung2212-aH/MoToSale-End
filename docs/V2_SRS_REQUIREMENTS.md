# Đặc tả yêu cầu phần mềm (SRS) — Hệ thống quản lý cửa hàng xe máy MoToSale v2

Phiên bản: 1.0 · Ngày: 04/06/2026

---

## 1. Giới thiệu

### 1.1 Mục đích
Tài liệu mô tả **yêu cầu chức năng và phi chức năng** của hệ thống quản trị (admin) cửa hàng kinh doanh **xe máy & phụ tùng** MoToSale v2: bán hàng, kho, hậu mãi, tài chính và báo cáo.

### 1.2 Phạm vi
- **Trong phạm vi**: quản lý sản phẩm/biến thể, bán hàng online & tại quầy (POS), tồn kho 1 cửa hàng, cung ứng/mua hàng, đổi trả/hoàn tiền, bảo hành, sửa chữa, chăm sóc khách hàng, thu chi/công nợ, báo cáo, phân quyền, kiểm toán, hóa đơn GTGT (bản in).
- **Ngoài phạm vi**: cổng thanh toán/ví điện tử trực tuyến, hóa đơn điện tử hợp pháp (mã CQT), vận chuyển tích hợp đơn vị giao hàng, đa chi nhánh/đa kho.

### 1.3 Đối tượng đọc
Giảng viên chấm đồ án, nhóm phát triển, người kiểm thử, người vận hành cửa hàng.

### 1.4 Thuật ngữ
| Từ | Nghĩa |
|---|---|
| SKU | Biến thể sản phẩm (mã hàng cụ thể: màu/phiên bản) |
| POS | Bán hàng tại quầy (Point of Sale) |
| Giữ chỗ (Reserved) | Tồn kho bị giữ cho đơn cọc, chưa xuất thật |
| Tồn khả dụng | OnHand − Reserved |
| Công nợ | Số tiền khách còn phải trả / cửa hàng còn phải trả NCC |
| COGS | Giá vốn hàng bán |

---

## 2. Tổng quan hệ thống

### 2.1 Mô tả
Hệ thống web quản trị giúp cửa hàng vận hành toàn bộ chuỗi: **Mua → Nhập kho → Bán → Hậu mãi → Thu/chi → Báo cáo**, dữ liệu tiền và tồn được đồng bộ xuyên suốt.

### 2.2 Vai trò người dùng (Actors)
| Vai trò | Mô tả | Quyền chính |
|---|---|---|
| **Admin (Quản trị)** | Chủ/quản lý cửa hàng | Toàn quyền: cấu hình, tài khoản, tài chính, mua hàng, master-data, báo cáo |
| **Nhân viên (Staff)** | Nhân viên bán hàng/kỹ thuật | Tác nghiệp: bán hàng/POS, đổi trả, bảo hành, sửa chữa, CSKH, chấm công; **không** truy cập tài chính/tài khoản/nhật ký/import |
| **Khách hàng** | Người mua (qua website) | Đặt đơn online, gửi đánh giá/liên hệ (ngoài phạm vi admin) |

### 2.3 Kiến trúc tóm tắt
Microservices: **ApiGateway** (Ocelot) → **AuthService** (xác thực/JWT, tài khoản) + **APIService** (nghiệp vụ); **1 CSDL** dùng chung; FE admin React. (Chi tiết ở tài liệu Kiến trúc.)

---

## 3. Yêu cầu chức năng (Functional Requirements)

> Quy ước mã: **FR-<MODULE>-<số>**.

### 3.1 Xác thực & phân quyền (AUTH)
- **FR-AUTH-01** Đăng nhập bằng email + mật khẩu, cấp JWT; từ chối sai thông tin.
- **FR-AUTH-02** Phân quyền theo vai trò Admin/Staff; chặn Staff truy cập chức năng Admin-only (cả trên menu lẫn API).
- **FR-AUTH-03** Quản lý tài khoản (Admin): tạo/sửa/khóa-mở Staff; không tự xóa tài khoản đang đăng nhập; không xóa Admin hoạt động cuối cùng.

### 3.2 Danh mục & sản phẩm (CAT)
- **FR-CAT-01** CRUD **Danh mục** (cây cha–con), **Hãng xe**, **Dòng xe**, **Hãng sản xuất phụ tùng** (kèm logo).
- **FR-CAT-02** CRUD **Sản phẩm** (xe máy / phụ tùng): mã, tên, danh mục, hãng/dòng (xe), hãng SX (phụ tùng), giá, trạng thái.
- **FR-CAT-03** Quản lý **biến thể (SKU)**: mã SKU, màu/phiên bản, giá, barcode; quản lý **ảnh**, **tương thích xe** (phụ tùng), **sản phẩm bán kèm**.
- **FR-CAT-04** Lọc/tìm sản phẩm theo danh mục, hãng, trạng thái, tồn, khoảng giá.

### 3.3 Bán hàng & đơn (SALE)
- **FR-SALE-01** **Bán tại quầy (POS)**: chọn SKU (mã/tên/barcode), sửa SL & giá, chọn khách lẻ hoặc **khách quen** (tra theo SĐT), áp voucher, thu tiền; **bán đứt** (trừ kho ngay) hoặc **đặt cọc** (giữ chỗ).
- **FR-SALE-02** **Đơn online**: tiếp nhận đơn từ giỏ hàng (Chờ thanh toán).
- **FR-SALE-03** Vòng đời đơn: Chờ thanh toán → Xác nhận → Soạn hàng/Xuất kho → Giao → Hoàn tất; có thể **Hủy**.
- **FR-SALE-04** **Ghi nhận thanh toán** thủ công nhiều đợt (cọc / phần còn lại / đủ); cập nhật trạng thái thanh toán & công nợ.
- **FR-SALE-05** **Giao hàng & xuất kho**: chốt đơn cọc → trừ tồn thật, nhả giữ chỗ, Hoàn tất (nếu đã thu đủ).
- **FR-SALE-06** **Sửa đơn**: sửa thông tin khách/giao + ghi chú; sửa **sản phẩm** chỉ khi đơn còn *Chờ thanh toán* (tự tính lại tiền/giữ chỗ).
- **FR-SALE-07** **Voucher**: CRUD mã giảm giá (%/số tiền, đơn tối thiểu, hạn mức, thời hạn); áp dụng khi tạo đơn.
- **FR-SALE-08** **Hóa đơn GTGT (VAT)**: in bản thể hiện (tách thuế, số tiền bằng chữ) từ chi tiết đơn + thông tin cửa hàng.

### 3.4 Khách hàng (CUST)
- **FR-CUST-01** CRUD khách hàng; ghi chú chăm sóc; xem lịch sử mua.
- **FR-CUST-02** Tự tạo "Khách lẻ" khi bán POS không chọn khách.

### 3.5 Kho & cung ứng (INV)
- **FR-INV-01** Xem tồn theo SKU: **Tồn thực / Đang giữ / Khả dụng / Ngưỡng**; lọc theo trạng thái tồn; xuất Excel.
- **FR-INV-02** **Điều chỉnh tồn** và **chứng từ kho** (nhập/xuất/điều chỉnh) có duyệt; **sổ cái kho bất biến** (append-only).
- **FR-INV-03** Đặt **ngưỡng cảnh báo**, đồng bộ tồn theo sổ cái, xem lịch sử biến động.
- **FR-INV-04** **Cung ứng**: CRUD Nhà cung cấp; **đơn mua** (Nháp → Duyệt → Nhận hàng (tồn tăng) → Thanh toán NCC); theo dõi công nợ NCC.

### 3.6 Hậu mãi & dịch vụ (SVC)
- **FR-SVC-01** **Đổi trả & hoàn tiền**: tạo phiếu trả từ đơn đã giao/hoàn tất; duyệt → **hoàn tồn** (hàng bán lại được) + **sinh phiếu hoàn tiền** + **ghi chi quỹ**; có thể từ chối.
- **FR-SVC-02** **Bảo hành**: tạo phiếu (số khung/số máy, lỗi, chi phí dự kiến/thực tế), dòng thời gian trạng thái; sửa thông tin khi mới tiếp nhận.
- **FR-SVC-03** **Sửa chữa**: tạo phiếu (kèm phụ tùng), luồng Nhận → Kiểm tra → Báo giá → Sửa (xuất kho phụ tùng) → Bàn giao; sửa thông tin khi mới tiếp nhận.
- **FR-SVC-04** **CSKH**: lịch chăm sóc/tương tác (tạo/sửa/hoàn thành/hủy).

### 3.7 Tài chính (FIN)
- **FR-FIN-01** **Sổ quỹ**: phiếu thu/chi; **tự ghi quỹ** khi thu tiền khách, hoàn tiền, thanh toán NCC; **đảo phiếu** để điều chỉnh.
- **FR-FIN-02** **Công nợ khách**: tổng hợp đơn còn phải thu (sau thanh toán & hoàn tiền).

### 3.8 Báo cáo & hệ thống (SYS)
- **FR-SYS-01** **Báo cáo**: doanh thu theo ngày, **lãi gộp/giá vốn (COGS)**, top sản phẩm, trạng thái đơn, mua hàng, thu chi, công nợ, dịch vụ, cảnh báo tồn; lọc theo kỳ; **xuất Excel**.
- **FR-SYS-02** **Nhật ký kiểm toán**: tự ghi mọi thao tác tạo/sửa/xóa/duyệt (actor, thời gian, đối tượng).
- **FR-SYS-03** **Cấu hình vận hành**: tên cửa hàng, MST, thuế suất VAT, ngưỡng tồn, chính sách…
- **FR-SYS-04** **Nhân sự / Ca làm**: phân ca (Admin), **chấm công** check-in/out.
- **FR-SYS-05** **Import dữ liệu** sản phẩm hàng loạt (XLSX) (Admin).

---

## 4. Use cases tiêu biểu

### UC-01 Bán hàng tại quầy (POS) — Actor: Nhân viên/Admin
1. Chọn sản phẩm (tìm mã/tên/barcode), thêm vào đơn, chỉnh SL/giá.
2. Chọn khách: để trống = khách lẻ; hoặc tra **khách quen** theo SĐT.
3. (Tùy) áp voucher. Chọn **bán đứt** hoặc **đặt cọc** + nhập tiền cọc.
4. Thu tiền → tạo đơn.
- *Bán đứt thu đủ*: đơn **Hoàn tất**, trừ kho ngay, ghi thu quỹ.
- *Đặt cọc*: đơn **Đã xác nhận/Đã đặt cọc**, giữ chỗ tồn, công nợ = phần còn lại.
5. (Tùy) In **Hóa đơn VAT**.

### UC-02 Tất toán & giao đơn cọc — Actor: Nhân viên/Admin
1. Mở đơn cọc → **Ghi nhận thanh toán → Thu phần còn lại** → đơn **Đã thanh toán**.
2. **Giao hàng & xuất kho** → trừ tồn thật, nhả giữ chỗ, đơn **Hoàn tất**.
- *Ngoại lệ*: khách bỏ cọc → **Hủy đơn** (nhả giữ chỗ, mặc định khách mất cọc).

### UC-03 Đổi trả & hoàn tiền — Actor: Nhân viên/Admin
1. Tạo phiếu trả từ đơn đã giao, chọn sản phẩm + tình trạng (bán lại/hư hỏng/bảo hành).
2. **Duyệt** → hàng bán lại được **nhập về kho**, **sinh phiếu hoàn tiền**, **ghi chi quỹ**; công nợ điều chỉnh.

### UC-04 Mua hàng nhập kho — Actor: Admin
1. Tạo Nhà cung cấp → **Đơn mua** → **Duyệt** → **Nhận hàng** (tồn tăng) → **Thanh toán NCC** (chi quỹ).

*(Các UC khác: quản lý sản phẩm/biến thể, bảo hành, sửa chữa, voucher, báo cáo, phân quyền — theo mục 3.)*

---

## 5. Quy tắc nghiệp vụ (Business Rules)

- **BR-01 (1 kho)** Toàn hệ thống dùng **một kho duy nhất**; không có khái niệm StoreId.
- **BR-02 (Tồn khả dụng)** Khả dụng = Tồn thực − Đang giữ chỗ; mọi thao tác bán/giữ chỗ kiểm theo khả dụng.
- **BR-03 (Sổ cái bất biến)** Biến động kho ghi vào StockMovement **chỉ thêm, không sửa/xóa**.
- **BR-04 (Doanh thu)** Chỉ tính cho đơn **đã thanh toán đủ** và **đã giao/hoàn tất**. **Lãi gộp** = doanh thu − giá vốn bình quân (từ phiếu nhập).
- **BR-05 (Đặt cọc)** 0 < tiền cọc < tổng tiền; đơn cọc giữ chỗ tồn, chỉ trừ kho thật khi **giao hàng**; hủy đơn cọc mặc định **khách mất cọc**.
- **BR-06 (Trạng thái thanh toán)** Chưa thu → *Chưa thanh toán*; thu một phần → *Đặt cọc/Một phần*; thu đủ → *Đã thanh toán*. Thu đủ + đã giao → đơn **Hoàn tất**.
- **BR-07 (Tự ghi quỹ)** Thu tiền khách = thu quỹ; hoàn tiền/thanh toán NCC = chi quỹ; hủy phiếu thu/chi = đảo phiếu.
- **BR-08 (Loại thanh toán theo trạng thái)** Đơn đã cọc/đã thu một phần chỉ cho **Thu phần còn lại/Trả góp**, không cho thu vượt số nợ.
- **BR-09 (Chặn xóa)** Không xóa cứng đối tượng đã phát sinh giao dịch: **Voucher đã dùng**, **SKU đã có đơn/tồn**, **Tài khoản đã có đơn**, **Danh mục/Hãng còn sản phẩm/con** → chặn, gợi ý "ngừng/khóa". Sản phẩm/ca làm dùng **xóa mềm**.
- **BR-10 (Chặn sửa)** Dữ liệu giao dịch không sửa trực tiếp: **đơn** chỉ sửa sản phẩm khi *Chờ thanh toán*; **bảo hành/sửa chữa** chỉ sửa thông tin khi *mới tiếp nhận*; tài chính/kho/thanh toán không sửa, chỉ hành động bù trừ (hủy/đảo/điều chỉnh).
- **BR-11 (Phân quyền)** Tài chính, tài khoản, nhật ký, import, phiếu nhập kho, nhà cung cấp, ca làm, đơn mua-thanh toán = **Admin**; tác nghiệp bán hàng/dịch vụ = Staff.
- **BR-12 (Kiểm toán)** Mọi thao tác thay đổi dữ liệu đều ghi nhật ký.

---

## 6. Yêu cầu phi chức năng (Non-functional)

- **NFR-01 Bảo mật**: xác thực JWT; phân quyền theo vai trò ở cả API; mật khẩu băm (PBKDF2). *(Khi production: HTTPS + secrets ra biến môi trường.)*
- **NFR-02 Toàn vẹn dữ liệu**: giao dịch (transaction) cho thao tác đa bước (bán/đặt cọc/đổi trả/nhập hàng); ràng buộc khóa ngoại; sổ cái bất biến.
- **NFR-03 Khả dụng/Hiệu năng**: API phản hồi nhanh với dữ liệu cỡ cửa hàng; FE tách route (lazy-load) giảm tải khởi động.
- **NFR-04 Khả dùng (Usability)**: giao diện tiếng Việt nhất quán; tiền VNĐ, ngày giờ định dạng VN; thông báo lỗi rõ ràng; trạng thái rỗng/đang tải.
- **NFR-05 Khả bảo trì**: kiến trúc phân lớp (Common/Entities/DTO/Repository/Services); FE service tách theo domain.
- **NFR-06 Xuất dữ liệu**: báo cáo/danh sách xuất **Excel .xlsx**; hóa đơn/phiếu in được.
- **NFR-07 Kiểm thử**: có unit/integration test backend + quy trình & báo cáo kiểm thử E2E.

---

## 7. Ràng buộc & giả định
- **C-01** Nền tảng: .NET 8 + EF Core + SQL Server; FE React + Vite; chạy nội bộ (LAN/localhost) cho phạm vi đồ án.
- **C-02** Thanh toán **thủ công** (tiền mặt/chuyển khoản ghi tay), không tích hợp cổng.
- **C-03** Một cửa hàng, một kho.
- **A-01** Giá bán đã **gồm VAT** (hóa đơn tách ngược thuế).
- **A-02** Dữ liệu khởi tạo (seed) gồm tài khoản Admin/Staff và dữ liệu mẫu.
