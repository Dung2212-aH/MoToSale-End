import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import orderService from '../../services/orderService';
import { ORDER_STATUS, PAYMENT_STATUS, PAYMENT_METHODS } from '../../utils/constants';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';

const OrderDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [newStatus, setNewStatus] = useState('');
  const [newPaymentStatus, setNewPaymentStatus] = useState('');
  const [cancelReason, setCancelReason] = useState('');
  const [updating, setUpdating] = useState(false);

  const fetchOrder = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await orderService.getById(id);
      setOrder(res.data);
    } catch (err) {
      setError('Không thể tải thông tin đơn hàng.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrder();
  }, [id]);

  const handleUpdateStatus = async () => {
    if (!newStatus) return;
    setUpdating(true);
    try {
      await orderService.updateStatus(id, { status: newStatus });
      setShowStatusModal(false);
      setNewStatus('');
      fetchOrder();
    } catch (err) {
      alert('Cập nhật trạng thái thất bại. Vui lòng thử lại.');
    } finally {
      setUpdating(false);
    }
  };

  const handleUpdatePaymentStatus = async () => {
    if (!newPaymentStatus) return;
    setUpdating(true);
    try {
      await orderService.updateStatus(id, { paymentStatus: newPaymentStatus });
      setShowPaymentModal(false);
      setNewPaymentStatus('');
      fetchOrder();
    } catch (err) {
      alert('Cập nhật thanh toán thất bại. Vui lòng thử lại.');
    } finally {
      setUpdating(false);
    }
  };

  const handleCancel = async () => {
    if (!cancelReason.trim()) {
      alert('Vui lòng nhập lý do hủy đơn.');
      return;
    }
    setUpdating(true);
    try {
      await orderService.cancel(id, { reason: cancelReason });
      setShowCancelModal(false);
      setCancelReason('');
      fetchOrder();
    } catch (err) {
      alert('Hủy đơn hàng thất bại. Vui lòng thử lại.');
    } finally {
      setUpdating(false);
    }
  };

  const getStatusBadge = (status) => {
    const s = ORDER_STATUS[status];
    if (!s) return <span className="badge badge-secondary">{status}</span>;
    return <span className={`badge badge-${s.color}`}>{s.label}</span>;
  };

  const getPaymentStatusBadge = (status) => {
    const s = PAYMENT_STATUS[status];
    if (!s) return <span className="badge badge-secondary">{status}</span>;
    return <span className={`badge badge-${s.color}`}>{s.label}</span>;
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
  const payment = order.thanhToan || order.payment || null;
  const voucher = order.voucher || null;
  const inventoryHolds = order.tonKhoGiuCho || order.inventoryHolds || [];
  const orderStatus = order.trangThaiDonHang || order.trangThai || order.status;
  const totalAmount = order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? 0;
  const customerName = order.hoTenNhanHang || order.tenKhachHang || order.customerName;
  const address = order.diaChiNhanHang || order.diaChi || order.address;
  const phone = order.soDienThoaiNhanHang || order.soDienThoai || order.phone;
  const email = order.emailNhanHang || order.email;
  const paymentStatus = orderStatus === 'Cancelled'
    ? 'Cancelled'
    : (payment?.trangThai || payment?.status || order.trangThaiThanhToan || order.paymentStatus);

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <button className="btn btn-default mb-2" onClick={() => navigate('/orders')}>
                <i className="fas fa-arrow-left"></i> Quay lại
              </button>
              <h1 className="m-0">Chi tiết đơn hàng</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {/* Order Info */}
          <div className="row">
            <div className="col-md-6">
              <div className="card">
                <div className="card-header">
                  <h3 className="card-title">Thông tin đơn hàng</h3>
                </div>
                <div className="card-body">
                  <table className="table table-sm">
                    <tbody>
                      <tr>
                        <td><strong>Mã đơn:</strong></td>
                        <td>{order.maDonHang || order.orderCode || order.id}</td>
                      </tr>
                      <tr>
                        <td><strong>Trạng thái:</strong></td>
                        <td>{getStatusBadge(orderStatus)}</td>
                      </tr>
                      <tr>
                        <td><strong>Tổng tiền:</strong></td>
                        <td><strong className="text-primary">{formatCurrency(totalAmount)}</strong></td>
                      </tr>
                      <tr>
                        <td><strong>Ngày tạo:</strong></td>
                        <td>{formatDate(order.ngayTao || order.createdAt)}</td>
                      </tr>
                      <tr>
                        <td><strong>Ghi chú:</strong></td>
                        <td>{order.ghiChu || order.note || '—'}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>

            <div className="col-md-6">
              <div className="card">
                <div className="card-header">
                  <h3 className="card-title">Thông tin khách hàng</h3>
                </div>
                <div className="card-body">
                  <table className="table table-sm">
                    <tbody>
                      <tr>
                        <td><strong>Khách hàng:</strong></td>
                        <td>{customerName || '—'}</td>
                      </tr>
                      <tr>
                        <td><strong>Địa chỉ:</strong></td>
                        <td>{address || '—'}</td>
                      </tr>
                      <tr>
                        <td><strong>SĐT:</strong></td>
                        <td>{phone || '—'}</td>
                      </tr>
                      <tr>
                        <td><strong>Email:</strong></td>
                        <td>{email || '—'}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>

          {/* Order Items */}
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Sản phẩm trong đơn</h3>
            </div>
            <div className="card-body p-0">
              <table className="table table-bordered table-striped mb-0">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Sản phẩm</th>
                    <th>SKU</th>
                    <th>Đơn giá</th>
                    <th>Số lượng</th>
                    <th>Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length === 0 ? (
                    <tr>
                      <td colSpan="6" className="text-center text-muted">Không có sản phẩm</td>
                    </tr>
                  ) : (
                    items.map((item, idx) => (
                      <tr key={item.maChiTietDonHang || item.id || idx}>
                        <td>{idx + 1}</td>
                        <td>{item.tenSanPhamSnapshot || item.tenSanPham || item.productName || '—'}</td>
                        <td>{item.skuSnapshot || item.sku || '—'}</td>
                        <td>{formatCurrency(item.donGia || item.unitPrice || 0)}</td>
                        <td>{item.soLuong || item.quantity || 0}</td>
                        <td>{formatCurrency(item.thanhTien || item.subtotal || (item.donGia || item.unitPrice || 0) * (item.soLuong || item.quantity || 0))}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Payment Info */}
          {payment && (
            <div className="card">
              <div className="card-header">
                <h3 className="card-title">Thông tin thanh toán</h3>
              </div>
              <div className="card-body">
                <table className="table table-sm">
                  <tbody>
                    <tr>
                      <td><strong>Phương thức:</strong></td>
                      <td>{PAYMENT_METHODS[payment.phuongThuc || payment.method] || payment.phuongThuc || payment.method || '—'}</td>
                    </tr>
                    <tr>
                      <td><strong>Số tiền:</strong></td>
                      <td>{formatCurrency(payment.soTien || payment.amount)}</td>
                    </tr>
                    <tr>
                      <td><strong>Trạng thái:</strong></td>
                      <td>{getPaymentStatusBadge(paymentStatus)}</td>
                    </tr>
                    <tr>
                      <td><strong>Ngày thanh toán:</strong></td>
                      <td>{formatDate(payment.ngayThanhToan || payment.paidAt)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Voucher Info */}
          {voucher && (
            <div className="card">
              <div className="card-header">
                <h3 className="card-title">Thông tin Voucher</h3>
              </div>
              <div className="card-body">
                <table className="table table-sm">
                  <tbody>
                    <tr>
                      <td><strong>Mã voucher:</strong></td>
                      <td>{voucher.maVoucher || voucher.code || '—'}</td>
                    </tr>
                    <tr>
                      <td><strong>Giảm giá:</strong></td>
                      <td>{formatCurrency(voucher.giaTriGiam || voucher.discountAmount || 0)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Inventory Holds */}
          {inventoryHolds.length > 0 && (
            <div className="card">
              <div className="card-header">
                <h3 className="card-title">Tồn kho giữ chỗ</h3>
              </div>
              <div className="card-body p-0">
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th>Sản phẩm</th>
                      <th>Biến thể</th>
                      <th>Số lượng giữ</th>
                      <th>Trạng thái</th>
                      <th>Hết hạn</th>
                    </tr>
                  </thead>
                  <tbody>
                    {inventoryHolds.map((hold, idx) => (
                      <tr key={hold.id || idx}>
                        <td>{hold.tenSanPham || hold.productName || '—'}</td>
                        <td>{hold.tenBienThe || hold.variantName || '—'}</td>
                        <td>{hold.soLuong || hold.quantity || 0}</td>
                        <td>
                          <span className={`badge badge-${hold.trangThai === 'Active' || hold.status === 'Active' ? 'warning' : 'secondary'}`}>
                            {hold.trangThai || hold.status || '—'}
                          </span>
                        </td>
                        <td>{formatDate(hold.hetHan || hold.expiresAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="mb-4">
            <button
              className="btn btn-primary mr-2"
              onClick={() => setShowStatusModal(true)}
              disabled={orderStatus === 'Cancelled' || orderStatus === 'Delivered'}
            >
              <i className="fas fa-edit"></i> Cập nhật trạng thái
            </button>
            <button
              className="btn btn-success mr-2"
              onClick={() => setShowPaymentModal(true)}
              disabled={orderStatus === 'Cancelled'}
            >
              <i className="fas fa-money-bill-wave"></i> Xác nhận thanh toán
            </button>
            <button
              className="btn btn-danger"
              onClick={() => setShowCancelModal(true)}
              disabled={orderStatus === 'Cancelled' || orderStatus === 'Delivered'}
            >
              <i className="fas fa-times"></i> Hủy đơn
            </button>
          </div>
        </div>
      </section>

      {/* Update Status Modal */}
      {showStatusModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog" style={{ maxHeight: '90vh' }}>
            <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
              <div className="modal-header">
                <h5 className="modal-title">Cập nhật trạng thái đơn hàng</h5>
                <button type="button" className="close" onClick={() => setShowStatusModal(false)}>
                  <span>&times;</span>
                </button>
              </div>
              <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                <div className="form-group">
                  <label>Trạng thái mới</label>
                  <select
                    className="form-control"
                    value={newStatus}
                    onChange={(e) => setNewStatus(e.target.value)}
                  >
                    <option value="">-- Chọn trạng thái --</option>
                    {Object.entries(ORDER_STATUS).map(([key, val]) => (
                      <option key={key} value={key}>{val.label}</option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-default" onClick={() => setShowStatusModal(false)}>Đóng</button>
                <button className="btn btn-primary" onClick={handleUpdateStatus} disabled={updating || !newStatus}>
                  {updating ? 'Đang cập nhật...' : 'Cập nhật'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Cancel Modal */}
      {showCancelModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog" style={{ maxHeight: '90vh' }}>
            <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
              <div className="modal-header">
                <h5 className="modal-title">Hủy đơn hàng</h5>
                <button type="button" className="close" onClick={() => setShowCancelModal(false)}>
                  <span>&times;</span>
                </button>
              </div>
              <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                <div className="form-group">
                  <label>Lý do hủy đơn <span className="text-danger">*</span></label>
                  <textarea
                    className="form-control"
                    rows="3"
                    placeholder="Nhập lý do hủy đơn hàng..."
                    value={cancelReason}
                    onChange={(e) => setCancelReason(e.target.value)}
                  ></textarea>
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-default" onClick={() => setShowCancelModal(false)}>Đóng</button>
                <button className="btn btn-danger" onClick={handleCancel} disabled={updating}>
                  {updating ? 'Đang xử lý...' : 'Xác nhận hủy'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Update Payment Modal */}
      {showPaymentModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog" style={{ maxHeight: '90vh' }}>
            <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
              <div className="modal-header">
                <h5 className="modal-title">Cập nhật thanh toán thủ công</h5>
                <button type="button" className="close" onClick={() => setShowPaymentModal(false)}>
                  <span>&times;</span>
                </button>
              </div>
              <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                <div className="form-group">
                  <label>Trạng thái thanh toán mới</label>
                  <select
                    className="form-control"
                    value={newPaymentStatus}
                    onChange={(e) => setNewPaymentStatus(e.target.value)}
                  >
                    <option value="">-- Chọn trạng thái --</option>
                    <option value="Paid">Đã thanh toán</option>
                    <option value="Unpaid">Chưa thanh toán</option>
                    <option value="PartiallyPaid">Thanh toán một phần</option>
                    <option value="Failed">Thất bại</option>
                    <option value="Cancelled">Đã hủy</option>
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-default" onClick={() => setShowPaymentModal(false)}>Đóng</button>
                <button className="btn btn-success" onClick={handleUpdatePaymentStatus} disabled={updating || !newPaymentStatus}>
                  {updating ? 'Đang cập nhật...' : 'Xác nhận'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrderDetail;
