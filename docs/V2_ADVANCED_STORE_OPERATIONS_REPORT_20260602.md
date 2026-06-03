# V2 Advanced Store Operations Report - 2026-06-02

## Kết quả

- Backend build: `PASS`
- Backend official tests: `PASS` (`14/14`)
- Frontend Admin production build: `PASS`
- SQL Server migration: `PASS`
- API smoke test through gateway `5100`: `PASS`
- UI route `/advanced-operations`: render thực tế `PASS`

## Nghiệp vụ đã bổ sung

### Trả hàng / hoàn tiền

- Tạo phiếu trả hàng theo đơn và dòng đơn.
- Chỉ cho trả đơn `Delivered` hoặc `Completed`.
- Chặn số lượng trả vượt số lượng đã bán/trả trước đó.
- Phân loại hàng trả: `Resellable`, `Damaged`, `Warranty`.
- Duyệt phiếu trả hàng.
- Hàng `Resellable` được nhập lại tồn kho và ghi `StockMovement` có `RefType = SalesReturn`.
- Tạo phiếu hoàn tiền `Refund`.
- Có thao tác từ chối phiếu trả hàng.

### Công nợ / tiền cọc

- API tổng hợp theo đơn:
  - Tổng giá trị đơn.
  - Tiền cọc yêu cầu.
  - Tổng tiền đã thu.
  - Tổng tiền đã hoàn.
  - Tiền thu ròng.
  - Số tiền còn phải thu.
- Tận dụng `Payments` và `Refunds`, không tạo ledger trùng dữ liệu.

### Bảo hành chi tiết

- Mở rộng hồ sơ bảo hành:
  - Khách hàng, SĐT.
  - Số khung, số máy.
  - Lỗi khách báo.
  - Chi phí dự kiến, chi phí thực tế.
  - Ngày tiếp nhận, ngày hoàn tất.
- Luồng xử lý:
  - `Received`
  - `Processing`
  - `WaitingParts`
  - `Completed`
  - `Rejected`
- Có bảng `WarrantyHistories`.
- Mỗi lần cập nhật trạng thái ghi lịch sử, ghi chú, chi phí và người thực hiện.

### Phân ca nhân viên

- Tạo, sửa, xóa ca làm việc.
- Ca gắn với nhân viên và cửa hàng.
- Trạng thái: `Scheduled`, `Completed`, `Cancelled`.
- Chặn thời gian bắt đầu lớn hơn hoặc bằng thời gian kết thúc.
- Chặn hai ca trùng giờ của cùng nhân viên.

## Schema English mới

- `SalesReturns`
- `SalesReturnLines`
- `Refunds`
- `StaffShifts`
- `WarrantyHistories`
- Các cột mở rộng trong `Warranties`

Migration:

`D:\MotorTeam\MoToSale-End\v2\backend\src\MoToSale.Repository\Migrations\20260602063919_AdvancedStoreOperations.cs`

Đã apply vào SQL Server LocalDB `MoToSaleV2`.

## Backend API mới

- `GET /api/advanced-operations/returns`
- `GET /api/advanced-operations/returns/{id}`
- `POST /api/advanced-operations/returns`
- `POST /api/advanced-operations/returns/{id}/approve`
- `POST /api/advanced-operations/returns/{id}/reject`
- `GET /api/advanced-operations/refunds`
- `GET /api/advanced-operations/receivables`
- `GET /api/advanced-operations/shifts`
- `POST /api/advanced-operations/shifts`
- `PUT /api/advanced-operations/shifts/{id}`
- `DELETE /api/advanced-operations/shifts/{id}`

## FE Admin

- Thêm menu `Vận hành nâng cao`.
- Thêm route `/advanced-operations`.
- Ba tab:
  - `Trả hàng / hoàn tiền`
  - `Công nợ / cọc`
  - `Ca làm việc`
- Thêm adapter để trang bảo hành cũ dùng contract English mới.

## Test

Backend test mới:

- Duyệt trả hàng nhập lại tồn, tạo refund và cập nhật công nợ.
- Chặn phân ca trùng giờ.
- Bảo hành ghi lịch sử và chi phí hoàn tất.

Smoke API thật:

- `GET /api/advanced-operations/receivables`: trả `6` đơn.
- Tạo ca test qua gateway: `PASS`.
- Danh sách thấy ca vừa tạo: `PASS`.
- Xóa ca test cleanup: `PASS`.
- DB sau cleanup: `SalesReturns = 0`, `Refunds = 0`, `StaffShifts = 0`, `WarrantyHistories = 0`.

## Ghi chú

- Các service hiện chạy tại `5100`, `5101`, `5102`; FE dev server tại `5176`.
- UI route mới đã render thực tế. Kết nối điều khiển trình duyệt chậm khi click chuyển tab, nên nên chạy thêm một vòng UI regression riêng trước khi đưa vào cửa hàng.
