# Quy trình kiểm thử tay (Manual Test Runbook) — MoToSale v2

Phiên bản: 1.0 · Ngày: 04/06/2026
Đây là **kịch bản chạy tuần tự** để 1 người tự test toàn hệ thống bằng tay. Mỗi bước có **thao tác → kết quả mong đợi → ô ✅/❌**. Làm theo thứ tự (dữ liệu bước trước dùng cho bước sau).

Tài liệu liên quan: `V2_FULLSTACK_TEST_PLAN_USER_ADMIN.md` (kế hoạch phủ chi tiết theo trang), `V2_USER_MANUAL.md`, `V2_ADMIN_PAGES_GUIDE.md`.

---

## 0. Chuẩn bị

### 0.1 Khởi động hệ thống
- [ ] Backend chạy (AuthService 5101, APIService 5102, ApiGateway 5100). Kiểm: mở `http://localhost:5100/health/api` và `/health/auth` → trả 200/OK.
- [ ] Storefront (khách): `http://localhost:5174`
- [ ] Admin (quản trị): `http://localhost:5176`
- [ ] Mở **DevTools (F12) → Console + Network** ở cả 2 tab để bắt lỗi 4xx/5xx và lỗi JS.

### 0.2 Tài khoản
| Vai trò | Email | Mật khẩu | Dùng ở |
|---|---|---|---|
| Admin | `admin@motosale.local` | `Admin@123` | 5176 |
| Nhân viên | `staff@motosale.local` | `Staff@123` | 5176 |
| Khách | `customer@motosale.local` | `Customer@123` | 5174 |

### 0.3 Cấu hình ngân hàng (để test chuyển khoản QR)
- [ ] Admin → **Cấu hình vận hành** → điền `Ngân hàng`, `Mã ngân hàng VietQR` (VCB/TCB…), `Số tài khoản`, `Tên chủ tài khoản` → **Lưu cấu hình**.

### 0.4 Quy ước ghi lỗi
Khi lỗi, ghi: **trang · thao tác · dữ liệu nhập · kết quả mong đợi · kết quả thực tế · ảnh · mức độ** (Critical/High/Medium/Low).

---

# PHẦN A — STOREFRONT (KHÁCH HÀNG) · http://localhost:5174

## A1. Đăng ký & Đăng nhập
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A1.1 | Vào `/register`, đăng ký email mới + mật khẩu | Tạo tài khoản, tự đăng nhập hoặc chuyển login | |
| A1.2 | Đăng ký lại đúng email đó | Báo lỗi "Email đã được sử dụng" | |
| A1.3 | `/login` sai mật khẩu | Báo lỗi, không vào được | |
| A1.4 | `/login` đúng `customer@motosale.local / Customer@123` | Vào trang chủ, header hiện tên/menu tài khoản | |
| A1.5 | Đang đăng nhập, gõ URL `/login` | Tự chuyển về trang chủ (không cho vào lại) | |

## A2. Trang chủ & Danh sách sản phẩm
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A2.1 | Mở trang chủ `/` | Banner + sản phẩm nổi bật hiển thị, không lỗi console | |
| A2.2 | Vào `/products` | Lưới sản phẩm, **giá hiển thị đúng (không phải 0đ)** | |
| A2.3 | Sản phẩm **không có giá KM** | Hiện đúng giá gốc, không gạch giá | |
| A2.4 | Sản phẩm **có giá KM** | Hiện giá KM + gạch giá gốc + % giảm | |
| A2.5 | Lọc theo danh mục / sắp xếp giá tăng-giảm | Kết quả đổi đúng | |
| A2.6 | Tìm từ khóa không có | Hiện trạng thái rỗng rõ ràng | |
| A2.7 | Bấm tim **Yêu thích** trên thẻ SP | Thêm vào yêu thích, badge tăng (nếu chưa login → nhắc đăng nhập) | |

## A3. Chi tiết sản phẩm
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A3.1 | Click 1 sản phẩm → trang chi tiết | **Trang hiển thị đầy đủ** (tên, giá, ảnh/placeholder, mô tả, tabs) — không trắng trang | |
| A3.2 | Sản phẩm còn tồn | Nút **Thêm vào giỏ** & **Mua ngay** bật được; hiện "Tồn kho: N" | |
| A3.3 | Chọn biến thể (màu/phiên bản) nếu có | Giá/ảnh đổi theo biến thể | |
| A3.4 | Tăng/giảm số lượng | Số lượng đổi, không vượt tồn | |
| A3.5 | **Thêm vào giỏ** | Toast "Đã thêm vào giỏ", badge giỏ +1 | |
| A3.6 | Khu **Đánh giá**: chưa mua SP này | Hiện "cần mua trước khi đánh giá" / không cho gửi | |

## A4. Giỏ hàng (`/cart`)
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A4.1 | Vào `/cart` | Liệt kê item, tạm tính/tổng đúng | |
| A4.2 | Tăng/giảm số lượng item | Thành tiền + tổng cập nhật | |
| A4.3 | Xóa 1 item | Item biến mất, các item khác còn nguyên | |
| A4.4 | Đăng xuất rồi vào `/cart` | Redirect `/login` (route bảo vệ) | |

## A5. Thanh toán — COD
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A5.1 | Có hàng trong giỏ → vào `/checkout` | Form giao hàng **tự điền sẵn** họ tên/SĐT/email/địa chỉ từ hồ sơ | |
| A5.2 | Bấm nút **"Điền từ hồ sơ"** sau khi xóa vài ô | Các ô được điền lại từ hồ sơ + địa chỉ mặc định | |
| A5.3 | Chọn phương thức **COD** → **Đặt hàng** | Tạo đơn → chuyển trang "Đặt hàng thành công" | |
| A5.4 | Bỏ trống Họ tên/SĐT → Đặt hàng | Chặn + báo lỗi từng trường | |
| A5.5 | Sau đặt hàng, mở lại `/cart` | Giỏ đã rỗng, badge về 0 | |

## A6. Thanh toán — Chuyển khoản (QR)
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A6.1 | Thêm hàng → `/checkout` → chọn **Chuyển khoản** → **Đặt hàng** | Hiện **modal QR** + thông tin TK ngân hàng + số tiền + nội dung CK (mã đơn) | |
| A6.2 | Bấm **"Tôi đã chuyển khoản"** | Chuyển trang thành công ở trạng thái **chờ xác nhận** | |
| A6.3 | (Hoặc) bấm **"Thanh toán sau"** | Vẫn tạo đơn, về trang thành công | |
| A6.4 | Vào **Đơn hàng của tôi → đơn vừa tạo** | Hiện thẻ **"Đang chờ cửa hàng xác nhận thanh toán"** | |

> ⏸ Giữ đơn này để sang **Phần C (2 chiều)** admin xác nhận.

## A7. Đơn của tôi & Hủy đơn
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A7.1 | `/orders` | Chỉ thấy **đơn của mình**, mã/tổng/trạng thái đúng | |
| A7.2 | Mở chi tiết 1 đơn còn *Chờ thanh toán* → **Hủy đơn** (nhập lý do) | Đơn chuyển **Đã hủy** | |
| A7.3 | **Bảo mật:** sửa URL `/orders/<id-không-phải-của-mình>` | Bị chặn (không xem được) | |

## A8. Yêu thích & Tài khoản
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A8.1 | `/favorites` | Hiện SP đã thích (kèm ảnh/tên/giá) | |
| A8.2 | Bỏ thích | SP biến mất, badge giảm | |
| A8.3 | `/account` → sửa Họ tên/SĐT → Lưu | Lưu thành công, hiển thị lại đúng | |
| A8.4 | Đổi mật khẩu (sai mật khẩu hiện tại) | Bị chặn | |
| A8.5 | Đổi mật khẩu đúng → đăng xuất → đăng nhập mật khẩu mới | Vào được | |
| A8.6 | Nhập tên **rất dài (>150 ký tự)** → Lưu | Báo lỗi 400 (không 500/không trắng trang) | |

## A9. Nội dung & cửa hàng
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| A9.1 | `/he-thong-cua-hang` | Hiện 1 cửa hàng (tên/địa chỉ/SĐT/giờ từ Cấu hình) + bản đồ; nút **Chỉ đường** mở Google Maps | |
| A9.2 | Gửi form **Liên hệ** (nếu có ở trang) | Gửi thành công | |

---

# PHẦN B — ADMIN (QUẢN TRỊ) · http://localhost:5176

## B0. Đăng nhập & giao diện
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B0.1 | Login `admin@motosale.local / Admin@123` | Vào dashboard, **icon hiển thị đầy đủ** (sidebar/menu) | |
| B0.2 | Quan sát menu | Đúng **5 nhóm**: Bán hàng · Sản phẩm & Kho · Dịch vụ & Hậu mãi · Tài chính & Báo cáo · Hệ thống | |

## B1. Bán hàng
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B1.1 | **POS** → tìm SKU → thêm → chọn **Bán đứt** → thu tiền → Tạo đơn | Đơn **Hoàn tất/Đã thanh toán/Đã giao**, tồn **giảm** | |
| B1.2 | POS → **Đặt cọc** (nhập cọc) → Tạo đơn | Đơn **Đã xác nhận/Đã đặt cọc**, còn nợ = tổng − cọc | |
| B1.3 | POS giỏ trống / cọc ≥ tổng | Bị chặn | |
| B1.4 | POS **khách quen** (tra SĐT) | Đơn gắn đúng khách | |
| B1.5 | **In hóa đơn VAT** | Bản in đúng (tách thuế, số tiền bằng chữ) | |
| B1.6 | **Đơn hàng** → mở đơn cọc → Ghi nhận thanh toán phần còn lại → Giao hàng & xuất kho | Đơn **Hoàn tất**, tồn trừ thật | |
| B1.7 | **Voucher** → tạo mã (%/tiền, hạn mức) → áp ở POS | Giảm đúng | |
| B1.8 | Xóa voucher **đã dùng** | Bị chặn | |

## B2. Sản phẩm & Kho
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B2.1 | **Sản phẩm** → thêm phụ tùng (gắn Hãng SX) + SKU + giá | Tạo thành công, hiện ở danh sách | |
| B2.2 | Sửa sản phẩm | Lưu đúng | |
| B2.3 | Xóa sản phẩm | **Xóa mềm** (chuyển Ngừng bán), ẩn khỏi danh sách đang bán | |
| B2.4 | **Tồn kho** → điều chỉnh +N | Tồn tăng | |
| B2.5 | **Chứng từ kho** → tạo phiếu nhập → **Duyệt** | Tồn cập nhật sau duyệt | |
| B2.6 | **Cung ứng** → NCC → đơn mua → duyệt → **nhận hàng** | Tồn **+**; → **thanh toán NCC** → ghi chi quỹ | |

## B3. Xóa master-data (kiểm chính sách xóa)
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B3.1 | Tạo **Hãng SX** mới + 1 phụ tùng thuộc hãng → xóa hãng | **Bị chặn** ("còn sản phẩm đang bán") | |
| B3.2 | Ngừng bán (xóa mềm) phụ tùng đó → xóa hãng lại | **Xóa được** (mềm), hãng biến khỏi danh sách | |
| B3.3 | Tương tự với **Thương hiệu / Dòng xe / Danh mục** | Chặn khi còn SP/con đang dùng; sau khi ngừng bán hết → xóa được | |
| B3.4 | Sản phẩm cũ của hãng đã xóa | Vẫn hiển thị đúng tên hãng (không mồ côi) | |

## B4. Dịch vụ & Hậu mãi
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B4.1 | **Đổi trả** → tạo từ đơn đã giao → chọn tình trạng → **Duyệt** | Hàng bán-lại-được **nhập kho** + **sinh phiếu hoàn tiền** + **ghi chi quỹ** | |
| B4.2 | Sửa phiếu trả đã duyệt | Bị chặn | |
| B4.3 | **Bảo hành** → tạo → sửa khi mới tiếp nhận → chuyển trạng thái | Đúng luồng; chặn sửa sau xử lý | |
| B4.4 | **Sửa chữa** → tạo (kèm phụ tùng) → chuyển sang Sửa | Xuất kho phụ tùng | |
| B4.5 | **CSKH** → tạo + hoàn thành tương tác | OK | |
| B4.6 | **Đánh giá** → duyệt review chờ (xem Phần C) | review hiển thị công khai sau duyệt | |

## B5. Tài chính & Báo cáo
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B5.1 | **Sổ quỹ** → xem thu/chi tự sinh; đảo 1 phiếu | Có giao dịch; đảo phiếu OK | |
| B5.2 | **Công nợ** | Liệt kê đơn còn phải thu | |
| B5.3 | **Báo cáo** → chọn kỳ | Có doanh thu, **lãi gộp/COGS**, top SP, thu chi | |
| B5.4 | **Xuất Excel** | File `.xlsx` mở được, số liệu khớp, tiếng Việt không lỗi | |
| B5.5 | Đơn **đã hủy** | Không tính doanh thu | |

## B6. Hệ thống
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| B6.1 | **Tài khoản** → tạo Staff | OK | |
| B6.2 | "Xóa" 1 tài khoản khách (không phải Admin cuối, không phải mình) | **Khóa mềm** (status=Inactive), vẫn còn trong DB, **đăng nhập tài khoản đó bị chặn** | |
| B6.3 | Thử xóa tài khoản **đang đăng nhập** / **Admin hoạt động cuối** | Bị chặn | |
| B6.4 | **Phân ca / Chấm công** | Xếp ca (chặn trùng giờ); check-in/out | |
| B6.5 | **Cấu hình** → sửa & lưu | Lưu, reload còn | |
| B6.6 | **Nhật ký kiểm toán** | Có bản ghi sau các thao tác | |
| B6.7 | **Liên hệ** | Thấy liên hệ khách gửi; đánh dấu đã xử lý | |
| B6.8 | **Đăng nhập Staff** → mở trang Tài chính/Tài khoản/Cấu hình | Bị chặn (Admin-only) | |

---

# PHẦN C — LUỒNG 2 CHIỀU (KHÁCH ↔ ADMIN)

## C1. Đặt hàng online → admin xử lý → khách thấy cập nhật
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| C1.1 | (Khách 5174) đặt 1 đơn (COD) | Đơn *Chờ thanh toán* | |
| C1.2 | (Admin 5176) **Đơn hàng** → thấy đơn vừa đặt (đúng khách/SP/tổng) | Hiện trong danh sách | |
| C1.3 | Admin: Ghi nhận thanh toán đủ → **Giao hàng & xuất kho** | Đơn **Hoàn tất**, tồn giảm | |
| C1.4 | (Khách) mở lại đơn đó | Trạng thái cập nhật theo (Hoàn tất/Đã thanh toán) | |

## C2. Chuyển khoản → admin xác nhận → khách thấy "Đã thanh toán"
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| C2.1 | Dùng đơn chờ xác nhận ở **A6** | Đơn đang *chờ xác nhận* | |
| C2.2 | (Admin) mở đơn đó → card **"Chuyển khoản chờ xác nhận"** → **Xác nhận thanh toán** | Đơn chuyển **Đã thanh toán** (ghi thu quỹ) | |
| C2.3 | (Khách) mở lại đơn | Hiện **"Đã thanh toán"** | |

## C3. Vòng đời đánh giá
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| C3.1 | (Khách) có đơn **đã giao/hoàn tất** chứa SP P → vào `/products/P` → **Đánh giá** (sao + nội dung) → gửi | Báo "chờ duyệt" | |
| C3.2 | (Admin) **Đánh giá** → thấy review *Pending* → **Duyệt** | Approved | |
| C3.3 | (Khách/khách vãng lai) xem `/products/P` | Review hiển thị công khai + điểm trung bình cập nhật | |

## C4. Đồng bộ cấu hình & nội dung
| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| C4.1 | (Admin) đổi **Cấu hình** tên/SĐT cửa hàng → Lưu | (Khách) `/he-thong-cua-hang` phản ánh đúng | |
| C4.2 | (Admin) tạo **Bài viết** Published / Draft | (Khách) chỉ thấy bài Published | |
| C4.3 | (Khách) đăng ký tài khoản mới | (Admin) **Khách hàng** thấy tài khoản đó | |

---

# PHẦN D — TRƯỜNG HỢP ĐẶC BIỆT / BẢO MẬT

| # | Thao tác | Kết quả mong đợi | ✅/❌ |
|---|---|---|---|
| D1 | Khách gọi thẳng API admin: mở `http://localhost:5100/api/inventory` (đã login khách) | **403** | |
| D2 | Vào route bảo vệ khi chưa đăng nhập (`/cart`,`/orders`,`/account`) | Redirect `/login` | |
| D3 | Mua SKU số lượng **vượt tồn** | Bị chặn "tồn không đủ" | |
| D4 | Đặt nhiều đơn POS liên tiếp trong vài giây | **Không trùng mã đơn** | |
| D5 | Voucher dưới đơn tối thiểu / vượt giá trị đơn | Không hợp lệ / tổng không âm | |
| D6 | Nhập tên/ghi chú **rất dài + ký tự đặc biệt + tiếng Việt** | Xử lý gọn (validate 400 nếu quá dài), không 500 | |
| D7 | Token hết hạn (để lâu) / xóa token trong DevTools | Tự đăng xuất, không trắng trang | |
| D8 | Mạng chậm/đứt (DevTools throttling) | Loading/timeout/thông báo rõ, không vỡ | |

---

# PHẦN E — GIAO DIỆN & ĐA NỀN TẢNG
| # | Kiểm tra | Mong đợi | ✅/❌ |
|---|---|---|---|
| E1 | Storefront ở **mobile 390px** (DevTools responsive) | Header thu gọn/menu, giỏ & checkout dùng được | |
| E2 | Không **mojibake** (Ã, áº…), tiền **VNĐ**, ngày **kiểu VN**, trạng thái tiếng Việt | Đúng ở cả 2 FE | |
| E3 | Mở/đóng modal (QR, đánh giá, hủy đơn) bằng nút X / Hủy | Hoạt động, không kẹt overlay | |
| E4 | Back/Forward trình duyệt | Không vỡ trạng thái | |

---

# PHẦN F — BUILD & TEST TỰ ĐỘNG (chốt cuối)
| # | Lệnh | Mong đợi | ✅/❌ |
|---|---|---|---|
| F1 | `cd v2/backend ; dotnet build` | 0 lỗi | |
| F2 | `cd v2/backend ; dotnet test` | Toàn bộ PASS (cần tắt service để khỏi khóa DLL) | |
| F3 | `cd v2/frontend-admin ; npm run build` | Build OK | |
| F4 | `cd v2/frontend-store ; npm run build` | Build OK | |
| F5 | (tùy) `cd v2 ; docker compose up --build` | Cả hệ lên: store 8081 · admin 8080 · gateway 5100 | |

---

## Mẫu ghi nhận lỗi
| ID | Trang/FE | Thao tác | Dữ liệu | Mong đợi | Thực tế | Mức độ | Trạng thái |
|---|---|---|---|---|---|---|---|
| BUG-xxx | | | | | | | |

## Tiêu chí đạt (Definition of Done)
- Tất cả luồng A/B/C chạy thông; tiền/tồn/đơn/đổi-trả/báo-cáo không sai logic.
- Phân quyền khách/Staff/Admin chặt (D1, D2, B6.8).
- Không lỗi giao diện nghiêm trọng, không mojibake; build FE/BE + test BE pass (F1–F4).
- Dữ liệu demo đủ phong phú để trình bày.
