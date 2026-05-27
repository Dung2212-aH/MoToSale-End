# Admin New Features Test Report - 2026-05-26

## Môi trường

- FE Admin: `http://127.0.0.1:5175`
- Gateway: `http://localhost:5000`
- AuthService: `http://localhost:5001`
- CatalogService: `http://localhost:5002`
- OrderService: `http://localhost:5003`
- PaymentService: `http://localhost:5004`
- DB: `(localdb)\MSSQLLocalDB`, database `ShowroomDB`

## Tài khoản test

- Admin: `testadmin@test.com / Codex@12345`
- Staff: `codex.staff@test.com / Staff@12345`

## Artifact

- API run: `D:\MotorTeam\MoToSale-End\FrontendAdmin\test-artifacts\admin-new-features-20260526-160449\api-results.json`
- Context: `D:\MotorTeam\MoToSale-End\FrontendAdmin\test-artifacts\admin-new-features-20260526-160449\run-context.json`
- Screenshots: `D:\MotorTeam\MoToSale-End\FrontendAdmin\test-artifacts\admin-new-features-screenshots-20260526`

## Kết quả chính

| Nhóm | Kết quả | Ghi chú |
|---|---:|---|
| Health check FE/BE | Pass | Auth, Catalog, Order, Payment, Gateway, FE đều chạy |
| Đăng nhập Admin UI | Pass | Vào dashboard, thấy sidebar mới |
| Đăng nhập Staff UI | Pass | Vào dashboard bằng Staff |
| Phân quyền Staff | Pass | Staff không thấy Người dùng/Nhật ký; API audit/settings-save trả `403` |
| Nhật ký hệ thống | Pass | Admin đọc được, filter/reset UI mở được |
| Phiếu kho API | Pass | Tạo nháp, duyệt, hủy đều thành công |
| Phiếu kho UI | Pass | Danh sách, nút tạo, xem chi tiết, nút in, export hiển thị/bấm được |
| Khách hàng API/UI | Pass | Danh sách, ghi chú chăm sóc, export UI bấm được |
| Bảo hành API/UI | Pass | Tạo phiếu, cập nhật trạng thái, lịch sử xử lý, nút in |
| Cấu hình vận hành | Pass | Admin lưu warehouse/settings; Staff chỉ xem |
| Dashboard vận hành | Pass sau khi bật PaymentService | Hiển thị các thẻ vận hành mới |
| Screenshot bảng/UI mới | Pass | Đã chụp Dashboard, Phiếu kho, Khách hàng, Bảo hành, Cấu hình |
| Console browser | Pass | Không có error/warning runtime trong lượt kiểm tra |
| Build cuối | Pass | FE Admin + Auth/Catalog/Order/Payment |

## DB đối chiếu sau test

| Bảng | Số bản ghi |
|---|---:|
| `HE_THONG_NHATKY` | 11 |
| `TONKHO_PHIEU` | 3 |
| `TONKHO_PHIEU_CHITIET` | 3 |
| `KHACHHANG_GHICHU_CHAMSOC` | 1 |
| `BAOHANH_PHIEU` | 1 |
| `BAOHANH_LICHSU` | 2 |
| `CUAHANG_KHO` | 3 |
| `HETHONG_CAUHINH` | 8 |

## Lưu ý trong quá trình test

- Dashboard bị kẹt `Đang tải...` khi chưa bật PaymentService. Sau khi bật PaymentService, dashboard hiển thị đúng các thẻ vận hành. Nếu vận hành thật cần đảm bảo PaymentService được chạy cùng bộ service hoặc dashboard phải fallback tốt hơn khi PaymentService tắt.
- Khi test API thủ công bằng PowerShell, các request có tiếng Việt cần gửi `Content-Type: application/json; charset=utf-8`. UI dùng axios nên không gặp lỗi này trong lượt test UI.
- Test in chứng từ được xác nhận nút/mẫu mở từ UI; không bấm sâu vào hộp thoại in hệ điều hành để tránh treo automation.

## Build cuối

- `npm run build` trong `FrontendAdmin`: Pass, còn warning chunk size lớn do bundle hiện tại.
- `dotnet build Backend/AuthService/AuthService.csproj`: Pass.
- `dotnet build Backend/CatalogService/CatalogService.csproj`: Pass.
- `dotnet build Backend/OrderService/OrderService.csproj`: Pass.
- `dotnet build Backend/PaymentService/PaymentService.csproj`: Pass.

## Kết luận

Các phần mới tạo đã pass test chức năng chính, phân quyền, UI route/modal/nút chính, API/DB và build. Chưa phát hiện lỗi block vận hành sau khi chạy đủ service.
