import React, { useEffect, useState } from 'react';
import operationsService from '../../services/operationsService';
import { useAuth } from '../../contexts/AuthContext';

const DEFAULT_SETTINGS = [
  ['DefaultLowStockThreshold', 'Nguong ton thap mac dinh'],
  ['DepositPolicy', 'Chinh sach dat coc'],
  ['CancelPolicy', 'Chinh sach huy don'],
  ['WarrantyPolicy', 'Chinh sach bao hanh'],
  ['DefaultShippingFee', 'Phi van chuyen mac dinh'],
  // Tài khoản nhận chuyển khoản (BankBin/BankAccountNo/BankAccountName) cấu hình ở trang riêng /settings/payment.
  ['InstallmentAnnualRate', 'Lai suat tra gop/nam (%)'],
  ['InstallmentMinDownPaymentPercent', 'Ty le tra truoc toi thieu khi tra gop (%)'],
  ['InstallmentAllowedTerms', 'Cac ky han tra gop cho phep (thang, cach nhau dau phay)'],
  ['PaymentHoldMinutes', 'Thoi gian giu cho ton kho cho thanh toan (phut)'],
  ['DepositMinPercent', 'Ty le dat coc toi thieu cho don Dat coc (%)'],
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
      setError(getApiMessage(err, 'Khong the tai cau hinh van hanh.'));
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
      alert(getApiMessage(err, 'Khong the luu cau hinh he thong.'));
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
          <h1 className="m-0">Cau hinh van hanh</h1>
        </div>
      </div>
      <section className="content">
        <div className="container-fluid">
          {error && <div className="alert alert-danger">{error}</div>}
          {!canEdit && <div className="alert alert-info">Staff chi duoc xem cau hinh, chi Admin duoc chinh sua.</div>}

          <div className="card">
            <div className="card-header"><h3 className="card-title">Cau hinh he thong</h3></div>
            <div className="card-body">
              {loading ? (
                <div className="text-center">Dang tai...</div>
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
              {canEdit && <button className="btn btn-primary" onClick={saveSettings} disabled={saving || loading}>Luu cau hinh</button>}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default OperationsSettings;
