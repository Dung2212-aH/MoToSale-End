import React, { useState, useEffect } from 'react';
import productService from '../../services/productService';
import brandService from '../../services/brandService';

/**
 * Tạo slug từ chuỗi tiếng Việt
 */
function generateSlug(str) {
  if (!str) return '';
  let slug = str.toLowerCase().trim();
  // Vietnamese diacritics removal
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

const ProductForm = ({ show, onClose, onSaved, product, categories, brands }) => {
  const isEdit = !!product;
  const [models, setModels] = useState([]);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState({});

  const [form, setForm] = useState({
    maSP: '',
    tenSanPham: '',
    slug: '',
    loaiSP: 'XeMay',
    danhMucId: '',
    hangXeId: '',
    dongXeId: '',
    moTaNgan: '',
    giaGoc: '',
    giaKhuyenMai: '',
    soLuongTon: '',
    anhChinhUrl: '',
    anhChinhFile: null,
    trangThai: 'Active',
  });

  useEffect(() => {
    if (product) {
      setForm({
        maSP: product.maSanPhamKinhDoanh || product.maSP || product.sku || '',
        tenSanPham: product.tenSanPham || product.name || '',
        slug: product.slug || '',
        loaiSP: product.loaiSanPham || product.loaiSP || product.type || 'XeMay',
        danhMucId: String(product.maDanhMuc || product.danhMucId || product.categoryId || ''),
        hangXeId: String(product.maHangXe || product.hangXeId || product.brandId || ''),
        dongXeId: String(product.maDongXe || product.dongXeId || product.modelId || ''),
        moTaNgan: product.moTaNgan || product.shortDescription || '',
        giaGoc: product.giaGoc || product.basePrice || '',
        giaKhuyenMai: product.giaKhuyenMai || product.salePrice || '',
        soLuongTon: product.soLuongTon ?? product.stock ?? 0,
        anhChinhUrl: product.anhChinhUrl || product.mainImage || '',
        anhChinhFile: null,
        trangThai: product.trangThaiSanPham || product.trangThai || product.status || 'Available',
      });
    } else {
      setForm({
        maSP: '',
        tenSanPham: '',
        slug: '',
        loaiSP: 'XeMay',
        danhMucId: '',
        hangXeId: '',
        dongXeId: '',
        moTaNgan: '',
        giaGoc: '',
        giaKhuyenMai: '',
        soLuongTon: '',
        anhChinhUrl: '',
        anhChinhFile: null,
        trangThai: 'Available',
      });
    }
    setErrors({});
  }, [product, show]);

  // Load models when brand changes
  useEffect(() => {
    if (form.hangXeId) {
      brandService.getModels(form.hangXeId)
        .then(res => {
          const data = res.data;
          setModels(Array.isArray(data) ? data : data.items || data.data || []);
        })
        .catch(() => setModels([]));
    } else {
      setModels([]);
    }
  }, [form.hangXeId]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => {
      const updated = { ...prev, [name]: value };
      if (name === 'tenSanPham') {
        updated.slug = generateSlug(value);
      }
      if (name === 'hangXeId') {
        updated.dongXeId = '';
      }
      return updated;
    });
  };

  const validate = () => {
    const errs = {};
    if (!form.tenSanPham.trim()) errs.tenSanPham = 'Tên sản phẩm là bắt buộc';
    if (!form.giaGoc || Number(form.giaGoc) <= 0) errs.giaGoc = 'Giá gốc phải lớn hơn 0';
    if (!form.danhMucId) errs.danhMucId = 'Vui lòng chọn danh mục';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    setSaving(true);
    try {
      const mainImageFile = form.anhChinhFile;
      const payload = {
        maSanPhamKinhDoanh: form.maSP || undefined,
        tenSanPham: form.tenSanPham,
        slug: form.slug || undefined,
        loaiSanPham: form.loaiSP,
        maDanhMuc: form.danhMucId ? Number(form.danhMucId) : undefined,
        maHangXe: form.hangXeId ? Number(form.hangXeId) : null,
        maDongXe: form.dongXeId ? Number(form.dongXeId) : null,
        moTaNgan: form.moTaNgan || undefined,
        giaGoc: Number(form.giaGoc) || 0,
        giaKhuyenMai: Number(form.giaKhuyenMai) || null,
        anhChinhUrl: mainImageFile ? undefined : form.anhChinhUrl || undefined,
        trangThaiSanPham: form.trangThai,
      };
      if (!isEdit) {
        payload.soLuongTon = Number(form.soLuongTon) || 0;
      }

      let productId = product?.maSanPham || product?.id;
      if (isEdit) {
        const res = await productService.update(product.maSanPham || product.id, payload);
        productId = res.data?.id || productId;
      } else {
        const res = await productService.create(payload);
        productId = res.data?.id || productId;
      }

      if (mainImageFile && productId) {
        const formData = new FormData();
        formData.append('file', mainImageFile);
        formData.append('isMain', 'true');
        const uploadRes = await productService.uploadImage(productId, formData);
        const uploadedUrl = uploadRes.data?.urlAnh;
        if (uploadedUrl) {
          await productService.update(productId, { anhChinhUrl: uploadedUrl });
        }
      }
      onSaved();
    } catch (err) {
      alert(isEdit ? 'Cập nhật sản phẩm thất bại!' : 'Thêm sản phẩm thất bại!');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  if (!show) return null;

  return (
    <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{isEdit ? 'Sửa sản phẩm' : 'Thêm sản phẩm mới'}</h5>
            <button type="button" className="close" onClick={onClose}>
              <span>&times;</span>
            </button>
          </div>
          <form onSubmit={handleSubmit}>
            <div className="modal-body" style={{ maxHeight: '70vh', overflowY: 'auto' }}>
              <div className="row">
                <div className="col-md-6">
                  <div className="form-group">
                    <label>Mã SP kinh doanh</label>
                    <input type="text" className="form-control" name="maSP" value={form.maSP} onChange={handleChange} placeholder="VD: SP001" />
                  </div>
                </div>
                <div className="col-md-6">
                  <div className="form-group">
                    <label>Tên sản phẩm <span className="text-danger">*</span></label>
                    <input type="text" className={`form-control ${errors.tenSanPham ? 'is-invalid' : ''}`} name="tenSanPham" value={form.tenSanPham} onChange={handleChange} />
                    {errors.tenSanPham && <div className="invalid-feedback">{errors.tenSanPham}</div>}
                  </div>
                </div>
              </div>

              <div className="row">
                <div className="col-md-6">
                  <div className="form-group">
                    <label>Slug</label>
                    <input type="text" className="form-control" name="slug" value={form.slug} onChange={handleChange} placeholder="Tự động tạo từ tên" />
                  </div>
                </div>
                <div className="col-md-6">
                  <div className="form-group">
                    <label>Loại sản phẩm</label>
                    <select className="form-control" name="loaiSP" value={form.loaiSP} onChange={handleChange}>
                      <option value="XeMay">Xe máy</option>
                      <option value="PhuTung">Phụ tùng</option>
                    </select>
                  </div>
                </div>
              </div>

              <div className="row">
                <div className="col-md-4">
                  <div className="form-group">
                    <label>Danh mục <span className="text-danger">*</span></label>
                    <select className={`form-control ${errors.danhMucId ? 'is-invalid' : ''}`} name="danhMucId" value={form.danhMucId} onChange={handleChange}>
                      <option value="">-- Chọn danh mục --</option>
                      {categories.map(c => (
                        <option key={c.maDanhMuc || c.id} value={String(c.maDanhMuc || c.id)}>{c.tenDanhMuc || c.name}</option>
                      ))}
                    </select>
                    {errors.danhMucId && <div className="invalid-feedback">{errors.danhMucId}</div>}
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="form-group">
                    <label>Hãng xe</label>
                    <select className="form-control" name="hangXeId" value={form.hangXeId} onChange={handleChange}>
                      <option value="">-- Chọn hãng --</option>
                      {brands.map(b => (
                        <option key={b.maHangXe || b.id} value={String(b.maHangXe || b.id)}>{b.tenHang || b.name}</option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="form-group">
                    <label>Dòng xe</label>
                    <select className="form-control" name="dongXeId" value={form.dongXeId} onChange={handleChange} disabled={!form.hangXeId}>
                      <option value="">-- Chọn dòng xe --</option>
                      {models.map(m => (
                        <option key={m.maDongXe || m.id} value={String(m.maDongXe || m.id)}>{m.tenDongXe || m.name}</option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>

              <div className="row">
                <div className="col-md-6">
                  <div className="form-group">
                    <label>Trạng thái</label>
                    <select className="form-control" name="trangThai" value={form.trangThai} onChange={handleChange}>
                      <option value="Available">Đang bán</option>
                      <option value="Inactive">Ngừng bán</option>
                    </select>
                  </div>
                </div>
              </div>

              <div className="form-group">
                <label>Mô tả ngắn</label>
                <textarea className="form-control" name="moTaNgan" value={form.moTaNgan} onChange={handleChange} rows="3" />
              </div>

              <div className="row">
                <div className="col-md-4">
                  <div className="form-group">
                    <label>Giá gốc <span className="text-danger">*</span></label>
                    <input type="number" className={`form-control ${errors.giaGoc ? 'is-invalid' : ''}`} name="giaGoc" value={form.giaGoc} onChange={handleChange} min="0" />
                    {errors.giaGoc && <div className="invalid-feedback">{errors.giaGoc}</div>}
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="form-group">
                    <label>Giá khuyến mại</label>
                    <input type="number" className="form-control" name="giaKhuyenMai" value={form.giaKhuyenMai} onChange={handleChange} min="0" />
                  </div>
                </div>
                {!isEdit && (
                  <div className="col-md-4">
                    <div className="form-group">
                      <label>Tồn kho ban đầu</label>
                      <input type="number" className="form-control" name="soLuongTon" value={form.soLuongTon} onChange={handleChange} min="0" />
                    </div>
                  </div>
                )}
              </div>

              <div className="form-group">
                <label>Ảnh chính</label>
                <div className="custom-file">
                  <input
                    type="file"
                    className="custom-file-input"
                    id="mainImageFile"
                    accept="image/*"
                    onChange={(e) => {
                      const file = e.target.files[0];
                      if (file) {
                        setForm(prev => ({ ...prev, anhChinhFile: file, anhChinhUrl: URL.createObjectURL(file) }));
                      }
                    }}
                  />
                  <label className="custom-file-label" htmlFor="mainImageFile">
                    {form.anhChinhFile ? form.anhChinhFile.name : 'Chọn ảnh từ máy tính...'}
                  </label>
                </div>
                {form.anhChinhUrl && (
                  <img src={form.anhChinhUrl} alt="Preview" className="mt-2 rounded border" style={{ maxHeight: 100, maxWidth: 150, objectFit: 'cover' }} />
                )}
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={onClose}>Hủy</button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? <><span className="spinner-border spinner-border-sm mr-1"></span>Đang lưu...</> : (isEdit ? 'Cập nhật' : 'Thêm mới')}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default ProductForm;
