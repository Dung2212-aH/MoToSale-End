import React, { useState, useEffect } from 'react';
import RevenueChart from '../../components/charts/RevenueChart';
import OrderStatusChart from '../../components/charts/OrderStatusChart';
import TopProductChart from '../../components/charts/TopProductChart';
import reportService from '../../services/reportService';
import { formatCurrency } from '../../utils/formatCurrency';

const getDefaultRange = () => {
  const end = new Date();
  const start = new Date();
  start.setDate(end.getDate() - 29);
  return {
    startDate: start.toISOString().slice(0, 10),
    endDate: end.toISOString().slice(0, 10),
  };
};

const ReportsPage = () => {
  const [range, setRange] = useState(getDefaultRange);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [data, setData] = useState({
    revenueSeries: [],
    orderStatusSeries: [],
    topProducts: [],
    payments: [],
    orders: [],
    stats: { productCount: 0, orderCount: 0, monthRevenue: 0, userCount: 0 },
  });

  const fetchReports = async () => {
    setLoading(true);
    setError('');
    try {
      const result = await reportService.getReports(range);
      setData(result);
    } catch (err) {
      setError('Không thể tải dữ liệu báo cáo. Vui lòng thử lại.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, []);

  const handleApply = (e) => {
    e.preventDefault();
    fetchReports();
  };

  const totalRevenue = data.payments.reduce(
    (sum, p) => sum + Number(p.soTien || p.amount || p.tongTien || p.totalAmount || 0),
    0
  );
  const totalOrders = data.orders.length;
  const avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Báo cáo & Thống kê</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {/* Date Range Picker */}
          <div className="card">
            <div className="card-body">
              <form className="form-inline" onSubmit={handleApply}>
                <label className="mr-2 font-weight-bold">Khoảng thời gian:</label>
                <input
                  type="date"
                  className="form-control form-control-sm mr-2"
                  value={range.startDate}
                  onChange={(e) => setRange((prev) => ({ ...prev, startDate: e.target.value }))}
                />
                <span className="mr-2">đến</span>
                <input
                  type="date"
                  className="form-control form-control-sm mr-2"
                  value={range.endDate}
                  onChange={(e) => setRange((prev) => ({ ...prev, endDate: e.target.value }))}
                />
                <button type="submit" className="btn btn-primary btn-sm" disabled={loading}>
                  <i className="fas fa-filter"></i> Áp dụng
                </button>
              </form>
            </div>
          </div>

          {error && <div className="alert alert-danger">{error}</div>}

          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="sr-only">Đang tải...</span>
              </div>
            </div>
          ) : (
            <>
              {/* Summary Cards */}
              <div className="row">
                <div className="col-lg-4 col-md-6">
                  <div className="info-box">
                    <span className="info-box-icon bg-success"><i className="fas fa-money-bill-wave"></i></span>
                    <div className="info-box-content">
                      <span className="info-box-text">Tổng doanh thu</span>
                      <span className="info-box-number">{formatCurrency(totalRevenue)}</span>
                    </div>
                  </div>
                </div>
                <div className="col-lg-4 col-md-6">
                  <div className="info-box">
                    <span className="info-box-icon bg-info"><i className="fas fa-receipt"></i></span>
                    <div className="info-box-content">
                      <span className="info-box-text">Tổng đơn hàng</span>
                      <span className="info-box-number">{totalOrders}</span>
                    </div>
                  </div>
                </div>
                <div className="col-lg-4 col-md-6">
                  <div className="info-box">
                    <span className="info-box-icon bg-warning"><i className="fas fa-calculator"></i></span>
                    <div className="info-box-content">
                      <span className="info-box-text">Giá trị đơn trung bình</span>
                      <span className="info-box-number">{formatCurrency(avgOrderValue)}</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Revenue Chart */}
              <div className="card">
                <div className="card-header">
                  <h3 className="card-title">
                    <i className="fas fa-chart-line mr-1"></i> Doanh thu theo ngày
                  </h3>
                </div>
                <div className="card-body">
                  {data.revenueSeries.length > 0 ? (
                    <RevenueChart data={data.revenueSeries} label="Doanh thu" />
                  ) : (
                    <div className="text-center text-muted py-5">
                      <i className="fas fa-chart-line fa-3x mb-3"></i>
                      <p>Không có dữ liệu doanh thu trong khoảng thời gian này.</p>
                    </div>
                  )}
                </div>
              </div>

              {/* Order Status + Top Products */}
              <div className="row">
                <div className="col-lg-5">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">
                        <i className="fas fa-chart-pie mr-1"></i> Đơn hàng theo trạng thái
                      </h3>
                    </div>
                    <div className="card-body">
                      {data.orderStatusSeries.length > 0 ? (
                        <OrderStatusChart data={data.orderStatusSeries} />
                      ) : (
                        <div className="text-center text-muted py-5">
                          <i className="fas fa-chart-pie fa-3x mb-3"></i>
                          <p>Không có dữ liệu đơn hàng.</p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
                <div className="col-lg-7">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">
                        <i className="fas fa-trophy mr-1"></i> Top 10 sản phẩm bán chạy
                      </h3>
                    </div>
                    <div className="card-body">
                      {data.topProducts.length > 0 ? (
                        <TopProductChart data={data.topProducts} />
                      ) : (
                        <div className="text-center text-muted py-5">
                          <i className="fas fa-trophy fa-3x mb-3"></i>
                          <p>Không có dữ liệu sản phẩm bán chạy.</p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* Top Products Table */}
              <div className="card">
                <div className="card-header">
                  <h3 className="card-title">
                    <i className="fas fa-list-ol mr-1"></i> Chi tiết sản phẩm bán chạy
                  </h3>
                </div>
                <div className="card-body p-0">
                  <table className="table table-bordered table-striped mb-0">
                    <thead>
                      <tr>
                        <th className="table-col-code">#</th>
                        <th className="table-col-text">Sản phẩm</th>
                        <th className="table-col-number">Số lượng bán</th>
                        <th className="table-col-money">Doanh thu ước tính</th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.topProducts.length === 0 ? (
                        <tr>
                          <td colSpan="4" className="text-center text-muted py-4">
                            Không có dữ liệu.
                          </td>
                        </tr>
                      ) : (
                        data.topProducts.map((product, idx) => (
                          <tr key={product.id || idx}>
                            <td className="table-col-code">{idx + 1}</td>
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
            </>
          )}
        </div>
      </section>
    </div>
  );
};

export default ReportsPage;
