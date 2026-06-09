# CODE_REVIEW — ShowroomDB (Backend / Frontend / FrontendAdmin)

Phạm vi: chỉ `Backend`, `Frontend`, `FrontendAdmin`. **Không** rà soát `v2`.

## Nhiệm vụ 2 — Đổi tên hàm (ĐÃ HOÀN TẤT đợt này)

Chỉ đổi tên **method**; **không** đổi route attribute, **không** đổi tên bảng/cột/property tiếng Việt. Đã cập nhật `nameof(...)` trong `CreatedAtAction` và mọi caller. Build sạch: `dotnet build ShowroomBackend.sln` (0 error), `npm run build` ở `Frontend` & `FrontendAdmin` (OK).

### Backend — action controller

| File | Tên cũ | Tên mới |
|---|---|---|
| `CatalogService/Controllers/BrandsController.cs` | `GetAll/GetById/Create/Update/Delete` | `GetBrands/GetBrandById/CreateBrand/UpdateBrand/DeleteBrand` |
| `CatalogService/Controllers/ModelsController.cs` | `GetAll/GetById/Create/Update/Delete` | `GetVehicleModels/GetVehicleModelById/CreateVehicleModel/UpdateVehicleModel/DeleteVehicleModel` |
| `CatalogService/Controllers/WarrantiesController.cs` | `GetAll/GetById/Create` | `GetWarranties/GetWarrantyById/CreateWarranty` |
| `CatalogService/Controllers/AuditLogsController.cs` | `GetAll` | `GetAuditLogs` |
| `CatalogService/Controllers/InventoryController.cs` | `GetAll` | `GetInventory` |
| `CatalogService/Controllers/ProductsController.cs` | `GetRelatedItems` / `GetProductPromotions` | `GetRelatedProducts` / `GetApplicableVouchers` |
| `OrderService/Controllers/VouchersController.cs` | `GetAll/GetById/Create/Update/Delete` | `GetVouchers/GetVoucherById/CreateVoucher/UpdateVoucher/DeleteVoucher` |
| `OrderService/Controllers/BusinessOperationsController.cs` | `GetSummary` / `Code` / `Trim` | `GetOperationsSummary` / `GenerateCode` / `TrimToNull` |
| `OrderService/Controllers/AdvancedOperationsController.cs` | `Code` / `Trim` | `GenerateCode` / `TrimToNull` |
| `AuthService/Controllers/AuthController.cs` | `Me` | `GetCurrentUser` |

Đã sẵn tên tốt (giữ nguyên): `CategoriesController` (`GetCategories/GetCategoryById/...`), `OrdersController` (`GetOrders/GetOrderById`), `AuthController` (`Login/Register/ForgotPassword/ResetPassword/Logout`).

### Frontend service method

| File | Tên cũ | Tên mới |
|---|---|---|
| `FrontendAdmin/src/services/businessOperationsService.js` | `getSummary` | `getOperationsSummary` (caller: `pages/operations/BusinessOperations.jsx`) |
| `FrontendAdmin/src/services/productService.js` | `getRelatedItems` / `getPromotions` | `getRelatedProducts` / `getApplicableVouchers` (callers: `ProductRelatedManager.jsx`, `ProductPromotionsModal.jsx`) |
| `Frontend/src/services/api.js` (đợt trước) | alias `getProducts/getProductById/getCart/getOrderById/listVouchers/getUsers/getUserById` | gỡ/thay bằng `getAll/getById/getMine` |

Đã `grep` xác nhận không còn tham chiếu tên cũ (`nameof(GetById/GetAll/...)`, `getRelatedItems`, `service.getSummary`).
Lưu ý: `reportService.getSummary` và hàm cục bộ `getSummary` trong `PostList.jsx` là TÊN KHÁC NGỮ CẢNH — giữ nguyên.

## Nhiệm vụ 1 — Phát hiện cần xử lý tiếp (chưa sửa trong đợt này)

- **Contract JSON tiếng Anh lẫn lộn:** `AuthService/UsersController.ToAddress` trả `fullName/phoneNumber/addressLine/ward/district/province/isDefault`; `AuthController.GetCurrentUser` trả `userId/name/email/roles`. Cần quy về camelCase tiếng Việt canonical (nhiệm vụ 3A) + cập nhật normalizer phía `Frontend` (đang đọc `data.name/data.fullName`).
- **EnsureTable chạy mỗi request:** `ProductsController` (`CatalogSchema.EnsureRelatedTableAsync`), `AuditLogsController`/`AuditLogService.EnsureTableAsync`, `UsersController.EnsureCustomerNoteTableAsync` → nên dồn về bootstrap khởi động (theo `OrderService/Program.cs::EnsureOperationsSchemaAsync`).
- **`GetCurrentUserId` trùng lặp:** bản inline trong `AuthService/UsersController` + helper `int?`(Catalog) vs `int`(Order/Payment). Nên gộp về `ControllerHelpers` mỗi service, chuẩn `GetCurrentUserId()` (nullable) + `RequireCurrentUserId()` (ném lỗi).
- **`ICatalogService` dùng nửa vời:** chỉ vài chỗ gọi; nhiều controller query thẳng `DbContext`. Cần chọn 1 hướng nhất quán.
- **`PaymentsController.Confirm(int, ConfirmPaymentRequest?)`:** làm rõ contract body nullable.
- **`HttpPut` + `HttpPatch` cùng `UpdateUser`** (`AuthService/UsersController`): admin `userService.update` đang dùng PUT → nếu bỏ PUT phải đồng thời đổi frontend sang PATCH.
- **File rác:** `FrontendAdmin/src/services/reportService.js.backup` (bản backup) — cân nhắc xóa.
- **Util trùng giữa 2 app:** `formatCurrency` (khác implementation), `printInstallmentApplication` (trùng), `constants`/`statusMappings`.
- **Mojibake tiếng Việt** trong một số file `Frontend` + comment controller — để hạng mục riêng, không trộn vào commit refactor API.

### Giữ nguyên (cố ý)
- Shadow entity `User`/`Product`/`Order` lặp ở nhiều service: cần cho FK & query cross-service trên cùng DB → **không xóa**.

## Trạng thái
- Nhiệm vụ 2: ✅ xong (backend actions + frontend service methods, build sạch).
- Nhiệm vụ 1 & 3: đã liệt kê đầu việc ở trên + trong `REFACTOR_PLAN.md`, làm theo từng commit nhỏ.
