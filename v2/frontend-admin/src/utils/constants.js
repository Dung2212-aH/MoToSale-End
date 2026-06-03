export const ORDER_STATUS_LABELS = {
  Pending: { label: 'Legacy: chờ xử lý', color: 'secondary' },
  Checkout: { label: 'Đang checkout', color: 'secondary' },
  AwaitingPayment: { label: 'Chờ thanh toán / xác nhận', color: 'warning' },
  Confirmed: { label: 'Đã xác nhận', color: 'primary' },
  Allocated: { label: 'Đã phân bổ', color: 'info' },
  Shipping: { label: 'Đang giao', color: 'info' },
  Delivered: { label: 'Đã giao', color: 'success' },
  Completed: { label: 'Hoàn tất', color: 'success' },
  Cancelled: { label: 'Đã hủy', color: 'danger' },
};

export const ORDER_STATUS_OPTIONS = [
  { value: 'AwaitingPayment', label: 'Chờ thanh toán / xác nhận' },
  { value: 'Confirmed', label: 'Đã xác nhận' },
  { value: 'Allocated', label: 'Đã phân bổ' },
  { value: 'Shipping', label: 'Đang giao' },
  { value: 'Delivered', label: 'Đã giao' },
  { value: 'Completed', label: 'Hoàn tất' },
  { value: 'Cancelled', label: 'Đã hủy' },
];

export const ORDER_NEXT_STATUS = {
  Pending: ['AwaitingPayment', 'Confirmed', 'Cancelled'],
  Checkout: ['AwaitingPayment', 'Confirmed', 'Cancelled'],
  AwaitingPayment: ['Confirmed', 'Cancelled'],
  Confirmed: ['Allocated', 'Cancelled'],
  Allocated: ['Shipping', 'Cancelled'],
  Shipping: ['Delivered'],
  Delivered: ['Completed'],
  Completed: [],
  Cancelled: [],
};

export const normalizeOrderStatus = (status) => String(status || '');

export const getOrderStatusMeta = (status) => (
  ORDER_STATUS_LABELS[normalizeOrderStatus(status)] || { label: status || 'Khác', color: 'secondary' }
);

export const PAYMENT_STATUS = {
  Unpaid: { label: 'Chưa thanh toán', color: 'secondary' },
  Pending: { label: 'Chờ xác nhận thanh toán', color: 'warning' },
  PartiallyPaid: { label: 'Thanh toán một phần / đã đặt cọc', color: 'info' },
  Paid: { label: 'Đã thanh toán', color: 'success' },
  Failed: { label: 'Thanh toán thất bại', color: 'danger' },
  Refunded: { label: 'Đã hoàn tiền', color: 'dark' },
  Cancelled: { label: 'Đã hủy thanh toán', color: 'secondary' },
};

export const PAYMENT_STATUS_OPTIONS = [
  { value: 'Unpaid', label: 'Chưa thanh toán' },
  { value: 'Pending', label: 'Chờ xác nhận thanh toán' },
  { value: 'PartiallyPaid', label: 'Thanh toán một phần / đã đặt cọc' },
  { value: 'Paid', label: 'Đã thanh toán' },
  { value: 'Failed', label: 'Thanh toán thất bại' },
  { value: 'Refunded', label: 'Đã hoàn tiền' },
  { value: 'Cancelled', label: 'Đã hủy thanh toán' },
];

export const SHIPPING_STATUS = {
  Unallocated: { label: 'Chưa phân bổ', color: 'secondary' },
  Allocated: { label: 'Đã phân bổ', color: 'warning' },
  Shipped: { label: 'Đang giao', color: 'info' },
  Fulfilled: { label: 'Đã giao', color: 'success' },
};

export const SHIPPING_STATUS_OPTIONS = Object.entries(SHIPPING_STATUS)
  .map(([value, meta]) => ({ value, label: meta.label }));

export const DELIVERY_SHIPPING_STATUS_OPTIONS = SHIPPING_STATUS_OPTIONS;
export const PICKUP_SHIPPING_STATUS_OPTIONS = SHIPPING_STATUS_OPTIONS;

export const getPaymentStatusMeta = (status) => (
  PAYMENT_STATUS[status] || { label: status || 'Khác', color: 'secondary' }
);

export const getShippingStatusMeta = (status) => (
  SHIPPING_STATUS[status] || { label: status || 'Khác', color: 'secondary' }
);

export const PAYMENT_METHODS = {
  Cash: 'Tiền mặt',
  COD: 'Tiền mặt (COD)',
  BankTransfer: 'Chuyển khoản',
  Card: 'Thẻ',
  Momo: 'Momo',
  VNPay: 'VNPay',
};

export const PRODUCT_STATUS = {
  Available: { label: 'Đang bán', color: 'success' },
  Inactive: { label: 'Ngừng bán', color: 'secondary' },
  OutOfStock: { label: 'Hết hàng', color: 'danger' },
  Discontinued: { label: 'Ngừng kinh doanh', color: 'dark' },
};

export const USER_STATUS = {
  Active: { label: 'Hoạt động', color: 'success' },
  Inactive: { label: 'Khóa', color: 'danger' },
};

export const ROLES = {
  Admin: 'Quản trị viên',
  Staff: 'Nhân viên',
  Customer: 'Khách hàng',
};
