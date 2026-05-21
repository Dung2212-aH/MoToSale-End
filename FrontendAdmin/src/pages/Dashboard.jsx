import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import StatCard from '../components/StatCard';
import RevenueChart from '../components/charts/RevenueChart';
import OrderStatusChart from '../components/charts/OrderStatusChart';
import reportService from '../services/reportService';
import { formatCurrency } from '../utils/formatCurrency';
import { formatDate } from '../utils/formatDate';

const Dashboard = () => {
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
  const getOrderAmount = (order) => order.tongTien || order.totalAmount || order.amount || 0;
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
                <StatCard
                  color="warning"
                  icon="fas fa-users"
                  label="Người dùng"
                  value={data.stats.userCount}
                  to="/users"
                />
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
                            <th>Mã đơn</th>
                            <th>Khách hàng</th>
                            <th>Tổng tiền</th>
                            <th>Trạng thái</th>
                            <th>Ngày tạo</th>
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
                                <td>
                                  <Link to={`/orders/${order.id || order.maDonHang}`}>
                                    <strong>{getOrderCode(order)}</strong>
                                  </Link>
                                </td>
                                <td>{getCustomerName(order)}</td>
                                <td>{formatCurrency(getOrderAmount(order))}</td>
                                <td><span className="badge badge-info">{getOrderStatus(order)}</span></td>
                                <td>{formatDate(order.ngayTao || order.createdAt)}</td>
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
                            <th>Sản phẩm</th>
                            <th className="text-right">Đã bán</th>
                            <th className="text-right">Doanh thu</th>
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
                                <td>{product.name}</td>
                                <td className="text-right">{product.sold}</td>
                                <td className="text-right">{formatCurrency(product.revenue)}</td>
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
