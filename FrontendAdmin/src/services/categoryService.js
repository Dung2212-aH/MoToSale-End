import api from './api';

const kindToLoai = (k) => (Number(k) === 2 ? 'PhuTung' : 'XeMay');
const loaiToKind = (l) => (l === 'PhuTung' ? 2 : 1);

const mapFromV2 = (c) => ({
  ...c,
  maDanhMuc: c.id,
  maDanhMucCha: c.parentId ?? null,
  tenDanhMuc: c.name,
  slug: c.slug,
  thuTu: c.sortOrder,
  thuTuHienThi: c.sortOrder,
  loaiSanPham: kindToLoai(c.kind),
  dangHoatDong: c.status !== 0,
});

const toV2 = (d) => ({
  parentId: (d.maDanhMucCha ?? d.parentId) || null,
  name: d.tenDanhMuc ?? d.name,
  slug: d.slug || null,
  kind: d.kind ?? loaiToKind(d.loaiSanPham),
  sortOrder: Number(d.thuTu ?? d.thuTuHienThi ?? d.sortOrder ?? 0),
  status: d.dangHoatDong === false ? 0 : (d.status ?? 1),
});

const categoryService = {
  getAll: async (params) => {
    const res = await api.get('/categories', { params });
    const items = (res.data.items || res.data || []).map(mapFromV2);
    return { ...res, data: { items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get('/categories');
    const found = (res.data.items || []).map(mapFromV2).find((c) => c.id === Number(id));
    return { ...res, data: found };
  },
  create: (data) => api.post('/categories', toV2(data)),
  update: (id, data) => api.put(`/categories/${id}`, toV2(data)),
  delete: (id) => api.delete(`/categories/${id}`),
};

export default categoryService;
