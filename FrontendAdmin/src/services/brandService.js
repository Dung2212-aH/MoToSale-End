import api from './api';

const mapBrand = (b) => ({ ...b, maHangXe: b.id, tenHang: b.name, slug: b.slug, logoUrl: b.logoUrl, dangHoatDong: b.status !== 0 });
const mapModel = (m) => ({ ...m, maDongXe: m.id, maHangXe: m.brandId, tenDongXe: m.name, slug: m.slug, dangHoatDong: m.status !== 0 });

const brandToV2 = (d) => ({ name: d.tenHang ?? d.name, slug: d.slug || null, logoUrl: d.logoUrl || null, status: d.dangHoatDong === false ? 0 : (d.status ?? 1) });
const modelToV2 = (d) => ({ brandId: d.maHangXe ?? d.brandId, name: d.tenDongXe ?? d.name, slug: d.slug || null, status: d.dangHoatDong === false ? 0 : (d.status ?? 1) });

const send = (method, url, data) => {
  const isFormData = typeof FormData !== 'undefined' && data instanceof FormData;
  return api.request({ method, url, data, headers: isFormData ? { 'Content-Type': 'multipart/form-data' } : undefined });
};

const brandService = {
  getAll: async (params) => {
    const res = await api.get('/brands', { params });
    const items = (res.data.items || res.data || []).map(mapBrand);
    return { ...res, data: { items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get('/brands');
    const found = (res.data.items || []).map(mapBrand).find((b) => b.id === Number(id));
    return { ...res, data: found };
  },
  create: (data) => api.post('/brands', brandToV2(data)),
  update: (id, data) => api.put(`/brands/${id}`, brandToV2(data)),
  uploadLogo: (id, formData) => send('post', `/brands/${id}/logo`, formData),
  delete: (id) => api.delete(`/brands/${id}`),

  getModels: async (brandId) => {
    const res = await api.get('/models', { params: { brandId } });
    const items = (res.data.items || res.data || []).map(mapModel);
    return { ...res, data: { items, data: items } };
  },
  getAllModels: async (params) => {
    const res = await api.get('/models', { params });
    const items = (res.data.items || res.data || []).map(mapModel);
    return { ...res, data: { items, data: items } };
  },
  createModel: (data) => api.post('/models', modelToV2(data)),
  updateModel: (id, data) => api.put(`/models/${id}`, modelToV2(data)),
  deleteModel: (id) => api.delete(`/models/${id}`),
};

export default brandService;
