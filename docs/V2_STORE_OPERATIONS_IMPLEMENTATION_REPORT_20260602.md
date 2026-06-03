# V2 Store Operations Implementation Report - 2026-06-02

## Kết quả

Đã triển khai lớp vận hành cửa hàng cốt lõi trên SQL Server `MoToSaleV2`.

### Database

Migration mới: `CompleteStoreOperations`.

Các bảng mới:

- `Suppliers`
- `PurchaseOrders`
- `PurchaseOrderLines`
- `GoodsReceipts`
- `GoodsReceiptLines`
- `CashTransactions`
- `RepairOrders`
- `RepairOrderLines`
- `CustomerInteractions`
- `StaffAttendances`

### Backend

- API mới: `/api/business-operations/*`.
- Tạo và sửa nhà cung cấp.
- Tạo đơn mua nhiều SKU, duyệt, hủy và nhận hàng từng phần.
- Nhận hàng cập nhật tồn kho và ghi `StockMovements`.
- Phiếu thu chi cơ bản.
- Phiếu sửa chữa cơ bản.
- Chăm sóc khách hàng và nhắc việc.
- Check-in, check-out nhân viên.
- Lookup tập trung để UI không nhập ID kỹ thuật.
- Summary vận hành cho dashboard.

### Frontend Admin

- Route mới: `/business-operations`.
- Sidebar mới: `Vận hành cửa hàng`.
- Sáu tab: Nhà cung cấp, Mua hàng, Thu chi, Sửa chữa, Chăm sóc KH, Chấm công.
- Export XLSX theo từng tab.
- Import XLSX nhà cung cấp có báo lỗi từng dòng.
- Trang `/advanced-operations` đã sửa lỗi font và thay ID bằng selector.

## Xác minh

- `dotnet test v2/backend/MoToSale.slnx --no-restore`: pass `17/17`.
- `npm run build`: pass.
- Migration đã apply lên SQL Server.
- Anonymous gọi API vận hành: `401`.
- Staff hợp lệ gọi API vận hành: `200`.
- Smoke benchmark endpoint mới: khoảng `6-22 ms` với dữ liệu hiện tại.
- Mutation E2E đã chạy và cleanup dữ liệu có tiền tố `E2E-`.

## Chưa hoàn tất

Các phần còn lại được ghi rõ trong mục tiến độ của `V2_STORE_OPERATIONS_COMPLETION_PLAN.md`. Không coi hệ thống là nghiệm thu cửa hàng hoàn chỉnh trước khi các mục `Pending` cuối cùng được xử lý.
