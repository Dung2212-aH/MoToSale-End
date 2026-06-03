import React, { useState, useEffect, useCallback } from 'react';
import manufacturerService from '../../services/manufacturerService';
import { useAuth } from '../../contexts/AuthContext';

const ManufacturerList = () => {
  const { isAdmin } = useAuth();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ tenHangSanXuat: '', moTa: '', dangHoatDong: true });

  const fetchData = useCallback(async () => {
    setLoading(true); setError('');
    try {
      const res = await manufacturerService.getAll();
      setItems(res.data.items || []);
    } catch (err) { setError('Không thể tải danh sách hãng sản xuất.'); console.error(err); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  const openAdd = () => { setEditItem(null); setForm({ tenHangSanXuat: '', moTa: '', dangHoatDong: true }); setShowModal(true); };
  const openEdit = (it) => { setEditItem(it); setForm({ tenHangSanXuat: it.tenHangSanXuat || '', moTa: it.moTa || '', dangHoatDong: it.dangHoatDong ?? true }); setShowModal(true); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenHangSanXuat.trim()) { alert('Tên hãng sản xuất là bắt buộc!'); return; }
    setSaving(true);
    try {
      if (editItem) await manufacturerService.update(editItem.maHangSanXuat, form);
      else await manufacturerService.create(form);
      setShowModal(false); fetchData();
    } catch (err) { alert(err.response?.data?.message || 'Lưu thất bại!'); }
    finally { setSaving(false); }
  };

  const handleDelete = async (it) => {
    if (!window.confirm(`Xóa hãng sản xuất "${it.tenHangSanXuat}"?`)) return;
    try { await manufacturerService.delete(it.maHangSanXuat); fetchData(); }
    catch (err) { alert(err.response?.data?.message || 'Xóa thất bại!'); }
  };

  return (
    <div className="content-wrapper">
      <div className="content-header"><div className="container-fluid"><div className="row mb-2"><div className="col-sm-6"><h1 className="m-0">Hãng sản xuất phụ tùng</h1></div></div></div></div>
      <section className="content"><div className="container-fluid"><div className="card">
        <div className="card-header">
          <h3 className="card-title">Danh sách hãng sản xuất</h3>
          <div className="card-tools"><button className="btn btn-primary btn-sm" onClick={openAdd}><i className="fas fa-plus"></i> Thêm hãng</button></div>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          {loading ? (
            <div className="text-center py-4"><div className="spinner-border text-primary" role="status"></div></div>
          ) : (
            <div className="table-responsive">
              <table className="table table-bordered table-striped table-sm">
                <thead><tr><th>Tên hãng</th><th>Mô tả</th><th className="text-center">Trạng thái</th><th className="text-right">Thao tác</th></tr></thead>
                <tbody>
                  {items.map((it) => (
                    <tr key={it.maHangSanXuat}>
                      <td className="font-weight-bold">{it.tenHangSanXuat}</td>
                      <td>{it.moTa || '—'}</td>
                      <td className="text-center"><span className={`badge badge-${it.dangHoatDong ? 'success' : 'secondary'}`}>{it.dangHoatDong ? 'Hoạt động' : 'Ẩn'}</span></td>
                      <td className="text-right">
                        <button className="btn btn-xs btn-info mr-1" onClick={() => openEdit(it)}><i className="fas fa-edit"></i></button>
                        {isAdmin() && <button className="btn btn-xs btn-danger" onClick={() => handleDelete(it)}><i className="fas fa-trash"></i></button>}
                      </td>
                    </tr>
                  ))}
                  {items.length === 0 && <tr><td colSpan={4} className="text-center text-muted py-3">Chưa có hãng sản xuất.</td></tr>}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div></div></section>

      {showModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header"><h5 className="modal-title">{editItem ? 'Sửa hãng sản xuất' : 'Thêm hãng sản xuất'}</h5><button type="button" className="close" onClick={() => setShowModal(false)}><span>&times;</span></button></div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="form-group"><label>Tên hãng <span className="text-danger">*</span></label><input type="text" className="form-control" value={form.tenHangSanXuat} onChange={(e) => setForm({ ...form, tenHangSanXuat: e.target.value })} placeholder="VD: Michelin, Motul, GS" /></div>
                  <div className="form-group"><label>Mô tả</label><textarea className="form-control" rows="2" value={form.moTa} onChange={(e) => setForm({ ...form, moTa: e.target.value })} /></div>
                  <div className="custom-control custom-switch"><input type="checkbox" className="custom-control-input" id="mfgActive" checked={form.dangHoatDong} onChange={(e) => setForm({ ...form, dangHoatDong: e.target.checked })} /><label className="custom-control-label" htmlFor="mfgActive">{form.dangHoatDong ? 'Hoạt động' : 'Ẩn'}</label></div>
                </div>
                <div className="modal-footer"><button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>Hủy</button><button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Đang lưu...' : (editItem ? 'Cập nhật' : 'Thêm mới')}</button></div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ManufacturerList;
