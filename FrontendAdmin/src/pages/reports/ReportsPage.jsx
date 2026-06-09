import React, { useState, useEffect } from 'react';
import RevenueChart from '../../components/charts/RevenueChart';
import OrderStatusChart from '../../components/charts/OrderStatusChart';
import TopProductChart from '../../components/charts/TopProductChart';
import reportService from '../../services/reportService';
import businessOperationsService from '../../services/businessOperationsService';
import advancedOperationsService from '../../services/advancedOperationsService';
import inventoryService from '../../services/inventoryService';
import warrantyService from '../../services/warrantyService';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/formatDate';
import { createDateStamp, exportWorkbook } from '../../utils/exportExcel';

const getDefaultRange = () => {
  const end = new Date();
  const start = new Date();
  start.setDate(end.getDate() - 29);
  return {
    startDate: start.toISOString().slice(0, 10),
    endDate: end.toISOString().slice(0, 10),
  };
};

const asItems = (payload) => payload?.items || payload?.data || payload || [];
const inRange = (value, startDate, endDate) => {
  if (!value) return false;
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return false;
  return date >= new Date(startDate) && date <= new Date(`${endDate}T23:59:59`);
};

const ReportsPage = () => {
  const [activeTab, setActiveTab] = useState('sales');
  const [range, setRange] = useState(getDefaultRange);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [data, setData] = useState({
    revenueSeries: [],
    orderStatusSeries: [],
    topProducts: [],
    payments: [],
    orders: [],
    purchaseReports: [],
    cashReports: [],
    receivableReports: [],
    serviceReports: { repairs: [], warranties: [] },
    inventoryWarnings: [],
    stats: { productCount: 0, orderCount: 0, monthRevenue: 0, userCount: 0 },
  });

  const fetchReports = async () => {
    setLoading(true);
    setError('');
    try {
      const [
        result,
        purchaseRes,
        cashRes,
        receivableRes,
        repairRes,
        warrantyRes,
        inventoryRes,
      ] = await Promise.allSettled([
        reportService.getReports(range),
        businessOperationsService.getPurchases(),
        businessOperationsService.getCash(),
        advancedOperationsService.getReceivables(),
        businessOperationsService.getRepairs(),
        warrantyService.getAll({ page: 1, pageSize: 500 }),
        inventoryService.getAll({ page: 1, pageSize: 100, lowStockOnly: true }),
      ]);

      if (result.status !== 'fulfilled') throw result.reason;
      const base = result.value;
      const purchaseReports = purchaseRes.status === 'fulfilled'
        ? asItems(purchaseRes.value.data).filter((row) => inRange(row.createdDate || row.ngayTao, range.startDate, range.endDate))
        : [];
      const cashReports = cashRes.status === 'fulfilled'
        ? asItems(cashRes.value.data).filter((row) => inRange(row.occurredAt || row.ngayGiaoDich, range.startDate, range.endDate))
        : [];
      const receivableReports = receivableRes.status === 'fulfilled' ? asItems(receivableRes.value.data) : [];
      const repairs = repairRes.status === 'fulfilled'
        ? asItems(repairRes.value.data).filter((row) => inRange(row.receivedAt || row.ngayTiepNhan, range.startDate, range.endDate))
        : [];
      const warranties = warrantyRes.status === 'fulfilled'
        ? asItems(warrantyRes.value.data).filter((row) => inRange(row.receivedAt || row.ngayTiepNhan || row.ngayTao, range.startDate, range.endDate))
        : [];
      const inventoryWarnings = inventoryRes.status === 'fulfilled' ? asItems(inventoryRes.value.data) : [];

      setData({
        ...base,
        purchaseReports,
        cashReports,
        receivableReports,
        serviceReports: { repairs, warranties },
        inventoryWarnings,
      });
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

  const totalRevenue = data.stats.monthRevenue || 0;
  const totalOrders = data.orders.length;
  const revenueOrderCount = data.stats.revenueOrderCount || 0;
  const avgOrderValue = revenueOrderCount > 0 ? totalRevenue / revenueOrderCount : 0;

  const handleExportExcel = async () => {
    try {
      await exportWorkbook({
        fileName: `bao-cao-admin-${range.startDate}-${range.endDate}-${createDateStamp()}.xlsx`,
        sheets: [
          {
            name: 'HuongDan',
            columns: [
              { header: 'Mục', key: 'label', width: 30 },
              { header: 'Nội dung', key: 'value', width: 90 },
            ],
            rows: [
              { label: 'Tên báo cáo', value: 'Báo cáo doanh thu và vận hành showroom' },
              { label: 'Khoảng thời gian', value: `${range.startDate} đến ${range.endDate}` },
              { label: 'Sheet TongQuan', value: 'Các chỉ số chính để quản lý nhìn nhanh hiệu quả kinh doanh.' },
              { label: 'Sheet DoanhThuTheoNgay', value: 'Doanh thu từng ngày trong kỳ.' },
              { label: 'Sheet TrangThaiDonHang', value: 'Phân bổ đơn theo trạng thái.' },
              { label: 'Sheet SanPhamBanChay', value: 'Sản phẩm bán tốt trong kỳ.' },
              { label: 'Sheet MuaHang', value: 'Theo dõi đơn mua và công nợ nhà cung cấp.' },
              { label: 'Sheet ThuChi', value: 'Theo dõi phiếu thu/chi trong kỳ.' },
              { label: 'Sheet CongNo', value: 'Theo dõi các đơn còn phải thu từ khách.' },
              { label: 'Sheet DichVuSuaChua', value: 'Theo dõi phiếu sửa chữa trong kỳ.' },
              { label: 'Sheet BaoHanh', value: 'Theo dõi phiếu bảo hành trong kỳ.' },
              { label: 'Sheet CanhBaoTonKho', value: 'Theo dõi SKU hết hàng/sắp hết hàng.' },
            ],
          },
          {
            name: 'TongQuan',
            columns: [
              { header: 'Chỉ tiêu', key: 'label', width: 28 },
              { header: 'Giá trị', key: 'value', width: 24 },
            ],
            rows: [
              { label: 'Từ ngày', value: range.startDate },
              { label: 'Đến ngày', value: range.endDate },
              { label: 'Tổng doanh thu', value: formatCurrency(totalRevenue) },
              { label: 'Tổng đơn hàng', value: totalOrders },
              { label: 'Số đơn có doanh thu', value: revenueOrderCount },
              { label: 'Giá trị đơn trung bình', value: formatCurrency(avgOrderValue) },
            ],
          },
          {
            name: 'DoanhThuTheoNgay',
            columns: [
              { header: 'Ngày', key: 'label', width: 16 },
              { header: 'Doanh thu', key: 'value', type: 'currency', width: 18 },
            ],
            rows: data.revenueSeries,
          },
          {
            name: 'TrangThaiDonHang',
            columns: [
              { header: 'Nhóm trạng thái', key: 'label', width: 24 },
              { header: 'Số lượng', key: 'value', type: 'number', width: 14 },
            ],
            rows: data.orderStatusSeries,
          },
          {
            name: 'SanPhamBanChay',
            columns: [
              { header: 'STT', key: 'index', type: 'number', width: 8 },
              { header: 'Mã sản phẩm/SKU', key: 'id', width: 18 },
              { header: 'Tên sản phẩm', key: 'name', width: 36 },
              { header: 'Số lượng bán', key: 'sold', type: 'number', width: 16 },
              { header: 'Doanh thu', key: 'revenue', type: 'currency', width: 18 },
            ],
            rows: data.topProducts.map((product, index) => ({ index: index + 1, ...product })),
          },
          {
            name: 'MuaHang',
            columns: [
              { header: 'Mã đơn mua', key: 'code', width: 18 },
              { header: 'Nhà cung cấp', key: 'supplierName', width: 28 },
              { header: 'Trạng thái', key: 'purchaseStatus', width: 18 },
              { header: 'Tổng tiền', key: 'totalAmount', type: 'currency', width: 18 },
              { header: 'Đã trả', key: 'paidAmount', type: 'currency', width: 18 },
              { header: 'Còn phải trả', key: 'outstanding', type: 'currency', width: 18 },
              { header: 'Ngày tạo', key: 'createdDate', type: 'date', width: 20 },
            ],
            rows: data.purchaseReports,
          },
          {
            name: 'ThuChi',
            columns: [
              { header: 'Mã phiếu', key: 'code', width: 18 },
              { header: 'Loại', key: 'transactionType', width: 14 },
              { header: 'Nhóm', key: 'category', width: 18 },
              { header: 'Số tiền', key: 'amount', type: 'currency', width: 18 },
              { header: 'Hình thức', key: 'method', width: 16 },
              { header: 'Ngày ghi nhận', key: 'occurredAt', type: 'date', width: 20 },
              { header: 'Ghi chú', key: 'note', width: 36 },
            ],
            rows: data.cashReports,
          },
          {
            name: 'CongNo',
            columns: [
              { header: 'Mã đơn', key: 'orderCode', width: 18 },
              { header: 'Khách hàng', key: 'customerName', width: 28 },
              { header: 'Tổng đơn', key: 'grandTotal', type: 'currency', width: 18 },
              { header: 'Đã thu', key: 'paidAmount', type: 'currency', width: 18 },
              { header: 'Còn phải thu', key: 'outstanding', type: 'currency', width: 18 },
            ],
            rows: data.receivableReports,
          },
          {
            name: 'DichVuSuaChua',
            columns: [
              { header: 'Mã phiếu', key: 'code', width: 18 },
              { header: 'Xe', key: 'vehicleDescription', width: 28 },
              { header: 'Lỗi ghi nhận', key: 'reportedIssue', width: 36 },
              { header: 'Trạng thái', key: 'repairStatus', width: 18 },
              { header: 'Tổng phí', key: 'total', type: 'currency', width: 18 },
              { header: 'Ngày nhận', key: 'receivedAt', type: 'date', width: 20 },
            ],
            rows: data.serviceReports.repairs,
          },
          {
            name: 'BaoHanh',
            columns: [
              { header: 'Mã phiếu', key: 'code', width: 18 },
              { header: 'Sản phẩm', key: 'productSnapshot', width: 32 },
              { header: 'Khách hàng', key: 'customerName', width: 28 },
              { header: 'Trạng thái', key: 'status', width: 18 },
              { header: 'Ngày nhận', key: 'receivedAt', type: 'date', width: 20 },
            ],
            rows: data.serviceReports.warranties,
          },
          {
            name: 'CanhBaoTonKho',
            columns: [
              { header: 'SKU', key: 'SKU', width: 18 },
              { header: 'Sản phẩm', key: 'TenSanPham', width: 34 },
              { header: 'Tồn thực', key: 'TonKhoThucTe', type: 'number', width: 12 },
              { header: 'Đang giữ', key: 'SoLuongDangGiu', type: 'number', width: 12 },
              { header: 'Khả dụng', key: 'TonKhoKhaDung', type: 'number', width: 12 },
              { header: 'Ngưỡng thấp', key: 'MucCanhBaoTonThap', type: 'number', width: 12 },
              { header: 'Cảnh báo', key: 'TrangThaiTon', width: 18 },
            ],
            rows: data.inventoryWarnings,
          },
        ],
      });
    } catch (err) {
      alert('Xuất Excel thất bại. Vui lòng thử lại.');
    }
  };

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
          <div className="card">
            <div className="card-body">
              <form className="form-inline" onSubmit={handleApply}>
                <label className="mr-2 font-weight-bold">Khoảng thời gian:</label>
                <input type="date" className="form-control form-control-sm mr-2" value={range.startDate} onChange={(e) => setRange((prev) => ({ ...prev, startDate: e.target.value }))} />
                <span className="mr-2">đến</span>
                <input type="date" className="form-control form-control-sm mr-2" value={range.endDate} onChange={(e) => setRange((prev) => ({ ...prev, endDate: e.target.value }))} />
                <button type="submit" className="btn btn-primary btn-sm" disabled={loading}><i className="fas fa-filter"></i> Áp dụng</button>
                <button type="button" className="btn btn-success btn-sm ml-2" onClick={handleExportExcel} disabled={loading}>
                  <i className="fas fa-file-excel"></i> Xuất báo cáo
                </button>
              </form>
            </div>
          </div>

          {error && <div className="alert alert-danger">{error}</div>}

          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status"><span className="sr-only">Đang tải...</span></div>
            </div>
          ) : (
            <>
              <div className="card">
                <div className="card-header p-2">
                  <div className="nav nav-pills">
                    {[
                      ['sales', 'Bán hàng'],
                      ['purchase', 'Mua hàng'],
                      ['cash', 'Thu chi/Công nợ'],
                      ['service', 'Dịch vụ'],
                      ['inventory', 'Kho'],
                    ].map(([key, label]) => (
                      <button key={key} type="button" className={`nav-link ${activeTab === key ? 'active' : ''}`} onClick={() => setActiveTab(key)}>{label}</button>
                    ))}
                  </div>
                </div>
              </div>

              {activeTab === 'sales' && (
                <>
                  <div className="row">
                    <div className="col-lg-4 col-md-6"><InfoBox color="success" icon="fas fa-money-bill-wave" label="Tổng doanh thu" value={formatCurrency(totalRevenue)} /></div>
                    <div className="col-lg-4 col-md-6"><InfoBox color="info" icon="fas fa-receipt" label="Tổng đơn hàng" value={totalOrders} /></div>
                    <div className="col-lg-4 col-md-6"><InfoBox color="warning" icon="fas fa-calculator" label="Giá trị đơn trung bình" value={formatCurrency(avgOrderValue)} /></div>
                  </div>

                  <div className="card">
                    <div className="card-header"><h3 className="card-title"><i className="fas fa-chart-line mr-1"></i> Doanh thu theo ngày</h3></div>
                    <div className="card-body">
                      {data.revenueSeries.length > 0 ? <RevenueChart data={data.revenueSeries} label="Doanh thu" /> : <EmptyState icon="fas fa-chart-line" text="Không có dữ liệu doanh thu trong khoảng thời gian này." />}
                    </div>
                  </div>

                  <div className="row">
                    <div className="col-lg-5">
                      <div className="card">
                        <div className="card-header"><h3 className="card-title"><i className="fas fa-chart-pie mr-1"></i> Đơn hàng theo trạng thái</h3></div>
                        <div className="card-body">
                          {data.orderStatusSeries.length > 0 ? <OrderStatusChart data={data.orderStatusSeries} /> : <EmptyState icon="fas fa-chart-pie" text="Không có dữ liệu đơn hàng." />}
                        </div>
                      </div>
                    </div>
                    <div className="col-lg-7">
                      <div className="card">
                        <div className="card-header"><h3 className="card-title"><i className="fas fa-trophy mr-1"></i> Top 10 sản phẩm bán chạy</h3></div>
                        <div className="card-body">
                          {data.topProducts.length > 0 ? <TopProductChart data={data.topProducts} /> : <EmptyState icon="fas fa-trophy" text="Không có dữ liệu sản phẩm bán chạy." />}
                        </div>
                      </div>
                    </div>
                  </div>

                  <ReportTable title="Chi tiết sản phẩm bán chạy" description="Sản phẩm bán chạy trong kỳ." headers={['#', 'Sản phẩm', 'Số lượng bán', 'Doanh thu ước tính']}>
                    {data.topProducts.map((product, idx) => <tr key={product.id || idx}><td>{idx + 1}</td><td>{product.name}</td><td className="text-right">{product.sold}</td><td className="text-right">{formatCurrency(product.revenue)}</td></tr>)}
                  </ReportTable>
                </>
              )}

              {activeTab === 'purchase' && <ReportTable title="Báo cáo mua hàng" description="Theo dõi đơn mua, trạng thái nhận hàng và công nợ nhà cung cấp." headers={['Mã đơn mua', 'Nhà cung cấp', 'Trạng thái', 'Tổng tiền', 'Đã trả', 'Còn phải trả', 'Ngày tạo']}>
                {(data.purchaseReports || []).map((row) => <tr key={row.id}><td>{row.code}</td><td>{row.supplierName}</td><td>{row.purchaseStatus}</td><td className="text-right">{formatCurrency(row.totalAmount)}</td><td className="text-right">{formatCurrency(row.paidAmount)}</td><td className="text-right">{formatCurrency(row.outstanding)}</td><td>{formatDate(row.createdDate)}</td></tr>)}
              </ReportTable>}

              {activeTab === 'cash' && <>
                <ReportTable title="Báo cáo thu chi" description="Theo dõi các phiếu thu/chi và nguồn phát sinh trong kỳ." headers={['Mã phiếu', 'Loại', 'Nhóm', 'Số tiền', 'Hình thức', 'Ngày ghi nhận', 'Ghi chú']}>
                  {(data.cashReports || []).map((row) => <tr key={row.id}><td>{row.code}</td><td>{row.transactionType === 'Receipt' ? 'Thu' : 'Chi'}</td><td>{row.category}</td><td className="text-right">{formatCurrency(row.amount)}</td><td>{row.method}</td><td>{formatDate(row.occurredAt)}</td><td>{row.note || '-'}</td></tr>)}
                </ReportTable>
                <ReportTable title="Công nợ khách hàng" description="Các đơn còn phải thu theo API công nợ hiện tại." headers={['Mã đơn', 'Khách hàng', 'Tổng đơn', 'Đã thu', 'Còn phải thu']}>
                  {(data.receivableReports || []).map((row, index) => <tr key={row.orderId || index}><td>{row.orderCode || row.maDonHangKinhDoanh}</td><td>{row.customerName || row.tenKhachHang}</td><td className="text-right">{formatCurrency(row.grandTotal || row.tongThanhToan)}</td><td className="text-right">{formatCurrency(row.paidAmount || row.daThanhToan)}</td><td className="text-right font-weight-bold">{formatCurrency(row.outstanding || row.soTienConNo)}</td></tr>)}
                </ReportTable>
              </>}

              {activeTab === 'service' && <>
                <ReportTable title="Dịch vụ sửa chữa" description="Theo dõi phiếu sửa chữa, trạng thái và tổng phí." headers={['Mã phiếu', 'Xe', 'Lỗi ghi nhận', 'Trạng thái', 'Tổng phí', 'Ngày nhận']}>
                  {(data.serviceReports.repairs || []).map((row) => <tr key={row.id}><td>{row.code}</td><td>{row.vehicleDescription}</td><td>{row.reportedIssue}</td><td>{row.repairStatus}</td><td className="text-right">{formatCurrency(row.total)}</td><td>{formatDate(row.receivedAt)}</td></tr>)}
                </ReportTable>
                <ReportTable title="Bảo hành" description="Theo dõi phiếu bảo hành và thời hạn xử lý." headers={['Mã phiếu', 'Sản phẩm', 'Khách hàng', 'Trạng thái', 'Ngày nhận']}>
                  {(data.serviceReports.warranties || []).map((row, index) => <tr key={row.id || row.maBaoHanh || index}><td>{row.code || row.maBaoHanhKinhDoanh || row.id}</td><td>{row.productSnapshot || row.tenSanPham || '-'}</td><td>{row.customerName || row.tenKhachHang || '-'}</td><td>{row.status || row.trangThai}</td><td>{formatDate(row.receivedAt || row.ngayTiepNhan || row.ngayTao)}</td></tr>)}
                </ReportTable>
              </>}

              {activeTab === 'inventory' && <ReportTable title="Cảnh báo tồn kho" description="Danh sách SKU hết hàng hoặc sắp hết hàng theo ngưỡng cảnh báo." headers={['SKU', 'Sản phẩm', 'Tồn thực', 'Đang giữ', 'Khả dụng', 'Ngưỡng', 'Cảnh báo']}>
                {(data.inventoryWarnings || []).map((row, index) => <tr key={`${row.maSanPham}-${row.maBienSanPham || index}`}><td>{row.SKU || row.skuCode || '-'}</td><td>{row.TenSanPham || row.tenSanPham || row.productName}</td><td className="text-right">{row.TonKhoThucTe ?? row.tonKhoThucTe}</td><td className="text-right">{row.SoLuongDangGiu ?? row.soLuongDangGiu}</td><td className="text-right">{row.TonKhoKhaDung ?? row.tonKhoKhaDung}</td><td className="text-right">{row.MucCanhBaoTonThap ?? row.mucCanhBaoTonThap}</td><td className="text-center"><span className={`badge badge-${(row.TonKhoKhaDung ?? row.tonKhoKhaDung ?? 0) <= 0 ? 'danger' : 'warning'}`}>{row.TrangThaiTon || row.trangThaiTon}</span></td></tr>)}
              </ReportTable>}
            </>
          )}
        </div>
      </section>
    </div>
  );
};

const InfoBox = ({ color, icon, label, value }) => (
  <div className="info-box">
    <span className={`info-box-icon bg-${color}`}><i className={icon}></i></span>
    <div className="info-box-content">
      <span className="info-box-text">{label}</span>
      <span className="info-box-number">{value}</span>
    </div>
  </div>
);

const EmptyState = ({ icon, text }) => (
  <div className="text-center text-muted py-5">
    <i className={`${icon} fa-3x mb-3`}></i>
    <p>{text}</p>
  </div>
);

const ReportTable = ({ title, description, headers, children }) => (
  <div className="card">
    <div className="card-header"><h3 className="card-title">{title}</h3></div>
    <div className="card-body">
      <p className="text-muted">{description}</p>
      <div className="table-responsive">
        <table className="table table-bordered table-striped">
          <thead><tr>{headers.map((header) => <th key={header}>{header}</th>)}</tr></thead>
          <tbody>{React.Children.count(children) ? children : <tr><td colSpan={headers.length} className="text-center text-muted py-4">Chưa có dữ liệu.</td></tr>}</tbody>
        </table>
      </div>
    </div>
  </div>
);

export default ReportsPage;
