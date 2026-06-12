import api from './api';

const reviewService = {
  getAll: (params) => api.get('/reviews', { params }),
  updateStatus: (id, data) => api.patch(`/reviews/${id}/status`, data),
  delete: (id) => api.delete(`/reviews/${id}`),
};

export default reviewService;
