import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import orderService from '../../services/orderService';
import {
  DELIVERY_SHIPPING_STATUS_OPTIONS,
  ORDER_NEXT_STATUS,
  ORDER_STATUS_OPTIONS,
  ORDER_TYPE_LABELS,
  PAYMENT_METHODS,
  PAYMENT_STATUS_OPTIONS,
  PICKUP_SHIPPING_STATUS_OPTIONS,
  SHIPPING_STATUS_OPTIONS,
  getOrderStatusMeta,
  getPaymentStatusContextual,
  getPaymentStatusMeta,
  getShippingStatusMeta,
} from '../../utils/constants';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';
import { printInstallmentApplication } from '../../utils/printInstallmentApplication';

const isLockedOrder = (status) => ['Cancelled', 'Completed'].includes(status);
const canCancelOrder = (status) => !['Cancelled', 'Delivered', 'Completed'].includes(status);
const SHIPPING_STATUS_BY_ORDER_STATUS = {
  Processing: 'Preparing',
  Shipping: 'Shipping',
  Delivered: 'Delivered',
};

const EVENT_LABELS = {
  Created: 'Tạo đơn',
  OrderStatus: 'Trạng thái đơn',
  PaymentStatus: 'Thanh toán',
  ShippingStatus: 'Vận chuyển',
};

const formatTimelineDate = (value) => {
  if (!value) return '';
  const raw = String(value);
  const hasTimezone = /(?:z|[+-]\d{2}:?\d{2})$/i.test(raw);
  const normalized = hasTimezone ? raw : `${raw}Z`;
  const date = new Date(normalized);

  if (Number.isNaN(date.getTime())) return '';

  return new Intl.DateTimeFormat('vi-VN', {
    timeZone: 'Asia/Ho_Chi_Minh',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).format(date);
};

const getTimelineTimestamp = (value) => {
  if (!value) return 0;
  const raw = String(value);
  const hasTimezone = /(?:z|[+-]\d{2}:?\d{2})$/i.test(raw);
  const date = new Date(hasTimezone ? raw : `${raw}Z`);
  return Number.isNaN(date.getTime()) ? 0 : date.getTime();
};

const OrderDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [showShippingModal, setShowShippingModal] = useState(false);
  const [newStatus, setNewStatus] = useState('');
  const [newPaymentStatus, setNewPaymentStatus] = useState('');
  const [newShippingStatus, setNewShippingStatus] = useState('');
  const [cancelReason, setCancelReason] = useState('');
  const [paymentNote, setPaymentNote] = useState('');
  const [shippingNote, setShippingNote] = useState('');
  const [updating, setUpdating] = useState(false);
  const [paymentInfo, setPaymentInfo] = useState(null);

  const fetchOrder = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await orderService.getById(id);
      setOrder(res.data);
      try {
        const infoRes = await orderService.getPaymentInfo(id);
        setPaymentInfo(infoRes.data);
      } catch {
        setPaymentInfo(null);
      }
    } catch (err) {
      setError('Không thể tải thông tin đơn hàng.');
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmPayment = async (maKyTraGop) => {
    const message = maKyTraGop
      ? 'Xác nhận đã nhận thanh toán cho kỳ trả góp này?'
      : 'Xác nhận đã nhận thanh toán (chuyển khoản) cho đơn này?';
    if (!window.confirm(message)) return;
    setUpdating(true);
    try {
      await orderService.confirmPayment(id, maKyTraGop ? { maKyTraGop } : {});
      await fetchOrder();
    } catch (err) {
      alert(err?.response?.data?.message || 'Xác nhận thanh toán thất bại. Vui lòng thử lại.');
    } finally {
      setUpdating(false);
    }
  };

  useEffect(() => {
    fetchOrder();
  }, [id]);

  const orderStatus = order?.trangThaiDonHang || order?.trangThai || order?.status || '';
  const paymentStatus = order?.trangThaiThanhToan || order?.paymentStatus || order?.thanhToan?.trangThai || order?.payment?.status || '';
  const shippingStatus = order?.trangThaiVanChuyen || order?.shippingStatus || '';
  const receiveMethod = order?.phuongThucNhanHang || order?.receivingMethod || 'Delivery';

  const nextStatusOptions = useMemo(() => {
    const allowed = ORDER_NEXT_STATUS[orderStatus] || [];
    return ORDER_STATUS_OPTIONS.filter((opt) => allowed.includes(opt.value));
  }, [orderStatus]);

  const shippingOptions = useMemo(() => {
    if (receiveMethod === 'Pickup') return PICKUP_SHIPPING_STATUS_OPTIONS;
    if (receiveMethod === 'Delivery') return DELIVERY_SHIPPING_STATUS_OPTIONS;
    return SHIPPING_STATUS_OPTIONS;
  }, [receiveMethod]);

  const renderBadge = (meta) => <span className={`badge badge-${meta.color}`}>{meta.label}</span>;

  const runUpdate = async (payload, onDone) => {
    setUpdating(true);
    try {
      await orderService.updateStatus(id, payload);
      onDone?.();
      await fetchOrder();
    } catch (err) {
      alert(err?.response?.data?.message || 'Cập nhật thất bại. Vui lòng thử lại.');
    } finally {
      setUpdating(false);
    }
  };

  const handleUpdateStatus = async () => {
    if (!newStatus) return;
    if (newStatus === 'Cancelled' && !cancelReason.trim()) {
      alert('Vui lòng nhập lý do hủy đơn.');
      return;
    }
    await runUpdate(
      {
        trangThaiDonHang: newStatus,
        trangThaiVanChuyen: SHIPPING_STATUS_BY_ORDER_STATUS[newStatus],
        lyDoHuyDon: cancelReason.trim() || undefined,
      },
      () => {
        setShowStatusModal(false);
        setNewStatus('');
        setCancelReason('');
      }
    );
  };

  const handleUpdatePaymentStatus = async () => {
    if (!newPaymentStatus) return;
    if (newPaymentStatus === 'Refunded' && !paymentNote.trim()) {
      alert('Vui lòng nhập ghi chú/lý do hoàn tiền.');
      return;
    }
    await runUpdate(
      { trangThaiThanhToan: newPaymentStatus, ghiChuThanhToan: paymentNote.trim() || undefined },
      () => {
        setShowPaymentModal(false);
        setNewPaymentStatus('');
        setPaymentNote('');
      }
    );
  };

  const handleUpdateShippingStatus = async () => {
    if (!newShippingStatus) return;
    await runUpdate(
      { trangThaiVanChuyen: newShippingStatus, ghiChuGiaoNhan: shippingNote.trim() || undefined },
      () => {
        setShowShippingModal(false);
        setNewShippingStatus('');
        setShippingNote('');
      }
    );
  };

  const handleCancel = async () => {
    if (!cancelReason.trim()) {
      alert('Vui lòng nhập lý do hủy đơn.');
      return;
    }
    setUpdating(true);
    try {
      await orderService.cancel(id, { reason: cancelReason.trim() });
      setShowCancelModal(false);
      setCancelReason('');
      await fetchOrder();
    } catch (err) {
      alert(err?.response?.data?.message || 'Hủy đơn hàng thất bại. Vui lòng thử lại.');
    } finally {
      setUpdating(false);
    }
  };

  if (loading) {
    return (
      <div className="content-wrapper">
        <section className="content">
          <div className="container-fluid">
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="sr-only">Đang tải...</span>
              </div>
            </div>
          </div>
        </section>
      </div>
    );
  }

  if (error) {
    return (
      <div className="content-wrapper">
        <section className="content">
          <div className="container-fluid">
            <div className="alert alert-danger mt-3">{error}</div>
            <button className="btn btn-default" onClick={() => navigate('/orders')}>
              <i className="fas fa-arrow-left"></i> Quay lại
            </button>
          </div>
        </section>
      </div>
    );
  }

  if (!order) return null;

  const items = order.chiTiet || order.items || [];
  const histories = order.lichSu || order.histories || order.orderHistories || [];
  const payments = order.danhSachThanhToan || order.thanhToan || [];
  const voucher = order.voucher || null;
  const inventoryHolds = order.tonKhoGiuCho || order.inventoryHolds || [];
  const totalAmount = order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? 0;
  const customerName = order.hoTenNhanHang || order.tenKhachHang || order.customerName;
  const address = order.diaChiNhanHang || order.diaChi || order.address;
  const phone = order.soDienThoaiNhanHang || order.soDienThoai || order.phone;
  const email = order.emailNhanHang || order.email;
  const actionsLocked = isLockedOrder(orderStatus);
  const orderCode = order.maDonHangKinhDoanh || order.maDonHang || order.orderCode || order.id;
  const installment = order.traGop || order.TraGop || null;
  const amountDue = Number(paymentInfo?.soTienCanThanhToan ?? paymentInfo?.SoTienCanThanhToan ?? 0);
  const canConfirmPayment = orderStatus !== 'Cancelled' && paymentStatus !== 'Paid' && amountDue > 0;
  const refunds = order.yeuCauHoanTien || order.YeuCauHoanTien || [];
  const pendingRefund = refunds.find((r) => r.trangThai === 'Pending');

  const handleConfirmRefund = async (refundId) => {
    const txnRef = window.prompt('Mã giao dịch hoàn tiền (tùy chọn):', '');
    if (txnRef === null) return; // cancelled
    const note = window.prompt('Ghi chú (tùy chọn):', '') || undefined;
    setUpdating(true);
    try {
      await orderService.confirmRefund(id, refundId, { maGiaoDichHoan: txnRef || undefined, ghiChuAdmin: note });
      await fetchOrder();
    } catch (err) {
      alert(err?.response?.data?.message || 'Xác nhận hoàn tiền thất bại.');
    } finally {
      setUpdating(false);
    }
  };

  const handlePrintOrder = () => {
    const rows = items.map((item, idx) => `
      <tr>
        <td>${idx + 1}</td>
        <td>${item.tenSanPhamSnapshot || item.tenSanPham || item.productName || '-'}</td>
        <td>${item.skuSnapshot || item.sku || '-'}</td>
        <td class="right">${formatCurrency(item.donGia || item.unitPrice || 0)}</td>
        <td class="right">${item.soLuong || item.quantity || 0}</td>
        <td class="right">${formatCurrency(item.thanhTien || item.subtotal || (item.donGia || item.unitPrice || 0) * (item.soLuong || item.quantity || 0))}</td>
      </tr>
    `).join('');

    const printWindow = window.open('', '_blank', 'width=900,height=700');
    if (!printWindow) return;
    printWindow.document.write(`
      <html>
        <head>
          <title>Phiếu đơn hàng ${orderCode}</title>
          <style>
            body { font-family: Arial, sans-serif; color: #222; padding: 24px; }
            h1 { font-size: 22px; margin: 0 0 6px; }
            h2 { font-size: 16px; margin: 24px 0 8px; }
            .muted { color: #666; }
            .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-top: 16px; }
            table { width: 100%; border-collapse: collapse; margin-top: 8px; }
            th, td { border: 1px solid #ddd; padding: 8px; font-size: 13px; text-align: left; }
            th { background: #f3f4f6; }
            .right { text-align: right; }
            .total { font-size: 18px; font-weight: 700; color: #0d6efd; }
            .signatures { display: grid; grid-template-columns: 1fr 1fr; gap: 60px; margin-top: 42px; text-align: center; }
            @media print { body { padding: 0; } }
          </style>
        </head>
        <body>
          <h1>MoToSale - Phiếu đơn hàng</h1>
          <div class="muted">Mã đơn: ${orderCode} | Ngày in: ${formatTimelineDate(new Date().toISOString())}</div>
          <div class="grid">
            <div>
              <h2>Thông tin đơn</h2>
              <div>Ngày tạo: ${formatDate(order.ngayTao || order.createdAt)}</div>
              <div>Trạng thái đơn: ${getOrderStatusMeta(orderStatus).label}</div>
              <div>Thanh toán: ${getPaymentStatusMeta(paymentStatus).label}</div>
              <div>Vận chuyển: ${getShippingStatusMeta(shippingStatus).label}</div>
            </div>
            <div>
              <h2>Khách hàng</h2>
              <div>Họ tên: ${customerName || '-'}</div>
              <div>SĐT: ${phone || '-'}</div>
              <div>Email: ${email || '-'}</div>
              <div>Địa chỉ: ${address || '-'}</div>
            </div>
          </div>
          <h2>Sản phẩm</h2>
          <table>
            <thead>
              <tr><th>#</th><th>Sản phẩm</th><th>SKU</th><th class="right">Đơn giá</th><th class="right">SL</th><th class="right">Thành tiền</th></tr>
            </thead>
            <tbody>${rows || '<tr><td colspan="6">Không có sản phẩm</td></tr>'}</tbody>
          </table>
          <p class="right total">Tổng thanh toán: ${formatCurrency(totalAmount)}</p>
          <div class="signatures">
            <div>Người lập phiếu<br><br><br>........................</div>
            <div>Khách hàng<br><br><br>........................</div>
          </div>
          <script>window.onload = () => { window.print(); };</script>
        </body>
      </html>
    `);
    printWindow.document.close();
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <button className="btn btn-default mb-2" onClick={() => navigate('/orders')}>
            <i className="fas fa-arrow-left"></i> Quay lại
          </button>
          <button className="btn btn-outline-primary mb-2 ml-2" onClick={handlePrintOrder}>
            <i className="fas fa-print"></i> In phiếu đơn hàng
          </button>
          <h1 className="m-0">Chi tiết đơn hàng</h1>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="row">
            <div className="col-md-6">
              <div className="card">
                <div className="card-header"><h3 className="card-title">Thông tin đơn hàng</h3></div>
                <div className="card-body">
                  <table className="table table-sm">
                    <tbody>
                      <tr><td><strong>Mã đơn:</strong></td><td>{order.maDonHangKinhDoanh || order.maDonHang || order.orderCode || order.id}</td></tr>
                      <tr><td><strong>Tổng tiền:</strong></td><td><strong className="text-primary">{formatCurrency(totalAmount)}</strong></td></tr>
                      <tr><td><strong>Ngày tạo:</strong></td><td>{formatDate(order.ngayTao || order.createdAt)}</td></tr>
                      <tr><td><strong>Ghi chú:</strong></td><td>{order.ghiChu || order.note || '-'}</td></tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>

            <div className="col-md-6">
              <div className="card">
                <div className="card-header"><h3 className="card-title">Thông tin khách hàng</h3></div>
                <div className="card-body">
                  <table className="table table-sm">
                    <tbody>
                      <tr><td><strong>Khách hàng:</strong></td><td>{customerName || '-'}</td></tr>
                      <tr><td><strong>Địa chỉ:</strong></td><td>{address || '-'}</td></tr>
                      <tr><td><strong>SĐT:</strong></td><td>{phone || '-'}</td></tr>
                      <tr><td><strong>Email:</strong></td><td>{email || '-'}</td></tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            <StatusCard
              title="Trạng thái đơn hàng"
              badge={renderBadge(getOrderStatusMeta(orderStatus))}
              buttonText="Cập nhật trạng thái đơn"
              disabled={actionsLocked || nextStatusOptions.length === 0}
              onClick={() => setShowStatusModal(true)}
            />
            <StatusCard
              title={`Thanh toán (${ORDER_TYPE_LABELS[order.loaiDonHang] || ORDER_TYPE_LABELS[order.orderType] || 'Đơn hàng'})`}
              badge={renderBadge(getPaymentStatusContextual(paymentStatus, order.loaiDonHang || order.orderType))}
              buttonText="Cập nhật thanh toán"
              disabled={orderStatus === 'Cancelled'}
              onClick={() => setShowPaymentModal(true)}
            />
            <StatusCard
              title="Vận chuyển"
              badge={renderBadge(getShippingStatusMeta(shippingStatus))}
              buttonText="Cập nhật vận chuyển"
              disabled={actionsLocked}
              onClick={() => setShowShippingModal(true)}
            />
          </div>

          {refunds.length > 0 && (
            <div className="card card-outline card-warning">
              <div className="card-header"><h3 className="card-title">Yêu cầu hoàn tiền của khách hàng</h3></div>
              <div className="card-body p-0">
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th>Ngày gửi</th>
                      <th className="table-col-money">Số tiền</th>
                      <th>Ngân hàng</th>
                      <th>Số tài khoản</th>
                      <th>Chủ tài khoản</th>
                      <th>Lý do</th>
                      <th>Trạng thái</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {refunds.map((r) => (
                      <tr key={r.maYeuCauHoanTien}>
                        <td>{formatDate(r.ngayTao)}</td>
                        <td className="table-col-money"><strong className="text-danger">{formatCurrency(r.soTien)}</strong></td>
                        <td>{r.tenNganHang}</td>
                        <td><code>{r.soTaiKhoan}</code></td>
                        <td>{r.chuTaiKhoan}</td>
                        <td>{r.lyDo || '-'}</td>
                        <td>
                          <span className={`badge badge-${r.trangThai === 'Completed' ? 'success' : r.trangThai === 'Rejected' ? 'secondary' : 'warning'}`}>
                            {r.trangThai === 'Completed' ? 'Đã hoàn tiền' : r.trangThai === 'Rejected' ? 'Từ chối' : 'Đang xử lý'}
                          </span>
                          {r.maGiaoDichHoan && <div className="small text-muted mt-1">GD: {r.maGiaoDichHoan}</div>}
                        </td>
                        <td>
                          {r.trangThai === 'Pending' && (
                            <button className="btn btn-sm btn-success" onClick={() => handleConfirmRefund(r.maYeuCauHoanTien)} disabled={updating}>
                              <i className="fas fa-check"></i> Đã hoàn tiền
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {pendingRefund && (
                <div className="card-footer text-muted small">
                  Sau khi chuyển khoản tổng <strong>{formatCurrency(pendingRefund.soTien)}</strong> đến tài khoản trên, bấm <em>Đã hoàn tiền</em> để cập nhật trạng thái đơn.
                </div>
              )}
            </div>
          )}

          {canConfirmPayment && (
            <div className="card card-outline card-success">
              <div className="card-header"><h3 className="card-title">Xác nhận thanh toán (chuyển khoản)</h3></div>
              <div className="card-body">
                <p className="mb-2">
                  Số tiền cần thu hiện tại: <strong className="text-primary">{formatCurrency(amountDue)}</strong>
                  {paymentInfo?.noiDungChuyenKhoan && <> — Nội dung CK: <strong>{paymentInfo.noiDungChuyenKhoan}</strong></>}
                </p>
                <p className="text-muted">Khi đã nhận được tiền chuyển khoản của khách, bấm nút bên dưới để xác nhận thanh toán{installment ? ' tiền trả trước' : ''}. Hệ thống sẽ trừ tồn kho và chuyển đơn sang "Đã xác nhận".</p>
                <button className="btn btn-success" onClick={() => handleConfirmPayment()} disabled={updating}>
                  <i className="fas fa-check"></i> {updating ? 'Đang xử lý...' : 'Đã nhận thanh toán'}
                </button>
              </div>
            </div>
          )}

          {installment && (
            <div className="card">
              <div className="card-header"><h3 className="card-title">Hồ sơ trả góp</h3></div>
              <div className="card-body">
                <table className="table table-sm">
                  <tbody>
                    <tr><td><strong>Trả trước:</strong></td><td>{formatCurrency(installment.tienTraTruoc)}</td><td><strong>Số kỳ:</strong></td><td>{installment.soKy} tháng</td></tr>
                    <tr><td><strong>Lãi suất:</strong></td><td>{installment.laiSuatNam}% /năm</td><td><strong>Tổng lãi:</strong></td><td>{formatCurrency(installment.tongTienLai)}</td></tr>
                    <tr><td><strong>Tổng phải trả (gốc + lãi):</strong></td><td colSpan="3">{formatCurrency(installment.tongPhaiTra)}</td></tr>
                  </tbody>
                </table>
                {installment.hoTenNguoiVay && (
                  <div className="mb-3 p-3 bg-light rounded">
                    <div className="d-flex justify-content-between align-items-center mb-2">
                      <strong>Hồ sơ vay</strong>
                      <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => printInstallmentApplication(order, installment)}>
                        <i className="fas fa-file-pdf"></i> Xuất PDF
                      </button>
                    </div>
                    <table className="table table-sm mb-0">
                      <tbody>
                        <tr><td><strong>Họ tên:</strong></td><td>{installment.hoTenNguoiVay}</td><td><strong>CCCD/CMND:</strong></td><td>{installment.soCCCD}</td></tr>
                        <tr><td><strong>Ngày cấp CCCD:</strong></td><td>{installment.ngayCapCCCD ? formatDate(installment.ngayCapCCCD) : '-'}</td><td><strong>Nơi cấp CCCD:</strong></td><td>{installment.noiCapCCCD || '-'}</td></tr>
                        <tr><td><strong>Ngày sinh:</strong></td><td>{installment.ngaySinh ? formatDate(installment.ngaySinh) : '-'}</td><td><strong>SĐT:</strong></td><td>{installment.soDienThoai || '-'}</td></tr>
                        <tr><td><strong>Địa chỉ TT:</strong></td><td colSpan="3">{installment.diaChiThuongTru || '-'}</td></tr>
                        <tr><td><strong>Nghề nghiệp:</strong></td><td>{installment.ngheNghiep || '-'}</td><td><strong>Công ty:</strong></td><td>{installment.tenCongTy || '-'}</td></tr>
                        <tr><td><strong>Thâm niên:</strong></td><td>{installment.thoiGianLamViecThang ? `${installment.thoiGianLamViecThang} tháng` : '-'}</td><td><strong>Thu nhập/tháng:</strong></td><td>{installment.thuNhapHangThang ? formatCurrency(installment.thuNhapHangThang) : '-'}</td></tr>
                      </tbody>
                    </table>
                  </div>
                )}
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr><th>Kỳ</th><th>Đến hạn</th><th className="table-col-money">Gốc</th><th className="table-col-money">Lãi</th><th className="table-col-money">Tổng</th><th>Trạng thái</th><th></th></tr>
                  </thead>
                  <tbody>
                    {(installment.terms || []).map((t) => (
                      <tr key={t.maKyTraGop}>
                        <td>Kỳ {t.kyThu}</td>
                        <td>{formatDate(t.ngayDenHan)}</td>
                        <td className="table-col-money">{formatCurrency(t.soTienGoc)}</td>
                        <td className="table-col-money">{formatCurrency(t.soTienLai)}</td>
                        <td className="table-col-money">{formatCurrency(t.tongTien)}</td>
                        <td>
                          <span className={`badge badge-${t.trangThai === 'Paid' ? 'success' : t.trangThai === 'Cancelled' ? 'secondary' : 'warning'}`}>
                            {t.trangThai === 'Paid' ? 'Đã trả' : t.trangThai === 'Cancelled' ? 'Đã hủy' : 'Chờ trả'}
                          </span>
                        </td>
                        <td>
                          {t.trangThai === 'Pending' && orderStatus !== 'Cancelled' && (
                            <button className="btn btn-sm btn-outline-success" onClick={() => handleConfirmPayment(t.maKyTraGop)} disabled={updating}>
                              Xác nhận
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          <div className="card">
            <div className="card-header"><h3 className="card-title">Lịch sử đơn hàng</h3></div>
            <div className="card-body">
              <OrderTimeline order={order} histories={histories} />
            </div>
          </div>

          <div className="card">
            <div className="card-header"><h3 className="card-title">Sản phẩm trong đơn</h3></div>
            <div className="card-body p-0">
              <table className="table table-bordered table-striped mb-0">
                <thead>
                  <tr>
                    <th className="table-col-code">#</th>
                    <th className="table-col-text">Sản phẩm</th>
                    <th className="table-col-code">SKU</th>
                    <th className="table-col-money">Đơn giá</th>
                    <th className="table-col-number">Số lượng</th>
                    <th className="table-col-money">Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length === 0 ? (
                    <tr><td colSpan="6" className="text-center text-muted">Không có sản phẩm</td></tr>
                  ) : (
                    items.map((item, idx) => (
                      <tr key={item.maChiTietDonHang || item.id || idx}>
                        <td className="table-col-code">{idx + 1}</td>
                        <td className="table-col-text">{item.tenSanPhamSnapshot || item.tenSanPham || item.productName || '-'}</td>
                        <td className="table-col-code">{item.skuSnapshot || item.sku || '-'}</td>
                        <td className="table-col-money">{formatCurrency(item.donGia || item.unitPrice || 0)}</td>
                        <td className="table-col-number">{item.soLuong || item.quantity || 0}</td>
                        <td className="table-col-money">{formatCurrency(item.thanhTien || item.subtotal || (item.donGia || item.unitPrice || 0) * (item.soLuong || item.quantity || 0))}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {payments.length > 0 && (
            <div className="card">
              <div className="card-header"><h3 className="card-title">Lịch sử thanh toán</h3></div>
              <div className="card-body p-0">
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th>Mã GD</th>
                      <th>Phương thức</th>
                      <th>Loại</th>
                      <th className="table-col-money">Số tiền</th>
                      <th>Trạng thái</th>
                      <th>Thời gian</th>
                    </tr>
                  </thead>
                  <tbody>
                    {payments.map((p, idx) => (
                      <tr key={p.maThanhToan || p.maThanhToanKinhDoanh || idx}>
                        <td>{p.maThanhToanKinhDoanh || p.maThanhToan}</td>
                        <td>{PAYMENT_METHODS[p.phuongThuc] || p.phuongThuc || '-'}</td>
                        <td>{p.loaiThanhToan || '-'}</td>
                        <td className="table-col-money">{formatCurrency(p.soTien || 0)}</td>
                        <td><span className={`badge badge-${p.trangThai === 'Paid' ? 'success' : p.trangThai === 'Cancelled' ? 'secondary' : p.trangThai === 'Failed' ? 'danger' : 'warning'}`}>{p.trangThai}</span></td>
                        <td>{formatDate(p.daThanhToanLuc || p.ngayTao)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {voucher && (
            <div className="card">
              <div className="card-header"><h3 className="card-title">Thông tin Voucher</h3></div>
              <div className="card-body">
                <table className="table table-sm">
                  <tbody>
                    <tr><td><strong>Mã voucher:</strong></td><td>{voucher.maVoucher || voucher.code || '-'}</td></tr>
                    <tr><td><strong>Giảm giá:</strong></td><td>{formatCurrency(voucher.giaTriGiam || voucher.discountAmount || 0)}</td></tr>
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {inventoryHolds.length > 0 && (
            <div className="card">
              <div className="card-header"><h3 className="card-title">Tồn kho giữ chỗ</h3></div>
              <div className="card-body p-0">
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th className="table-col-text">Sản phẩm</th>
                      <th className="table-col-text">Biến thể</th>
                      <th className="table-col-number">Số lượng giữ</th>
                      <th className="table-col-status">Trạng thái</th>
                      <th className="table-col-date">Hết hạn</th>
                    </tr>
                  </thead>
                  <tbody>
                    {inventoryHolds.map((hold, idx) => (
                      <tr key={hold.id || idx}>
                        <td className="table-col-text">{hold.tenSanPham || hold.productName || '-'}</td>
                        <td className="table-col-text">{hold.tenBienThe || hold.variantName || '-'}</td>
                        <td className="table-col-number">{hold.soLuong || hold.quantity || 0}</td>
                        <td className="table-col-status"><span className={`badge badge-${hold.trangThai === 'Active' || hold.status === 'Active' ? 'warning' : 'secondary'}`}>{hold.trangThai || hold.status || '-'}</span></td>
                        <td className="table-col-date">{formatDate(hold.hetHan || hold.expiresAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          <div className="mb-4">
            <button className="btn btn-danger" onClick={() => setShowCancelModal(true)} disabled={!canCancelOrder(orderStatus)}>
              <i className="fas fa-times"></i> Hủy đơn
            </button>
          </div>
        </div>
      </section>

      {showStatusModal && (
        <Modal title="Cập nhật trạng thái đơn hàng" onClose={() => setShowStatusModal(false)}>
          <div className="form-group">
            <label>Trạng thái hiện tại</label>
            <div>{renderBadge(getOrderStatusMeta(orderStatus))}</div>
          </div>
          <div className="form-group">
            <label>Trạng thái mới</label>
            <select className="form-control" value={newStatus} onChange={(e) => setNewStatus(e.target.value)}>
              <option value="">-- Chọn trạng thái --</option>
              {nextStatusOptions.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
            </select>
          </div>
          {newStatus === 'Cancelled' && (
            <div className="form-group">
              <label>Lý do hủy đơn <span className="text-danger">*</span></label>
              <textarea className="form-control" rows="3" value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} />
            </div>
          )}
          {newStatus === 'Completed' && (
            <div className="alert alert-warning">Hoàn tất đơn sẽ khóa các thao tác sửa/hủy thông thường.</div>
          )}
          <ModalFooter onClose={() => setShowStatusModal(false)} onSubmit={handleUpdateStatus} disabled={updating || !newStatus} loading={updating} submitText="Cập nhật" />
        </Modal>
      )}

      {showPaymentModal && (
        <Modal title="Cập nhật thanh toán thủ công" onClose={() => setShowPaymentModal(false)}>
          <div className="form-group">
            <label>Trạng thái thanh toán</label>
            <select className="form-control" value={newPaymentStatus} onChange={(e) => setNewPaymentStatus(e.target.value)}>
              <option value="">-- Chọn trạng thái --</option>
              {PAYMENT_STATUS_OPTIONS.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>{newPaymentStatus === 'Refunded' ? 'Lý do hoàn tiền *' : 'Ghi chú thanh toán'}</label>
            <textarea className="form-control" rows="3" value={paymentNote} onChange={(e) => setPaymentNote(e.target.value)} />
          </div>
          <ModalFooter onClose={() => setShowPaymentModal(false)} onSubmit={handleUpdatePaymentStatus} disabled={updating || !newPaymentStatus} loading={updating} submitText="Cập nhật" />
        </Modal>
      )}

      {showShippingModal && (
        <Modal title="Cập nhật vận chuyển" onClose={() => setShowShippingModal(false)}>
          <div className="form-group">
            <label>Trạng thái vận chuyển</label>
            <select className="form-control" value={newShippingStatus} onChange={(e) => setNewShippingStatus(e.target.value)}>
              <option value="">-- Chọn trạng thái --</option>
              {shippingOptions.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>Ghi chú giao nhận</label>
            <textarea className="form-control" rows="3" value={shippingNote} onChange={(e) => setShippingNote(e.target.value)} />
          </div>
          <ModalFooter onClose={() => setShowShippingModal(false)} onSubmit={handleUpdateShippingStatus} disabled={updating || !newShippingStatus} loading={updating} submitText="Cập nhật" />
        </Modal>
      )}

      {showCancelModal && (
        <Modal title="Hủy đơn hàng" onClose={() => setShowCancelModal(false)}>
          <div className="form-group">
            <label>Lý do hủy đơn <span className="text-danger">*</span></label>
            <textarea className="form-control" rows="3" value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} />
          </div>
          <ModalFooter onClose={() => setShowCancelModal(false)} onSubmit={handleCancel} disabled={updating} loading={updating} submitText="Xác nhận hủy" danger />
        </Modal>
      )}
    </div>
  );
};

const StatusCard = ({ title, badge, buttonText, disabled, onClick }) => (
  <div className="col-md-4">
    <div className="card">
      <div className="card-header"><h3 className="card-title">{title}</h3></div>
      <div className="card-body">
        <div className="mb-3">{badge}</div>
        <button className="btn btn-primary btn-sm" disabled={disabled} onClick={onClick}>
          <i className="fas fa-edit"></i> {buttonText}
        </button>
      </div>
    </div>
  </div>
);

const getHistoryValueLabel = (eventType, value) => {
  if (!value) return '-';
  if (eventType === 'OrderStatus') return getOrderStatusMeta(value).label;
  if (eventType === 'PaymentStatus') return getPaymentStatusMeta(value).label;
  if (eventType === 'ShippingStatus') return getShippingStatusMeta(value).label;
  return value;
};

const OrderTimeline = ({ order, histories }) => {
  const createdAt = order.ngayTao || order.createdAt;
  const items = [
    {
      id: 'created',
      loaiSuKien: 'Created',
      giaTriCu: null,
      giaTriMoi: order.trangThaiDonHang || order.status || 'AwaitingPayment',
      ghiChu: 'Đơn hàng được tạo',
      thoiGian: createdAt,
    },
    ...histories.map((item) => ({
      id: item.maLichSuDonHang || item.id,
      loaiSuKien: item.loaiSuKien || item.eventType,
      giaTriCu: item.giaTriCu ?? item.oldValue,
      giaTriMoi: item.giaTriMoi ?? item.newValue,
      ghiChu: item.ghiChu || item.note,
      maNguoiThucHien: item.maNguoiThucHien || item.actorUserId,
      thoiGian: item.thoiGian || item.createdAt,
    })),
  ].filter((item) => item.thoiGian)
    .sort((a, b) => getTimelineTimestamp(a.thoiGian) - getTimelineTimestamp(b.thoiGian));

  if (items.length === 0) {
    return <div className="text-muted">Chưa có lịch sử đơn hàng.</div>;
  }

  return (
    <div className="order-timeline">
      {items.map((item, index) => (
        <div
          key={`${item.id || index}-${item.loaiSuKien}`}
          className="d-flex align-items-start mb-3"
          style={{ gap: 14 }}
        >
          <div className="text-center flex-shrink-0" style={{ width: 34 }}>
            <span
              className="badge badge-primary rounded-circle d-inline-flex align-items-center justify-content-center"
              style={{ width: 28, height: 28, fontSize: 13 }}
            >
              {index + 1}
            </span>
          </div>
          <div className="flex-fill border-bottom pb-3" style={{ minWidth: 0 }}>
            <strong className="d-block">
              {EVENT_LABELS[item.loaiSuKien] || item.loaiSuKien || 'Sự kiện'}
            </strong>
            <div className="text-muted small mt-1">{formatTimelineDate(item.thoiGian)}</div>
            {item.loaiSuKien === 'Created' ? (
              <div className="mt-1">{item.ghiChu}</div>
            ) : (
              <div className="mt-1 d-flex flex-wrap align-items-center" style={{ gap: 8 }}>
                <span className="text-muted">Từ:</span> {getHistoryValueLabel(item.loaiSuKien, item.giaTriCu)}
                <span>→</span>
                <span className="text-muted">Sang:</span> {getHistoryValueLabel(item.loaiSuKien, item.giaTriMoi)}
              </div>
            )}
            {item.ghiChu && item.loaiSuKien !== 'Created' && (
              <div className="text-muted small mt-1">Ghi chú: {item.ghiChu}</div>
            )}
            {item.maNguoiThucHien && (
              <div className="text-muted small mt-1">Người thực hiện: #{item.maNguoiThucHien}</div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

const Modal = ({ title, onClose, children }) => (
  <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
    <div className="modal-dialog" style={{ maxHeight: '90vh' }}>
      <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
        <div className="modal-header">
          <h5 className="modal-title">{title}</h5>
          <button type="button" className="close" onClick={onClose}><span>&times;</span></button>
        </div>
        <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>{children}</div>
      </div>
    </div>
  </div>
);

const ModalFooter = ({ onClose, onSubmit, disabled, loading, submitText, danger = false }) => (
  <div className="modal-footer">
    <button className="btn btn-default" onClick={onClose}>Đóng</button>
    <button className={`btn btn-${danger ? 'danger' : 'primary'}`} onClick={onSubmit} disabled={disabled}>
      {loading ? 'Đang xử lý...' : submitText}
    </button>
  </div>
);

export default OrderDetail;
