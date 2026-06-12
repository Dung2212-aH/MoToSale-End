import React, { useEffect, useState } from 'react';
import operationsService from '../../services/operationsService';
import { useAuth } from '../../contexts/AuthContext';

const DEFAULT_SETTINGS = [
  ['DefaultLowStockThreshold', 'Ngưỡng tồn thấp mặc định'],
  ['DepositPolicy', 'Chính sách đặt cọc'],
  ['CancelPolicy', 'Chính sách hủy đơn'],
  ['WarrantyPolicy', 'Chính sách bảo hành'],
  ['DefaultShippingFee', 'Phí vận chuyển mặc định'],
  ['InstallmentAnnualRate', 'Lãi suất trả góp/năm (%)'],
  ['InstallmentMinDownPaymentPercent', 'Tỷ lệ trả trước tối thiểu khi trả góp (%)'],
  ['InstallmentAllowedTerms', 'Các kỳ hạn trả góp cho phép (tháng, cách nhau dấu phẩy)'],
  ['PaymentHoldMinutes', 'Thời gian giữ chỗ tồn kho cho thanh toán (phút)'],
  ['DepositMinPercent', 'Tỷ lệ đặt cọc tối thiểu cho đơn Đặt cọc (%)'],
];

const getApiMessage = (err, fallback) => err?.response?.data?.message || fallback;

const OperationsSettings = () => {
  const { isAdmin } = useAuth();
  const [settings, setSettings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const canEdit = isAdmin();

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const settingsRes = await operationsService.getSettings();
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
            <div className="card-header"><h3 className="card-title">Cấu hình hệ thống</h3></div>
            <div className="card-body">
              {loading ? (
                <div className="text-center">Đang tải...</div>
              ) : (
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
              )}
              {canEdit && <button className="btn btn-primary" onClick={saveSettings} disabled={saving || loading}>Lưu cấu hình</button>}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default OperationsSettings;
