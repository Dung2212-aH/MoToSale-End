import React, { useMemo, useState, useEffect } from 'react';
import productService from '../../services/productService';
import { useAuth } from '../../contexts/AuthContext';

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

function getErrorMessage(error, fallback = 'Thao tác thất bại. Vui lòng thử lại.') {
  return error?.response?.data?.message || error?.message || fallback;
}

const VariantManager = ({ productId, onClose }) => {
  const { isAdmin } = useAuth();
  const [variants, setVariants] = useState([]);
  const [images, setImages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editVariant, setEditVariant] = useState(null);
  const [saving, setSaving] = useState(false);
  const [uploadingVariantId, setUploadingVariantId] = useState(null);

  const [form, setForm] = useState({
    tenBienThe: '',
    sku: '',
    phienBan: '',
    mauSac: '',
    giaGoc: '',
    giaKhuyenMai: '',
    soLuongTon: '',
    trangThai: 'Available',
  });

  const fetchVariants = async () => {
    setLoading(true);
    setError('');
    try {
      const [variantRes, imageRes] = await Promise.all([
        productService.getVariants(productId),
        productService.getImages(productId),
      ]);
      const data = variantRes.data;
      const imageData = imageRes.data;
      setVariants(Array.isArray(data) ? data : data.items || data.data || []);
      setImages(Array.isArray(imageData) ? imageData : imageData.items || imageData.data || []);
    } catch (err) {
      setError(getErrorMessage(err, 'Không thể tải danh sách biến thể.'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (productId) fetchVariants();
  }, [productId]);

  useEffect(() => {
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, []);

  const openAdd = () => {
    setEditVariant(null);
    setForm({ tenBienThe: '', sku: '', phienBan: '', mauSac: '', giaGoc: '', giaKhuyenMai: '', soLuongTon: '', trangThai: 'Available' });
    setShowForm(true);
  };

  const openEdit = (v) => {
    setEditVariant(v);
    setForm({
      tenBienThe: v.tenBienThe || v.name || '',
      sku: v.sku || '',
      phienBan: v.phienBan || v.version || '',
      mauSac: v.mauSac || v.color || '',
      giaGoc: v.giaGoc ?? '',
      giaKhuyenMai: v.giaKhuyenMai ?? '',
      soLuongTon: v.soLuongTon ?? v.stock ?? '',
      trangThai: v.trangThai || v.status || 'Available',
    });
    setShowForm(true);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const getVariantId = (variant) => variant.id || variant.maBienSanPham;

  const imagesByVariant = useMemo(() => {
    const grouped = new Map();
    images.forEach((image) => {
      if (!image.maBienSanPham) return;
      const key = String(image.maBienSanPham);
      if (!grouped.has(key)) grouped.set(key, []);
      grouped.get(key).push(image);
    });
    return grouped;
  }, [images]);

  const validateFiles = (files) => {
    const validFiles = [];
    const errors = [];

    files.forEach((file) => {
      if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
        errors.push(`${file.name}: chỉ hỗ trợ JPG, PNG hoặc WebP`);
        return;
      }

      if (file.size > MAX_FILE_SIZE) {
        errors.push(`${file.name}: vượt quá 5MB`);
        return;
      }

      validFiles.push(file);
    });

    return { validFiles, errors };
  };

  const handleVariantImagesSelected = async (variant, fileList) => {
    const variantId = getVariantId(variant);
    const files = Array.from(fileList || []);
    if (!files.length || !variantId) return;

    const { validFiles, errors } = validateFiles(files);
    if (errors.length) {
      setError(errors.join('\n'));
    }

    if (!validFiles.length) return;

    const currentVariantImages = imagesByVariant.get(String(variantId)) || [];
    setUploadingVariantId(variantId);
    setError('');
    setSuccess('');

    try {
      for (let index = 0; index < validFiles.length; index += 1) {
        const formData = new FormData();
        formData.append('file', validFiles[index]);
        formData.append('maBienSanPham', variantId);
        formData.append('isMain', currentVariantImages.length === 0 && index === 0 ? 'true' : 'false');
        await productService.uploadImage(productId, formData);
      }

      await fetchVariants();
      setSuccess(`Đã upload ${validFiles.length} ảnh cho ${variant.tenBienThe || variant.name}.`);
    } catch (err) {
      setError(getErrorMessage(err, 'Upload ảnh biến thể thất bại.'));
    } finally {
      setUploadingVariantId(null);
    }
  };

  const handleSetVariantMainImage = async (imageId) => {
    try {
      setError('');
      const formData = new FormData();
      formData.append('imageId', imageId);
      formData.append('isMain', 'true');
      await productService.uploadImage(productId, formData);
      await fetchVariants();
      setSuccess('Đã đặt ảnh chính cho biến thể.');
    } catch (err) {
      setError(getErrorMessage(err, 'Đặt ảnh chính thất bại.'));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenBienThe.trim()) {
      alert('Tên biến thể là bắt buộc!');
      return;
    }
    if (!(Number(form.giaGoc) > 0)) {
      alert('Giá gốc của biến thể phải lớn hơn 0!');
      return;
    }
    setSaving(true);
    try {
      // Giá thật nằm ở biến thể: giaGoc (bắt buộc) + giaKhuyenMai (tùy chọn).
      // Backend: giaKhuyenMai <= 0 -> bỏ khuyến mãi (lưu null).
      const payload = {
        ...form,
        giaGoc: Number(form.giaGoc) || 0,
        giaKhuyenMai: Number(form.giaKhuyenMai) || 0,
      };
      if (editVariant) {
        // Backend bỏ qua soLuongTon khi update; gửi chuỗi rỗng còn gây lỗi 400 binding
        delete payload.soLuongTon;
        await productService.updateVariant(productId, getVariantId(editVariant), payload);
      } else {
        payload.soLuongTon = Number(form.soLuongTon) || 0;
        await productService.createVariant(productId, payload);
      }
      setShowForm(false);
      fetchVariants();
    } catch (err) {
      alert(getErrorMessage(err, 'Lưu biến thể thất bại!'));
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (variantId, name) => {
    if (!window.confirm(`Xóa biến thể "${name}"?`)) return;
    try {
      await productService.deleteVariant(productId, variantId);
      fetchVariants();
    } catch (err) {
      alert(getErrorMessage(err, 'Xóa biến thể thất bại!'));
      console.error(err);
    }
  };

  return (
    <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-xl variant-manager-dialog" style={{ maxHeight: '90vh' }}>
        <div className="modal-content" style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
          <div className="modal-header">
            <h5 className="modal-title">Quản lý biến thể sản phẩm</h5>
            <button type="button" className="close" onClick={onClose}>
              <span>&times;</span>
            </button>
          </div>
          <div className="modal-body variant-manager-body" style={{ overflowY: 'auto', flex: 1 }}>
            <div className="mb-3">
              <button className="btn btn-primary btn-sm" onClick={openAdd}>
                <i className="fas fa-plus"></i> Thêm biến thể
              </button>
            </div>

            {error && <div className="alert alert-danger white-space-pre-line">{error}</div>}
            {success && <div className="alert alert-success">{success}</div>}

            {loading ? (
              <div className="text-center py-3">
                <div className="spinner-border spinner-border-sm text-primary" role="status"></div>
                <span className="ml-2">Đang tải...</span>
              </div>
            ) : variants.length === 0 ? (
              <p className="text-muted text-center">Chưa có biến thể nào.</p>
            ) : (
              <div className="variant-card-list">
                {variants.map((variant) => {
                  const variantId = getVariantId(variant);
                  const variantImages = imagesByVariant.get(String(variantId)) || [];
                  const isUploading = String(uploadingVariantId) === String(variantId);
                  const inputId = `variant-image-${variantId}`;

                  return (
                    <div key={variantId} className="variant-image-card">
                      <div className="variant-image-card-main">
                        <div className="variant-meta">
                          <strong>{variant.tenBienThe || variant.name}</strong>
                          <span className="text-muted">{variant.sku || 'Chưa có SKU'}</span>
                          <div className="mt-2 d-flex flex-wrap" style={{ gap: 6 }}>
                            {(variant.phienBan || variant.version) && <span className="badge badge-light">{variant.phienBan || variant.version}</span>}
                            {(variant.mauSac || variant.color) && <span className="badge badge-info">{variant.mauSac || variant.color}</span>}
                            <span className="badge badge-dark">
                              Giá: {Number(variant.giaKhuyenMai ?? variant.giaGoc ?? 0).toLocaleString('vi-VN')}đ
                              {variant.giaKhuyenMai != null && Number(variant.giaKhuyenMai) > 0 && Number(variant.giaKhuyenMai) < Number(variant.giaGoc) && (
                                <span className="ml-1 text-decoration-line-through" style={{ opacity: 0.6 }}>{Number(variant.giaGoc).toLocaleString('vi-VN')}đ</span>
                              )}
                            </span>
                            <span className="badge badge-secondary">Tồn: {variant.soLuongTon ?? variant.stock ?? 0}</span>
                            <span className={`badge badge-${(variant.trangThai || variant.status) === 'Available' ? 'success' : 'secondary'}`}>
                              {(variant.trangThai || variant.status) === 'Available' ? 'Hoạt động' : 'Ngừng'}
                            </span>
                          </div>
                        </div>

                        <div className="variant-actions">
                          <input
                            id={inputId}
                            type="file"
                            accept="image/jpeg,image/png,image/webp"
                            multiple
                            className="d-none"
                            disabled={isUploading}
                            onChange={(event) => {
                              handleVariantImagesSelected(variant, event.target.files);
                              event.target.value = '';
                            }}
                          />
                          <label className="btn btn-sm btn-primary mb-0" htmlFor={inputId}>
                            {isUploading ? (
                              <>
                                <span className="spinner-border spinner-border-sm mr-1"></span>
                                Đang upload
                              </>
                            ) : (
                              <>
                                <i className="fas fa-image mr-1"></i>
                                Chọn ảnh
                              </>
                            )}
                          </label>
                          <button className="btn btn-sm btn-info" onClick={() => openEdit(variant)}>
                            <i className="fas fa-edit mr-1"></i>Sửa
                          </button>
                          {isAdmin() && (
                            <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(variantId, variant.tenBienThe || variant.name)}>
                              <i className="fas fa-trash"></i>
                            </button>
                          )}
                        </div>
                      </div>

                      <div className="variant-image-strip">
                        {variantImages.length === 0 ? (
                          <div className="variant-image-empty">
                            <i className="fas fa-camera mr-1"></i>
                            Chưa có ảnh cho biến thể này
                          </div>
                        ) : (
                          variantImages.map((image) => (
                            <div key={image.id || image.maAnhSanPham} className={`variant-image-thumb ${image.laAnhChinh ? 'is-main' : ''}`}>
                              <img src={image.urlAnh || image.url} alt={image.altText || ''} />
                              {image.laAnhChinh ? (
                                <span className="badge badge-primary">Chính</span>
                              ) : (
                                <button type="button" onClick={() => handleSetVariantMainImage(image.id || image.maAnhSanPham)}>
                                  Đặt chính
                                </button>
                              )}
                            </div>
                          ))
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}

            {/* Inline Form */}
            {showForm && (
              <div className="card mt-3 variant-form-card">
                <div className="card-header">
                  <h6 className="card-title m-0">{editVariant ? 'Sửa biến thể' : 'Thêm biến thể mới'}</h6>
                </div>
                <div className="card-body">
                  <form onSubmit={handleSubmit}>
                    <div className="row">
                      <div className="col-md-4">
                        <div className="form-group">
                          <label>Tên biến thể <span className="text-danger">*</span></label>
                          <input type="text" className="form-control form-control-sm" name="tenBienThe" value={form.tenBienThe} onChange={handleChange} />
                        </div>
                      </div>
                      <div className="col-md-4">
                        <div className="form-group">
                          <label>SKU</label>
                          <input type="text" className="form-control form-control-sm" name="sku" value={form.sku} onChange={handleChange} />
                        </div>
                      </div>
                      <div className="col-md-4">
                        <div className="form-group">
                          <label>Phiên bản</label>
                          <input type="text" className="form-control form-control-sm" name="phienBan" value={form.phienBan} onChange={handleChange} />
                        </div>
                      </div>
                    </div>
                    <div className="row">
                      <div className="col-md-3">
                        <div className="form-group">
                          <label>Màu sắc</label>
                          <input type="text" className="form-control form-control-sm" name="mauSac" value={form.mauSac} onChange={handleChange} />
                        </div>
                      </div>
                      <div className="col-md-3">
                        <div className="form-group">
                          <label>Giá gốc <span className="text-danger">*</span></label>
                          <input type="number" className="form-control form-control-sm" name="giaGoc" value={form.giaGoc} onChange={handleChange} min="0" />
                        </div>
                      </div>
                      <div className="col-md-3">
                        <div className="form-group">
                          <label>Giá khuyến mãi</label>
                          <input type="number" className="form-control form-control-sm" name="giaKhuyenMai" value={form.giaKhuyenMai} onChange={handleChange} min="0" />
                        </div>
                      </div>
                      <div className="col-md-3">
                        <div className="form-group">
                          <label>Trạng thái</label>
                          <select className="form-control form-control-sm" name="trangThai" value={form.trangThai} onChange={handleChange}>
                            <option value="Available">Hoạt động</option>
                            <option value="Inactive">Ngừng</option>
                          </select>
                        </div>
                      </div>
                    </div>
                    {!editVariant && (
                      <div className="row">
                        <div className="col-md-3">
                          <div className="form-group">
                            <label>Tồn kho ban đầu</label>
                            <input type="number" className="form-control form-control-sm" name="soLuongTon" value={form.soLuongTon} onChange={handleChange} min="0" />
                          </div>
                        </div>
                      </div>
                    )}
                    <button type="submit" className="btn btn-primary btn-sm mr-2" disabled={saving}>
                      {saving ? 'Đang lưu...' : 'Lưu'}
                    </button>
                    <button type="button" className="btn btn-secondary btn-sm" onClick={() => setShowForm(false)}>Hủy</button>
                  </form>
                </div>
              </div>
            )}
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={onClose}>Đóng</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VariantManager;
