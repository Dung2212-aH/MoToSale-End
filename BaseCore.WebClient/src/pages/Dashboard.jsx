import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import AdminPage from '../components/admin/AdminPage';
import DataTable from '../components/admin/DataTable';
import { ErrorState, LoadingState } from '../components/admin/UiState';
import { formatMoney, StatusBadge } from '../components/admin/FormControls';
import { categoryApi, getApiErrorMessage, normalizePagedResponse, orderApi, productApi, showroomApi } from '../services/api';

const StatCard = ({ label, value, meta, icon, color = 'primary', to }) => {
  const card = (
    <div className="card admin-stat-card">
      <div className="card-body">
        <span className={`admin-stat-icon bg-${color}`}>
          <i className={`${icon} text-white`}></i>
        </span>
        <div>
          <div className="admin-stat-label">{label}</div>
          <div className="admin-stat-value">{value}</div>
          {meta && <div className="admin-stat-meta">{meta}</div>}
        </div>
      </div>
    </div>
  );

  return to ? <Link to={to} className="text-reset text-decoration-none">{card}</Link> : card;
};

const Dashboard = () => {
  const [stats, setStats] = useState({ products: 0, categories: 0, showrooms: 0, orders: 0, lowStock: 0 });
  const [lowStockProducts, setLowStockProducts] = useState([]);
  const [recentOrders, setRecentOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadDashboard = async () => {
    setLoading(true);
    setError('');
    try {
      const [productResponse, categoryResponse, showroomResponse, orderResponse] = await Promise.allSettled([
        productApi.getAll({ page: 1, pageSize: 100 }),
        categoryApi.getAll({ activeOnly: false }),
        showroomApi.getAll({ activeOnly: false }),
        orderApi.getAll({ page: 1, pageSize: 6 }),
      ]);

      if (productResponse.status === 'rejected') {
        throw productResponse.reason;
      }

      const productPage = productResponse.value.data;
      const products = productPage.items || [];
      const lowStock = products.filter((product) => Number(product.stockQuantity || 0) <= 5);
      const orders = orderResponse.status === 'fulfilled'
        ? normalizePagedResponse(orderResponse.value.data).items
        : [];

      setStats({
        products: productPage.totalCount,
        categories: categoryResponse.status === 'fulfilled' ? categoryResponse.value.data.length : 0,
        showrooms: showroomResponse.status === 'fulfilled' ? showroomResponse.value.data.length : 0,
        orders: orderResponse.status === 'fulfilled' ? normalizePagedResponse(orderResponse.value.data).totalCount : 0,
        lowStock: lowStock.length,
      });
      setLowStockProducts(lowStock.slice(0, 6));
      setRecentOrders(orders);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc dashboard.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDashboard();
  }, []);

  const orderColumns = [
    { key: 'maDonHangKinhDoanh', label: 'Order', render: (order) => <strong>{order.maDonHangKinhDoanh || `#${order.maDonHang}`}</strong> },
    { key: 'tongThanhToan', label: 'Total', className: 'text-right', render: (order) => formatMoney(order.tongThanhToan) },
    { key: 'trangThaiDonHang', label: 'Status', render: (order) => <StatusBadge value={order.trangThaiDonHang} /> },
  ];

  const stockColumns = [
    { key: 'name', label: 'Product', render: (product) => <strong>{product.name}</strong> },
    { key: 'stockQuantity', label: 'Stock', className: 'text-right' },
    { key: 'status', label: 'Status', render: (product) => <StatusBadge value={product.status} /> },
  ];

  return (
    <AdminPage
      title="Dashboard"
      subtitle="Overview from available backend endpoints."
      actions={(
        <button type="button" className="btn btn-outline-primary btn-sm" onClick={loadDashboard}>
          <i className="fas fa-sync-alt mr-1"></i>
          Refresh
        </button>
      )}
    >
      {loading ? <LoadingState label="Loading dashboard..." /> : error ? <ErrorState message={error} onRetry={loadDashboard} /> : (
        <>
          <div className="row">
            <div className="col-xl-3 col-md-6">
              <StatCard label="Products" value={stats.products} meta={`${stats.lowStock} low stock`} icon="fas fa-box" color="primary" to="/products" />
            </div>
            <div className="col-xl-3 col-md-6">
              <StatCard label="Categories" value={stats.categories} meta="Catalog groups" icon="fas fa-tags" color="success" to="/categories" />
            </div>
            <div className="col-xl-3 col-md-6">
              <StatCard label="Showrooms" value={stats.showrooms} meta="Active and inactive" icon="fas fa-store" color="info" to="/showrooms" />
            </div>
            <div className="col-xl-3 col-md-6">
              <StatCard label="Orders" value={stats.orders} meta="Visible to current account" icon="fas fa-receipt" color="warning" to="/orders" />
            </div>
          </div>

          <div className="row">
            <div className="col-lg-7">
              <div className="card">
                <div className="card-header">
                  <h3 className="admin-section-title">Recent orders</h3>
                </div>
                <div className="card-body p-0">
                  <DataTable columns={orderColumns} rows={recentOrders} rowKey="maDonHang" emptyTitle="No orders found" />
                </div>
              </div>
            </div>
            <div className="col-lg-5">
              <div className="card">
                <div className="card-header">
                  <h3 className="admin-section-title">Low stock</h3>
                </div>
                <div className="card-body p-0">
                  <DataTable columns={stockColumns} rows={lowStockProducts} emptyTitle="Stock is healthy" />
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </AdminPage>
  );
};

export default Dashboard;
