import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import orderService from '../../services/orderService';
import { ORDER_STATUS, PAYMENT_STATUS } from '../../utils/constants';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';

const OrderList = () => {
  const navigate = useNavigate();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  const fetchOrders = async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (statusFilter) params.status = statusFilter;
      if (typeFilter) params.type = typeFilter;
      const res = await orderService.getAll(params);
      const data = res.data;
      setOrders(data.items || data.data || data || []);
      setTotalPages(data.totalPages || Math.ceil((data.total || 0) / pageSize) || 1);
    } catch (err) {
      setError('Không thể tải danh sách đơn hàng. Vui lòng thử lại.');
      setOrders([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, [page, statusFilter, typeFilter]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchOrders();
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

  const getOrderStatusValue = (order) => order.trangThaiDonHang || order.trangThai || order.status;

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Đơn hàng</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách đơn hàng</h3>
            </div>
            <div className="card-body">
              {/* Filters */}
              <div className="row mb-3">
                <div className="col-md-4">
                  <form onSubmit={handleSearch}>
                    <div className="input-group">
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Tìm theo mã đơn hàng..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                      />
                      <div className="input-group-append">
                        <button className="btn btn-default" type="submit">
                          <i className="fas fa-search"></i>
                        </button>
                      </div>
                    </div>
                  </form>
                </div>
                <div className="col-md-3">
                  <select
                    className="form-control"
                    value={statusFilter}
                    onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
                  >
                    <option value="">-- Trạng thái đơn --</option>
                    {Object.entries(ORDER_STATUS).map(([key, val]) => (
                      <option key={key} value={key}>{val.label}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-3">
                  <select
                    className="form-control"
                    value={typeFilter}
                    onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
                  >
                    <option value="">-- Loại đơn hàng --</option>
                    <option value="Online">Online</option>
                    <option value="InStore">Tại cửa hàng</option>
                  </select>
                </div>
              </div>

              {/* Error */}
              {error && (
                <div className="alert alert-danger">{error}</div>
              )}

              {/* Loading */}
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
                          <th>Mã đơn</th>
                          <th>Khách hàng</th>
                          <th>Tổng tiền</th>
                          <th>Trạng thái đơn</th>
                          <th>Trạng thái TT</th>
                          <th>Ngày tạo</th>
                          <th>Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {orders.map((order) => (
                          <tr key={order.id || order.maDonHang}>
                            <td><strong>{order.maDonHang || order.orderCode || order.id}</strong></td>
                            <td>{order.tenKhachHang || order.customerName || '—'}</td>
                            <td>{formatCurrency(order.tongTien || order.totalAmount || 0)}</td>
                            <td>{getStatusBadge(getOrderStatusValue(order))}</td>
                            <td>{getPaymentStatusBadge(
                              getOrderStatusValue(order) === 'Cancelled'
                                ? 'Cancelled'
                                : (order.trangThaiThanhToan || order.paymentStatus)
                            )}</td>
                            <td>{formatDate(order.ngayTao || order.createdAt)}</td>
                            <td>
                              <button
                                className="btn btn-info btn-sm"
                                onClick={() => navigate(`/orders/${order.id || order.maDonHang}`)}
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

                  {/* Pagination */}
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
