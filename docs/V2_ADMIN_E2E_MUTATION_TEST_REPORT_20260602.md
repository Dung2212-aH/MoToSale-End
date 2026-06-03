# V2 Admin E2E Mutation Test Report - 2026-06-02

## Kết quả

- Trạng thái: `PASS`
- Marker dữ liệu test: `E2E-20260602115922`
- API base: `http://localhost:5100`
- Artifact chi tiết: `D:\MotorTeam\MoToSale-End\test-artifacts\v2-e2e-mutation-20260602\mutation-results.json`
- Script chạy lại: `D:\MotorTeam\MoToSale-End\test-artifacts\v2-e2e-mutation-20260602\run-e2e-mutation.ps1`

## Phạm vi đã test

1. Sản phẩm/phụ tùng
   - Tạo sản phẩm phụ tùng mới.
   - Sửa thông tin sản phẩm.
   - Tạo SKU/biến thể.
   - Sửa SKU/biến thể.
   - Xóa SKU phụ.
   - Xóa sản phẩm qua API, xác nhận soft-delete `Status = 0`.

2. Voucher
   - Tạo voucher amount.
   - Sửa thành voucher percent.
   - Đọc lại chi tiết để xác nhận dữ liệu cập nhật.
   - Xóa voucher qua API.

3. Đơn hàng
   - Đăng nhập customer.
   - Dọn giỏ hàng customer trước khi test.
   - Chọn SKU có tồn khả dụng.
   - Thêm vào giỏ hàng.
   - Checkout tạo đơn mới.
   - Admin đọc chi tiết đơn.
   - Admin cập nhật trạng thái `Confirmed`.
   - Admin cập nhật trạng thái `Shipping`.
   - Admin hủy đơn.
   - Xác nhận trạng thái cuối `Cancelled`.
   - Xác nhận có lịch sử đơn hàng, tổng cộng 5 log.

4. Phiếu kho
   - Tạo phiếu nhập kho ở trạng thái `Draft`.
   - Đọc chi tiết phiếu để xác nhận dòng phiếu.
   - Hủy phiếu kho.
   - Xác nhận trạng thái cuối `Cancelled`.

5. Bài viết
   - Tạo bài viết draft.
   - Sửa thành bài viết published.
   - Đọc lại chi tiết để xác nhận title/status.
   - Xóa bài viết qua API.

## Cleanup

- Cleanup theo marker `E2E-20260602115922`: trước cleanup còn 3 dòng rác nghiệp vụ mềm, sau cleanup còn 0.
- Sau đó kiểm tra toàn DB theo prefix `E2E-%`, phát hiện 1 phiếu kho test cũ `E2E-20260602 draft cancel` và đã dọn nốt.
- Kết quả cuối cùng: `E2ERows = 0`.

## Ghi chú

- Các phần có endpoint xóa thật như voucher và bài viết đã xóa qua API.
- Sản phẩm đang dùng soft-delete theo nghiệp vụ, nên script xác nhận `Status = 0` trước khi SQL cleanup dòng test.
- Đơn hàng không có endpoint xóa vì cần giữ lịch sử nghiệp vụ, nên script test update/cancel qua API rồi cleanup bằng SQL theo marker E2E.
- Phiếu kho đã hủy qua API; SQL chỉ dọn dòng test để không để rác trong DB sau E2E.
