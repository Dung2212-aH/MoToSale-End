import React, { useEffect, useMemo, useRef, useState } from 'react';
import productService from '../../services/productService';
import { useAuth } from '../../contexts/AuthContext';

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const SUCCESS_AUTO_DISMISS_MS = 4000;

function getErrorMessage(error, fallback = 'Thao tác thất bại. Vui lòng thử lại.') {
  return error?.response?.data?.message || error?.message || fallback;
}

function formatBytes(bytes) {
  if (!Number.isFinite(bytes) || bytes <= 0) return '';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
}

function normalizeUrl(url) {
  if (!url) return '';
  return url;
}

function getImageId(img) {
  return img?.id ?? img?.maAnhSanPham;
}

function getVariantId(variant) {
  return variant?.id ?? variant?.maBienSanPham;
}

const ImageManager = ({ productId, onClose }) => {
  const { isAdmin } = useAuth();
  const [images, setImages] = useState([]);
  const [variants, setVariants] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState({ current: 0, total: 0, currentName: '' });
  const [previews, setPreviews] = useState([]);
  const [selectedVariant, setSelectedVariant] = useState('');
  const [markFirstAsMain, setMarkFirstAsMain] = useState(true);
  const [viewFilter, setViewFilter] = useState('all');
  const [isDraggingOver, setIsDraggingOver] = useState(false);
  const [busyImageId, setBusyImageId] = useState(null);
  const fileInputRef = useRef(null);
  const previewsRef = useRef([]);
  const successTimerRef = useRef(null);
  const dragCounterRef = useRef(0);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [imgRes, varRes] = await Promise.all([
        productService.getImages(productId),
        productService.getVariants(productId),
      ]);
      const imgData = imgRes.data;
      const varData = varRes.data;
      setImages(Array.isArray(imgData) ? imgData : imgData.items || imgData.data || []);
      setVariants(Array.isArray(varData) ? varData : varData.items || varData.data || []);
    } catch (err) {
      setError(getErrorMessage(err, 'Không thể tải dữ liệu ảnh sản phẩm.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (productId) fetchData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productId]);

  useEffect(() => {
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, []);

  useEffect(() => {
    previewsRef.current = previews;
  }, [previews]);

  useEffect(() => {
    return () => {
      previewsRef.current.forEach((preview) => URL.revokeObjectURL(preview.preview));
      if (successTimerRef.current) clearTimeout(successTimerRef.current);
    };
  }, []);

  const flashSuccess = (message) => {
    setSuccess(message);
    if (successTimerRef.current) clearTimeout(successTimerRef.current);
    successTimerRef.current = setTimeout(() => setSuccess(''), SUCCESS_AUTO_DISMISS_MS);
  };

  const getVariantName = (maBienSanPham) => {
    if (!maBienSanPham) return null;
    const v = variants.find((vr) => String(getVariantId(vr)) === String(maBienSanPham));
    return v ? (v.tenBienThe || v.name || `Biến thể #${maBienSanPham}`) : `Biến thể #${maBienSanPham}`;
  };

  const addFiles = (fileList) => {
    const files = Array.from(fileList || []);
    if (!files.length) return;

    const rejected = [];
    const accepted = files
      .filter((file) => {
        if (!ALLOWED_TYPES.includes(file.type)) {
          rejected.push(`${file.name}: chỉ hỗ trợ JPG, PNG hoặc WebP`);
          return false;
        }
        if (file.size > MAX_FILE_SIZE) {
          rejected.push(`${file.name}: vượt quá 5MB`);
          return false;
        }
        return true;
      })
      .map((file) => ({
        file,
        preview: URL.createObjectURL(file),
        name: file.name,
        size: file.size,
        key: `${file.name}-${file.size}-${file.lastModified}-${Math.random().toString(36).slice(2, 8)}`,
      }));

    if (rejected.length) {
      setError(rejected.join('\n'));
    } else {
      setError('');
    }

    if (accepted.length) {
      setSuccess('');
      setPreviews((prev) => [...prev, ...accepted]);
    }
  };

  const handleFileSelect = (event) => {
    addFiles(event.target.files);
    // Reset so the same file can be re-selected after removing from the queue.
    event.target.value = '';
  };

  const handleDragEnter = (event) => {
    event.preventDefault();
    if (uploading) return;
    dragCounterRef.current += 1;
    setIsDraggingOver(true);
  };

  const handleDragLeave = (event) => {
    event.preventDefault();
    dragCounterRef.current = Math.max(0, dragCounterRef.current - 1);
    if (dragCounterRef.current === 0) setIsDraggingOver(false);
  };

  const handleDrop = (event) => {
    event.preventDefault();
    dragCounterRef.current = 0;
    setIsDraggingOver(false);
    if (uploading) return;
    addFiles(event.dataTransfer.files);
  };

  const removePreview = (index) => {
    setPreviews((prev) => {
      const updated = [...prev];
      URL.revokeObjectURL(updated[index].preview);
      updated.splice(index, 1);
      return updated;
    });
  };

  const movePreview = (index, direction) => {
    setPreviews((prev) => {
      const target = index + direction;
      if (target < 0 || target >= prev.length) return prev;
      const updated = [...prev];
      [updated[index], updated[target]] = [updated[target], updated[index]];
      return updated;
    });
  };

  const clearPreviews = () => {
    previews.forEach((preview) => URL.revokeObjectURL(preview.preview));
    setPreviews([]);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleUpload = async () => {
    if (!previews.length) {
      setError('Vui lòng chọn ít nhất 1 ảnh.');
      return;
    }

    setUploading(true);
    setError('');
    setSuccess('');
    setUploadProgress({ current: 0, total: previews.length, currentName: previews[0]?.name || '' });

    let uploadedCount = 0;
    let failedItem = null;

    for (let i = 0; i < previews.length; i += 1) {
      const item = previews[i];
      setUploadProgress({ current: i, total: previews.length, currentName: item.name });
      try {
        const formData = new FormData();
        formData.append('file', item.file);
        formData.append('isMain', markFirstAsMain && i === 0 ? 'true' : 'false');
        if (selectedVariant) {
          formData.append('maBienSanPham', selectedVariant);
        }
        // eslint-disable-next-line no-await-in-loop
        await productService.uploadImage(productId, formData);
        uploadedCount += 1;
      } catch (err) {
        failedItem = { name: item.name, message: getErrorMessage(err) };
        break;
      }
    }

    setUploadProgress({ current: uploadedCount, total: previews.length, currentName: '' });
    setUploading(false);

    if (failedItem) {
      setError(`Upload "${failedItem.name}" thất bại: ${failedItem.message}${
        uploadedCount > 0 ? ` (đã upload ${uploadedCount}/${previews.length} ảnh trước đó).` : '.'
      }`);
      // Remove successfully uploaded items from the queue so user can retry the rest.
      setPreviews((prev) => {
        const remaining = prev.slice(uploadedCount);
        prev.slice(0, uploadedCount).forEach((p) => URL.revokeObjectURL(p.preview));
        return remaining;
      });
    } else {
      clearPreviews();
      flashSuccess(`Đã upload ${uploadedCount} ảnh thành công.`);
    }

    await fetchData();
  };

  const handleDelete = async (imageId) => {
    if (!window.confirm('Xóa ảnh này? Hành động không thể hoàn tác.')) return;
    setBusyImageId(imageId);
    setError('');
    try {
      await productService.deleteImage(productId, imageId);
      await fetchData();
      flashSuccess('Đã xóa ảnh.');
    } catch (err) {
      setError(getErrorMessage(err, 'Xóa ảnh thất bại.'));
    } finally {
      setBusyImageId(null);
    }
  };

  const handleSetMain = async (imageId) => {
    setBusyImageId(imageId);
    setError('');
    try {
      const formData = new FormData();
      formData.append('imageId', imageId);
      formData.append('isMain', 'true');
      await productService.uploadImage(productId, formData);
      await fetchData();
      flashSuccess('Đã đặt ảnh chính.');
    } catch (err) {
      setError(getErrorMessage(err, 'Đặt ảnh chính thất bại.'));
    } finally {
      setBusyImageId(null);
    }
  };

  const filteredImages = images.filter((img) => {
    if (viewFilter === 'all') return true;
    if (viewFilter === 'common') return !img.maBienSanPham;
    return String(img.maBienSanPham) === String(viewFilter);
  });

  const commonImages = images.filter((img) => !img.maBienSanPham);
  const variantGroups = useMemo(
    () => variants.map((variant) => ({
      ...variant,
      images: images.filter((img) => String(img.maBienSanPham) === String(getVariantId(variant))),
    })),
    [images, variants],
  );

  const selectedVariantName = selectedVariant ? getVariantName(selectedVariant) : 'Ảnh chung sản phẩm';
  const mainHelpText = selectedVariant
    ? 'Ảnh đầu tiên sẽ là ảnh chính cho biến thể đang chọn.'
    : 'Ảnh đầu tiên sẽ là ảnh đại diện chung của sản phẩm.';

  const totalQueueSize = previews.reduce((sum, p) => sum + p.size, 0);

  return (
    <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-xl image-manager-dialog">
        <div className="modal-content">
          <div className="modal-header align-items-start">
            <div>
              <h5 className="modal-title">
                <i className="fas fa-images mr-2"></i>Quản lý ảnh sản phẩm
              </h5>
              <small className="text-muted">Upload ảnh chung hoặc ảnh riêng theo màu/phiên bản. Định dạng: JPG/PNG/WebP, tối đa 5MB và 4000×4000 px mỗi ảnh.</small>
            </div>
            <button type="button" className="close" onClick={onClose} aria-label="Đóng">
              <span>&times;</span>
            </button>
          </div>

          <div className="modal-body image-manager-body">
            <div className="image-upload-panel mb-3">
              <div className="row">
                <div className="col-lg-4">
                  <label className="small font-weight-bold" htmlFor="variantSelect">
                    Gắn ảnh vào
                    {variants.length === 0 && <span className="text-muted font-weight-normal"> (chưa có biến thể)</span>}
                  </label>
                  <select
                    id="variantSelect"
                    className="form-control"
                    value={selectedVariant}
                    onChange={(event) => setSelectedVariant(event.target.value)}
                    disabled={uploading || variants.length === 0}
                  >
                    <option value="">Ảnh chung sản phẩm</option>
                    {variants.map((variant) => {
                      const id = getVariantId(variant);
                      return (
                        <option key={id} value={id}>
                          {variant.tenBienThe || variant.name}
                          {variant.mauSac ? ` (${variant.mauSac})` : ''}
                          {variant.sku ? ` [${variant.sku}]` : ''}
                        </option>
                      );
                    })}
                  </select>
                  <small className="form-text text-muted mt-2">
                    <i className="fas fa-tag mr-1"></i>{selectedVariantName}
                  </small>

                  <div className="custom-control custom-checkbox mt-3">
                    <input
                      type="checkbox"
                      className="custom-control-input"
                      id="markFirstAsMain"
                      checked={markFirstAsMain}
                      onChange={(event) => setMarkFirstAsMain(event.target.checked)}
                      disabled={uploading}
                    />
                    <label className="custom-control-label font-weight-bold" htmlFor="markFirstAsMain">
                      Đặt ảnh đầu tiên làm ảnh chính
                    </label>
                    <small className="form-text text-muted">{mainHelpText}</small>
                  </div>
                </div>

                <div className="col-lg-8 mt-3 mt-lg-0">
                  <div
                    className={`image-dropzone ${uploading ? 'is-disabled' : ''} ${isDraggingOver ? 'is-dragging' : ''}`}
                    onDragOver={(event) => event.preventDefault()}
                    onDragEnter={handleDragEnter}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                  >
                    <input
                      ref={fileInputRef}
                      id="productImageUpload"
                      type="file"
                      accept="image/jpeg,image/png,image/webp"
                      multiple
                      className="image-dropzone-input"
                      onChange={handleFileSelect}
                      disabled={uploading}
                    />
                    <label htmlFor="productImageUpload" className="image-dropzone-label">
                      <span className="image-dropzone-icon">
                        <i className={`fas ${isDraggingOver ? 'fa-hand-paper' : 'fa-cloud-upload-alt'}`}></i>
                      </span>
                      <span className="font-weight-bold">
                        {isDraggingOver ? 'Thả ảnh vào để thêm' : 'Kéo thả ảnh vào đây hoặc bấm để chọn'}
                      </span>
                      <span className="text-muted">JPG, PNG, WebP • ≤ 5MB • ≤ 4000×4000 px</span>
                    </label>
                  </div>
                </div>
              </div>

              {previews.length > 0 && (
                <div className="mt-3">
                  <div className="d-flex flex-wrap align-items-center justify-content-between mb-2" style={{ gap: 8 }}>
                    <strong>
                      {previews.length} ảnh đang chờ upload
                      <span className="text-muted font-weight-normal ml-2">({formatBytes(totalQueueSize)})</span>
                    </strong>
                    <button
                      type="button"
                      className="btn btn-link btn-sm text-danger p-0"
                      onClick={clearPreviews}
                      disabled={uploading}
                    >
                      <i className="fas fa-trash-alt mr-1"></i>Xóa danh sách chờ
                    </button>
                  </div>
                  <div className="image-preview-strip">
                    {previews.map((preview, index) => {
                      const isMainSlot = markFirstAsMain && index === 0;
                      return (
                        <div
                          key={preview.key}
                          className={`image-preview-tile ${isMainSlot ? 'is-main-slot' : ''}`}
                        >
                          <img src={preview.preview} alt={preview.name} />
                          {isMainSlot && (
                            <span className="badge badge-primary image-preview-main">
                              <i className="fas fa-star mr-1"></i>Ảnh chính
                            </span>
                          )}
                          <button
                            type="button"
                            className="image-preview-remove"
                            onClick={() => removePreview(index)}
                            disabled={uploading}
                            aria-label={`Bỏ ${preview.name}`}
                            title="Bỏ khỏi danh sách"
                          >
                            ×
                          </button>
                          <div className="image-preview-meta">
                            <small className="image-preview-name" title={preview.name}>{preview.name}</small>
                            <small className="image-preview-size">{formatBytes(preview.size)}</small>
                          </div>
                          <div className="image-preview-reorder">
                            <button
                              type="button"
                              onClick={() => movePreview(index, -1)}
                              disabled={uploading || index === 0}
                              aria-label="Đưa lên trước"
                              title="Đưa lên trước"
                            >
                              <i className="fas fa-arrow-left"></i>
                            </button>
                            <button
                              type="button"
                              onClick={() => movePreview(index, 1)}
                              disabled={uploading || index === previews.length - 1}
                              aria-label="Đưa ra sau"
                              title="Đưa ra sau"
                            >
                              <i className="fas fa-arrow-right"></i>
                            </button>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                  <div className="mt-3 d-flex flex-wrap align-items-center" style={{ gap: 10 }}>
                    <button type="button" className="btn btn-primary" onClick={handleUpload} disabled={uploading}>
                      {uploading ? (
                        <>
                          <span className="spinner-border spinner-border-sm mr-2"></span>
                          Đang upload {uploadProgress.current}/{uploadProgress.total}
                          {uploadProgress.currentName ? ` — ${uploadProgress.currentName}` : ''}
                        </>
                      ) : (
                        <>
                          <i className="fas fa-upload mr-2"></i>
                          Upload {previews.length} ảnh
                        </>
                      )}
                    </button>
                    <span className="text-muted small">
                      <i className="fas fa-tag mr-1"></i>{selectedVariantName}
                    </span>
                  </div>
                  {uploading && uploadProgress.total > 0 && (
                    <div className="progress mt-2" style={{ height: 6 }}>
                      <div
                        className="progress-bar bg-primary"
                        role="progressbar"
                        style={{ width: `${(uploadProgress.current / uploadProgress.total) * 100}%` }}
                        aria-valuenow={uploadProgress.current}
                        aria-valuemin="0"
                        aria-valuemax={uploadProgress.total}
                      />
                    </div>
                  )}
                </div>
              )}
            </div>

            {error && (
              <div className="alert alert-danger white-space-pre-line d-flex align-items-start">
                <i className="fas fa-exclamation-circle mr-2 mt-1"></i>
                <div className="flex-grow-1">{error}</div>
                <button type="button" className="close ml-2" onClick={() => setError('')} aria-label="Đóng">
                  <span>&times;</span>
                </button>
              </div>
            )}
            {success && (
              <div className="alert alert-success d-flex align-items-center">
                <i className="fas fa-check-circle mr-2"></i>
                <span className="flex-grow-1">{success}</span>
              </div>
            )}

            {loading ? (
              <div className="text-center py-4">
                <div className="spinner-border text-primary"></div>
              </div>
            ) : (
              <>
                <div className="mb-3 image-manager-filter">
                  <div className="btn-group btn-group-sm flex-wrap" role="group">
                    <button
                      type="button"
                      className={`btn ${viewFilter === 'all' ? 'btn-primary' : 'btn-outline-secondary'}`}
                      onClick={() => setViewFilter('all')}
                    >
                      Tất cả <span className="badge badge-light ml-1">{images.length}</span>
                    </button>
                    <button
                      type="button"
                      className={`btn ${viewFilter === 'common' ? 'btn-primary' : 'btn-outline-secondary'}`}
                      onClick={() => setViewFilter('common')}
                    >
                      Ảnh chung <span className="badge badge-light ml-1">{commonImages.length}</span>
                    </button>
                    {variantGroups.map((group) => {
                      const id = getVariantId(group);
                      return (
                        <button
                          key={id}
                          type="button"
                          className={`btn ${viewFilter === String(id) ? 'btn-primary' : 'btn-outline-secondary'}`}
                          onClick={() => setViewFilter(String(id))}
                        >
                          {group.mauSac || group.tenBienThe || group.name}
                          <span className="badge badge-light ml-1">{group.images.length}</span>
                        </button>
                      );
                    })}
                  </div>
                </div>

                {filteredImages.length === 0 ? (
                  <div className="text-center text-muted py-4">
                    <i className="fas fa-image fa-2x mb-2"></i>
                    <p className="mb-0">Chưa có ảnh nào{viewFilter !== 'all' ? ' trong nhóm này' : ''}.</p>
                    <small>Kéo thả ảnh vào khung phía trên để bắt đầu.</small>
                  </div>
                ) : (
                  <div className="row product-image-grid">
                    {filteredImages.map((img) => {
                      const id = getImageId(img);
                      const isBusy = busyImageId === id;
                      return (
                        <div key={id} className="col-lg-3 col-md-4 col-sm-6 mb-3">
                          <div className={`card h-100 ${img.laAnhChinh ? 'border-primary' : ''} ${isBusy ? 'image-card-busy' : ''}`}>
                            <div className="product-image-thumb">
                              <img
                                src={normalizeUrl(img.urlAnh || img.url)}
                                alt={img.altText || ''}
                                loading="lazy"
                                onError={(event) => {
                                  event.currentTarget.style.display = 'none';
                                }}
                              />
                              {isBusy && (
                                <div className="image-card-overlay">
                                  <span className="spinner-border spinner-border-sm text-light"></span>
                                </div>
                              )}
                            </div>
                            <div className="card-body p-2">
                              {img.laAnhChinh && (
                                <span className="badge badge-primary mb-1 d-block">
                                  <i className="fas fa-star mr-1"></i>Ảnh chính
                                </span>
                              )}
                              <span className="badge badge-light d-block mb-2">
                                {img.maBienSanPham ? getVariantName(img.maBienSanPham) : 'Ảnh chung'}
                              </span>
                              <div className="btn-group btn-group-sm w-100">
                                {!img.laAnhChinh && (
                                  <button
                                    type="button"
                                    className="btn btn-outline-primary"
                                    onClick={() => handleSetMain(id)}
                                    disabled={isBusy}
                                    title="Đặt làm ảnh chính"
                                  >
                                    <i className="fas fa-star mr-1"></i>Chính
                                  </button>
                                )}
                                {isAdmin() && (
                                  <button
                                    type="button"
                                    className="btn btn-outline-danger"
                                    onClick={() => handleDelete(id)}
                                    disabled={isBusy}
                                    title="Xóa ảnh"
                                  >
                                    <i className="fas fa-trash"></i>
                                  </button>
                                )}
                              </div>
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </>
            )}
          </div>

          <div className="modal-footer">
            <small className="text-muted mr-auto">
              <i className="fas fa-info-circle mr-1"></i>
              Chọn biến thể trước khi upload nếu ảnh chỉ thuộc một màu/phiên bản cụ thể. Dùng mũi tên để đổi thứ tự upload — ảnh đầu danh sách sẽ là ảnh chính.
            </small>
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={uploading}>Đóng</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ImageManager;
