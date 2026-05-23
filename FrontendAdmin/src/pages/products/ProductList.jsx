import React, { useState, useEffect, useCallback, useMemo } from 'react';
import productService from '../../services/productService';
import categoryService from '../../services/categoryService';
import brandService from '../../services/brandService';
import { PRODUCT_STATUS } from '../../utils/constants';
import { formatCurrency } from '../../utils/formatCurrency';
import ProductForm from './ProductForm';
import VariantManager from './VariantManager';
import ImageManager from './ImageManager';
import CompatibilityManager from './CompatibilityManager';

const PAGE_CONFIG = {
  XeMay: {
    title: 'Quản lý xe máy',
    listTitle: 'Danh sách xe máy',
    addLabel: 'Thêm xe máy',
    emptyText: 'Không có xe máy nào.',
    searchPlaceholder: 'Tìm theo tên/mã xe...',
    categoryPlaceholder: '-- Danh mục xe máy --',
    brandPlaceholder: '-- Hãng xe --',
    nameHeader: 'Tên xe',
    codeHeader: 'Mã xe',
    showBrand: true,
    showVariants: true,
    rootNames: ['xe máy', 'xe may'],
  },
  PhuTung: {
    title: 'Quản lý phụ tùng',
    listTitle: 'Danh sách phụ tùng',
    addLabel: 'Thêm phụ tùng',
    emptyText: 'Không có phụ tùng nào.',
    searchPlaceholder: 'Tìm theo tên/mã phụ tùng/SKU...',
    categoryPlaceholder: '-- Danh mục phụ tùng --',
    brandPlaceholder: '-- Hãng tương thích --',
    nameHeader: 'Tên phụ tùng',
    codeHeader: 'Mã phụ tùng',
    showBrand: false,
    showVariants: false,
    rootNames: ['phụ tùng', 'phu tung', 'phụ kiện', 'phu kien'],
  },
};

const getCategoryId = (category) => category.maDanhMuc || category.id;
const getCategoryName = (category) => category.tenDanhMuc || category.name || '';
const getParentCategoryId = (category) => category.maDanhMucCha ?? category.parentCategoryId ?? category.danhMucChaId ?? null;
const normalizeText = (value) => String(value || '')
  .toLowerCase()
  .normalize('NFD')
  .replace(/[\u0300-\u036f]/g, '')
  .replace(/đ/g, 'd')
  .trim();

const ProductList = ({ productType = 'XeMay' }) => {
  const config = PAGE_CONFIG[productType] || PAGE_CONFIG.XeMay;
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const [search, setSearch] = useState('');
  const [filterCategory, setFilterCategory] = useState('');
  const [filterBrand, setFilterBrand] = useState('');
  const [filterStatus, setFilterStatus] = useState('');

  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const pageSize = 10;

  const [showForm, setShowForm] = useState(false);
  const [editProduct, setEditProduct] = useState(null);
  const [showVariants, setShowVariants] = useState(null);
  const [showImages, setShowImages] = useState(null);
  const [showCompatibility, setShowCompatibility] = useState(null);

  const getProductId = (product) => product.maSanPham || product.id;

  const filteredCategories = useMemo(() => {
    const byParent = new Map();
    categories.forEach((category) => {
      const parentId = getParentCategoryId(category);
      const key = parentId == null ? 'root' : Number(parentId);
      if (!byParent.has(key)) byParent.set(key, []);
      byParent.get(key).push(category);
    });

    const root = categories.find((category) => {
      const isRoot = getParentCategoryId(category) == null;
      const name = normalizeText(getCategoryName(category));
      return isRoot && config.rootNames.some((rootName) => name === normalizeText(rootName));
    });

    if (!root) {
      return categories.filter((category) => getParentCategoryId(category) != null);
    }

    const rootId = Number(getCategoryId(root));
    const allowedIds = new Set();
    const visit = (parentId) => {
      (byParent.get(Number(parentId)) || []).forEach((child) => {
        const childId = Number(getCategoryId(child));
        allowedIds.add(childId);
        visit(childId);
      });
    };
    visit(rootId);
    if (allowedIds.size === 0) allowedIds.add(rootId);
    return categories.filter((category) => allowedIds.has(Number(getCategoryId(category))));
  }, [categories, config.rootNames]);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = {
        page,
        pageSize,
        loaiSanPham: productType,
        keyword: search || undefined,
        maDanhMuc: filterCategory || undefined,
        maHangXe: config.showBrand ? filterBrand || undefined : undefined,
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
      setError(`Không thể tải ${productType === 'XeMay' ? 'danh sách xe máy' : 'danh sách phụ tùng'}.`);
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [page, search, filterCategory, filterBrand, filterStatus, productType, config.showBrand]);

  const fetchFilters = async () => {
    try {
      const [catRes, brandRes] = await Promise.allSettled([
        categoryService.getAll(),
        brandService.getAll(),
      ]);
      if (catRes.status === 'fulfilled') {
        const data = catRes.value.data;
        setCategories(Array.isArray(data) ? data : data.items || data.data || []);
      }
      if (brandRes.status === 'fulfilled') {
        const data = brandRes.value.data;
        setBrands(Array.isArray(data) ? data : data.items || data.data || []);
      }
    } catch (err) {
      console.error('Lỗi tải bộ lọc:', err);
    }
  };

  useEffect(() => {
    fetchFilters();
  }, []);

  useEffect(() => {
    setPage(1);
    setFilterCategory('');
    setFilterBrand('');
    setFilterStatus('');
    setSearch('');
    setEditProduct(null);
    setShowForm(false);
    setShowVariants(null);
    setShowImages(null);
    setShowCompatibility(null);
  }, [productType]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleDelete = async (id, name) => {
    const itemName = productType === 'XeMay' ? 'xe máy' : 'phụ tùng';
    if (!window.confirm(`Bạn có chắc muốn xóa ${itemName} "${name}"?`)) return;
    try {
      await productService.delete(id);
      fetchProducts();
    } catch (err) {
      alert(`Xóa ${itemName} thất bại!`);
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
    return (
      <nav>
        <ul className="pagination pagination-sm m-0 float-right">
          <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => setPage(page - 1)}>«</button>
          </li>
          {Array.from({ length: totalPages }, (_, i) => i + 1).map((pageNumber) => (
            <li key={pageNumber} className={`page-item ${pageNumber === page ? 'active' : ''}`}>
              <button className="page-link" onClick={() => setPage(pageNumber)}>{pageNumber}</button>
            </li>
          ))}
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
              <h1 className="m-0">{config.title}</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">{config.listTitle}</h3>
              <div className="card-tools">
                <button className="btn btn-primary btn-sm" onClick={openAdd}>
                  <i className="fas fa-plus"></i> {config.addLabel}
                </button>
              </div>
            </div>
            <div className="card-body">
              <form onSubmit={handleSearch} className="row mb-3">
                <div className="col-md-3">
                  <input
                    type="text"
                    className="form-control form-control-sm"
                    placeholder={config.searchPlaceholder}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                </div>
                <div className="col-md-2">
                  <select className="form-control form-control-sm" value={filterCategory} onChange={(e) => { setFilterCategory(e.target.value); setPage(1); }}>
                    <option value="">{config.categoryPlaceholder}</option>
                    {filteredCategories.map((category) => (
                      <option key={getCategoryId(category)} value={getCategoryId(category)}>{getCategoryName(category)}</option>
                    ))}
                  </select>
                </div>
                {config.showBrand && (
                  <div className="col-md-2">
                    <select className="form-control form-control-sm" value={filterBrand} onChange={(e) => { setFilterBrand(e.target.value); setPage(1); }}>
                      <option value="">{config.brandPlaceholder}</option>
                      {brands.map((brand) => (
                        <option key={brand.maHangXe || brand.id} value={brand.maHangXe || brand.id}>{brand.tenHang || brand.name}</option>
                      ))}
                    </select>
                  </div>
                )}
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

              {error && <div className="alert alert-danger">{error}</div>}

              {loading ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Đang tải...</span>
                  </div>
                </div>
              ) : products.length === 0 ? (
                <div className="text-center py-4 text-muted">
                  <i className="fas fa-box-open fa-2x mb-2"></i>
                  <p>{config.emptyText}</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped table-sm">
                      <thead>
                        <tr>
                          <th className="table-col-code">{config.codeHeader}</th>
                          <th className="table-col-text">{config.nameHeader}</th>
                          <th className="table-col-text">Danh mục</th>
                          {config.showBrand && <th className="table-col-text">Hãng xe</th>}
                          <th className="table-col-money">Giá gốc</th>
                          <th className="table-col-money">Giá KM</th>
                          <th className="table-col-number">Tồn kho</th>
                          <th className="table-col-status">Trạng thái</th>
                          <th className="table-col-actions">Thao tác</th>
                        </tr>
                      </thead>
                      <tbody>
                        {products.map((product) => {
                          const statusKey = product.trangThaiSanPham || product.trangThai || product.status;
                          const status = PRODUCT_STATUS[statusKey] || { label: statusKey || 'N/A', color: 'secondary' };
                          const category = categories.find((item) => item.id === product.maDanhMuc || item.maDanhMuc === product.maDanhMuc);
                          const brand = brands.find((item) => item.id === product.maHangXe || item.maHangXe === product.maHangXe);
                          return (
                            <tr key={getProductId(product)}>
                              <td className="table-col-code">{product.maSanPhamKinhDoanh || product.maSP || product.sku || product.id}</td>
                              <td className="table-col-text">{product.tenSanPham || product.name}</td>
                              <td className="table-col-text">{category?.tenDanhMuc || category?.name || ''}</td>
                              {config.showBrand && <td className="table-col-text">{brand?.tenHang || brand?.name || ''}</td>}
                              <td className="table-col-money">{formatCurrency(product.giaGoc || product.basePrice || 0)}</td>
                              <td className="table-col-money">{formatCurrency(product.giaKhuyenMai || product.giaBan || product.salePrice || 0)}</td>
                              <td className="table-col-number">{product.soLuongTon ?? product.stock ?? 0}</td>
                              <td className="table-col-status"><span className={`badge badge-${status.color}`}>{status.label}</span></td>
                              <td className="table-col-actions">
                                <button type="button" className="btn btn-xs btn-info mr-1" title="Sửa" onClick={() => openEdit(product)}>
                                  <i className="fas fa-edit"></i>
                                </button>
                                {config.showVariants && (
                                  <button type="button" className="btn btn-xs btn-warning mr-1" title="Biến thể" onClick={() => setShowVariants(getProductId(product))}>
                                    <i className="fas fa-layer-group"></i>
                                  </button>
                                )}
                                {!config.showVariants && (
                                  <button type="button" className="btn btn-xs btn-warning mr-1" title="Tương thích xe" onClick={() => setShowCompatibility(product)}>
                                    <i className="fas fa-link"></i>
                                  </button>
                                )}
                                <button type="button" className="btn btn-xs btn-success mr-1" title="Ảnh" onClick={() => setShowImages(getProductId(product))}>
                                  <i className="fas fa-images"></i>
                                </button>
                                <button type="button" className="btn btn-xs btn-danger" title="Xóa" onClick={() => handleDelete(getProductId(product), product.tenSanPham || product.name)}>
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
                      <span className="text-muted">Hiển thị {products.length} / {totalItems} {productType === 'XeMay' ? 'xe máy' : 'phụ tùng'}</span>
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

      {showForm && (
        <ProductForm
          show={showForm}
          onClose={() => setShowForm(false)}
          onSaved={() => { setShowForm(false); fetchProducts(); }}
          product={editProduct}
          categories={categories}
          brands={brands}
          fixedProductType={productType}
        />
      )}

      {showVariants && (
        <VariantManager
          productId={showVariants}
          onClose={() => setShowVariants(null)}
        />
      )}

      {showImages && (
        <ImageManager
          productId={showImages}
          onClose={() => { setShowImages(null); fetchProducts(); }}
        />
      )}

      {showCompatibility && (
        <CompatibilityManager
          product={showCompatibility}
          onClose={() => setShowCompatibility(null)}
        />
      )}
    </div>
  );
};

export default ProductList;
