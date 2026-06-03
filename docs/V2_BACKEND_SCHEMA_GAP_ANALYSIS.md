# V2 Backend And Schema Gap Analysis

> Trang thai: In Progress
>
> Muc dich: danh gia backend/schema v2 truoc khi noi sau `FrontendAdmin` hien co vao API moi.

## 1. Ket luan nhanh

Backend v2 da di dung huong BaseCore:

- Co `Common`, `Entities`, `DTO`, `Repository`, `Services`, `AuthService`, `APIService`, `ApiGateway`.
- Co `AppDbContext` dung chung va EF Core migrations.
- Schema dung tieng Anh.
- Ton kho da chuyen sang `Sku`, `InventoryItem(StoreId, SkuId)` va `StockMovement`.
- Don hang da tach `Allocation`.

Tuy nhien schema hien tai moi la nen mong. Chua nen chuyen toan bo FE vao contract nay cho toi khi cac blocker ben duoi duoc xu ly.

## 2. Blocker schema

### P0 - Bat buoc sua truoc khi dua du lieu nghiep vu vao DB

- [x] Cau hinh FK that cho cac bang ton kho:
  - `InventoryItems.StoreId -> Stores.Id`
  - `InventoryItems.SkuId -> Skus.Id`
  - `StockMovements.StoreId -> Stores.Id`
  - `StockMovements.SkuId -> Skus.Id`
  - `StockDocuments.StoreId -> Stores.Id`
  - `StockDocuments.ToStoreId -> Stores.Id`
  - `StockDocumentLines.SkuId -> Skus.Id`
  - `Reservations.OrderId -> Orders.Id`
  - `Reservations.OrderLineId -> OrderLines.Id`
  - `Reservations.SkuId -> Skus.Id`
  - `Reservations.StoreId -> Stores.Id`
- [x] Cau hinh FK that cho ban hang:
  - `Carts.UserId -> Users.Id`
  - `CartItems.SkuId -> Skus.Id`
  - `Orders.UserId -> Users.Id`
  - `OrderLines.SkuId -> Skus.Id`
  - `Allocations.StoreId -> Stores.Id`
  - `OrderStatusHistories.OrderId -> Orders.Id`
  - `Payments.OrderId -> Orders.Id`
- [x] Cau hinh FK that cho review/warranty/content:
  - `Reviews.ProductId -> Products.Id`
  - `Reviews.UserId -> Users.Id`
  - `Reviews.OrderId -> Orders.Id`
  - `Warranties.OrderId -> Orders.Id`
  - `Warranties.SkuId -> Skus.Id`
  - `Warranties.CustomerId -> Users.Id`
  - `ContactRequests.ProductId -> Products.Id`
- [x] Cau hinh FK catalog va actor bo sung:
  - `ProductImages.SkuId -> Skus.Id`
  - `PartCompatibilities.BrandId -> Brands.Id`
  - `PartCompatibilities.VehicleModelId -> VehicleModels.Id`
  - `StockDocuments.CreatedBy/ApprovedBy -> Users.Id`
  - `StockMovements.PerformedBy -> Users.Id`
  - `OrderStatusHistories.ChangedBy -> Users.Id`
  - `Payments.RecordedBy -> Users.Id`
  - `Posts.AuthorId -> Users.Id`
- [x] Them check constraint co ban cho so luong, tien, rating va khoang ngay.
- [ ] Them check constraint cho enum string sau khi chot state machine.
- [x] Dam bao `StockMovement` append-only: `AppDbContext` chan update/delete qua EF.
- [ ] Dam bao `InventoryItem.OnHand >= 0`, `Reserved >= 0`, `Available >= 0`.
- [x] Cap nhat snapshot ton va ghi ledger trong cung mot `SaveChanges`.
- [x] Boc checkout trong `IUnitOfWork` transaction de khong luu don nua chung.
- [x] Quet source BE va FE khong thay pattern mojibake luu trong file. Ky tu meo khi doc bang PowerShell la loi render terminal.

### P1 - Bat buoc co de dung du nghiep vu da chot

- [x] Them `VoucherScope(VoucherId, ScopeType, RefId)`.
- [x] Them `VoucherRedemption(VoucherId, UserId, OrderId, Amount, RedeemedAt)`.
- [x] Them `OrderVoucher(OrderId, VoucherCodeSnapshot, DiscountAmount)`.
- [x] Them `AuditLog(Entity, EntityId, Action, OldValueJson, NewValueJson, ActorId, ActorName, At)`.
- [ ] Them cau hinh van hanh co schema ro rang thay vi bang key-value tuy y neu cac field da on dinh.
- [x] Them `Warranty.OrderLineId` de doi soat dung mat hang da mua, khong chi `OrderId`.
- [ ] Luu lich su bao hanh neu can theo doi luong tiep nhan -> xu ly -> hoan tat.
- [ ] Kiem tra review: chi cho tao khi don da `Delivered` hoac `Completed`.

## 3. Diem can chot nghiep vu

- [ ] Don `Pickup` co bat buoc chon store ngay luc checkout hay chi khi admin allocate?
- [ ] Reservation pool `StoreId = null` se tang `Reserved` o dau? Can mot quy tac nhat quan de khong double-count.
- [ ] Khi allocate: tru `OnHand` ngay luc allocate, luc pick, hay luc ship?
- [ ] Thanh toan thu cong co can buoc `Pending -> Paid`, hay tao payment la ghi nhan `Paid` ngay?
- [ ] Don dat coc va tra gop co cho phep giao xe khi con cong no khong?
- [ ] Xoa san pham la soft-delete hay hard-delete? Khuyen nghi soft-delete khi da phat sinh giao dich.
- [ ] Mot admin duy nhat la rule nghiep vu hay chi la seed ban dau?

## 4. Khoang trong API v2

- [ ] User admin CRUD va status management.
- [ ] Payment detail va confirm thu cong.
- [ ] Audit log list/filter/detail.
- [ ] Operations settings va store management day du.
- [ ] Report/dashboard endpoint tong hop server-side.
- [ ] Product soft-delete/deactivate.
- [ ] Order timeline DTO tra lich su trang thai.
- [ ] Allocation suggestion + allocation detail day du cho FE.

## 5. Quy tac trien khai tiep

1. Sua schema va migration truoc.
2. Sua repository/service theo transaction va rule nghiep vu.
3. Bo sung controller/API contract.
4. Chuyen service FE theo module.
5. Chinh component FE dua tren DTO v2.
6. Build va test UI that sau tung lo.

## 6. Bootstrap DB

- [x] `APIService` la owner duy nhat cua migrate + seed DB.
- [x] `AuthService` khong tu migrate/seed khi khoi dong de tranh race condition.
- [x] Seed user idempotent theo tung email.
- [x] Test khoi dong `APIService` hai lan lien tiep.
- [x] Test khoi dong `APIService` va `AuthService` song song.
- [x] Test dang nhap `admin@motosale.local`.

## 7. Rich Demo Seed

- [x] Seed tang dan theo code/slug, chay lai khong nhan ban du lieu.
- [x] Catalog: 5 hang, 10 dong xe, 10 danh muc, 14 san pham, 25 SKU.
- [x] Inventory: 4 kho/showroom, 100 snapshot ton va 100 ledger receipt dau ky.
- [x] Marketing/content: 3 voucher, 2 voucher scope, 3 bai viet, 3 FAQ, 2 banner, 2 lien he.
- [x] Operations: 10 user trong file seed rieng (1 admin, 1 staff, khach hang hoat dong va bi khoa), dia chi giao nhan, 6 don phu cac trang thai, 8 dong don, 11 timeline, 4 payment, review va warranty.
