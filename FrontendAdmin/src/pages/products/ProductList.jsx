import React, { useState, useEffect, useCallback } from 'react';
import productService from '../../services/productService';
import categoryService from '../../services/categoryService';
import brandService from '../../services/brandService';
import { PRODUCT_STATUS } from '../../utils/constants';
import { formatCurrency } from '../../utils/formatCurrency';
import ProductForm from './ProductForm';
import VariantManager from './VariantManager';
import ImageManager from './ImageManager';

const ProductList = () => {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Filters
  const [search, setSearch] = useState('');
  const [filterCategory, setFilterCategory] = useState('');
  const [filterBrand, setFilterBrand] = useState('');
  const [filterStatus, setFilterStatus] = useState('');

  // Pagination
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const pageSize = 10;

  // Modal
  const [showForm, setShowForm] = useState(false);
  const [editProduct, setEditProduct] = useState(null);
  const [showVariants, setShowVariants] = useState(null);
  const [showImages, setShowImages] = useState(null);

  const getProductId = (product) => product.maSanPham || product.id;

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = {
        page,
        pageSize,
        keyword: search || undefined,
        maDanhMuc: filterCategory || undefined,
        maHangXe: filterBrand || undefined,
        trangThaiSanPham: filterStatus || undefined,
      };
      const res = await productService.getAll(params);
      const data = res.data;
      if (Array.isArray(data)) {
        setProducts(data);
        setTotalPages(1);
        setTotalItems(data.length);
      } else {
        setProducts(data.items || data.data || []);
        setTotalPages(data.totalPages || Math.ceil((data.totalItems || 0) / pageSize) || 1);
        setTotalItems(data.totalItems || data.total || data.totalCount || 0);
      }
    } catch (err) {
      setError('Không thể tải danh sách sản phẩm.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [page, search, filterCategory, filterBrand, filterStatus]);

  const fetchFilters = async () => {
    try {
      const [catRes, brandRes] = await Promise.allSettled([
        categoryService.getAll(),
        brandService.getAll(),
      ]);
      if (catRes.status === 'fulfilled') {
        const d = catRes.value.data;
        setCategories(Array.isArray(d) ? d : d.items || d.data || []);
      }
      if (brandRes.status === 'fulfilled') {
        const d = brandRes.value.data;
        setBrands(Array.isArray(d) ? d : d.items || d.data || []);
      }
    } catch (err) {
      console.error('Lỗi tải bộ lọc:', err);
    }
  };

  useEffect(() => {
    fetchFilters();
  }, []);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleDelete = async (id, name) => {
    if (!window.confirm(`Bạn có chắc muốn xóa sản phẩm "${name}"?`)) return;
    try {
      await productService.delete(id);
      fetchProducts();
    } catch (err) {
      alert('Xóa sản phẩm thất bại!');
      console.error(err);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchProducts();
  };

  const openAdd = () => {
    setEditProduct(null);
    setShowForm(true);
  };

  const openEdit = (product) => {
    setEditProduct(product);
    setShowForm(true);
  };

  const renderPagination = () => {
    if (totalPages <= 1) return null;
    const pages = [];
    for (let i = 1; i <= totalPages; i++) {
      pages.push(
        <li key={i} className={`page-item ${i === page ? 'active' : ''}`}>
          <button className="page-link" onClick={() => setPage(i)}>{i}</button>
        </li>
      );
    }
    return (
      <nav>
        <ul className="pagination pagination-sm m-0 float-right">
          <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => setPage(page - 1)}>«</button>
          </li>
          {pages}
          <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => setPage(page + 1)}>»</button>
          </li>
        </ul>
      </nav>
    );
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Sản phẩm</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Danh sách sản phẩm</h3>
              <div className="card-tools">
                <button className="btn btn-primary btn-sm" onClick={openAdd}>
                  <i className="fas fa-plus"></i> Thêm sản phẩm
                </button>
              </div>
            </div>
            <div className="card-body">
              {/* Filters */}
              <form onSubmit={handleSearch} className="row mb-3">
                <div className="col-md-3">
                  <input
                    type="text"
                    className="form-control form-control-sm"
                    placeholder="Tìm theo tên/mã SP..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                </div>
                <div className="col-md-2">
                  <select className="form-control form-control-sm" value={filterCategory} onChange={(e) => { setFilterCategory(e.target.value); setPage(1); }}>
                    <option value="">-- Danh mục --</option>
                    {categories.map(c => (
                      <option key={c.maDanhMuc || c.id} value={c.maDanhMuc || c.id}>{c.tenDanhMuc || c.name}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-2">
                  <select className="form-control form-control-sm" value={filterBrand} onChange={(e) => { setFilterBrand(e.target.value); setPage(1); }}>
                    <option value="">-- Hãng xe --</option>
                    {brands.map(b => (
                      <option key={b.maHangXe || b.id} value={b.maHangXe || b.id}>{b.tenHang || b.name}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-2">
                  <select className="form-control form-control-sm" value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPage(1); }}>
                    <option value="">-- Trạng thái --</option>
                    {Object.entries(PRODUCT_STATUS).map(([key, val]) => (
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

              {/* Error */}
              {error && <div className="alert alert-danger">{error}</div>}

              {/* Loading */}
              {loading ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Đang tải...</span>
                  </div>
                </div>
              ) : products.length === 0 ? (
                <div className="text-center py-4 text-muted">
                  <i className="fas fa-box-open fa-2x mb-2"></i>
                  <p>Không có sản phẩm nào.</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped table-sm">
                      <thead>
                        <tr>
                          <th>Mã SP</th>
                          <th>Tên sản phẩm</th>
                          <th>Danh mục</th>
                          <th>Hãng xe</th>
                          <th>Giá gốc</th>
                          <th>Giá KM</th>
                          <th>Tồn kho</th>
                          <th>Trạng thái</th>
                          <th>Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {products.map((p) => {
                          const statusKey = p.trangThaiSanPham || p.trangThai || p.status;
                          const status = PRODUCT_STATUS[statusKey] || { label: statusKey || 'N/A', color: 'secondary' };
                          const catName = categories.find(c => c.id === p.maDanhMuc || c.maDanhMuc === p.maDanhMuc);
                          const brandName = brands.find(b => b.id === p.maHangXe || b.maHangXe === p.maHangXe);
                          return (
                            <tr key={getProductId(p)}>
                              <td>{p.maSanPhamKinhDoanh || p.maSP || p.sku || p.id}</td>
                              <td>{p.tenSanPham || p.name}</td>
                              <td>{catName?.tenDanhMuc || catName?.name || ''}</td>
                              <td>{brandName?.tenHang || brandName?.name || ''}</td>
                              <td>{formatCurrency(p.giaGoc || p.basePrice || 0)}</td>
                              <td>{formatCurrency(p.giaKhuyenMai || p.giaBan || p.salePrice || 0)}</td>
                              <td>{p.soLuongTon ?? p.stock ?? 0}</td>
                              <td><span className={`badge badge-${status.color}`}>{status.label}</span></td>
                              <td>
                                <button type="button" className="btn btn-xs btn-info mr-1" title="Edit" onClick={() => openEdit(p)}>
                                  <i className="fas fa-edit"></i>
                                </button>
                                <button type="button" className="btn btn-xs btn-warning mr-1" title="Variants" onClick={() => setShowVariants(getProductId(p))}>
                                  <i className="fas fa-layer-group"></i>
                                </button>
                                <button type="button" className="btn btn-xs btn-success mr-1" title="Images" onClick={() => setShowImages(getProductId(p))}>
                                  <i className="fas fa-images"></i>
                                </button>
                                <button type="button" className="btn btn-xs btn-danger" title="Delete" onClick={() => handleDelete(getProductId(p), p.tenSanPham || p.name)}>
                                  <i className="fas fa-trash"></i>
                                </button>
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>

                  <div className="row mt-3">
                    <div className="col-sm-6">
                      <span className="text-muted">Hiển thị {products.length} / {totalItems} sản phẩm</span>
                    </div>
                    <div className="col-sm-6">
                      {renderPagination()}
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </section>

      {/* Product Form Modal */}
      {showForm && (
        <ProductForm
          show={showForm}
          onClose={() => setShowForm(false)}
          onSaved={() => { setShowForm(false); fetchProducts(); }}
          product={editProduct}
          categories={categories}
          brands={brands}
        />
      )}

      {/* Variant Manager Modal */}
      {showVariants && (
        <VariantManager
          productId={showVariants}
          onClose={() => setShowVariants(null)}
        />
      )}

      {/* Image Manager Modal */}
      {showImages && (
        <ImageManager
          productId={showImages}
          onClose={() => setShowImages(null)}
        />
      )}
    </div>
  );
};

export default ProductList;
