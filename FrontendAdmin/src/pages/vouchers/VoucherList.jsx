import React, { useState, useEffect } from 'react';
import voucherService from '../../services/voucherService';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate, formatDateShort } from '../../utils/formatDate';

const VOUCHER_TYPES = {
  Percent: 'Phần trăm (%)',
  Fixed: 'Cố định (VNĐ)',
};

const VOUCHER_STATUS = {
  Active: { label: 'Hoạt động', color: 'success' },
  Inactive: { label: 'Ngừng', color: 'secondary' },
  Expired: { label: 'Hết hạn', color: 'danger' },
};

const defaultForm = {
  code: '',
  discountType: 'Percent',
  discountValue: '',
  minOrderValue: '',
  maxDiscountValue: '',
  startDate: '',
  endDate: '',
  usageLimit: '',
  description: '',
  scope: '',
  status: 'Active',
};

const VoucherList = () => {
  const [vouchers, setVouchers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({ ...defaultForm });
  const [saving, setSaving] = useState(false);
  const pageSize = 10;

  const fetchVouchers = async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize };
      const res = await voucherService.getAll(params);
      const data = res.data;
      setVouchers(data.items || data.data || data || []);
      setTotalPages(data.totalPages || Math.ceil((data.total || 0) / pageSize) || 1);
    } catch (err) {
      setError('Không thể tải danh sách voucher. Vui lòng thử lại.');
      setVouchers([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchVouchers();
  }, [page]);

  const openAddModal = () => {
    setEditingId(null);
    setForm({ ...defaultForm });
    setShowModal(true);
  };

  const openEditModal = (voucher) => {
    setEditingId(voucher.id);
    setForm({
      code: voucher.maVoucher || voucher.code || '',
      discountType: voucher.loaiGiam || voucher.discountType || 'Percent',
      discountValue: voucher.giaTriGiam || voucher.discountValue || '',
      minOrderValue: voucher.donToiThieu || voucher.minOrderValue || '',
      maxDiscountValue: voucher.giamToiDa || voucher.maxDiscountValue || '',
      startDate: voucher.ngayBatDau || voucher.startDate || '',
      endDate: voucher.ngayKetThuc || voucher.endDate || '',
      usageLimit: voucher.gioiHanSuDung || voucher.usageLimit || '',
      description: voucher.moTa || voucher.description || '',
      scope: voucher.phamViApDung || voucher.scope || '',
      status: voucher.trangThai || voucher.status || 'Active',
    });
    setShowModal(true);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSave = async (e) => {
    e.preventDefault();
    if (!form.code.trim()) {
      alert('Vui lòng nhập mã voucher.');
      return;
    }
    if (!form.discountValue) {
      alert('Vui lòng nhập giá trị giảm.');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        code: form.code,
        discountType: form.discountType,
        discountValue: Number(form.discountValue),
        minOrderValue: form.minOrderValue ? Number(form.minOrderValue) : null,
        maxDiscountValue: form.maxDiscountValue ? Number(form.maxDiscountValue) : null,
        startDate: form.startDate || null,
        endDate: form.endDate || null,
        usageLimit: form.usageLimit ? Number(form.usageLimit) : null,
        description: form.description,
        scope: form.scope,
        status: form.status,
      };
      if (editingId) {
        await voucherService.update(editingId, payload);
      } else {
        await voucherService.create(payload);
      }
      setShowModal(false);
      fetchVouchers();
    } catch (err) {
      alert('Lưu voucher thất bại. Vui lòng thử lại.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Bạn có chắc muốn xóa voucher này?')) return;
    try {
      await voucherService.delete(id);
      fetchVouchers();
    } catch (err) {
      alert('Xóa voucher thất bại. Vui lòng thử lại.');
    }
  };

  const getStatusBadge = (status) => {
    const s = VOUCHER_STATUS[status];
    if (!s) return <span className="badge badge-secondary">{status}</span>;
    return <span className={`badge badge-${s.color}`}>{s.label}</span>;
  };

  const formatDiscountValue = (voucher) => {
    const type = voucher.loaiGiam || voucher.discountType;
    const value = voucher.giaTriGiam || voucher.discountValue || 0;
    if (type === 'Percent') return `${value}%`;
    return formatCurrency(value);
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Voucher</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách Voucher</h3>
              <div className="card-tools">
                <button className="btn btn-primary btn-sm" onClick={openAddModal}>
                  <i className="fas fa-plus"></i> Thêm Voucher
                </button>
              </div>
            </div>
            <div className="card-body">
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
              ) : vouchers.length === 0 ? (
                <div className="text-center py-4">
                  <i className="fas fa-ticket-alt fa-3x text-muted mb-3"></i>
                  <p className="text-muted">Chưa có voucher nào.</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped">
                      <thead>
                        <tr>
                          <th>Mã voucher</th>
                          <th>Loại giảm</th>
                          <th>Giá trị</th>
                          <th>Đơn tối thiểu</th>
                          <th>Thời hạn</th>
                          <th>Đã dùng/Giới hạn</th>
                          <th>Trạng thái</th>
                          <th>Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {vouchers.map((voucher) => (
                          <tr key={voucher.id}>
                            <td><strong>{voucher.maVoucher || voucher.code}</strong></td>
                            <td>{VOUCHER_TYPES[voucher.loaiGiam || voucher.discountType] || voucher.loaiGiam || voucher.discountType}</td>
                            <td>{formatDiscountValue(voucher)}</td>
                            <td>{formatCurrency(voucher.donToiThieu || voucher.minOrderValue || 0)}</td>
                            <td>
                              {formatDateShort(voucher.ngayBatDau || voucher.startDate)} - {formatDateShort(voucher.ngayKetThuc || voucher.endDate)}
                            </td>
                            <td>
                              {voucher.daDung || voucher.usedCount || 0} / {voucher.gioiHanSuDung || voucher.usageLimit || '∞'}
                            </td>
                            <td>{getStatusBadge(voucher.trangThai || voucher.status)}</td>
                            <td>
                              <button
                                className="btn btn-warning btn-sm mr-1"
                                onClick={() => openEditModal(voucher)}
                                title="Sửa"
                              >
                                <i className="fas fa-edit"></i>
                              </button>
                              <button
                                className="btn btn-danger btn-sm"
                                onClick={() => handleDelete(voucher.id)}
                                title="Xóa"
                              >
                                <i className="fas fa-trash"></i>
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

      {/* Add/Edit Modal */}
      {showModal && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-lg" style={{ maxHeight: '90vh' }}>
            <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
              <form onSubmit={handleSave}>
                <div className="modal-header">
                  <h5 className="modal-title">{editingId ? 'Sửa Voucher' : 'Thêm Voucher'}</h5>
                  <button type="button" className="close" onClick={() => setShowModal(false)}>
                    <span>&times;</span>
                  </button>
                </div>
                <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Mã code <span className="text-danger">*</span></label>
                        <input
                          type="text"
                          className="form-control"
                          name="code"
                          value={form.code}
                          onChange={handleChange}
                          placeholder="VD: SALE50"
                        />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Loại giảm giá <span className="text-danger">*</span></label>
                        <select
                          className="form-control"
                          name="discountType"
                          value={form.discountType}
                          onChange={handleChange}
                        >
                          <option value="Percent">Phần trăm (%)</option>
                          <option value="Fixed">Cố định (VNĐ)</option>
                        </select>
                      </div>
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-4">
                      <div className="form-group">
                        <label>Giá trị giảm <span className="text-danger">*</span></label>
                        <input
                          type="number"
                          className="form-control"
                          name="discountValue"
                          value={form.discountValue}
                          onChange={handleChange}
                          placeholder={form.discountType === 'Percent' ? 'VD: 10' : 'VD: 50000'}
                        />
                      </div>
                    </div>
                    <div className="col-md-4">
                      <div className="form-group">
                        <label>Giá trị đơn tối thiểu</label>
                        <input
                          type="number"
                          className="form-control"
                          name="minOrderValue"
                          value={form.minOrderValue}
                          onChange={handleChange}
                          placeholder="VD: 200000"
                        />
                      </div>
                    </div>
                    <div className="col-md-4">
                      <div className="form-group">
                        <label>Giá trị giảm tối đa</label>
                        <input
                          type="number"
                          className="form-control"
                          name="maxDiscountValue"
                          value={form.maxDiscountValue}
                          onChange={handleChange}
                          placeholder="VD: 100000"
                        />
                      </div>
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-4">
                      <div className="form-group">
                        <label>Ngày bắt đầu</label>
                        <input
                          type="date"
                          className="form-control"
                          name="startDate"
                          value={form.startDate ? form.startDate.substring(0, 10) : ''}
                          onChange={handleChange}
                        />
                      </div>
                    </div>
                    <div className="col-md-4">
                      <div className="form-group">
                        <label>Ngày kết thúc</label>
                        <input
                          type="date"
                          className="form-control"
                          name="endDate"
                          value={form.endDate ? form.endDate.substring(0, 10) : ''}
                          onChange={handleChange}
                        />
                      </div>
                    </div>
                    <div className="col-md-4">
                      <div className="form-group">
                        <label>Giới hạn sử dụng</label>
                        <input
                          type="number"
                          className="form-control"
                          name="usageLimit"
                          value={form.usageLimit}
                          onChange={handleChange}
                          placeholder="Để trống = không giới hạn"
                        />
                      </div>
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Phạm vi áp dụng</label>
                        <input
                          type="text"
                          className="form-control"
                          name="scope"
                          value={form.scope}
                          onChange={handleChange}
                          placeholder="VD: Tất cả, Danh mục X, Hãng Y"
                        />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Trạng thái</label>
                        <select
                          className="form-control"
                          name="status"
                          value={form.status}
                          onChange={handleChange}
                        >
                          <option value="Active">Hoạt động</option>
                          <option value="Inactive">Ngừng</option>
                        </select>
                      </div>
                    </div>
                  </div>
                  <div className="form-group">
                    <label>Mô tả</label>
                    <textarea
                      className="form-control"
                      name="description"
                      rows="2"
                      value={form.description}
                      onChange={handleChange}
                      placeholder="Mô tả voucher..."
                    ></textarea>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-default" onClick={() => setShowModal(false)}>Đóng</button>
                  <button type="submit" className="btn btn-primary" disabled={saving}>
                    {saving ? 'Đang lưu...' : (editingId ? 'Cập nhật' : 'Tạo mới')}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default VoucherList;
