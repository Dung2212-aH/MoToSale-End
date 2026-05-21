// Trạng thái đơn hàng
export const ORDER_STATUS = {
  Pending: { label: 'Chờ xử lý', color: 'warning' },
  Checkout: { label: 'Đang checkout', color: 'info' },
  AwaitingPayment: { label: 'Chờ thanh toán', color: 'primary' },
  Confirmed: { label: 'Đã xác nhận', color: 'success' },
  Shipping: { label: 'Đang giao', color: 'info' },
  Delivered: { label: 'Đã giao', color: 'success' },
  Cancelled: { label: 'Đã hủy', color: 'danger' },
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
