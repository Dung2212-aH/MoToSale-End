import { useEffect, useState } from 'react';
import { FaStar, FaUser } from 'react-icons/fa';
import api from '../../services/api.js';
import { normalizeImageUrl } from '../../utils/formatters.js';

export default function ProductReviews({ productId }) {
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchReviews = async () => {
      try {
        const response = await api.get(`/reviews/product/${productId}`);
        setReviews(response.data || response);
      } catch {
        setReviews([]);
      } finally {
        setLoading(false);
      }
    };

    if (productId) {
      fetchReviews();
    }
  }, [productId]);

  if (loading) {
    return <div className="py-8 text-center text-gray-500">Đang tải đánh giá...</div>;
  }

  return (
    <div className="mt-12 bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
      <h3 className="mb-6 text-xl font-bold text-gray-900">Đánh giá sản phẩm ({reviews.length})</h3>

      {reviews.length === 0 ? (
        <div className="py-8 text-center text-gray-500">
          Chưa có đánh giá nào cho sản phẩm này.
        </div>
      ) : (
        <div className="space-y-6">
          {reviews.map((review) => {
            const reviewImageUrl = normalizeImageUrl(review.hinhAnhUrl);

            return (
              <div key={review.maDanhGia} className="border-b border-gray-100 pb-6 last:border-0 last:pb-0">
                <div className="mb-2 flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gray-100 text-gray-500">
                      <FaUser size={20} />
                    </div>
                    <div>
                      <div className="font-medium text-gray-900">
                        Khách hàng {review.maNguoiDung}
                      </div>
                      <div className="text-sm text-gray-500">
                        {new Date(review.ngayTao).toLocaleDateString('vi-VN')}
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center">
                    {[...Array(5)].map((_, i) => (
                      <FaStar
                        key={i}
                        size={16}
                        className={i < review.diem ? 'text-yellow-400 fill-yellow-400' : 'text-gray-200'}
                      />
                    ))}
                  </div>
                </div>

                {review.tieuDe && (
                  <h4 className="mb-1 font-semibold text-gray-800">{review.tieuDe}</h4>
                )}

                <p className="mb-4 whitespace-pre-line text-gray-600">{review.noiDung}</p>

                {reviewImageUrl && (
                  <div className="mt-3">
                    <img
                      src={reviewImageUrl}
                      alt="Ảnh đánh giá"
                      className="h-24 w-24 cursor-pointer rounded-lg border border-gray-200 object-cover transition-opacity hover:opacity-90"
                      onClick={() => window.open(reviewImageUrl, '_blank')}
                    />
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
