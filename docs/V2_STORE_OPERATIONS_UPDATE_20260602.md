# V2 Store Operations Update - 2026-06-02

Migration `SupplierPaymentsAndRepairTimeline` đã được apply.

- Thêm bảng `RepairStatusHistories`.
- Thêm thanh toán NCC nhiều lần qua phiếu chi liên kết đơn mua.
- Xuất kho phụ tùng đúng một lần khi phiếu sửa chuyển sang `Repairing`.
- Backend test hiện pass `19/19`.
- Frontend production build pass.
- Thêm bản in độc lập cho đơn mua, phiếu thu chi và phiếu sửa chữa; có thể lưu PDF từ hộp thoại in của trình duyệt.
