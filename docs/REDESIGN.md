# MoToSale — Thiết kế lại toàn bộ hệ thống (v2)

> Trạng thái: **ĐÃ DUYỆT — KHÓA SPEC (2026-06-01)** · Phạm vi: toàn bộ hệ thống · Dữ liệu: làm mới hoàn toàn (greenfield + seed data)
> Mục tiêu: một thiết kế nghiệp vụ + DB sạch, đúng chuẩn hiện nay, hỗ trợ **bán lẻ đa cửa hàng** (tồn riêng từng cửa hàng, chuyển kho, admin phân phối đơn) mà không phát sinh "trôi số".

---

## 1. Nguyên tắc thiết kế

1. **Mọi đơn vị bán đều là SKU.** Sản phẩm không biến thể vẫn có đúng 1 SKU mặc định → bỏ hẳn cảnh tồn 2 tầng (`SANPHAM` + `BIENSANPHAM`) phải sync bằng trigger.
2. **Tồn kho theo sổ cái (ledger).** Không lưu "số tổng" rời rạc làm nguồn sự thật. `OnHand`/`Reserved` là snapshot, mọi thay đổi đi qua bản ghi bất biến `StockMovement` → truy vết tuyệt đối, không lệch số.
3. **Tồn luôn gắn địa điểm.** `Inventory(StoreId, SkuId)`. Không có khái niệm tồn "toàn cục" — tổng chỉ là phép cộng các cửa hàng.
4. **Tách Đơn hàng khỏi Phân bổ giao (Allocation).** Đơn ghi "mua gì"; Allocation ghi "cửa hàng nào giao bao nhiêu". Admin phân phối hay tự động chỉ là cách sinh Allocation. Hỗ trợ tách 1 dòng cho nhiều cửa hàng.
5. **Tính đúng đắn tồn kho tập trung 1 nơi** (Inventory service), không rải ở Payment/Order như hiện tại.
6. **Schema code-first bằng EF Core Migrations.** Bỏ file SQL viết tay khổng lồ + các `EnsureTable` chạy lúc runtime.
7. **Định danh tiếng Anh, PascalCase** ở DB/API; nhãn tiếng Việt ở giao diện. Tiền `decimal(18,2)`, thời gian `datetime2` lưu UTC.
8. **Trạng thái = chuỗi có ràng buộc** (enum tài liệu hóa), audit mọi thao tác ghi.

---

## 2. Kiến trúc tổng thể (theo khuôn BaseCore)

Cấu trúc **solution nhiều project class-library dùng chung** + microservices ở trên, **1 DB dùng chung** — đúng khuôn BaseCore.

```
[Frontend Khách (React)]   [Frontend Admin (React)]
            \                       /
              ▼                   ▼
            [ApiGateway (Ocelot)]
              /              \
       [AuthService]     [APIService]      ← microservice host (controllers)
              \              /
   ┌───────────────────────────────────────┐
   │  Thư viện dùng chung (class libraries)  │
   │  Common · Entities · DTO · Repository · Services
   └───────────────────────────────────────┘
                     │
              [SQL Server — 1 DB chung: MoToSale]
```

**Project trong solution**

| Project | Loại | Nội dung |
|---|---|---|
| **MoToSale.Common** | lib | `BaseEntity` (Id, CreatedDate, UpdatedDate, Status), Constants, Enums, Auth (RoleConstant, TokenHelper), Security (PasswordHasher), Helpers |
| **MoToSale.Entities** | lib | Tất cả entity + folder Audit + SeedConfiguration |
| **MoToSale.DTO** | lib | Common (`PagingRequest/PagingResponse/BaseResponse`), Response (`ApiResponse/ErrorResponse`), DTO theo domain |
| **MoToSale.Repository** | lib | `IRepository<T>` + `Repository<T>` (generic CRUD) + repo cụ thể; `AppDbContext` (dùng chung) |
| **MoToSale.Services** | lib | Interface + service nghiệp vụ (Controller → Service → Repository → DB) |
| **MoToSale.AuthService** | API | Xác thực, người dùng, role (host controllers) |
| **MoToSale.APIService** | API | Catalog, Inventory, Ordering, Payment, Content... (host controllers) |
| **MoToSale.ApiGateway** | API | Ocelot — định tuyến |

> Domain (Catalog/Inventory/Ordering/Payment/Content...) tách theo **folder/namespace** trong các lib dùng chung và controllers trong APIService, **không** tách thành DB/service riêng (theo basecore). Warranty/Review/Audit đặt trong lib dùng chung.

---

## 3. Mô hình dữ liệu (schema theo context)

Quy ước: PK `Id INT IDENTITY` trừ khi nói khác; `CreatedAt/UpdatedAt datetime2 (UTC)`; FK in *nghiêng*.

### 3.1 Identity
- **User**(Id, FullName, Email *unique*, PhoneNumber, PasswordHash, IsActive, CreatedAt, UpdatedAt)
- **Role**(Id, Code *unique* [Admin|Staff|Customer], Name)
- **UserRole**(*UserId*, *RoleId*) — PK kép
- **Address**(Id, *UserId*, RecipientName, Phone, Line, Ward, District, Province, IsDefault)

### 3.2 Catalog
- **Brand**(Id, Name, Slug *unique*, LogoUrl, IsActive)
- **VehicleModel**(Id, *BrandId*, Name, Slug *unique*, IsActive)
- **Category**(Id, *ParentId?*, Name, Slug *unique*, Kind [Motorcycle|Part], SortOrder, IsActive)
- **Product**(Id, Code *unique*, Name, Slug *unique*, *CategoryId*, *BrandId?*, *VehicleModelId?*, Kind [Motorcycle|Part], ShortDescription, Description, **IsFeatured**, **IsHotDeal**, Status [Active|Inactive], IsActive, CreatedAt, UpdatedAt)
  - *Không* chứa giá, *không* chứa tồn.
- **Sku**(Id, *ProductId*, SkuCode *unique*, VariantName, Color, Version, **ListPrice**, **SalePrice?**, Barcode?, IsActive)
  - Mỗi Product có ≥1 Sku; sản phẩm "không biến thể" → 1 Sku mặc định.
- **ProductImage**(Id, *ProductId*, *SkuId?*, Url, Alt, IsPrimary, SortOrder)
- **PartCompatibility**(Id, *PartProductId*, *BrandId?*, *VehicleModelId?*, YearFrom?, YearTo?, AppliesToAll, Note, IsActive)
- **Store**(Id, Code *unique*, Name, Slug, Type [Showroom|Warehouse|Online], AddressLine, Province, District, Ward, Phone, Email, Latitude?, Longitude?, OpeningHours, IsActive, IsDefault)

### 3.3 Inventory (lõi mới quan trọng nhất)
- **InventoryItem**(Id, *StoreId*, *SkuId*, **OnHand**, **Reserved**, ReorderPoint, UpdatedAt) — *unique(StoreId, SkuId)*
  - `Available = OnHand − Reserved` (tính, không lưu).
- **StockMovement**(Id, *StoreId*, *SkuId*, Type [Receipt|Issue|AdjustIn|AdjustOut|TransferOut|TransferIn|ReserveHold|ReserveRelease], QtyDelta, BalanceAfter, RefType, RefId, Reason, PerformedBy, OccurredAt)
  - **Bất biến** (append-only). `OnHand` luôn = tổng QtyDelta của các movement ảnh hưởng OnHand.
- **StockDocument**(Id, Code *unique*, Type [Receipt|Issue|Adjustment|Stocktake|Transfer], Status [Draft|Approved|Cancelled], *StoreId*, *ToStoreId?* (chuyển kho), Note, CreatedBy, ApprovedBy, CreatedAt, ApprovedAt)
- **StockDocumentLine**(Id, *DocumentId*, *SkuId*, Qty, Note)
- **Reservation**(Id, *OrderId*, *OrderLineId*, *SkuId*, *StoreId?*, Qty, Status [Active|Confirmed|Released|Expired], ExpiresAt, CreatedAt)
  - `StoreId NULL` = giữ chỗ ở mức tổng (pool); gán cửa hàng khi phân bổ.

### 3.4 Ordering
- **Cart**(Id, *UserId*, Status [Active|CheckedOut|Abandoned], CreatedAt) · **CartItem**(Id, *CartId*, *SkuId*, Qty, UnitPriceSnapshot)
- **Order**(Id, Code *unique*, *UserId*, Channel [Online|POS], Type [FullPayment|Deposit|Installment], Status [Pending|AwaitingPayment|Confirmed|Allocated|Shipping|Delivered|Completed|Cancelled], PaymentStatus [Unpaid|DepositPaid|PartiallyPaid|Paid|Refunded], FulfillmentStatus [Unallocated|Allocated|Shipped|Fulfilled], Subtotal, DiscountTotal, ShippingFee, GrandTotal, DepositAmount, RemainingAmount, ShippingRecipient, ShippingPhone, ShippingEmail, ShippingAddress, ReceivingMethod [Delivery|Pickup], Note, PlacedAt, CreatedAt, UpdatedAt)
- **OrderLine**(Id, *OrderId*, *SkuId*, ProductNameSnapshot, SkuCodeSnapshot, UnitPrice, Qty, LineTotal)
- **Allocation**(Id, *OrderLineId*, *StoreId*, Qty, Status [Planned|Picked|Shipped|Fulfilled|Cancelled], CreatedAt) — **cho phép 1 dòng tách nhiều cửa hàng**
- **OrderStatusHistory**(Id, *OrderId*, FromStatus, ToStatus, Note, ChangedBy, ChangedAt)
- **Voucher**(Id, Code *unique*, Description, DiscountType [Percent|Amount], DiscountValue, MaxDiscount?, MinOrderValue, UsageLimit?, PerUserLimit?, StartAt, EndAt, IsActive)
- **VoucherScope**(Id, *VoucherId*, ScopeType [Product|Category|Brand], RefId) — targeting tùy chọn
- **VoucherRedemption**(Id, *VoucherId*, *UserId*, *OrderId*, Amount, RedeemedAt)
- **OrderVoucher**(Id, *OrderId*, VoucherCodeSnapshot, DiscountAmount)

### 3.5 Payment
- **Payment**(Id, Code *unique*, *OrderId*, Type [Deposit|Full|Remaining|Installment], Amount, Method [COD|BankTransfer|VNPay|Momo|Card], Status [Pending|Paid|Failed|Cancelled], TransactionRef, PaidAt, CreatedAt)

### 3.6 Content / Review / Warranty / Audit
- **Post**(Id, Title, Slug *unique*, Summary, Body, CoverUrl, Category, Status [Draft|Published], PublishedAt, AuthorId)
- **Faq**(Id, Question, Answer, Category, SortOrder, IsActive)
- **ContactRequest**(Id, FullName, Phone, Email, Subject, Body, Type [General|Product|TestDrive|Consultation], *ProductId?*, Status [New|Processed], CreatedAt, HandledAt)
- **HomeBanner**(Id, Position [Slider|BannerLeft|BannerRight|ProductBanner], Title, ImageUrl, Link, SortOrder, IsActive)
- **Review**(Id, *ProductId*, *UserId*, *OrderId*, Rating [1..5], Title, Comment, ImageUrl, Status [Pending|Approved|Rejected], CreatedAt) — chỉ cho đánh giá khi user đã mua & đơn Delivered/Completed
- **Warranty**(Id, Code, *OrderLineId?*, *SkuId*, CustomerId, ProductSnapshot, SerialNumber?, StartAt, Months, Status [Active|Expired|Void], Note)
- **AuditLog**(Id, Entity, EntityId, Action, OldValue (json), NewValue (json), ActorId, ActorName, At)

---

## 4. Ngữ nghĩa tồn kho & khả dụng

- **Khả dụng toàn hệ thống của 1 SKU** = `SUM(OnHand)_các cửa hàng − SUM(Reserved active/confirmed)`. Đây là con số khách thấy khi mua.
- **Khả dụng theo cửa hàng** = `OnHand − Reserved` của cửa hàng đó — hiển thị tham khảo cho khách, dùng để admin phân phối.
- Không bao giờ bán vượt tổng nhờ `Reserved` tăng ngay khi giữ chỗ.

---

## 5. Các luồng nghiệp vụ chính

### 5.1 Mua → giữ chỗ → thanh toán → phân phối → giao
```
1. Khách thêm SKU vào Cart (giá snapshot).
2. Checkout: tạo Order(Pending) + OrderLine; Inventory tạo Reservation(Active, StoreId=null),
   Reserved++ ở mức tổng; Order → AwaitingPayment (có hạn giữ chỗ).
3. Thanh toán thành công: Payment(Paid); Order.PaymentStatus = Paid/DepositPaid;
   Reservation → Confirmed (vẫn giữ, CHƯA trừ OnHand cửa hàng); Order.Status = Confirmed;
   FulfillmentStatus = Unallocated.
4. Phân phối (admin thủ công HOẶC tự động gợi ý): sinh Allocation theo dòng theo cửa hàng
   (tách được). Khi chốt: Inventory ghi StockMovement(Issue) tại cửa hàng → OnHand--,
   Reservation Release → Reserved--. FulfillmentStatus = Allocated.
5. Giao: Allocated → Shipped → Fulfilled; Order → Shipping → Delivered → Completed.
```

### 5.2 Hủy / hoàn
- Hủy **trước phân phối**: Reservation → Released (Reserved--). Nếu đã thanh toán → hoàn tiền (Payment Refunded).
- Hủy **sau phân phối**: ghi StockMovement đảo chiều (Receipt trả lại OnHand) + hoàn tiền.

### 5.3 Chuyển kho
- Tạo StockDocument(Type=Transfer, StoreId=A, ToStoreId=B, Draft) → duyệt → ghi `TransferOut` tại A và `TransferIn` tại B. Tổng không đổi.

### 5.4 Nhập / xuất / điều chỉnh / kiểm kê
- StockDocument theo cửa hàng → duyệt → sinh StockMovement tương ứng (Receipt/Issue/AdjustIn/AdjustOut). Kiểm kê (Stocktake) so sánh đếm thực tế và sinh AdjustIn/AdjustOut.

### 5.5 Đặt cọc / trả góp
- Order.Type = Deposit/Installment; DepositAmount + RemainingAmount theo dõi; nhiều Payment cộng dồn; trang chủ nhắc "đơn còn phải thanh toán".

---

## 6. API chính (tóm tắt, qua Gateway)

| Nhóm | Endpoint tiêu biểu |
|---|---|
| Auth | `POST /api/auth/login`, `register`, `GET /api/users/me` |
| Catalog | `GET /api/products`, `GET /api/products/{id}`, `GET /api/skus/{id}`, `GET /api/categories`, `GET /api/brands`, `GET /api/stores` |
| Inventory | `GET /api/inventory?storeId=`, `POST /api/inventory/documents`, `POST /api/inventory/documents/{id}/approve`, `POST /api/inventory/transfers`, `GET /api/inventory/movements` |
| Cart/Order | `GET/POST /api/cart/items`, `POST /api/orders` (checkout), `GET /api/orders/{id}`, `POST /api/orders/{id}/allocate`, `PUT /api/orders/{id}/status` |
| Promotion | `POST /api/vouchers/validate`, `GET /api/vouchers` |
| Payment | `POST /api/payments`, `POST /api/payments/{id}/confirm` |
| Admin phân phối | `POST /api/orders/{id}/allocations` (sinh/đổi allocation theo cửa hàng), gợi ý tự động `GET /api/orders/{id}/allocation-suggestion` |

---

## 7. Phân quyền (tóm tắt)
- **Customer**: duyệt sp, giỏ, đặt hàng, thanh toán, đánh giá (nếu đã mua), xem đơn của mình.
- **Staff**: quản lý sp/tồn/đơn/phân phối/chuyển kho/nội dung; không xóa cứng, không quản user.
- **Admin**: toàn quyền + quản user/role + xóa + xem audit.

---

## 8. Seed data (vì làm mới hoàn toàn)
- 2–3 Store (1 Online mặc định + 1–2 Showroom).
- Danh mục Xe máy/Phụ tùng + vài Brand/VehicleModel.
- ~10–20 Product (mỗi cái 1–3 Sku) + ảnh mẫu + tồn ban đầu rải các cửa hàng.
- 1 Admin, 1 Staff, vài Customer mẫu.
- Vài Voucher, Post, Faq, Banner trang chủ.

---

## 9. Quyết định — ĐÃ CHỐT ✅
1. ✅ **Cấu trúc theo BaseCore**: solution nhiều class-library dùng chung (Common/Entities/DTO/Repository/Services) + microservices (ApiGateway/AuthService/APIService).
2. ✅ **1 DB dùng chung** (`MoToSale`) — không tách DB theo service.
3. ✅ **Repository**: `IRepository<T>` + `Repository<T>` generic base + repo cụ thể thêm method nghiệp vụ (theo docx).
4. ✅ **Định danh schema tiếng Anh** (PascalCase); nhãn UI tiếng Việt.
5. ✅ **Phân phối đơn**: admin thủ công + nút "gợi ý tự động".
6. ✅ **Gateway**: giữ **Ocelot**.
7. ✅ **Giá đặt ở Sku** (ListPrice/SalePrice).
8. ✅ **BaseEntity** chung (Id, CreatedDate, UpdatedDate, Status); **DTO envelope** `ApiResponse`/`PagingResponse`.

---

## 10. Lộ trình triển khai (sau khi duyệt tài liệu)

| GĐ | Nội dung | Kết quả |
|---|---|---|
| **0** | Chốt tài liệu này + các quyết định mục 9 | Spec khóa |
| **1** | DB schema (EF migrations cho từng service) + seed | DB chạy được, có dữ liệu mẫu |
| **2** | Identity + Catalog service | Đăng nhập, duyệt sp/danh mục/cửa hàng |
| **3** | Inventory service (item, ledger, document, transfer, reservation) | Tồn theo cửa hàng + nhập/xuất/chuyển kho |
| **4** | Ordering (cart, checkout, allocation) + Promotion + Payment | Mua → trả tiền → phân phối → giao |
| **5** | Frontend Khách (duyệt, giỏ, checkout, đơn, tồn theo cửa hàng) | Web khách hoàn chỉnh |
| **6** | Frontend Admin (sp, tồn, phân phối đơn, chuyển kho, báo cáo) | Trang quản trị hoàn chỉnh |
| **7** | Review, Warranty, Content/Banner, Audit, Báo cáo | Tính năng phụ trợ |

### Quy tắc chuyển Frontend Admin

- Dùng `FrontendAdmin` hiện có làm nền để giữ nguyên các luồng UI, Tailwind CSS và những sửa lỗi bố cục đã hoàn thành.
- Bản làm việc nằm tại `v2/frontend-admin`.
- Không viết lại Frontend Admin từ đầu. Chuyển từng module sang API v2 theo `docs/V2_FRONTEND_ADMIN_MIGRATION_PLAN.md`.

---

## 11. Công nghệ
- Backend: **.NET 8**, **EF Core code-first + Migrations** (bỏ SQL viết tay + EnsureTable runtime), SQL Server.
- Gateway: Ocelot/YARP. Frontend: React 18 + Vite + Tailwind (giữ).
- Quy ước: tiền `decimal(18,2)`; thời gian UTC; trạng thái chuỗi có ràng buộc; audit thao tác ghi; tồn kho tập trung ở Inventory service.
