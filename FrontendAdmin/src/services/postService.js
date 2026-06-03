import api from './api';

const mapFromV2 = (p) => ({
  ...p,
  maBaiViet: p.id,
  tieuDe: p.title,
  slug: p.slug,
  tomTat: p.summary,
  noiDung: p.body,
  anhDaiDienUrl: p.coverUrl,
  danhMuc: p.category,
  trangThai: p.postStatus,
  xuatBanLuc: p.publishedAt,
  ngayTao: p.createdDate,
});

const toV2 = (d) => ({
  title: d.tieuDe ?? d.title,
  slug: d.slug || null,
  summary: d.tomTat ?? d.summary ?? null,
  body: d.noiDung ?? d.body ?? '',
  coverUrl: d.anhDaiDienUrl ?? d.coverUrl ?? null,
  category: d.danhMuc ?? d.category ?? null,
  postStatus: d.trangThai ?? d.postStatus ?? 'Draft',
  publishedAt: d.xuatBanLuc ?? d.publishedAt ?? null,
});

const postService = {
  getAll: async (params) => {
    const res = await api.get('/content/posts', { params });
    const d = res.data;
    const items = (d.items || []).map(mapFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get(`/content/posts/${id}`);
    return { ...res, data: mapFromV2(res.data) };
  },
  create: (data) => api.post('/content/posts', toV2(data)),
  update: (id, data) => api.put(`/content/posts/${id}`, toV2(data)),
  // v2: upload không gắn id -> trả {url}, FE lưu url vào trường ảnh
  uploadImage: (id, formData) => api.post('/content/posts/image', formData, { headers: { 'Content-Type': 'multipart/form-data' } }),
  delete: (id) => api.delete(`/content/posts/${id}`),
};

export default postService;
