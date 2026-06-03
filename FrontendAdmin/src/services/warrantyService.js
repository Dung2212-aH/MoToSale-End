import api from './api';

const mapFromV2 = (w) => ({
  ...w,
  maBaoHanh: w.id,
  code: w.code,
  maDonHang: w.orderId,
  maBienSanPham: w.skuId,
  maKhachHang: w.customerId,
  tenSanPham: w.productSnapshot,
  soSerial: w.serialNumber,
  ngayBatDau: w.startAt,
  soThang: w.months,
  trangThai: w.warrantyStatus,
  ghiChu: w.note,
  ngayTao: w.createdDate,
});

const toV2 = (d) => ({
  orderId: d.maDonHang ?? d.orderId ?? null,
  skuId: d.maBienSanPham ?? d.skuId ?? null,
  customerId: d.maKhachHang ?? d.customerId ?? null,
  productSnapshot: d.tenSanPham ?? d.productSnapshot ?? '',
  serialNumber: d.soSerial ?? d.serialNumber ?? null,
  startAt: d.ngayBatDau ?? d.startAt ?? null,
  months: Number(d.soThang ?? d.months) || 0,
  note: d.ghiChu ?? d.note ?? null,
});

const warrantyService = {
  getAll: async (params) => {
    const res = await api.get('/warranties', { params });
    const d = res.data;
    const items = (d.items || []).map(mapFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get(`/warranties/${id}`);
    return { ...res, data: mapFromV2(res.data) };
  },
  create: (data) => api.post('/warranties', toV2(data)),
  updateStatus: (id, data) => api.patch(`/warranties/${id}/status`, { status: data.trangThai || data.status || data.Status }),
};

export default warrantyService;
