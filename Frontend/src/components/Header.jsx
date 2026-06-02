import { useEffect, useMemo, useState } from 'react';
import {
  FiChevronDown,
  FiFileText,
  FiHeart,
  FiLogIn,
  FiMapPin,
  FiMenu,
  FiShoppingCart,
  FiUser,
  FiUserPlus,
} from 'react-icons/fi';
import { RiDiscountPercentLine } from 'react-icons/ri';
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom';
import { brandAssets, navItems, socialLinks } from '../assets/siteData.js';
import { useAuth } from '../contexts/AuthContext.jsx';
import { useCart } from '../contexts/CartContext.jsx';
import { useFavorite } from '../contexts/FavoriteContext.jsx';
import { productApi, voucherApi } from '../services/api.js';

function getDisplayName(user) {
  return user?.name || user?.username || user?.email || 'Tài khoản';
}

function navItemBaseClass(isActive = false) {
  return [
    'group relative inline-flex min-h-[72px] items-center gap-2 rounded-t-2xl px-3 text-[15px] font-bold transition-all duration-300',
    isActive ? 'text-[#f0a327]' : 'text-[#171717]',
    'hover:-translate-y-[1px] hover:text-[#f0a327]',
    'after:absolute after:right-3 after:bottom-4 after:left-3 after:h-[3px] after:origin-left after:scale-x-0 after:rounded-full after:bg-[#f0a327] after:transition-transform after:duration-300 hover:after:scale-x-100',
  ].join(' ');
}

function valueOf(source, ...keys) {
  for (const key of keys) {
    if (source?.[key] !== undefined && source?.[key] !== null) {
      return source[key];
    }
  }

  return undefined;
}

function Header() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [productMenuOpen, setProductMenuOpen] = useState(false);
  const [productFilters, setProductFilters] = useState({ brands: [], carModels: [] });
  const [voucherCount, setVoucherCount] = useState(0);
  const { user: currentUser, isAuthenticated } = useAuth();
  const { count: cartCount } = useCart();
  const { count: favoriteCount } = useFavorite();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (isAuthenticated) {
      voucherApi.getMyVoucherCount().then(setVoucherCount).catch(() => setVoucherCount(0));
    } else {
      setVoucherCount(0);
    }
  }, [isAuthenticated, location.pathname]);

  useEffect(() => {
    setProductMenuOpen(false);
  }, [location.pathname, location.search, location.hash]);

  useEffect(() => {
    let mounted = true;

    productApi.getFilters()
      .then((filters) => {
        if (mounted) {
          setProductFilters({
            brands: filters?.brands || [],
            carModels: filters?.carModels || [],
          });
        }
      })
      .catch(() => {
        if (mounted) {
          setProductFilters({ brands: [], carModels: [] });
        }
      });

    return () => {
      mounted = false;
    };
  }, []);

  const productBrandGroups = useMemo(() => {
    const carModels = productFilters.carModels || [];

    return (productFilters.brands || [])
      .map((brand) => {
        const brandId = valueOf(brand, 'id', 'Id', 'maHangXe', 'MaHangXe');
        const brandName = valueOf(brand, 'name', 'Name', 'tenHang', 'TenHang') || '';

        return {
          id: brandId,
          name: brandName,
          items: carModels
            .filter((model) => String(valueOf(model, 'brandId', 'BrandId', 'maHangXe', 'MaHangXe')) === String(brandId))
            .map((model) => ({
              id: valueOf(model, 'id', 'Id', 'maDongXe', 'MaDongXe'),
              name: valueOf(model, 'name', 'Name', 'tenDongXe', 'TenDongXe') || '',
            }))
            .filter((model) => model.id && model.name),
        };
      })
      .filter((group) => group.id && group.name);
  }, [productFilters]);

  function handleNavClick(item, event) {
    setMenuOpen(false);
    setProductMenuOpen(false);

    if (item.to === '/' && location.pathname === '/') {
      event.preventDefault();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  return (
    <header className="sticky top-0 z-20 bg-white shadow-[0_10px_30px_rgba(0,0,0,0.08)]">
      <div className="mx-auto grid w-full max-w-[1200px] grid-cols-1 gap-2 px-4 pt-2 xl:grid-cols-[260px_1fr] xl:gap-5">
        <Link className="flex items-center justify-center py-2 xl:justify-start xl:py-3" to="/">
          {brandAssets.logo ? (
            <img src={brandAssets.logo} alt="EURO Moto" className="h-auto w-[190px] max-w-full object-contain xl:w-[238px]" />
          ) : (
            <span className="inline-flex items-center gap-3 text-[34px] font-black tracking-tight text-[#d71920]">
              <span className="grid h-16 w-16 place-items-center rounded-2xl bg-[#d71920] text-white">€</span>
              <span>Moto</span>
            </span>
          )}
        </Link>

        <div className="grid">
          <div className="rounded-xl bg-[#d71920] px-4 py-3 text-white xl:rounded-t-none xl:rounded-br-none xl:rounded-bl-[18px] xl:px-5 xl:py-2.5">
            <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
              <div className="flex flex-wrap items-center gap-y-2 text-[12px] font-medium xl:text-[13px]">
                <Link className="inline-flex items-center gap-1.5 border-white/60 pr-3 transition hover:text-[#ffe082] lg:border-r" to="/he-thong-cua-hang">
                  <FiMapPin className="h-4 w-4" />
                  Hệ thống cửa hàng
                </Link>

                {isAuthenticated ? (
                  <Link
                    className="inline-flex items-center gap-1.5 px-0 font-semibold transition hover:text-[#ffe082] lg:px-3"
                    to="/account"
                  >
                    <FiUser className="h-4 w-4" />
                    <span className="underline decoration-white/50 decoration-1 underline-offset-4">{getDisplayName(currentUser)}</span>
                  </Link>
                ) : (
                  <>
                    <Link className="inline-flex items-center gap-1.5 border-white/60 px-0 transition hover:text-[#ffe082] lg:border-r lg:px-3" to="/login">
                      <FiLogIn className="h-4 w-4" />
                      Đăng nhập
                    </Link>
                    <Link className="inline-flex items-center gap-1.5 px-0 transition hover:text-[#ffe082] lg:px-3" to="/register">
                      <FiUserPlus className="h-4 w-4" />
                      Đăng ký
                    </Link>
                  </>
                )}
              </div>

              <div className="flex items-center gap-2">
                {socialLinks.map((item, index) => {
                  const Icon = item.icon;
                  return (
                    <a
                      key={index}
                      className={`grid h-8 w-8 place-items-center rounded-full border-2 border-white/90 text-lg font-bold text-white transition duration-300 hover:-translate-y-1 hover:scale-105 ${item.className}`}
                      href={item.href}
                    >
                      {Icon && <Icon />}
                    </a>
                  );
                })}
              </div>
            </div>
          </div>

          <div className="relative flex min-h-[72px] items-center justify-between gap-4 bg-white">
            <nav className="hidden min-w-0 flex-1 xl:block">
              <div className="flex min-h-[72px] items-center gap-3 whitespace-nowrap">
                {navItems.map((item) => {
                  if (item.label === 'Trang chủ') {
                    return (
                      <NavLink
                        key={item.label}
                        to={item.to}
                        end
                        onClick={(event) => handleNavClick(item, event)}
                        className={({ isActive }) => navItemBaseClass(isActive)}
                      >
                        <span>{item.label}</span>
                      </NavLink>
                    );
                  }

                  if (item.label === 'Sản phẩm') {
                    return (
                      <div
                        key={item.label}
                        className="relative"
                        onMouseEnter={() => setProductMenuOpen(true)}
                        onMouseLeave={() => setProductMenuOpen(false)}
                      >
                        <button
                          type="button"
                          className={`${navItemBaseClass(false)} bg-transparent`}
                          onClick={() => setProductMenuOpen((value) => !value)}
                        >
                          <span>{item.label}</span>
                          <FiChevronDown className={`h-3.5 w-3.5 translate-y-[1px] transition duration-300 ${productMenuOpen ? 'rotate-180' : ''}`} />
                        </button>

                        <div className={`fixed left-1/2 top-[132px] z-30 -translate-x-1/2 pt-3 transition duration-300 ${productMenuOpen ? 'visible translate-y-0 opacity-100' : 'invisible translate-y-3 opacity-0'}`}>
                          <div className="w-[1120px] max-w-[calc(100vw-64px)] overflow-hidden rounded-[22px] border border-zinc-200 bg-white px-6 py-5 shadow-[0_24px_60px_rgba(0,0,0,0.16)]">
                            <div className="grid max-h-[70vh] grid-cols-4 gap-x-8 gap-y-7 overflow-y-auto pr-1">
                              {productBrandGroups.map((group) => (
                                <div key={group.id} className="min-w-0">
                                  <Link
                                    to={`/products?brandId=${encodeURIComponent(group.id)}`}
                                    onClick={() => setProductMenuOpen(false)}
                                    className="mb-3 block truncate text-[18px] font-bold text-[#e33232] transition hover:text-[#b81218]"
                                  >
                                    {group.name}
                                  </Link>
                                  <div className="grid gap-2">
                                    {group.items.map((groupItem) => (
                                      <Link
                                        key={`${group.id}-${groupItem.id}`}
                                        to={`/products?brandId=${encodeURIComponent(group.id)}&carModelId=${encodeURIComponent(groupItem.id)}`}
                                        onClick={() => setProductMenuOpen(false)}
                                        className="block truncate rounded-lg px-2 py-1.5 text-[15px] font-medium text-zinc-700 transition duration-200 hover:bg-[#fff6e6] hover:text-[#d71920]"
                                      >
                                        {groupItem.name}
                                      </Link>
                                    ))}
                                  </div>
                                </div>
                              ))}
                              {!productBrandGroups.length && (
                                <div className="col-span-4 px-2 py-4 text-sm font-semibold text-zinc-500">Chưa tải được danh sách hãng xe</div>
                              )}
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                  }

                  return (
                    <NavLink key={item.label} to={item.to} onClick={(event) => handleNavClick(item, event)} className={({ isActive }) => navItemBaseClass(isActive)}>
                      <span>{item.label}</span>
                      {item.hasCaret && <FiChevronDown className="h-3.5 w-3.5 translate-y-[1px] transition duration-300 group-hover:rotate-180" />}
                    </NavLink>
                  );
                })}
              </div>
            </nav>

            <div className="flex shrink-0 items-center gap-3">
              <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/favorites" aria-label="Yêu thích">
                <FiHeart className="h-7 w-7" />
                <span className="absolute right-0 top-1 grid h-[18px] w-[18px] place-items-center rounded-full bg-[#d71920] text-[11px] font-extrabold text-white">
                  {favoriteCount}
                </span>
              </Link>
              {isAuthenticated && (
                <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/vouchers" aria-label="Voucher">
                  <RiDiscountPercentLine className="h-7 w-7" />
                  {voucherCount > 0 && (
                    <span className="absolute right-0 top-1 grid h-[18px] w-[18px] place-items-center rounded-full bg-[#d71920] text-[11px] font-extrabold text-white">
                      {voucherCount}
                    </span>
                  )}
                </Link>
              )}
              {isAuthenticated && (
                <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/orders" aria-label="Đơn hàng">
                  <FiFileText className="h-7 w-7" />
                </Link>
              )}
              <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/cart" aria-label="Giỏ hàng">
                <FiShoppingCart className="h-7 w-7" />
                <span className="absolute right-0 top-1 grid h-[18px] w-[18px] place-items-center rounded-full bg-[#d71920] text-[11px] font-extrabold text-white">
                  {cartCount}
                </span>
              </Link>
              <button
                type="button"
                className="inline-flex h-11 items-center justify-center rounded-xl border border-zinc-200 px-4 text-sm font-bold transition hover:border-[#d71920] hover:text-[#d71920] xl:hidden"
                onClick={() => setMenuOpen((value) => !value)}
              >
                <FiMenu className="mr-2 h-4 w-4" />
                Menu
              </button>
            </div>
          </div>

          {menuOpen && (
            <nav className="grid gap-1 border-t border-zinc-200 py-3 xl:hidden">
              {navItems.map((item) => (
                <div key={item.label} className="grid gap-1">
                  <Link
                    to={item.to}
                    onClick={(event) => handleNavClick(item, event)}
                    className="flex min-h-11 items-center justify-between rounded-xl px-3 text-[15px] font-bold text-[#171717] transition hover:bg-zinc-100 hover:text-[#d71920]"
                  >
                    <span>{item.label}</span>
                    {item.hasCaret && <FiChevronDown className="h-3.5 w-3.5" />}
                  </Link>

                  {item.label === 'Sản phẩm' && (
                    <div className="grid gap-3 pl-3">
                      {productBrandGroups.map((group) => (
                        <div key={group.id} className="grid gap-1">
                          <Link
                            to={`/products?brandId=${encodeURIComponent(group.id)}`}
                            onClick={() => setMenuOpen(false)}
                            className="px-3 text-sm font-bold text-[#d71920]"
                          >
                            {group.name}
                          </Link>
                          {group.items.map((dropdownItem) => (
                            <Link
                              key={`${group.id}-${dropdownItem.id}`}
                              to={`/products?brandId=${encodeURIComponent(group.id)}&carModelId=${encodeURIComponent(dropdownItem.id)}`}
                              onClick={() => setMenuOpen(false)}
                              className="flex min-h-10 items-center rounded-xl px-3 text-sm font-medium text-zinc-600 transition hover:bg-[#fff6e6] hover:text-[#d71920]"
                            >
                              {dropdownItem.name}
                            </Link>
                          ))}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </nav>
          )}
        </div>
      </div>
    </header>
  );
}

export default Header;
