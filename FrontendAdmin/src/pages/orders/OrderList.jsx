import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import orderService from '../../services/orderService';
import {
  ORDER_STATUS_OPTIONS,
  PAYMENT_STATUS_OPTIONS,
  SHIPPING_STATUS_OPTIONS,
  getOrderStatusMeta,
  getPaymentStatusMeta,
  getShippingStatusMeta,
} from '../../utils/constants';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';
import { createDateStamp, exportWorkbook } from '../../utils/exportExcel';
import { fetchAllPages } from '../../utils/fetchAllPages';

const pageSize = 10;

const OrderList = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [keyword, setKeyword] = useState(searchParams.get('keyword') || '');
  const [orderStatus, setOrderStatus] = useState(searchParams.get('trangThaiDonHang') || '');
  const [paymentStatus, setPaymentStatus] = useState(searchParams.get('trangThaiThanhToan') || '');
  const [shippingStatus, setShippingStatus] = useState(searchParams.get('trangThaiVanChuyen') || '');
  const [page, setPage] = useState(Number(searchParams.get('page')) || 1);
  const [totalPages, setTotalPages] = useState(1);
  const [exporting, setExporting] = useState(false);

  useEffect(() => {
    const params = new URLSearchParams();
    if (keyword.trim()) params.set('keyword', keyword.trim());
    if (orderStatus) params.set('trangThaiDonHang', orderStatus);
    if (paymentStatus) params.set('trangThaiThanhToan', paymentStatus);
    if (shippingStatus) params.set('trangThaiVanChuyen', shippingStatus);
    if (page > 1) params.set('page', String(page));
    setSearchParams(params, { replace: true });
  }, [keyword, orderStatus, paymentStatus, shippingStatus, page, setSearchParams]);

  useEffect(() => {
    const fetchOrders = async () => {
      setLoading(true);
      setError('');
      try {
        const params = { page, pageSize };
        if (keyword.trim()) params.keyword = keyword.trim();
        if (orderStatus) params.trangThaiDonHang = orderStatus;
        if (paymentStatus) params.trangThaiThanhToan = paymentStatus;
        if (shippingStatus) params.trangThaiVanChuyen = shippingStatus;

        const res = await orderService.getAll(params);
        const data = res.data;
        const totalItems = data.totalItems ?? data.total ?? 0;
        setOrders(data.items || data.data || data || []);
        setTotalPages(data.totalPages || Math.ceil(totalItems / pageSize) || 1);
      } catch (err) {
        setError('Không thể tải danh sách đơn hàng. Vui lòng thử lại.');
        setOrders([]);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, [page, keyword, orderStatus, paymentStatus, shippingStatus]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
  };

  const renderBadge = (meta) => <span className={`badge badge-${meta.color}`}>{meta.label}</span>;

  const getCustomerName = (order) => (
    order.hoTenNhanHang
    || order.tenNguoiNhan
    || order.tenKhachHang
    || order.customerName
    || order.hoTen
    || order.fullName
    || '-'
  );

  const getOrderId = (order) => order.maDonHang || order.orderCode || order.id;
  const getOrderCode = (order) => order.maDonHangKinhDoanh || order.maDonHang || order.orderCode || order.id;
  const getOrderAmount = (order) => order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? 0;

  const buildFilterParams = (extra = {}) => {
    const params = { ...extra };
    if (keyword.trim()) params.keyword = keyword.trim();
    if (orderStatus) params.trangThaiDonHang = orderStatus;
    if (paymentStatus) params.trangThaiThanhToan = paymentStatus;
    if (shippingStatus) params.trangThaiVanChuyen = shippingStatus;
    return params;
  };

  const handleExportExcel = async () => {
    setExporting(true);
    try {
      // Server clamp pageSize về 100 → phải gọi theo trang để lấy ĐỦ đơn theo bộ lọc
      const { items: exportRows } = await fetchAllPages(
        (params) => orderService.getAll(params),
        buildFilterParams()
      );

      await exportWorkbook({
        fileName: `don-hang-${createDateStamp()}.xlsx`,
        sheets: [
          {
            name: 'HuongDan',
            columns: [
              { header: 'Mục', key: 'label', width: 30 },
              { header: 'Nội dung', key: 'value', width: 90 },
            ],
            rows: [
              { label: 'Tên báo cáo', value: 'Danh sách đơn hàng theo bộ lọc hiện tại' },
              { label: 'Mục đích', value: 'Dùng để đối soát đơn hàng, chăm sóc khách hàng, kiểm tra thanh toán và vận chuyển.' },
              { label: 'Từ khóa', value: keyword.trim() || 'Không lọc' },
              { label: 'Trạng thái đơn', value: orderStatus ? getOrderStatusMeta(orderStatus).label : 'Tất cả' },
              { label: 'Thanh toán', value: paymentStatus ? getPaymentStatusMeta(paymentStatus).label : 'Tất cả' },
              { label: 'Vận chuyển', value: shippingStatus ? getShippingStatusMeta(shippingStatus).label : 'Tất cả' },
              { label: 'Sheet DonHang', value: 'Mỗi dòng là một đơn hàng, gồm khách hàng, ngày tạo, trạng thái đơn, thanh toán, vận chuyển và tổng tiền.' },
              { label: 'Lưu ý doanh thu', value: 'File này là danh sách đơn để đối soát, không thay thế báo cáo doanh thu.' },
            ],
          },
          {
            name: 'DonHang',
            columns: [
              { header: 'Mã đơn', key: 'orderCode', width: 18 },
              { header: 'Khách hàng', key: 'customerName', width: 28 },
              { header: 'SĐT', key: 'phone', width: 16 },
              { header: 'Email', key: 'email', width: 26 },
              { header: 'Ngày tạo', key: 'createdAt', type: 'date', width: 20 },
              { header: 'Trạng thái đơn', key: 'orderStatus', width: 24 },
              { header: 'Thanh toán', key: 'paymentStatus', width: 24 },
              { header: 'Vận chuyển', key: 'shippingStatus', width: 24 },
              { header: 'Tổng tiền', key: 'amount', type: 'currency', width: 18 },
            ],
            rows: exportRows.map((order) => ({
              orderCode: getOrderCode(order),
              customerName: getCustomerName(order),
              phone: order.soDienThoaiNhanHang || order.soDienThoai || order.phone || '',
              email: order.emailNhanHang || order.email || '',
              createdAt: order.ngayTao || order.createdAt,
              orderStatus: getOrderStatusMeta(order.trangThaiDonHang || order.trangThai || order.status).label,
              paymentStatus: getPaymentStatusMeta(order.trangThaiThanhToan || order.paymentStatus).label,
              shippingStatus: getShippingStatusMeta(order.trangThaiVanChuyen || order.shippingStatus).label,
              amount: getOrderAmount(order),
            })),
          },
        ],
      });
    } catch (err) {
      alert('Xuất Excel đơn hàng thất bại. Vui lòng thử lại.');
    } finally {
      setExporting(false);
    }
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý đơn hàng</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách đơn hàng</h3>
              <div className="card-tools">
                <button
                  className="btn btn-success btn-sm"
                  onClick={handleExportExcel}
                  disabled={exporting || loading}
                  title="Xuất danh sách đơn hàng theo bộ lọc hiện tại để đối soát khách hàng, thanh toán và vận chuyển"
                >
                  <i className="fas fa-file-excel"></i>{exporting ? ' Đang xuất...' : ' Xuất danh sách đơn'}
                </button>
              </div>
            </div>
            <div className="card-body">
              <div className="row mb-3">
                <div className="col-md-4">
                  <form onSubmit={handleSearch}>
                    <div className="input-group">
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Mã đơn, tên, SĐT, email..."
                        value={keyword}
                        onChange={(e) => { setKeyword(e.target.value); setPage(1); }}
                      />
                      <div className="input-group-append">
                        <button className="btn btn-default" type="submit">
                          <i className="fas fa-search"></i>
                        </button>
                      </div>
                    </div>
                  </form>
                </div>
                <div className="col-md-2">
                  <select className="form-control" value={orderStatus} onChange={(e) => { setOrderStatus(e.target.value); setPage(1); }}>
                    <option value="">Trạng thái đơn</option>
                    {ORDER_STATUS_OPTIONS.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                  </select>
                </div>
                <div className="col-md-3">
                  <select className="form-control" value={paymentStatus} onChange={(e) => { setPaymentStatus(e.target.value); setPage(1); }}>
                    <option value="">Thanh toán</option>
                    {PAYMENT_STATUS_OPTIONS.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                  </select>
                </div>
                <div className="col-md-3">
                  <select className="form-control" value={shippingStatus} onChange={(e) => { setShippingStatus(e.target.value); setPage(1); }}>
                    <option value="">Vận chuyển</option>
                    {SHIPPING_STATUS_OPTIONS.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                  </select>
                </div>
              </div>

              {error && <div className="alert alert-danger">{error}</div>}

              {loading ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Đang tải...</span>
                  </div>
                </div>
              ) : orders.length === 0 ? (
                <div className="text-center py-4">
                  <i className="fas fa-inbox fa-3x text-muted mb-3"></i>
                  <p className="text-muted">Không có đơn hàng nào.</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped">
                      <thead>
                        <tr>
                          <th className="table-col-code">Mã đơn</th>
                          <th className="table-col-text">Khách hàng</th>
                          <th className="table-col-money">Tổng tiền</th>
                          <th className="table-col-status">Trạng thái đơn</th>
                          <th className="table-col-status">Thanh toán</th>
                          <th className="table-col-status">Vận chuyển</th>
                          <th className="table-col-date">Ngày tạo</th>
                          <th className="table-col-actions">Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {orders.map((order) => (
                          <tr key={order.maDonHang || order.id}>
                            <td className="table-col-code"><strong>{getOrderId(order)}</strong></td>
                            <td className="table-col-text">{getCustomerName(order)}</td>
                            <td className="table-col-money">{formatCurrency(order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? 0)}</td>
                            <td className="table-col-status">{renderBadge(getOrderStatusMeta(order.trangThaiDonHang || order.trangThai || order.status))}</td>
                            <td className="table-col-status">{renderBadge(getPaymentStatusMeta(order.trangThaiThanhToan || order.paymentStatus))}</td>
                            <td className="table-col-status">{renderBadge(getShippingStatusMeta(order.trangThaiVanChuyen || order.shippingStatus))}</td>
                            <td className="table-col-date">{formatDate(order.ngayTao || order.createdAt)}</td>
                            <td className="table-col-actions">
                              <button
                                className="btn btn-info btn-sm"
                                onClick={() => navigate(`/orders/${order.maDonHang || order.id}`)}
                                title="Xem chi tiết"
                              >
                                <i className="fas fa-eye"></i> Chi tiết
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {totalPages > 1 && (
                    <nav className="mt-3">
                      <ul className="pagination justify-content-center">
                        <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}>
                          <button className="page-link" onClick={() => setPage(page - 1)}>«</button>
                        </li>
                        {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                          <li key={p} className={`page-item ${p === page ? 'active' : ''}`}>
                            <button className="page-link" onClick={() => setPage(p)}>{p}</button>
                          </li>
                        ))}
                        <li className={`page-item ${page >= totalPages ? 'disabled' : ''}`}>
                          <button className="page-link" onClick={() => setPage(page + 1)}>»</button>
                        </li>
                      </ul>
                    </nav>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default OrderList;
