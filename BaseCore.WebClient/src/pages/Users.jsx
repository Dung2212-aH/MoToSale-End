import React, { useCallback, useEffect, useState } from 'react';
import AdminPage from '../components/admin/AdminPage';
import DataTable, { FilterBar, Pagination } from '../components/admin/DataTable';
import { TextInput, StatusBadge } from '../components/admin/FormControls';
import { ConfirmActionButton, ErrorState, LoadingState } from '../components/admin/UiState';
import { getApiErrorMessage, normalizePagedResponse, userApi } from '../services/api';

const Users = () => {
  const [users, setUsers] = useState([]);
  const [keyword, setKeyword] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize] = useState(15);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await userApi.getAll({ keyword: keyword || undefined, page, pageSize });
      const result = normalizePagedResponse(response.data);
      setUsers(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc nguoi dung.'));
    } finally {
      setLoading(false);
    }
  }, [keyword, page, pageSize]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  const toggleStatus = async (user) => {
    setSaving(true);
    setError('');
    try {
      await userApi.toggleStatus(user.id);
      await loadUsers();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong cap nhat duoc trang thai nguoi dung.'));
    } finally {
      setSaving(false);
    }
  };

  const columns = [
    {
      key: 'name',
      label: 'User',
      render: (user) => (
        <div>
          <strong>{user.name || user.email}</strong>
          <div className="text-muted small">{user.email}</div>
        </div>
      ),
    },
    { key: 'phone', label: 'Phone' },
    { key: 'role', label: 'Role' },
    { key: 'status', label: 'Status', render: (user) => <StatusBadge value={user.status} /> },
    {
      key: 'actions',
      label: 'Actions',
      className: 'text-right',
      render: (user) => (
        <ConfirmActionButton
          className="btn btn-sm btn-outline-warning"
          confirmMessage="Toggle this user status?"
          onConfirm={() => toggleStatus(user)}
          disabled={saving}
        >
          Toggle status
        </ConfirmActionButton>
      ),
    },
  ];

  return (
    <AdminPage title="Users" subtitle="Admin user list from /api/admin/users.">
      {error && <ErrorState message={error} onRetry={loadUsers} />}
      <div className="card">
        <div className="card-header">
          <FilterBar onSubmit={(event) => { event.preventDefault(); setPage(1); loadUsers(); }}>
            <div className="col-md-10">
              <TextInput label="Search" value={keyword} onChange={(event) => setKeyword(event.target.value)} placeholder="Name, email, phone" />
            </div>
            <div className="col-md-2 d-flex align-items-end">
              <button type="submit" className="btn btn-primary btn-block">Search</button>
            </div>
          </FilterBar>
        </div>
        <div className="card-body p-0">
          {loading ? <LoadingState label="Loading users..." /> : <DataTable columns={columns} rows={users} emptyTitle="No users found" />}
        </div>
        <div className="card-footer">
          <Pagination page={page} totalPages={totalPages} totalCount={totalCount} label="users" onPageChange={setPage} />
        </div>
      </div>
    </AdminPage>
  );
};

export default Users;
