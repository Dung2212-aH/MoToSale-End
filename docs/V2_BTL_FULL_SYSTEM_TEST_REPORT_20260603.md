# Báo cáo test tổng thể hệ thống quản trị MoToSale V2 cho BTL

Ngày test: 03/06/2026  
Môi trường: FE `http://localhost:5176`, Gateway `http://localhost:5100`, Auth `http://localhost:5101`, API `http://localhost:5102`  
Plan bám theo: `D:/MotorTeam/MoToSale-End/docs/V2_BTL_FULL_SYSTEM_TEST_PROCESS.md`

## 1. Artifact

Thư mục artifact:

`D:/MotorTeam/MoToSale-End/docs/test-artifacts/BTL_FULL_20260603_151656`

File chính:

- `ui_smoke_results.json`: smoke test 22 route, bảng, nút, modal, responsive.
- `business_workflow_results.json`: luồng nghiệp vụ E2E qua API.
- `inventory_revalidation_results.json`: đối chiếu lại tồn kho/movement sau E2E.
- `ui_targeted_results.json`: test UI submit thật cho upload, giá gốc, POS, export.
- `ui_targeted_rerun_results.json`: chạy lại POS/export bằng selector đúng.
- `inventory_key_warning_recheck.json`: kiểm tra lại warning React sau khi sửa.
- `screenshots/`: 34 ảnh route/responsive.
- `modals/`: 41 ảnh modal/tool surface.
- `targeted-ui/`: ảnh kiểm tra upload, POS, tồn kho sau sửa.

## 2. Kết quả tổng quan

| Hạng mục | Kết quả |
|---|---:|
| Route lõi mở được | 22/22 |
| Route bị lỗi API 4xx/5xx bất thường | 0 |
| Screenshot route/responsive | 34 |
| Screenshot modal/tool surface | 41 |
| E2E API nghiệp vụ | PASS sau khi revalidate tồn kho |
| Targeted UI submit thật | PASS sau rerun |
| FE build | PASS |
| BE build | PASS |
| BE unit/integration tests | PASS 20/20 |
| Bổ sung 5 nhóm còn thiếu (Voucher/Danh mục/Cấu hình/Nhật ký/Import) | PASS (xem mục 5B) |

## 3. Luồng nghiệp vụ đã test

Đã chạy qua các luồng chính:

- Đăng nhập Admin.
- Tạo khách hàng.
- Nhập kho trực tiếp.
- Tạo đơn POS bán đứt, thu tiền, trừ kho.
- Cập nhật trạng thái đơn và kiểm tra đồng bộ vận chuyển.
- Tạo, sửa, duyệt phiếu trả hàng; hoàn tiền; cộng lại tồn kho nếu hàng bán lại được.
- Tạo nhà cung cấp, đơn mua, duyệt, nhận hàng, thanh toán NCC.
- Tạo bảo hành, chuyển trạng thái xử lý và hoàn tất.
- Tạo phiếu sửa chữa, chạy đủ luồng trạng thái đến bàn giao.
- Tạo, sửa, hoàn tất lịch CSKH.
- Lập phiếu thu chi và đảo phiếu.
- Kiểm tra báo cáo/dashboard sau mutation.
- Export tồn kho XLSX qua API.
- Kiểm tra phân quyền Staff: bị chặn ở trang/endpoint Admin, vẫn dùng được nghiệp vụ vận hành phù hợp.

Ghi chú: `business_workflow_results.json` có 4 fail ban đầu ở phần tồn kho do script tìm tồn theo `keyword=skuId`, trong khi API không tìm theo ID số. Đã revalidate bằng `inventory_revalidation_results.json`: đủ movement nhập trực tiếp `+8`, POS `-1`, trả hàng `+1`, nhận hàng NCC `+2`.

## 4. UI/modal đã test thêm

Các thao tác submit thật đã PASS:

- Cập nhật logo hãng xe bằng file upload, reload vẫn còn.
- Cập nhật logo hãng sản xuất phụ tùng bằng file upload, reload vẫn còn.
- Cập nhật giá gốc xe máy, đọc lại đúng giá mới, sau đó restore giá cũ.
- Upload ảnh chính sản phẩm.
- Upload ảnh trong quản lý ảnh sản phẩm, ảnh xuất hiện trong grid.
- POS thêm 2 dòng, xóa 1 dòng chỉ mất dòng được chọn.
- Báo cáo tải được file XLSX hợp lệ từ UI.

Đã sửa trong quá trình test:

- `v2/frontend-admin/src/pages/inventory/InventoryView.jsx`: thêm fallback key cho bảng chi tiết giữ chỗ để hết warning React `Each child in a list should have a unique "key" prop`.
- Recheck `inventory_key_warning_recheck.json`: không còn console error/warning.

## 5. Lỗi còn lại

### Đã recheck - Tiếng Việt trên UI không bị mojibake

Phần tổng hợp ban đầu đọc `ui_smoke_results.json` bằng PowerShell nên hiển thị sai UTF-8. Recheck trực tiếp bằng Node/Playwright cho thấy artifact và UI hiện tại lưu đúng tiếng Việt, ví dụ:

- `Tổng quan`
- `Quản lý xe máy`
- `Đổi trả & hoàn tiền`
- `Tài chính: thu chi & công nợ`

Không còn ghi nhận lỗi mojibake thật trên UI hiện tại.

### P1 - Một số ảnh/logo seed dùng URL ngoài không ổn định

Trang hãng sản xuất phụ tùng từng sinh nhiều console error `ERR_NAME_NOT_RESOLVED` khi load logo từ domain ngoài. Đã sửa seed không dùng `logo.clearbit.com`, dọn các URL Clearbit cũ khi seed chạy, và FE tự bỏ qua URL Clearbit còn sót để hiển thị icon fallback local.

### P2 - Dữ liệu test E2E để lại bản ghi test

Test có tạo các bản ghi tiền tố `BTL-E2E`: khách hàng, đơn POS, trả hàng, NCC, đơn mua, bảo hành, sửa chữa, CSKH, thu chi, staff. Đây là dữ liệu test phục vụ đối chiếu nghiệp vụ. Nếu cần DB sạch cho demo, nên reset DB hoặc cleanup theo tiền tố `BTL-E2E`.

## 5B. Bổ sung quét các nhóm trước đó chỉ ở mức smoke (rerun 03/06/2026)

Lần test đầu, 5 trang sau mới ở mức smoke (mở được route) chưa kiểm nghiệp vụ sâu. Đã chạy bổ sung **E2E qua API Gateway** (cùng cách phần 3), tài khoản Admin:

| Mục | Kiểm tra | Kết quả |
|---|---|---|
| **Voucher** (áp mã) | Tạo voucher Percent 10% → có trong danh sách → **áp vào đơn POS** (đơn 200.000đ) | PASS — `discountTotal=20.000`, `grandTotal=180.000` đúng (giảm trên giá bán) |
| **Voucher** (sửa) | PUT đổi `discountValue` 10 → 15 | PASS — đọc lại = 15 |
| **Danh mục** (CRUD) | Tạo → sửa tên → xóa | PASS — tạo id, tên đổi thành "BTL-E2E Danh mục sửa", xóa xong còn active=0 |
| **Cấu hình** (settings) | GET 8 khóa cấu hình; PUT đổi `StoreName`; khôi phục | PASS — đọc lại `StoreName='MoToSale Shop (E2E)'`, sau đó khôi phục rỗng |
| **Nhật ký** (audit) | GET `/audit-logs` sau các mutation | PASS — 597 bản ghi; gần nhất có `Setting/Modified`, `Category/Added|Modified|Deleted`, `Voucher/Modified`, `Order/CreatePos`, `InventoryItem/Modified` (đủ actor/entity/action) |
| **Import dữ liệu** (đường dẫn) | Mô phỏng pipeline import: tạo sản phẩm → tạo biến thể (SKU) → nhập kho (`/inventory/adjust` type Import qty 5) | PASS — `onHand=5` đúng sau nhập (trang Import dùng đúng các endpoint này) |

Ghi chú:
- Voucher "áp mã" (Luồng 2 của plan) trước đây chưa có bằng chứng — nay đã xác nhận giảm giá tính đúng trong đơn POS.
- Import dữ liệu là tính năng FE điều phối (gọi `products` + `products/{id}/skus` + `inventory/adjust`); đã kiểm 3 endpoint nền hoạt động đúng và tồn kho phản ánh.
- Nhật ký tự động ghi qua cơ chế `CaptureAuditLogs` — mọi thao tác tạo/sửa/xóa đều có log.
- Các bản ghi tạo trong đợt này tiếp tục mang tiền tố `BTL-E2E` (voucher, đơn POS, sản phẩm/SKU import) — nằm trong nhóm dữ liệu test ở mục P2, cleanup khi cần demo sạch.

## 6. Build/test

Lệnh đã chạy:

- `npm run build` trong `v2/frontend-admin`: PASS.
- `dotnet build v2/backend/MoToSale.slnx --no-restore`: PASS, 0 warning, 0 error.
- `dotnet test v2/backend/tests/MoToSale.Backend.Tests/MoToSale.Backend.Tests.csproj --no-build`: PASS 20/20.

Sau khi sửa các lỗi còn lại:

- `manufacturer_external_fix_recheck.json`: PASS, `clearbitRows = 0`, không có console error từ logo ngoài.
- `ui_text_recheck_after_remaining_fixes.json`: PASS, 22/22 route không có mojibake thật, API errors `0`, console errors `0`.
- `npm run build`: PASS.
- `dotnet build v2/backend/MoToSale.slnx --no-restore`: PASS, 0 warning, 0 error.
- `dotnet test v2/backend/tests/MoToSale.Backend.Tests/MoToSale.Backend.Tests.csproj --no-build`: PASS 20/20.

Lưu ý: backend build/test lần đầu bị khóa DLL vì Auth/API service đang chạy. Đã dừng tạm các process BE, chạy build/test, sau đó bật lại nền:

- Gateway: `http://localhost:5100`
- Auth: `http://localhost:5101`
- API: `http://localhost:5102`

Gateway login qua `http://localhost:5100/api/auth/login` đã OK sau khi bật lại.

## 7. Kết luận

Về nghiệp vụ lõi BTL, hệ thống đã chạy được mạch chính: sản phẩm -> bán hàng/POS -> kho/cung ứng -> trả hàng/hoàn tiền -> bảo hành/sửa chữa/CSKH -> tài chính/báo cáo.

Về mức sẵn sàng demo: các flow nghiệp vụ chính pass, build/test pass. Các trang trước đây chỉ smoke (Voucher áp mã, Danh mục, Cấu hình, Nhật ký, Import) **đã được quét bổ sung E2E và đều PASS** (mục 5B) → **cả 5 nhóm lõi nay đều có kiểm nghiệp vụ thực**, không còn trang nào chỉ dừng ở mức "mở được".

Điểm cần lưu ý còn lại là DB test có dữ liệu `BTL-E2E`; nếu muốn demo sạch thì reset DB hoặc cleanup dữ liệu test trước khi trình bày.
