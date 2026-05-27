import React, { useEffect, useMemo, useState } from 'react';
import userService from '../../services/userService';
import orderService from '../../services/orderService';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';
import { createDateStamp, exportWorkbook } from '../../utils/exportExcel';

const getApiMessage = (err, fallback) => err?.response?.data?.message || fallback;
const normalize = (value) => String(value || '').trim().toLowerCase();

const CustomerList = () => {
  const [customers, setCustomers] = useState([]);
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [selected, setSelected] = useState(null);
  const [careNote, setCareNote] = useState('');
  const [saving, setSaving] = useState(false);
  const [exporting, setExporting] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [customersRes, ordersRes] = await Promise.allSettled([
        userService.getCustomers({ search: search || undefined, status: status || undefined, pageSize: 100 }),
        orderService.getAll({ page: 1, pageSize: 1000 }),
      ]);

      if (customersRes.status !== 'fulfilled') throw customersRes.reason;
      const customerData = customersRes.value.data;
      setCustomers(customerData.items || customerData.data || []);

      const orderData = ordersRes.status === 'fulfilled' ? ordersRes.value.data : {};
      setOrders(orderData.items || orderData.data || []);
    } catch (err) {
      setError(getApiMessage(err, 'Không thể tải danh sách khách hàng.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [search, status]);

  const statsByCustomer = useMemo(() => {
    const map = new Map();
    orders.forEach((order) => {
      const phone = normalize(order.soDienThoaiNhanHang || order.soDienThoai || order.phone);
      const email = normalize(order.emailNhanHang || order.email);
      const keys = [phone, email].filter(Boolean);
      keys.forEach((key) => {
        const current = map.get(key) || { totalOrders: 0, totalSpent: 0, cancelledOrders: 0, lastOrderAt: null };
        current.totalOrders += 1;
        current.totalSpent += Number(order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? 0);
        if ((order.trangThaiDonHang || order.status) === 'Cancelled') current.cancelledOrders += 1;
        const date = order.ngayTao || order.createdAt;
        if (date && (!current.lastOrderAt || new Date(date) > new Date(current.lastOrderAt))) current.lastOrderAt = date;
        map.set(key, current);
      });
    });
    return map;
  }, [orders]);

  const enrichCustomer = (customer) => {
    const stats =
      statsByCustomer.get(normalize(customer.soDienThoai)) ||
      statsByCustomer.get(normalize(customer.email)) ||
      { totalOrders: 0, totalSpent: 0, cancelledOrders: 0, lastOrderAt: null };
    return { ...customer, ...stats };
  };

  const openNote = (customer) => {
    setSelected(customer);
    setCareNote(customer.ghiChuChamSoc || '');
  };

  const saveNote = async () => {
    if (!selected) return;
    setSaving(true);
    try {
      await userService.updateCustomerCareNote(selected.id, { ghiChuChamSoc: careNote });
      setSelected(null);
      await fetchData();
    } catch (err) {
      alert(getApiMessage(err, 'Không thể lưu ghi chú chăm sóc.'));
    } finally {
      setSaving(false);
    }
  };

  const exportCustomers = async () => {
    setExporting(true);
    try {
      await exportWorkbook({
        fileName: `khach-hang-${createDateStamp()}.xlsx`,
        sheets: [
          {
            name: 'KhachHang',
            columns: [
              { header: 'Họ tên', key: 'name', width: 28 },
              { header: 'SĐT', key: 'phone', width: 16 },
              { header: 'Email', key: 'email', width: 28 },
              { header: 'Trạng thái', key: 'status', width: 14 },
              { header: 'Tổng đơn', key: 'orders', type: 'number', width: 12 },
              { header: 'Tổng chi tiêu', key: 'spent', type: 'currency', width: 18 },
              { header: 'Đơn hủy', key: 'cancelled', type: 'number', width: 12 },
              { header: 'Đơn gần nhất', key: 'lastOrderAt', type: 'date', width: 20 },
              { header: 'Ghi chú chăm sóc', key: 'note', width: 50 },
            ],
            rows: customers.map(enrichCustomer).map((customer) => ({
              name: customer.hoTen,
              phone: customer.soDienThoai,
              email: customer.email,
              status: customer.trangThai,
              orders: customer.totalOrders,
              spent: customer.totalSpent,
              cancelled: customer.cancelledOrders,
              lastOrderAt: customer.lastOrderAt,
              note: customer.ghiChuChamSoc || '',
            })),
          },
        ],
      });
    } catch (err) {
      alert('Xuất Excel khách hàng thất bại.');
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
              <h1 className="m-0">Khách hàng</h1>
            </div>
            <div className="col-sm-6 text-right">
              <button className="btn btn-outline-success" onClick={exportCustomers} disabled={exporting}>
                <i className="fas fa-file-excel mr-1"></i>
                {exporting ? 'Đang xuất...' : 'Xuất Excel'}
              </button>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {error && <div className="alert alert-danger">{error}</div>}
          <div className="card">
            <div className="card-body">
              <div className="row">
                <div className="col-md-8">
                  <input className="form-control" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Tìm theo tên, SĐT hoặc email..." />
                </div>
                <div className="col-md-4">
                  <select className="form-control" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="">Tất cả trạng thái</option>
                    <option value="Active">Đang hoạt động</option>
                    <option value="Inactive">Ngừng hoạt động</option>
                    <option value="Locked">Đã khóa</option>
                  </select>
                </div>
              </div>
            </div>
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th>Khách hàng</th>
                      <th>Liên hệ</th>
                      <th className="text-center">Tổng đơn</th>
                      <th className="text-right">Tổng chi tiêu</th>
                      <th className="text-center">Đơn hủy</th>
                      <th>Đơn gần nhất</th>
                      <th>Ghi chú chăm sóc</th>
                      <th className="text-center">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {loading ? (
                      <tr><td colSpan="8" className="text-center py-4">Đang tải khách hàng...</td></tr>
                    ) : customers.length === 0 ? (
                      <tr><td colSpan="8" className="text-center text-muted py-4">Chưa có khách hàng phù hợp.</td></tr>
                    ) : customers.map((raw) => {
                      const customer = enrichCustomer(raw);
                      return (
                        <tr key={customer.id}>
                          <td>
                            <strong>{customer.hoTen}</strong>
                            <div className="text-muted small">{customer.trangThai}</div>
                          </td>
                          <td>
                            <div>{customer.soDienThoai || '-'}</div>
                            <div className="text-muted small">{customer.email || '-'}</div>
                          </td>
                          <td className="text-center">{customer.totalOrders}</td>
                          <td className="text-right">{formatCurrency(customer.totalSpent)}</td>
                          <td className="text-center">{customer.cancelledOrders}</td>
                          <td>{formatDate(customer.lastOrderAt)}</td>
                          <td className="text-break">{customer.ghiChuChamSoc || '-'}</td>
                          <td className="text-center">
                            <button className="btn btn-xs btn-info" onClick={() => openNote(customer)} title="Ghi chú chăm sóc">
                              <i className="fas fa-sticky-note"></i>
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </section>

      {selected && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Ghi chú chăm sóc - {selected.hoTen}</h5>
                <button type="button" className="close" onClick={() => setSelected(null)}><span>&times;</span></button>
              </div>
              <div className="modal-body">
                <textarea className="form-control" rows="5" value={careNote} onChange={(e) => setCareNote(e.target.value)} placeholder="Nhu cầu, lịch hẹn, lưu ý chăm sóc khách hàng..." />
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setSelected(null)} disabled={saving}>Đóng</button>
                <button className="btn btn-primary" onClick={saveNote} disabled={saving}>{saving ? 'Đang lưu...' : 'Lưu ghi chú'}</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CustomerList;
