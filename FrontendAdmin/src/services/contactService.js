import api from './api';

const mapFromV2 = (c) => ({
  ...c,
  maLienHe: c.id,
  hoTen: c.fullName,
  soDienThoai: c.phone,
  email: c.email,
  tieuDe: c.subject,
  noiDung: c.body,
  loaiYeuCau: c.type,
  maSanPham: c.productId,
  trangThai: c.contactStatus,
  ngayTao: c.createdDate,
  daXuLyLuc: c.handledAt,
});

const contactService = {
  getAll: async (params) => {
    const res = await api.get('/content/contacts', { params });
    const d = res.data;
    const items = (d.items || []).map(mapFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get('/content/contacts', { params: { page: 1, pageSize: 1000 } });
    const found = (res.data.items || []).map(mapFromV2).find((c) => c.id === Number(id));
    return { data: found };
  },
  markProcessed: (id) => api.patch(`/content/contacts/${id}/process`),
};

export default contactService;
