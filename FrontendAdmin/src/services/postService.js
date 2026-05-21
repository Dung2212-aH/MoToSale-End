import api from './api';

const postService = {
  getAll: (params) => api.get('/content/posts', { params }),
  getById: (id) => api.get(`/content/posts/${id}`),
  create: (data) => api.post('/content/posts', data),
  update: (id, data) => api.put(`/content/posts/${id}`, data),
  delete: (id) => api.delete(`/content/posts/${id}`),
};

export default postService;
