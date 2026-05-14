import React, { useCallback, useEffect, useMemo, useState } from 'react';
import AdminPage from '../components/admin/AdminPage';
import DataTable, { FilterBar, Pagination } from '../components/admin/DataTable';
import { TextInput, SelectInput, TextArea, SwitchInput, StatusBadge, formatMoney } from '../components/admin/FormControls';
import { ConfirmActionButton, ErrorState, LoadingState } from '../components/admin/UiState';
import { getApiErrorMessage, productApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const emptyProduct = {
  productCode: '',
  name: '',
  slug: '',
  categoryId: '',
  brandId: '',
  carModelId: '',
  showroomId: '',
  productType: 'Motorcycle',
  shortDescription: '',
  description: '',
  basePrice: 0,
  salePrice: '',
  stockQuantity: 0,
  mainImageUrl: '',
  isActive: true,
  status: 'Available',
};

const emptyImage = {
  file: null,
  altText: '',
  isPrimary: true,
  sortOrder: 0,
};

const typeOptions = ['Motorcycle', 'Accessory'];
const statusOptions = ['Available', 'Inactive', 'Hidden'];

const toSlug = (value) => value
  .trim()
  .toLowerCase()
  .normalize('NFD')
  .replace(/[\u0300-\u036f]/g, '')
  .replace(/\u0111/g, 'd')
  .replace(/[^a-z0-9]+/g, '-')
  .replace(/^-+|-+$/g, '');

const cleanParams = (query, page, pageSize) => {
  const params = { page, pageSize };
  if (query.keyword) params.keyword = query.keyword;
  if (query.categoryId) params.maDanhMuc = query.categoryId;
  if (query.productType) params.loaiSanPham = query.productType;
  if (query.status) params.trangThaiSanPham = query.status;
  return params;
};

const Products = () => {
  const { user, isAdmin } = useAuth();
  const canManage = isAdmin() || user?.roles?.includes('Staff') || user?.role === 'Staff';

  const [products, setProducts] = useState([]);
  const [filters, setFilters] = useState({ categories: [], brands: [], carModels: [], showrooms: [] });
  const [query, setQuery] = useState({ keyword: '', categoryId: '', productType: '', status: '' });
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [formData, setFormData] = useState(emptyProduct);
  const [imageData, setImageData] = useState(emptyImage);
  const [productImages, setProductImages] = useState([]);
  const [formError, setFormError] = useState('');

  const loadFilters = useCallback(async () => {
    try {
      const response = await productApi.getFilters();
      setFilters({
        categories: response.data?.categories || [],
        brands: response.data?.brands || [],
        carModels: response.data?.carModels || [],
        showrooms: response.data?.showrooms || [],
      });
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc bo loc san pham.'));
    }
  }, []);

  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await productApi.getAll(cleanParams(query, page, pageSize));
      setProducts(response.data.items);
      setTotalPages(response.data.totalPages);
      setTotalCount(response.data.totalCount);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc san pham.'));
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, query]);

  useEffect(() => {
    loadFilters();
  }, [loadFilters]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  const filteredModels = useMemo(
    () => filters.carModels.filter((model) => !formData.brandId || Number(model.brandId) === Number(formData.brandId)),
    [filters.carModels, formData.brandId],
  );

  const openModal = async (product = null) => {
    setFormError('');
    setImageData(emptyImage);
    setProductImages([]);
    setShowModal(true);

    if (!product) {
      setEditingProduct(null);
      setFormData({
        ...emptyProduct,
        categoryId: filters.categories[0]?.id || '',
        showroomId: filters.showrooms[0]?.id || '',
      });
      return;
    }

    setEditingProduct(product);
    setFormData({ ...emptyProduct, ...product, salePrice: product.salePrice ?? '' });

    try {
      const response = await productApi.getById(product.id);
      const detail = response.data;
      setFormData({ ...emptyProduct, ...detail, salePrice: detail.salePrice ?? '' });
      setProductImages(detail.images || []);
    } catch (err) {
      setFormError(getApiErrorMessage(err, 'Khong tai duoc chi tiet san pham.'));
    }
  };

  const closeModal = () => {
    if (saving) return;
    setShowModal(false);
    setEditingProduct(null);
    setFormData(emptyProduct);
    setImageData(emptyImage);
    setProductImages([]);
    setFormError('');
  };

  const setField = (field, value) => {
    setFormData((current) => ({ ...current, [field]: value }));
  };

  const validateForm = () => {
    if (!formData.productCode.trim()) return 'Product code is required.';
    if (!formData.name.trim()) return 'Product name is required.';
    if (!formData.categoryId) return 'Category is required.';
    if (Number(formData.basePrice) < 0) return 'Base price cannot be negative.';
    if (formData.salePrice !== '' && Number(formData.salePrice) < 0) return 'Sale price cannot be negative.';
    if (Number(formData.stockQuantity) < 0) return 'Stock cannot be negative.';
    return '';
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    const validationError = validateForm();
    if (validationError) {
      setFormError(validationError);
      return;
    }

    setSaving(true);
    setFormError('');
    try {
      const payload = {
        ...formData,
        slug: formData.slug || toSlug(formData.name),
      };

      if (editingProduct) {
        await productApi.update(editingProduct.id, payload);
        if (imageData.file) {
          await productApi.uploadImage(editingProduct.id, imageData);
        }
      } else {
        await productApi.createWithImage(payload, imageData);
      }

      closeModal();
      await loadProducts();
      await loadFilters();
    } catch (err) {
      setFormError(getApiErrorMessage(err, 'Khong luu duoc san pham.'));
    } finally {
      setSaving(false);
    }
  };

  const hideProduct = async (id) => {
    try {
      await productApi.delete(id);
      await loadProducts();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong an duoc san pham.'));
    }
  };

  const columns = [
    {
      key: 'product',
      label: 'Product',
      render: (product) => (
        <div className="d-flex align-items-center">
          {product.mainImageUrl ? (
            <img className="admin-product-thumb mr-2" src={product.mainImageUrl} alt={product.name} />
          ) : null}
          <div>
            <div className="font-weight-bold">{product.name}</div>
            <div className="text-muted small">{product.productCode || '-'}</div>
          </div>
        </div>
      ),
    },
    { key: 'productType', label: 'Type' },
    {
      key: 'price',
      label: 'Price',
      className: 'text-right',
      render: (product) => (
        <div>
          <strong>{formatMoney(product.salePrice || product.basePrice)}</strong>
          {product.salePrice ? <div className="text-muted small"><s>{formatMoney(product.basePrice)}</s></div> : null}
        </div>
      ),
    },
    { key: 'stockQuantity', label: 'Stock', className: 'text-right' },
    { key: 'status', label: 'Status', render: (product) => <StatusBadge value={product.status || (product.isActive ? 'Available' : 'Inactive')} /> },
  ];

  if (canManage) {
    columns.push({
      key: 'actions',
      label: 'Actions',
      className: 'text-right',
      render: (product) => (
        <div className="admin-actions">
          <button type="button" className="btn btn-sm btn-info" title="Edit" onClick={() => openModal(product)}>
            <i className="fas fa-edit"></i>
          </button>
          <ConfirmActionButton title="Hide" confirmMessage="Hide this product?" onConfirm={() => hideProduct(product.id)}>
            <i className="fas fa-eye-slash"></i>
          </ConfirmActionButton>
        </div>
      ),
    });
  }

  return (
    <AdminPage
      title="Products"
      subtitle="Catalog data mapped to CatalogService product endpoints."
      actions={canManage && (
        <button type="button" className="btn btn-success btn-sm" onClick={() => openModal()}>
          <i className="fas fa-plus mr-1"></i>
          Add product
        </button>
      )}
    >
      {error && <ErrorState message={error} onRetry={loadProducts} />}

      <div className="card">
        <div className="card-header">
          <FilterBar onSubmit={(event) => { event.preventDefault(); setPage(1); loadProducts(); }}>
            <div className="col-md-4">
              <TextInput label="Search" value={query.keyword} onChange={(event) => setQuery((current) => ({ ...current, keyword: event.target.value }))} />
            </div>
            <div className="col-md-3">
              <SelectInput label="Category" value={query.categoryId} onChange={(event) => { setPage(1); setQuery((current) => ({ ...current, categoryId: event.target.value })); }}>
                <option value="">All categories</option>
                {filters.categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
              </SelectInput>
            </div>
            <div className="col-md-2">
              <SelectInput label="Type" value={query.productType} onChange={(event) => { setPage(1); setQuery((current) => ({ ...current, productType: event.target.value })); }}>
                <option value="">All types</option>
                {typeOptions.map((type) => <option key={type} value={type}>{type}</option>)}
              </SelectInput>
            </div>
            <div className="col-md-2">
              <SelectInput label="Status" value={query.status} onChange={(event) => { setPage(1); setQuery((current) => ({ ...current, status: event.target.value })); }}>
                <option value="">All statuses</option>
                {statusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
              </SelectInput>
            </div>
            <div className="col-md-1 d-flex align-items-end">
              <button type="submit" className="btn btn-primary btn-block">
                <i className="fas fa-search"></i>
              </button>
            </div>
          </FilterBar>
        </div>
        <div className="card-body p-0">
          {loading ? <LoadingState label="Loading products..." /> : <DataTable columns={columns} rows={products} emptyTitle="No products found" />}
        </div>
        <div className="card-footer">
          <Pagination page={page} totalPages={totalPages} totalCount={totalCount} label="products" onPageChange={setPage} />
        </div>
      </div>

      {showModal && (
        <>
          <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1" role="dialog" aria-modal="true">
            <div className="modal-dialog modal-xl" role="document">
              <div className="modal-content">
                <form onSubmit={handleSubmit}>
                  <div className="modal-header">
                    <h5 className="modal-title">{editingProduct ? 'Edit product' : 'Add product'}</h5>
                    <button type="button" className="close" onClick={closeModal} disabled={saving}>
                      <span>&times;</span>
                    </button>
                  </div>
                  <div className="modal-body">
                    {formError && <div className="alert alert-danger">{formError}</div>}
                    <div className="form-row">
                      <div className="col-md-4">
                        <TextInput label="Code" value={formData.productCode} onChange={(event) => setField('productCode', event.target.value)} required disabled={Boolean(editingProduct)} />
                      </div>
                      <div className="col-md-5">
                        <TextInput label="Name" value={formData.name} onChange={(event) => setFormData((current) => ({ ...current, name: event.target.value, slug: current.slug || toSlug(event.target.value) }))} required />
                      </div>
                      <div className="col-md-3">
                        <TextInput label="Slug" value={formData.slug} onChange={(event) => setField('slug', event.target.value)} required />
                      </div>
                      <div className="col-md-3">
                        <SelectInput label="Category" value={formData.categoryId} onChange={(event) => setField('categoryId', event.target.value)} required>
                          <option value="">Choose category</option>
                          {filters.categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
                        </SelectInput>
                      </div>
                      <div className="col-md-3">
                        <SelectInput label="Brand" value={formData.brandId || ''} onChange={(event) => setFormData((current) => ({ ...current, brandId: event.target.value, carModelId: '' }))}>
                          <option value="">No brand</option>
                          {filters.brands.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
                        </SelectInput>
                      </div>
                      <div className="col-md-3">
                        <SelectInput label="Model" value={formData.carModelId || ''} onChange={(event) => setField('carModelId', event.target.value)}>
                          <option value="">No model</option>
                          {filteredModels.map((model) => <option key={model.id} value={model.id}>{model.name}</option>)}
                        </SelectInput>
                      </div>
                      <div className="col-md-3">
                        <SelectInput label="Showroom" value={formData.showroomId || ''} onChange={(event) => setField('showroomId', event.target.value)}>
                          <option value="">No showroom</option>
                          {filters.showrooms.map((showroom) => <option key={showroom.id} value={showroom.id}>{showroom.name}</option>)}
                        </SelectInput>
                      </div>
                      <div className="col-md-3">
                        <SelectInput label="Type" value={formData.productType} onChange={(event) => setField('productType', event.target.value)}>
                          {typeOptions.map((type) => <option key={type} value={type}>{type}</option>)}
                        </SelectInput>
                      </div>
                      <div className="col-md-3">
                        <TextInput label="Base price" type="number" min="0" value={formData.basePrice} onChange={(event) => setField('basePrice', event.target.value)} />
                      </div>
                      <div className="col-md-3">
                        <TextInput label="Sale price" type="number" min="0" value={formData.salePrice} onChange={(event) => setField('salePrice', event.target.value)} />
                      </div>
                      <div className="col-md-3">
                        <TextInput label="Stock" type="number" min="0" value={formData.stockQuantity} onChange={(event) => setField('stockQuantity', event.target.value)} />
                      </div>
                      <div className="col-md-3">
                        <SelectInput label="Status" value={formData.status} onChange={(event) => setField('status', event.target.value)}>
                          {statusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
                        </SelectInput>
                      </div>
                      <div className="col-md-3 d-flex align-items-center pt-md-4">
                        <SwitchInput id="productActive" label="Active" checked={Boolean(formData.isActive)} onChange={(checked) => setField('isActive', checked)} />
                      </div>
                      <div className="col-md-12">
                        <TextArea label="Short description" rows={2} value={formData.shortDescription || ''} onChange={(event) => setField('shortDescription', event.target.value)} />
                      </div>
                      <div className="col-md-12">
                        <TextArea label="Description" rows={4} value={formData.description || ''} onChange={(event) => setField('description', event.target.value)} />
                      </div>
                    </div>

                    <div className="admin-form-section mb-0">
                      <h3 className="admin-section-title">Product image upload</h3>
                      <div className="form-row mt-3">
                        <div className="col-md-5">
                          <TextInput label={editingProduct ? 'Upload new image' : 'Main image'} type="file" accept="image/jpeg,image/png,image/webp" onChange={(event) => setImageData((current) => ({ ...current, file: event.target.files?.[0] || null }))} />
                        </div>
                        <div className="col-md-4">
                          <TextInput label="Alt text" value={imageData.altText} onChange={(event) => setImageData((current) => ({ ...current, altText: event.target.value }))} />
                        </div>
                        <div className="col-md-2">
                          <TextInput label="Sort" type="number" min="0" value={imageData.sortOrder} onChange={(event) => setImageData((current) => ({ ...current, sortOrder: event.target.value }))} />
                        </div>
                        <div className="col-md-1 d-flex align-items-center pt-md-4">
                          <SwitchInput id="imagePrimary" label="Main" checked={Boolean(imageData.isPrimary)} onChange={(checked) => setImageData((current) => ({ ...current, isPrimary: checked }))} />
                        </div>
                      </div>
                      {productImages.length > 0 && (
                        <DataTable
                          columns={[
                            {
                              key: 'imageUrl',
                              label: 'Image',
                              render: (image) => (
                                <div className="d-flex align-items-center">
                                  <img className="admin-product-thumb mr-2" src={image.imageUrl} alt={image.altText || formData.name} />
                                  <div>
                                    <div className="admin-url-cell">{image.imageUrl}</div>
                                    <div className="text-muted small">{image.altText || '-'}</div>
                                  </div>
                                </div>
                              ),
                            },
                            { key: 'sortOrder', label: 'Sort', className: 'text-right' },
                            { key: 'isPrimary', label: 'Main', render: (image) => (image.isPrimary ? <StatusBadge value="Primary" /> : '-') },
                          ]}
                          rows={productImages}
                          emptyTitle="No images"
                        />
                      )}
                    </div>
                  </div>
                  <div className="modal-footer">
                    <button type="button" className="btn btn-secondary" onClick={closeModal} disabled={saving}>Cancel</button>
                    <button type="submit" className="btn btn-primary" disabled={saving}>
                      {saving ? <span className="spinner-border spinner-border-sm mr-1"></span> : null}
                      {editingProduct ? 'Update product' : 'Create product'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
          <div className="modal-backdrop fade show"></div>
        </>
      )}
    </AdminPage>
  );
};

export default Products;
