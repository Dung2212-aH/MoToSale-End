export const ORDER_STATUS_MAP = {
  AwaitingPayment: 'Chờ thanh toán',
  Confirmed: 'Đã xác nhận',
  Completed: 'Hoàn thành',
  Cancelled: 'Đã hủy',
  Pending: 'Chờ xử lý',
  Processing: 'Đang xử lý',
};

export const SHIPPING_STATUS_MAP = {
  NotShipped: 'Chưa giao hàng',
  Preparing: 'Đang chuẩn bị hàng',
  Shipping: 'Đang giao hàng',
  Delivered: 'Đã giao thành công',
  PickupReady: 'Sẵn sàng nhận tại showroom',
  PickedUp: 'Đã nhận tại showroom',
  Cancelled: 'Đã hủy',
  AwaitingPickup: 'Chờ lấy hàng',
  InTransit: 'Đang giao hàng',
};

export const PAYMENT_STATUS_MAP = {
  Unpaid: 'Chưa thanh toán',
  Pending: 'Đang chờ xử lý',
  Paid: 'Đã thanh toán',
  DepositPaid: 'Đã thanh toán tiền cọc',
  PartiallyPaid: 'Thanh toán một phần',
  Refunded: 'Đã hoàn tiền',
  PartiallyRefunded: 'Hoàn tiền một phần',
  Failed: 'Thất bại',
};

export const PAYMENT_METHOD_MAP = {
  BankTransfer: 'Chuyển khoản ngân hàng',
  Card: 'Thẻ tín dụng/ghi nợ',
  Momo: 'Ví MoMo',
  VNPay: 'VNPay',
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
  Completed: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
  Pending: 'bg-amber-100 text-amber-700',
  Processing: 'bg-blue-100 text-blue-700',
  Failed: 'bg-red-100 text-red-700',
};

const SHIPPING_STATUS_COLOR_MAP = {
  NotShipped: 'bg-zinc-100 text-zinc-600',
  Preparing: 'bg-amber-100 text-amber-700',
  Shipping: 'bg-blue-100 text-blue-700',
  Delivered: 'bg-green-100 text-green-700',
  PickupReady: 'bg-sky-100 text-sky-700',
  PickedUp: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
  AwaitingPickup: 'bg-zinc-100 text-zinc-600',
  InTransit: 'bg-blue-100 text-blue-700',
};

const PAYMENT_STATUS_COLOR_MAP = {
  Unpaid: 'bg-red-100 text-red-700',
  Pending: 'bg-red-100 text-red-700',
  Paid: 'bg-green-100 text-green-700',
  DepositPaid: 'bg-orange-100 text-orange-700',
  PartiallyPaid: 'bg-orange-100 text-orange-700',
  Refunded: 'bg-purple-100 text-purple-700',
  PartiallyRefunded: 'bg-purple-100 text-purple-700',
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
