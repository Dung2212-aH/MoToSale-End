// Lớp service gọi backend của storefront (3 lớp: page -> api -> http; chuẩn hóa ở normalizers.js).
// - Gọi axios qua `api` (httpClient.js lo baseURL/token/interceptor).
// - Map dữ liệu qua helper trong normalizers.js (mapOrder/mapVoucher/field/toQuery...), trả shape ổn định cho UI.
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
  field,
  mapOrder,
  mapPayment,
  mapVoucher,
  mapFavorite,
  mapReview,
  buildReviewForm,
  toQuery,
  listOf,
  mapAddressBody,
  normalizeCart,
  normalizeCategory,
  normalizeFilters,
  normalizeProduct,
  normalizeProductList,
} from './normalizers.js';
import { notifyCartChanged } from '../utils/cartEvents.js';

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

  getMyOrders: () => orderApi.getAll(),

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
};

// ===== Voucher =====

export const voucherApi = {
  getAll: (params) => api.get('/vouchers/active', { params: toQuery(params) }).then((res) => {
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
};

export { AUTH_CHANGED_EVENT };
export default api;
