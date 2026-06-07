import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import orderService from '../../services/orderService';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';

const STATUS_TABS = [
  { key: 'Pending', label: 'Chờ duyệt', color: 'warning' },
  { key: 'Paid', label: 'Đã thanh toán', color: 'success' },
  { key: 'Cancelled', label: 'Đã hủy', color: 'secondary' },
  { key: 'all', label: 'Tất cả', color: 'primary' },
];

const isOverdue = (term) => term.trangThai === 'Pending' && new Date(term.ngayDenHan) < new Date();

const InstallmentTermList = () => {
  const [status, setStatus] = useState('Pending');
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);

  const fetchTerms = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/orders/installments', { params: { status } });
      setItems(res.data?.items || []);
    } catch (err) {
      setError(err?.response?.data?.message || 'Không tải được danh sách kỳ trả góp.');
    } finally {
      setLoading(false);
    }
  }, [status]);

  useEffect(() => {
    fetchTerms();
  }, [fetchTerms]);

  const summary = useMemo(() => {
    const totalDue = items.filter((t) => t.trangThai === 'Pending').reduce((sum, t) => sum + Number(t.tongTien || 0), 0);
    const overdue = items.filter(isOverdue).length;
    return { totalDue, overdue };
  }, [items]);

  const handleConfirm = async (term) => {
    const ref = window.prompt(`Mã giao dịch (nếu có) cho kỳ ${term.kyThu} của đơn #${term.maDonHangKinhDoanh}:`, '');
    if (ref === null) return;
    setBusyId(term.maKyTraGop);
    try {
      await orderService.confirmPayment(term.maDonHang, { maKyTraGop: term.maKyTraGop, maGiaoDich: ref || undefined });
      await fetchTerms();
    } catch (err) {
      alert(err?.response?.data?.message || 'Xác nhận thất bại.');
    } finally {
      setBusyId(null);
    }
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid d-flex justify-content-between align-items-center">
          <div>
            <h1 className="m-0">Duyệt kỳ trả góp</h1>
            <p className="text-muted mb-0 mt-1">Theo dõi các kỳ trả góp của khách hàng và xác nhận khi đã nhận được tiền chuyển khoản.</p>
          </div>
          <button className="btn btn-default" onClick={fetchTerms} disabled={loading}>
            <i className="fas fa-sync"></i> Làm mới
          </button>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {/* Summary cards */}
          <div className="row">
            <div className="col-md-4">
              <div className="info-box">
                <span className="info-box-icon bg-warning"><i className="fas fa-clock"></i></span>
                <div className="info-box-content">
                  <span className="info-box-text">Tổng tiền chờ duyệt</span>
                  <span className="info-box-number">{formatCurrency(summary.totalDue)}</span>
                </div>
              </div>
            </div>
            <div className="col-md-4">
              <div className="info-box">
                <span className="info-box-icon bg-danger"><i className="fas fa-exclamation-triangle"></i></span>
                <div className="info-box-content">
                  <span className="info-box-text">Số kỳ quá hạn</span>
                  <span className="info-box-number">{summary.overdue}</span>
                </div>
              </div>
            </div>
            <div className="col-md-4">
              <div className="info-box">
                <span className="info-box-icon bg-info"><i className="fas fa-list"></i></span>
                <div className="info-box-content">
                  <span className="info-box-text">Đang hiển thị</span>
                  <span className="info-box-number">{items.length} kỳ</span>
                </div>
              </div>
            </div>
          </div>

          {/* Tabs */}
          <div className="card">
            <div className="card-header p-2">
              <ul className="nav nav-pills">
                {STATUS_TABS.map((t) => (
                  <li key={t.key} className="nav-item">
                    <button
                      type="button"
                      className={`nav-link ${status === t.key ? 'active' : ''}`}
                      onClick={() => setStatus(t.key)}
                    >
                      {t.label}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
            <div className="card-body p-0">
              {error && <div className="alert alert-danger m-3">{error}</div>}
              {loading ? (
                <div className="text-center py-5"><div className="spinner-border text-primary" /></div>
              ) : items.length === 0 ? (
                <div className="text-center text-muted py-5">Không có kỳ trả góp nào ở trạng thái này.</div>
              ) : (
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th>Mã đơn</th>
                      <th>Khách hàng</th>
                      <th>SĐT</th>
                      <th className="text-center">Kỳ</th>
                      <th>Đến hạn</th>
                      <th className="text-right">Gốc</th>
                      <th className="text-right">Lãi</th>
                      <th className="text-right">Tổng</th>
                      <th className="text-center">Trạng thái</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map((t) => {
                      const overdue = isOverdue(t);
                      return (
                        <tr key={t.maKyTraGop} className={overdue ? 'table-danger' : ''}>
                          <td>
                            <Link to={`/orders/${t.maDonHang}`}>{t.maDonHangKinhDoanh || `#${t.maDonHang}`}</Link>
                          </td>
                          <td>{t.hoTenNguoiVay}<br /><small className="text-muted">CCCD: {t.soCCCD}</small></td>
                          <td>{t.soDienThoai || '-'}</td>
                          <td className="text-center"><strong>{t.kyThu}</strong>/{t.soKy}</td>
                          <td>
                            {formatDate(t.ngayDenHan)}
                            {overdue && <span className="badge badge-danger ml-2">QUÁ HẠN</span>}
                          </td>
                          <td className="text-right">{formatCurrency(t.soTienGoc)}</td>
                          <td className="text-right">{formatCurrency(t.soTienLai)}</td>
                          <td className="text-right"><strong>{formatCurrency(t.tongTien)}</strong></td>
                          <td className="text-center">
                            <span className={`badge badge-${t.trangThai === 'Paid' ? 'success' : t.trangThai === 'Cancelled' ? 'secondary' : 'warning'}`}>
                              {t.trangThai === 'Paid' ? 'Đã trả' : t.trangThai === 'Cancelled' ? 'Đã hủy' : 'Chờ trả'}
                            </span>
                            {t.ngayThanhToan && <div className="small text-muted mt-1">{formatDate(t.ngayThanhToan)}</div>}
                          </td>
                          <td>
                            {t.trangThai === 'Pending' && (
                              <button
                                className="btn btn-sm btn-success"
                                onClick={() => handleConfirm(t)}
                                disabled={busyId === t.maKyTraGop}
                              >
                                <i className="fas fa-check"></i> Đã nhận
                              </button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default InstallmentTermList;
