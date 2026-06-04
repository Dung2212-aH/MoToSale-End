const path = 'C:/Users/DONGTONG/AppData/Roaming/npm/node_modules/pptxgenjs';
const pptxgen = require(path);
const pptx = new pptxgen();
pptx.defineLayout({ name: 'W', width: 13.333, height: 7.5 });
pptx.layout = 'W';

// palette
const DARK = '0E2236', NAVY = '14375E', ORANGE = 'FF6B35', TEAL = '1C7293';
const LIGHT = 'F1F5F9', MUTE = '5B6B7B', WHITE = 'FFFFFF', INK = '1B2733', LINE = 'D5DEE8';
const HF = 'Georgia', BF = 'Calibri';
const W = 13.333, H = 7.5;

function title(s, t, kicker) {
  if (kicker) s.addText(kicker.toUpperCase(), { x: 0.6, y: 0.45, w: 12, h: 0.3, fontFace: BF, fontSize: 12, bold: true, color: ORANGE, charSpacing: 2 });
  s.addText(t, { x: 0.6, y: 0.72, w: 12.1, h: 0.8, fontFace: HF, fontSize: 30, bold: true, color: NAVY });
}
function num(s, n, x, y) {
  s.addShape(pptx.ShapeType.ellipse, { x, y, w: 0.5, h: 0.5, fill: { color: ORANGE } });
  s.addText(n, { x, y, w: 0.5, h: 0.5, align: 'center', valign: 'middle', fontFace: HF, fontSize: 18, bold: true, color: WHITE });
}
function card(s, x, y, w, h, fill) {
  s.addShape(pptx.ShapeType.roundRect, { x, y, w, h, rectRadius: 0.08, fill: { color: fill || WHITE }, line: { color: LINE, width: 1 }, shadow: { type: 'outer', color: 'BBBBBB', blur: 4, offset: 2, angle: 90, opacity: 0.3 } });
}

// ---------- Slide 1: Title ----------
let s = pptx.addSlide();
s.background = { color: DARK };
s.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: 0.22, h: H, fill: { color: ORANGE } });
s.addText('HỌC VIỆN KỸ THUẬT QUÂN SỰ  ·  KHOA CÔNG NGHỆ THÔNG TIN', { x: 0.7, y: 0.7, w: 12, h: 0.4, fontFace: BF, fontSize: 13, color: 'CADCFC', charSpacing: 1 });
s.addText('BÁO CÁO ĐỒ ÁN — CÔNG NGHỆ WEB', { x: 0.7, y: 1.9, w: 12, h: 0.5, fontFace: BF, fontSize: 16, bold: true, color: ORANGE, charSpacing: 1 });
s.addText('Website bán xe máy & phụ tùng', { x: 0.68, y: 2.5, w: 12, h: 1.0, fontFace: HF, fontSize: 46, bold: true, color: WHITE });
s.addText('Hệ thống quản trị MoToSale v2', { x: 0.7, y: 3.55, w: 12, h: 0.6, fontFace: HF, fontSize: 24, italic: true, color: 'CADCFC' });
s.addText([
  { text: 'GVHD:  ', options: { bold: true, color: WHITE } }, { text: 'Trần Văn An', options: { color: 'CADCFC' } },
], { x: 0.7, y: 5.2, w: 12, h: 0.4, fontFace: BF, fontSize: 16 });
s.addText([
  { text: 'SVTH:  ', options: { bold: true, color: WHITE } }, { text: 'Tống Văn Đông  ·  MSSV: [MSSV]', options: { color: 'CADCFC' } },
], { x: 0.7, y: 5.65, w: 12, h: 0.4, fontFace: BF, fontSize: 16 });
s.addText('Năm học 2025 – 2026', { x: 0.7, y: 6.5, w: 12, h: 0.4, fontFace: BF, fontSize: 14, color: '8FA6BC' });

// ---------- Slide 2: Bài toán ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Bài toán đặt ra', 'Giới thiệu');
const probs = [
  ['Tồn kho khó kiểm soát', 'Xe nhiều biến thể, phụ tùng đa dạng; quản lý tay dễ sai lệch tồn.'],
  ['Dòng tiền & công nợ rời rạc', 'Thu/chi, đặt cọc, hoàn tiền không gắn với đơn hàng theo thời gian thực.'],
  ['Hậu mãi phức tạp', 'Bảo hành, sửa chữa, đổi trả cần theo dõi trạng thái và lịch sử.'],
  ['Thiếu báo cáo chính xác', 'Khó biết doanh thu thực nhận, lãi gộp, giá vốn theo kỳ.'],
];
probs.forEach((pr, i) => {
  const x = 0.6 + (i % 2) * 6.15, y = 2.0 + Math.floor(i / 2) * 2.05;
  card(s, x, y, 5.9, 1.8);
  num(s, String(i + 1), x + 0.3, y + 0.35);
  s.addText(pr[0], { x: x + 1.0, y: y + 0.28, w: 4.7, h: 0.5, fontFace: HF, fontSize: 18, bold: true, color: NAVY });
  s.addText(pr[1], { x: x + 1.0, y: y + 0.82, w: 4.7, h: 0.85, fontFace: BF, fontSize: 14, color: MUTE });
});

// ---------- Slide 3: Mục tiêu & phạm vi ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Mục tiêu & Phạm vi', 'Định hướng');
card(s, 0.6, 1.95, 7.4, 4.9, LIGHT);
s.addText('Mục tiêu', { x: 0.95, y: 2.2, w: 6.7, h: 0.5, fontFace: HF, fontSize: 20, bold: true, color: ORANGE });
const goals = ['Quản lý sản phẩm, biến thể (SKU) và tồn kho một cửa hàng', 'Bán hàng online + tại quầy (POS): bán đứt, đặt cọc, voucher', 'Hậu mãi: đổi trả – hoàn tiền, bảo hành, sửa chữa, CSKH', 'Tài chính: sổ quỹ, công nợ; báo cáo doanh thu & lãi gộp', 'Phân quyền Admin/Nhân viên, nhật ký kiểm toán'];
s.addText(goals.map(g => ({ text: g, options: { bullet: { code: '2022', indent: 14 } } })), { x: 0.95, y: 2.8, w: 6.8, h: 3.9, fontFace: BF, fontSize: 15, color: INK, lineSpacingMultiple: 1.25, paraSpaceAfter: 8 });
card(s, 8.25, 1.95, 4.5, 4.9, NAVY);
s.addText('Phạm vi', { x: 8.55, y: 2.2, w: 3.9, h: 0.5, fontFace: HF, fontSize: 20, bold: true, color: ORANGE });
s.addText('Trong phạm vi', { x: 8.55, y: 2.8, w: 3.9, h: 0.35, fontFace: BF, fontSize: 13, bold: true, color: 'CADCFC' });
s.addText('Toàn bộ khu quản trị cho mô hình 1 cửa hàng / 1 kho.', { x: 8.55, y: 3.15, w: 3.95, h: 0.8, fontFace: BF, fontSize: 14, color: WHITE });
s.addText('Ngoài phạm vi', { x: 8.55, y: 4.2, w: 3.9, h: 0.35, fontFace: BF, fontSize: 13, bold: true, color: 'CADCFC' });
s.addText('Cổng thanh toán trực tuyến, hóa đơn điện tử (mã CQT), tích hợp vận chuyển, đa chi nhánh.', { x: 8.55, y: 4.55, w: 3.95, h: 1.6, fontFace: BF, fontSize: 14, color: WHITE });

// ---------- Slide 4: Công nghệ ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Công nghệ sử dụng', 'Nền tảng');
const techs = [
  ['Backend', '.NET 8 · ASP.NET Core · EF Core (code-first)'],
  ['CSDL', 'SQL Server (LocalDB) — MoToSaleV2'],
  ['API Gateway', 'Ocelot — điểm vào duy nhất, định tuyến'],
  ['Xác thực', 'JWT · băm mật khẩu PBKDF2'],
  ['Frontend', 'React 18 · Vite · Tailwind/AdminLTE · Axios'],
  ['Tiện ích', 'Swagger · ExcelJS · in hóa đơn trình duyệt'],
];
techs.forEach((t, i) => {
  const x = 0.6 + (i % 3) * 4.12, y = 2.15 + Math.floor(i / 3) * 2.2;
  card(s, x, y, 3.9, 1.95);
  s.addShape(pptx.ShapeType.roundRect, { x: x + 0.3, y: y + 0.32, w: 0.55, h: 0.55, rectRadius: 0.1, fill: { color: TEAL } });
  s.addText(['◆', '▦', '⇆', '◈', '⬢', '✦'][i], { x: x + 0.3, y: y + 0.32, w: 0.55, h: 0.55, align: 'center', valign: 'middle', fontSize: 16, color: WHITE });
  s.addText(t[0], { x: x + 1.0, y: y + 0.34, w: 2.8, h: 0.5, fontFace: HF, fontSize: 17, bold: true, color: NAVY });
  s.addText(t[1], { x: x + 0.3, y: y + 1.0, w: 3.35, h: 0.85, fontFace: BF, fontSize: 13, color: MUTE });
});

// ---------- Slide 5: Kiến trúc ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Kiến trúc hệ thống', 'Thiết kế');
function box(x, y, w, h, t, sub, fill, tc) {
  s.addShape(pptx.ShapeType.roundRect, { x, y, w, h, rectRadius: 0.08, fill: { color: fill }, line: { color: fill === WHITE ? LINE : fill, width: 1 } });
  s.addText(t, { x, y: y + (sub ? 0.12 : 0), w, h: sub ? h - 0.45 : h, align: 'center', valign: 'middle', fontFace: HF, fontSize: 16, bold: true, color: tc });
  if (sub) s.addText(sub, { x, y: y + h - 0.5, w, h: 0.4, align: 'center', fontFace: BF, fontSize: 11, color: tc === WHITE ? 'CADCFC' : MUTE });
}
// horizontal-only arrows for reliable rendering; tall GW & DB boxes span both service rows
box(0.7, 3.5, 2.3, 1.2, 'Frontend', 'React SPA (5176)', NAVY, WHITE);
box(3.9, 2.9, 2.3, 2.4, 'API Gateway', 'Ocelot (5100)', ORANGE, WHITE);
box(7.1, 2.65, 2.6, 1.1, 'AuthService', '5101 · JWT, tài khoản', TEAL, WHITE);
box(7.1, 4.45, 2.6, 1.1, 'APIService', '5102 · nghiệp vụ', TEAL, WHITE);
box(10.5, 2.9, 2.1, 2.4, 'SQL Server', 'MoToSaleV2', '36506B', WHITE);
function harrow(x1, x2, y) { s.addShape(pptx.ShapeType.line, { x: x1, y, w: x2 - x1, h: 0, line: { color: MUTE, width: 2, endArrowType: 'triangle' } }); }
harrow(3.0, 3.9, 4.1);   // FE -> GW
harrow(6.2, 7.1, 3.2);   // GW -> Auth
harrow(6.2, 7.1, 5.0);   // GW -> API
harrow(9.7, 10.5, 3.2);  // Auth -> DB
harrow(9.7, 10.5, 5.0);  // API -> DB
s.addText('Backend phân lớp: Common → Entities → DTO → Repository (AppDbContext, UnitOfWork, Audit) → Services → Host', { x: 0.7, y: 6.2, w: 12, h: 0.5, align: 'center', fontFace: BF, fontSize: 14, italic: true, color: MUTE });

// ---------- Slide 6: CSDL ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Cơ sở dữ liệu', 'Thiết kế');
card(s, 0.6, 2.0, 3.0, 4.7, NAVY);
s.addText('~50', { x: 0.6, y: 2.7, w: 3.0, h: 1.1, align: 'center', fontFace: HF, fontSize: 64, bold: true, color: ORANGE });
s.addText('bảng dữ liệu', { x: 0.6, y: 3.85, w: 3.0, h: 0.4, align: 'center', fontFace: BF, fontSize: 16, color: WHITE });
s.addText('Code-first (EF Core)\nXóa mềm qua BaseEntity\nSổ cái kho/quỹ bất biến', { x: 0.6, y: 4.5, w: 3.0, h: 1.8, align: 'center', fontFace: BF, fontSize: 13, color: 'CADCFC', lineSpacingMultiple: 1.4 });
const groups = [['Identity', 'Users · Roles · Addresses'], ['Catalog', 'Products · Skus · Categories · Brands'], ['Inventory', 'InventoryItems · StockMovements · Reservations'], ['Ordering', 'Orders · OrderLines · Vouchers · Warranties'], ['Operations', 'SalesReturns · Refunds · PurchaseOrders · Repairs'], ['System', 'Payments · CashTransactions · AuditLogs · Settings']];
groups.forEach((g, i) => {
  const x = 3.8 + (i % 2) * 4.5, y = 2.0 + Math.floor(i / 2) * 1.6;
  card(s, x, y, 4.3, 1.4);
  s.addText(g[0], { x: x + 0.25, y: y + 0.18, w: 3.85, h: 0.4, fontFace: HF, fontSize: 16, bold: true, color: ORANGE });
  s.addText(g[1], { x: x + 0.25, y: y + 0.62, w: 3.9, h: 0.7, fontFace: BF, fontSize: 12.5, color: MUTE });
});

// ---------- Slide 7: Chức năng chính ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Chức năng chính — 5 nhóm', 'Tính năng');
const feats = [
  ['Bán hàng', 'POS, Đơn hàng, Khách hàng, Voucher'],
  ['Sản phẩm & Kho', 'Sản phẩm, Danh mục, Tồn kho, Cung ứng'],
  ['Dịch vụ & Hậu mãi', 'Đổi trả, Bảo hành, Sửa chữa, CSKH'],
  ['Tài chính & Báo cáo', 'Sổ quỹ, Công nợ, Doanh thu, Lãi gộp'],
  ['Hệ thống', 'Tài khoản, Phân ca, Cấu hình, Nhật ký'],
];
feats.forEach((f, i) => {
  let x, y, w = 3.9, h = 2.0;
  if (i < 3) { x = 0.6 + i * 4.12; y = 2.1; }
  else { x = 2.66 + (i - 3) * 4.12; y = 4.35; }
  card(s, x, y, w, h, i % 2 === 0 ? LIGHT : WHITE);
  num(s, String(i + 1), x + 0.3, y + 0.32);
  s.addText(f[0], { x: x + 1.0, y: y + 0.34, w: 2.8, h: 0.55, fontFace: HF, fontSize: 17, bold: true, color: NAVY });
  s.addText(f[1], { x: x + 0.3, y: y + 1.05, w: 3.35, h: 0.85, fontFace: BF, fontSize: 13.5, color: MUTE });
});

// ---------- Slide 8: POS & đặt cọc ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Nghiệp vụ nổi bật: Bán tại quầy & Đặt cọc', 'Điểm nhấn');
const steps = ['Chọn SKU\n+ khách quen', 'Bán đứt\nhoặc đặt cọc', 'Giữ chỗ tồn\n(đơn cọc)', 'Giao hàng\n& xuất kho', 'Thu đủ →\nHoàn tất'];
steps.forEach((st, i) => {
  const x = 0.7 + i * 2.45;
  s.addShape(pptx.ShapeType.roundRect, { x, y: 2.4, w: 2.1, h: 1.5, rectRadius: 0.1, fill: { color: i === 1 || i === 3 ? NAVY : LIGHT }, line: { color: LINE, width: 1 } });
  s.addText(String(i + 1), { x: x + 0.1, y: 2.5, w: 0.5, h: 0.4, fontFace: HF, fontSize: 16, bold: true, color: ORANGE });
  s.addText(st, { x: x + 0.1, y: 2.95, w: 1.9, h: 0.9, align: 'center', fontFace: BF, fontSize: 13, bold: true, color: i === 1 || i === 3 ? WHITE : INK });
  if (i < 4) s.addShape(pptx.ShapeType.line, { x: x + 2.1, y: 3.15, w: 0.35, h: 0, line: { color: ORANGE, width: 2, endArrowType: 'triangle' } });
});
card(s, 0.7, 4.5, 11.95, 2.2, LIGHT);
s.addText('Bảo đảm nhất quán', { x: 1.0, y: 4.7, w: 11, h: 0.4, fontFace: HF, fontSize: 17, bold: true, color: ORANGE });
s.addText([
  { text: 'Toàn bộ chạy trong một transaction: ', options: { bold: true } },
  { text: 'kiểm tồn khả dụng → tạo đơn → ghi sổ cái kho → cập nhật tồn → ghi phiếu thu & sổ quỹ → đặt trạng thái. Đơn cọc giữ chỗ tồn (Reserved), chỉ trừ kho thật khi giao hàng; hủy đơn cọc mặc định khách mất cọc.', options: {} },
], { x: 1.0, y: 5.15, w: 11.4, h: 1.4, fontFace: BF, fontSize: 15, color: INK, lineSpacingMultiple: 1.2 });

// ---------- Slide 9: Đổi trả + COGS ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Đổi trả → Hoàn tiền → Quỹ  ·  Lãi gộp (COGS)', 'Điểm nhấn');
card(s, 0.6, 2.0, 6.0, 4.7);
s.addText('Chuỗi đổi trả tự động', { x: 0.9, y: 2.25, w: 5.4, h: 0.5, fontFace: HF, fontSize: 19, bold: true, color: NAVY });
['Duyệt phiếu trả hàng', 'Hàng bán lại được → nhập về kho', 'Sinh phiếu hoàn tiền', 'Ghi một khoản chi vào sổ quỹ', 'Cập nhật công nợ khách'].forEach((t, i) => {
  num(s, String(i + 1), 0.95, 2.95 + i * 0.72);
  s.addText(t, { x: 1.6, y: 2.95 + i * 0.72, w: 4.8, h: 0.5, valign: 'middle', fontFace: BF, fontSize: 15, color: INK });
});
card(s, 6.85, 2.0, 5.8, 4.7, NAVY);
s.addText('Báo cáo lãi gộp / giá vốn', { x: 7.15, y: 2.25, w: 5.2, h: 0.5, fontFace: HF, fontSize: 19, bold: true, color: ORANGE });
s.addText('Lãi gộp = Doanh thu − Giá vốn', { x: 7.15, y: 3.0, w: 5.3, h: 0.5, fontFace: HF, fontSize: 18, bold: true, color: WHITE });
s.addText([
  { text: 'Giá vốn ', options: { bold: true, color: 'CADCFC' } },
  { text: 'tính bình quân từ các phiếu nhập kho (GoodsReceipt).', options: { color: WHITE } },
], { x: 7.15, y: 3.7, w: 5.3, h: 0.8, fontFace: BF, fontSize: 15 });
s.addText('Doanh thu chỉ tính cho đơn đã thanh toán đủ và đã giao/hoàn tất; đơn hủy không tính; hoàn tiền điều chỉnh tiền thực nhận.', { x: 7.15, y: 4.6, w: 5.3, h: 1.8, fontFace: BF, fontSize: 14, color: 'CADCFC', lineSpacingMultiple: 1.3 });

// ---------- Slide 10: Phân quyền & toàn vẹn ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Phân quyền & Toàn vẹn dữ liệu', 'Chất lượng');
card(s, 0.6, 2.0, 5.95, 4.7, LIGHT);
s.addText('Phân quyền', { x: 0.9, y: 2.25, w: 5.4, h: 0.5, fontFace: HF, fontSize: 19, bold: true, color: ORANGE });
s.addText([
  { text: 'Admin: ', options: { bold: true } }, { text: 'toàn quyền (tài chính, tài khoản, cung ứng, cấu hình, nhật ký, import).\n', options: {} },
  { text: 'Nhân viên: ', options: { bold: true } }, { text: 'bán hàng/POS, đổi trả, bảo hành, sửa chữa, CSKH, chấm công.\n', options: {} },
  { text: 'Enforce ở cả API ', options: { bold: true } }, { text: '— Staff gọi endpoint Admin bị chặn (403).', options: {} },
], { x: 0.9, y: 2.85, w: 5.4, h: 3.6, fontFace: BF, fontSize: 15, color: INK, lineSpacingMultiple: 1.35, paraSpaceAfter: 8 });
card(s, 6.8, 2.0, 5.85, 4.7, LIGHT);
s.addText('Toàn vẹn dữ liệu', { x: 7.1, y: 2.25, w: 5.3, h: 0.5, fontFace: HF, fontSize: 19, bold: true, color: ORANGE });
['Transaction cho thao tác đa bước', 'Sổ cái kho & quỹ bất biến (append-only)', 'Chặn xóa đối tượng đã phát sinh giao dịch', 'Chỉ sửa giao dịch khi chưa có hiệu lực', 'Mọi thay đổi đều ghi nhật ký kiểm toán'].forEach((t, i) => {
  s.addText('✓', { x: 7.1, y: 2.95 + i * 0.7, w: 0.4, h: 0.4, fontFace: BF, fontSize: 16, bold: true, color: TEAL });
  s.addText(t, { x: 7.55, y: 2.9 + i * 0.7, w: 4.9, h: 0.55, valign: 'middle', fontFace: BF, fontSize: 14.5, color: INK });
});

// ---------- Slide 11: Kiểm thử ----------
s = pptx.addSlide(); s.background = { color: DARK };
s.addText('KIỂM THỬ', { x: 0.6, y: 0.5, w: 12, h: 0.3, fontFace: BF, fontSize: 12, bold: true, color: ORANGE, charSpacing: 2 });
s.addText('Kết quả kiểm thử', { x: 0.6, y: 0.8, w: 12, h: 0.8, fontFace: HF, fontSize: 30, bold: true, color: WHITE });
const stats = [['59/59', 'E2E mức API\nPASS'], ['20/20', 'Unit/Integration\ntest PASS'], ['0 / 0', 'Build FE & BE\ncảnh báo / lỗi'], ['1', 'lỗi thật\nđã phát hiện & sửa']];
stats.forEach((st, i) => {
  const x = 0.7 + i * 3.07;
  s.addShape(pptx.ShapeType.roundRect, { x, y: 2.2, w: 2.8, h: 2.4, rectRadius: 0.1, fill: { color: '17314A' }, line: { color: '2A4A6B', width: 1 } });
  s.addText(st[0], { x, y: 2.55, w: 2.8, h: 1.1, align: 'center', fontFace: HF, fontSize: 44, bold: true, color: ORANGE });
  s.addText(st[1], { x, y: 3.7, w: 2.8, h: 0.8, align: 'center', fontFace: BF, fontSize: 14, color: 'CADCFC' });
});
s.addText([
  { text: 'BUG-01 (đã sửa): ', options: { bold: true, color: ORANGE } },
  { text: 'trùng mã đơn khi tạo nhiều đơn trong cùng một giây → thêm mili-giây vào mã đơn, kiểm thử lại đạt.', options: { color: WHITE } },
], { x: 0.7, y: 5.4, w: 11.9, h: 1.2, fontFace: BF, fontSize: 15, align: 'center', lineSpacingMultiple: 1.2 });

// ---------- Slide 12: Kết luận ----------
s = pptx.addSlide(); s.background = { color: WHITE };
title(s, 'Kết luận & Hướng phát triển', 'Tổng kết');
const cols = [
  ['Đạt được', TEAL, ['Hệ thống quản trị hoàn chỉnh', 'Bao quát mua – kho – bán – hậu mãi – tài chính', 'Dữ liệu tiền/tồn/công nợ nhất quán', 'Kiểm thử đầy đủ, build sạch']],
  ['Hạn chế', 'B5651D', ['Chưa tích hợp cổng thanh toán', 'Hóa đơn GTGT mới in trình duyệt', 'Mô hình 1 cửa hàng / 1 kho']],
  ['Hướng phát triển', NAVY, ['Cổng thanh toán & hóa đơn điện tử', 'Đa kho / đa chi nhánh', 'Web bán hàng cho khách', 'HTTPS, Docker, CI/CD']],
];
cols.forEach((c, i) => {
  const x = 0.6 + i * 4.12;
  card(s, x, 2.0, 3.9, 4.7);
  s.addShape(pptx.ShapeType.roundRect, { x: x, y: 2.0, w: 3.9, h: 0.7, rectRadius: 0.08, fill: { color: c[1] } });
  s.addText(c[0], { x, y: 2.0, w: 3.9, h: 0.7, align: 'center', valign: 'middle', fontFace: HF, fontSize: 17, bold: true, color: WHITE });
  s.addText(c[2].map(t => ({ text: t, options: { bullet: { code: '2022', indent: 14 } } })), { x: x + 0.3, y: 2.95, w: 3.35, h: 3.55, fontFace: BF, fontSize: 14, color: INK, lineSpacingMultiple: 1.2, paraSpaceAfter: 9 });
});

// ---------- Slide 13: Thank you ----------
s = pptx.addSlide(); s.background = { color: DARK };
s.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: 0.22, h: H, fill: { color: ORANGE } });
s.addText('Cảm ơn thầy và các bạn', { x: 0.8, y: 2.7, w: 12, h: 1.0, fontFace: HF, fontSize: 44, bold: true, color: WHITE });
s.addText('đã lắng nghe!', { x: 0.82, y: 3.7, w: 12, h: 0.8, fontFace: HF, fontSize: 30, italic: true, color: 'CADCFC' });
s.addText('MoToSale v2  ·  Website bán xe máy & phụ tùng  ·  Tống Văn Đông', { x: 0.82, y: 5.4, w: 12, h: 0.4, fontFace: BF, fontSize: 15, color: '8FA6BC' });

pptx.writeFile({ fileName: 'D:/MotorTeam/MoToSale-End/docs/Slide_MoToSaleV2.pptx' }).then(f => console.log('WROTE', f));
