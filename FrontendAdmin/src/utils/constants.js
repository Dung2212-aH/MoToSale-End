// Trạng thái đơn hàng (UI gộp 4 trạng thái nghiệp vụ)
export const ORDER_STATUS_LABELS = {
  Pending: { label: 'Chờ xác nhận', color: 'warning' },
  Checkout: { label: 'Chờ xác nhận', color: 'warning' },
  AwaitingPayment: { label: 'Chờ xác nhận', color: 'warning' },
  Confirmed: { label: 'Chờ xác nhận', color: 'warning' },
  Shipping: { label: 'Đang giao', color: 'info' },
  Delivered: { label: 'Đã giao', color: 'success' },
  Cancelled: { label: 'Đã hủy', color: 'danger' },
  Completed: { label: 'Đã giao', color: 'success' },
  Processing: { label: 'Chờ xác nhận', color: 'warning' },
};

export const ORDER_STATUS_OPTIONS = [
  { value: 'Pending', label: 'Chờ xác nhận' },
  { value: 'Shipping', label: 'Đang giao' },
  { value: 'Delivered', label: 'Đã giao' },
  { value: 'Cancelled', label: 'Đã hủy' },
];

export const normalizeOrderStatus = (status) => {
  const value = String(status || '');
  if (['Pending', 'Checkout', 'AwaitingPayment', 'Confirmed', 'Processing'].includes(value)) return 'Pending';
  if (['Shipping'].includes(value)) return 'Shipping';
  if (['Delivered', 'Completed'].includes(value)) return 'Delivered';
  if (['Cancelled', 'Canceled'].includes(value)) return 'Cancelled';
  return value;
};

export const getOrderStatusMeta = (status) => {
  const normalized = normalizeOrderStatus(status);
  return ORDER_STATUS_LABELS[normalized] || { label: status || 'Khác', color: 'secondary' };
};

// Trạng thái thanh toán
export const PAYMENT_STATUS = {
  Unpaid: { label: 'Chưa thanh toán', color: 'secondary' },
  Pending: { label: 'Chờ thanh toán', color: 'warning' },
  Paid: { label: 'Đã thanh toán', color: 'success' },
  PartiallyPaid: { label: 'Thanh toán một phần', color: 'info' },
  Failed: { label: 'Thất bại', color: 'danger' },
  Cancelled: { label: 'Đã hủy', color: 'secondary' },
};

// Phương thức thanh toán
export const PAYMENT_METHODS = {
  COD: 'Tiền mặt (COD)',
  BankTransfer: 'Chuyển khoản',
  Card: 'Thẻ',
  Momo: 'Momo',
  VNPay: 'VNPay',
};

// Trạng thái sản phẩm
export const PRODUCT_STATUS = {
  Available: { label: 'Đang bán', color: 'success' },
  Inactive: { label: 'Ngừng bán', color: 'secondary' },
  OutOfStock: { label: 'Hết hàng', color: 'danger' },
  Discontinued: { label: 'Ngừng kinh doanh', color: 'dark' },
};

// Trạng thái user
export const USER_STATUS = {
  Active: { label: 'Hoạt động', color: 'success' },
  Inactive: { label: 'Khóa', color: 'danger' },
};

// Vai trò
export const ROLES = {
  Admin: 'Quản trị viên',
  Staff: 'Nhân viên',
  Customer: 'Khách hàng',
};
