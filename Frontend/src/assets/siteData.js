import {
  FaFacebookF,
  FaYoutube,
  FaTwitter,
  FaPinterestP,
  FaInstagram
} from "react-icons/fa";
import banner1 from "./banners/banner1.png";
import banner2 from "./banners/banner2.png";
import banner3 from "./banners/banner3.png";
import banner4 from "./banners/banner4.png";
import banner5 from "./banners/banner5.png";
import maintenanceService from "./services/maintenance.png";
import logoEuroMoto from "./logo.png";

export const brandAssets = {
  logo: logoEuroMoto,
  footerLogo: logoEuroMoto,
  slider: banner1,
  bannerOne: banner4,
  bannerTwo: banner5,
  productBanner: banner2,
  collectionBannerOne: banner3,
  collectionBannerTwo: banner5,
};

export const homeHeroSlides = [
  {
    id: 'banner-1',
    image: banner1,
    alt: 'EURO Moto',
    to: '/products',
  },
  {
    id: 'banner-2',
    image: banner2,
    alt: 'EURO Moto',
    to: '/products',
  },
  {
    id: 'banner-4',
    image: banner4,
    alt: 'EURO Moto',
    to: '/products',
  },
  {
    id: 'banner-3',
    image: banner3,
    alt: 'EURO Moto',
    to: '/products',
  },
  {
    id: 'banner-5',
    image: banner5,
    alt: 'EURO Moto',
    to: '/products',
  },
];

export const homeCategoryReferences = [
  {
    id: 'featured-scooter',
    name: 'Xe tay ga',
    slug: 'xe-tay-ga',
    image: '',
    to: '/products?categorySlug=xe-tay-ga',
    match: ['xe tay ga', 'tay ga', 'scooter'],
  },
  {
    id: 'featured-manual',
    name: 'Xe số',
    slug: 'xe-so',
    image: '',
    to: '/products?categorySlug=xe-so',
    match: ['xe so', 'xe số', 'so', 'underbone'],
  },
  {
    id: 'featured-sport',
    name: 'Xe côn tay',
    slug: 'xe-con-tay',
    image: '',
    to: '/products?categorySlug=xe-con-tay',
    match: ['xe con tay', 'xe côn tay', 'con tay', 'sport'],
  },
  {
    id: 'featured-pkl',
    name: 'Xe phân khối lớn',
    slug: 'xe-phan-khoi-lon',
    image: '',
    to: '/products?categorySlug=xe-phan-khoi-lon',
    match: ['xe phan khoi lon', 'xe phân khối lớn', 'phan khoi lon', 'pkl'],
  },
];

export const serviceHighlights = [
  {
    id: 'bao-duong',
    title: 'Bảo dưỡng xe',
    description: 'Bảo dưỡng định kỳ, thay dầu, kiểm tra máy và hệ thống phanh để xe luôn vận hành ổn định.',
    image: maintenanceService,
  },
  {
    id: 'phu-tung',
    title: 'Phụ tùng chính hãng',
    description: 'Cung cấp linh kiện và phụ tùng đúng tiêu chuẩn chính hãng cho các dòng xe phổ biến.',
    image: banner3,
  },
  {
    id: 'luu-dong',
    title: 'Sửa chữa lưu động',
    description: 'Hỗ trợ xử lý sự cố nhanh, tư vấn tại chỗ và sắp xếp kỹ thuật viên khi khách hàng cần gấp.',
    image: banner4,
  },
  {
    id: 've-sinh',
    title: 'Vệ sinh buồng đốt',
    description: 'Làm sạch hệ thống buồng đốt, kim phun và họng máy để cải thiện hiệu suất và tiết kiệm nhiên liệu.',
    image: banner5,
  },
];

export const navItems = [
  { label: 'Trang chủ', to: '/' },
  { label: 'Sản phẩm', to: '/products', hasCaret: true },
  { label: 'Liên hệ', to: '/' },
  { label: 'Hệ thống cửa hàng', to: '/he-thong-cua-hang' },
  { label: 'Câu hỏi thường gặp', to: '/faq' },
];

export const socialLinks = [
  {
    icon: FaFacebookF,
    className: "bg-[#1877f2]",
    href: "#"
  },
  {
    icon: FaYoutube,
    className: "bg-[#ff0000]",
    href: "#"
  },
  {
    icon: FaTwitter,
    className: "bg-[#1d9bf0]",
    href: "#"
  },
  {
    icon: FaPinterestP,
    className: "bg-[#e60023]",
    href: "#"
  },
  {
    icon: FaInstagram,
    className:
      "bg-[linear-gradient(135deg,#ffb347,#fd1d1d_55%,#c13584)]",
    href: "#"
  }
];

export const productBrandGroups = [
  {
    brandLabel: 'Honda',
    brandSlug: 'honda',
    items: [
      { label: 'Xe ga', categorySlug: 'xe-tay-ga' },
      { label: 'Xe côn tay', categorySlug: 'xe-con-tay' },
      { label: 'Xe số', categorySlug: 'xe-so' },
    ],
  },
  {
    brandLabel: 'Yamaha',
    brandSlug: 'yamaha',
    items: [
      { label: 'Xe ga', categorySlug: 'xe-tay-ga' },
      { label: 'Xe côn tay', categorySlug: 'xe-con-tay' },
      { label: 'Xe số', categorySlug: 'xe-so' },
    ],
  },
  {
    brandLabel: 'SYM',
    brandSlug: 'sym',
    items: [
      { label: 'Xe ga', categorySlug: 'xe-tay-ga' },
      { label: 'Xe côn tay', categorySlug: 'xe-con-tay' },
      { label: 'Xe số', categorySlug: 'xe-so' },
    ],
  },
];
