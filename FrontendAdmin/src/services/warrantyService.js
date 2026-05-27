import api from './api';

const warrantyService = {
  getAll: (params) => api.get('/warranties', { params }),
  getById: (id) => api.get(`/warranties/${id}`),
  create: (data) => api.post('/warranties', data),
  updateStatus: (id, data) => api.patch(`/warranties/${id}/status`, data),
};

export default warrantyService;
