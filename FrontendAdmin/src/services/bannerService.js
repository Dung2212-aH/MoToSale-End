import api from './api';

const mapFromV2 = (b) => ({
  ...b,
  maBanner: b.id,
  viTri: b.position,
  tieuDe: b.title,
  urlAnh: b.imageUrl,
  lienKet: b.link,
  thuTu: b.sortOrder,
  dangHoatDong: b.status !== 0,
});

const toV2 = (d) => ({
  position: d.viTri ?? d.position ?? 'Slider',
  title: d.tieuDe ?? d.title ?? null,
  imageUrl: d.urlAnh ?? d.imageUrl,
  link: d.lienKet ?? d.link ?? null,
  sortOrder: Number(d.thuTu ?? d.sortOrder ?? 0),
  status: d.dangHoatDong === false ? 0 : (d.status ?? 1),
});

const bannerService = {
  getAll: async () => {
    const res = await api.get('/content/home-banners', { params: { all: true } });
    const items = (res.data.items || res.data || []).map(mapFromV2);
    return { ...res, data: { items, data: items } };
  },
  create: (data) => api.post('/content/home-banners', toV2(data)),
  update: (id, data) => api.put(`/content/home-banners/${id}`, toV2(data)),
  delete: (id) => api.delete(`/content/home-banners/${id}`),
  uploadImage: (formData) => api.post('/content/home-banners/image', formData, { headers: { 'Content-Type': 'multipart/form-data' } }),
};

export default bannerService;
