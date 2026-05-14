import { useEffect, useState } from 'react';
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom';
import { brandAssets, navItems, productBrandGroups, socialLinks } from '../assets/siteData.js';
import { useAuth } from '../contexts/AuthContext.jsx';
import { useCart } from '../contexts/CartContext.jsx';
import { useFavorite } from '../contexts/FavoriteContext.jsx';

function IconPin() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="h-4 w-4">
      <path d="M12 22s7-6.2 7-12a7 7 0 1 0-14 0c0 5.8 7 12 7 12Zm0-9.5A2.5 2.5 0 1 0 12 7a2.5 2.5 0 0 0 0 5.5Z" fill="currentColor" />
    </svg>
  );
}

function IconLogin() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="h-4 w-4">
      <path
        d="M13 4h5a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-5M10 17l5-5-5-5M15 12H4"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function IconUserPlus() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="h-4 w-4">
      <path
        d="M15 19a4 4 0 0 0-8 0M11 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm8 8v-6m-3 3h6"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function IconSearch() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="h-7 w-7">
      <circle cx="11" cy="11" r="7" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <path d="m20 20-3.5-3.5" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function IconHeart() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="h-7 w-7">
      <path
        d="M12 20.2 4.8 13A4.6 4.6 0 1 1 11.3 6.6L12 7.3l.7-.7A4.6 4.6 0 1 1 19.2 13L12 20.2Z"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function IconBag() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="h-7 w-7">
      <path d="M6 8h12l-1 12H7L6 8Z" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
      <path d="M9 9V7a3 3 0 1 1 6 0v2" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

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

function Header() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [productMenuOpen, setProductMenuOpen] = useState(false);
  const { user: currentUser, isAuthenticated, logout } = useAuth();
  const { count: cartCount, resetCart } = useCart();
  const { count: favoriteCount } = useFavorite();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    setProductMenuOpen(false);
  }, [location.pathname, location.search, location.hash]);

  function handleLogout() {
    logout();
    resetCart();
    navigate('/');
  }

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
          <img src={brandAssets.logo} alt="EURO Moto" className="h-auto w-[190px] max-w-full xl:w-[238px]" />
        </Link>

        <div className="grid">
          <div className="rounded-xl bg-[#d71920] px-4 py-3 text-white xl:rounded-t-none xl:rounded-br-none xl:rounded-bl-[18px] xl:px-5 xl:py-2.5">
            <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
              <div className="flex flex-wrap items-center gap-y-2 text-[12px] font-medium xl:text-[13px]">
                <Link className="inline-flex items-center gap-1.5 border-white/60 pr-3 transition hover:text-[#ffe082] lg:border-r" to="/he-thong-cua-hang">
                  <IconPin />
                  Hệ thống cửa hàng
                </Link>

                {isAuthenticated ? (
                  <>
                    <Link
                      className="inline-flex items-center gap-1.5 px-0 font-semibold transition hover:text-[#ffe082] lg:px-3"
                      to="/account"
                    >
                      Xin chào, <span className="underline decoration-white/50 decoration-1 underline-offset-4">{getDisplayName(currentUser)}</span>
                    </Link>
                    <button
                      type="button"
                      className="inline-flex items-center gap-1.5 bg-transparent px-0 transition hover:text-[#ffe082] lg:px-3"
                      onClick={handleLogout}
                    >
                      <IconLogin />
                      Đăng xuất
                    </button>
                  </>
                ) : (
                  <>
                    <Link className="inline-flex items-center gap-1.5 border-white/60 px-0 transition hover:text-[#ffe082] lg:border-r lg:px-3" to="/login">
                      <IconLogin />
                      Đăng nhập
                    </Link>
                    <Link className="inline-flex items-center gap-1.5 px-0 transition hover:text-[#ffe082] lg:px-3" to="/register">
                      <IconUserPlus />
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
                          <small className={`translate-y-[1px] text-[11px] transition duration-300 ${productMenuOpen ? 'rotate-180' : ''}`}>▼</small>
                        </button>

                        <div className={`absolute left-0 top-full z-30 pt-3 transition duration-300 ${productMenuOpen ? 'visible translate-y-0 opacity-100' : 'invisible translate-y-3 opacity-0'}`}>
                          <div className="w-[1040px] max-w-[calc(100vw-120px)] overflow-hidden rounded-[22px] border border-zinc-200 bg-white px-6 py-5 shadow-[0_24px_60px_rgba(0,0,0,0.16)]">
                            <div className="grid grid-cols-3 gap-12">
                              {productBrandGroups.map((group) => (
                                <div key={group.brandSlug}>
                                  <div className="mb-4 text-[20px] font-bold text-[#e33232]">{group.brandLabel}</div>
                                  <div className="grid gap-3">
                                    {group.items.map((groupItem) => (
                                      <Link
                                        key={`${group.brandSlug}-${groupItem.categorySlug || groupItem.productType}`}
                                        to={`/products?brandSlug=${encodeURIComponent(group.brandSlug)}&categorySlug=${encodeURIComponent(groupItem.categorySlug || groupItem.productType)}`}
                                        onClick={() => setProductMenuOpen(false)}
                                        className="inline-flex w-fit text-[17px] font-medium text-zinc-800 transition duration-200 hover:translate-x-1 hover:text-[#d71920]"
                                      >
                                        {groupItem.label}
                                      </Link>
                                    ))}
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                  }

                  return (
                    <NavLink key={item.label} to={item.to} onClick={(event) => handleNavClick(item, event)} className={({ isActive }) => navItemBaseClass(isActive)}>
                      <span>{item.label}</span>
                      {item.hasCaret && <small className="translate-y-[1px] text-[11px] transition duration-300 group-hover:rotate-180">▼</small>}
                    </NavLink>
                  );
                })}
              </div>
            </nav>

            <div className="flex shrink-0 items-center gap-3">
              <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/favorites" aria-label="Yêu thích">
                <IconHeart />
                <span className="absolute right-0 top-1 grid h-[18px] w-[18px] place-items-center rounded-full bg-[#d71920] text-[11px] font-extrabold text-white">
                  {favoriteCount}
                </span>
              </Link>
              {isAuthenticated && (
                <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/orders" aria-label="Đơn hàng">
                  <svg viewBox="0 0 24 24" aria-hidden="true" className="h-7 w-7"><path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /><path d="M9 14l2 2 4-4" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /></svg>
                </Link>
              )}
              <Link className="group relative inline-grid h-11 w-11 place-items-center rounded-full text-[#111] transition duration-300 hover:bg-zinc-100 hover:text-[#d71920]" to="/cart" aria-label="Giỏ hàng">
                <IconBag />
                <span className="absolute right-0 top-1 grid h-[18px] w-[18px] place-items-center rounded-full bg-[#d71920] text-[11px] font-extrabold text-white">
                  {cartCount}
                </span>
              </Link>
              <button
                type="button"
                className="inline-flex h-11 items-center justify-center rounded-xl border border-zinc-200 px-4 text-sm font-bold transition hover:border-[#d71920] hover:text-[#d71920] xl:hidden"
                onClick={() => setMenuOpen((value) => !value)}
              >
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
                    {item.hasCaret && <small className="text-[11px]">▼</small>}
                  </Link>

                  {item.label === 'Sản phẩm' && (
                    <div className="grid gap-3 pl-3">
                      {productBrandGroups.map((group) => (
                        <div key={group.brandSlug} className="grid gap-1">
                          <div className="px-3 text-sm font-bold text-[#d71920]">{group.brandLabel}</div>
                          {group.items.map((dropdownItem) => (
                            <Link
                              key={`${group.brandSlug}-${dropdownItem.categorySlug || dropdownItem.productType}`}
                              to={`/products?brandSlug=${encodeURIComponent(group.brandSlug)}&categorySlug=${encodeURIComponent(dropdownItem.categorySlug || dropdownItem.productType)}`}
                              className="flex min-h-10 items-center rounded-xl px-3 text-sm font-medium text-zinc-600 transition hover:bg-[#fff6e6] hover:text-[#d71920]"
                            >
                              {dropdownItem.label}
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
