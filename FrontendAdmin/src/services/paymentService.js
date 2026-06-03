import api from './api';

const mapFromV2 = (p) => ({
  ...p,
  maThanhToan: p.id,
  maThanhToanKinhDoanh: p.code,
  maDonHang: p.orderId,
  maDonHangKinhDoanh: p.orderCode,
  loaiThanhToan: p.paymentType,
  soTien: p.amount,
  phuongThuc: p.method,
  trangThai: p.status,
  daThanhToanLuc: p.paidAt,
  ngayTao: p.createdDate ?? p.paidAt,
});

const paymentService = {
  getAll: async (params) => {
    const res = await api.get('/payments', { params });
    const d = res.data;
    const items = (d.items || []).map(mapFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getByOrder: async (orderId) => {
    const res = await api.get(`/payments/order/${orderId}`);
    return { ...res, data: (res.data.items || []).map(mapFromV2) };
  },
  // v2 ghi nhận thanh toán thủ công (= đã thu ngay)
  create: (data) => api.post('/payments', {
    orderId: data.maDonHang ?? data.orderId,
    paymentType: data.loaiThanhToan ?? data.paymentType ?? 'Full',
    amount: Number(data.soTien ?? data.amount) || 0,
    method: data.phuongThuc ?? data.method ?? 'Cash',
    transactionRef: data.maGiaoDich ?? data.transactionRef ?? null,
    note: data.ghiChu ?? data.note ?? null,
  }),
  // v2 không có bước "confirm" riêng (ghi nhận = đã thu) → no-op
  confirm: () => Promise.resolve({ data: { message: 'Đã ghi nhận.' } }),
  cancel: (id, data) => api.post(`/payments/${id}/cancel`, { reason: data?.lyDoHuy || data?.reason || null }),
};

export default paymentService;
