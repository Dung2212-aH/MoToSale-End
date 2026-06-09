import { formatCurrency } from './formatCurrency';

const SHOP_NAME = 'SHOWROOM XE MOTOSALE';
const SHOP_ADDRESS = 'Số 1 Đại Cồ Việt, Phường Bách Khoa, Quận Hai Bà Trưng, TP. Hà Nội';
const SHOP_REPRESENTATIVE = 'PHẠM TIẾN DŨNG';

const fmtDate = (value) => {
  if (!value) return '_______________';
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? '_______________' : d.toLocaleDateString('vi-VN');
};

const escapeHtml = (value) => {
  if (value === null || value === undefined || value === '') return '';
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
};

const fillBlank = (value, dots = 30) => {
  if (value === null || value === undefined || value === '') return '.'.repeat(dots);
  return escapeHtml(value);
};

function numberToVietnameseWords(n) {
  if (n === null || n === undefined) return '';
  n = Math.round(Number(n));
  if (Number.isNaN(n)) return '';
  if (n === 0) return 'không';
  const digits = ['không', 'một', 'hai', 'ba', 'bốn', 'năm', 'sáu', 'bảy', 'tám', 'chín'];
  const readTriplet = (num, full) => {
    const h = Math.floor(num / 100);
    const t = Math.floor((num % 100) / 10);
    const u = num % 10;
    const parts = [];
    if (h > 0 || full) {
      parts.push(digits[h] + ' trăm');
      if (t === 0 && u > 0) parts.push('lẻ ' + digits[u]);
    }
    if (t > 0) {
      if (t === 1) parts.push('mười');
      else parts.push(digits[t] + ' mươi');
    }
    if (u > 0 && t !== 0) {
      if (u === 1 && t > 1) parts.push('mốt');
      else if (u === 5 && t > 0) parts.push('lăm');
      else parts.push(digits[u]);
    } else if (u > 0 && !full && t === 0) {
      parts.push(digits[u]);
    }
    return parts.join(' ').trim();
  };
  const units = ['', 'nghìn', 'triệu', 'tỷ'];
  const groups = [];
  let temp = n;
  while (temp > 0) {
    groups.push(temp % 1000);
    temp = Math.floor(temp / 1000);
  }
  const words = [];
  for (let i = groups.length - 1; i >= 0; i--) {
    if (groups[i] > 0 || (i === 0 && words.length > 0)) {
      const part = readTriplet(groups[i], i !== groups.length - 1);
      if (part) words.push(part + (units[i] ? ' ' + units[i] : ''));
    }
  }
  return words.join(' ').replace(/\s+/g, ' ').trim();
}

const currencyInWords = (amount) => {
  const w = numberToVietnameseWords(amount);
  return w ? w.charAt(0).toUpperCase() + w.slice(1) + ' đồng' : '';
};

export function printInstallmentApplication(order, plan) {
  if (!plan) return;
  const w = window.open('', '_blank', 'width=900,height=700');
  if (!w) {
    alert('Trình duyệt đã chặn cửa sổ in. Vui lòng cho phép pop-up rồi thử lại.');
    return;
  }

  const items = order?.chiTiet || order?.items || [];
  const firstItem = items[0] || {};
  const productName = firstItem.tenSanPhamSnapshot || firstItem.productNameSnapshot || firstItem.tenSanPham || firstItem.productName || '_____________________';
  const productSku = firstItem.skuSnapshot || firstItem.sku || '_______________';
  const orderCode = order?.maDonHangKinhDoanh || order?.orderCode || order?.maDonHang || order?.id || '';
  const buyerAddress = order?.diaChiNhanHang || order?.shippingAddressLine || '';

  const totalAmount = Number(order?.tongThanhToan ?? order?.totalAmount ?? 0);
  const downPayment = Number(plan.tienTraTruoc || 0);
  const principal = Number(plan.soTienGoc || 0);
  const termCount = Number(plan.soKy || 0);
  const monthlyPayment = termCount > 0 ? Math.round(Number(plan.tongPhaiTra || 0) / termCount) : 0;
  const firstTermDate = (plan.terms || [])[0]?.ngayDenHan;
  const dueDayOfMonth = firstTermDate ? new Date(firstTermDate).getDate() : '';

  const now = new Date();
  const dd = now.getDate();
  const mm = now.getMonth() + 1;
  const yy = now.getFullYear();

  const contractNo = `HS-${plan.maHoSoTraGop || ''}/HĐMB`;
  const buyerName = plan.hoTenNguoiVay || '';
  const buyerCccd = plan.soCCCD || '';
  const buyerPermanent = plan.diaChiThuongTru || '';

  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="utf-8" />
  <title>Hợp đồng mua bán xe trả góp — #${escapeHtml(orderCode)}</title>
  <style>
    * { box-sizing: border-box; }
    body { font-family: 'Times New Roman', serif; color: #000; padding: 40px 50px; font-size: 13pt; line-height: 1.5; max-width: 800px; margin: 0 auto; }
    .header { text-align: center; font-weight: bold; }
    .header .country { font-size: 13pt; }
    .header .motto { font-size: 13pt; text-decoration: underline; margin-top: 2px; }
    h1 { text-align: center; font-size: 16pt; margin: 24px 0 4px; }
    .contract-no { text-align: center; margin-bottom: 16px; }
    .meta { margin: 14px 0 18px; }
    .party { margin-top: 14px; }
    .party-title { font-weight: bold; }
    .indent { padding-left: 24px; }
    .article { margin-top: 14px; }
    .article-title { font-weight: bold; }
    .signatures { margin-top: 60px; display: grid; grid-template-columns: 1fr 1fr; gap: 60px; text-align: center; }
    .signatures div { padding-bottom: 90px; }
    .signatures b { display: block; font-size: 14pt; }
    .signatures i { display: block; font-size: 11pt; font-style: italic; margin-top: 2px; }
    table.schedule { width: 100%; border-collapse: collapse; margin-top: 10px; font-size: 11pt; }
    table.schedule th, table.schedule td { border: 1px solid #000; padding: 5px 8px; text-align: left; }
    table.schedule th { background: #f0f0f0; text-align: center; }
    table.schedule td.right { text-align: right; }
    table.schedule td.ctr { text-align: center; }
    h2.section { font-size: 13pt; margin-top: 24px; }
    @media print { body { padding: 0 20mm; max-width: 100%; } }
  </style>
</head>
<body>
  <div class="header">
    <div class="country">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
    <div class="motto">Độc lập – Tự do – Hạnh phúc</div>
  </div>

  <h1>HỢP ĐỒNG MUA BÁN XE MÁY TRẢ GÓP</h1>
  <div class="contract-no">Số: ${escapeHtml(contractNo)}</div>

  <div class="meta">
    Hôm nay, ngày <b>${dd}</b> tháng <b>${mm}</b> năm <b>${yy}</b>, tại ${SHOP_NAME}.<br/>
    Chúng tôi gồm:
  </div>

  <div class="party">
    <div class="party-title">BÊN A (Bên bán):</div>
    <div class="indent">
      - Tên cửa hàng: <b>${SHOP_NAME}</b><br/>
      - Địa chỉ: ${SHOP_ADDRESS}<br/>
      - Do ông/bà: ${SHOP_REPRESENTATIVE} làm đại diện
    </div>
  </div>

  <div class="party">
    <div class="party-title">BÊN B (Bên mua):</div>
    <div class="indent">
      - Họ và tên: <b>${fillBlank(buyerName, 40)}</b><br/>
      - Số CMND/CCCD: <b>${fillBlank(buyerCccd, 20)}</b> &nbsp; Ngày cấp: <b>${escapeHtml(fmtDate(plan.ngayCapCCCD))}</b> &nbsp; Nơi cấp: <b>${fillBlank(plan.noiCapCCCD, 30)}</b><br/>
      - Địa chỉ thường trú: ${fillBlank(buyerPermanent, 50)}<br/>
      - Chỗ ở hiện tại: ${fillBlank(buyerAddress, 50)}<br/>
      - Số điện thoại: ${fillBlank(plan.soDienThoai, 15)}
    </div>
  </div>

  <div class="article">
    <div class="article-title">Điều 1: Bên A đồng ý bán trả góp cho Bên B xe gắn máy với thông tin như sau:</div>
    <div class="indent">
      - Xe: <b>${escapeHtml(productName)}</b><br/>
      - SKU/Model: ${escapeHtml(productSku)}<br/>
      - Số khung: .................................................. (do Bên A điền khi giao xe)<br/>
      - Số máy: ..................................................... (do Bên A điền khi giao xe)<br/>
      - Với giá: <b>${escapeHtml(formatCurrency(totalAmount))}</b>
        (bằng chữ: <i>${escapeHtml(currencyInWords(totalAmount))}</i>)
    </div>
  </div>

  <div class="article">
    <div class="article-title">Điều 2: Phương thức thanh toán</div>
    <div class="indent">
      Bên B trả trước cho Bên A số tiền là: <b>${escapeHtml(formatCurrency(downPayment))}</b>
      (bằng chữ: <i>${escapeHtml(currencyInWords(downPayment))}</i>) vào ngày nhận xe.<br/>
      Số tiền còn lại (gốc): <b>${escapeHtml(formatCurrency(principal))}</b>
      (bằng chữ: <i>${escapeHtml(currencyInWords(principal))}</i>) Bên B sẽ thanh toán trong <b>${escapeHtml(termCount)}</b> tháng,
      lãi suất <b>${escapeHtml(plan.laiSuatNam)}%/năm</b> (lãi phẳng tính trên dư nợ gốc).<br/>
      Tổng tiền lãi: <b>${escapeHtml(formatCurrency(plan.tongTienLai))}</b>.
      Tổng tiền phải trả qua các kỳ (gốc + lãi): <b>${escapeHtml(formatCurrency(plan.tongPhaiTra))}</b>.<br/>
      Mỗi tháng Bên B trả khoảng <b>${escapeHtml(formatCurrency(monthlyPayment))}</b>, vào ngày <b>${escapeHtml(dueDayOfMonth)}</b> mỗi tháng (theo lịch chi tiết bên dưới).<br/>
      Nếu Bên B thanh toán chậm sẽ bị phạt theo lãi suất trả chậm hàng tháng là (<b>0,05</b>%/ngày).
    </div>

    <h2 class="section">Lịch thanh toán chi tiết</h2>
    <table class="schedule">
      <thead>
        <tr>
          <th>Kỳ</th>
          <th>Ngày đến hạn</th>
          <th>Tiền gốc</th>
          <th>Tiền lãi</th>
          <th>Tổng phải trả</th>
        </tr>
      </thead>
      <tbody>
        ${(plan.terms || []).map((t) => `
          <tr>
            <td class="ctr">Kỳ ${escapeHtml(t.kyThu)}</td>
            <td class="ctr">${escapeHtml(fmtDate(t.ngayDenHan))}</td>
            <td class="right">${escapeHtml(formatCurrency(t.soTienGoc))}</td>
            <td class="right">${escapeHtml(formatCurrency(t.soTienLai))}</td>
            <td class="right"><b>${escapeHtml(formatCurrency(t.tongTien))}</b></td>
          </tr>`).join('')}
      </tbody>
    </table>
  </div>

  <div class="article">
    <div class="article-title">Điều 3: Tranh chấp</div>
    <div class="indent">
      Mọi tranh chấp xảy ra sẽ được hai bên hòa giải, thương lượng, nếu không hòa giải được sẽ do Tòa án địa phương giải quyết mọi tranh chấp.
      Hợp đồng làm thành 02 bản, mỗi bên giữ 01 bản và có giá trị pháp lý như nhau.
    </div>
  </div>

  <div class="signatures">
    <div>
      <b>BÊN A</b>
      <i>(ký và ghi họ tên)</i>
    </div>
    <div>
      <b>BÊN B</b>
      <i>(ký và ghi họ tên)</i>
    </div>
  </div>

  <script>window.onload = function () { window.print(); };</script>
</body>
</html>`;

  w.document.write(html);
  w.document.close();
}
