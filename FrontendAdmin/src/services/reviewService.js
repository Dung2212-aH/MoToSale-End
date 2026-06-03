import api from './api';

const mapFromV2 = (r) => ({
  ...r,
  maDanhGia: r.id,
  maSanPham: r.productId,
  tenSanPham: r.productName,
  maNguoiDung: r.userId,
  tenNguoiDung: r.userName,
  diem: r.rating,
  tieuDe: r.title,
  noiDung: r.comment,
  hinhAnhUrl: r.imageUrl,
  trangThai: r.reviewStatus,
  ngayTao: r.createdDate,
});

const reviewService = {
  getAll: async (params) => {
    const res = await api.get('/reviews', { params });
    const d = res.data;
    const items = (d.items || []).map(mapFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get('/reviews', { params: { page: 1, pageSize: 1000 } });
    const found = (res.data.items || []).map(mapFromV2).find((x) => x.id === Number(id));
    return { data: found };
  },
  updateStatus: (id, data) => api.patch(`/reviews/${id}/status`, { status: data.trangThai || data.status || data.Status }),
  delete: (id) => api.delete(`/reviews/${id}`),
};

export default reviewService;
