// Lớp service gọi backend của storefront (chỉ 2 lớp: page -> api -> http).
// - Gọi axios qua `api` (httpClient.js lo baseURL/token/interceptor).
// - Map dữ liệu ngay tại chỗ bằng các helper bên dưới, rồi trả shape ổn định cho UI.
// Tên hàm rút gọn theo nhóm: get/getById/create/update/remove + tên nghiệp vụ rõ.
import api, {
  responseData,
  getToken,
  decodeJwtPayload,
  getClaim,
  isTokenExpired,
  clearAuthStorage,
  getStoredUser,
  normalizeLoginResponse,
  saveAuthUser,
  mergeStoredUser,
  AUTH_CHANGED_EVENT,
} from './httpClient.js';
import {
  normalizeCart,
  normalizeCategory,
  normalizeFilters,
  normalizeProduct,
  normalizeProductList,
} from '../utils/productMappers.js';
import { notifyCartChanged } from '../utils/cartEvents.js';
import { normalizeImageUrl } from '../utils/formatters.js';

// ===== Helper map dữ liệu (backend trả nhiều kiểu key -> gom về 1 shape) =====

// Lấy giá trị đầu tiên không null/undefined theo danh sách key ưu tiên.
const field = (source, ...keys) => {
  for (const key of keys) {
    if (source?.[key] !== undefined && source?.[key] !== null) {
      return source[key];
    }
  }
  return undefined;
};

const mapOrder = (raw = {}) => {
  const items = field(raw, 'items', 'Items') || [];
  const vouchers = field(raw, 'vouchers', 'Vouchers') || [];

  return {
    ...raw,
    id: field(raw, 'id', 'Id', 'maDonHang', 'MaDonHang'),
    orderCode: field(raw, 'orderCode', 'OrderCode', 'maDonHangKinhDoanh', 'MaDonHangKinhDoanh'),
    userId: field(raw, 'userId', 'UserId', 'maNguoiDung', 'MaNguoiDung'),
    cartId: field(raw, 'cartId', 'CartId', 'maGioHang', 'MaGioHang'),
    shippingFullName: field(raw, 'shippingFullName', 'ShippingFullName', 'hoTenNhanHang', 'HoTenNhanHang'),
    shippingPhoneNumber: field(raw, 'shippingPhoneNumber', 'ShippingPhoneNumber', 'soDienThoaiNhanHang', 'SoDienThoaiNhanHang'),
    shippingEmail: field(raw, 'shippingEmail', 'ShippingEmail', 'emailNhanHang', 'EmailNhanHang'),
    shippingAddressLine: field(raw, 'shippingAddressLine', 'ShippingAddressLine', 'diaChiNhanHang', 'DiaChiNhanHang'),
    subtotal: Number(field(raw, 'subtotal', 'Subtotal', 'tongTienHang', 'TongTienHang') || 0),
    discountAmount: Number(field(raw, 'discountAmount', 'DiscountAmount', 'tienGiam', 'TienGiam') || 0),
    shippingFee: Number(field(raw, 'shippingFee', 'ShippingFee', 'phiVanChuyen', 'PhiVanChuyen') || 0),
    totalAmount: Number(field(raw, 'totalAmount', 'TotalAmount', 'tongThanhToan', 'TongThanhToan') || 0),
    orderStatus: field(raw, 'orderStatus', 'OrderStatus', 'trangThaiDonHang', 'TrangThaiDonHang'),
    paymentStatus: field(raw, 'paymentStatus', 'PaymentStatus', 'trangThaiThanhToan', 'TrangThaiThanhToan'),
    shippingStatus: field(raw, 'shippingStatus', 'ShippingStatus', 'trangThaiVanChuyen', 'TrangThaiVanChuyen'),
    receivingMethod: field(raw, 'receivingMethod', 'ReceivingMethod', 'phuongThucNhanHang', 'PhuongThucNhanHang'),
    paymentMethod: field(raw, 'paymentMethod', 'PaymentMethod', 'phuongThucThanhToan', 'PhuongThucThanhToan', 'phuongThuc', 'PhuongThuc'),
    orderType: field(raw, 'orderType', 'OrderType', 'loaiDonHang', 'LoaiDonHang'),
    depositAmount: Number(field(raw, 'depositAmount', 'DepositAmount', 'tienDatCoc', 'TienDatCoc') || 0),
    remainingAmount: Number(field(raw, 'remainingAmount', 'RemainingAmount', 'soTienConLai', 'SoTienConLai') || 0),
    note: field(raw, 'note', 'Note', 'ghiChu', 'GhiChu'),
    createdAt: field(raw, 'createdAt', 'CreatedAt', 'ngayTao', 'NgayTao'),
    updatedAt: field(raw, 'updatedAt', 'UpdatedAt', 'ngayCapNhat', 'NgayCapNhat'),
    items: items.map((item) => ({
      ...item,
      id: field(item, 'id', 'Id', 'maChiTietDonHang', 'MaChiTietDonHang'),
      productId: field(item, 'productId', 'ProductId', 'maSanPham', 'MaSanPham'),
      productVariantId: field(item, 'productVariantId', 'ProductVariantId', 'maBienSanPham', 'MaBienSanPham'),
      productNameSnapshot: field(item, 'productNameSnapshot', 'ProductNameSnapshot', 'tenSanPhamSnapshot', 'TenSanPhamSnapshot'),
      skuSnapshot: field(item, 'skuSnapshot', 'SkuSnapshot', 'skuSnapshot', 'SKUSnapshot'),
      unitPrice: Number(field(item, 'unitPrice', 'UnitPrice', 'donGia', 'DonGia') || 0),
      quantity: Number(field(item, 'quantity', 'Quantity', 'soLuong', 'SoLuong') || 0),
      lineTotal: Number(field(item, 'lineTotal', 'LineTotal', 'thanhTien', 'ThanhTien') || 0),
    })),
    vouchers: vouchers.map((voucher) => ({
      ...voucher,
      voucherCodeSnapshot: field(voucher, 'voucherCodeSnapshot', 'VoucherCodeSnapshot', 'maVoucherCodeSnapshot', 'MaVoucherCodeSnapshot'),
      discountAmount: Number(field(voucher, 'discountAmount', 'DiscountAmount', 'soTienGiam', 'SoTienGiam') || 0),
      discountTypeSnapshot: field(voucher, 'discountTypeSnapshot', 'DiscountTypeSnapshot', 'loaiGiamGiaSnapshot', 'LoaiGiamGiaSnapshot'),
      discountValueSnapshot: Number(field(voucher, 'discountValueSnapshot', 'DiscountValueSnapshot', 'giaTriGiamSnapshot', 'GiaTriGiamSnapshot') || 0),
    })),
  };
};

const mapPayment = (raw = {}) => ({
  ...raw,
  id: field(raw, 'id', 'Id', 'maThanhToan', 'MaThanhToan'),
  paymentCode: field(raw, 'paymentCode', 'PaymentCode', 'maThanhToanKinhDoanh', 'MaThanhToanKinhDoanh'),
  orderId: field(raw, 'orderId', 'OrderId', 'maDonHang', 'MaDonHang'),
  orderCode: field(raw, 'orderCode', 'OrderCode', 'maDonHangKinhDoanh', 'MaDonHangKinhDoanh'),
  amount: Number(field(raw, 'amount', 'Amount', 'soTien', 'SoTien') || 0),
  paymentMethod: field(raw, 'paymentMethod', 'PaymentMethod', 'phuongThuc', 'PhuongThuc'),
  paymentStatus: field(raw, 'paymentStatus', 'PaymentStatus', 'trangThai', 'TrangThai'),
  transactionRef: field(raw, 'transactionRef', 'TransactionRef', 'maGiaoDich', 'MaGiaoDich'),
  paidAt: field(raw, 'paidAt', 'PaidAt', 'daThanhToanLuc', 'DaThanhToanLuc'),
  createdAt: field(raw, 'createdAt', 'CreatedAt', 'ngayTao', 'NgayTao'),
});

const mapVoucher = (raw = {}) => ({
  ...raw,
  id: field(raw, 'id', 'Id', 'maVoucher', 'MaVoucher'),
  code: field(raw, 'code', 'Code', 'maVoucherCode', 'MaVoucherCode'),
  description: field(raw, 'description', 'Description', 'moTa', 'MoTa'),
  discountType: field(raw, 'discountType', 'DiscountType', 'loaiGiamGia', 'LoaiGiamGia'),
  discountValue: Number(field(raw, 'discountValue', 'DiscountValue', 'giaTriGiam', 'GiaTriGiam') || 0),
  maxDiscountValue: field(raw, 'maxDiscountValue', 'MaxDiscountValue', 'giaTriGiamToiDa', 'GiaTriGiamToiDa'),
  minOrderValue: Number(field(raw, 'minOrderValue', 'MinOrderValue', 'giaTriDonToiThieu', 'GiaTriDonToiThieu') || 0),
  remainingUses: field(raw, 'remainingUses', 'RemainingUses') ?? null,
});

const mapFavorite = (raw = {}) => {
  const product = normalizeProduct(field(raw, 'product', 'Product') || raw);
  return {
    ...raw,
    userId: field(raw, 'userId', 'UserId', 'maNguoiDung', 'MaNguoiDung'),
    productId: field(raw, 'productId', 'ProductId', 'maSanPham', 'MaSanPham') || product?.id,
    createdAt: field(raw, 'createdAt', 'CreatedAt', 'ngayTao', 'NgayTao'),
    product,
  };
};

const mapReview = (raw = {}) => ({
  ...raw,
  id: field(raw, 'id', 'Id', 'maDanhGia', 'MaDanhGia'),
  productId: field(raw, 'productId', 'ProductId', 'maSanPham', 'MaSanPham'),
  userId: field(raw, 'userId', 'UserId', 'maNguoiDung', 'MaNguoiDung'),
  userName: field(raw, 'userName', 'UserName', 'tenNguoiDung', 'TenNguoiDung'),
  orderId: field(raw, 'orderId', 'OrderId', 'maDonHang', 'MaDonHang'),
  rating: Number(field(raw, 'rating', 'Rating', 'diem', 'Diem') || 0),
  title: field(raw, 'title', 'Title', 'tieuDe', 'TieuDe'),
  comment: field(raw, 'comment', 'Comment', 'noiDung', 'NoiDung'),
  imageUrl: normalizeImageUrl(field(raw, 'imageUrl', 'ImageUrl', 'hinhAnhUrl', 'HinhAnhUrl')),
  status: field(raw, 'status', 'Status', 'trangThai', 'TrangThai'),
  createdAt: field(raw, 'createdAt', 'CreatedAt', 'ngayTao', 'NgayTao'),
  updatedAt: field(raw, 'updatedAt', 'UpdatedAt', 'ngayCapNhat', 'NgayCapNhat'),
});

// Đóng gói payload đánh giá thành multipart/form-data theo đúng tên field backend.
const buildReviewForm = (payload = {}) => {
  const form = new FormData();
  form.append('Diem', payload.rating ?? payload.diem);
  form.append('NoiDung', payload.comment ?? payload.noiDung ?? '');
  if (payload.productId ?? payload.maSanPham) form.append('MaSanPham', payload.productId ?? payload.maSanPham);
  if (payload.orderId ?? payload.maDonHang) form.append('MaDonHang', payload.orderId ?? payload.maDonHang);
  if (payload.title ?? payload.tieuDe) form.append('TieuDe', payload.title ?? payload.tieuDe);
  if (payload.image) form.append('Image', payload.image);
  return form;
};

// Chuyển param UI (sortBy, categoryId...) sang tên query backend (MaDanhMuc, SortBy...).
const toQuery = (params = {}) => {
  const sortMap = {
    'price-asc': { SortBy: 'price', SortDescending: false },
    'price-desc': { SortBy: 'price', SortDescending: true },
    'name-asc': { SortBy: 'name', SortDescending: false },
    'name-desc': { SortBy: 'name', SortDescending: true },
    'year-asc': { SortBy: 'created', SortDescending: false },
    'year-desc': { SortBy: 'created', SortDescending: true },
    price_asc: { SortBy: 'price', SortDescending: false },
    price_desc: { SortBy: 'price', SortDescending: true },
    name_asc: { SortBy: 'name', SortDescending: false },
    name_desc: { SortBy: 'name', SortDescending: true },
    year_asc: { SortBy: 'created', SortDescending: false },
    year_desc: { SortBy: 'created', SortDescending: true },
  };

  const paramMap = {
    categoryId: 'MaDanhMuc',
    brandId: 'MaHangXe',
    carModelId: 'MaDongXe',
    compatibleCarModelId: 'MaDongXeTuongThich',
    productType: 'LoaiSanPham',
    status: 'TrangThaiSanPham',
    minPrice: 'GiaTu',
    maxPrice: 'GiaDen',
  };

  const source = { ...params };
  if (sortMap[params.sortBy]) {
    delete source.sortBy;
    Object.assign(source, sortMap[params.sortBy]);
  }

  const mapped = Object.entries(source).reduce((acc, [key, value]) => {
    acc[paramMap[key] || key] = value;
    return acc;
  }, {});

  return Object.fromEntries(
    Object.entries(mapped).filter(([, value]) => value !== '' && value !== undefined && value !== null),
  );
};

// Lấy mảng items từ response (backend có thể trả mảng trực tiếp hoặc bọc trong items/Items).
const listOf = (data) => (Array.isArray(data) ? data : data?.items || data?.Items || []);

// ===== Auth =====

export const authApi = {
  async login({ username, password, rememberMe }) {
    const { data } = await api.post('/auth/login', { email: username, matKhau: password });
    const user = normalizeLoginResponse(data);
    saveAuthUser(user, rememberMe === true || rememberMe === 'true' || rememberMe === 'on');
    return user;
  },

  register: (data) => api.post('/auth/register', {
    hoTen: data.name,
    email: data.email,
    soDienThoai: data.phone,
    matKhau: data.password,
  }),

  forgotPassword: (email) => api.post('/auth/forgot-password', { email }).then(responseData),

  resetPassword: (data) => api.post('/auth/reset-password', {
    email: data.email,
    token: data.token,
    matKhauMoi: data.password,
  }).then(responseData),

  logout() {
    clearAuthStorage();
  },

  getCurrentUser() {
    const token = getToken();
    if (!token || isTokenExpired(token)) {
      clearAuthStorage(false);
      return null;
    }

    const storedUser = getStoredUser();
    if (storedUser) {
      return storedUser;
    }

    const claims = decodeJwtPayload(token);
    return {
      token,
      userId: getClaim(claims, 'nameidentifier') || claims?.sub,
      username: getClaim(claims, 'name'),
      name: getClaim(claims, 'name'),
      email: getClaim(claims, 'email') || getClaim(claims, 'name'),
      role: getClaim(claims, 'role'),
      raw: claims,
    };
  },

  getToken: () => getToken(),

  updateStoredUser: (data) => mergeStoredUser(data),
};

// ===== Sản phẩm =====

export const productApi = {
  getAll: (params) => api.get('/products', { params: toQuery({ DangHoatDong: true, ...params }) }).then((res) => normalizeProductList(res.data)),
  getById: (id) => api.get(`/products/${id}`).then((res) => normalizeProduct(res.data)),
  getFilters: () => api.get('/products/filters').then((res) => normalizeFilters(res.data)),
};

// ===== Đánh giá =====

export const reviewApi = {
  getByProduct: (productId) => api.get(`/products/${productId}/reviews`).then((res) => listOf(res.data).map(mapReview)),

  async getSummary(productId) {
    const { data } = await api.get(`/products/${productId}/reviews/summary`);
    return {
      productId: field(data, 'productId', 'ProductId', 'maSanPham', 'MaSanPham'),
      totalReviews: Number(field(data, 'totalReviews', 'TotalReviews', 'tongDanhGia', 'TongDanhGia') || 0),
      averageRating: Number(field(data, 'averageRating', 'AverageRating', 'diemTrungBinh', 'DiemTrungBinh') || 0),
    };
  },

  async getMine(productId) {
    const { data } = await api.get(`/reviews/product/${productId}/me`);
    const myReview = field(data, 'myReview', 'MyReview', 'danhGiaCuaToi', 'DanhGiaCuaToi');
    return {
      productId: field(data, 'productId', 'ProductId', 'maSanPham', 'MaSanPham'),
      isAuthenticated: field(data, 'isAuthenticated', 'IsAuthenticated', 'daDangNhap', 'DaDangNhap') === true,
      hasPurchased: field(data, 'hasPurchased', 'HasPurchased', 'daMua', 'DaMua') === true,
      canReview: field(data, 'canReview', 'CanReview', 'coTheDanhGia', 'CoTheDanhGia') === true,
      eligibleOrderId: field(data, 'eligibleOrderId', 'EligibleOrderId', 'maDonHangDuDieuKien', 'MaDonHangDuDieuKien'),
      reason: field(data, 'reason', 'Reason', 'lyDo', 'LyDo'),
      myReview: myReview ? mapReview(myReview) : null,
    };
  },

  async create(productId, payload) {
    const { data } = await api.post(`/products/${productId}/reviews`, buildReviewForm({ ...payload, productId }), {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return { ...data, review: data?.review || data?.Review ? mapReview(data.review || data.Review) : null };
  },

  async updateMine(productId, payload) {
    const { data } = await api.patch(`/products/${productId}/reviews/me`, buildReviewForm(payload), {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return { ...data, review: data?.review || data?.Review ? mapReview(data.review || data.Review) : null };
  },
};

// ===== Danh mục =====

export const categoryApi = {
  async getAll() {
    const response = await api.get('/categories');
    return { ...response, data: listOf(response.data).map(normalizeCategory) };
  },
};

// ===== Giỏ hàng =====

const handleCart = (response) => {
  const cart = normalizeCart(response.data);
  notifyCartChanged(cart);
  return cart;
};

export const cartApi = {
  getMine: () => api.get('/cart').then(handleCart),

  getCount: () => api.get('/cart/count').then((res) => {
    const data = res.data;
    return Number(data?.count ?? data?.totalItems ?? data ?? 0);
  }),

  addItem: (data) => api.post('/cart/items', {
    maSanPham: data.productId,
    maBienSanPham: data.variantId ?? data.productVariantId ?? null,
    soLuong: data.quantity,
  }).then(handleCart),

  async updateItem(id, quantityOrData) {
    const data = typeof quantityOrData === 'object' ? quantityOrData : { quantity: quantityOrData };
    await api.put(`/cart/items/${id}`, { soLuong: data.quantity ?? data.soLuong });
    return cartApi.getMine();
  },

  async removeItem(id) {
    await api.delete(`/cart/items/${id}`);
    return cartApi.getMine();
  },

  clear: () => api.delete('/cart/clear').then(handleCart),
};

// ===== Đơn hàng =====

export const orderApi = {
  async getAll(params) {
    const { data } = await api.get('/orders', { params });
    if (Array.isArray(data)) return data.map(mapOrder);
    const items = data?.items || data?.Items;
    return items ? items.map(mapOrder) : mapOrder(data);
  },

  getMine: () => orderApi.getAll(),

  async getById(id) {
    const { data } = await api.get(`/orders/${id}`);
    const order = mapOrder(data);
    return { ...order, order, details: order.items, vouchers: order.vouchers };
  },

  create: (data) => api.post('/orders', {
    maDiaChiNhanHang: data.shippingAddressId ?? data.maDiaChiNhanHang ?? null,
    hoTenNhanHang: data.shippingFullName,
    soDienThoaiNhanHang: data.shippingPhoneNumber,
    emailNhanHang: data.shippingEmail,
    diaChiNhanHang: [data.shippingAddressLine, data.shippingWard, data.shippingDistrict, data.shippingProvince].filter(Boolean).join(', '),
    shippingProvince: data.shippingProvince,
    maVoucherCode: data.voucherCode,
    ghiChu: data.note,
    phuongThucNhanHang: data.receivingMethod,
    loaiDonHang: data.orderType,
    phuongThucThanhToan: data.paymentMethod,
    soKyTraGop: data.soKyTraGop ?? null,
    hoSoTraGop: data.installmentApplication ?? null,
    tienDatCoc: data.depositAmount ?? 0,
    ngayHenNhanXe: data.pickupAppointmentAt,
    ghiChuGiaoNhan: data.fulfillmentNote,
    soPhutGiuCho: data.holdMinutes ?? 15,
  }).then((res) => mapOrder(res.data)),

  getPaymentInfo: (id) => api.get(`/orders/${id}/payment-info`).then(responseData),

  getShippingQuote: (data) => api.post('/orders/shipping-quote', {
    phuongThucNhanHang: data.receivingMethod,
    shippingProvince: data.shippingProvince,
    maVoucherCode: data.voucherCode,
    orderType: data.orderType,
  }).then(responseData),

  cancel: (id, reason) => api.put(`/orders/${id}/cancel`, { lyDoHuyDon: reason }).then((res) => mapOrder(res.data)),

  requestRefund: (id, data) => api.post(`/orders/${id}/request-refund`, {
    tenNganHang: data.bankName,
    soTaiKhoan: data.accountNo,
    chuTaiKhoan: data.accountName,
    lyDo: data.reason,
  }).then((res) => mapOrder(res.data)),
};

// ===== Thanh toán =====

export const paymentApi = {
  getByOrder: (orderId) => api.get(`/payments/order/${orderId}`).then((res) => {
    const data = res.data;
    const items = data?.items || data?.Items || data?.payments || data?.Payments || data;
    return Array.isArray(items) ? items.map(mapPayment) : items;
  }),

  create: (data) => api.post('/payments', {
    maDonHang: data.orderId ?? data.maDonHang,
    loaiThanhToan: data.paymentType ?? data.loaiThanhToan ?? 'Full',
    soTien: data.amount ?? data.soTien,
    phuongThuc: data.paymentMethod ?? data.phuongThuc ?? 'BankTransfer',
    maGiaoDich: data.transactionRef ?? data.maGiaoDich,
    noiDungChuyenKhoan: data.transferContent ?? data.noiDungChuyenKhoan,
    maNganHang: data.bankCode ?? data.maNganHang,
    responseRaw: data.responseRaw,
  }).then((res) => mapPayment(res.data)),

  confirmSuccess: (paymentId, data = {}) => api.post(`/payments/${paymentId}/confirm-success`, {
    maGiaoDich: data.transactionRef ?? data.maGiaoDich,
    responseRaw: data.responseRaw,
  }).then(responseData),
};

// ===== Voucher =====

export const voucherApi = {
  getAll: (params) => api.get('/vouchers', { params: toQuery(params) }).then((res) => {
    const items = res.data?.items || res.data?.Items || res.data;
    return Array.isArray(items) ? items.map(mapVoucher) : items;
  }),

  async validate(data) {
    const { data: result } = await api.post('/vouchers/validate', data);
    return {
      ...result,
      valid: field(result, 'valid', 'Valid', 'hopLe', 'HopLe') === true,
      message: field(result, 'message', 'Message', 'lyDoKhongHopLe', 'LyDoKhongHopLe'),
      discountAmount: Number(field(result, 'discountAmount', 'DiscountAmount', 'soTienGiam', 'SoTienGiam') || 0),
      voucher: mapVoucher(field(result, 'voucher', 'Voucher') || result),
    };
  },

  getApplicable: (data) => api.post('/vouchers/applicable', data).then((res) => {
    const items = res.data?.items || res.data?.Items || res.data;
    return Array.isArray(items) ? items.map(mapVoucher) : items;
  }),

  save: (code) => api.post('/vouchers/save', { code }).then(responseData),

  getMine: () => api.get('/vouchers/my').then((res) => {
    const items = res.data?.items || res.data?.Items || res.data;
    return Array.isArray(items) ? items.map(mapVoucher) : items;
  }),

  getMineCount: () => api.get('/vouchers/my/count').then((res) => res.data?.count ?? 0),
};

// ===== Người dùng & địa chỉ =====

const mapAddressBody = (data) => ({
  hoTenNhanHang: data.fullName,
  soDienThoaiNhanHang: data.phoneNumber,
  diaChiNhanHang: data.addressLine,
  ward: data.ward,
  district: data.district,
  province: data.province,
  ghiChu: data.note,
});

export const userApi = {
  getProfile: () => api.get('/users/me').then(responseData),

  updateProfile: (data) => api.put('/users/me', {
    hoTen: data.name,
    email: data.email,
    soDienThoai: data.phone,
  }).then(responseData),

  changePassword: (data) => api.put('/users/me/password', {
    matKhauHienTai: data.currentPassword,
    matKhauMoi: data.newPassword,
  }).then(responseData),

  getAddress: () => api.get('/users/me/address').then(responseData),

  async getAddresses() {
    try {
      const { data } = await api.get('/users/me/addresses');
      return data?.items || data?.Items || [];
    } catch (error) {
      if (error?.response?.status !== 404) throw error;
      const fallback = await userApi.getAddress();
      return fallback && Object.keys(fallback).length ? [fallback] : [];
    }
  },

  updateAddress: (data) => api.put('/users/me/address', { ...mapAddressBody(data), laMacDinh: true }).then(responseData),

  async createAddress(data) {
    try {
      const { data: result } = await api.post('/users/me/addresses', { ...mapAddressBody(data), laMacDinh: Boolean(data.isDefault) });
      return result;
    } catch (error) {
      if (error?.response?.status !== 404) throw error;
      return userApi.updateAddress(data);
    }
  },

  async updateAddressById(id, data) {
    try {
      const { data: result } = await api.put(`/users/me/addresses/${id}`, { ...mapAddressBody(data), laMacDinh: Boolean(data.isDefault) });
      return result;
    } catch (error) {
      if (error?.response?.status !== 404) throw error;
      return userApi.updateAddress(data);
    }
  },

  setDefaultAddress: (id) => api.put(`/users/me/addresses/${id}/default`).then(responseData),

  deleteAddress: (id) => api.delete(`/users/me/addresses/${id}`).then(responseData),

  getAll: (params) => api.get('/users', { params }).then(responseData),

  getById: (id) => api.get(`/users/${id}`).then(responseData),
};

// ===== Yêu thích =====

export const favoriteApi = {
  getMine: () => api.get('/favorites').then((res) => {
    const items = res.data?.items || res.data?.Items || res.data;
    return Array.isArray(items) ? items.map(mapFavorite) : [];
  }),

  add: (productId) => api.post(`/favorites/${productId}`).then((res) => mapFavorite(res.data)),

  remove: (productId) => api.delete(`/favorites/${productId}`),
};

// ===== Nội dung (blog, FAQ, liên hệ, voucher công khai) =====

export const contentApi = {
  getBlogPosts: (params) => api.get('/content/blog-posts', { params }),
  getFaqs: (params) => api.get('/content/faqs', { params }),
  createContactRequest: (data) => api.post('/content/contact-requests', {
    hoTen: data.fullName ?? data.name ?? data.hoTen,
    soDienThoai: data.phoneNumber ?? data.phone ?? data.soDienThoai,
    email: data.email,
    tieuDe: data.subject ?? data.tieuDe,
    noiDung: data.message ?? data.noiDung,
    loaiYeuCau: data.inquiryType ?? data.loaiYeuCau,
    maSanPham: data.productId ?? data.maSanPham,
  }),
  getVoucher: (code) => api.get(`/content/vouchers/${code}`),
};

export { AUTH_CHANGED_EVENT };
export default api;
