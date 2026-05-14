import React, { useMemo, useState, useEffect } from 'react';
import { categoryApi, getApiErrorMessage } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import AdminPage from '../components/admin/AdminPage';
import DataTable, { FilterBar, Pagination } from '../components/admin/DataTable';
import { ConfirmActionButton, ErrorState, LoadingState } from '../components/admin/UiState';
import { SelectInput, StatusBadge, SwitchInput, TextArea, TextInput } from '../components/admin/FormControls';
const emptyCategory = {
  name: '',
  slug: '',
  parentCategoryId: '',
  description: '',
  sortOrder: 0,
  isActive: true
};
const toSlug = value => value.trim().toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/Ä‘/g, 'd').replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
const Categories = () => {
  const [categories, setCategories] = useState([]);
  const [allCategories, setAllCategories] = useState([]);
  const [filters, setFilters] = useState({
    keyword: '',
    parentCategoryId: '',
    isActive: ''
  });
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editingCategory, setEditingCategory] = useState(null);
  const [formData, setFormData] = useState(emptyCategory);
  const [error, setError] = useState('');
  const {
    isAdmin
  } = useAuth();
  const loadCategories = async () => {
    setLoading(true);
    setPageError('');
    try {
      const [response, lookupResponse] = await Promise.all([categoryApi.getAll({
        keyword: filters.keyword || undefined,
        parentCategoryId: filters.parentCategoryId || undefined,
        isActive: filters.isActive === '' ? undefined : filters.isActive,
        page,
        pageSize
      }), categoryApi.getAll({
        page: 1,
        pageSize: 200
      })]);
      const items = Array.isArray(response.data) ? response.data : response.data?.items || [];
      const lookupItems = Array.isArray(lookupResponse.data) ? lookupResponse.data : lookupResponse.data?.items || [];
      setCategories(items);
      setTotalPages(Math.max(1, Math.ceil(items.length / pageSize)));
      setTotalCount(items.length);
      setAllCategories(lookupItems);
    } catch (err) {
      setPageError(getApiErrorMessage(err, 'Kh?ng ti c danh m?c'));
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    loadCategories();
  }, [filters, page]);
  const openModal = (category = null) => {
    if (category) {
      setEditingCategory(category);
      setFormData({
        name: category.name,
        slug: category.slug || '',
        parentCategoryId: category.parentCategoryId || '',
        description: category.description || '',
        sortOrder: category.sortOrder || 0,
        isActive: category.isActive ?? true
      });
    } else {
      setEditingCategory(null);
      setFormData(emptyCategory);
    }
    setError('');
    setShowModal(true);
  };
  const closeModal = () => {
    if (saving) return;
    setShowModal(false);
    setEditingCategory(null);
    setError('');
  };
  const handleSubmit = async e => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      const payload = {
        ...formData,
        slug: formData.slug || toSlug(formData.name),
        parentCategoryId: formData.parentCategoryId ? parseInt(formData.parentCategoryId, 10) : null,
        sortOrder: parseInt(formData.sortOrder, 10) || 0
      };
      if (editingCategory) {
        await categoryApi.update(editingCategory.id, {
          id: editingCategory.id,
          ...payload
        });
      } else {
        await categoryApi.create(payload);
      }
      closeModal();
      await loadCategories();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };
  const handleDelete = async id => {
    try {
      await categoryApi.delete(id);
      await loadCategories();
    } catch (err) {
      setPageError(getApiErrorMessage(err, 'Kh?ng xa c danh m?c. C th danh m?c loading c products lin quan.'));
    }
  };
  const columns = useMemo(() => [{
    key: 'id',
    label: 'ID',
    style: {
      width: '80px'
    }
  }, {
    key: 'name',
    label: "Categories",
    render: category => <>
                    <strong>{category.name}</strong>
                    <div className="text-muted small">{category.slug}</div>
                </>
  }, {
    key: 'parent',
    label: "Parent category",
    render: category => category.parentCategory?.name || '-'
  }, {
    key: 'sortOrder',
    label: "Sort",
    render: category => category.sortOrder ?? 0
  }, {
    key: 'status',
    label: "Status",
    render: category => <StatusBadge value={category.isActive ? "Active" : "Inactive"} />
  }, {
    key: 'actions',
    label: "Actions",
    className: 'text-right',
    render: category => isAdmin() ? <div className="admin-actions">
                    <button type="button" className="btn btn-sm btn-info" onClick={() => openModal(category)} title="Edit">
                        <i className="fas fa-edit"></i>
                    </button>
                    <ConfirmActionButton confirmMessage="Xa danh m?c ny Nu ch mun n, hy chuyn tr?ng th?i sloading ?ng??ng active." onConfirm={() => handleDelete(category.id)} title="Delete">
                        <i className="fas fa-trash"></i>
                    </ConfirmActionButton>
                </div> : '-'
  }], [isAdmin]);
  return <AdminPage title="Product Categories" subtitle="Manage category tree, display order, and active status." breadcrumbs={[{
    label: "Categories",
    active: true
  }]} actions={isAdmin() && <button className="btn btn-success btn-sm" onClick={() => openModal()} type="button">
                    <i className="fas fa-plus mr-1"></i>
                    Add category
                </button>}>
            {pageError && <ErrorState message={pageError} onRetry={loadCategories} />}

            <div className="card">
                <div className="card-header">
                    <h3 className="admin-section-title">Category list</h3>
                    <FilterBar onSubmit={event => {
          event.preventDefault();
          setPage(1);
          loadCategories();
        }}>
                        <div className="col-md-4">
                            <TextInput label="Search" value={filters.keyword} onChange={event => setFilters(current => ({
              ...current,
              keyword: event.target.value
            }))} placeholder="Name, slug, description" />
                        </div>
                        <div className="col-md-3">
                            <SelectInput label="Parent category" value={filters.parentCategoryId} onChange={event => setFilters(current => ({
              ...current,
              parentCategoryId: event.target.value
            }))}>
                                <option value="">All parent categories</option>
                                {allCategories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
                            </SelectInput>
                        </div>
                        <div className="col-md-2">
                            <SelectInput label="Status" value={filters.isActive} onChange={event => setFilters(current => ({
              ...current,
              isActive: event.target.value
            }))}>
                                <option value="">All statuses</option>
                                <option value="true">ang active</option>
                                <option value="false">Inactive</option>
                            </SelectInput>
                        </div>
                        <div className="col-md-3 d-flex align-items-end">
                            <button type="submit" className="btn btn-primary btn-block">Apply</button>
                        </div>
                    </FilterBar>
                </div>
                <div className="card-body p-0">
                    {loading ? <LoadingState label="Loading categories..." /> : <DataTable columns={columns} rows={categories} emptyTitle="No categories found" emptyDescription="Create a root category or product group to organize the catalog." />}
                </div>
                <div className="card-footer">
                    <Pagination page={page} totalPages={totalPages} totalCount={totalCount} label="categories" onPageChange={setPage} />
                </div>
            </div>

            {showModal && <div className="modal fade show" style={{
      display: 'block'
    }} tabIndex="-1">
                    <div className="modal-dialog">
                        <div className="modal-content">
                            <div className="modal-header">
                                <h5 className="modal-title">
                                    {editingCategory ? "Edit category" : "Add category"}
                                </h5>
                                <button type="button" className="close" onClick={closeModal} disabled={saving}>
                                    <span>&times;</span>
                                </button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger">{error}</div>}
                                    <TextInput label="Name" value={formData.name} onChange={e => setFormData({
                ...formData,
                name: e.target.value,
                slug: formData.slug || toSlug(e.target.value)
              })} required />
                                    <TextInput label="Slug" value={formData.slug} onChange={e => setFormData({
                ...formData,
                slug: e.target.value
              })} required />
                                    <SelectInput label="Parent category" value={formData.parentCategoryId} onChange={e => setFormData({
                ...formData,
                parentCategoryId: e.target.value
              })}>
                                        <option value="">Categories gc</option>
                                        {allCategories.filter(category => category.id !== editingCategory?.id).map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
                                    </SelectInput>
                                    <TextArea label="Description" value={formData.description} onChange={e => setFormData({
                ...formData,
                description: e.target.value
              })} />
                                    <TextInput type="number" label="Sort order" value={formData.sortOrder} onChange={e => setFormData({
                ...formData,
                sortOrder: e.target.value
              })} />
                                    <SwitchInput id="categoryActive" label="ang active" checked={formData.isActive} onChange={checked => setFormData({
                ...formData,
                isActive: checked
              })} />
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary" onClick={closeModal} disabled={saving}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="btn btn-primary" disabled={saving}>
                                        {saving ? <span className="spinner-border spinner-border-sm mr-1"></span> : null}
                                        {editingCategory ? "Update" : "Create"}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>}
            {showModal && <div className="modal-backdrop fade show"></div>}
        </AdminPage>;
};
export default Categories;

