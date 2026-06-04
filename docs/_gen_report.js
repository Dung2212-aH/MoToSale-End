const fs = require('fs');
const docx = require('C:/Users/DONGTONG/AppData/Roaming/npm/node_modules/docx');
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  AlignmentType, LevelFormat, HeadingLevel, BorderStyle, WidthType, ShadingType,
  VerticalAlign, PageNumber, PageBreak, Footer, TableOfContents
} = docx;

const CW = 9026; // A4 content width (11906 - 2*1440)

// ---------- helpers ----------
const h1 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(t)] });
const h2 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(t)] });
const h3 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_3, children: [new TextRun(t)] });
const p = (t) => new Paragraph({ spacing: { after: 120 }, alignment: AlignmentType.JUSTIFIED, children: [new TextRun(t)] });
const pb = (runs) => new Paragraph({ spacing: { after: 120 }, alignment: AlignmentType.JUSTIFIED, children: runs });
const li = (t) => new Paragraph({ numbering: { reference: 'b', level: 0 }, spacing: { after: 60 }, children: [new TextRun(t)] });
const liBold = (label, rest) => new Paragraph({ numbering: { reference: 'b', level: 0 }, spacing: { after: 60 }, children: [new TextRun({ text: label, bold: true }), new TextRun(rest)] });
const pageBreak = () => new Paragraph({ children: [new PageBreak()] });

const border = { style: BorderStyle.SINGLE, size: 1, color: 'AAAAAA' };
const borders = { top: border, bottom: border, left: border, right: border };
function cell(text, w, { head = false, bold = false } = {}) {
  return new TableCell({
    borders, width: { size: w, type: WidthType.DXA },
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    shading: head ? { fill: 'D9E2F3', type: ShadingType.CLEAR } : undefined,
    verticalAlign: VerticalAlign.CENTER,
    children: [new Paragraph({ children: [new TextRun({ text: text, bold: head || bold })] })],
  });
}
function table(widths, rows) {
  return new Table({
    width: { size: widths.reduce((a, b) => a + b, 0), type: WidthType.DXA },
    columnWidths: widths,
    rows: rows.map((r, i) =>
      new TableRow({ tableHeader: i === 0, children: r.map((c, j) => cell(c, widths[j], { head: i === 0 })) })),
  });
}

// ---------- cover ----------
const center = (text, opts = {}) => new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: opts.after ?? 120 }, children: [new TextRun({ text, bold: opts.bold, size: opts.size, allCaps: opts.caps, italics: opts.italics })] });

const cover = [
  center('HỌC VIỆN KỸ THUẬT QUÂN SỰ', { bold: true, size: 28, after: 60 }),
  center('KHOA CÔNG NGHỆ THÔNG TIN', { bold: true, size: 24, after: 600 }),
  center('BÁO CÁO ĐỒ ÁN MÔN HỌC', { bold: true, size: 30, after: 60 }),
  center('Môn: Công nghệ Web', { size: 26, after: 800 }),
  center('ĐỀ TÀI', { bold: true, size: 26, after: 60 }),
  center('XÂY DỰNG WEBSITE BÁN XE MÁY & PHỤ TÙNG', { bold: true, size: 30, after: 40 }),
  center('(Hệ thống quản trị MoToSale v2)', { italics: true, size: 26, after: 1000 }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 80 }, children: [new TextRun({ text: 'Giảng viên hướng dẫn: ', bold: true }), new TextRun('Trần Văn An')] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 80 }, children: [new TextRun({ text: 'Sinh viên thực hiện: ', bold: true }), new TextRun('Tống Văn Đông')] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 800 }, children: [new TextRun({ text: 'MSSV: ', bold: true }), new TextRun('[MSSV]')] }),
  center('Năm học 2025 – 2026', { bold: true, size: 24 }),
  pageBreak(),
];

// ---------- TOC ----------
const toc = [
  new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun('MỤC LỤC')] }),
  new TableOfContents('Mục lục', { hyperlink: true, headingStyleRange: '1-3' }),
  pageBreak(),
];

// ---------- body ----------
const body = [];
const A = (...x) => body.push(...x);

// LỜI MỞ ĐẦU
A(h1('LỜI MỞ ĐẦU'));
A(p('Thương mại điện tử và số hóa quản lý bán hàng đang trở thành nhu cầu thiết yếu của các cửa hàng bán lẻ, trong đó có lĩnh vực kinh doanh xe máy và phụ tùng. Việc quản lý thủ công bằng sổ sách hoặc bảng tính dễ dẫn đến sai lệch tồn kho, thất thoát doanh thu, khó tra cứu công nợ và lịch sử giao dịch.'));
A(p('Đồ án "Xây dựng website bán xe máy & phụ tùng – Hệ thống MoToSale v2" xây dựng một hệ thống quản trị (admin) hoàn chỉnh cho cửa hàng xe máy, bao quát toàn bộ chuỗi nghiệp vụ: mua hàng – nhập kho – bán hàng (online & tại quầy) – hậu mãi – tài chính – báo cáo, với dữ liệu tiền, tồn kho và công nợ được đồng bộ nhất quán. Báo cáo trình bày phân tích yêu cầu, thiết kế, cài đặt, kiểm thử và kết quả đạt được của hệ thống.'));
A(pageBreak());

// CHƯƠNG 1
A(h1('CHƯƠNG 1. TỔNG QUAN ĐỀ TÀI'));
A(h2('1.1. Lý do chọn đề tài'));
A(p('Cửa hàng xe máy có nghiệp vụ đặc thù: sản phẩm giá trị cao (xe), nhiều biến thể (màu/phiên bản), kèm phụ tùng đa dạng; phát sinh đặt cọc, trả góp, bảo hành, sửa chữa và đổi trả. Một phần mềm quản trị tốt phải gắn chặt bán hàng với tồn kho và dòng tiền theo thời gian thực. Đây là bài toán giàu nghiệp vụ, phù hợp để vận dụng kiến thức Công nghệ Web (kiến trúc web nhiều tầng, API, CSDL, bảo mật, giao diện).'));
A(h2('1.2. Mục tiêu'));
A(liBold('Quản lý sản phẩm & kho: ', 'danh mục, sản phẩm/biến thể, tồn kho một cửa hàng theo sổ cái.'));
A(liBold('Bán hàng đa kênh: ', 'đơn online và bán tại quầy (POS) với bán đứt, đặt cọc, voucher, hóa đơn GTGT.'));
A(liBold('Hậu mãi: ', 'đổi trả – hoàn tiền, bảo hành, sửa chữa, chăm sóc khách hàng.'));
A(liBold('Tài chính & báo cáo: ', 'sổ quỹ thu/chi, công nợ, doanh thu, lãi gộp/giá vốn.'));
A(liBold('Hệ thống: ', 'phân quyền Admin/Nhân viên, nhật ký kiểm toán, cấu hình.'));
A(h2('1.3. Phạm vi'));
A(p('Trong phạm vi: toàn bộ khu quản trị (admin) nêu trên cho mô hình một cửa hàng/một kho. Ngoài phạm vi: cổng thanh toán trực tuyến, hóa đơn điện tử hợp pháp (mã cơ quan thuế), tích hợp đơn vị vận chuyển, đa chi nhánh/đa kho.'));
A(h2('1.4. Công nghệ sử dụng'));
A(table([2600, 6426], [
  ['Thành phần', 'Công nghệ'],
  ['Backend', '.NET 8, ASP.NET Core, Entity Framework Core (code-first)'],
  ['CSDL', 'SQL Server (LocalDB) – cơ sở dữ liệu MoToSaleV2'],
  ['API Gateway', 'Ocelot (reverse-proxy, điểm vào duy nhất)'],
  ['Xác thực', 'JWT (JSON Web Token), băm mật khẩu PBKDF2'],
  ['Frontend', 'React 18, Vite, Tailwind/AdminLTE, Axios'],
  ['Khác', 'Swagger (đặc tả API), ExcelJS (xuất Excel), in hóa đơn qua trình duyệt'],
]));
A(pageBreak());

// CHƯƠNG 2
A(h1('CHƯƠNG 2. PHÂN TÍCH YÊU CẦU'));
A(h2('2.1. Tác nhân và phân quyền'));
A(table([2400, 3200, 3426], [
  ['Tác nhân', 'Mô tả', 'Quyền chính'],
  ['Admin (Quản trị)', 'Chủ/quản lý cửa hàng', 'Toàn quyền: cấu hình, tài khoản, tài chính, mua hàng, danh mục, báo cáo'],
  ['Nhân viên (Staff)', 'Nhân viên bán hàng/kỹ thuật', 'Bán hàng/POS, đổi trả, bảo hành, sửa chữa, CSKH, chấm công'],
  ['Khách hàng', 'Người mua (online)', 'Đặt đơn, gửi liên hệ/đánh giá (ngoài khu admin)'],
]));
A(h2('2.2. Yêu cầu chức năng'));
A(table([2600, 6426], [
  ['Nhóm chức năng', 'Mô tả'],
  ['Xác thực & phân quyền', 'Đăng nhập JWT; phân quyền theo vai trò ở cả API; quản lý tài khoản/vai trò'],
  ['Danh mục & sản phẩm', 'CRUD danh mục/hãng xe/dòng xe/hãng SX; sản phẩm, biến thể (SKU), ảnh, tương thích, bán kèm'],
  ['Bán hàng & đơn', 'POS bán đứt/đặt cọc, khách quen, voucher; đơn online; vòng đời đơn; ghi nhận thanh toán; giao hàng & xuất kho; sửa đơn; hóa đơn GTGT'],
  ['Khách hàng', 'CRUD khách, ghi chú chăm sóc, lịch sử mua; tự tạo "khách lẻ" khi bán POS'],
  ['Kho & cung ứng', 'Tồn kho (thực/giữ chỗ/khả dụng), điều chỉnh, chứng từ kho có duyệt, sổ cái bất biến; nhà cung cấp, đơn mua → nhận hàng → thanh toán'],
  ['Hậu mãi & dịch vụ', 'Đổi trả → hoàn tồn + hoàn tiền + ghi quỹ; bảo hành; sửa chữa (xuất phụ tùng); CSKH'],
  ['Tài chính', 'Sổ quỹ thu/chi tự sinh; đảo phiếu; công nợ khách'],
  ['Báo cáo & hệ thống', 'Doanh thu, lãi gộp/COGS, top sản phẩm, thu chi, công nợ, cảnh báo tồn; nhật ký kiểm toán; cấu hình; phân ca/chấm công; import'],
]));
A(h2('2.3. Yêu cầu phi chức năng'));
A(liBold('Bảo mật: ', 'xác thực JWT, phân quyền theo vai trò ở API, mật khẩu băm PBKDF2.'));
A(liBold('Toàn vẹn dữ liệu: ', 'giao dịch (transaction) cho thao tác đa bước; sổ cái kho/quỹ bất biến.'));
A(liBold('Hiệu năng & khả dụng: ', 'API phản hồi nhanh ở quy mô cửa hàng; giao diện tách route (lazy-load).'));
A(liBold('Khả dùng: ', 'giao diện tiếng Việt, tiền VNĐ, ngày giờ định dạng Việt Nam, thông báo lỗi rõ ràng.'));
A(liBold('Khả bảo trì: ', 'kiến trúc phân lớp, service tách theo domain.'));
A(h2('2.4. Quy tắc nghiệp vụ tiêu biểu'));
A(li('Một cửa hàng/một kho; tồn khả dụng = tồn thực − đang giữ chỗ.'));
A(li('Sổ cái kho và sổ quỹ ghi append-only (chỉ thêm, không sửa/xóa).'));
A(li('Doanh thu chỉ tính cho đơn đã thanh toán đủ và đã giao/hoàn tất; lãi gộp = doanh thu − giá vốn bình quân.'));
A(li('Đơn đặt cọc giữ chỗ tồn, chỉ trừ kho thật khi giao hàng; hủy đơn cọc mặc định khách mất cọc.'));
A(li('Chặn xóa đối tượng đã phát sinh giao dịch (voucher đã dùng, SKU đã có đơn/tồn, tài khoản đã có đơn, danh mục/hãng còn tham chiếu).'));
A(li('Dữ liệu giao dịch chỉ sửa khi chưa phát sinh hiệu lực (đơn sửa dòng khi Chờ thanh toán; bảo hành/sửa chữa sửa khi mới tiếp nhận).'));
A(li('Mọi thao tác thay đổi dữ liệu đều được ghi nhật ký kiểm toán.'));
A(pageBreak());

// CHƯƠNG 3
A(h1('CHƯƠNG 3. THIẾT KẾ HỆ THỐNG'));
A(h2('3.1. Kiến trúc tổng thể'));
A(p('Hệ thống theo kiến trúc microservices đặt sau một API Gateway, dùng chung một cơ sở dữ liệu; giao diện quản trị là ứng dụng SPA React. Frontend gọi Gateway (cổng 5100); Gateway định tuyến các yêu cầu xác thực/tài khoản tới AuthService (5101) và phần nghiệp vụ còn lại tới APIService (5102).'));
A(table([2600, 6426], [
  ['Thành phần', 'Trách nhiệm'],
  ['ApiGateway (Ocelot) – 5100', 'Điểm vào duy nhất; định tuyến /api/auth, /api/users → Auth; còn lại → API; chuyển tiếp JWT'],
  ['AuthService – 5101', 'Đăng nhập, phát hành/kiểm tra JWT, quản lý tài khoản & vai trò'],
  ['APIService – 5102', 'Toàn bộ nghiệp vụ: danh mục, kho, đơn/POS, thanh toán, hậu mãi, cung ứng, tài chính, báo cáo'],
  ['Frontend admin', 'Giao diện React (Vite), gọi API qua Gateway'],
]));
A(p('Backend phân lớp: Common (BaseEntity, enum, JWT, hashing) → Entities (thực thể) → DTO → Repository (AppDbContext, Repository<T>, UnitOfWork/transaction, audit) → Services (logic nghiệp vụ) → các host service (controller + DI).'));
A(h2('3.2. Thiết kế cơ sở dữ liệu'));
A(p('CSDL gồm khoảng 50 bảng, tổ chức theo các nhóm domain. Mọi thực thể kế thừa BaseEntity (Id, CreatedDate, UpdatedDate, Status) hỗ trợ xóa mềm.'));
A(table([2200, 6826], [
  ['Nhóm', 'Bảng tiêu biểu'],
  ['Identity', 'Users, Roles, UserRoles, Addresses'],
  ['Catalog', 'Categories, Brands, VehicleModels, Manufacturers, Products, Skus, ProductImages, PartCompatibilities, Reviews'],
  ['Inventory', 'InventoryItems, StockMovements, StockDocuments, StockDocumentLines, Reservations'],
  ['Ordering', 'Orders, OrderLines, Allocations, OrderStatusHistories, Vouchers, OrderVouchers, Warranties'],
  ['Payments', 'Payments'],
  ['Operations', 'SalesReturns, Refunds, Suppliers, PurchaseOrders, GoodsReceipts, CashTransactions, RepairOrders, CustomerInteractions, StaffShifts, StaffAttendances'],
  ['System', 'Settings, AuditLogs'],
]));
A(h3('Một số bảng trọng tâm'));
A(liBold('InventoryItems: ', 'tồn theo SKU (OnHand, Reserved, ReorderPoint); Available = OnHand − Reserved.'));
A(liBold('StockMovements: ', 'sổ cái kho append-only (SkuId, Type, QtyDelta, BalanceAfter, RefType/RefId).'));
A(liBold('Orders: ', 'Code, UserId, Channel, OrderType, OrderStatus, PaymentStatus, FulfillmentStatus, Subtotal/GrandTotal/DepositAmount/RemainingAmount.'));
A(liBold('Payments / CashTransactions: ', 'phiếu thu của đơn và sổ quỹ thu/chi (Receipt/Payment, Category, ReferenceType/Id).'));
A(liBold('SalesReturns / Refunds: ', 'phiếu trả (ItemCondition) và phiếu hoàn tiền liên kết đơn.'));
A(h3('Các tập trạng thái'));
A(table([2600, 6426], [
  ['Lĩnh vực', 'Giá trị'],
  ['OrderStatus', 'Pending · AwaitingPayment · Confirmed · Allocated · Shipping · Delivered · Completed · Cancelled'],
  ['PaymentStatus', 'Unpaid · DepositPaid · PartiallyPaid · Paid · Refunded'],
  ['FulfillmentStatus', 'Unallocated · Allocated · Shipped · Fulfilled'],
  ['OrderType', 'FullPayment · Deposit · Installment'],
  ['ReservationStatus', 'Active · Confirmed · Released · Expired'],
]));
A(h2('3.3. Thiết kế API'));
A(p('API theo phong cách REST/JSON qua Gateway (tiền tố /api), xác thực bằng JWT trong header Authorization. Đặc tả chi tiết tham số/response được cung cấp qua Swagger UI ở mỗi service.'));
A(table([2400, 4826, 1800], [
  ['Nhóm', 'Endpoint tiêu biểu', 'Quyền'],
  ['Xác thực', 'POST /api/auth/login', 'Công khai'],
  ['Sản phẩm', 'GET/POST/PUT /api/products; DELETE (xóa mềm)', 'Admin'],
  ['Đơn hàng', 'GET /api/orders; POST /api/orders/pos; PUT /api/orders/{id}; POST /api/orders/{id}/fulfill', 'Admin/Staff'],
  ['Thanh toán', 'POST /api/payments; POST /api/payments/{id}/cancel', 'Admin/Staff'],
  ['Kho', 'GET /api/inventory; chứng từ kho + duyệt', 'Admin'],
  ['Đổi trả', 'POST /api/operations/returns; .../approve', 'Admin/Staff'],
  ['Cung ứng', 'purchase-orders (+duyệt/nhận hàng/thanh toán)', 'Admin'],
  ['Báo cáo', 'GET /api/reports/dashboard; GET /api/reports?from&to', 'Admin'],
]));
A(h2('3.4. Thiết kế giao diện'));
A(p('Giao diện dùng bố cục AdminLTE/Bootstrap kết hợp Tailwind: header + sidebar trái + vùng nội dung; route nạp trễ (lazy-load). Một lớp adapter Axios tự dịch khóa Việt–Anh để giao diện thuần tiếng Việt. Menu được tổ chức thành 5 nhóm theo domain:'));
A(table([2800, 6226], [
  ['Nhóm menu', 'Trang chính'],
  ['Bán hàng', 'Bán tại quầy (POS), Đơn hàng, Khách hàng, Voucher'],
  ['Sản phẩm & Kho', 'Sản phẩm, Danh mục/Hãng/Dòng/Hãng SX, Tồn kho, Chứng từ kho, Cung ứng'],
  ['Dịch vụ & Hậu mãi', 'Đổi trả & hoàn tiền, Bảo hành, Sửa chữa, CSKH'],
  ['Tài chính & Báo cáo', 'Sổ quỹ, Công nợ, Báo cáo'],
  ['Hệ thống', 'Tài khoản & vai trò, Phân ca/Chấm công, Cấu hình, Nhật ký kiểm toán'],
]));
A(pageBreak());

// CHƯƠNG 4
A(h1('CHƯƠNG 4. CÀI ĐẶT VÀ TRIỂN KHAI'));
A(h2('4.1. Cấu trúc dự án'));
A(liBold('backend/src: ', 'MoToSale.Common, .Entities, .DTO, .Repository, .Services và 3 host: .AuthService, .APIService, .ApiGateway.'));
A(liBold('backend/tests: ', 'MoToSale.Backend.Tests (unit/integration test).'));
A(liBold('frontend-admin: ', 'ứng dụng React (Vite) cho khu quản trị.'));
A(h2('4.2. Môi trường & cách chạy'));
A(p('Yêu cầu: .NET SDK 8, Node.js ≥ 20.19, SQL Server LocalDB. APIService tự động áp migration và seed dữ liệu mẫu khi khởi động lần đầu.'));
A(li('Backend: chạy lần lượt AuthService (5101), APIService (5102), ApiGateway (5100) bằng "dotnet run".'));
A(li('Frontend: "npm install" rồi "npm run dev" → http://localhost:5176 (proxy /api → 5100).'));
A(li('Đăng nhập: admin@motosale.local / Admin@123 (Admin); staff@motosale.local / Staff@123 (Nhân viên).'));
A(h2('4.3. Một số nghiệp vụ tiêu biểu đã cài đặt'));
A(h3('Bán tại quầy (POS) và đặt cọc'));
A(p('Khi tạo đơn POS bán đứt, hệ thống chạy trong một transaction: kiểm tồn khả dụng → tạo đơn và dòng đơn → ghi sổ cái xuất kho → cập nhật tồn → ghi phiếu thu và sổ quỹ → đặt trạng thái Hoàn tất. Với đơn đặt cọc, hệ thống giữ chỗ tồn (Reserved) và chỉ trừ kho thật khi thực hiện "Giao hàng & xuất kho".'));
A(h3('Đổi trả → hoàn tiền → ghi quỹ'));
A(p('Khi duyệt phiếu trả, hàng "bán lại được" được nhập lại kho, đồng thời hệ thống sinh phiếu hoàn tiền và ghi một khoản chi vào sổ quỹ, cập nhật công nợ — bảo đảm dòng tiền và tồn kho luôn khớp.'));
A(h3('Báo cáo lãi gộp/giá vốn (COGS)'));
A(p('Giá vốn được tính theo bình quân từ các phiếu nhập kho (GoodsReceipt), từ đó báo cáo đưa ra giá vốn hàng bán và lãi gộp theo kỳ, thay vì chỉ thống kê doanh thu.'));
A(pageBreak());

// CHƯƠNG 5
A(h1('CHƯƠNG 5. KIỂM THỬ'));
A(h2('5.1. Chiến lược kiểm thử'));
A(liBold('Unit/Integration test: ', 'khoảng 20 test ở tầng service kiểm các quy tắc lõi (chặn bán quá tồn, duyệt phiếu kho cập nhật tồn, đổi trả sinh hoàn tiền, nhận hàng restock, chặn xóa/sửa…).'));
A(liBold('Kiểm thử E2E mức API: ', 'kịch bản xuyên suốt qua Gateway bằng JWT, kiểm từng giá trị (trạng thái đơn, tiền, tồn, công nợ).'));
A(liBold('Kiểm thử giao diện thủ công: ', 'theo kế hoạch chi tiết từng trang/modal/field; kiểm hiển thị, ràng buộc và luồng nghiệp vụ.'));
A(h2('5.2. Kết quả'));
A(table([5800, 3226], [
  ['Hạng mục', 'Kết quả'],
  ['E2E – Xác thực + Danh mục/Sản phẩm', '15/15 PASS'],
  ['E2E – Bán hàng/POS/Đơn/Voucher', '12/12 PASS'],
  ['E2E – Kho/Đổi trả/Bảo hành/Sửa chữa/CSKH/Chấm công', '16/16 PASS'],
  ['E2E – Cung ứng/Tài chính/Báo cáo/Nhật ký/Phân quyền', '16/16 PASS'],
  ['Tổng kiểm thử E2E', '59/59 PASS'],
  ['Unit/Integration test (backend)', '20/20 PASS'],
  ['Build frontend / backend', 'PASS (0 cảnh báo, 0 lỗi)'],
]));
A(h2('5.3. Lỗi phát hiện và đã sửa'));
A(pb([new TextRun({ text: 'BUG-01 (mức Cao): ', bold: true }), new TextRun('trùng mã đơn khi tạo nhiều đơn trong cùng một giây (mã đơn dùng dấu thời gian tới giây). Đã sửa bằng cách thêm mili-giây vào mã đơn (POS/đơn online), kiểm thử lại không còn trùng.')]));
A(pageBreak());

// CHƯƠNG 6
A(h1('CHƯƠNG 6. KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN'));
A(h2('6.1. Kết quả đạt được'));
A(li('Xây dựng hệ thống quản trị hoàn chỉnh cho cửa hàng xe máy & phụ tùng theo kiến trúc microservices + SPA.'));
A(li('Bao quát đầy đủ chuỗi nghiệp vụ: mua – nhập kho – bán (online & POS) – hậu mãi – tài chính – báo cáo.'));
A(li('Bảo đảm tính nhất quán dữ liệu tiền/tồn/công nợ qua transaction và sổ cái bất biến; có phân quyền và nhật ký kiểm toán.'));
A(li('Kiểm thử đầy đủ: 59/59 E2E PASS, 20/20 unit test PASS, build sạch.'));
A(h2('6.2. Hạn chế'));
A(li('Chưa tích hợp cổng thanh toán trực tuyến và hóa đơn điện tử hợp pháp; thanh toán ghi nhận thủ công.'));
A(li('Mô hình một cửa hàng/một kho, chưa hỗ trợ đa chi nhánh.'));
A(li('Hóa đơn GTGT mới ở dạng in qua trình duyệt.'));
A(h2('6.3. Hướng phát triển'));
A(li('Tích hợp cổng thanh toán (VNPay/Momo) và hóa đơn điện tử theo chuẩn cơ quan thuế.'));
A(li('Mở rộng đa kho/đa chi nhánh, quản lý điều chuyển tồn.'));
A(li('Bổ sung ứng dụng cho khách hàng (web bán hàng) và báo cáo nâng cao (dự báo tồn, phân tích khách hàng).'));
A(li('Triển khai HTTPS, đưa secrets ra biến môi trường, đóng gói Docker và CI/CD.'));
A(pageBreak());

// TÀI LIỆU THAM KHẢO
A(h1('TÀI LIỆU THAM KHẢO'));
A(li('Microsoft, "ASP.NET Core documentation" & "Entity Framework Core documentation", docs.microsoft.com.'));
A(li('Ocelot API Gateway documentation – https://ocelot.readthedocs.io.'));
A(li('React documentation – https://react.dev; Vite – https://vitejs.dev.'));
A(li('JWT – https://jwt.io; tài liệu nội bộ dự án (SRS, Thiết kế, Triển khai, Kiểm thử) trong thư mục docs.'));

// ---------- document ----------
const doc = new Document({
  styles: {
    default: { document: { run: { font: 'Times New Roman', size: 26 } } }, // 13pt body
    paragraphStyles: [
      { id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { size: 32, bold: true, font: 'Times New Roman', color: '1F3864' },
        paragraph: { spacing: { before: 240, after: 160 }, outlineLevel: 0 } },
      { id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { size: 28, bold: true, font: 'Times New Roman', color: '2E4E8F' },
        paragraph: { spacing: { before: 180, after: 100 }, outlineLevel: 1 } },
      { id: 'Heading3', name: 'Heading 3', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { size: 26, bold: true, italics: true, font: 'Times New Roman' },
        paragraph: { spacing: { before: 120, after: 80 }, outlineLevel: 2 } },
    ],
  },
  numbering: {
    config: [{ reference: 'b', levels: [{ level: 0, format: LevelFormat.BULLET, text: '•', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 540, hanging: 280 } } } }] }],
  },
  sections: [{
    properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1440, right: 1134, bottom: 1440, left: 1418 } } },
    footers: {
      default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun('Trang '), new TextRun({ children: [PageNumber.CURRENT] }), new TextRun(' / '), new TextRun({ children: [PageNumber.TOTAL_PAGES] })] })] }),
    },
    children: [...cover, ...toc, ...body],
  }],
});

Packer.toBuffer(doc).then((buf) => {
  fs.writeFileSync('D:/MotorTeam/MoToSale-End/docs/BaoCao_DoAn_MoToSaleV2.docx', buf);
  console.log('WROTE docx', buf.length, 'bytes');
});
