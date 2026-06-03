# V2 Admin Modal Full Submit Test Plan

## Execution Result - 2026-06-02
Status: Done

- Đã chạy test UI thật bằng Playwright theo harness `tools/admin-modal-submit-test.mjs`.
- Report kết quả: `docs/V2_ADMIN_MODAL_FULL_SUBMIT_TEST_REPORT_20260602.md`.
- Evidence screenshots: `docs/modal-full-submit-test-20260602/`.
- Kết quả cuối: 66 Pass, 0 Fail, 0 Blocked.
- `npm run build` sau khi sửa lỗi: Pass.

## Mục tiêu
Kiểm tra toàn bộ modal/dialog/confirm trong `v2/frontend-admin` bằng UI thật, bảo đảm không chỉ mở modal mà phải bấm hết các nút, nhập hết các trường quan trọng, submit thật, kiểm tra response từ BE, kiểm tra dữ liệu hiển thị lại trên bảng và kiểm tra reload/quay lại trang.

## Rule bắt buộc
- Không được kết luận `Pass` nếu chỉ mở modal mà chưa bấm nút submit/action chính.
- Mọi modal có nút `Lưu`, `Cập nhật`, `Thêm`, `Xóa`, `Đồng ý`, `In`, `Upload`, `Xác nhận` phải được bấm thật.
- Mọi nút đóng modal phải được test: nút `x`, `Đóng`, `Hủy`, click thao tác mở lại sau khi đóng.
- Mọi modal form phải test ít nhất 4 bộ dữ liệu: hợp lệ, thiếu field bắt buộc, sai định dạng, dữ liệu dài/ký tự tiếng Việt có dấu.
- Modal có upload file phải test file hợp lệ, file sai định dạng, file quá lớn nếu BE có giới hạn, reload sau upload.
- Modal có chọn nhiều mục phải test không chọn gì, chọn một, chọn nhiều, bỏ chọn, lưu lại.
- Modal có dữ liệu phụ thuộc BE phải kiểm tra request/response, HTTP status, payload gửi lên, dữ liệu trả về.
- Sau submit thành công phải kiểm tra bảng/list/detail phản ánh dữ liệu mới.
- Sau submit thất bại phải kiểm tra thông báo lỗi có dấu, rõ nghiệp vụ, modal không mất dữ liệu đang nhập.
- Sau mỗi nhóm trang phải chạy `npm run build`.
- Nếu phát hiện lỗi, ghi `Fail`, nguyên nhân, file liên quan, ảnh chụp màn hình, request/response, rồi sửa và test lại.

## Chuẩn ghi nhận kết quả
Mỗi modal dùng format:

| Trang | Modal | Nút/Action | Test data | Expected | Actual | Status | Evidence |
|---|---|---|---|---|---|---|---|

Status chỉ dùng: `Pending`, `In Progress`, `Pass`, `Fail`, `Blocked`, `Fixed`, `Retest Pass`.

Evidence gồm:
- Screenshot trước submit.
- Screenshot sau submit.
- Log network các endpoint liên quan.
- ID bản ghi được tạo/sửa/xóa nếu có.
- File ảnh/export nếu có.

## Phase 1 - Chuẩn bị môi trường
Status: Pending

- Xác nhận BE v2, Gateway, AuthService và FE admin đang chạy đúng port.
- Đăng nhập bằng tài khoản admin duy nhất.
- Chạy `npm run build` baseline.
- Tạo thư mục evidence: `docs/modal-full-submit-test-20260602/`.
- Chuẩn bị file test:
  - PNG/JPG hợp lệ cho logo/banner/ảnh sản phẩm.
  - File không phải ảnh để test lỗi upload.
  - Bộ dữ liệu tiếng Việt có dấu.
  - Bộ dữ liệu dài để test tràn modal.
- Bật capture network bằng Playwright cho mọi request `/api`.

## Phase 2 - Modal nội dung CRUD thấp rủi ro
Status: Pending

### FAQ
Status: Pending
- Modal thêm FAQ: submit thiếu câu hỏi, thiếu câu trả lời, hợp lệ.
- Modal sửa FAQ: kiểm tra dữ liệu cũ được fill đúng, sửa tiếng Việt có dấu, đổi trạng thái hiển thị/ẩn.
- Confirm xóa FAQ: cancel và confirm.
- Kiểm tra bảng sau reload.

### Danh mục
Status: Pending
- Modal thêm danh mục cha.
- Modal thêm danh mục con.
- Modal sửa danh mục: tên, slug, danh mục cha, trạng thái.
- Confirm xóa danh mục có/không có sản phẩm liên quan.
- Kiểm tra danh mục xe máy/phụ tùng hiển thị đúng nghiệp vụ.

### Hãng xe
Status: Pending
- Modal thêm hãng: tên, slug, trạng thái.
- Modal sửa hãng: dữ liệu fill đúng, đổi tên, đổi trạng thái.
- Upload logo hợp lệ và reload kiểm tra ảnh còn.
- Upload file sai định dạng.
- Confirm xóa hãng.

### Dòng xe
Status: Pending
- Modal thêm dòng xe: bắt buộc chọn hãng.
- Modal sửa dòng xe: hãng, tên dòng xe, slug, trạng thái.
- Confirm xóa dòng xe.
- Kiểm tra filter dòng xe theo hãng sau khi submit.

## Phase 3 - Modal nội dung và truyền thông
Status: Pending

### Bài viết
Status: Pending
- Modal thêm bài viết: tiêu đề, slug, nội dung dài, danh mục, trạng thái, ảnh đại diện file.
- Modal sửa bài viết: kiểm tra không tràn nội dung, dữ liệu cũ fill đúng.
- Upload ảnh đại diện hợp lệ/sai định dạng.
- Confirm xóa bài viết.
- Reload và quay lại trang kiểm tra ảnh, trạng thái, nội dung.

### Banner trang chủ
Status: Pending
- Modal thêm banner: tiêu đề, mô tả, link, thứ tự, trạng thái, ảnh file.
- Modal sửa banner: dữ liệu cũ fill đúng, đổi ảnh, reload còn ảnh.
- Test dữ liệu dài không vỡ modal.
- Confirm xóa banner.

### Liên hệ
Status: Pending
- Modal xem chi tiết liên hệ.
- Nút đánh dấu đã xử lý/chưa xử lý nếu nằm trong modal hoặc action liên quan.
- Đóng bằng `x`, `Đóng`, mở lại cùng bản ghi.

## Phase 4 - Modal voucher và khuyến mại
Status: Pending

### Voucher
Status: Pending
- Modal thêm voucher: mã, loại giảm, giá trị, hạn mức, ngày bắt đầu/kết thúc, số lượt, trạng thái.
- Phạm vi áp dụng: tất cả, danh mục, sản phẩm cụ thể, hãng xe.
- Kiểm tra chọn bằng checkbox: không chọn gì, chọn một, chọn nhiều, bỏ chọn.
- Modal sửa voucher: dữ liệu cũ fill đúng, chữ có dấu, không vỡ layout cuối modal.
- Submit ngày sai, giá trị âm, mã trùng.
- Confirm xóa voucher.
- Kiểm tra voucher xuất hiện đúng ở danh sách và API.

### Khuyến mại sản phẩm
Status: Pending
- Modal xem chương trình khuyến mại của sản phẩm.
- Kiểm tra nội dung khuyến mại khớp voucher/phạm vi áp dụng.
- Đóng/mở lại modal.

## Phase 5 - Modal sản phẩm, ảnh, biến thể
Status: Pending

### Sản phẩm
Status: Pending
- Modal thêm xe máy: loại sản phẩm, danh mục đúng, hãng/dòng xe, giá, trạng thái, ảnh chính file.
- Modal thêm phụ tùng: không yêu cầu hãng/dòng xe trực tiếp nếu nghiệp vụ tương thích quản lý riêng.
- Modal sửa sản phẩm: dữ liệu cũ fill đúng, không còn tồn kho trong form sửa sản phẩm nếu đã bỏ.
- Submit thiếu tên, thiếu danh mục, giá sai, dữ liệu dài.
- Confirm xóa sản phẩm: kiểm tra xóa thật hay ngừng bán theo nghiệp vụ hiện tại.

### Quản lý biến thể
Status: Pending
- Modal quản lý biến thể: thêm biến thể, sửa biến thể, xóa biến thể.
- Test SKU/mã biến thể trùng, thiếu tên, thiếu giá nếu bắt buộc.
- Kiểm tra chữ trong form thêm biến thể đồng đều, không to nhỏ lệch.
- Reload sản phẩm kiểm tra biến thể còn.

### Quản lý ảnh sản phẩm
Status: Pending
- Modal ảnh sản phẩm: upload ảnh chính/phụ bằng file.
- Gán ảnh cho biến thể nếu có.
- Đặt ảnh chính, xóa ảnh, cancel xóa.
- Reload kiểm tra ảnh không mất.
- Kiểm tra ảnh chính ở modal sửa sản phẩm có logic rõ với ảnh trong quản lý ảnh.

### Tương thích phụ tùng với xe
Status: Pending
- Modal tương thích xe: phạm vi áp dụng dễ hiểu, không fix cứng.
- Chọn theo hãng, chọn dòng xe, chọn tất cả dòng của hãng nếu có.
- Nhập `Từ năm`, `Đến năm`, ghi chú.
- Thêm, sửa, xóa cấu hình tương thích.
- Kiểm tra lỗi `Lưu tương thích thất bại` nếu còn.

### Bán kèm/phụ kiện liên quan
Status: Pending
- Modal bán kèm: thêm sản phẩm liên quan, số lượng gợi ý, ghi chú.
- Xóa cấu hình bán kèm.
- Kiểm tra sản phẩm chính/phụ không bị chọn trùng sai nghiệp vụ.

### Mã vạch và tuổi tồn kho
Status: Pending
- Modal mã vạch: mở, kiểm tra mã, bấm in.
- Modal tuổi tồn kho: mở, dữ liệu ngày nhập/tồn lâu, đóng.

## Phase 6 - Modal tồn kho
Status: Pending

### Giữ chỗ tồn kho
Status: Pending
- Modal danh sách giữ chỗ: mở từ bản ghi có giữ chỗ và không có giữ chỗ.
- Kiểm tra đơn liên quan, số lượng, thời hạn, trạng thái.
- Đóng/mở lại.

### Điều chỉnh tồn kho
Status: Pending
- Modal điều chỉnh: nhập kho, xuất kho, điều chỉnh tăng, điều chỉnh giảm.
- Test số lượng 0, âm, lớn hơn tồn khả dụng.
- Confirm trước khi ghi.
- Kiểm tra tồn thực tế, đang giữ, khả dụng, lịch sử kho sau submit.

### Ngưỡng cảnh báo tồn thấp
Status: Pending
- Modal ngưỡng: nhập số hợp lệ, số âm, rỗng.
- Submit và kiểm tra badge/trạng thái tồn thay đổi.

## Phase 7 - Modal đơn hàng
Status: Pending

### Cập nhật trạng thái đơn
Status: Pending
- Modal cập nhật trạng thái: chờ xác nhận, đang giao, đã giao, đã hủy.
- Kiểm tra trạng thái vận chuyển có đồng bộ theo nghiệp vụ hiện tại.
- Không cho chuyển trạng thái sai luồng nếu BE quy định.
- Kiểm tra lịch sử đơn hàng sinh log ngay sau từng cập nhật.

### Xác nhận thanh toán thủ công
Status: Pending
- Modal/action xác nhận thanh toán nếu còn.
- Kiểm tra unpaid/paid/canceled hiển thị hợp lý với trạng thái đơn.
- Kiểm tra trường ngày giờ đúng timezone Việt Nam.

### Hủy đơn
Status: Pending
- Confirm hủy đơn: cancel và confirm.
- Kiểm tra tồn kho/giữ chỗ/lich sử đơn nếu có rollback.

## Phase 8 - Modal khách hàng và chăm sóc khách hàng
Status: Pending

### Ghi chú chăm sóc
Status: Pending
- Modal ghi chú khách hàng: nhập ghi chú hợp lệ, rỗng, dài.
- Submit và kiểm tra timeline/ghi chú khách hàng.

### Hồ sơ khách hàng 360
Status: Pending
- Modal hồ sơ: kiểm tra dữ liệu đơn hàng, công nợ, bảo hành, chăm sóc.
- Kiểm tra các nút trong modal nếu có.
- Đóng/mở lại sau reload.

## Phase 9 - Modal vận hành nâng cao
Status: Pending

### Nhà cung cấp
Status: Pending
- Modal thêm/sửa nhà cung cấp.
- Test tên, điện thoại, email, địa chỉ, trạng thái.

### Mua hàng
Status: Pending
- Modal phiếu mua hàng: nhà cung cấp, sản phẩm/biến thể, số lượng, giá nhập.
- Submit thiếu dòng hàng, thiếu nhà cung cấp, số lượng sai.
- Kiểm tra phiếu kho/công nợ nếu BE sinh tự động.

### Thu chi/công nợ
Status: Pending
- Modal phiếu thu/chi: loại giao dịch, số tiền, đối tượng, ghi chú.
- Test số tiền âm/0, thiếu đối tượng.
- Kiểm tra báo cáo/kế toán cập nhật.

### Sửa chữa/bảo hành
Status: Pending
- Modal phiếu sửa chữa: khách hàng, xe/sản phẩm, lỗi, dịch vụ, trạng thái.
- Cập nhật trạng thái sửa chữa nếu có modal/action.
- Kiểm tra lịch sử dịch vụ khách hàng.

### CRM/chăm sóc
Status: Pending
- Modal lịch chăm sóc: khách hàng, loại chăm sóc, ngày hẹn, nội dung.
- Test ngày quá khứ/tương lai.

### Chấm công/nhân viên
Status: Pending
- Modal chấm công: nhân viên, cửa hàng, ca, giờ.
- Test quyền staff/admin nếu nghiệp vụ áp dụng.

## Phase 10 - Modal import/export
Status: Pending

### Import dữ liệu vận hành
Status: Pending
- Modal/confirm ghi dữ liệu import: cancel và confirm.
- File đúng mẫu, sai mẫu, thiếu cột, dữ liệu tiếng Việt.
- Kiểm tra dữ liệu sau import.

### Export Excel/PDF/Word nếu có modal xác nhận
Status: Pending
- Bấm export từng trang có hỗ trợ.
- Kiểm tra file tải về đúng định dạng, font tiếng Việt, tiêu đề, cột, giá trị.

## Phase 11 - Kiểm tra giao diện modal
Status: Pending

- Desktop 1440x900: modal không tràn, footer luôn thấy nút.
- Tablet 768x1024: modal cuộn thân, header/footer không đè nội dung.
- Mobile 390x844: không mất nút, không vỡ input/select.
- Text tiếng Việt có dấu hiển thị đúng.
- Select dài, checkbox nhiều mục, textarea dài không phá layout.
- Modal mở không làm footer/sidebar/layout nền vỡ.

## Phase 12 - Báo cáo và chốt
Status: Pending

- Tạo báo cáo `docs/V2_ADMIN_MODAL_FULL_SUBMIT_TEST_REPORT_20260602.md`.
- Liệt kê toàn bộ modal đã test, pass/fail/fixed.
- Đính kèm screenshot evidence.
- Chạy `npm run build`.
- Chỉ kết luận hoàn thành khi tất cả modal ở trạng thái `Pass` hoặc `Retest Pass`.
