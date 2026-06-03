import api from './api';

const mapFromV2 = (f) => ({
  ...f,
  maFAQ: f.id,
  cauHoi: f.question,
  cauTraLoi: f.answer,
  danhMuc: f.category,
  thuTu: f.sortOrder,
  thuTuHienThi: f.sortOrder,
  dangHoatDong: f.status !== 0,
});

const toV2 = (d) => ({
  question: d.cauHoi ?? d.question,
  answer: d.cauTraLoi ?? d.answer ?? '',
  category: d.danhMuc ?? d.category ?? null,
  sortOrder: Number(d.thuTu ?? d.thuTuHienThi ?? d.sortOrder ?? 0),
  status: d.dangHoatDong === false ? 0 : (d.status ?? 1),
});

const faqService = {
  getAll: async (params) => {
    const res = await api.get('/content/faq', { params });
    const items = (res.data.items || res.data || []).map(mapFromV2);
    return { ...res, data: { items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get('/content/faq');
    const found = (res.data.items || []).map(mapFromV2).find((f) => f.id === Number(id));
    return { data: found };
  },
  create: (data) => api.post('/content/faq', toV2(data)),
  update: (id, data) => api.put(`/content/faq/${id}`, toV2(data)),
  delete: (id) => api.delete(`/content/faq/${id}`),
};

export default faqService;
