import api from './api';

const mapFromV2 = (m) => ({
  ...m,
  maHangSanXuat: m.id,
  tenHangSanXuat: m.name,
  moTa: m.description,
  dangHoatDong: m.status !== 0,
});

const toV2 = (d) => ({
  name: d.tenHangSanXuat ?? d.name,
  description: d.moTa ?? d.description ?? null,
  status: d.dangHoatDong === false ? 0 : (d.status ?? 1),
});

const manufacturerService = {
  getAll: async () => {
    const res = await api.get('/manufacturers');
    const items = (res.data.items || res.data || []).map(mapFromV2);
    return { ...res, data: { items, data: items } };
  },
  create: (data) => api.post('/manufacturers', toV2(data)),
  update: (id, data) => api.put(`/manufacturers/${id}`, toV2(data)),
  delete: (id) => api.delete(`/manufacturers/${id}`),
};

export default manufacturerService;
