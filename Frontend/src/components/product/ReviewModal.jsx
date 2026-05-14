import { useState } from 'react';
import api from '../../services/api.js';
import { FaStar } from 'react-icons/fa';
import { FiUpload, FiX } from 'react-icons/fi';

export default function ReviewModal({ isOpen, onClose, product, orderId }) {
  const [rating, setRating] = useState(5);
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [image, setImage] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  if (!isOpen) return null;

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setImage(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');

    const formData = new FormData();
    formData.append('MaSanPham', product.productId);
    formData.append('MaDonHang', orderId);
    formData.append('Diem', rating);
    formData.append('TieuDe', title);
    formData.append('NoiDung', content);
    if (image) {
      formData.append('Image', image);
    }

    try {
      await api.post('/reviews', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        }
      });
      setSuccess(true);
      setTimeout(() => {
        onClose();
        setSuccess(false);
      }, 2000);
    } catch (err) {
      setError(err.response?.data?.message || 'Có lỗi xảy ra khi gửi đánh giá');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-2xl relative max-h-[90vh] overflow-y-auto">
        <button
          onClick={onClose}
          className="absolute right-4 top-4 text-gray-400 hover:text-gray-600"
        >
          <FiX size={24} />
        </button>

        <h3 className="text-xl font-bold text-gray-900 mb-6">Đánh giá sản phẩm</h3>

        {success ? (
          <div className="text-center py-8">
            <div className="w-16 h-16 bg-green-100 text-green-500 rounded-full flex items-center justify-center mx-auto mb-4">
              <FaStar size={32} className="fill-green-500" />
            </div>
            <p className="text-lg font-medium text-gray-900">Cảm ơn bạn đã đánh giá!</p>
            <p className="text-gray-500 mt-2">Đánh giá của bạn sẽ giúp ích cho những người mua sau.</p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <p className="text-sm font-medium text-gray-700 mb-2">Đánh giá của bạn về sản phẩm này?</p>
              <div className="flex gap-2">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    type="button"
                    onClick={() => setRating(star)}
                    className="focus:outline-none transition-transform hover:scale-110"
                  >
                    <FaStar
                      size={32}
                      className={star <= rating ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}
                    />
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tiêu đề đánh giá</label>
              <input
                type="text"
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Tóm tắt đánh giá của bạn (ví dụ: Sản phẩm rất tốt)"
                className="w-full rounded-xl border border-gray-300 px-4 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-1 focus:ring-red-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Chi tiết đánh giá</label>
              <textarea
                required
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Hãy chia sẻ trải nghiệm của bạn về sản phẩm..."
                rows={4}
                className="w-full rounded-xl border border-gray-300 px-4 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-1 focus:ring-red-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Thêm hình ảnh (không bắt buộc)</label>
              <div className="flex items-center gap-4">
                <label className="cursor-pointer flex items-center justify-center w-24 h-24 border-2 border-dashed border-gray-300 rounded-xl hover:border-red-500 hover:bg-red-50 transition-colors">
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageChange}
                    className="hidden"
                  />
                  <div className="flex flex-col items-center text-gray-500">
                    <FiUpload size={20} className="mb-1" />
                    <span className="text-xs">Tải ảnh</span>
                  </div>
                </label>
                {imagePreview && (
                  <div className="relative w-24 h-24">
                    <img src={imagePreview} alt="Preview" className="w-full h-full object-cover rounded-xl border border-gray-200" />
                    <button
                      type="button"
                      onClick={() => { setImage(null); setImagePreview(null); }}
                      className="absolute -top-2 -right-2 w-6 h-6 bg-white border border-gray-200 rounded-full flex items-center justify-center text-red-500 hover:bg-red-50 shadow-sm"
                    >
                      <FiX size={14} />
                    </button>
                  </div>
                )}
              </div>
            </div>

            {error && (
              <div className="p-3 bg-red-50 text-red-600 text-sm rounded-lg border border-red-100">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="w-full rounded-xl bg-red-600 py-3 text-sm font-bold text-white transition hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {submitting ? 'Đang gửi...' : 'Gửi đánh giá'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
