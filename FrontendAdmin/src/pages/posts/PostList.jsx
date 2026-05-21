import React, { useState, useEffect, useCallback } from 'react';
import postService from '../../services/postService';
import { formatDate } from '../../utils/formatDate';

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

const POST_STATUS = {
  Published: { label: 'Đã xuất bản', color: 'success' },
  Draft: { label: 'Bản nháp', color: 'secondary' },
};

const PostList = () => {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [saving, setSaving] = useState(false);

  // Pagination
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  const [form, setForm] = useState({
    tieuDe: '',
    slug: '',
    tomTat: '',
    noiDung: '',
    anhDaiDien: '',
    danhMuc: '',
    trangThai: 'Draft',
  });

  const fetchPosts = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize };
      const res = await postService.getAll(params);
      const data = res.data;
      if (Array.isArray(data)) {
        setPosts(data);
        setTotalPages(1);
      } else {
        setPosts(data.items || data.data || []);
        setTotalPages(data.totalPages || Math.ceil((data.total || 0) / pageSize) || 1);
      }
    } catch (err) {
      setError('Không thể tải danh sách bài viết.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    fetchPosts();
  }, [fetchPosts]);

  const openAdd = () => {
    setEditItem(null);
    setForm({ tieuDe: '', slug: '', tomTat: '', noiDung: '', anhDaiDien: '', danhMuc: '', trangThai: 'Draft' });
    setShowModal(true);
  };

  const openEdit = (item) => {
    setEditItem(item);
    setForm({
      tieuDe: item.tieuDe || item.title || '',
      slug: item.slug || '',
      tomTat: item.tomTat || item.summary || '',
      noiDung: item.noiDung || item.content || '',
      anhDaiDien: item.anhDaiDien || item.thumbnail || '',
      danhMuc: item.danhMuc || item.category || '',
      trangThai: item.trangThai || item.status || 'Draft',
    });
    setShowModal(true);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => {
      const updated = { ...prev, [name]: value };
      if (name === 'tieuDe') {
        updated.slug = generateSlug(value);
      }
      return updated;
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tieuDe.trim()) {
      alert('Tiêu đề là bắt buộc!');
      return;
    }
    setSaving(true);
    try {
      if (editItem) {
        await postService.update(editItem.id, form);
      } else {
        await postService.create(form);
      }
      setShowModal(false);
      fetchPosts();
    } catch (err) {
      alert('Lưu bài viết thất bại!');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id, title) => {
    if (!window.confirm(`Bạn có chắc muốn xóa bài viết "${title}"?`)) return;
    try {
      await postService.delete(id);
      fetchPosts();
    } catch (err) {
      alert('Xóa bài viết thất bại!');
      console.error(err);
    }
  };

  const getStatusBadge = (status) => {
    const info = POST_STATUS[status];
    if (info) return <span className={`badge badge-${info.color}`}>{info.label}</span>;
    return <span className="badge badge-secondary">{status}</span>;
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Bài viết</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách bài viết</h3>
              <div className="card-tools">
                <button className="btn btn-primary btn-sm" onClick={openAdd}>
                  <i className="fas fa-plus"></i> Thêm bài viết
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
              ) : posts.length === 0 ? (
                <div className="text-center py-4 text-muted">
                  <i className="fas fa-newspaper fa-2x mb-2"></i>
                  <p>Chưa có bài viết nào.</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped table-sm">
                      <thead>
                        <tr>
                          <th>Tiêu đề</th>
                          <th>Danh mục</th>
                          <th>Trạng thái</th>
                          <th>Ngày xuất bản</th>
                          <th>Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {posts.map(p => (
                          <tr key={p.id}>
                            <td>{p.tieuDe || p.title}</td>
                            <td>{p.danhMuc || p.category || '-'}</td>
                            <td>{getStatusBadge(p.trangThai || p.status)}</td>
                            <td>{formatDate(p.ngayXuatBan || p.publishedAt || p.createdAt)}</td>
                            <td>
                              <button className="btn btn-xs btn-info mr-1" onClick={() => openEdit(p)} title="Sửa">
                                <i className="fas fa-edit"></i>
                              </button>
                              <button className="btn btn-xs btn-danger" onClick={() => handleDelete(p.id, p.tieuDe || p.title)} title="Xóa">
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
          <div className="modal-dialog modal-lg" style={{ maxHeight: '90vh' }}>
            <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
              <div className="modal-header">
                <h5 className="modal-title">{editItem ? 'Sửa bài viết' : 'Thêm bài viết mới'}</h5>
                <button type="button" className="close" onClick={() => setShowModal(false)}>
                  <span>&times;</span>
                </button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body" style={{ overflowY: 'auto', flex: 1 }}>
                  <div className="form-group">
                    <label>Tiêu đề <span className="text-danger">*</span></label>
                    <input type="text" className="form-control" name="tieuDe" value={form.tieuDe} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Slug</label>
                    <input type="text" className="form-control" name="slug" value={form.slug} onChange={handleChange} />
                    <small className="form-text text-muted">Tự động tạo từ tiêu đề</small>
                  </div>
                  <div className="form-group">
                    <label>Tóm tắt</label>
                    <textarea className="form-control" name="tomTat" value={form.tomTat} onChange={handleChange} rows="2" />
                  </div>
                  <div className="form-group">
                    <label>Nội dung</label>
                    <textarea className="form-control" name="noiDung" value={form.noiDung} onChange={handleChange} rows="8" />
                  </div>
                  <div className="form-group">
                    <label>Ảnh đại diện (URL)</label>
                    <input type="text" className="form-control" name="anhDaiDien" value={form.anhDaiDien} onChange={handleChange} placeholder="https://..." />
                  </div>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Danh mục</label>
                        <input type="text" className="form-control" name="danhMuc" value={form.danhMuc} onChange={handleChange} />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="form-group">
                        <label>Trạng thái</label>
                        <select className="form-control" name="trangThai" value={form.trangThai} onChange={handleChange}>
                          <option value="Draft">Bản nháp</option>
                          <option value="Published">Đã xuất bản</option>
                        </select>
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

export default PostList;
