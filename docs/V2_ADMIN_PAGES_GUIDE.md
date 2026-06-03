# MoToSale v2 — Hướng dẫn ý nghĩa các trang Admin

> Tài liệu tham chiếu toàn bộ trang quản trị (frontend-admin v2). Cập nhật: 2026-06-03.
> Mô hình: **1 cửa hàng / 1 kho duy nhất**. Vai trò: **Admin** và **Nhân viên (Staff)**.
>
> **Chú thích trạng thái** (cấu hình cho đồ án — chấm theo *chiều sâu*):
> - 🟢 **LÕI** — thuộc phạm vi nộp bài, đang hiển thị trên menu.
> - ⚪ **MỞ RỘNG (đang ẩn)** — đã làm nhưng **ẩn khỏi menu + route** để gọn phạm vi; file/code vẫn còn, bật lại được.

Menu hiện chia **5 nhóm** (+ Tổng quan), khớp các mục bên dưới.

---

## 0. Tổng quan
| Trang | Route | Trạng thái | Ý nghĩa |
|---|---|---|---|
| **Tổng quan (Dashboard)** | `/` | 🟢 | KPI nhanh (doanh thu hôm nay/tháng, đơn chờ xử lý, còn phải thu, cần trả NCC, đơn mua đang xử lý, cảnh báo tồn), biểu đồ doanh thu + trạng thái đơn, đơn gần đây. Ô số bấm được để đi tới khu vực liên quan. |

## 1. KINH DOANH & SẢN PHẨM
| Trang | Route | Trạng thái | Ý nghĩa |
|---|---|---|---|
| **Xe máy** | `/motorcycles` | 🟢 | Quản lý sản phẩm *xe máy*: thông tin, **biến thể (SKU)**, bộ ảnh, mã vạch, khuyến mãi, sản phẩm bán kèm, tuổi tồn. |
| **Phụ tùng** | `/parts` | 🟢 | Như trên cho *phụ tùng*; thêm **tương thích xe** (lắp cho hãng/dòng/đời nào) và **Hãng sản xuất**. |
| **Danh mục** | `/categories` | 🟢 | Cây danh mục phân loại sản phẩm (nhóm Xe máy / Phụ tùng). |
| **Hãng xe & Dòng xe** | `/brands` | 🟢 | Hãng xe (Honda, Yamaha…) và dòng xe (Vision, Exciter…) — dùng cho lọc & tương thích phụ tùng. |
| **Hãng sản xuất phụ tùng** | `/manufacturers` | 🟢 | Nhà sản xuất phụ tùng (NGK, Denso…) kèm logo — gán cho phụ tùng. |

## 2. BÁN HÀNG
| Trang | Route | Trạng thái | Ý nghĩa |
|---|---|---|---|
| **Đơn hàng** | `/orders`, `/orders/:id` | 🟢 | Danh sách + chi tiết đơn online. Vòng đời: Chờ thanh toán → Xác nhận → **Soạn hàng/xuất kho** → Giao → Hoàn tất. Chi tiết: ghi nhận thanh toán (cọc/còn lại/đủ), xem cọc & công nợ, cập nhật vận chuyển, hủy đơn (hoàn tồn), **In phiếu đơn hàng** và **In Hóa đơn GTGT (VAT)**. |
| **Bán tại quầy (POS)** | `/pos` | 🟢 | Lập đơn bán trực tiếp: chọn SKU (tìm theo mã/tên/barcode), sửa giá, bán đứt (trừ kho ngay) hoặc **đặt cọc** (giữ hàng), thu tiền. Sau khi tạo đơn → banner cho **In Hóa đơn GTGT (VAT)** / Xem đơn / Tạo đơn mới. |
| **Voucher** | `/vouchers` | 🟢 | Tạo/quản lý mã giảm giá (%/số tiền, hạn mức, thời hạn). |
| **Khách hàng** | `/customers` | 🟢 | Hồ sơ khách, ghi chú chăm sóc, lịch sử tương tác. |

## 3. KHO & CUNG ỨNG
| Trang | Route | Trạng thái | Ý nghĩa |
|---|---|---|---|
| **Tồn kho** | `/inventory` | 🟢 | Tồn theo SKU: **Tồn thực − Đang giữ = Khả dụng**, ngưỡng cảnh báo, điều chỉnh, đồng bộ sổ cái, xuất Excel. |
| **Chứng từ kho** | `/stock-documents` | 🟢 | Phiếu nhập/xuất/điều chỉnh kho có duyệt/hủy (sổ cái bất biến). |
| **Cung ứng & mua hàng** | `/supply` | 🟢 | Nhà cung cấp + đơn mua (Nháp → Duyệt → Nhận hàng vào kho → Thanh toán NCC). |

## 4. HẬU MÃI & DỊCH VỤ
| Trang | Route | Trạng thái | Ý nghĩa |
|---|---|---|---|
| **Đổi trả & hoàn tiền** | `/returns` | 🟢 | Phiếu trả hàng (tạo → duyệt/từ chối). Duyệt: tự **hoàn tồn kho** + **sinh phiếu hoàn tiền** + **ghi chi quỹ**. |
| **Bảo hành** | `/warranties` | 🟢 | Phiếu bảo hành: số khung/số máy, lỗi khách báo, chi phí dự kiến/thực tế, dòng thời gian trạng thái. |
| **Dịch vụ & CSKH** | `/service-crm` | 🟢 | Phiếu sửa chữa xe (Nhận → Kiểm tra → Báo giá → Sửa → Bàn giao) + lịch chăm sóc khách hàng. |

## 5. TÀI CHÍNH & HỆ THỐNG
| Trang | Route | Trạng thái | Ý nghĩa |
|---|---|---|---|
| **Tài chính: thu chi & công nợ** | `/finance` *(Admin)* | 🟢 | Sổ quỹ (thu/chi tiền mặt/CK) + công nợ khách (đơn còn phải thu sau thanh toán & hoàn tiền). |
| **Báo cáo & thống kê** | `/reports` | 🟢 | Doanh thu, **lãi gộp/giá vốn (COGS)**, top sản phẩm, trạng thái đơn, mua hàng, thu chi, công nợ, dịch vụ, cảnh báo tồn — lọc theo kỳ + xuất Excel. |
| **Tài khoản hệ thống** | `/users` *(Admin)* | 🟢 | Quản lý tài khoản & vai trò (Admin/Nhân viên). |
| **Nhật ký hệ thống** | `/audit-logs` *(Admin)* | 🟢 | Lịch sử thao tác (ai làm gì, khi nào) để kiểm toán. |
| **Cấu hình vận hành** | `/settings` | 🟢 | Thông tin cửa hàng/kho, tham số hệ thống. Gồm **Mã số thuế (TaxCode)** và **Thuế suất VAT (VatRate)** dùng cho hóa đơn GTGT. |
| **Import dữ liệu** | `/operational-imports` *(Admin)* | 🟢 | Nhập nhanh sản phẩm hàng loạt (XLSX/nhập nhanh). |

---

## Phần MỞ RỘNG — đã làm nhưng đang ẩn ⚪

Ẩn để tập trung chấm theo *chiều sâu* (gỡ khỏi menu + route; **code/file vẫn còn**).

| Trang | Route | Vì sao tách khỏi lõi |
|---|---|---|
| **Nhân sự / Ca làm** (phân ca + chấm công) | `/staff` | Phân hệ **HR**, ngoài lõi bán hàng. |
| **Đánh giá** | `/reviews` | Nội dung khách (kiểm duyệt review), không thuộc pipeline thương mại. |
| **Banner trang chủ** | `/home-banners` | **CMS website** cho trang khách. |
| **Bài viết** | `/posts` | CMS website. |
| **FAQ** | `/faq` | CMS website. |
| **Liên hệ** | `/contacts` | Tiếp nhận form liên hệ từ website khách. |

**Bật lại 1 trang**: thêm lại 3 chỗ — `lazy import` + `<Route>` trong `src/App.jsx`, và mục `<Link>` trong `src/components/Sidebar.jsx`.

---

## Mạch nghiệp vụ xuyên suốt (phần lõi)

```
Mua (Cung ứng) → Nhập kho (Chứng từ/Tồn kho) → Bán (Đơn/POS) → Soạn & giao
        → Hậu mãi (Đổi trả / Bảo hành / Sửa chữa)
        → Tiền (Thanh toán → Quỹ & Công nợ)
        → Báo cáo tổng hợp (gồm lãi gộp)
```

Mọi thay đổi **tiền** và **tồn** đều phản ánh đồng bộ vào **Tồn kho**, **Sổ quỹ** và **Báo cáo**:
- Thu tiền khách / hoàn tiền trả hàng → ghi **Sổ quỹ** (thu/chi) tự động.
- Bán đứt / nhận hàng / đổi trả → cập nhật **Tồn kho** (OnHand) qua sổ cái bất biến.
- Đặt cọc / giữ hàng → tăng **Đang giữ (Reserved)**; xuất kho/hủy → giảm lại.
- Doanh thu chỉ tính đơn **đã thanh toán đủ** và **đã giao/hoàn tất**; lãi gộp = doanh thu − giá vốn bình quân (từ phiếu nhập).

## Hóa đơn GTGT (VAT)

- **Vị trí**: nút **"Hóa đơn VAT"** ở trang Chi tiết đơn hàng (`/orders/:id`) và banner sau khi tạo đơn ở **POS** (`/pos`).
- **Cơ chế**: in trực tiếp từ trình duyệt (cửa sổ in → có thể lưu PDF). Dùng chung tiện ích `src/utils/vatInvoice.js`.
- **Nội dung hóa đơn**: đơn vị bán (lấy từ Cấu hình: Tên cửa hàng, Địa chỉ, Hotline, **Mã số thuế**), người mua (từ đơn), bảng hàng hóa, **tách thuế** (giá bán đã gồm VAT → Cộng tiền hàng chưa thuế + Tiền thuế GTGT + Tổng thanh toán), **số tiền bằng chữ**, ô ký tên.
- **Thuế suất**: lấy từ Cấu hình `VatRate` (để trống = 10%).
- **Lưu ý**: đây là **bản thể hiện in được** đúng layout hóa đơn GTGT phục vụ học tập/demo; **chưa phải hóa đơn điện tử hợp pháp** (HĐĐT thật cần tích hợp nhà cung cấp có mã CQT + ký số).

## Ghi chú điều hướng

- Route cũ vẫn **redirect** để không gãy link: `/products → /motorcycles`, `/advanced-operations → /returns`, `/business-operations → /supply`.
- Trang gắn *(Admin)* chỉ tài khoản quản trị truy cập; nhân viên không thấy trên thanh điều hướng.
- "Công nợ khách" xuất hiện ở **Tài chính** (`/finance`) và cả trong **Báo cáo** (`/reports`).
