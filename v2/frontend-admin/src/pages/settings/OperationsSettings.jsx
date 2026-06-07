import React, { useEffect, useState } from 'react';
import operationsService from '../../services/operationsService';
import { useAuth } from '../../contexts/AuthContext';

const DEFAULT_SETTINGS = [
  ['StoreName', 'Tên cửa hàng'],
  ['Hotline', 'Hotline'],
  ['Address', 'Địa chỉ'],
  ['DefaultLowStockThreshold', 'Ngưỡng tồn thấp mặc định'],
  ['DepositPolicy', 'Chính sách đặt cọc'],
  ['CancelPolicy', 'Chính sách hủy đơn'],
  ['WarrantyPolicy', 'Chính sách bảo hành'],
  ['DefaultShippingFee', 'Phí vận chuyển mặc định'],
];

const typeLabels = {
  StoreWarehouse: 'Cửa hàng kiêm kho',
  Showroom: 'Showroom',
  Warehouse: 'Kho',
};

const getApiMessage = (err, fallback) => err?.response?.data?.message || fallback;

const OperationsSettings = () => {
  const { isAdmin } = useAuth();
  const [warehouses, setWarehouses] = useState([]);
  const [settings, setSettings] = useState([]);
  const [warehouseForm, setWarehouseForm] = useState({ tenKho: '', loaiKho: 'StoreWarehouse', diaChi: '', hotline: '', dangHoatDong: true });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const canEdit = isAdmin();

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [warehousesRes, settingsRes] = await Promise.all([
        operationsService.getWarehouses(),
        operationsService.getSettings(),
      ]);
      setWarehouses(warehousesRes.data.items || []);
      const fromApi = settingsRes.data.items || [];
      const map = new Map(fromApi.map((item) => [item.key ?? item.Key, item]));
      setSettings(DEFAULT_SETTINGS.map(([key, label]) => ({
        key,
        label,
        value: map.get(key)?.value ?? map.get(key)?.Value ?? '',
        moTa: map.get(key)?.moTa ?? map.get(key)?.MoTa ?? label,
      })));
    } catch (err) {
      setError(getApiMessage(err, 'Không thể tải cấu hình vận hành.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const saveWarehouse = async () => {
    if (!warehouseForm.tenKho.trim()) {
      alert('Tên kho/showroom là bắt buộc.');
      return;
    }
    setSaving(true);
    try {
      await operationsService.saveWarehouse(warehouseForm);
      setWarehouseForm({ tenKho: '', loaiKho: 'StoreWarehouse', diaChi: '', hotline: '', dangHoatDong: true });
      await fetchData();
    } catch (err) {
      alert(getApiMessage(err, 'Không thể lưu kho/showroom.'));
    } finally {
      setSaving(false);
    }
  };

  const editWarehouse = (item) => {
    setWarehouseForm({
      maKho: item.maKho ?? item.MaKho,
      tenKho: item.tenKho ?? item.TenKho,
      loaiKho: item.loaiKho ?? item.LoaiKho,
      diaChi: item.diaChi ?? item.DiaChi ?? '',
      hotline: item.hotline ?? item.Hotline ?? '',
      dangHoatDong: item.dangHoatDong ?? item.DangHoatDong ?? true,
    });
  };

  const saveSettings = async () => {
    setSaving(true);
    try {
      await operationsService.saveSettings(settings.map((item) => ({ key: item.key, value: item.value, moTa: item.moTa })));
      await fetchData();
    } catch (err) {
      alert(getApiMessage(err, 'Không thể lưu cấu hình hệ thống.'));
    } finally {
      setSaving(false);
    }
  };

  const updateSetting = (index, value) => {
    setSettings((prev) => prev.map((item, i) => (i === index ? { ...item, value } : item)));
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <h1 className="m-0">Cấu hình vận hành</h1>
        </div>
      </div>
      <section className="content">
        <div className="container-fluid">
          {error && <div className="alert alert-danger">{error}</div>}
          {!canEdit && <div className="alert alert-info">Staff chỉ được xem cấu hình, chỉ Admin được chỉnh sửa.</div>}

          <div className="card">
            <div className="card-header"><h3 className="card-title">Showroom/Kho</h3></div>
            <div className="card-body">
              {canEdit && (
                <div className="row mb-3">
                  <div className="col-md-3"><input className="form-control" placeholder="Tên kho/showroom" value={warehouseForm.tenKho} onChange={(e) => setWarehouseForm((p) => ({ ...p, tenKho: e.target.value }))} /></div>
                  <div className="col-md-2">
                    <select className="form-control" value={warehouseForm.loaiKho} onChange={(e) => setWarehouseForm((p) => ({ ...p, loaiKho: e.target.value }))}>
                      {Object.entries(typeLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                    </select>
                  </div>
                  <div className="col-md-3"><input className="form-control" placeholder="Địa chỉ" value={warehouseForm.diaChi} onChange={(e) => setWarehouseForm((p) => ({ ...p, diaChi: e.target.value }))} /></div>
                  <div className="col-md-2"><input className="form-control" placeholder="Hotline" value={warehouseForm.hotline} onChange={(e) => setWarehouseForm((p) => ({ ...p, hotline: e.target.value }))} /></div>
                  <div className="col-md-2"><button className="btn btn-primary btn-block" onClick={saveWarehouse} disabled={saving}>Lưu kho</button></div>
                </div>
              )}
              <div className="table-responsive">
                <table className="table table-bordered table-striped">
                  <thead>
                    <tr>
                      <th className="table-col-text">Tên</th>
                      <th className="table-col-status">Loại</th>
                      <th className="table-col-text">Địa chỉ</th>
                      <th className="table-col-code">Hotline</th>
                      <th className="table-col-status">Trạng thái</th>
                      <th className="table-col-actions">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {loading ? (
                      <tr><td colSpan="6" className="text-center">Đang tải...</td></tr>
                    ) : warehouses.map((item) => (
                      <tr key={item.maKho ?? item.MaKho}>
                        <td className="table-col-text">{item.tenKho ?? item.TenKho}</td>
                        <td className="table-col-status">{typeLabels[item.loaiKho ?? item.LoaiKho] || item.loaiKho || item.LoaiKho}</td>
                        <td className="table-col-text">{item.diaChi ?? item.DiaChi ?? '-'}</td>
                        <td className="table-col-code">{item.hotline ?? item.Hotline ?? '-'}</td>
                        <td className="table-col-status">{item.dangHoatDong ?? item.DangHoatDong ? 'Đang hoạt động' : 'Ngừng hoạt động'}</td>
                        <td className="table-col-actions">{canEdit && <button className="btn btn-xs btn-info" onClick={() => editWarehouse(item)}><i className="fas fa-edit"></i></button>}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <div className="card">
            <div className="card-header"><h3 className="card-title">Cấu hình hệ thống</h3></div>
            <div className="card-body">
              <div className="row">
                {settings.map((item, index) => (
                  <div className="col-md-6" key={item.key}>
                    <div className="form-group">
                      <label>{item.label}</label>
                      <textarea className="form-control" rows="2" value={item.value || ''} disabled={!canEdit} onChange={(e) => updateSetting(index, e.target.value)} />
                    </div>
                  </div>
                ))}
              </div>
              {canEdit && <button className="btn btn-primary" onClick={saveSettings} disabled={saving}>Lưu cấu hình</button>}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default OperationsSettings;
