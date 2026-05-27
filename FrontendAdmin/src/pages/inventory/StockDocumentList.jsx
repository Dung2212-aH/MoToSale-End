import React, { useEffect, useMemo, useState } from 'react';
import inventoryService from '../../services/inventoryService';
import productService from '../../services/productService';
import { formatDate } from '../../utils/formatDate';
import { createDateStamp, exportWorkbook } from '../../utils/exportExcel';

const TYPES = {
  Import: 'Phiếu nhập kho',
  Export: 'Phiếu xuất kho',
  Adjust: 'Phiếu điều chỉnh tồn',
};

const STATUS = {
  Draft: { label: 'Nháp', color: 'secondary' },
  Approved: { label: 'Đã duyệt', color: 'success' },
  Cancelled: { label: 'Đã hủy', color: 'danger' },
};

const getApiMessage = (err, fallback) => err?.response?.data?.message || fallback;
const productIdOf = (item) => item.maSanPham ?? item.id ?? item.MaSanPham;
const productNameOf = (item) => item.tenSanPham ?? item.name ?? item.TenSanPham ?? '';
const productCodeOf = (item) => item.maSanPhamKinhDoanh ?? item.code ?? item.MaSanPhamKinhDoanh ?? productIdOf(item);
const variantIdOf = (item) => item.maBienSanPham ?? item.id ?? item.MaBienSanPham;
const variantNameOf = (item) => item.tenBienThe ?? item.name ?? item.TenBienThe ?? item.sku ?? item.SKU ?? '';

const emptyLine = { maSanPham: '', maBienSanPham: '', soLuong: 1, ghiChu: '' };

const StockDocumentList = () => {
  const [documents, setDocuments] = useState([]);
  const [selectedDocument, setSelectedDocument] = useState(null);
  const [details, setDetails] = useState([]);
  const [products, setProducts] = useState([]);
  const [variantsByProduct, setVariantsByProduct] = useState({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [error, setError] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [filterType, setFilterType] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [form, setForm] = useState({ loaiPhieu: 'Import', ghiChu: '', items: [emptyLine] });

  const fetchDocuments = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await inventoryService.getDocuments({
        status: filterStatus || undefined,
        type: filterType || undefined,
        pageSize: 100,
      });
      setDocuments(res.data.items || []);
    } catch (err) {
      setError(getApiMessage(err, 'Không thể tải danh sách phiếu kho.'));
    } finally {
      setLoading(false);
    }
  };

  const fetchProducts = async () => {
    try {
      const res = await productService.getAll({ page: 1, pageSize: 500 });
      const data = res.data;
      setProducts(data.items || data.data || data || []);
    } catch (err) {
      setProducts([]);
    }
  };

  useEffect(() => {
    fetchDocuments();
  }, [filterStatus, filterType]);

  useEffect(() => {
    fetchProducts();
  }, []);

  const loadVariants = async (productId) => {
    if (!productId || variantsByProduct[productId]) return;
    try {
      const res = await productService.getVariants(productId);
      setVariantsByProduct((prev) => ({ ...prev, [productId]: res.data || [] }));
    } catch (err) {
      setVariantsByProduct((prev) => ({ ...prev, [productId]: [] }));
    }
  };

  const openCreate = () => {
    setForm({ loaiPhieu: 'Import', ghiChu: '', items: [{ ...emptyLine }] });
    setShowCreate(true);
  };

  const updateLine = async (index, field, value) => {
    const next = form.items.map((item, i) => (i === index ? { ...item, [field]: value } : item));
    if (field === 'maSanPham') {
      next[index].maBienSanPham = '';
      await loadVariants(value);
    }
    setForm((prev) => ({ ...prev, items: next }));
  };

  const addLine = () => setForm((prev) => ({ ...prev, items: [...prev.items, { ...emptyLine }] }));
  const removeLine = (index) => setForm((prev) => ({ ...prev, items: prev.items.filter((_, i) => i !== index) }));

  const saveDocument = async () => {
    const items = form.items
      .filter((item) => item.maSanPham && Number(item.soLuong) > 0)
      .map((item) => ({
        maSanPham: Number(item.maSanPham),
        maBienSanPham: item.maBienSanPham ? Number(item.maBienSanPham) : null,
        soLuong: Number(item.soLuong),
        ghiChu: item.ghiChu,
      }));

    if (items.length === 0) {
      alert('Phiếu kho phải có ít nhất một dòng hàng hợp lệ.');
      return;
    }

    setSaving(true);
    try {
      await inventoryService.createDocument({ loaiPhieu: form.loaiPhieu, ghiChu: form.ghiChu, items });
      setShowCreate(false);
      await fetchDocuments();
    } catch (err) {
      alert(getApiMessage(err, 'Không thể tạo phiếu kho.'));
    } finally {
      setSaving(false);
    }
  };

  const openDetail = async (document) => {
    setSelectedDocument(document);
    setShowDetail(true);
    setDetails([]);
    try {
      const res = await inventoryService.getDocumentById(document.maPhieuKho ?? document.MaPhieuKho);
      setSelectedDocument(res.data.document || document);
      setDetails(res.data.details || []);
    } catch (err) {
      alert(getApiMessage(err, 'Không thể tải chi tiết phiếu kho.'));
    }
  };

  const approveDocument = async () => {
    if (!selectedDocument) return;
    if (!window.confirm('Duyệt phiếu kho này? Sau khi duyệt, tồn kho sẽ được cập nhật và không thể sửa phiếu.')) return;
    setSaving(true);
    try {
      await inventoryService.approveDocument(selectedDocument.maPhieuKho ?? selectedDocument.MaPhieuKho);
      await fetchDocuments();
      await openDetail(selectedDocument);
    } catch (err) {
      alert(getApiMessage(err, 'Duyệt phiếu kho thất bại.'));
    } finally {
      setSaving(false);
    }
  };

  const cancelDocument = async () => {
    if (!selectedDocument) return;
    const reason = window.prompt('Lý do hủy phiếu kho?') || '';
    setSaving(true);
    try {
      await inventoryService.cancelDocument(selectedDocument.maPhieuKho ?? selectedDocument.MaPhieuKho, { lyDo: reason });
      await fetchDocuments();
      await openDetail(selectedDocument);
    } catch (err) {
      alert(getApiMessage(err, 'Hủy phiếu kho thất bại.'));
    } finally {
      setSaving(false);
    }
  };

  const printSelectedDocument = () => {
    if (!selectedDocument) return;
    const code = selectedDocument.maPhieu ?? selectedDocument.MaPhieu;
    const type = TYPES[selectedDocument.loaiPhieu ?? selectedDocument.LoaiPhieu] || selectedDocument.loaiPhieu || selectedDocument.LoaiPhieu;
    const status = STATUS[selectedDocument.trangThai ?? selectedDocument.TrangThai]?.label || selectedDocument.trangThai || selectedDocument.TrangThai;
    const rows = details.map((detail, index) => `
      <tr>
        <td>${index + 1}</td>
        <td>${detail.maSanPhamKinhDoanh ?? detail.MaSanPhamKinhDoanh}</td>
        <td>${detail.sku ?? detail.SKU ?? '-'}</td>
        <td>${detail.tenSanPham ?? detail.TenSanPham} ${detail.tenBienThe || detail.TenBienThe ? `- ${detail.tenBienThe ?? detail.TenBienThe}` : ''}</td>
        <td class="right">${detail.tonTruoc ?? detail.TonTruoc}</td>
        <td class="right">${detail.soLuongThayDoi ?? detail.SoLuongThayDoi}</td>
        <td class="right">${detail.tonSau ?? detail.TonSau}</td>
      </tr>
    `).join('');

    const printWindow = window.open('', '_blank', 'width=900,height=700');
    if (!printWindow) return;
    printWindow.document.write(`
      <html>
        <head>
          <title>${type} ${code}</title>
          <style>
            body { font-family: Arial, sans-serif; color: #222; padding: 24px; }
            h1 { font-size: 22px; margin: 0 0 6px; }
            .muted { color: #666; margin-bottom: 16px; }
            .meta { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 24px; margin: 16px 0; }
            table { width: 100%; border-collapse: collapse; margin-top: 12px; }
            th, td { border: 1px solid #ddd; padding: 8px; font-size: 13px; text-align: left; }
            th { background: #f3f4f6; }
            .right { text-align: right; }
            .signatures { display: grid; grid-template-columns: 1fr 1fr; gap: 60px; margin-top: 42px; text-align: center; }
            @media print { body { padding: 0; } }
          </style>
        </head>
        <body>
          <h1>MoToSale - ${type}</h1>
          <div class="muted">Mã phiếu: ${code}</div>
          <div class="meta">
            <div>Trạng thái: ${status}</div>
            <div>Ngày tạo: ${formatDate(selectedDocument.ngayTao ?? selectedDocument.NgayTao)}</div>
            <div>Người tạo: ${selectedDocument.maNguoiTao ?? selectedDocument.MaNguoiTao ?? '-'}</div>
            <div>Ngày duyệt: ${formatDate(selectedDocument.ngayDuyet ?? selectedDocument.NgayDuyet)}</div>
            <div>Ghi chú: ${selectedDocument.ghiChu ?? selectedDocument.GhiChu ?? '-'}</div>
          </div>
          <table>
            <thead>
              <tr><th>#</th><th>Mã SP</th><th>SKU</th><th>Sản phẩm</th><th class="right">Tồn trước</th><th class="right">Thay đổi</th><th class="right">Tồn sau</th></tr>
            </thead>
            <tbody>${rows || '<tr><td colspan="7">Chưa có dòng hàng</td></tr>'}</tbody>
          </table>
          <div class="signatures">
            <div>Người lập phiếu<br><br><br>........................</div>
            <div>Người duyệt/nhận<br><br><br>........................</div>
          </div>
          <script>window.onload = () => { window.print(); };</script>
        </body>
      </html>
    `);
    printWindow.document.close();
  };

  const exportDocuments = async () => {
    setExporting(true);
    try {
      await exportWorkbook({
        fileName: `phieu-kho-${createDateStamp()}.xlsx`,
        sheets: [
          {
            name: 'PhieuKho',
            columns: [
              { header: 'Mã phiếu', key: 'code', width: 18 },
              { header: 'Loại phiếu', key: 'type', width: 22 },
              { header: 'Trạng thái', key: 'status', width: 14 },
              { header: 'Số dòng', key: 'lines', type: 'number', width: 12 },
              { header: 'Tổng số lượng', key: 'quantity', type: 'number', width: 16 },
              { header: 'Người tạo', key: 'creator', width: 14 },
              { header: 'Ngày tạo', key: 'createdAt', type: 'date', width: 20 },
              { header: 'Người duyệt', key: 'approver', width: 14 },
              { header: 'Ngày duyệt', key: 'approvedAt', type: 'date', width: 20 },
              { header: 'Ghi chú', key: 'note', width: 40 },
            ],
            rows: documents.map((item) => ({
              code: item.maPhieu ?? item.MaPhieu,
              type: TYPES[item.loaiPhieu ?? item.LoaiPhieu] || item.loaiPhieu || item.LoaiPhieu,
              status: STATUS[item.trangThai ?? item.TrangThai]?.label || item.trangThai || item.TrangThai,
              lines: item.soDong ?? item.SoDong ?? 0,
              quantity: item.tongSoLuong ?? item.TongSoLuong ?? 0,
              creator: item.maNguoiTao ?? item.MaNguoiTao,
              createdAt: item.ngayTao ?? item.NgayTao,
              approver: item.maNguoiDuyet ?? item.MaNguoiDuyet,
              approvedAt: item.ngayDuyet ?? item.NgayDuyet,
              note: item.ghiChu ?? item.GhiChu ?? '',
            })),
          },
        ],
      });
    } catch (err) {
      alert('Xuất Excel phiếu kho thất bại.');
    } finally {
      setExporting(false);
    }
  };

  const statusBadge = (status) => {
    const meta = STATUS[status] || STATUS.Draft;
    return <span className={`badge badge-${meta.color}`}>{meta.label}</span>;
  };

  const canApproveOrCancel = useMemo(() => {
    const status = selectedDocument?.trangThai ?? selectedDocument?.TrangThai;
    return status === 'Draft';
  }, [selectedDocument]);

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Phiếu kho</h1>
            </div>
            <div className="col-sm-6 text-right">
              <button className="btn btn-outline-success mr-2" onClick={exportDocuments} disabled={exporting}>
                <i className="fas fa-file-excel mr-1"></i>
                {exporting ? 'Đang xuất...' : 'Xuất Excel'}
              </button>
              <button className="btn btn-primary" onClick={openCreate}>
                <i className="fas fa-plus mr-1"></i>
                Tạo phiếu kho
              </button>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          {error && <div className="alert alert-danger">{error}</div>}
          <div className="card">
            <div className="card-body">
              <div className="row">
                <div className="col-md-3">
                  <select className="form-control" value={filterType} onChange={(e) => setFilterType(e.target.value)}>
                    <option value="">Tất cả loại phiếu</option>
                    {Object.entries(TYPES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                  </select>
                </div>
                <div className="col-md-3">
                  <select className="form-control" value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}>
                    <option value="">Tất cả trạng thái</option>
                    {Object.entries(STATUS).map(([value, meta]) => <option key={value} value={value}>{meta.label}</option>)}
                  </select>
                </div>
              </div>
            </div>
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-bordered table-striped mb-0">
                  <thead>
                    <tr>
                      <th>Mã phiếu</th>
                      <th>Loại phiếu</th>
                      <th className="text-center">Số dòng</th>
                      <th className="text-right">Tổng SL</th>
                      <th>Trạng thái</th>
                      <th>Ngày tạo</th>
                      <th>Ngày duyệt</th>
                      <th>Ghi chú</th>
                      <th className="text-center">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {loading ? (
                      <tr><td colSpan="9" className="text-center py-4">Đang tải phiếu kho...</td></tr>
                    ) : documents.length === 0 ? (
                      <tr><td colSpan="9" className="text-center text-muted py-4">Chưa có phiếu kho.</td></tr>
                    ) : documents.map((item) => {
                      const status = item.trangThai ?? item.TrangThai;
                      return (
                        <tr key={item.maPhieuKho ?? item.MaPhieuKho}>
                          <td><strong>{item.maPhieu ?? item.MaPhieu}</strong></td>
                          <td>{TYPES[item.loaiPhieu ?? item.LoaiPhieu] || item.loaiPhieu || item.LoaiPhieu}</td>
                          <td className="text-center">{item.soDong ?? item.SoDong}</td>
                          <td className="text-right">{item.tongSoLuong ?? item.TongSoLuong}</td>
                          <td>{statusBadge(status)}</td>
                          <td>{formatDate(item.ngayTao ?? item.NgayTao)}</td>
                          <td>{formatDate(item.ngayDuyet ?? item.NgayDuyet)}</td>
                          <td>{item.ghiChu ?? item.GhiChu ?? '-'}</td>
                          <td className="text-center">
                            <button className="btn btn-xs btn-info" onClick={() => openDetail(item)} title="Xem chi tiết">
                              <i className="fas fa-eye"></i>
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </section>

      {showCreate && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-xl">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Tạo phiếu kho</h5>
                <button type="button" className="close" onClick={() => setShowCreate(false)}><span>&times;</span></button>
              </div>
              <div className="modal-body">
                <div className="row">
                  <div className="col-md-4">
                    <div className="form-group">
                      <label>Loại phiếu</label>
                      <select className="form-control" value={form.loaiPhieu} onChange={(e) => setForm((prev) => ({ ...prev, loaiPhieu: e.target.value }))}>
                        {Object.entries(TYPES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                      </select>
                    </div>
                  </div>
                  <div className="col-md-8">
                    <div className="form-group">
                      <label>Ghi chú</label>
                      <input className="form-control" value={form.ghiChu} onChange={(e) => setForm((prev) => ({ ...prev, ghiChu: e.target.value }))} />
                    </div>
                  </div>
                </div>

                <div className="table-responsive">
                  <table className="table table-bordered">
                    <thead>
                      <tr>
                        <th style={{ minWidth: 260 }}>Sản phẩm</th>
                        <th style={{ minWidth: 220 }}>Biến thể/SKU</th>
                        <th style={{ width: 140 }}>Số lượng</th>
                        <th>Ghi chú dòng</th>
                        <th style={{ width: 70 }}></th>
                      </tr>
                    </thead>
                    <tbody>
                      {form.items.map((line, index) => {
                        const variants = variantsByProduct[line.maSanPham] || [];
                        return (
                          <tr key={index}>
                            <td>
                              <select className="form-control" value={line.maSanPham} onChange={(e) => updateLine(index, 'maSanPham', e.target.value)}>
                                <option value="">-- Chọn sản phẩm --</option>
                                {products.map((p) => (
                                  <option key={productIdOf(p)} value={productIdOf(p)}>
                                    {productCodeOf(p)} - {productNameOf(p)}
                                  </option>
                                ))}
                              </select>
                            </td>
                            <td>
                              <select className="form-control" value={line.maBienSanPham} onChange={(e) => updateLine(index, 'maBienSanPham', e.target.value)} disabled={!line.maSanPham}>
                                <option value="">Không chọn biến thể</option>
                                {variants.map((v) => (
                                  <option key={variantIdOf(v)} value={variantIdOf(v)}>
                                    {variantNameOf(v)}
                                  </option>
                                ))}
                              </select>
                            </td>
                            <td>
                              <input type="number" min="1" className="form-control text-right" value={line.soLuong} onChange={(e) => updateLine(index, 'soLuong', e.target.value)} />
                            </td>
                            <td>
                              <input className="form-control" value={line.ghiChu} onChange={(e) => updateLine(index, 'ghiChu', e.target.value)} />
                            </td>
                            <td className="text-center">
                              <button className="btn btn-xs btn-danger" type="button" onClick={() => removeLine(index)} disabled={form.items.length === 1}>
                                <i className="fas fa-trash"></i>
                              </button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
                <button className="btn btn-outline-primary" type="button" onClick={addLine}>
                  <i className="fas fa-plus mr-1"></i>
                  Thêm dòng hàng
                </button>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowCreate(false)} disabled={saving}>Đóng</button>
                <button type="button" className="btn btn-primary" onClick={saveDocument} disabled={saving}>{saving ? 'Đang lưu...' : 'Lưu phiếu nháp'}</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {showDetail && selectedDocument && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-xl">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Chi tiết phiếu kho - {selectedDocument.maPhieu ?? selectedDocument.MaPhieu}</h5>
                <button type="button" className="close" onClick={() => setShowDetail(false)}><span>&times;</span></button>
              </div>
              <div className="modal-body">
                <div className="row mb-3">
                  <div className="col-md-3"><strong>Loại:</strong> {TYPES[selectedDocument.loaiPhieu ?? selectedDocument.LoaiPhieu]}</div>
                  <div className="col-md-3"><strong>Trạng thái:</strong> {statusBadge(selectedDocument.trangThai ?? selectedDocument.TrangThai)}</div>
                  <div className="col-md-3"><strong>Ngày tạo:</strong> {formatDate(selectedDocument.ngayTao ?? selectedDocument.NgayTao)}</div>
                  <div className="col-md-3"><strong>Ngày duyệt:</strong> {formatDate(selectedDocument.ngayDuyet ?? selectedDocument.NgayDuyet)}</div>
                </div>
                <div className="table-responsive">
                  <table className="table table-bordered table-striped">
                    <thead>
                      <tr>
                        <th>Mã SP</th>
                        <th>SKU</th>
                        <th>Sản phẩm</th>
                        <th className="text-right">Tồn trước</th>
                        <th className="text-right">Thay đổi</th>
                        <th className="text-right">Tồn sau</th>
                        <th>Ghi chú</th>
                      </tr>
                    </thead>
                    <tbody>
                      {details.map((detail) => (
                        <tr key={detail.maChiTietPhieuKho ?? detail.MaChiTietPhieuKho}>
                          <td>{detail.maSanPhamKinhDoanh ?? detail.MaSanPhamKinhDoanh}</td>
                          <td>{detail.sku ?? detail.SKU ?? '-'}</td>
                          <td>{detail.tenSanPham ?? detail.TenSanPham} {detail.tenBienThe || detail.TenBienThe ? `- ${detail.tenBienThe ?? detail.TenBienThe}` : ''}</td>
                          <td className="text-right">{detail.tonTruoc ?? detail.TonTruoc}</td>
                          <td className="text-right">{detail.soLuongThayDoi ?? detail.SoLuongThayDoi}</td>
                          <td className="text-right">{detail.tonSau ?? detail.TonSau}</td>
                          <td>{detail.ghiChu ?? detail.GhiChu ?? '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
              <div className="modal-footer">
                {canApproveOrCancel && (
                  <>
                    <button className="btn btn-danger" type="button" onClick={cancelDocument} disabled={saving}>
                      Hủy phiếu
                    </button>
                    <button className="btn btn-success" type="button" onClick={approveDocument} disabled={saving}>
                      Duyệt phiếu
                    </button>
                  </>
                )}
                <button type="button" className="btn btn-outline-primary" onClick={printSelectedDocument}>
                  <i className="fas fa-print mr-1"></i>
                  In phiếu
                </button>
                <button type="button" className="btn btn-secondary" onClick={() => setShowDetail(false)}>Đóng</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default StockDocumentList;
