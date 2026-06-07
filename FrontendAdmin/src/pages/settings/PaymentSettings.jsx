import React, { useEffect, useMemo, useState } from 'react';
import operationsService from '../../services/operationsService';
import { useAuth } from '../../contexts/AuthContext';

// Common Vietnamese banks (VietQR BIN codes). The BIN is what img.vietqr.io expects.
const BANKS = [
  { bin: '970436', shortName: 'VCB', name: 'Vietcombank' },
  { bin: '970422', shortName: 'MBB', name: 'MB Bank' },
  { bin: '970418', shortName: 'BIDV', name: 'BIDV' },
  { bin: '970415', shortName: 'VTB', name: 'VietinBank' },
  { bin: '970405', shortName: 'AGB', name: 'Agribank' },
  { bin: '970407', shortName: 'TCB', name: 'Techcombank' },
  { bin: '970432', shortName: 'VPB', name: 'VPBank' },
  { bin: '970416', shortName: 'ACB', name: 'ACB' },
  { bin: '970423', shortName: 'TPB', name: 'TPBank' },
  { bin: '970403', shortName: 'STB', name: 'Sacombank' },
  { bin: '970437', shortName: 'HDB', name: 'HDBank' },
  { bin: '970448', shortName: 'OCB', name: 'OCB' },
  { bin: '970454', shortName: 'VCCB', name: 'VietCapital Bank' },
  { bin: '970441', shortName: 'VIB', name: 'VIB' },
  { bin: '970443', shortName: 'SHB', name: 'SHB' },
  { bin: '970426', shortName: 'MSB', name: 'Maritime Bank' },
];

const KEYS = ['BankBin', 'BankAccountNo', 'BankAccountName'];
const getApiMessage = (err, fallback) => err?.response?.data?.message || fallback;

const PaymentSettings = () => {
  const { isAdmin } = useAuth();
  const canEdit = isAdmin();

  const [bin, setBin] = useState('');
  const [accountNo, setAccountNo] = useState('');
  const [accountName, setAccountName] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    const fetchSettings = async () => {
      setLoading(true);
      setError('');
      try {
        const res = await operationsService.getSettings();
        const items = res.data.items || [];
        const map = new Map(items.map((it) => [it.key ?? it.Key, it.value ?? it.Value ?? '']));
        setBin(map.get('BankBin') || '');
        setAccountNo(map.get('BankAccountNo') || '');
        setAccountName(map.get('BankAccountName') || '');
      } catch (err) {
        setError(getApiMessage(err, 'Không tải được cấu hình hiện tại.'));
      } finally {
        setLoading(false);
      }
    };
    fetchSettings();
  }, []);

  const selectedBank = useMemo(() => BANKS.find((b) => b.bin === bin), [bin]);
  const isValid = bin.trim() && /^\d{6,20}$/.test(accountNo.trim()) && accountName.trim().length > 0;

  // Preview QR shows what customers will see — uses sample amount/content so admin can verify the
  // bank/account/name resolves correctly via VietQR before saving.
  const previewQrUrl = useMemo(() => {
    if (!isValid) return '';
    const params = new URLSearchParams({
      amount: '1000000',
      addInfo: 'PREVIEW-DON-HANG',
      accountName: accountName.trim(),
    });
    return `https://img.vietqr.io/image/${bin.trim()}-${accountNo.trim()}-compact2.png?${params.toString()}`;
  }, [bin, accountNo, accountName, isValid]);

  const handleSave = async () => {
    setError('');
    setSuccess('');
    if (!isValid) {
      setError('Vui lòng chọn ngân hàng, nhập số tài khoản và tên chủ tài khoản.');
      return;
    }
    setSaving(true);
    try {
      const items = [
        { key: 'BankBin', value: bin.trim(), moTa: 'Ma ngan hang / BIN nhan chuyen khoan (VietQR)' },
        { key: 'BankAccountNo', value: accountNo.trim(), moTa: 'So tai khoan nhan chuyen khoan' },
        { key: 'BankAccountName', value: accountName.trim().toUpperCase(), moTa: 'Ten chu tai khoan' },
      ];
      await operationsService.saveSettings(items);
      setSuccess('Đã lưu thông tin tài khoản nhận chuyển khoản.');
    } catch (err) {
      setError(getApiMessage(err, 'Không lưu được cấu hình.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <h1 className="m-0">Cấu hình thanh toán</h1>
          <p className="text-muted mb-0 mt-1">
            Tài khoản dưới đây sẽ được dùng để sinh mã QR (VietQR) cho mọi đơn hàng chờ thanh toán của khách.
          </p>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {error && <div className="alert alert-danger">{error}</div>}
          {success && <div className="alert alert-success">{success}</div>}
          {!canEdit && <div className="alert alert-info">Chỉ Admin được chỉnh sửa cấu hình thanh toán. Staff chỉ xem.</div>}

          <div className="row">
            {/* Form */}
            <div className="col-lg-7">
              <div className="card">
                <div className="card-header"><h3 className="card-title">Tài khoản nhận chuyển khoản</h3></div>
                <div className="card-body">
                  {loading ? (
                    <div className="text-center py-3">Đang tải...</div>
                  ) : (
                    <>
                      <div className="form-group">
                        <label>Ngân hàng <span className="text-danger">*</span></label>
                        <select
                          className="form-control"
                          value={bin}
                          onChange={(e) => setBin(e.target.value)}
                          disabled={!canEdit}
                        >
                          <option value="">-- Chọn ngân hàng --</option>
                          {BANKS.map((b) => (
                            <option key={b.bin} value={b.bin}>{b.name} ({b.shortName})</option>
                          ))}
                        </select>
                        <small className="form-text text-muted">
                          Nếu ngân hàng của bạn không có trong danh sách, nhập mã BIN VietQR thủ công vào ô bên dưới.
                        </small>
                        <input
                          type="text"
                          className="form-control mt-2"
                          placeholder="Hoặc nhập mã BIN (vd: 970436)"
                          value={bin}
                          onChange={(e) => setBin(e.target.value.trim())}
                          disabled={!canEdit}
                          maxLength={20}
                        />
                      </div>

                      <div className="form-group">
                        <label>Số tài khoản <span className="text-danger">*</span></label>
                        <input
                          type="text"
                          className="form-control"
                          placeholder="Vd: 0123456789"
                          value={accountNo}
                          onChange={(e) => setAccountNo(e.target.value.replace(/[^\d]/g, ''))}
                          disabled={!canEdit}
                          maxLength={20}
                        />
                      </div>

                      <div className="form-group">
                        <label>Tên chủ tài khoản <span className="text-danger">*</span></label>
                        <input
                          type="text"
                          className="form-control text-uppercase"
                          placeholder="Vd: NGUYEN VAN A"
                          value={accountName}
                          onChange={(e) => setAccountName(e.target.value)}
                          disabled={!canEdit}
                          maxLength={150}
                        />
                        <small className="form-text text-muted">Không dấu, đúng tên trên thẻ ngân hàng.</small>
                      </div>

                      {canEdit && (
                        <button
                          className="btn btn-primary"
                          onClick={handleSave}
                          disabled={saving || !isValid}
                        >
                          <i className="fas fa-save"></i> {saving ? 'Đang lưu...' : 'Lưu cấu hình'}
                        </button>
                      )}
                    </>
                  )}
                </div>
              </div>
            </div>

            {/* Preview */}
            <div className="col-lg-5">
              <div className="card card-outline card-info">
                <div className="card-header"><h3 className="card-title">Xem trước mã QR</h3></div>
                <div className="card-body text-center">
                  {previewQrUrl ? (
                    <>
                      <img
                        src={previewQrUrl}
                        alt="Preview VietQR"
                        className="img-fluid border rounded"
                        style={{ maxWidth: 280 }}
                      />
                      <div className="mt-3 text-left">
                        <p className="mb-1"><strong>Ngân hàng:</strong> {selectedBank?.name || bin}</p>
                        <p className="mb-1"><strong>Số tài khoản:</strong> {accountNo}</p>
                        <p className="mb-1"><strong>Chủ TK:</strong> {accountName.toUpperCase()}</p>
                        <p className="text-muted small mb-0 mt-2">
                          Đây là QR mẫu với số tiền 1.000.000 đ và nội dung <code>PREVIEW-DON-HANG</code>. Khi khách đặt
                          hàng thật, hệ thống tự sinh QR với đúng số tiền và mã đơn của họ.
                        </p>
                      </div>
                    </>
                  ) : (
                    <div className="text-muted py-5">
                      <i className="fas fa-qrcode fa-3x mb-3 d-block"></i>
                      Điền đầy đủ ngân hàng / số tài khoản / chủ TK ở bên trái để xem trước QR.
                    </div>
                  )}
                </div>
              </div>

              <div className="callout callout-info mt-3">
                <h5><i className="fas fa-info-circle"></i> Lưu ý</h5>
                <ul className="mb-0 pl-3">
                  <li>QR được sinh tự động bằng dịch vụ <strong>VietQR</strong> (không tốn phí).</li>
                  <li>Khách quét QR sẽ chuyển khoản đúng số tiền & nội dung (= mã đơn).</li>
                  <li>Sau khi nhận được tiền, vào chi tiết đơn để bấm <em>"Đã nhận thanh toán"</em>.</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default PaymentSettings;
