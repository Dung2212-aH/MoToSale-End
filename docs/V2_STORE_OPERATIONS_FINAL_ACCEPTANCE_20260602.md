# V2 Store Operations Final Acceptance - 2026-06-02

## Kết luận

Phạm vi vận hành cửa hàng trong kế hoạch đã được triển khai và chạy lại đến trạng thái pass.

## Hạng mục hoàn thành

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Nhà cung cấp và mua hàng | Done | CRUD NCC, import/export XLSX, đơn mua nhiều SKU, duyệt, hủy và nhận hàng từng phần |
| Kho và giá vốn | Done | Nhận hàng cập nhật tồn, phiếu kho, lịch sử dịch chuyển, xuất phụ tùng sửa chữa đúng một lần |
| Thu chi và công nợ NCC | Done | Ghi nhận nhiều lần thanh toán, phiếu chi liên kết đơn mua, cập nhật số còn phải trả |
| Sửa chữa và bảo hành | Done | Tiếp nhận, chi phí, phụ tùng, timeline đổi trạng thái, bàn giao |
| CRM và nhân sự cơ bản | Done | Nhắc chăm sóc khách hàng, gán Staff, check-in/check-out |
| Dashboard và báo cáo | Done | Chỉ số vận hành, XLSX theo tab, báo cáo bán hàng và tồn kho |
| Import sản phẩm và tồn đầu kỳ | Done | Route `/operational-imports`, đọc XLSX, preview, validation và xác nhận trước khi ghi DB |
| Chứng từ in PDF | Done | Các màn hình vận hành có bố cục in độc lập; dùng hộp thoại in của trình duyệt để lưu PDF |
| Biểu mẫu DOCX | Done | Đã tạo hai mẫu tiếp nhận sửa chữa và bảo hành; render PNG/PDF và visual QA pass |
| Security smoke | Done | API vận hành trả `401` khi anonymous và `200` khi Staff hợp lệ |

## Kết quả kiểm thử

### Backend

- `dotnet test v2/backend/MoToSale.slnx --no-restore`: pass `19/19`.
- Mutation E2E: luồng NCC -> mua -> nhận kho -> thu chi -> sửa chữa -> CRM -> chấm công đã pass và cleanup.
- DB cleanup: còn lại `0` sản phẩm, `0` đơn hàng, `0` NCC có tiền tố test `E2E-` hoặc `LOAD-`.

### Frontend

- `npm run build`: pass.
- `npm run test:ui`: pass `24/24`.
- Regression responsive mới chạy đủ `23` route trên desktop, tablet và mobile.
- Kiểm tra không có route lỗi tải dữ liệu, mojibake hoặc tràn ngang toàn trang.
- Đã sửa lỗi mobile được regression phát hiện:
  - Header công cụ trang Tồn kho không wrap.
  - Bảng top sản phẩm trang Báo cáo thiếu vùng cuộn ngang cục bộ.
- Đã sửa màu chữ KPI trang Vận hành cửa hàng để thẻ nền sáng có độ tương phản rõ ràng.
- Tab import sản phẩm/SKU và tồn đầu kỳ đã được click kiểm tra bằng UI thật.

### Benchmark rollback

Script: `tools/load_test_store_operations.sql`.

| Truy vấn | Dữ liệu test | Thời gian |
|---|---:|---:|
| Trang tồn kho | 15 / 10.000 SKU | 31 ms |
| Trang đơn hàng | 20 / 50.000 đơn | 0 ms |
| Tổng hợp doanh thu | 50.000 đơn | 168 ms |
| Top sản phẩm | 50.000 dòng đơn | 184 ms |

Tổng thời gian chạy script: `3.826 s`. Script dùng transaction và `ROLLBACK`, không để lại dữ liệu benchmark.

## Artifacts

- `docs/templates/Bien-ban-tiep-nhan-sua-chua.docx`
- `docs/templates/Phieu-tiep-nhan-bao-hanh.docx`
- `docs/templates/render-repair/page-1.png`
- `docs/templates/render-warranty/page-1.png`
- `docs/templates/render-repair/Bien-ban-tiep-nhan-sua-chua.pdf`
- `docs/templates/render-warranty/Phieu-tiep-nhan-bao-hanh.pdf`
- `docs/load-test-store-operations-output.txt`
- `test-artifacts/screenshots/operational-imports-stock.png`
- `test-artifacts/screenshots/business-operations.png`
- `test-artifacts/screenshots/advanced-operations.png`

## DOCX render QA

Đã cài LibreOffice bằng `winget`, chuyển hai DOCX sang PDF và raster thành PNG. Hai trang render đã được kiểm tra trực quan: không clipping, không tràn bảng, không lỗi dấu tiếng Việt, không còn dòng chấm bị wrap thành ký tự lẻ.
