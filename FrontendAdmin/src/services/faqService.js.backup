import api from './api';

const faqService = {
  getAll: (params) => api.get('/content/faq', { params }),
  getById: (id) => api.get(`/content/faq/${id}`),
  create: (data) => api.post('/content/faq', data),
  update: (id, data) => api.put(`/content/faq/${id}`, data),
  delete: (id) => api.delete(`/content/faq/${id}`),
};

export default faqService;
