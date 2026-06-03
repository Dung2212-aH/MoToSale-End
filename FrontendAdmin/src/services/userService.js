import api from './api';

const mapUser = (u) => ({
  ...u,
  maNguoiDung: u.id,
  hoTen: u.fullName,
  email: u.email,
  soDienThoai: u.phoneNumber,
  roles: u.roles || (u.role ? [u.role] : []),
  vaiTro: (u.roles && u.roles[0]) || u.role,
  trangThai: u.status === 0 ? 'Inactive' : 'Active',
  ghiChuChamSoc: u.careNote,
  careNote: u.careNote,
  ngayTao: u.createdDate,
});

const wrapList = (res) => {
  const d = res.data;
  const items = (d.items || d || []).map(mapUser);
  return { ...res, data: { ...d, items, data: items } };
};

const toV2 = (d) => ({
  fullName: d.hoTen ?? d.fullName,
  email: d.email,
  phoneNumber: d.soDienThoai ?? d.phoneNumber ?? null,
  password: d.matKhau ?? d.password,
  role: d.vaiTro ?? d.role ?? 'Customer',
  status: d.trangThai === 'Inactive' ? 0 : 1,
});

const userService = {
  getAll: async (params) => wrapList(await api.get('/users', { params })),
  getCustomers: async (params) => wrapList(await api.get('/users/customers', { params })),
  updateCustomerCareNote: (id, data) => api.patch(`/users/customers/${id}/care-note`, { careNote: data.ghiChuChamSoc ?? data.careNote ?? data.ghiChu ?? null }),
  getById: async (id) => {
    const res = await api.get(`/users/${id}`);
    return { ...res, data: mapUser(res.data) };
  },
  create: (data) => api.post('/users', toV2(data)),
  update: (id, data) => api.put(`/users/${id}`, toV2(data)),
  updateStatus: (id, data) => api.patch(`/users/${id}/status`, { status: (data.trangThai ?? data.status) === 'Inactive' ? 0 : 1 }),
  delete: (id) => api.delete(`/users/${id}`),
};

export default userService;
