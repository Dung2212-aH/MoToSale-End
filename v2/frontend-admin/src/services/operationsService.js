import api from './api';

const operationsService = {
  getWarehouses: () => api.get('/operations/warehouses'),
  saveWarehouse: (data) => api.post('/operations/warehouses', data),
  getSettings: () => api.get('/operations/settings'),
  saveSettings: (items) => api.put('/operations/settings', { items }),
};

export default operationsService;
