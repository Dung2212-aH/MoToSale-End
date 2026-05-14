const replacements = [
  ['Marketing & CSKH', 'Marketing & Customer Care'],
  ['Motor Admin', 'Motor Admin'],
  ['Admin Showroom', 'Admin Showroom'],
  ['Tổng quan', 'Dashboard'],
  ['Tá»•ng quan', 'Dashboard'],
  ['T?ng quan', 'Dashboard'],
  ['VẬN HÀNH', 'OPERATIONS'],
  ['DANH MỤC', 'CATALOG'],
  ['DANH M?C', 'CATALOG'],
  ['HỆ THỐNG', 'SYSTEM'],
  ['H? TH?NG', 'SYSTEM'],
  ['BaseCore Editles', 'Motor Admin'],
  ['Sign in Ä‘á»ƒ báº¯t Ä‘áº§u phiÃªn lÃ m viá»‡c', 'Sign in to start your session'],
  ['Thao tÃ¡c tháº¥t báº¡i', 'Action failed'],
  ['Váº¬N HÃ€NH', 'OPERATIONS'],
  ['Dashboard nhanh vÃ¡Â»Â danh mÃ¡Â»Â¥c, bÃƒÂ¡n hÃƒÂ ng vÃƒÂ  chÃ„Æ’m sÃƒÂ³c khÃƒÂ¡ch hÃƒÂ ng.', 'Quick overview of catalog, sales, and customer care.'],
  ['Ã„Âang tÃ¡ÂºÂ£i tÃ¡Â»â€¢ng quan...', 'Loading dashboard...'],
  ['Manage ??a ?i?m showroom, k?nh li?n h?, gi m ca v t?a ?? b?n .', 'Manage showroom locations, contact channels, opening hours, and map coordinates.'],
  ['Ã„Âang tÃ¡ÂºÂ£i showroom...', 'Loading showrooms...'],
  ['Theo d?i t?n kho products, variants, c?nh b?o tn thp, holds v t?c v? ??ng b?.', 'Track product inventory, variants, low-stock alerts, holds, and stock sync.'],
  ['??ng b? t?t c?', 'Sync all'],
  ['H?t h?n holds', 'Expire holds'],
  ['PRODUCTS? NAME THP', 'LOW-STOCK PRODUCTS'],
  ['VARIANTS? NAME THP', 'LOW-STOCK VARIANTS'],
  ['GI CH T?N KHO', 'INVENTORY HOLDS'],
  ['Ng?ng tn thp', 'Low stock threshold'],
  ['Loading t?n kho...', 'Loading inventory...'],
  ['Manage voucher, kh?ch h?ng, y?u c?u t? v?n, gi? h?ng, y?u th?ch v Reviews products.', 'Manage vouchers, customers, consultation requests, carts, favorites, and product reviews.'],
  ['CONTACTS M?I', 'NEW CONTACTS'],
  ['??NH HOLDS DUY?T', 'PENDING REVIEWS'],
  ['All ph?m vi', 'All scopes'],
  ['All tr?ng th?i', 'All statuses'],
  ['Ng?ng active', 'Inactive'],
  ['ang active', 'Active'],
  ['Manage b?i vi?t, FAQ, Thumbnail, tr?ng th?i published v xem? t?rc.', 'Manage blog posts, FAQ, thumbnails, publishing status, and previews.'],
  ['Add b?i vi?t', 'Add blog post'],
  ['All danh m?c', 'All categories'],
  ['Loading n?i dung...', 'Loading content...'],
  ['Manage xe m?y, ph? nameg, gi? b?n, t?n kho v tr?ng th?i bn.', 'Manage motorcycles, accessories, prices, inventory, and selling status.'],
  ['All loi', 'All types'],
  ['Th? t?', 'Sort'],
  ['Mi nht', 'Newest'],
  ['Gi thp n cao', 'Price low to high'],
  ['Gi cao n thp', 'Price high to low'],
  ['Nm mi nht', 'Newest year'],
  ['Ã„Âang tÃ¡ÂºÂ£i sÃ¡ÂºÂ£n phÃ¡ÂºÂ©m...', 'Loading products...'],
  ['Manage cy danh m?c, th? t? hi?n th? v tr?ng th?i active.', 'Manage category tree, display order, and active status.'],
  ['Danh s?ch danh m?c', 'Category list'],
  ['Categories cha', 'Parent category'],
  ['All danh m?c cha', 'All parent categories'],
  ['Ã„Âang tÃ¡ÂºÂ£i danh mÃ¡Â»Â¥c...', 'Loading categories...'],
  ['Manage h?ng s?n xu?t v c?c model dng trong catalog products.', 'Manage manufacturers and models used in the product catalog.'],
  ['Loading brand v model...', 'Loading brands and models...'],
  ['H? th?ng', 'System'],
  ['Manage t?i kho?n n?i b?, roles v settings vn hnh.', 'Manage internal accounts, roles, and operating settings.'],
  ['Add t?i khon ni b', 'Add internal account'],
  ['T?i kho?n n?i b?', 'Internal accounts'],
  ['C?u h?nh', 'Settings'],
  ['Loading dá»¯ liá»‡u...', 'Loading data...'],
  ['Tá»•ng:', 'Total:'],
  ['TrÆ°á»›c', 'Previous'],
  ['Editu', 'Next'],
  ['Tồn kho', 'Inventory'],
  ['T?n kho', 'Inventory'],
  ['Đơn hàng', 'Orders'],
  ['????n h?ng', 'Orders'],
  ['??n h?ng', 'Orders'],
  ['Nội dung', 'Content'],
  ['N?i dung', 'Content'],
  ['Báo cáo', 'Reports'],
  ['Sản phẩm', 'Products'],
  ['S?n ph?m', 'Products'],
  ['products', 'products'],
  ['Danh mục', 'Categories'],
  ['Danh m?c', 'Categories'],
  ['Hãng & dòng xe', 'Brands & Models'],
  ['Brand & d?ng xe', 'Brands & Models'],
  ['D?ng xe', 'Models'],
  ['Hệ thống', 'System'],
  ['H? tbrand', 'System'],
  ['Làm mới', 'Refresh'],
  ['L?m m?i', 'Refresh'],
  ['Thêm', 'Add'],
  ['Th?m', 'Add'],
  ['Sửa', 'Edit'],
  ['Cập nhật', 'Update'],
  ['C?p nh?t', 'Update'],
  ['Tạo', 'Create'],
  ['Hủy', 'Cancel'],
  ['H?y', 'Cancel'],
  ['Lưu', 'Save'],
  ['L?u', 'Save'],
  ['Đặt lại', 'Reset'],
  ['??t l?i', 'Reset'],
  ['Áp dụng', 'Apply'],
  ['?p d?ng', 'Apply'],
  ['Tìm kiếm', 'Search'],
  ['T?m ki?m', 'Search'],
  ['Tên', 'Name'],
  ['T?n', 'Name'],
  ['Trạng thái', 'Status'],
  ['Tr?ng th?i', 'Status'],
  ['Thao tác', 'Actions'],
  ['Đang hoạt động', 'Active'],
  ['Äang hoáº¡t Ä‘á»™ng', 'Active'],
  ['Ngừng hoạt động', 'Inactive'],
  ['Ngá»«ng hoáº¡t Ä‘á»™ng', 'Inactive'],
  ['Bị khóa', 'Locked'],
  ['Đang tải', 'Loading'],
  ['?ang t?i', 'Loading'],
  ['loading', 'Loading'],
  ['Không tìm thấy', 'No records found'],
  ['Không tìm? t?hấy', 'No records found'],
  ['Kh?ng t?m th?y', 'No records found'],
  ['Kbrand t?m th?y', 'No records found'],
  ['Chưa có', 'No'],
  ['Ch?a c?', 'No'],
  ['Dữ liệu', 'Data'],
  ['d? li?u', 'data'],
  ['Khách hàng', 'Customers'],
  ['Kh?ch h?ng', 'Customers'],
  ['Liên hệ', 'Contacts'],
  ['Li?n h?', 'Contacts'],
  ['Đánh giá', 'Reviews'],
  ['??nh gi?', 'Reviews'],
  ['Yêu thích', 'Favorites'],
  ['Y?u th?ch', 'Favorites'],
  ['Giỏ hàng', 'Cart'],
  ['Gi brand', 'Cart'],
  ['Voucher Usage', 'Voucher Usage'],
  ['Active vouchers', 'Active vouchers'],
  ['Liên hệ mới', 'New contacts'],
  ['Đánh giá chờ duyệt', 'Pending reviews'],
  ['Mã', 'Code'],
  ['Giảm giá', 'Discount'],
  ['Gi?m gi?', 'Discount'],
  ['Phạm vi', 'Scope'],
  ['Ph?m vi', 'Scope'],
  ['Sử dụng', 'Usage'],
  ['Ngày kết thúc', 'Ends'],
  ['Bài viết', 'Blog Posts'],
  ['B?i vi?t', 'Blog Posts'],
  ['FAQ', 'FAQ'],
  ['Xuất bản', 'Published'],
  ['xu?t b?n', 'published'],
  ['Bản nháp', 'Draft'],
  ['Xem trước', 'Preview'],
  ['Xem? t?rc', 'Preview'],
  ['Ảnh đại diện', 'Thumbnail'],
  ['?nh ??i di?n', 'Thumbnail'],
  ['Doanh thu', 'Revenue'],
  ['Đơn trong khoảng lọc', 'orders in range'],
  ['Giá trị', 'Value'],
  ['Gi? tr?', 'Value'],
  ['Phương thức thanh toán', 'Payment method'],
  ['Ph?ng thc thanh ton', 'Payment method'],
  ['Tồn kho thấp', 'Low Stock'],
  ['Name kho th?p', 'Low Stock'],
  ['Biến thể', 'Variants'],
  ['variants', 'variants'],
  ['Giữ chỗ', 'Holds'],
  ['Holds', 'Holds'],
  ['Tất cả', 'All'],
  ['T?t c?', 'All'],
  ['Hãng', 'Brand'],
  ['brand', 'brand'],
  ['Dòng xe', 'Model'],
  ['d?ng xe', 'model'],
  ['Loại', 'Type'],
  ['Lo?i', 'Type'],
  ['Màu', 'Color'],
  ['M?u', 'Color'],
  ['Giá gốc', 'Base price'],
  ['Gi? g?c', 'Base price'],
  ['Giá khuyến mãi', 'Sale price'],
  ['Gi? passwordy?n m?i', 'Sale price'],
  ['Giá riêng', 'Override price'],
  ['Gi? ri?ng', 'Override price'],
  ['Mã sản phẩm', 'Product code'],
  ['M? s?n ph?m', 'Product code'],
  ['Thông tin cơ bản', 'Basic information'],
  ['Thông số xe máy', 'Motorcycle specs'],
  ['Tbrand s xe m?y', 'Motorcycle specs'],
  ['Mô tả', 'Description'],
  ['M? t?', 'Description'],
  ['Tương thích phụ tùng', 'Accessory compatibility'],
  ['Tng thch ph? nameg', 'Accessory compatibility'],
  ['Quản lý', 'Manage'],
  ['Qu?n l?', 'Manage'],
  ['Tài khoản nội bộ', 'Internal accounts'],
  ['account ni b', 'internal account'],
  ['Cấu hình', 'Settings'],
  ['c?u h?nh', 'settings'],
  ['Vai trò', 'Roles'],
  ['roles', 'roles'],
  ['Ngày tạo', 'Created'],
  ['Mật khẩu', 'Password'],
  ['Máº­t kháº©u', 'Password'],
  ['Tên đăng nhập', 'Username'],
  ['TÃªn Ä‘Äƒng nháº­p', 'Username'],
  ['Đăng nhập', 'Sign in'],
  ['ÄÄƒng nháº­p', 'Sign in'],
  ['Ghi nhớ đăng nhập', 'Remember me'],
  ['Ghi nhá»› Ä‘Äƒng nháº­p', 'Remember me'],
  ['Bản quyền', 'Copyright'],
  ['Phiên bản', 'Version'],
];

const orderedReplacements = replacements.sort((a, b) => b[0].length - a[0].length);
const containsReplacements = [
  ['Dashboard nhanh', 'Quick overview of catalog, sales, and customer care.'],
  ['tÃ¡ÂºÂ£i tÃ¡Â»', 'Loading dashboard...'],
  ['tÃ¡ÂºÂ£i showroom', 'Loading showrooms...'],
  ['PRODUCTS? NAME THP', 'LOW-STOCK PRODUCTS'],
  ['VARIANTS? NAME THP', 'LOW-STOCK VARIANTS'],
  ['GI CH T?N KHO', 'INVENTORY HOLDS'],
  ['CONTACTS M?I', 'NEW CONTACTS'],
  ['??NH HOLDS DUY?T', 'PENDING REVIEWS'],
  ['Revenue, vn hnh', 'Revenue, operations, catalog demand, inventory risk, and voucher performance.'],
  ['Nh?m theo', 'Group by'],
  ['Inventory th?p', 'Low stock'],
  ['sÃ¡ÂºÂ£n phÃ¡ÂºÂ©m', 'Loading products...'],
  ['tÃ¡ÂºÂ£i danh', 'Loading categories...'],
  ['Add danh m?c', 'Add category'],
];
const attrs = ['placeholder', 'title', 'aria-label', 'alt'];

function translateValue(value) {
  if (!value || typeof value !== 'string') return value;
  let next = value;
  for (const [from, to] of orderedReplacements) {
    next = next.split(from).join(to);
  }
  for (const [from, to] of containsReplacements) {
    if (next.includes(from)) return to;
  }
  return next;
}

function translateNode(node) {
  if (node.nodeType === Node.TEXT_NODE) {
    const next = translateValue(node.nodeValue);
    if (next !== node.nodeValue) node.nodeValue = next;
    return;
  }
  if (node.nodeType !== Node.ELEMENT_NODE) return;
  for (const attr of attrs) {
    if (node.hasAttribute(attr)) {
      const current = node.getAttribute(attr);
      const next = translateValue(current);
      if (next !== current) node.setAttribute(attr, next);
    }
  }
  for (const child of node.childNodes) {
    translateNode(child);
  }
}

export function installEnglishUiText() {
  if (typeof window === 'undefined' || window.__englishUiTextInstalled) return;
  window.__englishUiTextInstalled = true;

  const run = () => translateNode(document.body);
  const observer = new MutationObserver((mutations) => {
    for (const mutation of mutations) {
      for (const node of mutation.addedNodes) {
        translateNode(node);
      }
      if (mutation.type === 'characterData') translateNode(mutation.target);
      if (mutation.type === 'attributes') translateNode(mutation.target);
    }
  });

  window.__runEnglishUiText = run;
  window.requestAnimationFrame(run);
  [50, 200, 500, 1000, 2000, 3500].forEach((delay) => window.setTimeout(run, delay));
  observer.observe(document.documentElement, {
    childList: true,
    subtree: true,
    characterData: true,
    attributes: true,
    attributeFilter: attrs,
  });
}
