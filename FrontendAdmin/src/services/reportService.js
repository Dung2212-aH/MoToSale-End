import productService from './productService';
import orderService from './orderService';
import paymentService from './paymentService';
import userService from './userService';
import { getOrderStatusMeta } from '../utils/constants';
import { fetchAllPages } from '../utils/fetchAllPages';

const getOrderAmount = (order) => Number(order?.tongThanhToan ?? order?.tongTien ?? order?.totalAmount ?? order?.amount ?? 0);

const getOrderStatus = (order) => order?.trangThaiDonHang || order?.TrangThaiDonHang || order?.trangThai || order?.status || '';

const getPaymentStatus = (order) => order?.trangThaiThanhToan || order?.TrangThaiThanhToan || order?.paymentStatus || order?.thanhToan?.trangThai || order?.payment?.status || '';

const getDateValue = (item) => item?.ngayTao || item?.NgayTao || item?.createdAt || item?.createdDate || item?.paidAt || item?.ngayThanhToan;

const getPaymentDateValue = (item) => item?.ngayThanhToanThanhCong || item?.NgayThanhToanThanhCong || item?.ngayThanhToan || item?.paidAt || item?.createdAt || item?.ngayTao;

// DB hiện chỉ cho phép TrangThaiDonHang: AwaitingPayment / Confirmed / Cancelled.
// Doanh thu = đơn Confirmed đã thu tiền (TrangThaiThanhToan: Paid hoặc PartiallyPaid).
// Giữ Delivered/Completed như nhánh OR cho dữ liệu cũ (legacy, vô hại).
const REVENUE_PAYMENT_STATUSES = ['Paid', 'PartiallyPaid'];

const isRevenueOrder = (order) => {
  const status = getOrderStatus(order);
  return REVENUE_PAYMENT_STATUSES.includes(getPaymentStatus(order))
    && (status === 'Confirmed' || ['Delivered', 'Completed'].includes(status));
};

const isDateInRange = (value, start, end) => {
  const date = new Date(value);
  return !Number.isNaN(date.getTime()) && date >= start && date <= end;
};

const toDateKey = (date) => date.toISOString().slice(0, 10);

const formatShortDate = (date) => date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });

const buildDateBuckets = (fromDate, days) => {
  return Array.from({ length: days }, (_, index) => {
    const date = new Date(fromDate);
    date.setDate(date.getDate() + index);
    return {
      key: toDateKey(date),
      label: formatShortDate(date),
      value: 0,
    };
  });
};

const getStatusReportGroup = (status) => {
  const groups = {
    Pending: 'Chờ xử lý',
    Checkout: 'Chờ xử lý',
    AwaitingPayment: 'Chờ thanh toán',
    Confirmed: 'Đã xác nhận',
    Processing: 'Đang xử lý',
    Shipping: 'Đang xử lý',
    Delivered: 'Hoàn tất',
    Completed: 'Hoàn tất',
    Cancelled: 'Đã hủy',
    Canceled: 'Đã hủy',
  };
  return groups[status] || 'Khác';
};

const getProductName = (item) => item?.tenSanPham || item?.productName || item?.name || item?.maSanPham || item?.sku || 'Sản phẩm';

const getOrderItems = (order) => order?.items || order?.Items || order?.chiTiet || order?.ChiTiet || [];

const canReadUsers = () => {
  try {
    const user = JSON.parse(localStorage.getItem('admin_user') || '{}');
    const roles = user?.roles || user?.Roles || (user?.role ? [user.role] : []);
    return roles.includes('Admin');
  } catch {
    return false;
  }
};

const getSoldQuantity = (item) => {
  const variants = item?.variants || item?.bienThe || item?.bienSanPham || [];
  const fromVariants = Array.isArray(variants)
    ? variants.reduce((sum, variant) => sum + Number(variant?.soldQuantity || variant?.soLuongDaBan || 0), 0)
    : 0;

  return Number(item?.soldQuantity || item?.soLuongDaBan || item?.totalSold || item?.quantitySold || fromVariants || 0);
};

const buildTopProductsFromOrders = (orders, limit) => {
  const productMap = new Map();

  orders.filter(isRevenueOrder).forEach((order) => {
    getOrderItems(order).forEach((item) => {
      const id = item?.maSanPham || item?.MaSanPham || item?.productId || item?.id || item?.skuSnapshot || item?.SKUSnapshot || getProductName(item);
      const quantity = Number(item?.soLuong ?? item?.SoLuong ?? item?.quantity ?? 0);
      const revenue = Number(item?.thanhTien ?? item?.ThanhTien ?? 0) ||
        quantity * Number(item?.donGia ?? item?.DonGia ?? item?.unitPrice ?? 0);

      if (!id || quantity <= 0) return;

      const current = productMap.get(id) || {
        id,
        name: item?.tenSanPhamSnapshot || item?.TenSanPhamSnapshot || getProductName(item),
        sold: 0,
        revenue: 0,
      };

      current.sold += quantity;
      current.revenue += revenue;
      productMap.set(id, current);
    });
  });

  return Array.from(productMap.values())
    .sort((a, b) => b.sold - a.sold || b.revenue - a.revenue)
    .slice(0, limit);
};

const reportService = {
  getSummary: async () => {
    const usersRequest = canReadUsers()
      ? fetchAllPages(userService.getAll)
      : Promise.resolve({ items: [], total: 0 });

    const [productsRes, ordersRes, paymentsRes, usersRes] = await Promise.allSettled([
      fetchAllPages(productService.getAll),
      fetchAllPages(orderService.getAll),
      fetchAllPages(paymentService.getAll),
      usersRequest,
    ]);

    const productsPayload = productsRes.status === 'fulfilled' ? productsRes.value : { items: [], total: 0 };
    const ordersPayload = ordersRes.status === 'fulfilled' ? ordersRes.value : { items: [], total: 0 };
    const paymentsPayload = paymentsRes.status === 'fulfilled' ? paymentsRes.value : { items: [], total: 0 };
    const usersPayload = usersRes.status === 'fulfilled' ? usersRes.value : { items: [], total: 0 };

    const products = productsPayload.items;
    const orders = ordersPayload.items;
    const payments = paymentsPayload.items;
    const users = usersPayload.items;

    const now = new Date();
    const month = now.getMonth();
    const year = now.getFullYear();
    const monthRevenueOrders = orders.filter((order) => {
      if (!isRevenueOrder(order)) return false;
      const date = new Date(getPaymentDateValue(order));
      return !Number.isNaN(date.getTime()) && date.getMonth() === month && date.getFullYear() === year;
    });
    const monthRevenue = monthRevenueOrders.reduce((sum, order) => sum + getOrderAmount(order), 0);

    return {
      products,
      orders,
      payments,
      users,
      stats: {
        productCount: productsPayload.total,
        orderCount: ordersPayload.total,
        monthRevenue,
        revenueOrderCount: monthRevenueOrders.length,
        userCount: usersPayload.total,
      },
    };
  },

  getDashboard: async () => {
    const summary = await reportService.getSummary();
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 6);

    return {
      ...summary,
      revenueSeries: reportService.buildRevenueSeries(summary.orders, startDate, 7),
      orderStatusSeries: reportService.buildOrderStatusSeries(summary.orders),
      recentOrders: reportService.getRecentOrders(summary.orders, 5),
      topProducts: reportService.getTopProducts(summary.orders, summary.products, 5),
    };
  },

  getReports: async ({ startDate, endDate }) => {
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffDays = Math.max(1, Math.round((end - start) / 86400000) + 1);
    const usersRequest = canReadUsers()
      ? fetchAllPages(userService.getAll)
      : Promise.resolve({ items: [], total: 0 });

    const [productsRes, ordersRes, paymentsRes, usersRes] = await Promise.allSettled([
      fetchAllPages(productService.getAll),
      fetchAllPages(orderService.getAll),
      fetchAllPages(paymentService.getAll),
      usersRequest,
    ]);

    const products = productsRes.status === 'fulfilled' ? productsRes.value.items : [];
    const allOrders = ordersRes.status === 'fulfilled' ? ordersRes.value.items : [];
    const allPayments = paymentsRes.status === 'fulfilled' ? paymentsRes.value.items : [];
    const users = usersRes.status === 'fulfilled' ? usersRes.value.items : [];

    // Filter payments client-side by date (payment API may not support date filter)
    const payments = allPayments.filter((payment) => {
      const date = new Date(getDateValue(payment));
      return !Number.isNaN(date.getTime()) && date >= start && date <= new Date(`${endDate}T23:59:59`);
    });

    const rangeEnd = new Date(`${endDate}T23:59:59`);
    const orders = allOrders.filter((order) => isDateInRange(getDateValue(order), start, rangeEnd));
    const revenueOrders = allOrders.filter((order) =>
      isRevenueOrder(order) && isDateInRange(getPaymentDateValue(order), start, rangeEnd)
    );
    const totalRevenue = revenueOrders.reduce((sum, order) => sum + getOrderAmount(order), 0);

    return {
      products,
      orders,
      payments,
      users,
      stats: {
        productCount: products.length,
        orderCount: orders.length,
        monthRevenue: totalRevenue,
        revenueOrderCount: revenueOrders.length,
        userCount: users.length,
      },
      revenueSeries: reportService.buildRevenueSeries(orders, start, diffDays),
      orderStatusSeries: reportService.buildOrderStatusSeries(orders),
      topProducts: reportService.getTopProducts(orders, products, 10),
    };
  },

  buildRevenueSeries: (orders, fromDate, days) => {
    const buckets = buildDateBuckets(fromDate, days);
    const map = new Map(buckets.map((bucket) => [bucket.key, bucket]));

    orders.forEach((order) => {
      if (!isRevenueOrder(order)) return;
      const date = new Date(getPaymentDateValue(order));
      if (Number.isNaN(date.getTime())) return;
      const bucket = map.get(toDateKey(date));
      if (bucket) {
        bucket.value += getOrderAmount(order);
      }
    });

    return buckets;
  },

  buildOrderStatusSeries: (orders) => {
    const counts = orders.reduce((acc, order) => {
      const group = getStatusReportGroup(getOrderStatus(order));
      acc[group] = (acc[group] || 0) + 1;
      return acc;
    }, {});

    return Object.entries(counts).map(([label, value]) => ({
      label,
      value,
    }));
  },

  getOrderStatusLabel: (order) => getOrderStatusMeta(getOrderStatus(order)).label,

  getRecentOrders: (orders, limit) => {
    return [...orders]
      .sort((a, b) => new Date(getDateValue(b)) - new Date(getDateValue(a)))
      .slice(0, limit);
  },

  getTopProducts: (orders, products, limit) => {
    const fromOrders = buildTopProductsFromOrders(orders, limit);
    if (fromOrders.length > 0) return fromOrders;

    return [...products]
      .map((product) => ({
        id: product?.id || product?.maSanPham || product?.productId,
        name: getProductName(product),
        sold: getSoldQuantity(product),
        revenue: getSoldQuantity(product) * Number(product?.giaBan || product?.price || product?.basePrice || 0),
      }))
      .sort((a, b) => b.sold - a.sold)
      .slice(0, limit);
  },
};

export default reportService;
