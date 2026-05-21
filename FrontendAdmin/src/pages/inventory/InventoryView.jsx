import React, { useState, useEffect } from 'react';
import api from '../../services/api';

const InventoryView = () => {
  const [inventory, setInventory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [syncing, setSyncing] = useState(false);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;

  const fetchInventory = async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize };
      // TODO: Backend cần bổ sung endpoint này (view v_TONKHO_KHADUNG)
      const res = await api.get('/inventory', { params });
      const data = res.data;
      setInventory(data.items || data.data || data || []);
      setTotalPages(data.totalPages || Math.ceil((data.total || 0) / pageSize) || 1);
    } catch (err) {
      setError('Không thể tải dữ liệu tồn kho. Vui lòng thử lại.');
      setInventory([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchInventory();
  }, [page]);

  const handleSync = async () => {
    if (!window.confirm('Bạn có chắc muốn đồng bộ tồn kho? Quá trình này có thể mất vài giây.')) return;
    setSyncing(true);
    try {
      // TODO: Backend cần bổ sung endpoint gọi SP sp_SANPHAM_DongBoTatCaSoLuongTon
      await api.post('/inventory/sync');
      alert('Đồng bộ tồn kho thành công!');
      fetchInventory();
    } catch (err) {
      alert('Đồng bộ tồn kho thất bại. Vui lòng thử lại.');
    } finally {
      setSyncing(false);
    }
  };

  return (
    <div className="content-wrapper">
      <div className="content-header">
        <div className="container-fluid">
          <div className="row mb-2">
            <div className="col-sm-6">
              <h1 className="m-0">Quản lý Tồn kho</h1>
            </div>
          </div>
        </div>
      </div>

      <section className="content">
        <div className="container-fluid">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Tồn kho sản phẩm</h3>
              <div className="card-tools">
                <button
                  className="btn btn-success btn-sm"
                  onClick={handleSync}
                  disabled={syncing}
                >
                  <i className={`fas fa-sync-alt ${syncing ? 'fa-spin' : ''}`}></i>
                  {syncing ? ' Đang đồng bộ...' : ' Đồng bộ tồn kho'}
                </button>
              </div>
            </div>
            <div className="card-body">
              {/* Error */}
              {error && (
                <div className="alert alert-danger">{error}</div>
              )}

              {/* Loading */}
              {loading ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Đang tải...</span>
                  </div>
                </div>
              ) : inventory.length === 0 ? (
                <div className="text-center py-4">
                  <i className="fas fa-boxes fa-3x text-muted mb-3"></i>
                  <p className="text-muted">Không có dữ liệu tồn kho.</p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-bordered table-striped">
                      <thead>
                        <tr>
                          <th>Sản phẩm</th>
                          <th>Biến thể</th>
                          <th className="text-center">Tồn kho thực tế</th>
                          <th className="text-center">Đang giữ chỗ</th>
                          <th className="text-center">Tồn kho khả dụng</th>
                        </tr>
                      </thead>
                      <tbody>
                        {inventory.map((item, idx) => (
                          <tr key={item.id || idx}>
                            <td>{item.tenSanPham || item.productName || '—'}</td>
                            <td>{item.tenBienThe || item.variantName || '—'}</td>
                            <td className="text-center">
                              <span className="font-weight-bold">{item.tonKhoThucTe ?? item.actualStock ?? 0}</span>
                            </td>
                            <td className="text-center">
                              <span className={`font-weight-bold ${(item.dangGiuCho ?? item.reserved ?? 0) > 0 ? 'text-warning' : ''}`}>
                                {item.dangGiuCho ?? item.reserved ?? 0}
                              </span>
                            </td>
                            <td className="text-center">
                              <span className={`font-weight-bold ${(item.tonKhoKhaDung ?? item.availableStock ?? 0) <= 0 ? 'text-danger' : 'text-success'}`}>
                                {item.tonKhoKhaDung ?? item.availableStock ?? 0}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {/* Pagination */}
                  {totalPages > 1 && (
                    <nav className="mt-3">
                      <ul className="pagination justify-content-center">
                        <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}>
                          <button className="page-link" onClick={() => setPage(page - 1)}>«</button>
                        </li>
                        {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                          <li key={p} className={`page-item ${p === page ? 'active' : ''}`}>
                            <button className="page-link" onClick={() => setPage(p)}>{p}</button>
                          </li>
                        ))}
                        <li className={`page-item ${page >= totalPages ? 'disabled' : ''}`}>
                          <button className="page-link" onClick={() => setPage(page + 1)}>»</button>
                        </li>
                      </ul>
                    </nav>
                  )}
                </>
              )}

              <div className="mt-3">
                <small className="text-muted">
                  <i className="fas fa-info-circle"></i> Dữ liệu tồn kho được lấy từ view <code>v_TONKHO_KHADUNG</code>. 
                  Nhấn "Đồng bộ tồn kho" để cập nhật lại số liệu từ stored procedure.
                </small>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default InventoryView;
