# REFACTOR_PLAN - Don dep code va don gian hoa luong ShowroomDB

Pham vi: chi ra soat `Backend`, `Frontend`, `FrontendAdmin`. Khong ra soat, khong sua thu muc `v2`.

Muc tieu: luong code tu `ShowroomDB/Backend` den `ShowroomDB/Frontend` va `ShowroomDB/FrontendAdmin` phai gon, de tim, de bao tri: backend tra contract on dinh, service frontend mong, page UI doc field canonical thay vi nhieu fallback.

## Trang thai cap nhat

Da lam trong dot nay:

- `Frontend/src/services/api.js`: bo alias mong khong can thiet:
  - `productApi.getProducts` -> dung `productApi.getAll`.
  - `productApi.getProductById` -> dung `productApi.getById`.
  - `cartApi.getCart` -> dung `cartApi.getMine`.
  - `orderApi.getOrderById` -> dung `orderApi.getById`.
  - `voucherApi.listVouchers`, `userApi.getUsers`, `userApi.getUserById` khong co caller ngoai file nen da go.
- Da cap nhat caller trong:
  - `Frontend/src/contexts/CartContext.jsx`
  - `Frontend/src/pages/FavoritesPage.jsx`
  - `Frontend/src/pages/HomePage.jsx`
  - `Frontend/src/pages/ProductDetailPage.jsx`
  - `Frontend/src/pages/ProductListPage.jsx`
  - `Frontend/src/pages/OrderDetailPage.jsx`
- Da grep lai: khong con tham chieu den cac alias vua xoa.

Can lam tiep theo plan ben duoi. Luu y worktree dang co nhieu thay doi backend/admin va nhieu file backup san co, nen cac refactor backend nen lam theo tung commit nho de tranh tron thay doi.

## Nhiem vu 1 - Kiem tra code clean, code thua, ham sai chuc nang/tham so

### Backend

Can kiem tra va sua theo thu tu:

1. `AuthService/Controllers/UsersController.cs`
   - `ToAddress` dang tra key tieng Anh: `id`, `fullName`, `phoneNumber`, `addressLine`, `ward`, `district`, `province`, `note`, `isDefault`.
   - Can chuyen response ve canonical camelCase tieng Viet: `maDiaChi`, `hoTenNhanHang`, `soDienThoaiNhanHang`, `diaChiNhanHang`, `phuongXa`, `quanHuyen`, `tinhThanh`, `ghiChu`, `macDinh`.
   - Sau khi backend doi contract, frontend chi normalize mot lan trong service, page khong doc fallback Anh/Viet nua.

2. `CatalogService`
   - `ProductsController` van goi `CatalogSchema.EnsureRelatedTableAsync(_dbContext)` trong tung request lien quan den related products. Nen dua ensure schema ve startup/program bootstrap, giong cach init tap trung.
   - `AuditLogsController` van goi `_auditLogService.EnsureTableAsync()` trong action list. Nen dua ve startup de action chi doc data.
   - `ProductsController`, `InventoryController`, `WarrantiesController` co helper `GetCurrentUserId` rieng; nen gom ve `ControllerHelpers.cs` de thong nhat.
   - `ICatalogService` dang chi duoc dung mot phan trong `CategoriesController`, trong khi nhieu controller dung truc tiep `DbContext`. Can chon mot huong nhat quan cho CatalogService: hoac day service layer ro rang, hoac giu controller mong + query truc tiep co helper dung chung. Khong nen giu nua voi.

3. `OrderService`
   - `BusinessOperationsController` va `AdvancedOperationsController` co helper ten mo ho `Code(prefix)` va `Trim(...)`. Doi thanh `GenerateCode(prefix)` va `TrimToNull(...)`.
   - `BusinessOperationsController.GetSummary` nen doi thanh `GetOperationsSummary` de tim nhanh theo nghiep vu.
   - `GetCurrentUserId` da co helper trong `ControllerHelpers.cs`; giu pattern nay va khong tao them ban inline.

4. `PaymentService`
   - `PaymentsController.Confirm(int id, ConfirmPaymentRequest? request)` nhan request nullable, can lam ro contract: neu endpoint cho phep body rong thi dat default request trong service/controller ro rang; neu khong, bo nullable va validate body.

5. Code/DTO/repository co kha nang thua
   - Truoc khi xoa bat ky DTO/interface/repository nao, chay grep theo ten type trong toan bo `Backend` tru `v2`.
   - Khong xoa shadow entity `User`, `Product`, `Order` trong cac service vi dang can cho FK/query cross-service database.

### Frontend va FrontendAdmin

Can kiem tra va sua theo thu tu:

1. Fallback field bi du
   - `FrontendAdmin/src/pages/orders/OrderList.jsx` con fallback dai: `hoTenNhanHang`, `tenNguoiNhan`, `customerName`, `hoTen`, `fullName`.
   - `FrontendAdmin/src/pages/orders/OrderDetail.jsx`, `Dashboard.jsx`, `UserList.jsx`, `ContactList.jsx` cung con doc ca field Anh va Viet.
   - Sau khi backend contract on dinh, chuyen fallback ve normalizer/service, page chi doc field canonical.

2. Normalizer chua tap trung
   - `Frontend/src/services/api.js` da co pattern `field(...)` va normalize order/product/cart.
   - `FrontendAdmin` chua co normalizer dung chung, cac page tu xu ly fallback.
   - Them `FrontendAdmin/src/utils/normalizers.js` hoac `FrontendAdmin/src/services/normalizers.js`, roi service tra data da normalize.

3. Util trung lap
   - `formatCurrency`, `formatDate`, `printInstallmentApplication`, status mapping dang trung giua `Frontend` va `FrontendAdmin`.
   - Neu chua co shared package, truoc mat thong nhat implementation noi bo tung app theo cung contract. Khong copy them ban moi.

4. Mojibake/text encoding
   - Nhieu file hien co text tieng Viet bi mojibake trong `Frontend` va mot so controller comment/message.
   - Day la hang muc rieng. Khong nen sua lan rong trong cung commit refactor API vi se tao diff lon va kho review.

## Nhiem vu 2 - Doi ten ham de de tim, de hieu chuc nang

Nguyen tac:

- Chi doi ten method/function noi bo. Khong doi route attribute, khong doi ten bang/cot/property entity tieng Viet.
- Sau moi lan doi, cap nhat `CreatedAtAction(nameof(...))` va grep lai ten cu.
- Ten frontend service nen theo CRUD canonical: `getAll`, `getById`, `create`, `update`, `delete`; ten nghiep vu dung dong tu ro: `login`, `register`, `getProfile`, `getMyOrders`, `getMyVouchers`.

### Da hoan tat

- Storefront API alias cleanup:
  - `getProducts`, `getProductById`, `getCart`, `getOrderById`, `listVouchers`, `getUsers`, `getUserById` da duoc go/cap nhat.

### Can hoan tat

Backend:

| File | Ten hien tai | Ten de xuat |
|---|---|---|
| `Backend/CatalogService/Controllers/ModelsController.cs` | `GetAll` | `GetVehicleModels` |
| `Backend/CatalogService/Controllers/ModelsController.cs` | `GetById` | `GetVehicleModelById` |
| `Backend/CatalogService/Controllers/ModelsController.cs` | `Create` | `CreateVehicleModel` |
| `Backend/CatalogService/Controllers/ModelsController.cs` | `Update` | `UpdateVehicleModel` |
| `Backend/CatalogService/Controllers/ModelsController.cs` | `Delete` | `DeleteVehicleModel` |
| `Backend/CatalogService/Controllers/ProductsController.cs` | `GetRelatedItems` | `GetRelatedProducts` |
| `Backend/CatalogService/Controllers/ProductsController.cs` | `GetProductPromotions` | `GetApplicableVouchers` |
| `Backend/OrderService/Controllers/BusinessOperationsController.cs` | `GetSummary` | `GetOperationsSummary` |
| `Backend/OrderService/Controllers/BusinessOperationsController.cs` | `Code` | `GenerateCode` |
| `Backend/OrderService/Controllers/AdvancedOperationsController.cs` | `Code` | `GenerateCode` |
| `Backend/OrderService/Controllers/{Business,Advanced}OperationsController.cs` | `Trim` | `TrimToNull` |
| `Backend/AuthService/Controllers/AuthController.cs` | `Me` | `GetCurrentUser` |

Da co ten tot, chi can giu:

- `BrandsController`: `GetBrands`, `GetBrandById`, `CreateBrand`, `UpdateBrand`, `DeleteBrand`.
- `CategoriesController`: `GetCategories`, `GetCategoryById`, `CreateCategory`, `UpdateCategory`, `DeleteCategory`.

FrontendAdmin:

- `businessOperationsService.getSummary` co the giu route `/summary`, nhung nen doi ten function thanh `getOperationsSummary` va cap nhat page caller.
- `productService.getRelatedItems` nen doi thanh `getRelatedProducts`.
- `productService.getPromotions` nen doi thanh `getApplicableVouchers` neu backend action da doi.

## Nhiem vu 3 - Sap xep lai luong Backend -> Frontend gon nhu tinh than v2

Khong copy kien truc `v2`. Chi ap dung cac nguyen tac lam luong don gian hon:

1. Mot contract canonical
   - Backend tra camelCase tieng Viet cho resource chinh.
   - Frontend service normalize response mot lan.
   - Page/component chi dung field canonical, khong fallback qua 4-5 ten field.

2. Controller mong
   - Controller chi parse request, goi service/query, tra response.
   - Schema/bootstrap nhu `EnsureTableAsync` chay luc startup, khong chay trong moi request.
   - Helper lap lai nhu `GetCurrentUserId`, `TrimToNull`, `GenerateCode` gom dung chung trong cung service/module.

3. Service frontend mong
   - Service chi goi endpoint, map payload request, normalize response.
   - Khong de page tu ghep contract backend.
   - Khong giu alias mong neu ten canonical da ro.

4. Thu tu thuc hien an toan
   - Phase 1: doi ten ham noi bo va bo alias frontend (da lam mot phan).
   - Phase 2: them normalizer cho `FrontendAdmin`, sua service order/user/contact/review/operations tra data canonical.
   - Phase 3: sua response `AuthService` address/profile ve camelCase tieng Viet, sau do go fallback trong `AccountPage`, `CheckoutPage`, `UserList`, `OrderList`, `OrderDetail`.
   - Phase 4: dua schema ensure ra startup cho Catalog/Order audit/related/banner tables.
   - Phase 5: cleanup util trung lap va text encoding neu can.

## Lenh kiem tra

Chay theo thu tu sau moi phase:

```powershell
rg -n "getProducts\(|getProductById\(|getCart\(|getOrderById\(|listVouchers\(|getUsers\(|getUserById\(" Frontend\src FrontendAdmin\src -g "*.js" -g "*.jsx"
rg -n "GetAll\(|GetById\(|GetSummary\(|GetRelatedItems\(|GetProductPromotions\(|\bCode\(" Backend -g "*.cs" -g "!v2/**"
dotnet build Backend\ShowroomBackend.sln --no-restore -p:UseAppHost=false
npm run build --prefix Frontend
npm run build --prefix FrontendAdmin
```

Chap nhan neu `dotnet build --no-restore` fail vi thieu restore/local package, nhung phai ghi ro loi. Neu can restore/install package thi lam rieng, khong tron vao commit refactor logic.

## Definition of Done

- Khong con alias API frontend mong.
- Cac action/backend helper trong bang rename khong con ten cu.
- `AuthService` user/address response co contract camelCase tieng Viet on dinh.
- `FrontendAdmin` page khong con fallback field Anh/Viet dai cho order/user/contact.
- Schema ensure khong chay trong action moi request.
- Build `Backend`, `Frontend`, `FrontendAdmin` pass hoac co log loi moi truong ro rang.
