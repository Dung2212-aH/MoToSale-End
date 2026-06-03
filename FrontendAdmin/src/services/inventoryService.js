import api from './api';

let defaultStoreId = null;
const getDefaultStoreId = async () => {
  if (defaultStoreId) return defaultStoreId;
  try {
    const res = await api.get('/stores');
    const items = res.data.items || res.data || [];
    defaultStoreId = (items.find((s) => s.isDefault) || items[0])?.id ?? null;
  } catch { /* ignore */ }
  return defaultStoreId;
};

const docTypeToV2 = (t) => {
  const m = { Receipt: 1, Import: 1, Issue: 2, Export: 2, Adjustment: 3, Adjust: 3, Stocktake: 4, Transfer: 5 };
  return m[t] ?? Number(t) ?? 1;
};

const mapItem = (r) => ({
  ...r,
  maCuaHang: r.storeId,
  tenCuaHang: r.storeName,
  maBienSanPham: r.skuId,
  sku: r.skuCode,
  SKU: r.skuCode,
  tenSanPham: r.productName,
  tenBienThe: r.skuCode,
  tonKhoThucTe: r.onHand,
  soLuongDangGiu: r.reserved,
  tonKhoKhaDung: r.available,
  mucCanhBaoTonThap: r.reorderPoint,
  trangThaiTon: r.available <= 0 ? 'OutOfStock' : r.available <= r.reorderPoint ? 'LowStock' : 'InStock',
});

const mapHold = (h) => ({ ...h, maGiuCho: h.id, maDonHang: h.orderId, maDonHangKinhDoanh: h.orderCode, maBienSanPham: h.skuId, sku: h.skuCode, tenSanPham: h.productName, soLuong: h.qty, trangThai: h.status, hetHanLuc: h.expiresAt });
const mapMove = (m) => ({ ...m, maBienSanPham: m.skuId, soLuongThayDoi: m.qtyDelta, tonSau: m.balanceAfter, loaiGiaoDich: m.type, lyDo: m.reason, ngayTao: m.occurredAt });
const mapDoc = (d) => ({ ...d, maPhieuKho: d.id, maPhieu: d.code, loaiPhieu: d.type, trangThai: d.status, tenCuaHang: d.storeName, ghiChu: d.note, ngayTao: d.createdDate, soDong: d.lineCount });

const inventoryService = {
  getAll: async (params) => {
    const res = await api.get('/inventory', { params });
    const d = res.data;
    const items = (d.items || []).map(mapItem);
    // map summary v2 -> field FE cũ
    const s = d.summary || {};
    const summary = {
      ...s,
      tongSku: s.totalSkus, totalSkus: s.totalSkus,
      hetHang: s.outOfStock, outOfStock: s.outOfStock,
      sapHet: s.lowStock, lowStock: s.lowStock,
      dangGiuCho: s.holding, holding: s.holding,
      tongTon: s.totalOnHand, tongGiuCho: s.totalReserved,
    };
    return { ...res, data: { ...d, items, data: items, summary, lastSyncAt: d.lastSyncAt } };
  },
  sync: () => api.post('/inventory/sync'),
  getHolds: async (params) => {
    const res = await api.get('/inventory/holds', { params });
    return { ...res, data: { items: (res.data.items || []).map(mapHold) } };
  },
  getAdjustments: async (params) => {
    const res = await api.get('/inventory/adjustments', { params });
    return { ...res, data: { items: (res.data.items || []).map(mapMove) } };
  },
  getDocuments: async (params) => {
    const res = await api.get('/inventory/documents', { params });
    const d = res.data;
    const items = (d.items || []).map(mapDoc);
    return { ...res, data: { ...d, items, data: items } };
  },
  getDocumentById: async (id) => {
    const res = await api.get(`/inventory/documents/${id}`);
    const doc = res.data;
    return { ...res, data: { ...mapDoc(doc.document || doc), chiTiet: (doc.lines || []).map((l) => ({ ...l, maBienSanPham: l.skuId, sku: l.skuCode, tenSanPham: l.productName, soLuong: l.qty })) } };
  },
  createDocument: async (payload) => {
    const storeId = payload.maCuaHang ?? payload.storeId ?? (await getDefaultStoreId());
    const lines = (payload.items || payload.lines || payload.chiTiet || []).map((it) => ({ skuId: it.maBienSanPham ?? it.skuId ?? it.maSanPham, qty: Number(it.soLuong ?? it.qty) || 0, note: it.ghiChu ?? it.note ?? null }));
    return api.post('/inventory/documents', { type: docTypeToV2(payload.loaiPhieu ?? payload.type), storeId, toStoreId: payload.maCuaHangNhan ?? payload.toStoreId ?? null, note: payload.ghiChu ?? payload.note ?? null, lines });
  },
  approveDocument: (id) => api.post(`/inventory/documents/${id}/approve`),
  cancelDocument: (id) => api.post(`/inventory/documents/${id}/cancel`),
  updateThreshold: async (payload) => {
    const storeId = payload.maCuaHang ?? payload.storeId ?? (await getDefaultStoreId());
    return api.put('/inventory/threshold', { storeId, skuId: payload.maBienSanPham ?? payload.skuId ?? payload.maSanPham, reorderPoint: Number(payload.mucCanhBaoTonThap ?? payload.reorderPoint) || 0 });
  },
  adjustStock: async (payload) => {
    const storeId = payload.maCuaHang ?? payload.storeId ?? (await getDefaultStoreId());
    return api.post('/inventory/adjust', { storeId, skuId: payload.maBienSanPham ?? payload.skuId ?? payload.maSanPham, transactionType: payload.loaiGiaoDich ?? payload.transactionType ?? 'Import', qty: Number(payload.soLuong ?? payload.qty) || 0, reason: payload.lyDo ?? payload.reason ?? '' });
  },
  exportCsv: (params) => api.get('/inventory/export', { params, responseType: 'blob' }),
};

export default inventoryService;
