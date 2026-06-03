SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now datetime2 = SYSUTCDATETIME();
DECLARE @CategoryId int = (SELECT TOP 1 Id FROM Categories ORDER BY Id);
DECLARE @StoreId int = (SELECT TOP 1 Id FROM Stores ORDER BY Id);
DECLARE @UserId int = (SELECT TOP 1 Id FROM Users ORDER BY Id);

INSERT INTO Products (Code, Name, Slug, CategoryId, BrandId, VehicleModelId, Kind, ShortDescription, Description, IsFeatured, IsHotDeal, ManufacturerId, CreatedDate, Status)
VALUES ('LOAD-PRODUCT', N'Sản phẩm benchmark', 'load-product', @CategoryId, NULL, NULL, 2, N'Benchmark', N'Benchmark', 0, 0, NULL, @Now, 1);
DECLARE @ProductId int = SCOPE_IDENTITY();

;WITH N AS (
  SELECT TOP (10000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
  FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO Skus (ProductId, SkuCode, VariantName, Color, Version, ListPrice, SalePrice, Barcode, CreatedDate, Status)
SELECT @ProductId, CONCAT('LOAD-SKU-', FORMAT(n, '00000')), CONCAT(N'Biến thể ', n), NULL, NULL, 100000, 90000, NULL, @Now, 1 FROM N;

INSERT INTO InventoryItems (StoreId, SkuId, OnHand, Reserved, ReorderPoint, CreatedDate, Status)
SELECT @StoreId, Id, 100, 0, 5, @Now, 1 FROM Skus WHERE ProductId = @ProductId;

DECLARE @SkuId int = (SELECT TOP 1 Id FROM Skus WHERE ProductId = @ProductId ORDER BY Id);
;WITH N AS (
  SELECT TOP (50000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
  FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO Orders (Code, UserId, Channel, OrderType, OrderStatus, PaymentStatus, FulfillmentStatus, Subtotal, DiscountTotal, ShippingFee, GrandTotal, DepositAmount, RemainingAmount, ShippingRecipient, ShippingPhone, ShippingEmail, ShippingAddress, ReceivingMethod, Note, PlacedAt, CreatedDate, Status)
SELECT CONCAT('LOAD-ORDER-', FORMAT(n, '00000')), @UserId, 'Offline', 'FullPayment', 'Delivered', 'Paid', 'Fulfilled', 90000, 0, 0, 90000, 0, 0, N'Benchmark', '0900000000', NULL, NULL, 'Pickup', NULL, @Now, DATEADD(second, n % 86400, DATEADD(day, -(n % 60), @Now)), 1 FROM N;

INSERT INTO OrderLines (OrderId, SkuId, ProductNameSnapshot, SkuCodeSnapshot, UnitPrice, Qty, LineTotal, CreatedDate, Status)
SELECT Id, @SkuId, N'Sản phẩm benchmark', 'LOAD-SKU-00001', 90000, 1, 90000, CreatedDate, 1 FROM Orders WHERE Code LIKE 'LOAD-ORDER-%';

DECLARE @StartedAt datetime2, @Elapsed int;
CREATE TABLE #Benchmarks (QueryName nvarchar(100), ElapsedMs int, ReturnedRows int);

SET @StartedAt = SYSUTCDATETIME();
SELECT TOP (15) i.Id FROM InventoryItems i JOIN Skus s ON s.Id=i.SkuId JOIN Products p ON p.Id=s.ProductId WHERE p.Id=@ProductId ORDER BY p.Name, s.SkuCode;
SET @Elapsed = DATEDIFF(millisecond, @StartedAt, SYSUTCDATETIME());
INSERT INTO #Benchmarks VALUES ('Inventory page 15/10000', @Elapsed, 15);

SET @StartedAt = SYSUTCDATETIME();
SELECT TOP (20) o.Id FROM Orders o WHERE o.Code LIKE 'LOAD-ORDER-%' ORDER BY o.Id DESC;
SET @Elapsed = DATEDIFF(millisecond, @StartedAt, SYSUTCDATETIME());
INSERT INTO #Benchmarks VALUES ('Order page 20/50000', @Elapsed, 20);

SET @StartedAt = SYSUTCDATETIME();
SELECT COUNT(*) OrderCount, SUM(GrandTotal) Revenue FROM Orders WHERE Code LIKE 'LOAD-ORDER-%' AND PaymentStatus='Paid' AND OrderStatus IN ('Delivered','Completed');
SET @Elapsed = DATEDIFF(millisecond, @StartedAt, SYSUTCDATETIME());
INSERT INTO #Benchmarks VALUES ('Revenue aggregate 50000', @Elapsed, 1);

SET @StartedAt = SYSUTCDATETIME();
SELECT TOP (10) l.SkuCodeSnapshot, SUM(l.Qty) Qty, SUM(l.LineTotal) Revenue FROM OrderLines l JOIN Orders o ON o.Id=l.OrderId WHERE o.Code LIKE 'LOAD-ORDER-%' GROUP BY l.SkuCodeSnapshot ORDER BY SUM(l.Qty) DESC;
SET @Elapsed = DATEDIFF(millisecond, @StartedAt, SYSUTCDATETIME());
INSERT INTO #Benchmarks VALUES ('Top products aggregate 50000', @Elapsed, 10);

SELECT QueryName, ElapsedMs, ReturnedRows FROM #Benchmarks ORDER BY QueryName;
SELECT COUNT(*) AS GeneratedSkus FROM Skus WHERE ProductId=@ProductId;
SELECT COUNT(*) AS GeneratedOrders FROM Orders WHERE Code LIKE 'LOAD-ORDER-%';
ROLLBACK;
