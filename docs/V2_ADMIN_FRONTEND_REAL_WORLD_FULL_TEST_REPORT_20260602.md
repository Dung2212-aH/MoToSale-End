# V2 Admin Frontend Real World Full Test Report - 2026-06-02

## Scope

Plan được thực hiện theo `D:/MotorTeam/MoToSale-End/docs/V2_ADMIN_FRONTEND_REAL_WORLD_FULL_TEST_PLAN.md`.

Môi trường test:

- FE: `http://localhost:5176`
- Gateway: `http://localhost:5100`
- AuthService: `http://localhost:5101`
- APIService: `http://localhost:5102`
- Admin: `admin@motosale.local / Admin@123`
- Staff: `staff@motosale.local / Staff@123`
- Customer: `customer@motosale.local / Customer@123`

Artifact:

- `D:/MotorTeam/MoToSale-End/test-artifacts/v2-admin-real-world-full-test-20260602/`
- Screenshot chính: `D:/MotorTeam/MoToSale-End/test-artifacts/v2-admin-real-world-full-test-20260602/screenshots/`

## Summary

| Hạng mục | Kết quả |
|---|---|
| API contract endpoints FE đang dùng | Pass phần nghiệp vụ chính |
| UI route sweep bằng browser thật | Pass 21 route, không route trắng/crash |
| Screenshot bảng/layout | Đã chụp cho 21 route |
| Đối chiếu bảng FE với sample API | Pass 15 bảng/trang chính |
| Login và phân quyền | Pass admin/staff/customer/sai mật khẩu |
| Export tồn kho API XLSX | Pass, file thật `inventory-export-api.xlsx` |
| Build backend | Pass sau khi dừng host đang khóa DLL |
| Build frontend | Pass, không còn warning chunk lớn |
| Playwright click/modal coverage | Pass 20/20 route |
| NPM audit | Pass 0 vulnerability |

## API Contract

File chi tiết: `test-artifacts/v2-admin-real-world-full-test-20260602/api-contract-results.json`.

Pass:

- `/products?kind=1`, `/products?kind=2`
- `/categories`, `/brands`, `/models`, `/manufacturers`
- `/orders`, `/orders/{id}`
- `/vouchers`
- `/inventory`, `/inventory/holds`, `/inventory/adjustments`, `/inventory/documents`, `/inventory/export`
- `/users`, `/users/customers`
- `/warranties`, `/reviews`
- `/content/posts`, `/content/faq`, `/content/contacts`, `/content/home-banners`
- `/payments`
- `/audit-logs`, `/stores`, `/skus`
- `/operations/settings`, `/operations/warehouses`

Ghi chú:

- Đã bổ sung `/reports/summary`, `/reports/dashboard`, `/reports`.
- `/payments` API tồn tại, nhưng FE không có route `/payments`; thanh toán được xử lý trong chi tiết đơn hàng theo nghiệp vụ đã chốt trước đó.

## UI Route Sweep

File chi tiết: `test-artifacts/v2-admin-real-world-full-test-20260602/ui-route-sweep-results.json`.

Đã mở và chụp screenshot:

- `/`
- `/motorcycles`
- `/parts`
- `/categories`
- `/brands`
- `/orders`
- `/vouchers`
- `/inventory`
- `/stock-documents`
- `/users`
- `/customers`
- `/warranties`
- `/reviews`
- `/posts`
- `/faq`
- `/contacts`
- `/home-banners`
- `/reports`
- `/audit-logs`
- `/settings`
- `/payments`

Kết quả:

- Không route nào trắng hoặc crash.
- Không phát hiện mojibake trong DOM browser thật.
- Không phát hiện footer phình cao bất thường.
- `/payments` redirect về `/`, phù hợp quyết định bỏ trang thanh toán riêng.
- `/settings` ban đầu có horizontal overflow ở bảng showroom/kho, đã sửa và retest pass.

## Data Table Comparison

File chi tiết: `test-artifacts/v2-admin-real-world-full-test-20260602/ui-api-table-comparison.json`.

Pass đối chiếu sample API và 3 dòng UI đầu cho:

- Xe máy
- Phụ tùng
- Đơn hàng
- Tồn kho
- Phiếu kho
- Người dùng
- Khách hàng
- Bảo hành
- Đánh giá
- Bài viết
- FAQ
- Liên hệ
- Banner
- Audit logs
- Settings/warehouses

## Auth And Permission

File chi tiết: `test-artifacts/v2-admin-real-world-full-test-20260602/ui-auth-role-results.json`.

Pass:

- Sai mật khẩu admin: vẫn ở `/login`, thông báo có dấu `Email hoặc mật khẩu không đúng.`
- Staff login: vào được admin FE nhưng không có quyền vào `/users`, UI báo `Bạn không có quyền truy cập khu vực này.`
- Customer login: bị chặn admin FE, UI báo `Tài khoản không có quyền truy cập trang quản trị.`
- Admin login: vào dashboard, navbar hiển thị `Quản trị viên`.

## Fixes During Test

### Settings Table Overflow

File sửa:

- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/src/pages/settings/OperationsSettings.jsx`

Vấn đề:

- Bảng Showroom/Kho trên `/settings` bị tràn ngang, cột thao tác bị cắt khỏi viewport.

Sửa:

- Bọc bảng bằng `.table-responsive`.
- Gắn class căn cột dùng chung: `table-col-text`, `table-col-code`, `table-col-status`, `table-col-actions`.

Retest:

- Screenshot: `screenshots/20-settings-after-fix.png`
- `overflow=false`.

### Inventory Threshold Validation

File sửa:

- `D:/MotorTeam/MoToSale-End/v2/backend/src/MoToSale.Services/Inventory/InventoryService.cs`

Vấn đề:

- `PUT /inventory/threshold` với payload `{}` trả 500 do `StoreId=0`, `SkuId=0` đi xuống DB.

Sửa:

- Validate `StoreId > 0`.
- Validate `SkuId > 0`.
- Giữ validate `ReorderPoint >= 0`.

Retest:

- File chi tiết: `test-artifacts/v2-admin-real-world-full-test-20260602/api-permission-validation-retest-results.json`
- Empty threshold request trả 400.
- Staff không list users: 403.
- Staff đọc orders: 200.
- Admin đọc audit: 200.

### Reports API Contract

File thêm:

- `D:/MotorTeam/MoToSale-End/v2/backend/src/MoToSale.APIService/Controllers/ReportsController.cs`

Endpoint mới:

- `GET /api/reports/summary`
- `GET /api/reports/dashboard`
- `GET /api/reports?startDate=...&endDate=...`

FE cập nhật:

- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/src/services/reportService.js`

Kết quả:

- Dashboard/Reports ưu tiên dùng contract BE.
- Vẫn có fallback client-side nếu chạy với BE cũ.

### UI Click Automation

File thêm:

- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/playwright.config.js`
- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/tests/admin-ui-click-all.spec.js`

Script mới:

- `npm run test:ui`

Kết quả:

- 20/20 route pass.
- Test tự login bằng API + localStorage để tập trung vào UI admin và tránh flaky login form.
- Test mở route, kiểm tra overflow, đọc button/table state, click các nút an toàn, mở/đóng modal hoặc quay lại route.

### Build And Audit Cleanup

FE cập nhật:

- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/vite.config.js`
- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/package.json`
- `D:/MotorTeam/MoToSale-End/v2/frontend-admin/package-lock.json`

Kết quả:

- Vite nâng lên `8.0.16`, React plugin nâng lên `6.0.2`.
- ExcelJS tách thành chunk riêng.
- `npm audit` trả 0 vulnerability.
- `npm run build` không còn warning chunk lớn.

## Build

Backend:

- `dotnet build D:/MotorTeam/MoToSale-End/v2/backend/MoToSale.slnx`
- Pass, 0 warning, 0 error sau khi dừng các host đang khóa DLL.

Frontend:

- `npm run build` tại `D:/MotorTeam/MoToSale-End/v2/frontend-admin`
- Pass.
- Không còn warning chunk lớn.

## Remaining Notes

- Không nên thêm lại route `/payments` nếu nghiệp vụ vẫn là thanh toán thủ công trong chi tiết đơn hàng.
- Nếu muốn mở rộng test destructive/state-changing, nên bổ sung test id và dữ liệu `E2E-` riêng để có thể tạo/cập nhật/xóa rồi cleanup tự động.
