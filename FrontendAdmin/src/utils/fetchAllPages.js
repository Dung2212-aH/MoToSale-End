// Helper dùng chung để vượt giới hạn MaxPageSize=100 của backend:
// gọi trang 1 để biết tổng số bản ghi rồi tải song song các trang còn lại.
// Dùng cho export/dropdown/thống kê cần ĐẦY ĐỦ dữ liệu (server clamp pageSize > 100 về 100).

const MAX_PAGES = 200; // chốt an toàn, tránh gọi API vô hạn khi total bất thường

export const unwrapList = (payload) => {
  const data = payload?.data ?? payload;
  if (Array.isArray(data)) return data;
  if (Array.isArray(data?.items)) return data.items;
  if (Array.isArray(data?.data)) return data.data;
  if (Array.isArray(data?.result)) return data.result;
  return [];
};

export const unwrapTotal = (payload) => {
  const data = payload?.data ?? payload;
  const list = unwrapList(data);
  return Number(data?.total ?? data?.totalItems ?? data?.totalCount ?? data?.totalRecords ?? data?.count ?? list.length ?? 0);
};

// fetcher: (params) => Promise<axios response | payload>
// Trả về { items, total } với items đã gộp đủ các trang.
export const fetchAllPages = async (fetcher, params = {}) => {
  const pageSize = Math.min(params.pageSize || 100, 100);
  const firstPayload = await fetcher({ ...params, page: 1, pageSize });
  const firstItems = unwrapList(firstPayload);
  const total = unwrapTotal(firstPayload);
  const totalPages = Math.min(MAX_PAGES, Math.max(1, Math.ceil(total / pageSize)));

  if (totalPages === 1) {
    return { items: firstItems, total };
  }

  const restResults = await Promise.allSettled(
    Array.from({ length: totalPages - 1 }, (_, index) =>
      fetcher({ ...params, page: index + 2, pageSize })
    )
  );

  const restItems = restResults.flatMap((result) =>
    result.status === 'fulfilled' ? unwrapList(result.value) : []
  );

  return {
    items: [...firstItems, ...restItems],
    total,
  };
};

export default fetchAllPages;
