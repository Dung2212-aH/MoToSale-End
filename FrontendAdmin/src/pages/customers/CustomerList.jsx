import React, { useEffect, useMemo, useState } from 'react';
import userService from '../../services/userService';
import orderService from '../../services/orderService';
import warrantyService from '../../services/warrantyService';
import businessOperationsService from '../../services/businessOperationsService';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';
import { createDateStamp, exportWorkbook } from '../../utils/exportExcel';

const getApiMessage = (err, fallback) => err?.response?.data?.message || fallback;
const normalize = (value) => String(value || '').trim().toLowerCase();
const asItems = (payload) => payload?.items || payload?.data || payload || [];

const CustomerList = () => {
  const [customers, setCustomers] = useState([]);
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [selected, setSelected] = useState(null);
  const [careNote, setCareNote] = useState('');
  const [profile, setProfile] = useState(null);
  const [profileLoading, setProfileLoading] = useState(false);
  const [crmForm, setCrmForm] = useState({ subject: '', note: '', followUpAt: '' });
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
      setCustomers(asItems(customersRes.value.data));
      setOrders(ordersRes.status === 'fulfilled' ? asItems(ordersRes.value.data) : []);
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
      const phone = normalize(order.soDienThoaiNhanHang || order.soDienThoai || order.shippingPhone || order.phone);
      const email = normalize(order.emailNhanHang || order.shippingEmail || order.email);
      [phone, email].filter(Boolean).forEach((key) => {
        const current = map.get(key) || { totalOrders: 0, totalSpent: 0, cancelledOrders: 0, lastOrderAt: null };
        current.totalOrders += 1;
        current.totalSpent += Number(order.tongThanhToan ?? order.tongTien ?? order.grandTotal ?? order.totalAmount ?? 0);
        if ((order.trangThaiDonHang || order.orderStatus || order.status) === 'Cancelled') current.cancelledOrders += 1;
        const date = order.ngayTao || order.createdDate || order.placedAt || order.createdAt;
        if (date && (!current.lastOrderAt || new Date(date) > new Date(current.lastOrderAt))) current.lastOrderAt = date;
        map.set(key, current);
      });
    });
    return map;
  }, [orders]);

  const customerName = (customer) => customer.hoTen || customer.fullName || customer.name || '';
  const customerPhone = (customer) => customer.soDienThoai || customer.phoneNumber || customer.phone || '';
  const customerStatus = (customer) => customer.trangThai || customer.status || '';
  const customerCareNote = (customer) => customer.ghiChuChamSoc || customer.careNote || '';

  const enrichCustomer = (customer) => {
    const stats =
      statsByCustomer.get(normalize(customerPhone(customer))) ||
      statsByCustomer.get(normalize(customer.email)) ||
      { totalOrders: 0, totalSpent: 0, cancelledOrders: 0, lastOrderAt: null };
    return { ...customer, ...stats };
  };

  const openNote = (customer) => {
    setSelected(customer);
    setCareNote(customerCareNote(customer));
  };

  const saveNote = async () => {
    if (!selected) return;
    setSaving(true);
    try {
      await userService.updateCustomerCareNote(selected.id, { ghiChuChamSoc: careNote, careNote });
      setSelected(null);
      await fetchData();
    } catch (err) {
      alert(getApiMessage(err, 'Không thể lưu ghi chú chăm sóc.'));
    } finally {
      setSaving(false);
    }
  };

  const matchesCustomer = (item, customer) => {
    const id = customer.id;
    const name = normalize(customerName(customer));
    const phone = normalize(customerPhone(customer));
    const email = normalize(customer.email);
    return (
      item.customerId === id ||
      item.maKhachHang === id ||
      item.maNguoiDung === id ||
      normalize(item.customerName || item.hoTenNhanHang || item.tenKhachHang).includes(name) ||
      (phone && normalize(item.soDienThoaiNhanHang || item.soDienThoai || item.shippingPhone || item.phone).includes(phone)) ||
      (email && normalize(item.emailNhanHang || item.shippingEmail || item.email).includes(email))
    );
  };

  const openProfile = async (customer) => {
    setProfile(null);
    setProfileLoading(true);
    setCrmForm({ subject: '', note: '', followUpAt: '' });
    try {
      const [warrantyRes, repairRes, interactionRes] = await Promise.allSettled([
        warrantyService.getAll({ page: 1, pageSize: 500 }),
        businessOperationsService.getRepairs(),
        businessOperationsService.getInteractions(),
      ]);
      const customerOrders = orders.filter((order) => matchesCustomer(order, customer));
      const warranties = warrantyRes.status === 'fulfilled' ? asItems(warrantyRes.value.data).filter((item) => matchesCustomer(item, customer)) : [];
      const repairs = repairRes.status === 'fulfilled' ? asItems(repairRes.value.data).filter((item) => matchesCustomer(item, customer)) : [];
      const interactions = interactionRes.status === 'fulfilled' ? asItems(interactionRes.value.data).filter((item) => matchesCustomer(item, customer)) : [];
      const summary = {
        orderCount: customerOrders.length,
        orderTotal: customerOrders.reduce((sum, order) => sum + Number(order.tongThanhToan ?? order.tongTien ?? order.grandTotal ?? order.totalAmount ?? 0), 0),
        warrantyCount: warranties.length,
        openCrmCount: interactions.filter((item) => (item.interactionStatus || item.trangThai) === 'Open').length,
      };
      const timeline = [
        ...customerOrders.map((order) => ({
          date: order.ngayTao || order.placedAt || order.createdAt,
          type: 'Đơn hàng',
          title: order.maDonHangKinhDoanh || order.orderCode || order.id,
          note: order.trangThaiDonHang || order.status,
          status: order.trangThaiThanhToan || order.paymentStatus,
        })),
        ...interactions.map((item) => ({
          date: item.followUpAt || item.ngayHenFollowUp,
          type: 'CSKH',
          title: item.subject || item.tieuDe,
          note: item.note || item.ghiChu,
          status: item.interactionStatus || item.trangThai,
        })),
      ].sort((a, b) => new Date(b.date || 0) - new Date(a.date || 0));

      setProfile({ customer, summary, orders: customerOrders, warranties, repairs, interactions, timeline });
    } catch (err) {
      alert(getApiMessage(err, 'Không thể tải hồ sơ khách hàng.'));
    } finally {
      setProfileLoading(false);
    }
  };

  const closeProfile = () => {
    setProfile(null);
    setProfileLoading(false);
  };

  const createFollowUp = async () => {
    if (!profile?.customer?.id) return;
    if (!crmForm.subject.trim()) {
      alert('Vui lòng nhập nội dung chăm sóc.');
      return;
    }
    setSaving(true);
    try {
      await businessOperationsService.createInteraction({
        customerId: profile.customer.id,
        assignedStaffId: null,
        interactionType: 'FollowUp',
        subject: crmForm.subject,
        note: crmForm.note,
        followUpAt: crmForm.followUpAt || null,
      });
      await openProfile(profile.customer);
    } catch (err) {
      alert(getApiMessage(err, 'Không thể tạo lịch chăm sóc.'));
    } finally {
      setSaving(false);
    }
  };

  const exportCustomers = async () => {
    setExporting(true);
    try {
      await exportWorkbook({
        fileName: `khach-hang-${createDateStamp()}.xlsx`,
        sheets: [{
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
            name: customerName(customer),
            phone: customerPhone(customer),
            email: customer.email,
            status: customerStatus(customer),
            orders: customer.totalOrders,
            spent: customer.totalSpent,
            cancelled: customer.cancelledOrders,
            lastOrderAt: customer.lastOrderAt,
            note: customerCareNote(customer),
          })),
        }],
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
            <div className="col-sm-6"><h1 className="m-0">Khách hàng</h1></div>
            <div className="col-sm-6 text-right">
              <button className="btn btn-outline-success" onClick={exportCustomers} disabled={exporting}>
                <i className="fas fa-file-excel mr-1"></i>{exporting ? 'Đang xuất...' : 'Xuất Excel'}
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
                            <strong>{customerName(customer)}</strong>
                            <div className="text-muted small">{customerStatus(customer)}</div>
                          </td>
                          <td>
                            <div>{customerPhone(customer) || '-'}</div>
                            <div className="text-muted small">{customer.email || '-'}</div>
                          </td>
                          <td className="text-center">{customer.totalOrders}</td>
                          <td className="text-right">{formatCurrency(customer.totalSpent)}</td>
                          <td className="text-center">{customer.cancelledOrders}</td>
                          <td>{formatDate(customer.lastOrderAt)}</td>
                          <td className="text-break">{customerCareNote(customer) || '-'}</td>
                          <td className="text-center">
                            <button className="btn btn-xs btn-primary mr-1" onClick={() => openProfile(customer)} title="Hồ sơ 360">
                              <i className="fas fa-user-clock"></i>
                            </button>
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
                <h5 className="modal-title">Ghi chú chăm sóc - {customerName(selected)}</h5>
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

      {(profile || profileLoading) && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-xl">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Hồ sơ khách hàng 360</h5>
                <button type="button" className="close" onClick={closeProfile}><span>&times;</span></button>
              </div>
              <div className="modal-body">
                {profileLoading ? (
                  <div className="text-center py-5">Đang tải hồ sơ khách hàng...</div>
                ) : (
                  <>
                    <div className="row">
                      <div className="col-md-3"><div className="small-box bg-info"><div className="inner"><h3>{profile.summary.orderCount}</h3><p>Đơn hàng</p></div></div></div>
                      <div className="col-md-3"><div className="small-box bg-success"><div className="inner"><h3>{formatCurrency(profile.summary.orderTotal)}</h3><p>Tổng mua</p></div></div></div>
                      <div className="col-md-3"><div className="small-box bg-warning"><div className="inner"><h3>{profile.summary.warrantyCount}</h3><p>Bảo hành</p></div></div></div>
                      <div className="col-md-3"><div className="small-box bg-primary"><div className="inner"><h3>{profile.summary.openCrmCount}</h3><p>CSKH mở</p></div></div></div>
                    </div>

                    <div className="card card-outline card-primary">
                      <div className="card-header"><h3 className="card-title">Tạo lịch chăm sóc</h3></div>
                      <div className="card-body">
                        <div className="row">
                          <div className="col-md-4"><input className="form-control" value={crmForm.subject} onChange={(e) => setCrmForm((prev) => ({ ...prev, subject: e.target.value }))} placeholder="Nội dung cần chăm sóc..." /></div>
                          <div className="col-md-3"><input type="datetime-local" className="form-control" value={crmForm.followUpAt} onChange={(e) => setCrmForm((prev) => ({ ...prev, followUpAt: e.target.value }))} /></div>
                          <div className="col-md-4"><input className="form-control" value={crmForm.note} onChange={(e) => setCrmForm((prev) => ({ ...prev, note: e.target.value }))} placeholder="Ghi chú..." /></div>
                          <div className="col-md-1"><button className="btn btn-primary btn-block" onClick={createFollowUp} disabled={saving}>Tạo</button></div>
                        </div>
                      </div>
                    </div>

                    <div className="row">
                      <div className="col-md-6">
                        <ProfileTable title="Đơn hàng gần đây" headers={['Mã', 'Trạng thái', 'Thanh toán', 'Tổng tiền', 'Ngày']}>
                          {profile.orders.map((x) => <tr key={x.maDonHang || x.id}><td>{x.maDonHangKinhDoanh || x.orderCode || x.id}</td><td>{x.trangThaiDonHang || x.status}</td><td>{x.trangThaiThanhToan || x.paymentStatus}</td><td className="text-right">{formatCurrency(x.tongThanhToan ?? x.totalAmount ?? 0)}</td><td>{formatDate(x.ngayTao || x.createdAt)}</td></tr>)}
                        </ProfileTable>
                      </div>
                      <div className="col-md-6">
                        <ProfileTable title="Timeline khách hàng" headers={['Ngày', 'Loại', 'Nội dung', 'Trạng thái']}>
                          {profile.timeline.map((x, index) => <tr key={`${x.type}-${index}`}><td>{formatDate(x.date)}</td><td>{x.type}</td><td>{x.title}<div className="text-muted small">{x.note || ''}</div></td><td>{x.status}</td></tr>)}
                        </ProfileTable>
                      </div>
                    </div>

                    <div className="row">
                      <div className="col-md-6">
                        <ProfileTable title="Bảo hành" headers={['Mã', 'Sản phẩm', 'Trạng thái', 'Ngày nhận']}>
                          {profile.warranties.map((x) => <tr key={x.id || x.maBaoHanh}><td>{x.code || x.maBaoHanhKinhDoanh || x.id}</td><td>{x.productSnapshot || x.tenSanPham || '-'}</td><td>{x.warrantyStatus || x.trangThai}</td><td>{formatDate(x.receivedAt || x.ngayTiepNhan || x.ngayTao)}</td></tr>)}
                        </ProfileTable>
                      </div>
                      <div className="col-md-6">
                        <ProfileTable title="Sửa chữa" headers={['Mã', 'Xe', 'Lỗi', 'Trạng thái', 'Tổng phí']}>
                          {profile.repairs.map((x) => <tr key={x.id}><td>{x.code}</td><td>{x.vehicleDescription}</td><td>{x.reportedIssue}</td><td>{x.repairStatus}</td><td className="text-right">{formatCurrency(x.total)}</td></tr>)}
                        </ProfileTable>
                      </div>
                    </div>
                  </>
                )}
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={closeProfile}>Đóng</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const ProfileTable = ({ title, headers, children }) => (
  <div className="card">
    <div className="card-header"><h3 className="card-title">{title}</h3></div>
    <div className="card-body p-0">
      <div className="table-responsive">
        <table className="table table-bordered table-sm mb-0">
          <thead><tr>{headers.map((header) => <th key={header}>{header}</th>)}</tr></thead>
          <tbody>{React.Children.count(children) ? children : <tr><td colSpan={headers.length} className="text-center text-muted">Chưa có dữ liệu.</td></tr>}</tbody>
        </table>
      </div>
    </div>
  </div>
);

export default CustomerList;
