import api from './api';

// ===== Adapter v2 <-> shape cũ (FE cũ đọc field tiếng Việt) =====
const kindToLoai = (k) => (Number(k) === 2 ? 'PhuTung' : 'XeMay');
const loaiToKind = (l) => (l === 'PhuTung' ? 2 : 1);

const mapVariantFromV2 = (s) => ({
  ...s,
  maBienSanPham: s.id,
  tenBienThe: s.variantName,
  sku: s.skuCode,
  SKU: s.skuCode,
  mauSac: s.color,
  phienBan: s.version,
  giaGhiDe: s.listPrice,
  giaKhuyenMai: s.salePrice,
  soLuongTon: s.soLuongTon ?? 0,
  trangThai: s.status === 0 ? 'Inactive' : 'Available',
});

const mapImageFromV2 = (i) => ({
  ...i,
  maAnhSanPham: i.id,
  urlAnh: i.url,
  altText: i.alt,
  laAnhChinh: i.isPrimary,
  thuTuHienThi: i.sortOrder,
  maBienSanPham: i.skuId,
});

const mapCompatFromV2 = (c) => ({
  ...c,
  maTuongThich: c.id,
  maPhuTung: c.partProductId,
  maHangXe: c.brandId,
  tenHang: c.brandName,
  maDongXe: c.vehicleModelId,
  tenDongXe: c.vehicleModelName,
  namTu: c.yearFrom,
  namDen: c.yearTo,
  apDungTatCaXe: c.appliesToAll,
  ghiChu: c.note,
});

const mapProductFromV2 = (p) => {
  if (!p) return p;
  const listPrice = p.listPrice ?? p.skus?.[0]?.listPrice ?? 0;
  const salePrice = p.salePrice ?? p.skus?.[0]?.salePrice ?? null;
  const primaryImg = p.mainImageUrl || p.images?.find((i) => i.isPrimary)?.url || p.images?.[0]?.url || null;
  return {
    ...p,
    maSanPham: p.id,
    maSanPhamKinhDoanh: p.code,
    maSP: p.code,
    tenSanPham: p.name,
    slug: p.slug,
    maDanhMuc: p.categoryId,
    maHangXe: p.brandId ?? null,
    maDongXe: p.vehicleModelId ?? null,
    loaiSanPham: kindToLoai(p.kind),
    moTaNgan: p.shortDescription ?? '',
    moTa: p.description ?? '',
    hangSanXuatId: p.manufacturerId ?? null,
    tenHangSanXuat: p.manufacturerName ?? '',
    hangSanXuat: p.manufacturerName ?? '',
    giaGoc: listPrice,
    giaKhuyenMai: salePrice,
    giaBan: salePrice ?? listPrice,
    soLuongTon: p.stockTotal ?? p.soLuongTon ?? 0,
    trangThaiSanPham: p.status === 0 ? 'Inactive' : 'Available',
    anhChinhUrl: primaryImg,
    noiBat: p.isFeatured,
    hotDeal: p.isHotDeal,
    bienThe: (p.skus || []).map(mapVariantFromV2),
    anh: (p.images || []).map(mapImageFromV2),
  };
};

const mapListParams = (params = {}) => {
  const out = { page: params.page, pageSize: params.pageSize };
  if (params.search || params.keyword) out.keyword = params.search || params.keyword;
  if (params.loaiSanPham) out.kind = loaiToKind(params.loaiSanPham);
  if (params.maDanhMuc) out.categoryId = params.maDanhMuc;
  if (params.maHangXe) out.brandId = params.maHangXe;
  return out;
};

const productService = {
  getAll: async (params) => {
    const res = await api.get('/products', { params: mapListParams(params) });
    const d = res.data;
    return { ...res, data: { ...d, items: (d.items || []).map(mapProductFromV2) } };
  },
  getById: async (id) => {
    const res = await api.get(`/products/${id}`);
    return { ...res, data: mapProductFromV2(res.data) };
  },
  create: (data) => api.post('/products', {
    code: data.maSanPhamKinhDoanh || undefined,
    name: data.tenSanPham,
    slug: data.slug || undefined,
    categoryId: data.maDanhMuc,
    brandId: data.maHangXe ?? null,
    vehicleModelId: data.maDongXe ?? null,
    kind: loaiToKind(data.loaiSanPham),
    shortDescription: data.moTaNgan || null,
    description: data.moTa || null,
    isFeatured: !!data.noiBat,
    isHotDeal: !!data.hotDeal,
    listPrice: Number(data.giaGoc) || 0,
    salePrice: data.giaKhuyenMai != null && data.giaKhuyenMai !== '' ? Number(data.giaKhuyenMai) : null,
    manufacturerId: data.hangSanXuatId ? Number(data.hangSanXuatId) : null,
  }),
  update: (id, data) => api.put(`/products/${id}`, {
    name: data.tenSanPham,
    slug: data.slug || null,
    categoryId: data.maDanhMuc,
    brandId: data.maHangXe ?? null,
    vehicleModelId: data.maDongXe ?? null,
    shortDescription: data.moTaNgan || null,
    description: data.moTa || null,
    isFeatured: !!data.noiBat,
    isHotDeal: !!data.hotDeal,
    status: data.trangThaiSanPham === 'Inactive' ? 0 : 1,
    manufacturerId: data.hangSanXuatId ? Number(data.hangSanXuatId) : null,
  }),
  delete: (id) => api.delete(`/products/${id}`),

  // Biến thể: v2 dùng /skus
  getVariants: async (productId) => {
    const res = await api.get(`/products/${productId}/skus`);
    return { ...res, data: (res.data.items || []).map(mapVariantFromV2) };
  },
  createVariant: (productId, data) => api.post(`/products/${productId}/skus`, {
    skuCode: data.sku || data.SKU || null, variantName: data.tenBienThe || null, color: data.mauSac || null,
    version: data.phienBan || null, listPrice: Number(data.giaGhiDe) || 0,
    salePrice: data.giaKhuyenMai != null && data.giaKhuyenMai !== '' ? Number(data.giaKhuyenMai) : null, barcode: data.barcode || null,
  }),
  updateVariant: (productId, variantId, data) => api.put(`/products/${productId}/skus/${variantId}`, {
    skuCode: data.sku || data.SKU || null, variantName: data.tenBienThe || null, color: data.mauSac || null,
    version: data.phienBan || null, listPrice: Number(data.giaGhiDe) || 0,
    salePrice: data.giaKhuyenMai != null && data.giaKhuyenMai !== '' ? Number(data.giaKhuyenMai) : null, barcode: data.barcode || null,
    status: data.trangThai === 'Inactive' ? 0 : 1,
  }),
  deleteVariant: (productId, variantId) => api.delete(`/products/${productId}/skus/${variantId}`),

  getImages: async (productId) => {
    const res = await api.get(`/products/${productId}/images`);
    return { ...res, data: (res.data.items || []).map(mapImageFromV2) };
  },
  uploadImage: (productId, formData) => api.post(`/products/${productId}/images`, formData, { headers: { 'Content-Type': 'multipart/form-data' } }),
  deleteImage: (productId, imageId) => api.delete(`/products/${productId}/images/${imageId}`),
  setPrimaryImage: (productId, imageId) => api.post(`/products/${productId}/images/${imageId}/primary`),

  getCompatibilities: async (productId) => {
    const res = await api.get(`/products/${productId}/compatibilities`);
    return { ...res, data: (res.data.items || []).map(mapCompatFromV2) };
  },
  createCompatibility: (productId, data) => api.post(`/products/${productId}/compatibilities`, {
    brandId: data.maHangXe ?? null, vehicleModelId: data.maDongXe ?? null, yearFrom: data.namTu ?? null, yearTo: data.namDen ?? null, appliesToAll: !!data.apDungTatCaXe, note: data.ghiChu || null,
  }),
  updateCompatibility: (productId, compatibilityId, data) => api.put(`/products/${productId}/compatibilities/${compatibilityId}`, {
    brandId: data.maHangXe ?? null, vehicleModelId: data.maDongXe ?? null, yearFrom: data.namTu ?? null, yearTo: data.namDen ?? null, appliesToAll: !!data.apDungTatCaXe, note: data.ghiChu || null,
  }),
  deleteCompatibility: (productId, compatibilityId) => api.delete(`/products/${productId}/compatibilities/${compatibilityId}`),
};

export default productService;
