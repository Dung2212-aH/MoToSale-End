import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import StatCard from '../components/StatCard';
import RevenueChart from '../components/charts/RevenueChart';
import OrderStatusChart from '../components/charts/OrderStatusChart';
import reportService from '../services/reportService';
import inventoryService from '../services/inventoryService';
import contactService from '../services/contactService';
import voucherService from '../services/voucherService';
import warrantyService from '../services/warrantyService';
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
    inventoryWarnings: [],
    crmTasks: [],
    operations: {},
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [operations, setOperations] = useState({
    awaitingOrders: 0,
    unpaidOrders: 0,
    shippingOrders: 0,
    outOfStock: 0,
    lowStock: 0,
    newContacts: 0,
    expiringVouchers: 0,
    activeWarranties: 0,
  });

  useEffect(() => {
    const fetchDashboard = async () => {
      setLoading(true);
      setError('');
      try {
        const dashboard = await reportService.getDashboard();
        setData(dashboard);
        const [inventoryRes, contactsRes, vouchersRes, warrantiesRes] = await Promise.allSettled([
          inventoryService.getAll({ page: 1, pageSize: 1 }),
          contactService.getAll({ status: 'New', page: 1, pageSize: 1 }),
          voucherService.getAll({ page: 1, pageSize: 100 }),
          warrantyService.getAll({ page: 1, pageSize: 100 }),
        ]);

        const orders = dashboard.orders || dashboard.recentOrders || [];
        const vouchers = vouchersRes.status === 'fulfilled' ? (vouchersRes.value.data.items || []) : [];
        const warranties = warrantiesRes.status === 'fulfilled' ? (warrantiesRes.value.data.items || []) : [];
        const now = new Date();
        const nextSevenDays = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
        const remoteOps = dashboard.operations || {};
        setOperations({
          awaitingOrders: remoteOps.pendingOrders ?? orders.filter((o) => ['AwaitingPayment', 'Pending', 'Checkout', 'Confirmed'].includes(o.trangThaiDonHang || o.status || o.orderStatus)).length,
          unpaidOrders: orders.filter((o) => ['Unpaid', 'Pending'].includes(o.trangThaiThanhToan || o.paymentStatus)).length,
          shippingOrders: remoteOps.shippingOrders ?? orders.filter((o) => ['Allocated', 'Shipped'].includes(o.trangThaiVanChuyen || o.shippingStatus || o.fulfillmentStatus)).length,
          outOfStock: remoteOps.outOfStock ?? (inventoryRes.status === 'fulfilled' ? (inventoryRes.value.data.summary?.outOfStock || 0) : 0),
          lowStock: remoteOps.lowStock ?? (inventoryRes.status === 'fulfilled' ? (inventoryRes.value.data.summary?.lowStock || 0) : 0),
          newContacts: contactsRes.status === 'fulfilled' ? (contactsRes.value.data.totalItems || contactsRes.value.data.items?.length || 0) : 0,
          expiringVouchers: vouchers.filter((v) => {
            const end = new Date(v.endsAt || v.endAt || v.ngayKetThuc);
            return !Number.isNaN(end.getTime()) && end >= now && end <= nextSevenDays;
          }).length,
          activeWarranties: remoteOps.openWarranties ?? warranties.filter((w) => ['Received', 'Processing', 'WaitingParts'].includes(w.trangThai || w.TrangThai || w.warrantyStatus)).length,
          pendingPurchases: remoteOps.pendingPurchases ?? 0,
          openRepairs: remoteOps.openRepairs ?? 0,
          openCrmTasks: remoteOps.openCrmTasks ?? 0,
          customerReceivable: remoteOps.customerReceivable ?? 0,
          supplierPayable: remoteOps.supplierPayable ?? 0,
          todayRevenue: remoteOps.todayRevenue ?? 0,
          paidTotal: remoteOps.paidTotal ?? 0,
          refundedTotal: remoteOps.refundedTotal ?? 0,
        });
      } catch (err) {
        setError('Không thể tải dữ liệu tổng quan. Vui lòng thử lại.');
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, []);

  const getOrderCode = (order) => order.maDonHangKinhDoanh || order.maDonHang || order.orderCode || order.code || order.id || 'N/A';
  const getOrderId = (order) => order.maDonHang || order.id;
  const getCustomerName = (order) => order.hoTenNhanHang || order.tenKhachHang || order.customerName || order.userName || 'Khách hàng';
  const getOrderAmount = (order) => order.tongThanhToan ?? order.tongTien ?? order.totalAmount ?? order.grandTotal ?? order.amount ?? 0;
  const getOrderStatus = (order) => reportService.getOrderStatusLabel(order);

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Tổng quan</h1>
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
                  to="/motorcycles"
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
                <StatCard color="primary" icon="fas fa-clipboard-check" label="Đơn cần xử lý" value={operations.awaitingOrders} to="/orders" />
                <StatCard color="warning" icon="fas fa-money-bill-wave" label="Chưa thanh toán" value={operations.unpaidOrders} to="/orders" />
                <StatCard color="info" icon="fas fa-truck" label="Đang giao/chuẩn bị" value={operations.shippingOrders} to="/orders" />
                <StatCard color="danger" icon="fas fa-box-open" label="Hết hàng" value={operations.outOfStock} to="/inventory" />
              </div>

              <div className="row">
                <StatCard color="warning" icon="fas fa-exclamation-triangle" label="Sắp hết hàng" value={operations.lowStock} to="/inventory" />
                <StatCard color="secondary" icon="fas fa-envelope" label="Liên hệ mới" value={operations.newContacts} to="/contacts" />
                <StatCard color="success" icon="fas fa-ticket-alt" label="Voucher sắp hết hạn" value={operations.expiringVouchers} to="/vouchers" />
                <StatCard color="info" icon="fas fa-tools" label="Bảo hành đang xử lý" value={operations.activeWarranties} to="/warranties" />
              </div>

              <div className="row">
                <StatCard color="success" icon="fas fa-calendar-day" label="Doanh thu hôm nay" value={formatCurrency(operations.todayRevenue || 0)} to="/reports" />
                <StatCard color="danger" icon="fas fa-hand-holding-usd" label="Còn phải thu" value={formatCurrency(operations.customerReceivable || 0)} to="/advanced-operations" />
                <StatCard color="warning" icon="fas fa-file-invoice-dollar" label="Cần trả NCC" value={formatCurrency(operations.supplierPayable || 0)} to="/business-operations" />
                <StatCard color="primary" icon="fas fa-phone-volume" label="CSKH cần xử lý" value={operations.openCrmTasks || 0} to="/business-operations" />
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
                                  <Link to={`/orders/${getOrderId(order)}`}>
                                    <strong>{getOrderCode(order)}</strong>
                                  </Link>
                                </td>
                                <td className="table-col-text">{getCustomerName(order)}</td>
                                <td className="table-col-money">{formatCurrency(getOrderAmount(order))}</td>
                                <td className="table-col-status"><span className="badge badge-info">{getOrderStatus(order)}</span></td>
                                <td className="table-col-date">{formatDate(order.ngayTao || order.createdAt || order.placedAt)}</td>
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

              <div className="row">
                <div className="col-lg-7">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">Cảnh báo tồn kho</h3>
                      <div className="card-tools">
                        <Link to="/inventory" className="btn btn-tool" title="Xem tồn kho"><i className="fas fa-external-link-alt"></i></Link>
                      </div>
                    </div>
                    <div className="card-body table-responsive p-0">
                      <table className="table table-bordered table-striped mb-0">
                        <thead>
                          <tr>
                            <th>SKU</th>
                            <th>Sản phẩm</th>
                            <th>Kho</th>
                            <th className="text-right">Khả dụng</th>
                            <th className="text-center">Cảnh báo</th>
                          </tr>
                        </thead>
                        <tbody>
                          {(data.inventoryWarnings || []).length === 0 ? (
                            <tr><td colSpan="5" className="text-center text-muted py-4">Không có cảnh báo tồn kho.</td></tr>
                          ) : data.inventoryWarnings.map((item) => (
                            <tr key={`${item.storeId}-${item.skuId}`}>
                              <td>{item.skuCode}</td>
                              <td>{item.productName}</td>
                              <td>{item.storeName}</td>
                              <td className="text-right">{item.available}</td>
                              <td className="text-center"><span className={`badge badge-${item.available <= 0 ? 'danger' : 'warning'}`}>{item.warningStatus}</span></td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
                <div className="col-lg-5">
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">CSKH cần xử lý</h3>
                      <div className="card-tools">
                        <Link to="/business-operations" className="btn btn-tool" title="Xem vận hành"><i className="fas fa-external-link-alt"></i></Link>
                      </div>
                    </div>
                    <div className="card-body table-responsive p-0">
                      <table className="table table-bordered table-striped mb-0">
                        <thead>
                          <tr>
                            <th>Khách hàng</th>
                            <th>Nội dung</th>
                            <th>Hẹn xử lý</th>
                          </tr>
                        </thead>
                        <tbody>
                          {(data.crmTasks || []).length === 0 ? (
                            <tr><td colSpan="3" className="text-center text-muted py-4">Không có lịch CSKH mở.</td></tr>
                          ) : data.crmTasks.slice(0, 8).map((task) => (
                            <tr key={task.id}>
                              <td>{task.customerName}</td>
                              <td>{task.subject}</td>
                              <td><span className={task.isOverdue ? 'text-danger font-weight-bold' : ''}>{formatDate(task.followUpAt)}</span></td>
                            </tr>
                          ))}
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
