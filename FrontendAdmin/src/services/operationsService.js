import api from './api';

const operationsService = {
  getSettings: () => api.get('/operations/settings'),
  saveSettings: (items) => api.put('/operations/settings', { items }),
  getWarehouses: () => api.get('/operations/warehouses'),
  saveWarehouse: (data) => api.post('/operations/warehouses', data),
};

export default operationsService;
