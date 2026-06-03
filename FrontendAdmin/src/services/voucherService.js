import api from './api';

const mapFromV2 = (v) => ({
  ...v,
  maVoucher: v.id,
  maVoucherCode: v.code,
  moTa: v.description,
  loaiGiamGia: v.discountType,
  giaTriGiam: v.discountValue,
  giaTriGiamToiDa: v.maxDiscount,
  giaTriDonToiThieu: v.minOrderValue,
  soLuotSuDung: v.usageLimit,
  soLuotMoiNguoi: v.perUserLimit,
  daSuDung: v.usedCount,
  ngayBatDau: v.startAt,
  ngayKetThuc: v.endAt,
  dangHoatDong: v.status !== 0,
});

const toV2 = (d) => ({
  code: d.code ?? d.maVoucherCode,
  description: d.moTa ?? d.description ?? null,
  discountType: d.loaiGiamGia ?? d.discountType ?? 'Percent',
  discountValue: Number(d.giaTriGiam ?? d.discountValue) || 0,
  maxDiscount: (d.giaTriGiamToiDa ?? d.maxDiscount) != null && (d.giaTriGiamToiDa ?? d.maxDiscount) !== '' ? Number(d.giaTriGiamToiDa ?? d.maxDiscount) : null,
  minOrderValue: Number(d.giaTriDonToiThieu ?? d.minOrderValue) || 0,
  usageLimit: (d.soLuotSuDung ?? d.usageLimit) ? Number(d.soLuotSuDung ?? d.usageLimit) : null,
  perUserLimit: (d.soLuotMoiNguoi ?? d.perUserLimit) ? Number(d.soLuotMoiNguoi ?? d.perUserLimit) : null,
  startAt: d.ngayBatDau ?? d.startAt ?? null,
  endAt: d.ngayKetThuc ?? d.endAt ?? null,
  status: d.dangHoatDong === false ? 0 : (d.status ?? 1),
});

const voucherService = {
  getAll: async (params) => {
    const res = await api.get('/vouchers', { params });
    const d = res.data;
    const items = (d.items || []).map(mapFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get(`/vouchers/${id}`);
    return { ...res, data: mapFromV2(res.data) };
  },
  create: (data) => api.post('/vouchers', toV2(data)),
  update: (id, data) => api.put(`/vouchers/${id}`, toV2(data)),
  delete: (id) => api.delete(`/vouchers/${id}`),
};

export default voucherService;
