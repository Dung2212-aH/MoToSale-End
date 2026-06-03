import api from './api';

const mapWarehouse = (w) => ({
  ...w,
  MaKho: w.maKho ?? w.MaKho,
  TenKho: w.tenKho ?? w.TenKho,
  LoaiKho: w.loaiKho ?? w.LoaiKho,
  DiaChi: w.diaChi ?? w.DiaChi,
  Hotline: w.hotline ?? w.Hotline,
  DangHoatDong: w.dangHoatDong ?? w.DangHoatDong,
});

const mapSetting = (s) => ({
  ...s,
  Key: s.key ?? s.Key,
  Value: s.value ?? s.Value,
  MoTa: s.moTa ?? s.MoTa,
});

const operationsService = {
  getWarehouses: async () => {
    const res = await api.get('/operations/warehouses');
    return { ...res, data: { items: (res.data.items || []).map(mapWarehouse) } };
  },
  saveWarehouse: (data) => api.post('/operations/warehouses', data),
  getSettings: async () => {
    const res = await api.get('/operations/settings');
    return { ...res, data: { items: (res.data.items || []).map(mapSetting) } };
  },
  saveSettings: (items) => api.put('/operations/settings', { items }),
};

export default operationsService;
