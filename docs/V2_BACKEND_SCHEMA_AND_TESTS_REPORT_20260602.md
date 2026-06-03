# V2 Backend Schema And Tests Report - 2026-06-02

## Kết quả

- Backend test chính thức: `PASS`
- FE Admin build sau thay đổi audit mapping: `PASS`
- Lệnh đã chạy:
  - `dotnet test v2/backend/MoToSale.slnx`
  - `npm run build` trong `D:\MotorTeam\MoToSale-End\v2\frontend-admin`

## Chuẩn hóa DB/schema tiếng Anh

DB/entity/migration chính của v2 hiện dùng schema tiếng Anh:

- Catalog: `Products`, `Skus`, `Categories`, `Brands`, `VehicleModels`, `Manufacturers`, `ProductImages`, `PartCompatibilities`
- Inventory: `InventoryItems`, `StockDocuments`, `StockDocumentLines`, `StockMovements`, `Reservations`
- Ordering: `Orders`, `OrderLines`, `Allocations`, `OrderStatusHistories`, `Vouchers`, `VoucherScopes`, `VoucherRedemptions`, `OrderVouchers`
- Content/System/Audit: `Posts`, `Faqs`, `ContactRequests`, `HomeBanners`, `Settings`, `AuditLogs`

Đã thêm test `EnglishSchemaTests` để khóa invariant:

- Table name phải là ASCII English identifier.
- Column name phải là ASCII English identifier.

Phần còn lại có tiếng Việt chủ yếu là seed data/nội dung hiển thị tiếng Việt, không phải schema. `OperationsController` đã được chuẩn hóa sang request/response English (`id`, `name`, `type`, `addressLine`, `phone`, `isActive`, `description`) và vẫn đọc alias legacy để không làm gãy FE cũ.

`AuditLogsController` đã trả response English-only cho dữ liệu chính: `entity`, `entityId`, `action`, `oldValue`, `newValue`, `actorName`, `actorId`, `at`.

## Backend tests đã thêm

Test project mới:

`D:\MotorTeam\MoToSale-End\v2\backend\tests\MoToSale.Backend.Tests`

Coverage hiện có:

1. `CatalogServiceTests`
   - Tạo/sửa/xóa mềm sản phẩm.
   - Tạo/sửa/xóa SKU phụ.
   - Kiểm tra phụ tùng không gắn trực tiếp `BrandId/VehicleModelId`.
   - Chặn duplicate product code.

2. `VoucherServiceTests`
   - Tạo/sửa/xóa voucher.
   - Validate min order, percent discount, max discount.
   - Chặn duplicate voucher code.

3. `InventoryServiceTests`
   - Tạo phiếu kho draft.
   - Duyệt phiếu nhập kho.
   - Kiểm tra ledger movement và tồn kho cập nhật.
   - Hủy phiếu draft và chặn hủy lại.

4. `OrderServiceTests`
   - Customer thêm giỏ hàng.
   - Checkout tạo đơn.
   - Tạo reservation.
   - Cập nhật trạng thái đơn sang shipping và đồng bộ fulfillment.
   - Hủy đơn, release reservation, kiểm tra history.
   - Chặn thêm giỏ vượt tồn khả dụng.

5. `OperationsControllerTests`
   - Controller nhận request English cho warehouse.
   - Response warehouse có field English.
   - Settings nhận `description` English.

6. `EnglishSchemaTests`
   - Khóa table/column schema theo ASCII English identifier.

## Ghi chú

- Test dùng EF Core InMemory với repository thật và seed fixture riêng cho từng test, không phụ thuộc SQL Server thật.
- `UnitOfWork` trong test được thay bằng `NoOpUnitOfWork` vì EF InMemory không hỗ trợ transaction thật.
- Trong lúc test, process `MoToSale.APIService` đang chạy đã được tắt để tránh lock file build.
