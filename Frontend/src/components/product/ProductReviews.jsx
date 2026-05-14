import { useState, useEffect } from 'react';
import api from '../../services/api.js';
import { FaStar, FaUser } from 'react-icons/fa';
import { normalizeImageUrl } from '../../utils/formatters.js';

export default function ProductReviews({ productId }) {
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchReviews = async () => {
      try {
        const response = await api.get(`/reviews/product/${productId}`);
        setReviews(response.data || response);
      } catch (error) {
        console.error('Failed to fetch reviews:', error);
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
      <h3 className="text-xl font-bold text-gray-900 mb-6">Đánh giá sản phẩm ({reviews.length})</h3>

      {reviews.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          Chưa có đánh giá nào cho sản phẩm này.
        </div>
      ) : (
        <div className="space-y-6">
          {reviews.map((review) => {
            const reviewImageUrl = normalizeImageUrl(review.hinhAnhUrl);

            return (
              <div key={review.maDanhGia} className="border-b border-gray-100 pb-6 last:border-0 last:pb-0">
              <div className="flex items-start justify-between mb-2">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center text-gray-500">
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
                <h4 className="font-semibold text-gray-800 mb-1">{review.tieuDe}</h4>
              )}

              <p className="text-gray-600 mb-4 whitespace-pre-line">{review.noiDung}</p>

              {reviewImageUrl && (
                <div className="mt-3">
                  <img
                    src={reviewImageUrl}
                    alt="Review image"
                    className="h-24 w-24 object-cover rounded-lg cursor-pointer hover:opacity-90 transition-opacity border border-gray-200"
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
