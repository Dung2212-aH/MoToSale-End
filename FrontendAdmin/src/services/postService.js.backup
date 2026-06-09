import api from './api';

const postService = {
  getAll: (params) => api.get('/content/posts', { params }),
  getById: (id) => api.get(`/content/posts/${id}`),
  create: (data) => api.post('/content/posts', data),
  update: (id, data) => api.put(`/content/posts/${id}`, data),
  uploadImage: (id, formData) => api.post(`/content/posts/${id}/image`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
  delete: (id) => api.delete(`/content/posts/${id}`),
};

export default postService;
