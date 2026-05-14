import React, { useCallback, useEffect, useState } from 'react';
import AdminPage from '../components/admin/AdminPage';
import DataTable from '../components/admin/DataTable';
import { StatusBadge } from '../components/admin/FormControls';
import { ErrorState, LoadingState } from '../components/admin/UiState';
import { getApiErrorMessage, showroomApi } from '../services/api';

const Showrooms = () => {
  const [showrooms, setShowrooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadShowrooms = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await showroomApi.getAll({ activeOnly: false });
      setShowrooms(response.data);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc showroom.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadShowrooms();
  }, [loadShowrooms]);

  const columns = [
    {
      key: 'name',
      label: 'Showroom',
      render: (showroom) => (
        <div>
          <strong>{showroom.name}</strong>
          <div className="text-muted small">{showroom.slug || '-'}</div>
        </div>
      ),
    },
    { key: 'address', label: 'Address' },
    { key: 'phone', label: 'Phone' },
    { key: 'email', label: 'Email' },
    { key: 'openingHours', label: 'Opening hours' },
    { key: 'status', label: 'Status', render: (showroom) => <StatusBadge value={showroom.isActive ? 'Active' : 'Inactive'} /> },
  ];

  return (
    <AdminPage title="Showrooms" subtitle="Read-only list from /api/showrooms.">
      {error && <ErrorState message={error} onRetry={loadShowrooms} />}
      <div className="card">
        <div className="card-body p-0">
          {loading ? <LoadingState label="Loading showrooms..." /> : <DataTable columns={columns} rows={showrooms} emptyTitle="No showrooms found" />}
        </div>
      </div>
    </AdminPage>
  );
};

export default Showrooms;
