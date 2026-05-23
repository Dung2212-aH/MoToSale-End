import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import StatCard from '../components/StatCard';
import RevenueChart from '../components/charts/RevenueChart';
import OrderStatusChart from '../components/charts/OrderStatusChart';
import reportService from '../services/reportService';
import { useAuth } from '../contexts/AuthContext';
import { formatCurrency } from '../utils/formatCurrency';
import { formatDate } from '../utils/formatDate';

const Dashboard = () => {
  const { isAdmin } = useAuth();
  const [data, setData] = useState({
    stats: {
      productCount: 0,
      orderCount: 0,
      monthRevenue: 0,
      userCount: 0,
    },
    revenueSeries: [],
    orderStatusSeries: [],
    recentOrders: [],
    topProducts: [],
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchDashboard = async () => {
      setLoading(true);
      setError('');
      try {
        const dashboard = await reportService.getDashboard();
        setData(dashboard);
      } catch (err) {
        setError('Không thể tải dữ liệu tổng quan. Vui lòng thử lại.');
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, []);

  const getOrderCode = (order) => order.maDonHang || order.orderCode || order.id || 'N/A';
  const getCustomerName = (order) => order.tenKhachHang || order.customerName || order.userName || 'Khách hàng';
  const getOrderAmount = (order) => order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? order.amount ?? 0;
  const getOrderStatus = (order) => order.trangThai || order.status || 'Mới';

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Tổng quan</h1>
            </div>
            <div className="col-sm-6">
              <ol className="breadcrumb float-sm-right">
                <li className="breadcrumb-item active">Dashboard</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {error && <div className="alert alert-danger">{error}</div>}

          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="sr-only">Đang tải...</span>
              </div>
            </div>
          ) : (
            <>
              <div className="row">
                <StatCard
                  color="info"
                  icon="fas fa-motorcycle"
                  label="Tổng sản phẩm"
                  value={data.stats.productCount}
                  to="/products"
                />
                <StatCard
                  color="success"
                  icon="fas fa-shopping-cart"
                  label="Tổng đơn hàng"
                  value={data.stats.orderCount}
                  to="/orders"
                />
                {isAdmin() && (
                <StatCard
                  color="warning"
                  icon="fas fa-users"
                  label="Người dùng"
                  value={data.stats.userCount}
                  to="/users"
                />
                )}
                <StatCard
                  color="danger"
                  icon="fas fa-chart-line"
                  label="Doanh thu tháng"
                  value={formatCurrency(data.stats.monthRevenue)}
                  to="/reports"
                />
              </div>

              <div className="row">
                <div className="col-lg-8">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">Doanh thu 7 ngày gần nhất</h3>
                    </div>
                    <div className="card-body">
                      <RevenueChart data={data.revenueSeries} />
                    </div>
                  </div>
                </div>

                <div className="col-lg-4">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">Đơn hàng theo trạng thái</h3>
                    </div>
                    <div className="card-body">
                      {data.orderStatusSeries.length > 0 ? (
                        <OrderStatusChart data={data.orderStatusSeries} />
                      ) : (
                        <div className="text-center text-muted py-5">
                          <i className="fas fa-chart-pie fa-3x mb-3"></i>
                          <p>Chưa có dữ liệu đơn hàng.</p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              <div className="row">
                <div className="col-lg-7">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">Đơn hàng mới nhất</h3>
                      <div className="card-tools">
                        <Link to="/orders" className="btn btn-tool" title="Xem tất cả">
                          <i className="fas fa-external-link-alt"></i>
                        </Link>
                      </div>
                    </div>
                    <div className="card-body table-responsive p-0">
                      <table className="table table-bordered table-striped mb-0">
                        <thead>
                          <tr>
                            <th className="table-col-code">Mã đơn</th>
                            <th className="table-col-text">Khách hàng</th>
                            <th className="table-col-money">Tổng tiền</th>
                            <th className="table-col-status">Trạng thái</th>
                            <th className="table-col-date">Ngày tạo</th>
                          </tr>
                        </thead>
                        <tbody>
                          {data.recentOrders.length === 0 ? (
                            <tr>
                              <td colSpan="5" className="text-center text-muted py-4">
                                Chưa có đơn hàng mới.
                              </td>
                            </tr>
                          ) : (
                            data.recentOrders.map((order) => (
                              <tr key={getOrderCode(order)}>
                                <td className="table-col-code">
                                  <Link to={`/orders/${order.id || order.maDonHang}`}>
                                    <strong>{getOrderCode(order)}</strong>
                                  </Link>
                                </td>
                                <td className="table-col-text">{getCustomerName(order)}</td>
                                <td className="table-col-money">{formatCurrency(getOrderAmount(order))}</td>
                                <td className="table-col-status"><span className="badge badge-info">{getOrderStatus(order)}</span></td>
                                <td className="table-col-date">{formatDate(order.ngayTao || order.createdAt)}</td>
                              </tr>
                            ))
                          )}
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>

                <div className="col-lg-5">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">Top sản phẩm bán chạy</h3>
                    </div>
                    <div className="card-body p-0">
                      <table className="table table-bordered table-striped mb-0">
                        <thead>
                          <tr>
                            <th className="table-col-text">Sản phẩm</th>
                            <th className="table-col-number">Đã bán</th>
                            <th className="table-col-money">Doanh thu</th>
                          </tr>
                        </thead>
                        <tbody>
                          {data.topProducts.length === 0 ? (
                            <tr>
                              <td colSpan="3" className="text-center text-muted py-4">
                                Chưa có dữ liệu bán chạy.
                              </td>
                            </tr>
                          ) : (
                            data.topProducts.map((product) => (
                              <tr key={product.id || product.name}>
                                <td className="table-col-text">{product.name}</td>
                                <td className="table-col-number">{product.sold}</td>
                                <td className="table-col-money">{formatCurrency(product.revenue)}</td>
                              </tr>
                            ))
                          )}
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
              </div>
            </>
          )}
        </div>
      </section>
    </div>
  );
};

export default Dashboard;
