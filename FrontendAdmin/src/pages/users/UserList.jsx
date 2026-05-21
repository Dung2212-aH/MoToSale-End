import React, { useState, useEffect, useCallback } from 'react';
import userService from '../../services/userService';
import { USER_STATUS, ROLES } from '../../utils/constants';
import { formatDate } from '../../utils/formatDate';

const UserList = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [saving, setSaving] = useState(false);

  // Search & Filter
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState('');

  // Pagination
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  const [form, setForm] = useState({
    hoTen: '',
    email: '',
    soDienThoai: '',
    matKhau: '',
    vaiTro: 'Customer',
    trangThai: 'Active',
  });

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (filterStatus) params.status = filterStatus;
      const res = await userService.getAll(params);
      const data = res.data;
      if (Array.isArray(data)) {
        setUsers(data);
        setTotalPages(1);
      } else {
        setUsers(data.items || data.data || []);
        setTotalPages(data.totalPages || Math.ceil((data.total || 0) / pageSize) || 1);
      }
    } catch (err) {
      setError('Không thể tải danh sách người dùng.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [page, search, filterStatus]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchUsers();
  };

  const openAdd = () => {
    setEditItem(null);
    setForm({ hoTen: '', email: '', soDienThoai: '', matKhau: '', vaiTro: 'Customer', trangThai: 'Active' });
    setShowModal(true);
  };

  const openEdit = (item) => {
    setEditItem(item);
    setForm({
      hoTen: item.hoTen || item.fullName || '',
      email: item.email || '',
      soDienThoai: item.soDienThoai || item.phone || '',
      matKhau: '',
      vaiTro: item.vaiTro || item.role || item.roles?.[0] || 'Customer',
      trangThai: item.trangThai || item.status || 'Active',
    });
    setShowModal(true);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.hoTen.trim() || !form.email.trim()) {
      alert('Họ tên và Email là bắt buộc!');
      return;
    }
    if (!editItem && !form.matKhau) {
      alert('Mật khẩu là bắt buộc khi thêm mới!');
      return;
    }
    setSaving(true);
    try {
      const payload = { ...form };
      if (editItem && !payload.matKhau) {
        delete payload.matKhau;
      }
      if (editItem) {
        await userService.update(editItem.id, payload);
      } else {
        await userService.create(payload);
      }
      setShowModal(false);
      fetchUsers();
    } catch (err) {
      alert('Lưu người dùng thất bại!');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id, name) => {
    if (!window.confirm(`Bạn có chắc muốn xóa người dùng "${name}"?`)) return;
    try {
      await userService.delete(id);
      fetchUsers();
    } catch (err) {
      alert('Xóa người dùng thất bại!');
      console.error(err);
    }
  };

  const getRoleBadge = (role) => {
    const colors = { Admin: 'danger', Staff: 'warning', Customer: 'info' };
    const label = ROLES[role] || role;
    return <span className={`badge badge-${colors[role] || 'secondary'}`}>{label}</span>;
  };

  const getStatusBadge = (status) => {
    const info = USER_STATUS[status];
    if (info) return <span className={`badge badge-${info.color}`}>{info.label}</span>;
    return <span className="badge badge-secondary">{status}</span>;
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Người dùng</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách người dùng</h3>
              <div className="card-tools">
                <button className="btn btn-primary btn-sm" onClick={openAdd}>
                  <i className="fas fa-plus"></i> Thêm người dùng
                </button>
              </div>
            </div>
            <div className="card-body">
              {/* Search & Filter */}
              <form className="row mb-3" onSubmit={handleSearch}>
                <div className="col-md-4">
                  <input
                    type="text"
                    className="form-control form-control-sm"
                    placeholder="Tìm theo tên, email, SĐT..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                </div>
                <div className="col-md-3">
                  <select
                    className="form-control form-control-sm"
                    value={filterStatus}
                    onChange={(e) => { setFilterStatus(e.target.value); setPage(1); }}
                  >
                    <option value="">-- Tất cả trạng thái --</option>
                    {Object.entries(USER_STATUS).map(([key, val]) => (
                      <option key={key} value={key}>{val.label}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-2">
                  <button type="submit" className="btn btn-info btn-sm btn-block">
                    <i className="fas fa-search"></i> Tìm
                  </button>
                </div>
              </form>

              {error && <div className="alert alert-danger">{error}</div>}

              {loading ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Đang tải...</span>
                  </div>
                </div>
              ) : users.length === 0 ? (
                <div className="text-center py-4 text-muted">
                  <i className="fas fa-users fa-2x mb-2"></i>
                  <p>Chưa có người dùng nào.</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped table-sm">
                      <thead>
                        <tr>
                          <th>Họ tên</th>
                          <th>Email</th>
                          <th>SĐT</th>
                          <th>Vai trò</th>
                          <th>Trạng thái</th>
                          <th>Ngày tạo</th>
                          <th>Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {users.map(u => (
                          <tr key={u.id}>
                            <td>{u.hoTen || u.fullName}</td>
                            <td>{u.email}</td>
                            <td>{u.soDienThoai || u.phone || '-'}</td>
                            <td>{getRoleBadge(u.vaiTro || u.role || u.roles?.[0])}</td>
                            <td>{getStatusBadge(u.trangThai || u.status)}</td>
                            <td>{formatDate(u.ngayTao || u.createdAt)}</td>
                            <td>
                              <button className="btn btn-xs btn-info mr-1" onClick={() => openEdit(u)} title="Sửa">
                                <i className="fas fa-edit"></i>
                              </button>
                              <button className="btn btn-xs btn-danger" onClick={() => handleDelete(u.id, u.hoTen || u.fullName)} title="Xóa">
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
                      <ul className="pagination pagination-sm justify-content-center">
                        <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}>
                          <button className="page-link" onClick={() => setPage(p => p - 1)}>«</button>
                        </li>
                        {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
                          <li key={p} className={`page-item ${p === page ? 'active' : ''}`}>
                            <button className="page-link" onClick={() => setPage(p)}>{p}</button>
                          </li>
                        ))}
                        <li className={`page-item ${page >= totalPages ? 'disabled' : ''}`}>
                          <button className="page-link" onClick={() => setPage(p => p + 1)}>»</button>
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

      {/* Modal Form */}
      {showModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog" style={{ maxHeight: '90vh' }}>
            <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
              <div className="modal-header">
                <h5 className="modal-title">{editItem ? 'Sửa người dùng' : 'Thêm người dùng mới'}</h5>
                <button type="button" className="close" onClick={() => setShowModal(false)}>
                  <span>&times;</span>
                </button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                  <div className="form-group">
                    <label>Họ tên <span className="text-danger">*</span></label>
                    <input type="text" className="form-control" name="hoTen" value={form.hoTen} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Email <span className="text-danger">*</span></label>
                    <input type="email" className="form-control" name="email" value={form.email} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Số điện thoại</label>
                    <input type="text" className="form-control" name="soDienThoai" value={form.soDienThoai} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Mật khẩu {!editItem && <span className="text-danger">*</span>}</label>
                    <input
                      type="password"
                      className="form-control"
                      name="matKhau"
                      value={form.matKhau}
                      onChange={handleChange}
                      placeholder={editItem ? 'Để trống nếu không đổi' : ''}
                    />
                  </div>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Vai trò</label>
                        <select className="form-control" name="vaiTro" value={form.vaiTro} onChange={handleChange}>
                          {Object.entries(ROLES).map(([key, label]) => (
                            <option key={key} value={key}>{label}</option>
                          ))}
                        </select>
                      </div>
                    </div>
                    {editItem && (
                      <div className="col-md-6">
                        <div className="form-group">
                          <label>Trạng thái</label>
                          <select className="form-control" name="trangThai" value={form.trangThai} onChange={handleChange}>
                            {Object.entries(USER_STATUS).map(([key, val]) => (
                              <option key={key} value={key}>{val.label}</option>
                            ))}
                          </select>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>Hủy</button>
                  <button type="submit" className="btn btn-primary" disabled={saving}>
                    {saving ? 'Đang lưu...' : (editItem ? 'Cập nhật' : 'Thêm mới')}
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

export default UserList;
