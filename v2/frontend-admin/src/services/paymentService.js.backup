import api from './api';

const paymentService = {
  getAll: (params) => api.get('/payments', { params }),
  getByOrder: (orderId) => api.get(`/payments/order/${orderId}`),
  record: (data) => api.post('/payments', data),
  cancel: (id, data) => api.post(`/payments/${id}/cancel`, { reason: data?.reason || data?.lyDoHuy }),
};

export default paymentService;
