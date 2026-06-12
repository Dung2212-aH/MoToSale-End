import api from './api';

const contactService = {
  getAll: (params) => api.get('/content/contacts', { params }),
  markProcessed: (id) => api.patch(`/content/contacts/${id}/process`),
};

export default contactService;
