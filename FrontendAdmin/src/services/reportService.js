import productService from './productService';
import orderService from './orderService';
import paymentService from './paymentService';
import userService from './userService';

const unwrapList = (payload) => {
  const data = payload?.data ?? payload;
  if (Array.isArray(data)) return data;
  if (Array.isArray(data?.items)) return data.items;
  if (Array.isArray(data?.data)) return data.data;
  if (Array.isArray(data?.result)) return data.result;
  return [];
};

const unwrapTotal = (payload) => {
  const data = payload?.data ?? payload;
  const list = unwrapList(data);
  return Number(data?.total ?? data?.totalCount ?? data?.count ?? list.length ?? 0);
};

const getAmount = (item) => Number(item?.soTien ?? item?.tongTien ?? item?.amount ?? item?.totalAmount ?? item?.paidAmount ?? 0);

const getDateValue = (item) => item?.ngayTao || item?.createdAt || item?.createdDate || item?.paidAt || item?.ngayThanhToan;

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

const getStatusLabel = (status) => {
  const labels = {
    Pending: 'Chờ xử lý',
    Confirmed: 'Đã xác nhận',
    Paid: 'Đã thanh toán',
    Completed: 'Hoàn tất',
    Cancelled: 'Đã hủy',
    Canceled: 'Đã hủy',
    Processing: 'Đang xử lý',
    Shipping: 'Đang giao',
  };
  return labels[status] || status || 'Khác';
};

const getProductName = (item) => item?.tenSanPham || item?.productName || item?.name || item?.maSanPham || item?.sku || 'Sản phẩm';

const getSoldQuantity = (item) => {
  const variants = item?.variants || item?.bienThe || item?.bienSanPham || [];
  const fromVariants = Array.isArray(variants)
    ? variants.reduce((sum, variant) => sum + Number(variant?.soldQuantity || variant?.soLuongDaBan || 0), 0)
    : 0;

  return Number(item?.soldQuantity || item?.soLuongDaBan || item?.totalSold || item?.quantitySold || fromVariants || 0);
};

const reportService = {
  getSummary: async () => {
    const [productsRes, ordersRes, paymentsRes, usersRes] = await Promise.allSettled([
      productService.getAll({ page: 1, pageSize: 100 }),
      orderService.getAll({ page: 1, pageSize: 100 }),
      paymentService.getAll({ page: 1, pageSize: 100 }),
      userService.getAll({ page: 1, pageSize: 100 }),
    ]);

    const productsPayload = productsRes.status === 'fulfilled' ? productsRes.value : {};
    const ordersPayload = ordersRes.status === 'fulfilled' ? ordersRes.value : {};
    const paymentsPayload = paymentsRes.status === 'fulfilled' ? paymentsRes.value : {};
    const usersPayload = usersRes.status === 'fulfilled' ? usersRes.value : {};

    const products = unwrapList(productsPayload);
    const orders = unwrapList(ordersPayload);
    const payments = unwrapList(paymentsPayload);
    const users = unwrapList(usersPayload);

    const now = new Date();
    const month = now.getMonth();
    const year = now.getFullYear();
    const monthRevenue = payments.reduce((sum, payment) => {
      const date = new Date(getDateValue(payment));
      if (Number.isNaN(date.getTime()) || date.getMonth() !== month || date.getFullYear() !== year) {
        return sum;
      }
      return sum + getAmount(payment);
    }, 0);

    return {
      products,
      orders,
      payments,
      users,
      stats: {
        productCount: unwrapTotal(productsPayload),
        orderCount: unwrapTotal(ordersPayload),
        monthRevenue,
        userCount: unwrapTotal(usersPayload),
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
      revenueSeries: reportService.buildRevenueSeries(summary.payments, startDate, 7),
      orderStatusSeries: reportService.buildOrderStatusSeries(summary.orders),
      recentOrders: reportService.getRecentOrders(summary.orders, 5),
      topProducts: reportService.getTopProducts(summary.products, 5),
    };
  },

  getReports: async ({ startDate, endDate }) => {
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffDays = Math.max(1, Math.round((end - start) / 86400000) + 1);

    // Fetch with date filters where supported
    const [productsRes, ordersRes, paymentsRes, usersRes] = await Promise.allSettled([
      productService.getAll({ page: 1, pageSize: 100 }),
      orderService.getAll({ page: 1, pageSize: 100, tuNgay: startDate, denNgay: endDate }),
      paymentService.getAll({ page: 1, pageSize: 100 }),
      userService.getAll({ page: 1, pageSize: 100 }),
    ]);

    const products = unwrapList(productsRes.status === 'fulfilled' ? productsRes.value : {});
    const orders = unwrapList(ordersRes.status === 'fulfilled' ? ordersRes.value : {});
    const allPayments = unwrapList(paymentsRes.status === 'fulfilled' ? paymentsRes.value : {});
    const users = unwrapList(usersRes.status === 'fulfilled' ? usersRes.value : {});

    // Filter payments client-side by date (payment API may not support date filter)
    const payments = allPayments.filter((payment) => {
      const date = new Date(getDateValue(payment));
      return !Number.isNaN(date.getTime()) && date >= start && date <= new Date(`${endDate}T23:59:59`);
    });

    const totalRevenue = payments.reduce((sum, p) => sum + getAmount(p), 0);

    return {
      products,
      orders,
      payments,
      users,
      stats: {
        productCount: products.length,
        orderCount: orders.length,
        monthRevenue: totalRevenue,
        userCount: users.length,
      },
      revenueSeries: reportService.buildRevenueSeries(payments, start, diffDays),
      orderStatusSeries: reportService.buildOrderStatusSeries(orders),
      topProducts: reportService.getTopProducts(products, 10),
    };
  },

  buildRevenueSeries: (payments, fromDate, days) => {
    const buckets = buildDateBuckets(fromDate, days);
    const map = new Map(buckets.map((bucket) => [bucket.key, bucket]));

    payments.forEach((payment) => {
      const date = new Date(getDateValue(payment));
      if (Number.isNaN(date.getTime())) return;
      const bucket = map.get(toDateKey(date));
      if (bucket) {
        bucket.value += getAmount(payment);
      }
    });

    return buckets;
  },

  buildOrderStatusSeries: (orders) => {
    const counts = orders.reduce((acc, order) => {
      const status = order?.trangThai || order?.status || 'Khác';
      acc[status] = (acc[status] || 0) + 1;
      return acc;
    }, {});

    return Object.entries(counts).map(([status, value]) => ({
      label: getStatusLabel(status),
      value,
    }));
  },

  getRecentOrders: (orders, limit) => {
    return [...orders]
      .sort((a, b) => new Date(getDateValue(b)) - new Date(getDateValue(a)))
      .slice(0, limit);
  },

  getTopProducts: (products, limit) => {
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
