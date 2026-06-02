export const ORDER_STATUS_MAP = {
  AwaitingPayment: 'Chờ thanh toán',
  Confirmed: 'Đã xác nhận',
  Processing: 'Đang chuẩn bị hàng',
  Shipping: 'Đang giao',
  Delivered: 'Đã giao',
  Completed: 'Hoàn tất',
  Cancelled: 'Đã hủy',
  Pending: 'Legacy: chờ xử lý',
  Checkout: 'Đang checkout',
};

export const SHIPPING_STATUS_MAP = {
  Preparing: 'Đang chuẩn bị hàng',
  Shipping: 'Đang giao',
  Delivered: 'Đã giao',
};

export const PAYMENT_STATUS_MAP = {
  Unpaid: 'Chưa thanh toán',
  Pending: 'Chờ xác nhận thanh toán',
  Paid: 'Đã thanh toán',
  DepositPaid: 'Đã thanh toán tiền cọc',
  PartiallyPaid: 'Thanh toán một phần / đã đặt cọc',
  Refunded: 'Đã hoàn tiền',
  PartiallyRefunded: 'Hoàn tiền một phần',
  Failed: 'Thanh toán thất bại',
  Cancelled: 'Đã hủy thanh toán',
};

export const PAYMENT_METHOD_MAP = {
  BankTransfer: 'Chuyển khoản ngân hàng',
  Card: 'Thẻ tín dụng/ghi nợ',
  Momo: 'Ví MoMo',
  VNPay: 'VNPay',
  COD: 'Tiền mặt (COD)',
};

export const ORDER_TYPE_MAP = {
  FullPayment: 'Thanh toán toàn bộ',
  Deposit: 'Đặt cọc',
  Installment: 'Trả góp',
};

export const RECEIVING_METHOD_MAP = {
  Delivery: 'Giao hàng tận nơi',
  Pickup: 'Nhận tại showroom',
};

const ORDER_STATUS_COLOR_MAP = {
  AwaitingPayment: 'bg-amber-100 text-amber-700',
  Confirmed: 'bg-blue-100 text-blue-700',
  Processing: 'bg-sky-100 text-sky-700',
  Shipping: 'bg-blue-100 text-blue-700',
  Delivered: 'bg-green-100 text-green-700',
  Completed: 'bg-emerald-100 text-emerald-700',
  Cancelled: 'bg-red-100 text-red-700',
  Pending: 'bg-zinc-100 text-zinc-700',
  Checkout: 'bg-zinc-100 text-zinc-700',
};

const SHIPPING_STATUS_COLOR_MAP = {
  Preparing: 'bg-amber-100 text-amber-700',
  Shipping: 'bg-blue-100 text-blue-700',
  Delivered: 'bg-green-100 text-green-700',
};

const PAYMENT_STATUS_COLOR_MAP = {
  Unpaid: 'bg-zinc-100 text-zinc-700',
  Pending: 'bg-amber-100 text-amber-700',
  Paid: 'bg-green-100 text-green-700',
  DepositPaid: 'bg-orange-100 text-orange-700',
  PartiallyPaid: 'bg-orange-100 text-orange-700',
  Refunded: 'bg-purple-100 text-purple-700',
  PartiallyRefunded: 'bg-purple-100 text-purple-700',
  Failed: 'bg-red-100 text-red-700',
  Cancelled: 'bg-zinc-100 text-zinc-700',
};

export function getOrderStatusLabel(status) {
  return ORDER_STATUS_MAP[status] || status || 'Không xác định';
}

export function getShippingStatusLabel(status) {
  return SHIPPING_STATUS_MAP[status] || status || 'Không xác định';
}

export function getPaymentStatusLabel(status) {
  return PAYMENT_STATUS_MAP[status] || status || 'Không xác định';
}

export function getPaymentMethodLabel(method) {
  return PAYMENT_METHOD_MAP[method] || method || 'Không xác định';
}

export function getOrderTypeLabel(type) {
  return ORDER_TYPE_MAP[type] || type || 'Không xác định';
}

export function getReceivingMethodLabel(method) {
  return RECEIVING_METHOD_MAP[method] || method || 'Không xác định';
}

export function getOrderStatusColor(status) {
  return ORDER_STATUS_COLOR_MAP[status] || 'bg-zinc-100 text-zinc-700';
}

export function getShippingStatusColor(status) {
  return SHIPPING_STATUS_COLOR_MAP[status] || 'bg-zinc-100 text-zinc-700';
}

export function getPaymentStatusColor(status) {
  return PAYMENT_STATUS_COLOR_MAP[status] || 'bg-zinc-100 text-zinc-700';
}
