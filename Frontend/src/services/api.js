import axios from 'axios';
import {
  normalizeCart,
  normalizeCategory,
  normalizeFilters,
  normalizeProduct,
  normalizeProductList,
} from '../utils/productMappers.js';
import { notifyCartChanged } from '../utils/cartEvents.js';
import { normalizeImageUrl } from '../utils/formatters.js';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';
const TOKEN_KEY = 'token';
const USER_KEY = 'user';

export const AUTH_CHANGED_EVENT = 'basecore:auth-changed';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const getStorage = (type) => {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    return window[type];
  } catch {
    return null;
  }
};

const sessionAuthStorage = {
  getItem(key) {
    return getStorage('sessionStorage')?.getItem(key) ?? null;
  },

  setItem(key, value) {
    getStorage('sessionStorage')?.setItem(key, value);
  },

  removeItem(key) {
    getStorage('sessionStorage')?.removeItem(key);
  },
};

const legacyAuthStorage = {
  getItem(key) {
    return getStorage('localStorage')?.getItem(key) ?? null;
  },

  setItem(key, value) {
    getStorage('localStorage')?.setItem(key, value);
  },

  removeItem(key) {
    getStorage('localStorage')?.removeItem(key);
  },
};

const responseData = (response) => response.data;

const field = (source, ...keys) => {
  for (const key of keys) {
    if (source?.[key] !== undefined && source?.[key] !== null) {
      return source[key];
    }
  }

  return undefined;
};

const normalizeOrder = (raw = {}) => {
  const items = field(raw, 'items', 'Items') || [];
  const vouchers = field(raw, 'vouchers', 'Vouchers') || [];

  return {
    ...raw,
    id: field(raw, 'id', 'Id', 'maDonHang', 'MaDonHang'),
    orderCode: field(raw, 'orderCode', 'OrderCode', 'maDonHangKinhDoanh', 'MaDonHangKinhDoanh'),
    userId: field(raw, 'userId', 'UserId', 'maNguoiDung', 'MaNguoiDung'),
    showroomId: field(raw, 'showroomId', 'ShowroomId', 'maShowroom', 'MaShowroom'),
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

const normalizePayment = (raw = {}) => ({
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

const normalizeVoucher = (raw = {}) => ({
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

const normalizeFavorite = (raw = {}) => {
  const productRaw = field(raw, 'product', 'Product') || raw;
  const product = normalizeProduct(productRaw);

  return {
    ...raw,
    userId: field(raw, 'userId', 'UserId', 'maNguoiDung', 'MaNguoiDung'),
    productId: field(raw, 'productId', 'ProductId', 'maSanPham', 'MaSanPham') || product?.id,
    createdAt: field(raw, 'createdAt', 'CreatedAt', 'ngayTao', 'NgayTao'),
    product,
  };
};

const normalizeReview = (raw = {}) => ({
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

const normalizeReviewPayload = (payload = {}) => {
  const formData = new FormData();
  formData.append('Diem', payload.rating ?? payload.diem);
  formData.append('NoiDung', payload.comment ?? payload.noiDung ?? '');

  if (payload.productId ?? payload.maSanPham) {
    formData.append('MaSanPham', payload.productId ?? payload.maSanPham);
  }

  if (payload.orderId ?? payload.maDonHang) {
    formData.append('MaDonHang', payload.orderId ?? payload.maDonHang);
  }

  if (payload.title ?? payload.tieuDe) {
    formData.append('TieuDe', payload.title ?? payload.tieuDe);
  }

  if (payload.image) {
    formData.append('Image', payload.image);
  }

  return formData;
};

const cleanParams = (params = {}) => {
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
    showroomId: 'MaShowroom',
    productType: 'LoaiSanPham',
    status: 'TrangThaiSanPham',
    minPrice: 'GiaTu',
    maxPrice: 'GiaDen',
  };

  const sourceParams = { ...params };
  if (sortMap[params.sortBy]) {
    delete sourceParams.sortBy;
    Object.assign(sourceParams, sortMap[params.sortBy]);
  }

  const mappedParams = Object.entries(sourceParams).reduce((acc, [key, value]) => {
    acc[paramMap[key] || key] = value;
    return acc;
  }, {});

  return Object.fromEntries(
    Object.entries(mappedParams).filter(
      ([, value]) => value !== '' && value !== undefined && value !== null,
    ),
  );
};

const notifyAuthChanged = (user = null) => {
  window.dispatchEvent(new CustomEvent(AUTH_CHANGED_EVENT, { detail: { user } }));
};

const clearAuthStorage = (notify = true) => {
  sessionAuthStorage.removeItem(TOKEN_KEY);
  sessionAuthStorage.removeItem(USER_KEY);
  legacyAuthStorage.removeItem(TOKEN_KEY);
  legacyAuthStorage.removeItem(USER_KEY);

  if (notify) {
    notifyAuthChanged(null);
  }
};

const getStoredUser = () => {
  const rawUser = sessionAuthStorage.getItem(USER_KEY) || legacyAuthStorage.getItem(USER_KEY);

  if (!rawUser) {
    return null;
  }

  try {
    return JSON.parse(rawUser);
  } catch {
    return null;
  }
};

const isTokenExpired = (token) => {
  const claims = decodeJwtPayload(token);
  const expiresAt = Number(claims?.exp);

  if (!Number.isFinite(expiresAt)) {
    return true;
  }

  return Date.now() >= expiresAt * 1000;
};

const decodeJwtPayload = (token) => {
  if (!token) {
    return null;
  }

  try {
    const [, payload] = token.split('.');
    if (!payload) {
      return null;
    }

    const normalizedPayload = payload.replace(/-/g, '+').replace(/_/g, '/');
    const decodedPayload = decodeURIComponent(
      atob(normalizedPayload)
        .split('')
        .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join(''),
    );

    return JSON.parse(decodedPayload);
  } catch {
    return null;
  }
};

const getClaim = (claims, key) => claims?.[key] || claims?.[`http://schemas.xmlsoap.org/ws/2005/05/identity/claims/${key}`];

const normalizeLoginResponse = (data) => {
  const user = data?.user || data?.User || data;
  const roles = user?.roles || user?.Roles || [];
  const role = data?.role || data?.Role || roles[0];

  return {
    token: data?.token || data?.Token,
    userId: data?.userId || data?.UserId || user?.id || user?.Id,
    username: data?.username || data?.Username || user?.email || user?.Email,
    name: data?.name || data?.Name || user?.hoTen || user?.HoTen,
    email: data?.email || data?.Email || user?.email || user?.Email,
    phone: data?.phone || data?.Phone || user?.soDienThoai || user?.SoDienThoai,
    role,
    roles,
    userType: data?.userType ?? data?.UserType,
    expiresIn: data?.expiresIn || data?.ExpiresIn,
    expiresAt: data?.expiresAt || data?.ExpiresAt,
    raw: data,
  };
};

const saveAuthUser = (user, rememberMe = false) => {
  if (!user?.token) {
    throw new Error('Không nhận được token đăng nhập từ máy chủ');
  }

  const targetStorage = rememberMe ? legacyAuthStorage : sessionAuthStorage;
  const staleStorage = rememberMe ? sessionAuthStorage : legacyAuthStorage;

  targetStorage.setItem(TOKEN_KEY, user.token);
  targetStorage.setItem(USER_KEY, JSON.stringify(user));
  staleStorage.removeItem(TOKEN_KEY);
  staleStorage.removeItem(USER_KEY);

  notifyAuthChanged(user);
};

const mergeStoredUser = (data = {}) => {
  const currentUser = getStoredUser();
  const token = currentUser?.token || getToken();

  if (!token) {
    return null;
  }

  const nextUser = {
    ...currentUser,
    ...data,
    token,
  };

  const targetStorage = sessionAuthStorage.getItem(TOKEN_KEY) ? sessionAuthStorage : legacyAuthStorage;
  targetStorage.setItem(USER_KEY, JSON.stringify(nextUser));
  notifyAuthChanged(nextUser);
  return nextUser;
};

api.interceptors.request.use(
  (config) => {
    const token = getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.message || error.response?.data?.Message;
    if (message) {
      error.message = message;
    }

    return Promise.reject(error);
  },
);

export const authApi = {
  async login({ username, password, rememberMe }) {
    const response = await api.post('/auth/login', { email: username, matKhau: password });
    const user = normalizeLoginResponse(responseData(response));
    saveAuthUser(user, rememberMe === true || rememberMe === 'true' || rememberMe === 'on');
    return user;
  },

  register: (data) => api.post('/auth/register', {
    hoTen: data.name,
    email: data.email,
    soDienThoai: data.phone,
    matKhau: data.password,
  }),

  async forgotPassword(email) {
    const response = await api.post('/auth/forgot-password', { email });
    return responseData(response);
  },

  async resetPassword(data) {
    const response = await api.post('/auth/reset-password', {
      email: data.email,
      token: data.token,
      matKhauMoi: data.password,
    });
    return responseData(response);
  },

  logout() {
    clearAuthStorage();
  },

  getCurrentUser() {
    const token = getToken();

    if (!token) {
      clearAuthStorage(false);
      return null;
    }

    if (isTokenExpired(token)) {
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

  updateStoredUser(data) {
    return mergeStoredUser(data);
  },
};

function getToken() {
  const token = sessionAuthStorage.getItem(TOKEN_KEY);

  if (token) {
    return token;
  }

  const legacyToken = legacyAuthStorage.getItem(TOKEN_KEY);
  if (legacyToken) {
    return legacyToken;
  }

  return null;
}

export const productApi = {
  async getAll(params) {
    const response = await api.get('/products', { params: cleanParams({ DangHoatDong: true, ...params }) });
    return normalizeProductList(responseData(response));
  },

  async getById(id) {
    const response = await api.get(`/products/${id}`);
    return normalizeProduct(responseData(response));
  },

  async getFilters() {
    const response = await api.get('/products/filters');
    return normalizeFilters(responseData(response));
  },

  getProducts(params) {
    return productApi.getAll(params);
  },

  getProductById(id) {
    return productApi.getById(id);
  },

};

export const reviewApi = {
  async getByProduct(productId) {
    const response = await api.get(`/products/${productId}/reviews`);
    const data = responseData(response);
    return (Array.isArray(data) ? data : data?.items || data?.Items || []).map(normalizeReview);
  },

  async getSummary(productId) {
    const response = await api.get(`/products/${productId}/reviews/summary`);
    const data = responseData(response);
    return {
      productId: field(data, 'productId', 'ProductId', 'maSanPham', 'MaSanPham'),
      totalReviews: Number(field(data, 'totalReviews', 'TotalReviews', 'tongDanhGia', 'TongDanhGia') || 0),
      averageRating: Number(field(data, 'averageRating', 'AverageRating', 'diemTrungBinh', 'DiemTrungBinh') || 0),
    };
  },

  async getMine(productId) {
    const response = await api.get(`/reviews/product/${productId}/me`);
    const data = responseData(response);
    const myReview = field(data, 'myReview', 'MyReview', 'danhGiaCuaToi', 'DanhGiaCuaToi');

    return {
      productId: field(data, 'productId', 'ProductId', 'maSanPham', 'MaSanPham'),
      isAuthenticated: field(data, 'isAuthenticated', 'IsAuthenticated', 'daDangNhap', 'DaDangNhap') === true,
      hasPurchased: field(data, 'hasPurchased', 'HasPurchased', 'daMua', 'DaMua') === true,
      canReview: field(data, 'canReview', 'CanReview', 'coTheDanhGia', 'CoTheDanhGia') === true,
      eligibleOrderId: field(data, 'eligibleOrderId', 'EligibleOrderId', 'maDonHangDuDieuKien', 'MaDonHangDuDieuKien'),
      reason: field(data, 'reason', 'Reason', 'lyDo', 'LyDo'),
      myReview: myReview ? normalizeReview(myReview) : null,
    };
  },

  async create(productId, payload) {
    const formData = normalizeReviewPayload({ ...payload, productId });
    const response = await api.post(`/products/${productId}/reviews`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    const data = responseData(response);
    return {
      ...data,
      review: data?.review || data?.Review ? normalizeReview(data.review || data.Review) : null,
    };
  },

  async updateMine(productId, payload) {
    const formData = normalizeReviewPayload(payload);
    const response = await api.patch(`/products/${productId}/reviews/me`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    const data = responseData(response);
    return {
      ...data,
      review: data?.review || data?.Review ? normalizeReview(data.review || data.Review) : null,
    };
  },
};

export const categoryApi = {
  async getAll() {
    const response = await api.get('/categories');
    const data = responseData(response);
    return {
      ...response,
      data: (Array.isArray(data) ? data : data?.items || data?.Items || []).map(normalizeCategory),
    };
  },
};

const normalizeCartResponse = (response) => {
  const cart = normalizeCart(responseData(response));
  notifyCartChanged(cart);
  return cart;
};

export const cartApi = {
  async getMine() {
    const response = await api.get('/cart');
    return normalizeCartResponse(response);
  },

  getCart() {
    return cartApi.getMine();
  },

  async getCount() {
    const response = await api.get('/cart/count');
    const data = responseData(response);
    return Number(data?.count ?? data?.totalItems ?? data ?? 0);
  },

  async addItem(data) {
    const response = await api.post('/cart/items', {
      maSanPham: data.productId,
      maBienSanPham: data.variantId ?? data.productVariantId ?? null,
      soLuong: data.quantity,
    });
    return normalizeCartResponse(response);
  },

  async updateItem(id, quantityOrData) {
    const data = typeof quantityOrData === 'object' ? quantityOrData : { quantity: quantityOrData };
    await api.put(`/cart/items/${id}`, {
      soLuong: data.quantity ?? data.soLuong,
    });
    return cartApi.getMine();
  },

  async removeItem(id) {
    await api.delete(`/cart/items/${id}`);
    return cartApi.getMine();
  },

  async clearCart() {
    const response = await api.delete('/cart/clear');
    return normalizeCartResponse(response);
  },
};

export const orderApi = {
  async getAll(params) {
    const response = await api.get('/orders', { params });
    const data = responseData(response);
    if (Array.isArray(data)) {
      return data.map(normalizeOrder);
    }

    const items = data?.items || data?.Items;
    return items ? items.map(normalizeOrder) : normalizeOrder(data);
  },

  getMyOrders() {
    return orderApi.getAll();
  },

  async getById(id) {
    const response = await api.get(`/orders/${id}`);
    const order = normalizeOrder(responseData(response));
    return {
      ...order,
      order,
      details: order.items,
      vouchers: order.vouchers,
    };
  },

  getOrderById(id) {
    return orderApi.getById(id);
  },

  async createOrder(data) {
    const response = await api.post('/orders', {
      maShowroom: data.showroomId ?? data.MaShowroom ?? null,
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
      tienDatCoc: data.depositAmount ?? 0,
      ngayHenNhanXe: data.pickupAppointmentAt,
      ghiChuGiaoNhan: data.fulfillmentNote,
      soPhutGiuCho: data.holdMinutes ?? 15,
    });
    return normalizeOrder(responseData(response));
  },

  async getShippingQuote(data) {
    const response = await api.post('/orders/shipping-quote', {
      phuongThucNhanHang: data.receivingMethod,
      shippingProvince: data.shippingProvince,
      maVoucherCode: data.voucherCode,
      orderType: data.orderType,
    });
    return responseData(response);
  },

  async cancelOrder(id, reason) {
    const response = await api.put(`/orders/${id}/cancel`, { lyDoHuyDon: reason });
    return normalizeOrder(responseData(response));
  },
};

export const paymentApi = {
  async getPaymentsByOrder(orderId) {
    const response = await api.get(`/payments/order/${orderId}`);
    const data = responseData(response);
    const items = data?.items || data?.Items || data?.payments || data?.Payments || data;
    return Array.isArray(items) ? items.map(normalizePayment) : items;
  },

  async createPayment(data) {
    const response = await api.post('/payments', {
      maDonHang: data.orderId ?? data.maDonHang,
      loaiThanhToan: data.paymentType ?? data.loaiThanhToan ?? 'Full',
      soTien: data.amount ?? data.soTien,
      phuongThuc: data.paymentMethod ?? data.phuongThuc ?? 'BankTransfer',
      maGiaoDich: data.transactionRef ?? data.maGiaoDich,
      noiDungChuyenKhoan: data.transferContent ?? data.noiDungChuyenKhoan,
      maNganHang: data.bankCode ?? data.maNganHang,
      responseRaw: data.responseRaw,
    });
    return normalizePayment(responseData(response));
  },

  async confirmSuccess(paymentId, data = {}) {
    const response = await api.post(`/payments/${paymentId}/confirm-success`, {
      maGiaoDich: data.transactionRef ?? data.maGiaoDich,
      responseRaw: data.responseRaw,
    });
    return responseData(response);
  },
};

export const voucherApi = {
  async getAll(params) {
    const response = await api.get('/vouchers', { params: cleanParams(params) });
    const data = responseData(response);
    const items = data?.items || data?.Items || data;
    return Array.isArray(items) ? items.map(normalizeVoucher) : items;
  },

  listVouchers(params) {
    return voucherApi.getAll(params);
  },

  async validateVoucher(data) {
    const response = await api.post('/vouchers/validate', data);
    const result = responseData(response);
    return {
      ...result,
      valid: field(result, 'valid', 'Valid', 'hopLe', 'HopLe') === true,
      message: field(result, 'message', 'Message', 'lyDoKhongHopLe', 'LyDoKhongHopLe'),
      discountAmount: Number(field(result, 'discountAmount', 'DiscountAmount', 'soTienGiam', 'SoTienGiam') || 0),
      voucher: normalizeVoucher(field(result, 'voucher', 'Voucher') || result),
    };
  },

  async getApplicableVouchers(data) {
    const response = await api.post('/vouchers/applicable', data);
    const result = responseData(response);
    const items = result?.items || result?.Items || result;
    return Array.isArray(items) ? items.map(normalizeVoucher) : items;
  },

  async saveVoucher(code) {
    const response = await api.post('/vouchers/save', { code });
    return responseData(response);
  },

  async getMyVouchers() {
    const response = await api.get('/vouchers/my');
    const data = responseData(response);
    const items = data?.items || data?.Items || data;
    return Array.isArray(items) ? items.map(normalizeVoucher) : items;
  },

  async getMyVoucherCount() {
    const response = await api.get('/vouchers/my/count');
    const data = responseData(response);
    return data?.count ?? 0;
  },
};

export const userApi = {
  async getProfile() {
    const response = await api.get('/users/me');
    return responseData(response);
  },

  async updateProfile(data) {
    const response = await api.put('/users/me', {
      hoTen: data.name,
      email: data.email,
      soDienThoai: data.phone,
    });
    return responseData(response);
  },

  async changePassword(data) {
    const response = await api.put('/users/me/password', {
      matKhauHienTai: data.currentPassword,
      matKhauMoi: data.newPassword,
    });
    return responseData(response);
  },

  async getAddress() {
    const response = await api.get('/users/me/address');
    return responseData(response);
  },

  async getAddresses() {
    try {
      const response = await api.get('/users/me/addresses');
      const data = responseData(response);
      return data?.items || data?.Items || [];
    } catch (error) {
      if (error?.response?.status !== 404) throw error;
      const fallback = await userApi.getAddress();
      return fallback && Object.keys(fallback).length ? [fallback] : [];
    }
  },

  async updateAddress(data) {
    const response = await api.put('/users/me/address', {
      hoTenNhanHang: data.fullName,
      soDienThoaiNhanHang: data.phoneNumber,
      diaChiNhanHang: data.addressLine,
      ward: data.ward,
      district: data.district,
      province: data.province,
      ghiChu: data.note,
      laMacDinh: true,
    });
    return responseData(response);
  },

  async createAddress(data) {
    try {
      const response = await api.post('/users/me/addresses', {
        hoTenNhanHang: data.fullName,
        soDienThoaiNhanHang: data.phoneNumber,
        diaChiNhanHang: data.addressLine,
        ward: data.ward,
        district: data.district,
        province: data.province,
        ghiChu: data.note,
        laMacDinh: Boolean(data.isDefault),
      });
      return responseData(response);
    } catch (error) {
      if (error?.response?.status !== 404) throw error;
      return userApi.updateAddress(data);
    }
  },

  async updateAddressById(id, data) {
    try {
      const response = await api.put(`/users/me/addresses/${id}`, {
        hoTenNhanHang: data.fullName,
        soDienThoaiNhanHang: data.phoneNumber,
        diaChiNhanHang: data.addressLine,
        ward: data.ward,
        district: data.district,
        province: data.province,
        ghiChu: data.note,
        laMacDinh: Boolean(data.isDefault),
      });
      return responseData(response);
    } catch (error) {
      if (error?.response?.status !== 404) throw error;
      return userApi.updateAddress(data);
    }
  },

  async setDefaultAddress(id) {
    const response = await api.put(`/users/me/addresses/${id}/default`);
    return responseData(response);
  },

  async deleteAddress(id) {
    const response = await api.delete(`/users/me/addresses/${id}`);
    return responseData(response);
  },

  async getAll(params) {
    const response = await api.get('/users', { params });
    return responseData(response);
  },

  async getById(id) {
    const response = await api.get(`/users/${id}`);
    return responseData(response);
  },

  getUsers(params) {
    return userApi.getAll(params);
  },

  getUserById(id) {
    return userApi.getById(id);
  },
};

export const favoriteApi = {
  async getMine() {
    const response = await api.get('/favorites');
    const data = responseData(response);
    const items = data?.items || data?.Items || data;
    return Array.isArray(items) ? items.map(normalizeFavorite) : [];
  },

  async add(productId) {
    const response = await api.post(`/favorites/${productId}`);
    return normalizeFavorite(responseData(response));
  },

  remove: (productId) => api.delete(`/favorites/${productId}`),
};

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
    maShowroom: data.showroomId ?? data.maShowroom,
  }),
  getVoucher: (code) => api.get(`/content/vouchers/${code}`),
};

export default api;
