import React, { useState, useEffect, useCallback } from 'react';
import categoryService from '../../services/categoryService';

/**
 * Tạo slug từ chuỗi tiếng Việt
 */
function generateSlug(str) {
  if (!str) return '';
  let slug = str.toLowerCase().trim();
  slug = slug.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, 'a');
  slug = slug.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, 'e');
  slug = slug.replace(/ì|í|ị|ỉ|ĩ/g, 'i');
  slug = slug.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, 'o');
  slug = slug.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, 'u');
  slug = slug.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, 'y');
  slug = slug.replace(/đ/g, 'd');
  slug = slug.replace(/[^a-z0-9\s-]/g, '');
  slug = slug.replace(/[\s_]+/g, '-');
  slug = slug.replace(/-+/g, '-');
  slug = slug.replace(/^-|-$/g, '');
  return slug;
}

const CategoryList = () => {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    tenDanhMuc: '',
    slug: '',
    moTa: '',
    danhMucChaId: '',
    thuTu: 0,
    dangHoatDong: true,
  });

  const fetchCategories = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await categoryService.getAll();
      const data = res.data;
      setCategories(Array.isArray(data) ? data : data.items || data.data || []);
    } catch (err) {
      setError('Không thể tải danh sách danh mục.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  const openAdd = () => {
    setEditItem(null);
    setForm({ tenDanhMuc: '', slug: '', moTa: '', danhMucChaId: '', thuTu: 0, dangHoatDong: true });
    setShowModal(true);
  };

  const openEdit = (item) => {
    setEditItem(item);
    setForm({
      tenDanhMuc: item.tenDanhMuc || item.name || '',
      slug: item.slug || '',
      moTa: item.moTa || item.description || '',
      danhMucChaId: item.danhMucChaId || item.maDanhMucCha || item.parentId || '',
      thuTu: item.thuTu || item.sortOrder || 0,
      dangHoatDong: item.dangHoatDong !== undefined ? item.dangHoatDong : true,
    });
    setShowModal(true);
  };

  const getCategoryId = (category) => category.id || category.maDanhMuc;

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => {
      const updated = { ...prev, [name]: value };
      if (name === 'tenDanhMuc') {
        updated.slug = generateSlug(value);
      }
      return updated;
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenDanhMuc.trim()) {
      alert('Tên danh mục là bắt buộc!');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        tenDanhMuc: form.tenDanhMuc,
        slug: form.slug,
        moTa: form.moTa,
        danhMucChaId: form.danhMucChaId ? Number(form.danhMucChaId) : null,
        thuTu: Number(form.thuTu) || 0,
        dangHoatDong: form.dangHoatDong,
      };
      if (editItem) {
        await categoryService.update(getCategoryId(editItem), payload);
      } else {
        await categoryService.create(payload);
      }
      setShowModal(false);
      fetchCategories();
    } catch (err) {
      alert('Lưu danh mục thất bại!');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id, name) => {
    if (!window.confirm(`Bạn có chắc muốn xóa danh mục "${name}"?`)) return;
    try {
      await categoryService.delete(id);
      fetchCategories();
    } catch (err) {
      alert('Xóa danh mục thất bại!');
      console.error(err);
    }
  };

  const getParentName = (parentId) => {
    if (!parentId) return '';
    const parent = categories.find(c => String(getCategoryId(c)) === String(parentId));
    return parent ? (parent.tenDanhMuc || parent.name) : '';
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Danh mục</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách danh mục</h3>
              <div className="card-tools">
                <button className="btn btn-primary btn-sm" onClick={openAdd}>
                  <i className="fas fa-plus"></i> Thêm danh mục
                </button>
              </div>
            </div>
            <div className="card-body">
              {error && <div className="alert alert-danger">{error}</div>}

              {loading ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Đang tải...</span>
                  </div>
                </div>
              ) : categories.length === 0 ? (
                <div className="text-center py-4 text-muted">
                  <i className="fas fa-folder-open fa-2x mb-2"></i>
                  <p>Chưa có danh mục nào.</p>
                </div>
              ) : (
                <div className="table-responsive">
                  <table className="table table-bordered table-striped table-sm">
                    <thead>
                      <tr>
                        <th>ID</th>
                        <th>Tên danh mục</th>
                        <th>Slug</th>
                        <th>Danh mục cha</th>
                        <th>Thứ tự</th>
                        <th>Trạng thái</th>
                        <th>Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      {categories.map(c => (
                        <tr key={getCategoryId(c)}>
                          <td>{getCategoryId(c)}</td>
                          <td>{c.tenDanhMuc || c.name}</td>
                          <td><code>{c.slug}</code></td>
                          <td>{getParentName(c.danhMucChaId || c.maDanhMucCha || c.parentId)}</td>
                          <td>{c.thuTu || c.thuTuHienThi || c.sortOrder || 0}</td>
                          <td>
                            <span className={`badge badge-${c.dangHoatDong ? 'success' : 'secondary'}`}>
                              {c.dangHoatDong ? 'Hoạt động' : 'Ẩn'}
                            </span>
                          </td>
                          <td>
                            <button className="btn btn-xs btn-info mr-1" onClick={() => openEdit(c)}>
                              <i className="fas fa-edit"></i>
                            </button>
                            <button className="btn btn-xs btn-danger" onClick={() => handleDelete(getCategoryId(c), c.tenDanhMuc || c.name)}>
                              <i className="fas fa-trash"></i>
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
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
                <h5 className="modal-title">{editItem ? 'Sửa danh mục' : 'Thêm danh mục mới'}</h5>
                <button type="button" className="close" onClick={() => setShowModal(false)}>
                  <span>&times;</span>
                </button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                  <div className="form-group">
                    <label>Tên danh mục <span className="text-danger">*</span></label>
                    <input type="text" className="form-control" name="tenDanhMuc" value={form.tenDanhMuc} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Slug</label>
                    <input type="text" className="form-control" name="slug" value={form.slug} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Mô tả</label>
                    <textarea className="form-control" name="moTa" value={form.moTa} onChange={handleChange} rows="3" />
                  </div>
                  <div className="form-group">
                    <label>Danh mục cha</label>
                    <select className="form-control" name="danhMucChaId" value={form.danhMucChaId} onChange={handleChange}>
                      <option value="">-- Không có (gốc) --</option>
                      {categories.filter(c => !editItem || getCategoryId(c) !== getCategoryId(editItem)).map(c => (
                        <option key={getCategoryId(c)} value={String(getCategoryId(c))}>{c.tenDanhMuc || c.name}</option>
                      ))}
                    </select>
                  </div>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Thứ tự hiển thị</label>
                        <input type="number" className="form-control" name="thuTu" value={form.thuTu} onChange={handleChange} min="0" />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Trạng thái</label>
                        <div className="custom-control custom-switch mt-2">
                          <input
                            type="checkbox"
                            className="custom-control-input"
                            id="catDangHoatDong"
                            checked={form.dangHoatDong}
                            onChange={(e) => setForm(prev => ({ ...prev, dangHoatDong: e.target.checked }))}
                          />
                          <label className="custom-control-label" htmlFor="catDangHoatDong">
                            {form.dangHoatDong ? 'Hoạt động' : 'Ẩn'}
                          </label>
                        </div>
                      </div>
                    </div>
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

export default CategoryList;
