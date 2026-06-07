// Đơn hàng chỉ còn 3 trạng thái sau khi đơn giản hóa. Tiến trình vận chuyển dùng SHIPPING_STATUS riêng.
export const ORDER_STATUS_LABELS = {
  AwaitingPayment: { label: 'Chờ thanh toán', color: 'warning' },
  Confirmed: { label: 'Đã xác nhận', color: 'primary' },
  Cancelled: { label: 'Đã hủy', color: 'danger' },
  // Legacy giá trị cũ — vẫn map để khỏi vỡ UI khi còn data
  Pending: { label: 'Chờ thanh toán', color: 'warning' },
  Checkout: { label: 'Chờ thanh toán', color: 'warning' },
  Processing: { label: 'Đã xác nhận', color: 'primary' },
  Shipping: { label: 'Đã xác nhận', color: 'primary' },
  Delivered: { label: 'Đã xác nhận', color: 'primary' },
  Completed: { label: 'Đã xác nhận', color: 'primary' },
};

// Simplified: order is either AwaitingPayment, Confirmed, or Cancelled.
// Preparation → shipping → delivery is tracked separately in TrangThaiVanChuyen.
export const ORDER_STATUS_OPTIONS = [
  { value: 'AwaitingPayment', label: 'Chờ thanh toán / xác nhận' },
  { value: 'Confirmed', label: 'Đã xác nhận' },
  { value: 'Cancelled', label: 'Đã hủy' },
];

export const ORDER_NEXT_STATUS = {
  Pending: ['AwaitingPayment', 'Confirmed', 'Cancelled'],
  Checkout: ['AwaitingPayment', 'Confirmed', 'Cancelled'],
  AwaitingPayment: ['Confirmed', 'Cancelled'],
  Confirmed: ['Cancelled'],
  Cancelled: [],
};

export const normalizeOrderStatus = (status) => String(status || '');

export const getOrderStatusMeta = (status) => (
  ORDER_STATUS_LABELS[normalizeOrderStatus(status)] || { label: status || 'Khác', color: 'secondary' }
);

export const PAYMENT_STATUS = {
  Unpaid: { label: 'Chưa thanh toán', color: 'secondary' },
  PartiallyPaid: { label: 'Đã thanh toán một phần', color: 'info' },
  Paid: { label: 'Đã thanh toán đủ', color: 'success' },
  Refunded: { label: 'Đã hoàn tiền', color: 'dark' },
  Cancelled: { label: 'Đã hủy thanh toán', color: 'secondary' },
  // Legacy values still shown if data is old
  Pending: { label: 'Chờ xác nhận thanh toán', color: 'warning' },
  Failed: { label: 'Thanh toán thất bại', color: 'danger' },
};

export const ORDER_TYPE_LABELS = {
  FullPayment: 'Thanh toán toàn bộ',
  Deposit: 'Đặt cọc trước',
  Installment: 'Trả góp',
};

/**
 * Cùng 'PartiallyPaid' nghĩa khác nhau theo orderType:
 *   Deposit     → "Đã đặt cọc"
 *   Installment → "Đang trả góp"
 *   FullPayment → "Đã thanh toán một phần"
 */
export const getPaymentStatusContextual = (paymentStatus, orderType) => {
  if (paymentStatus === 'PartiallyPaid') {
    if (orderType === 'Deposit') return { label: 'Đã đặt cọc', color: 'info' };
    if (orderType === 'Installment') return { label: 'Đang trả góp', color: 'info' };
    return { label: 'Đã thanh toán một phần', color: 'info' };
  }
  if (paymentStatus === 'Paid' && orderType === 'Installment') {
    return { label: 'Đã trả góp xong', color: 'success' };
  }
  return PAYMENT_STATUS[paymentStatus] || { label: paymentStatus || 'Khác', color: 'secondary' };
};

// Aligned with backend CK_DONHANG_PaymentStatus (Unpaid/PartiallyPaid/Paid/Refunded/Cancelled).
// 'Pending' and 'Failed' belong to per-transaction status, not order-level — keep their labels in
// PAYMENT_STATUS map for displaying legacy/transaction values, but don't expose as admin options.
export const PAYMENT_STATUS_OPTIONS = [
  { value: 'Unpaid', label: 'Chưa thanh toán' },
  { value: 'PartiallyPaid', label: 'Thanh toán một phần / đã đặt cọc' },
  { value: 'Paid', label: 'Đã thanh toán' },
  { value: 'Refunded', label: 'Đã hoàn tiền' },
  { value: 'Cancelled', label: 'Đã hủy thanh toán' },
];

export const SHIPPING_STATUS = {
  Preparing: { label: 'Đang chuẩn bị hàng', color: 'warning' },
  Shipping: { label: 'Đang giao', color: 'info' },
  Delivered: { label: 'Đã giao', color: 'success' },
};

export const SHIPPING_STATUS_OPTIONS = [
  { value: 'Preparing', label: 'Đang chuẩn bị hàng' },
  { value: 'Shipping', label: 'Đang giao' },
  { value: 'Delivered', label: 'Đã giao' },
];

export const DELIVERY_SHIPPING_STATUS_OPTIONS = SHIPPING_STATUS_OPTIONS;

export const PICKUP_SHIPPING_STATUS_OPTIONS = SHIPPING_STATUS_OPTIONS;

export const getPaymentStatusMeta = (status) => (
  PAYMENT_STATUS[status] || { label: status || 'Khác', color: 'secondary' }
);

export const getShippingStatusMeta = (status) => (
  SHIPPING_STATUS[status] || { label: status || 'Khác', color: 'secondary' }
);

export const PAYMENT_METHODS = {
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
