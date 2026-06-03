# V2 — Đối chiếu tính năng với project gốc (FrontendAdmin)

> Mục tiêu: v2 phải phủ **đầy đủ** tính năng admin gốc, không làm sơ sài.
> Trạng thái: ✅ xong · 🟡 mới cơ bản (thiếu chiều sâu) · ❌ chưa có

| # | Module gốc | Tính năng gốc (theo service/endpoint) | v2 |
|---|---|---|---|
| 1 | Auth | login, /users/me | ✅ |
| 2 | **Người dùng** | /users/all, create, update, updateStatus, delete | 🟡 (mới list) |
| 3 | **Khách hàng** | /users/customers, care-note (ghi chú chăm sóc) | ❌ |
| 4 | **Hãng + Dòng xe** | brand CRUD + upload logo; model CRUD | ❌ |
| 5 | **Danh mục** | category CRUD (cây cha/con) | ❌ |
| 6 | Sản phẩm | CRUD | ✅ |
| 7 | Biến thể (SKU) | CRUD | ✅ |
| 8 | Ảnh sản phẩm/biến thể | upload/xóa/đặt chính | ✅ |
| 9 | **Tương thích phụ tùng** | compatibilities CRUD (theo hãng/dòng/đời xe) | ❌ |
| 10 | Tồn kho | list | 🟡 |
| 11 | **Tồn kho — sâu** | sync, holds (giữ chỗ), adjustments (lịch sử ĐC), **adjust** trực tiếp, **threshold** (ngưỡng cảnh báo), **export CSV** | ❌ |
| 12 | Phiếu kho | list, detail, create, approve, cancel | ✅ |
| 13 | Đơn hàng | list, detail, updateStatus, cancel | ✅ (+phân phối) |
| 14 | **Thanh toán (trang riêng)** | list, detail, confirm, cancel | 🟡 (chỉ ghi nhận trong đơn) |
| 15 | **Voucher** | CRUD | ❌ |
| 16 | **Đánh giá** | list, detail, updateStatus (duyệt), delete | ❌ |
| 17 | **Bảo hành** | list, detail, create, updateStatus | ❌ |
| 18 | **Bài viết** | CRUD + ảnh | ❌ |
| 19 | **FAQ** | CRUD | ❌ |
| 20 | **Liên hệ** | list, mark processed | ❌ |
| 21 | **Banner trang chủ** | CRUD + ảnh | ❌ |
| 22 | **Cấu hình vận hành** | warehouses, settings | ❌ |
| 23 | **Nhật ký hệ thống (Audit)** | list | ❌ (backend đã ghi, chưa có API/đọc) |
| 24 | **Báo cáo & Thống kê** | dashboard (doanh thu theo ngày, trạng thái đơn, đơn gần đây, top sản phẩm), báo cáo theo khoảng ngày, export Excel | 🟡 (dashboard chỉ 3 thẻ) |

## Kế hoạch làm bù (theo lô, bám sát gốc)
- **Lô A — Catalog quản trị**: Hãng+Dòng xe, Danh mục, Tương thích phụ tùng.
- **Lô B — Tồn kho sâu**: giữ chỗ, lịch sử điều chỉnh, điều chỉnh trực tiếp, ngưỡng cảnh báo, export, sync.
- **Lô C — Bán hàng**: trang Thanh toán, Voucher.
- **Lô D — CSKH & nội dung**: Khách hàng (care-note), Đánh giá, Bảo hành, Bài viết, FAQ, Liên hệ, Banner.
- **Lô E — Quản trị hệ thống**: Người dùng (CRUD), Cấu hình vận hành, Audit log, Báo cáo + biểu đồ + export.
