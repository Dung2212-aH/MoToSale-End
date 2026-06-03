# V2 Store Operations Completion Plan

## 1. Mục tiêu

Hoàn thiện MoToSale V2 thành hệ thống quản lý vận hành thực tế cho cửa hàng bán xe máy và phụ tùng:

- Quản lý bán hàng xe máy, phụ tùng.
- Quản lý mua hàng và nhà cung cấp.
- Quản lý kho theo chứng từ và lịch sử biến động.
- Quản lý thu, chi, cọc, công nợ cơ bản.
- Quản lý nhân viên và ca làm việc.
- Quản lý bảo hành, bảo dưỡng, sửa chữa.
- Quản lý chăm sóc khách hàng sau bán.
- Báo cáo trực quan và kết xuất dữ liệu.
- Giữ giao diện admin đơn giản, dễ dùng, không yêu cầu người dùng nhập ID kỹ thuật.

Phạm vi không phải là ERP kế toán đầy đủ. Không triển khai sổ cái, thuế, bảng lương phức tạp hoặc tích hợp ngân hàng nếu chưa có yêu cầu riêng.

## 2. Quy tắc bắt buộc

- Tuyệt đối không tự ý dừng khi chưa hoàn thành toàn bộ plan trong phiên triển khai.
- Mỗi task phải có trạng thái: `Pending`, `In Progress`, `Done`, hoặc `Blocked`.
- Nếu gặp lỗi: ghi nhận lỗi, xác định nguyên nhân, sửa, test lại và tiếp tục.
- Schema database, entity, DTO và API mới phải dùng tiếng Anh. Giao diện hiển thị cho người dùng dùng tiếng Việt có dấu.
- Không để người dùng nhập ID kỹ thuật trong form nghiệp vụ. Phải dùng select có tìm kiếm, autocomplete hoặc chọn từ dữ liệu liên quan.
- Không xóa hoặc làm hỏng luồng nghiệp vụ đang hoạt động: sản phẩm, danh mục, hãng xe, dòng xe, kho, voucher, đơn hàng, bài viết, FAQ, liên hệ, đánh giá, báo cáo hiện có.
- Mọi mutation test phải dùng dữ liệu có tiền tố `E2E-` và cleanup tự động sau khi test.
- Sau mỗi phase phải chạy:
  - `dotnet test v2/backend/MoToSale.slnx --no-restore`
  - `npm run build` tại `v2/frontend-admin`
  - Test UI thật trên trình duyệt.
- Khi có migration mới: phải apply migration trên SQL Server `MoToSaleV2`, xác minh schema và test dữ liệu thực.
- Không chuyển sang phase tiếp theo nếu còn lỗi nghiêm trọng về dữ liệu, phân quyền hoặc nghiệp vụ của phase hiện tại.

## 3. Đánh giá hiện trạng

| Nhóm nghiệp vụ | Hiện trạng | Mức độ |
|---|---|---|
| Bán xe máy, phụ tùng | Đã có sản phẩm, biến thể, ảnh, đơn hàng, cập nhật trạng thái | Khá tốt |
| Mua hàng | Chưa có nhà cung cấp, đơn mua, nhận hàng theo đơn mua | Thiếu |
| Kho | Đã có tồn kho, phiếu kho, điều chỉnh, lịch sử | Khá tốt |
| Kế toán, công nợ | Đã có cọc, trả hàng, hoàn tiền nền tảng | Cơ bản |
| Nhân sự | Đã có nhân viên và ca làm việc nền tảng | Cơ bản |
| Bảo hành | Đã có bảo hành và lịch sử bảo hành | Khá |
| Sửa chữa, bảo dưỡng | Chưa có phiếu sửa chữa riêng và vật tư sử dụng | Thiếu |
| Chăm sóc khách hàng | Có liên hệ, chưa có lịch sử chăm sóc và nhắc việc | Thiếu |
| Báo cáo | Có doanh thu, đơn hàng, kho và XLSX một phần | Chưa đủ |
| UI vận hành nâng cao | Một số form còn nhập ID thủ công | Cần sửa sớm |
| Hiệu năng | Chưa có benchmark và load test chính thức | Chưa đánh giá |

## 4. Phân quyền mục tiêu

| Chức năng | Admin | Staff |
|---|---|---|
| Xem sản phẩm, tồn kho, khách hàng | Có | Có |
| Tạo đơn, cập nhật trạng thái giao hàng | Có | Có |
| Lập phiếu nhập, xuất, điều chỉnh kho | Có | Có theo quyền |
| Trả hàng, hoàn tiền | Có | Tạo yêu cầu, cần duyệt nếu vượt ngưỡng |
| Quản lý voucher, danh mục, hãng xe | Có | Không |
| Quản lý nhà cung cấp, đơn mua hàng | Có | Có theo quyền |
| Quản lý công nợ, thu chi | Có | Chỉ xem hoặc lập phiếu chờ duyệt |
| Quản lý nhân viên, ca làm | Có | Xem ca cá nhân |
| Bảo hành, sửa chữa, CRM | Có | Có |
| Báo cáo tổng hợp, lợi nhuận | Có | Hạn chế |
| Audit log, cấu hình hệ thống | Có | Không |

## 5. Kế hoạch triển khai

### Phase 0 - Baseline và rà soát mô hình dữ liệu

| Task | Trạng thái |
|---|---|
| Chụp baseline UI toàn bộ trang admin hiện có | Pending |
| Lập danh sách endpoint hiện có và đối chiếu với UI | Pending |
| Rà schema SQL Server `MoToSaleV2`, khóa quy tắc đặt tên tiếng Anh | Pending |
| Kiểm tra role `Admin`, `Staff` và policy hiện có | Pending |
| Chạy backend test, frontend build và lưu kết quả baseline | Pending |

### Phase 1 - Hoàn thiện UX trang vận hành nâng cao

| Task | Trạng thái |
|---|---|
| Thay toàn bộ ô nhập `OrderId`, `UserId`, `StoreId`, `StaffId`, `ProductVariantId` bằng searchable select | Pending |
| Khi tạo trả hàng: chọn đơn trước, sau đó chỉ hiển thị dòng hàng thuộc đơn | Pending |
| Hiển thị số lượng đã mua, đã trả, còn được trả và lý do trả | Pending |
| Khi hoàn tiền: chọn phiếu trả hàng hợp lệ, hiển thị số tiền tối đa có thể hoàn | Pending |
| Bổ sung filter, pagination, trạng thái và trang chi tiết cho trả hàng, hoàn tiền, ca làm | Pending |
| Bổ sung validation tiếng Việt có dấu và trạng thái loading/error/empty | Pending |
| Test toàn bộ nút, field, modal và responsive của `/advanced-operations` | Pending |

### Phase 2 - Nhà cung cấp và mua hàng

#### Backend và database

| Task | Trạng thái |
|---|---|
| Tạo `Suppliers` với mã NCC, tên, liên hệ, MST, địa chỉ, trạng thái | Pending |
| Tạo `PurchaseOrders`, `PurchaseOrderLines` | Pending |
| Chuẩn hóa trạng thái: `Draft`, `Approved`, `PartiallyReceived`, `Received`, `Cancelled` | Pending |
| Tạo `GoodsReceipts`, `GoodsReceiptLines` liên kết đơn mua | Pending |
| Hỗ trợ nhận hàng nhiều lần và không nhận vượt số lượng đặt | Pending |
| Khi nhận hàng: tạo stock movement và cập nhật tồn kho trong transaction | Pending |
| Lưu snapshot giá nhập để phục vụ báo cáo lợi nhuận | Pending |
| Tạo API CRUD nhà cung cấp, đơn mua, duyệt đơn, hủy đơn, nhận hàng | Pending |

#### Frontend admin

| Task | Trạng thái |
|---|---|
| Tạo trang Nhà cung cấp | Pending |
| Tạo trang Đơn mua hàng và chi tiết đơn mua | Pending |
| Tạo form chọn NCC, biến thể, số lượng, giá nhập, ghi chú | Pending |
| Tạo UI nhận hàng từng phần, hiển thị số đã nhận và còn thiếu | Pending |
| Thêm filter theo NCC, trạng thái, khoảng ngày | Pending |
| Test CRUD, duyệt, hủy, nhận hàng từng phần và cập nhật tồn kho | Pending |

### Phase 3 - Thu chi, cọc và công nợ

| Task | Trạng thái |
|---|---|
| Tạo `CashTransactions` và nhóm phiếu thu, phiếu chi | Pending |
| Liên kết thu chi với đơn bán, đơn mua, cọc, trả hàng, hoàn tiền | Pending |
| Tạo sổ công nợ khách hàng và nhà cung cấp | Pending |
| Hỗ trợ thanh toán nhiều lần, số đã trả, số còn nợ | Pending |
| Hiển thị lịch sử giao dịch theo đối tượng và theo khoảng ngày | Pending |
| Tạo trang Thu chi và trang Công nợ | Pending |
| Bổ sung quy trình duyệt hoàn tiền hoặc chi tiền vượt ngưỡng | Pending |
| Test đối soát: tổng tiền, cọc, đã thu, hoàn tiền, còn nợ | Pending |

### Phase 4 - Bảo hành, bảo dưỡng và sửa chữa

| Task | Trạng thái |
|---|---|
| Tạo `RepairOrders` và trạng thái tiếp nhận, kiểm tra, báo giá, đang sửa, hoàn thành, bàn giao, hủy | Pending |
| Tạo hạng mục công việc, công thợ và phụ tùng sử dụng | Pending |
| Gán kỹ thuật viên và lưu lịch sử xử lý | Pending |
| Liên kết phiếu sửa chữa với bảo hành khi thuộc diện bảo hành | Pending |
| Xuất kho phụ tùng khi sử dụng và hoàn kho khi hủy phù hợp | Pending |
| Tạo lịch hẹn bảo dưỡng và nhắc bảo dưỡng định kỳ | Pending |
| Tạo trang Sửa chữa, lịch hẹn và chi tiết luồng xử lý | Pending |
| Test phiếu sửa chữa có phí, bảo hành miễn phí và sử dụng phụ tùng | Pending |

### Phase 5 - Chăm sóc khách hàng

| Task | Trạng thái |
|---|---|
| Tạo `CustomerInteractions`: gọi điện, email, ghi chú, khiếu nại, nhắc việc | Pending |
| Tạo assignment nhân viên phụ trách và lịch hẹn follow-up | Pending |
| Hiển thị timeline khách hàng: mua hàng, bảo hành, sửa chữa, liên hệ | Pending |
| Tạo nhắc chăm sóc sau bán và nhắc bảo dưỡng | Pending |
| Bổ sung filter trạng thái, nhân viên, ngày cần xử lý | Pending |
| Test tạo, cập nhật, hoàn thành và quá hạn nhắc việc | Pending |

### Phase 6 - Nhân sự và ca làm

| Task | Trạng thái |
|---|---|
| Hoàn thiện hồ sơ nhân viên và trạng thái làm việc | Pending |
| Tạo lịch ca, check-in, check-out và ghi chú ca | Pending |
| Cho Staff xem ca cá nhân, Admin quản lý toàn bộ ca | Pending |
| Tạo thống kê số ca và hoạt động xử lý theo nhân viên | Pending |
| Không triển khai bảng lương phức tạp trong phạm vi này | Pending |
| Test role matrix Admin và Staff | Pending |

### Phase 7 - Báo cáo và dashboard

| Task | Trạng thái |
|---|---|
| Chuẩn hóa bộ lọc khoảng ngày, chi nhánh, trạng thái | Pending |
| Báo cáo bán hàng: doanh thu, đơn hàng, số lượng xe, phụ tùng | Pending |
| Báo cáo mua hàng theo NCC, đơn mua, giá trị nhập | Pending |
| Báo cáo tồn kho: nhập, xuất, điều chỉnh, tồn thấp, hàng chậm luân chuyển | Pending |
| Báo cáo lợi nhuận gộp dựa trên snapshot giá nhập | Pending |
| Báo cáo cọc, công nợ, hoàn tiền, thu chi | Pending |
| Báo cáo sửa chữa, bảo hành, vật tư sử dụng | Pending |
| Báo cáo hiệu quả nhân viên và chăm sóc khách hàng | Pending |
| Dashboard chỉ dùng dữ liệu thật từ API, không placeholder | Pending |
| Test tổng từng bảng chi tiết khớp với statistic card và DB | Pending |

### Phase 8 - Import và export linh hoạt

| Task | Trạng thái |
|---|---|
| Xuất XLSX thật cho sản phẩm, kho, đơn hàng, mua hàng, công nợ, thu chi, sửa chữa, báo cáo | Pending |
| Xuất PDF in được cho đơn bán, đơn mua, phiếu nhập, phiếu thu, phiếu chi, trả hàng, sửa chữa, bảo hành | Pending |
| Chỉ dùng DOCX cho biểu mẫu cần chỉnh sửa thủ công như biên bản bảo hành hoặc sửa chữa | Pending |
| Import XLSX sản phẩm, NCC và tồn đầu kỳ | Pending |
| Import phải có preview, validate từng dòng, báo lỗi và chỉ commit khi xác nhận | Pending |
| Kiểm tra Unicode tiếng Việt trong XLSX, CSV, PDF, DOCX | Pending |

### Phase 9 - Bảo mật và hiệu năng

| Task | Trạng thái |
|---|---|
| Rà toàn bộ controller, bảo đảm API admin không public ngoài ý muốn | Pending |
| Áp dụng policy Admin/Staff theo role matrix | Pending |
| Bổ sung audit log cho mutation quan trọng | Pending |
| Kiểm tra query projection, pagination, index và N+1 query | Pending |
| Benchmark danh sách kho 10.000 SKU và đơn hàng 50.000 bản ghi | Pending |
| Benchmark báo cáo theo khoảng ngày lớn | Pending |
| Mục tiêu: thao tác danh sách phổ biến p95 dưới 1 giây trong môi trường nội bộ | Pending |
| Mục tiêu: báo cáo tổng hợp phổ biến p95 dưới 3 giây | Pending |

### Phase 10 - Full regression và nghiệm thu

| Task | Trạng thái |
|---|---|
| Chạy toàn bộ backend unit, service, controller và schema test | Pending |
| Viết integration test trên SQL Server cho luồng mua, bán, kho, hoàn tiền, sửa chữa | Pending |
| Chạy mutation E2E sâu với dữ liệu `E2E-` và cleanup tự động | Pending |
| Test UI thật: toàn bộ trang, nút, field, modal, filter, pagination | Pending |
| Chụp screenshot desktop, tablet, mobile và kiểm tra cột bảng | Pending |
| Kiểm tra reload, chuyển trang rồi quay lại, sidebar, footer, responsive | Pending |
| Đối soát DB sau test: không còn dữ liệu rác, không sai tồn kho, không lệch công nợ | Pending |
| Chạy production build FE và backend test lần cuối | Pending |
| Chỉ kết thúc khi toàn bộ checklist là `Done` | Pending |

## 6. Luồng nghiệp vụ cần test E2E

### Luồng mua hàng

1. Tạo NCC.
2. Tạo đơn mua có nhiều biến thể.
3. Duyệt đơn mua.
4. Nhận một phần hàng.
5. Kiểm tra tồn kho và lịch sử kho.
6. Nhận phần còn lại.
7. Kiểm tra trạng thái đơn mua và công nợ NCC.

### Luồng bán hàng và hoàn tiền

1. Tạo đơn bán có xe máy và phụ tùng.
2. Thu cọc.
3. Xác nhận và giao hàng.
4. Kiểm tra doanh thu, lịch sử đơn và tồn kho.
5. Trả một dòng hàng hợp lệ.
6. Duyệt hoàn tiền.
7. Kiểm tra tồn kho, công nợ, thu chi và báo cáo.

### Luồng sửa chữa

1. Tiếp nhận xe.
2. Chọn khách hàng, xe, kỹ thuật viên.
3. Thêm công việc và phụ tùng.
4. Duyệt báo giá.
5. Xuất kho phụ tùng.
6. Hoàn thành và bàn giao.
7. Kiểm tra lịch sử khách hàng, kho và doanh thu dịch vụ.

### Luồng chăm sóc khách hàng

1. Tạo nhắc bảo dưỡng sau bán.
2. Gán Staff phụ trách.
3. Ghi nhận liên hệ.
4. Hoàn thành hoặc hẹn lại.
5. Kiểm tra timeline khách hàng và báo cáo quá hạn.

## 7. Tiêu chí nghiệm thu

- Không còn form nghiệp vụ yêu cầu nhập ID kỹ thuật thủ công.
- Nhà cung cấp, đơn mua, nhận hàng, tồn kho và công nợ liên kết đúng.
- Trả hàng, hoàn tiền, thu chi và tồn kho đối soát được bằng lịch sử.
- Bảo hành và sửa chữa có timeline rõ ràng, có kỹ thuật viên và vật tư sử dụng.
- Admin và Staff chỉ nhìn thấy và thao tác đúng phạm vi quyền.
- Dashboard và báo cáo dùng dữ liệu thật, tổng khớp bảng chi tiết và DB.
- Có XLSX cho dữ liệu quản trị chính, PDF cho chứng từ in, DOCX chỉ cho biểu mẫu phù hợp.
- Import XLSX có preview và validation.
- UI không lỗi bố cục, không tràn nội dung, không lỗi dấu tiếng Việt.
- Backend test, integration test, mutation E2E và frontend build đều pass.
- Database `MoToSaleV2` giữ schema tiếng Anh và không còn dữ liệu test rác sau cleanup.

## 8. Thứ tự ưu tiên triển khai

1. Phase 0: khóa baseline.
2. Phase 1: sửa UX nhập ID để trang vận hành nâng cao dùng được ngay.
3. Phase 2: nhà cung cấp và mua hàng, vì đây là đầu vào của kho và giá vốn.
4. Phase 3: thu chi và công nợ.
5. Phase 4 và 5: sửa chữa, bảo hành, CRM.
6. Phase 6: nhân sự và ca làm.
7. Phase 7 và 8: báo cáo, import, export.
8. Phase 9 và 10: bảo mật, hiệu năng, regression, nghiệm thu.

## 9. Deliverables

- Migration SQL Server và schema documentation.
- Backend entity, service, controller, policy và test chính thức.
- Frontend admin pages, searchable selects, validation và responsive UI.
- Bộ test E2E có cleanup tự động.
- Báo cáo đối soát DB, benchmark và kết quả nghiệm thu.
- Hướng dẫn vận hành ngắn cho Admin và Staff.

## 10. Tiến độ triển khai ngày 02/06/2026

### Đã hoàn thành

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Baseline backend test và frontend build | Done | Backend pass `17/17`, frontend production build pass |
| Schema SQL Server cho vận hành cửa hàng | Done | Đã thêm 10 bảng tiếng Anh và apply migration `CompleteStoreOperations` |
| UX trả hàng và phân ca | Done | Không còn nhập ID kỹ thuật, dùng selector từ dữ liệu thật |
| Nhà cung cấp | Done | Tạo, sửa, danh sách, export XLSX, import XLSX có báo lỗi từng dòng |
| Đơn mua hàng | Done | Tạo nhiều SKU, duyệt, hủy, nhận hàng từng phần |
| Tích hợp tồn kho khi nhận hàng | Done | Cập nhật `InventoryItems` và append `StockMovements` trong transaction |
| Thu chi cơ bản | Done | Tạo và xem phiếu thu, phiếu chi |
| Sửa chữa cơ bản | Done | Tiếp nhận, danh sách, chi phí và cập nhật trạng thái bàn giao |
| CRM cơ bản | Done | Tạo lịch chăm sóc, gán nhân viên, nhắc việc và hoàn thành |
| Chấm công cơ bản | Done | Check-in, check-out và lịch sử |
| Dashboard vận hành | Done | Có chỉ số NCC, đơn mua, giá trị mua, thu ròng, sửa chữa, CRM |
| Export XLSX vận hành | Done | Có cho sáu tab mới |
| Security smoke test | Done | Anonymous nhận `401`, Staff hợp lệ nhận `200` |
| API response smoke benchmark | Done | Endpoint mới phản hồi khoảng `6-22 ms` với dữ liệu hiện tại |
| Mutation E2E và cleanup | Done | Đã chạy luồng NCC -> mua -> nhận kho -> thu chi -> sửa chữa -> CRM -> chấm công và dọn dữ liệu `E2E-` |

### Còn phải triển khai trước nghiệm thu cửa hàng

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| PDF chứng từ in chuẩn mẫu | Pending | Cần mẫu in cho đơn mua, nhận hàng, thu chi, sửa chữa |
| DOCX biên bản bảo hành/sửa chữa | Pending | Chỉ triển khai cho biểu mẫu cần chỉnh sửa thủ công |
| Import XLSX sản phẩm và tồn đầu kỳ | Pending | Import NCC đã hoàn tất |
| Sổ công nợ NCC có ghi nhận thanh toán nhiều lần | Pending | Đã có tổng phải trả ở đơn mua, chưa có ledger thanh toán NCC riêng |
| Xuất kho phụ tùng theo phiếu sửa chữa | Pending | Đã có dòng phụ tùng trong schema, chưa post ledger kho |
| Timeline sửa chữa chi tiết | Pending | Cần lịch sử từng lần đổi trạng thái |
| Load test 10.000 SKU và 50.000 đơn | Pending | Hiện mới smoke benchmark dữ liệu hiện tại |
| UI regression desktop/tablet/mobile toàn hệ thống | Pending | Đã smoke test DOM route mới, chưa chạy lại toàn bộ trang cũ |

### Cập nhật bổ sung sau migration `SupplierPaymentsAndRepairTimeline`

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Backend test chính thức | Done | Đã tăng lên `19/19` test pass |
| Thanh toán công nợ NCC nhiều lần | Done | Thanh toán tạo phiếu chi liên kết đơn mua và cập nhật số còn phải trả |
| Timeline sửa chữa | Done | Đã thêm `RepairStatusHistories`, ghi lịch sử lúc tiếp nhận và khi đổi trạng thái |
| Xuất kho phụ tùng sửa chữa | Done | Khi chuyển sang `Repairing`, phụ tùng có SKU được xuất kho đúng một lần và ghi `StockMovements` |

## 11. Nghiệm thu cuối ngày 02/06/2026

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Import XLSX sản phẩm và tồn đầu kỳ | Done | Đã thêm `/operational-imports`, preview, validation và xác nhận trước khi ghi DB |
| PDF chứng từ | Done | Dùng bố cục in độc lập và hộp thoại in trình duyệt để lưu PDF |
| DOCX sửa chữa và bảo hành | Done | Đã tạo hai mẫu trong `docs/templates`; render PNG/PDF và visual QA pass |
| Load test 10.000 SKU và 50.000 đơn | Done | Tổng thời gian `3.826 s`, aggregate lớn nhất `184 ms`, rollback sạch |
| UI regression desktop/tablet/mobile | Done | Playwright pass `24/24`; route regression đủ `23` route trên ba viewport |
| Backend regression | Done | `dotnet test` pass `19/19` |
| DB cleanup | Done | Không còn dữ liệu `E2E-` hoặc `LOAD-` |

Báo cáo chi tiết: `docs/V2_STORE_OPERATIONS_FINAL_ACCEPTANCE_20260602.md`.
