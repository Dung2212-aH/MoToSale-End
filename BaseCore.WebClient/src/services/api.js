import axios from 'axios';

const API_BASE_URL = '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const getApiErrorMessage = (error, fallback = 'Thao tac that bai') => {
  const data = error?.response?.data;
  if (typeof data === 'string') return data;
  if (data?.errors && typeof data.errors === 'object') {
    const details = Object.entries(data.errors)
      .flatMap(([field, messages]) => {
        const values = Array.isArray(messages) ? messages : [messages];
        return values.map((message) => `${field}: ${message}`);
      })
      .join(' ');
    if (details) return details;
  }
  return data?.message || data?.title || error?.message || fallback;
};

export const normalizePagedResponse = (payload, dataKey = 'items') => {
  if (Array.isArray(payload)) {
    return { items: payload, totalCount: payload.length, totalPages: 1, page: 1, pageSize: payload.length };
  }

  const items = payload?.[dataKey] || payload?.data || [];
  const totalCount = payload?.totalCount ?? payload?.totalItems ?? items.length;
  const pageSize = payload?.pageSize ?? items.length ?? 1;

  return {
    items,
    totalCount,
    totalPages: payload?.totalPages ?? Math.max(1, Math.ceil(totalCount / Math.max(pageSize || 1, 1))),
    page: payload?.page ?? 1,
    pageSize,
  };
};

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  },
);

const mapCategory = (item = {}) => ({
  id: item.id ?? item.maDanhMuc,
  parentCategoryId: item.parentCategoryId ?? item.maDanhMucCha ?? '',
  name: item.name ?? item.tenDanhMuc ?? '',
  slug: item.slug ?? '',
  description: item.description ?? item.moTa ?? '',
  sortOrder: item.sortOrder ?? item.thuTuHienThi ?? 0,
  isActive: item.isActive ?? item.dangHoatDong ?? true,
});

const mapShowroom = (item = {}) => ({
  id: item.id ?? item.maShowroom,
  name: item.name ?? item.tenShowroom ?? '',
  slug: item.slug ?? '',
  address: item.address ?? item.diaChi ?? '',
  phone: item.phone ?? item.soDienThoai ?? '',
  email: item.email ?? '',
  openingHours: item.openingHours ?? item.gioMoCua ?? '',
  isActive: item.isActive ?? item.dangHoatDong ?? true,
});

export const mapProduct = (item = {}) => ({
  id: item.id ?? item.maSanPham,
  productCode: item.productCode ?? item.maSanPhamKinhDoanh ?? '',
  name: item.name ?? item.tenSanPham ?? '',
  slug: item.slug ?? '',
  categoryId: item.categoryId ?? item.maDanhMuc ?? '',
  brandId: item.brandId ?? item.maHangXe ?? '',
  carModelId: item.carModelId ?? item.maDongXe ?? '',
  showroomId: item.showroomId ?? item.maShowroom ?? '',
  productType: item.productType ?? item.loaiSanPham ?? 'Motorcycle',
  shortDescription: item.shortDescription ?? item.moTaNgan ?? '',
  description: item.description ?? item.moTa ?? '',
  basePrice: item.basePrice ?? item.giaGoc ?? 0,
  salePrice: item.salePrice ?? item.giaKhuyenMai ?? '',
  price: item.price ?? item.giaBan ?? item.giaKhuyenMai ?? item.giaGoc ?? 0,
  discountPercent: item.discountPercent ?? item.tyLeGiam ?? null,
  stockQuantity: item.stockQuantity ?? item.soLuongTon ?? 0,
  mainImageUrl: item.mainImageUrl ?? item.anhChinhUrl ?? '',
  isActive: item.isActive ?? item.dangHoatDong ?? true,
  status: item.status ?? item.trangThaiSanPham ?? '',
  variants: (item.variants ?? item.bienThe ?? []).map((variant) => ({
    id: variant.id ?? variant.maBienSanPham,
    productId: variant.productId ?? variant.maSanPham,
    name: variant.name ?? variant.tenBienThe ?? '',
    sku: variant.sku ?? variant.sku ?? variant.SKU ?? '',
    priceOverride: variant.priceOverride ?? variant.giaGhiDe ?? '',
    stockQuantity: variant.stockQuantity ?? variant.soLuongTon ?? '',
    status: variant.status ?? variant.trangThai ?? '',
    version: variant.version ?? variant.phienBan ?? '',
    color: variant.color ?? variant.mauSac ?? '',
  })),
  images: (item.images ?? item.anh ?? []).map((image) => ({
    id: image.id ?? image.maAnhSanPham,
    productId: image.productId ?? image.maSanPham,
    variantId: image.variantId ?? image.maBienSanPham ?? '',
    imageUrl: image.imageUrl ?? image.urlAnh ?? '',
    altText: image.altText ?? '',
    isPrimary: image.isPrimary ?? image.laAnhChinh ?? false,
    sortOrder: image.sortOrder ?? image.thuTuHienThi ?? 0,
  })),
});

const productPayload = (data) => ({
  maSanPhamKinhDoanh: data.productCode?.trim(),
  tenSanPham: data.name?.trim(),
  slug: data.slug?.trim(),
  maDanhMuc: Number(data.categoryId),
  maHangXe: data.brandId ? Number(data.brandId) : null,
  maDongXe: data.carModelId ? Number(data.carModelId) : null,
  maShowroom: data.showroomId ? Number(data.showroomId) : null,
  loaiSanPham: data.productType?.trim() || 'Motorcycle',
  moTaNgan: data.shortDescription?.trim() || null,
  moTa: data.description?.trim() || null,
  giaGoc: Number(data.basePrice || 0),
  giaKhuyenMai: data.salePrice === '' || data.salePrice === null ? null : Number(data.salePrice),
  soLuongTon: Number.parseInt(data.stockQuantity || 0, 10),
  anhChinhUrl: data.mainImageUrl?.trim() || null,
  dangHoatDong: Boolean(data.isActive),
  trangThaiSanPham: data.status || 'Available',
});

const categoryPayload = (data) => ({
  maDanhMucCha: data.parentCategoryId ? Number(data.parentCategoryId) : null,
  tenDanhMuc: data.name?.trim(),
  slug: data.slug?.trim(),
  moTa: data.description?.trim() || null,
  thuTuHienThi: Number.parseInt(data.sortOrder || 0, 10),
  dangHoatDong: Boolean(data.isActive),
});

const uploadProductImage = (productId, data) => {
  const formData = new FormData();
  formData.append('MaSanPham', String(productId));
  formData.append('Image', data.file);
  formData.append('AltText', data.altText || '');
  formData.append('LaAnhChinh', String(Boolean(data.isPrimary)));
  formData.append('ThuTuHienThi', String(Number.parseInt(data.sortOrder || 0, 10)));

  return api.post(`/products/${productId}/images`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
};

export const authApi = {
  login: (email, password) => api.post('/auth/login', { email, matKhau: password }),
};

export const productApi = {
  getAll: async (params) => {
    const response = await api.get('/products', { params });
    const paged = normalizePagedResponse(response.data);
    return { ...response, data: { ...paged, items: paged.items.map(mapProduct) } };
  },
  getById: async (id) => {
    const response = await api.get(`/products/${id}`);
    return { ...response, data: mapProduct(response.data) };
  },
  getFilters: () => api.get('/products/filters'),
  create: (data) => api.post('/products', productPayload(data)),
  update: (id, data) => api.put(`/products/${id}`, productPayload(data)),
  delete: (id) => api.delete(`/products/${id}`),
  uploadImage: uploadProductImage,
  createWithImage: async (data, image) => {
    const response = await api.post('/products', productPayload(data));
    const product = mapProduct(response.data);
    if (image?.file && product.id) {
      await uploadProductImage(product.id, image);
    }
    return { ...response, data: product };
  },
};

export const categoryApi = {
  getAll: async (params) => {
    const response = await api.get('/categories', { params });
    const items = Array.isArray(response.data) ? response.data.map(mapCategory) : [];
    return { ...response, data: items };
  },
  create: (data) => api.post('/categories', categoryPayload(data)),
  update: (id, data) => api.put(`/categories/${id}`, categoryPayload(data)),
  delete: (id) => api.delete(`/categories/${id}`),
};

export const showroomApi = {
  getAll: async (params) => {
    const response = await api.get('/showrooms', { params });
    const items = Array.isArray(response.data) ? response.data.map(mapShowroom) : [];
    return { ...response, data: items };
  },
};

export const orderApi = {
  getAll: (params) => api.get('/orders', { params }),
  getById: (id) => api.get(`/orders/${id}`),
  updateStatus: (id, status) => api.patch(`/orders/${id}/status`, { trangThaiDonHang: status }),
  updateShipping: (id, data) => api.patch(`/orders/${id}/shipping`, {
    trangThaiVanChuyen: data.shippingStatus,
    ngayHenNhanXe: data.pickupAppointmentAt || null,
    ghiChuGiaoNhan: data.fulfillmentNote || null,
  }),
  cancel: (id, reason) => api.post(`/orders/${id}/cancel`, { lyDoHuyDon: reason }),
};

export const userApi = {
  getAll: (params) => api.get('/admin/users', { params }),
  toggleStatus: (id) => api.put(`/admin/users/${id}/toggle-status`),
};

export default api;
