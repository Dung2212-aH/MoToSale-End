export const ORDER_STATUS_MAP = {
  Pending: 'Chờ xử lý',
  AwaitingPayment: 'Chờ thanh toán',
  Confirmed: 'Đã xác nhận',
  Processing: 'Đang xử lý',
  Shipping: 'Đang giao hàng',
  Completed: 'Hoàn thành',
  Cancelled: 'Đã hủy',
};

export const SHIPPING_STATUS_MAP = {
  NotShipped: 'Chưa giao',
  AwaitingPickup: 'Chờ lấy hàng',
  Preparing: 'Đang chuẩn bị hàng',
  InTransit: 'Đang giao hàng',
  Delivered: 'Đã giao thành công',
  Returned: 'Đã trả hàng',
  Failed: 'Giao thất bại',
};

export const PAYMENT_STATUS_MAP = {
  Unpaid: 'Chưa thanh toán',
  Pending: 'Đang chờ xử lý',
  Paid: 'Đã thanh toán',
  PartiallyPaid: 'Thanh toán một phần',
  Refunded: 'Đã hoàn tiền',
  PartiallyRefunded: 'Hoàn tiền một phần',
  Failed: 'Thất bại',
};

export const PAYMENT_METHOD_MAP = {
  COD: 'Thanh toán khi nhận hàng',
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
  switch (status) {
    case 'Pending':
    case 'AwaitingPayment':
      return 'bg-amber-100 text-amber-700';
    case 'Confirmed':
    case 'Processing':
      return 'bg-blue-100 text-blue-700';
    case 'Completed':
    case 'Delivered':
      return 'bg-green-100 text-green-700';
    case 'Cancelled':
    case 'Failed':
      return 'bg-red-100 text-red-700';
    default:
      return 'bg-zinc-100 text-zinc-700';
  }
}

export function getShippingStatusColor(status) {
  switch (status) {
    case 'NotShipped':
    case 'AwaitingPickup':
      return 'bg-zinc-100 text-zinc-600';
    case 'Preparing':
      return 'bg-amber-100 text-amber-700';
    case 'InTransit':
      return 'bg-blue-100 text-blue-700';
    case 'Delivered':
      return 'bg-green-100 text-green-700';
    case 'Returned':
    case 'Failed':
      return 'bg-red-100 text-red-700';
    default:
      return 'bg-zinc-100 text-zinc-700';
  }
}

export function getPaymentStatusColor(status) {
  switch (status) {
    case 'Unpaid':
    case 'Pending':
      return 'bg-red-100 text-red-700';
    case 'Paid':
      return 'bg-green-100 text-green-700';
    case 'PartiallyPaid':
      return 'bg-orange-100 text-orange-700';
    case 'Refunded':
    case 'PartiallyRefunded':
      return 'bg-purple-100 text-purple-700';
    default:
      return 'bg-zinc-100 text-zinc-700';
  }
}
