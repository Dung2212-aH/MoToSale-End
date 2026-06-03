import api from './api';

const mapLineFromV2 = (l) => ({
  ...l,
  maChiTietDonHang: l.id,
  maSanPham: l.skuId,
  tenSanPhamSnapshot: l.productName,
  tenSanPham: l.productName,
  skuSnapshot: l.skuCode,
  donGia: l.unitPrice,
  soLuong: l.qty,
  thanhTien: l.lineTotal,
  phanPhoi: l.allocations,
});

const mapOrderFromV2 = (o) => {
  if (!o) return o;
  return {
    ...o,
    maDonHang: o.id,
    maDonHangKinhDoanh: o.code,
    maNguoiDung: o.userId,
    tenKhachHang: o.customerName ?? o.shippingRecipient,
    hoTen: o.customerName ?? o.shippingRecipient,
    trangThaiDonHang: o.orderStatus,
    trangThaiThanhToan: o.paymentStatus,
    trangThaiVanChuyen: o.fulfillmentStatus,
    loaiDonHang: o.orderType,
    tongTienHang: o.subtotal,
    tienGiam: o.discountTotal,
    phiVanChuyen: o.shippingFee,
    tongThanhToan: o.grandTotal,
    tienDatCoc: o.depositAmount,
    soTienConLai: o.remainingAmount,
    hoTenNhanHang: o.shippingRecipient,
    soDienThoaiNhanHang: o.shippingPhone,
    emailNhanHang: o.shippingEmail,
    diaChiNhanHang: o.shippingAddress,
    phuongThucNhanHang: o.receivingMethod,
    ghiChu: o.note,
    ngayTao: o.placedAt ?? o.createdDate,
    items: (o.lines || []).map(mapLineFromV2),
    chiTiet: (o.lines || []).map(mapLineFromV2),
  };
};

const orderService = {
  getAll: async (params) => {
    const res = await api.get('/orders', { params });
    const d = res.data;
    const items = (d.items || []).map(mapOrderFromV2);
    return { ...res, data: { ...d, items, data: items } };
  },
  getById: async (id) => {
    const res = await api.get(`/orders/${id}`);
    return { ...res, data: mapOrderFromV2(res.data) };
  },
  updateStatus: (id, data) => api.put(`/orders/${id}/status`, {
    toStatus: data.toStatus || data.trangThaiDonHang || data.status,
    note: data.note || data.ghiChu || null,
  }),
  cancel: (id, data) => api.post(`/orders/${id}/cancel`, { reason: data?.lyDoHuyDon || data?.reason || null }),

  // v2-only: phân phối đơn về cửa hàng
  allocationSuggestion: (id) => api.get(`/orders/${id}/allocation-suggestion`),
  allocate: (id, allocations) => api.post(`/orders/${id}/allocate`, { allocations }),
};

export default orderService;
