import fs from 'node:fs/promises';
import path from 'node:path';
import { createRequire } from 'node:module';

const require = createRequire('D:/MotorTeam/MoToSale-End/v2/frontend-admin/package.json');
const { chromium } = require('playwright');

const STORE = 'http://127.0.0.1:5174';
const API = 'http://localhost:5100/api';
const ART = path.resolve('D:/MotorTeam/MoToSale-End/docs/test-artifacts/store-section12-20260605');
const results = [];
const createdOrders = [];

function rec(id, status, actual, evidence = {}) {
  results.push({ id, status, actual, evidence });
}

function ok(id, cond, pass, fail, evidence = {}) {
  rec(id, cond ? 'PASS' : 'FAIL', cond ? pass : fail, evidence);
}

function warn(id, actual, evidence = {}) {
  rec(id, 'WARN', actual, evidence);
}

function block(id, actual, evidence = {}) {
  rec(id, 'BLOCK', actual, evidence);
}

function money(n) {
  return Number(n || 0);
}

function norm(v = '') {
  return String(v).normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/[đĐ]/g, 'd').toLowerCase().trim();
}

function parentIdOf(category) {
  return category?.parentCategoryId ?? category?.parentId ?? category?.ParentCategoryId ?? category?.ParentId ?? null;
}

function userFromLogin(data) {
  return {
    token: data.token,
    userId: data.user?.id,
    username: data.user?.email,
    name: data.user?.fullName,
    email: data.user?.email,
    role: data.user?.roles?.[0] || 'Customer',
    raw: data,
  };
}

async function api(apiPath, opts = {}) {
  const headers = {
    ...(opts.body !== undefined ? { 'Content-Type': 'application/json' } : {}),
    ...(opts.token ? { Authorization: `Bearer ${opts.token}` } : {}),
    ...(opts.headers || {}),
  };
  const res = await fetch(API + apiPath, {
    method: opts.method || 'GET',
    headers,
    body: opts.body === undefined ? undefined : JSON.stringify(opts.body),
  });
  const text = await res.text();
  let data = null;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }
  return { ok: res.ok, status: res.status, data, text };
}

async function login(email, password) {
  const r = await api('/auth/login', { method: 'POST', body: { email, password } });
  if (!r.ok) throw new Error(`Login failed ${email} ${r.status} ${r.text}`);
  return userFromLogin(r.data);
}

async function clearCart(token) {
  const cart = await api('/cart', { token });
  for (const item of cart.data?.items || []) {
    // eslint-disable-next-line no-await-in-loop
    await api(`/cart/items/${item.id}`, { method: 'DELETE', token });
  }
}

async function addCart(token, skuId = 19, qty = 1) {
  return api('/cart/items', { method: 'POST', token, body: { skuId, qty } });
}

async function cartState(token) {
  return (await api('/cart', { token })).data;
}

async function safeCancel(token, orderId, reason = 'section12 cleanup') {
  if (!orderId) return null;
  const r = await api(`/orders/${orderId}/cancel`, { method: 'POST', token, body: { reason } });
  return { status: r.status, ok: r.ok, message: r.data?.message || r.text };
}

async function screenshot(page, name) {
  await fs.mkdir(ART, { recursive: true });
  const file = path.join(ART, `${name}.png`);
  await page.screenshot({ path: file, fullPage: true });
  return file;
}

async function authContext(browser, user, viewport = { width: 1366, height: 768 }) {
  const context = await browser.newContext({ viewport });
  await context.addInitScript((u) => {
    sessionStorage.setItem('token', u.token);
    sessionStorage.setItem('user', JSON.stringify(u));
  }, user);
  return context;
}

async function goto(page, route, opts = {}) {
  await page.goto(STORE + route, { waitUntil: 'domcontentloaded', timeout: 30000 });
  if (opts.networkidle !== false) {
    await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {});
  }
  await page.waitForTimeout(opts.wait || 300);
}

async function bodyInfo(page) {
  return page.evaluate(() => ({
    text: document.body.innerText,
    footer: !!document.querySelector('footer'),
    header: !!document.querySelector('header'),
    path: location.pathname + location.search,
    scrollW: document.documentElement.scrollWidth,
    clientW: document.documentElement.clientWidth,
  }));
}

await fs.mkdir(ART, { recursive: true });

const customer = await login('store_e2e_20260604174344@motosale.local', 'Store@12345');
const other = await login('customer@motosale.local', 'Customer@123');
const admin = await login('admin@motosale.local', 'Admin@123');
const filters = (await api('/products/filters')).data;
const product10 = (await api('/products/10')).data;
const vouchers = (await api('/vouchers/available')).data?.items || [];
const storesRaw = await api('/showrooms');
const stores = storesRaw.data?.items || storesRaw.data || [];

ok(
  '12.0-api-baseline',
  !!(customer.token && admin.token && filters?.categories?.length && product10?.id === 10),
  'API baseline OK: login/filter/product available',
  'API baseline missing auth/filter/product',
  { categories: filters?.categories?.length, vouchers: vouchers.length, stores: Array.isArray(stores) ? stores.length : null },
);

const browser = await chromium.launch({ headless: true });
try {
  const guest = await browser.newContext({ viewport: { width: 1366, height: 768 } });
  let page = await guest.newPage();

  await goto(page, '/vouchers');
  const voucherButtons = await page.locator('section button').count();
  await screenshot(page, '12-1-vouchers-guest');
  ok(
    '12.1-vouchers-public',
    page.url().includes('/vouchers') && voucherButtons > 0,
    'Guest opens /vouchers and sees voucher action buttons',
    'Guest /vouchers not usable or no buttons',
    { url: page.url(), buttons: voucherButtons },
  );

  if (voucherButtons > 0) {
    await page.locator('section button').first().click().catch(() => {});
    await page.waitForTimeout(500);
    const notice = await page.locator('.fixed.right-4.top-4').count();
    ok(
      '12.1-vouchers-guest-save-login-required',
      notice > 0 && page.url().includes('/vouchers'),
      'Guest save voucher stays on page and shows notification',
      'Guest save voucher did not show visible notification',
      { notice, url: page.url() },
    );
  }

  for (const route of ['/cart', '/favorites', '/checkout', '/checkout/success', '/orders', '/orders/73', '/account']) {
    await goto(page, route, { wait: 500 });
    ok(
      `12.1-protected-${route.replace(/[^a-z0-9]/gi, '_')}`,
      page.url().includes('/login'),
      'Guest redirected to login',
      'Guest was not redirected to login',
      { route, url: page.url() },
    );
  }

  await goto(page, '/no-such-route-section12');
  const nf = await bodyInfo(page);
  ok(
    '12.1-404-layout',
    !nf.header && !nf.footer && nf.text.length > 20,
    '404 route outside MainLayout as implemented, no crash',
    '404 route unexpected layout or blank',
    nf,
  );

  await goto(page, '/');
  const footerLinks = await page.evaluate(() => Array.from(document.querySelectorAll('footer a')).map((a) => a.getAttribute('href')));
  const hashLinkCount = footerLinks.filter((h) => h === '#').length;
  ok(
    '12.1-header-footer-placeholder-links',
    hashLinkCount > 0 && footerLinks.some((h) => h === '/'),
    'Footer placeholder and home links exist; clicked behavior recorded as placeholder scope',
    'Footer link inventory unexpected',
    { footerLinks: footerLinks.slice(0, 12), hashLinkCount },
  );
  await guest.close();

  const ac = await authContext(browser, customer);
  page = await ac.newPage();
  for (const route of ['/login', '/register']) {
    await goto(page, route, { wait: 500 });
    ok(
      `12.1-public-only-${route.slice(1)}`,
      !page.url().includes(route),
      'Authenticated user redirected away from public-only route',
      'Authenticated user remained on public-only route',
      { route, url: page.url() },
    );
  }

  await goto(page, '/account');
  await page.waitForSelector('input[name=email]', { timeout: 10000 }).catch(() => {});
  const emailReadonly = await page.locator('input[name=email]').evaluate((el) => ({ readOnly: el.readOnly, value: el.value })).catch((e) => ({ error: e.message }));
  const accountInputs = await page.evaluate(() => Array.from(document.querySelectorAll('input,textarea'))
    .map((el) => ({ name: el.name, type: el.type, readOnly: el.readOnly, value: el.value }))
    .filter((x) => x.name));
  ok(
    '12.2-account-email-readonly',
    emailReadonly.readOnly === true,
    'Account email is readOnly as expected',
    'Account email can be edited or selector missing',
    { emailReadonly, inputNames: accountInputs.map((x) => x.name) },
  );

  const partParent = filters.categories.find((c) => !parentIdOf(c) && norm(`${c.name} ${c.slug}`).includes('phu tung'));
  const motoParent = filters.categories.find((c) => !parentIdOf(c) && norm(`${c.name} ${c.slug}`).includes('xe may'));
  const motoChild = filters.categories.find((c) => motoParent && String(parentIdOf(c)) === String(motoParent.id));
  const compatible = filters.partCompatibleTypes?.[0];

  await goto(page, '/products');
  await page.waitForSelector('aside select', { timeout: 10000 }).catch(() => {});
  if (partParent && compatible) {
    await page.locator('aside select').nth(0).selectOption(String(partParent.id));
    await page.waitForTimeout(600);
    const selectCount = await page.locator('aside select').count();
    await page.locator('aside select').nth(1).selectOption(String(compatible.id)).catch(() => {});
    await page.waitForTimeout(600);
    ok(
      '12.2-filter-part-compatible',
      page.url().includes(`compatibleCarModelId=${compatible.id}`),
      'Part category shows compatible vehicle selector and writes URL param',
      'Part compatible vehicle selector missing or URL not updated',
      { partParent, compatible, selectCount, url: page.url() },
    );
  } else {
    block('12.2-filter-part-compatible', 'Seed lacks part parent category or compatible model', { partParent, compatible });
  }

  if (motoParent && motoChild) {
    await page.locator('aside select').nth(0).selectOption(String(motoParent.id));
    await page.waitForTimeout(600);
    await page.locator('aside select').nth(1).selectOption(String(motoChild.id)).catch(() => {});
    await page.waitForTimeout(600);
    ok(
      '12.2-filter-motorcycle-type',
      page.url().includes(`vehicleTypeCategoryId=${motoChild.id}`),
      'Motorcycle category shows vehicle type selector and writes URL param',
      'Motorcycle vehicle type selector missing or URL not updated',
      { motoParent, motoChild, url: page.url() },
    );
  } else {
    block('12.2-filter-motorcycle-type', 'Seed lacks motorcycle parent/child category', { motoParent, motoChild });
  }

  await goto(page, '/');
  const newsletter = await page.evaluate(async () => {
    const form = document.querySelector('footer form');
    const input = form?.querySelector('input[type=email]');
    if (input && form) {
      input.value = 'section12-newsletter@example.com';
      input.dispatchEvent(new Event('input', { bubbles: true }));
      form.requestSubmit();
      await new Promise((resolve) => setTimeout(resolve, 100));
    }

    const feedbackText = form?.parentElement?.innerText || '';
    return {
      form: !!form,
      input: !!input,
      required: input?.required || false,
      action: form?.getAttribute('action') || '',
      method: form?.getAttribute('method') || '',
      feedback: feedbackText.includes('Cảm ơn bạn') || feedbackText.includes('Vui lòng nhập email hợp lệ'),
    };
  });
  ok(
    '12.2-footer-newsletter',
    newsletter.form && newsletter.input && newsletter.required && newsletter.feedback,
    'Footer newsletter validates email and shows local feedback',
    'Footer newsletter missing required validation or submit feedback',
    newsletter,
  );

  await clearCart(customer.token);
  await addCart(customer.token, 19, 2);
  await goto(page, '/checkout');
  await page.waitForSelector('input[name=shippingFullName]', { timeout: 15000 });
  await screenshot(page, '12-2-checkout-prefill');
  const checkoutPrefill = await page.evaluate(() => Object.fromEntries(Array.from(document.querySelectorAll('#checkout-form input, #checkout-form textarea')).map((el) => [el.name || el.id, el.value])));
  ok(
    '12.2-checkout-prefill-profile',
    !!(checkoutPrefill.shippingFullName && checkoutPrefill.shippingPhoneNumber && checkoutPrefill.shippingEmail && checkoutPrefill.shippingAddressLine),
    'Checkout prefilled profile/address fields',
    'Checkout profile/address prefill incomplete',
    checkoutPrefill,
  );

  await page.locator('input[name=orderType][value=Deposit]').check({ force: true });
  await page.waitForSelector('input[name=depositAmount]', { timeout: 5000 });
  const beforeOrders = (await api('/orders/mine', { token: customer.token })).data?.items?.length || 0;
  const depositCases = [
    { v: '0', name: 'zero' },
    { v: '-1', name: 'negative' },
    { v: 'abc', name: 'letters' },
    { v: '780000', name: 'equal-total' },
    { v: '999999999', name: 'larger-total' },
  ];
  const depositEvidence = [];
  for (const c of depositCases) {
    await page.locator('input[name=depositAmount]').fill(c.v).catch(() => {});
    await page.locator('button[type=submit][form=checkout-form]').click();
    await page.waitForTimeout(450);
    const after = (await api('/orders/mine', { token: customer.token })).data?.items?.length || 0;
    const val = await page.locator('input[name=depositAmount]').inputValue().catch(() => null);
    const stillCheckout = page.url().includes('/checkout');
    depositEvidence.push({ case: c.name, input: c.v, actualInputValue: val, orderCountBefore: beforeOrders, orderCountAfter: after, stillCheckout });
  }
  ok(
    '12.2-checkout-deposit-invalid-cases',
    depositEvidence.every((x) => x.orderCountAfter === beforeOrders && x.stillCheckout),
    'Invalid deposit cases blocked without creating orders',
    'Some invalid deposit case created order or left checkout',
    { depositEvidence },
  );

  await clearCart(customer.token);
  await addCart(customer.token, 19, 1);
  await goto(page, '/checkout');
  await page.waitForSelector('input[name=shippingFullName]', { timeout: 15000 });
  await page.locator('input[name=receivingMethod][value=Pickup]').check({ force: true });
  await page.locator('input[name=orderType][value=FullPayment]').check({ force: true });
  await page.locator('input[name=paymentMethod][value=COD]').check({ force: true });
  await page.locator('input[name=pickupAppointmentAt]').fill('2020-01-01T09:30').catch(() => {});
  await page.locator('textarea[name=fulfillmentNote]').fill('section12 fulfillment note').catch(() => {});
  await page.locator('textarea[name=note]').fill('section12 order note').catch(() => {});
  await page.locator('button[type=submit][form=checkout-form]').click();
  await page.waitForURL(/checkout\/success|checkout/, { timeout: 15000 }).catch(() => {});
  const successUrl = page.url();
  const pickupOrderId = new URL(successUrl).searchParams.get('orderId');
  if (pickupOrderId) createdOrders.push(Number(pickupOrderId));
  const pickupDetail = pickupOrderId ? (await api(`/orders/${pickupOrderId}`, { token: customer.token })).data : null;
  ok(
    '12.2-pickup-past-date-validation',
    !pickupOrderId,
    'Past pickup date blocked before order creation',
    'Past pickup date accepted and created order',
    {
      orderId: pickupOrderId,
      url: successUrl,
      receivingMethod: pickupDetail?.receivingMethod,
      note: pickupDetail?.note,
      hasPickupAppointmentAt: Object.keys(pickupDetail || {}).some((k) => k.toLowerCase().includes('pickup')),
      hasFulfillmentNote: Object.keys(pickupDetail || {}).some((k) => k.toLowerCase().includes('fulfillmentnote')),
    },
  );
  if (pickupDetail) {
    ok(
      '12.2-fulfillment-note-persistence-on-past-date-order',
      String(pickupDetail.note || '').includes('section12 order note') && JSON.stringify(pickupDetail).includes('section12 fulfillment note'),
      'Past-date pickup order persisted both notes distinctly',
      'Past-date pickup order was created but fulfillmentNote/pickupAppointmentAt were not represented',
      { orderId: pickupOrderId, note: pickupDetail?.note, keys: Object.keys(pickupDetail || {}).filter((k) => /note|pickup|appointment|fulfillment/i.test(k)) },
    );
  } else {
    rec('12.2-fulfillment-note-persistence-on-past-date-order', 'INFO', 'Skipped persistence check for past-date order because order creation was correctly blocked.', { orderId: pickupOrderId });
  }
  if (pickupOrderId) {
    rec('12.2-pickup-order-cleanup', 'INFO', 'Cleanup cancel attempted for pickup test order', await safeCancel(customer.token, pickupOrderId));
  }

  await clearCart(customer.token);
  await addCart(customer.token, 19, 1);
  const validPickupCtx = await authContext(browser, customer);
  const validPickupPage = await validPickupCtx.newPage();
  await goto(validPickupPage, '/checkout', { wait: 2000 });
  await validPickupPage.waitForSelector('input[name=shippingFullName]', { timeout: 15000 });
  await validPickupPage.locator('input[name=receivingMethod][value=Pickup]').check({ force: true });
  await validPickupPage.locator('input[name=orderType][value=FullPayment]').check({ force: true });
  await validPickupPage.locator('input[name=paymentMethod][value=COD]').check({ force: true });
  await validPickupPage.locator('input[name=pickupAppointmentAt]').fill('2026-12-01T09:30').catch(() => {});
  await validPickupPage.locator('textarea[name=fulfillmentNote]').fill('section12 valid fulfillment note').catch(() => {});
  await validPickupPage.locator('textarea[name=note]').fill('section12 valid order note').catch(() => {});
  await validPickupPage.locator('button[type=submit][form=checkout-form]').click();
  await validPickupPage.waitForURL(/checkout\/success|checkout/, { timeout: 15000 }).catch(() => {});
  await screenshot(validPickupPage, '12-2-valid-pickup-submit');
  const validPickupOrderId = new URL(validPickupPage.url()).searchParams.get('orderId');
  if (validPickupOrderId) createdOrders.push(Number(validPickupOrderId));
  const validPickupDetail = validPickupOrderId ? (await api(`/orders/${validPickupOrderId}`, { token: customer.token })).data : null;
  ok(
    '12.2-fulfillment-note-valid-pickup-persistence',
    validPickupDetail && String(validPickupDetail.note || '').includes('section12 valid order note') && JSON.stringify(validPickupDetail).includes('section12 valid fulfillment note') && JSON.stringify(validPickupDetail).includes('2026-12-01'),
    'Valid pickup order persisted order note, fulfillment note and appointment distinctly',
    'Valid pickup order did not persist fulfillmentNote/pickupAppointmentAt distinctly',
    { orderId: validPickupOrderId, note: validPickupDetail?.note, keys: Object.keys(validPickupDetail || {}).filter((k) => /note|pickup|appointment|fulfillment/i.test(k)) },
  );
  if (validPickupOrderId) {
    rec('12.2-valid-pickup-order-cleanup', 'INFO', 'Cleanup cancel attempted for valid pickup test order', await safeCancel(customer.token, validPickupOrderId));
  }
  await validPickupCtx.close();

  await clearCart(customer.token);
  await addCart(customer.token, 19, 2);
  const validateAmount = await api('/vouchers/validate', { method: 'POST', token: customer.token, body: { code: 'STOREAMT20260604174344', subtotal: 780000 } });
  const validatePercent = await api('/vouchers/validate', { method: 'POST', token: customer.token, body: { code: 'PHUTUNG10', subtotal: 780000 } });
  ok(
    '12.4-voucher-amount-percent-api',
    validateAmount.data?.valid === true && money(validateAmount.data?.discountAmount) === 20000 && validatePercent.data?.valid === true && money(validatePercent.data?.discountAmount) === 78000,
    'Amount and percent vouchers calculate on current cart subtotal',
    'Voucher amount/percent calculation wrong',
    { amount: validateAmount.data, percent: validatePercent.data },
  );

  await goto(page, '/checkout');
  await page.waitForSelector('aside input[type=text]', { timeout: 10000 });
  await page.locator('aside input[type=text]').fill('STOREAMT20260604174344');
  await page.locator('aside input[type=text]').locator('xpath=following-sibling::button').click().catch(async () => {
    await page.locator('aside button').last().click();
  });
  await page.waitForTimeout(800);
  const voucherUi = await page.evaluate(() => document.body.innerText.includes('STOREAMT20260604174344'));
  ok('12.4-voucher-apply-ui', voucherUi, 'Voucher applied and visible in checkout summary', 'Voucher apply not visible in checkout summary', { code: 'STOREAMT20260604174344' });

  await page.locator('input[name=paymentMethod][value=BankTransfer]').check({ force: true });
  await page.locator('button[type=submit][form=checkout-form]').click();
  await page.waitForTimeout(2500);
  await screenshot(page, '12-4-bank-transfer-qr');
  const qrState = await page.evaluate(() => ({
    dialog: !!document.querySelector('[role=dialog]'),
    qr: !!document.querySelector('[role=dialog] img'),
    text: document.querySelector('[role=dialog]')?.innerText || '',
  }));
  ok(
    '12.4-bank-transfer-qr-ui',
    qrState.dialog === true,
    'Bank transfer submit opens QR/payment dialog',
    'Bank transfer submit did not open QR/payment dialog',
    { dialog: qrState.dialog, qr: qrState.qr, textSample: qrState.text.slice(0, 300) },
  );
  await page.locator('[role=dialog] button').last().click().catch(() => {});
  await page.waitForURL(/checkout\/success/, { timeout: 10000 }).catch(() => {});
  const qrOrderId = new URL(page.url()).searchParams.get('orderId');
  if (qrOrderId) createdOrders.push(Number(qrOrderId));
  const qrOrder = qrOrderId ? (await api(`/orders/${qrOrderId}`, { token: customer.token })).data : null;
  ok(
    '12.5-money-crosscheck-checkout-order',
    qrOrder && money(qrOrder.subtotal) === 780000 && money(qrOrder.discountTotal ?? qrOrder.discountAmount) === 20000 && money(qrOrder.grandTotal ?? qrOrder.totalAmount) === 760000,
    'Created transfer order financial values match API/cart/voucher',
    'Created transfer order financial values mismatch',
    { orderId: qrOrderId, subtotal: qrOrder?.subtotal, discountTotal: qrOrder?.discountTotal, grandTotal: qrOrder?.grandTotal, voucherCode: qrOrder?.vouchers?.[0]?.voucherCodeSnapshot },
  );
  if (qrOrderId) {
    rec('12.4-bank-transfer-order-cleanup', 'INFO', 'Cleanup cancel attempted for transfer order', await safeCancel(customer.token, qrOrderId));
  }

  await clearCart(customer.token);
  await goto(page, '/cart');
  const emptyCart = await page.evaluate(() => ({
    buttons: Array.from(document.querySelectorAll('button')).map((b) => ({ disabled: b.disabled, text: b.innerText })),
    links: Array.from(document.querySelectorAll('a')).map((a) => a.getAttribute('href')),
  }));
  ok(
    '12.3-cart-empty-checkout',
    !emptyCart.buttons.some((b) => /thanh/i.test(norm(b.text)) && !b.disabled) && emptyCart.links.includes('/products'),
    'Empty cart hides/blocks checkout and exposes shopping CTA',
    'Empty cart still has enabled checkout or missing CTA',
    emptyCart,
  );

  await goto(page, '/products/10');
  await page.waitForSelector('.gallery-main-image', { timeout: 12000 }).catch(() => {});
  await screenshot(page, '12-3-product-detail');
  const galleryBefore = await page.evaluate(() => ({
    thumbs: document.querySelectorAll('.gallery-thumb').length,
    hasNext: !!document.querySelector('.gallery-nav-next'),
    active: Array.from(document.querySelectorAll('.gallery-thumb')).findIndex((b) => b.className.includes('active')),
  }));
  if (galleryBefore.thumbs > 1) {
    await page.locator('.gallery-nav-next').click().catch(() => {});
    await page.waitForTimeout(300);
    const afterNext = await page.evaluate(() => Array.from(document.querySelectorAll('.gallery-thumb')).findIndex((b) => b.className.includes('active')));
    await page.locator('.gallery-thumb').nth(0).click().catch(() => {});
    await page.waitForTimeout(300);
    const afterThumb = await page.evaluate(() => Array.from(document.querySelectorAll('.gallery-thumb')).findIndex((b) => b.className.includes('active')));
    ok(
      '12.3-product-gallery-buttons',
      afterNext !== galleryBefore.active && afterThumb === 0,
      'Gallery next/thumb buttons change active image',
      'Gallery buttons/thumbs did not update active image',
      { galleryBefore, afterNext, afterThumb },
    );
  } else {
    warn('12.3-product-gallery-buttons', 'Product has one/fallback image only; nav unavailable for full prev/next test', galleryBefore);
  }

  await clearCart(customer.token);
  const beforeCart = await cartState(customer.token);
  await page.locator('button[aria-pressed]').first().click().catch(() => {});
  await page.waitForTimeout(500);
  const addButtons = await page.locator('button').evaluateAll((buttons) => buttons.map((button, index) => ({ index, text: button.innerText, disabled: button.disabled })));
  const addButton = addButtons.find((button) => norm(button.text).includes('them vao gio') && !button.disabled);
  if (addButton) {
    await page.locator('button').nth(addButton.index).click();
  }
  await page.waitForTimeout(800);
  const afterCart = await cartState(customer.token);
  ok(
    '12.3-product-detail-favorite-addcart',
    addButton && (afterCart.totalItems || 0) > (beforeCart.totalItems || 0),
    'Product detail favorite/add-cart buttons clickable and cart increased',
    'Product detail primary buttons failed or cart did not increase',
    { beforeTotal: beforeCart.totalItems, afterTotal: afterCart.totalItems, addButtons },
  );

  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(600);
  const reviewForm = await page.evaluate(() => ({
    textarea: !!document.querySelector('textarea'),
    starButtons: Array.from(document.querySelectorAll('button[aria-label$="sao"]')).length,
    submitButtons: Array.from(document.querySelectorAll('button')).filter((b) => {
      const text = String(b.innerText || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/[đĐ]/g, 'd').toLowerCase();
      return /gui|cap nhat/i.test(text);
    }).length,
  }));
  ok(
    '12.2-product-detail-review-form-visible',
    reviewForm.textarea && reviewForm.starButtons >= 5,
    'Product detail review form/star controls visible for eligible customer',
    'Product detail review form/star controls missing',
    reviewForm,
  );

  const relatedCards = await page.evaluate(() => Array.from(document.querySelectorAll('a[href^="/products/"]')).map((a) => a.getAttribute('href')).filter((h) => h !== '/products/10'));
  if (relatedCards.length) {
    ok('12.3-related-products', true, 'Related/viewed product links exist and are clickable candidates', '', { relatedCards: relatedCards.slice(0, 5) });
  } else {
    warn('12.3-related-products', 'No related product card rendered for current seed/detail', { relatedCards });
  }

  await goto(page, '/he-thong-cua-hang');
  await screenshot(page, '12-3-store-system');
  const storeInfo = await page.evaluate(() => ({
    cards: document.querySelectorAll('article').length,
    iframe: document.querySelector('iframe')?.src || '',
    telLinks: Array.from(document.querySelectorAll('a[href^="tel:"]')).map((a) => a.href),
    buttons: Array.from(document.querySelectorAll('button')).map((b) => b.innerText),
  }));
  if (storeInfo.cards) await page.locator('article button').first().click().catch(() => {});
  ok(
    '12.3-store-card-map-links',
    storeInfo.cards >= 1 && !!storeInfo.iframe && storeInfo.telLinks.length >= 1,
    'Store system renders store card, map iframe and tel link',
    'Store system missing card/map/tel',
    storeInfo,
  );

  const errCtx = await authContext(browser, customer);
  const errPage = await errCtx.newPage();
  await errPage.route('**/api/showrooms', (route) => route.fulfill({ status: 500, contentType: 'application/json', body: '{"message":"forced"}' }));
  await goto(errPage, '/he-thong-cua-hang');
  const storeRetry = await errPage.evaluate(() => ({ text: document.body.innerText, buttons: Array.from(document.querySelectorAll('button')).map((b) => b.innerText) }));
  ok(
    '12.3-store-error-retry',
    storeRetry.buttons.length > 0 && norm(storeRetry.text).includes('api /api/showrooms'),
    'Store API error renders retry/error state',
    'Store API error did not render retry/error state',
    { buttons: storeRetry.buttons },
  );
  await errCtx.close();

  const ordErrCtx = await authContext(browser, customer);
  const op = await ordErrCtx.newPage();
  await op.route('**/api/orders/mine', (route) => route.fulfill({ status: 500, contentType: 'application/json', body: '{"message":"forced orders"}' }));
  await goto(op, '/orders');
  const ordersRetry = await op.evaluate(() => ({ text: document.body.innerText, buttons: Array.from(document.querySelectorAll('button')).map((b) => b.innerText) }));
  ok(
    '12.3-orders-error-retry',
    ordersRetry.buttons.length > 0 && norm(ordersRetry.text).includes('forced'),
    'Orders API error renders retry state',
    'Orders API error did not render retry state',
    { buttons: ordersRetry.buttons },
  );
  await ordErrCtx.close();

  const detErrCtx = await authContext(browser, customer);
  const dp = await detErrCtx.newPage();
  await dp.route('**/api/orders/73', (route) => route.fulfill({ status: 500, contentType: 'application/json', body: '{"message":"forced order detail"}' }));
  await goto(dp, '/orders/73');
  const detailRetry = await dp.evaluate(() => ({ text: document.body.innerText, buttons: Array.from(document.querySelectorAll('button')).map((b) => b.innerText) }));
  ok(
    '12.3-order-detail-error-retry',
    detailRetry.buttons.length > 0 && norm(detailRetry.text).includes('forced'),
    'Order detail API error renders retry state',
    'Order detail API error did not render retry state',
    { buttons: detailRetry.buttons },
  );
  await detErrCtx.close();

  await goto(page, '/orders/73');
  await screenshot(page, '12-4-order-detail');
  const orderDetail = await bodyInfo(page);
  const orderDetailData = (await api('/orders/73', { token: customer.token })).data;
  const histories = orderDetailData?.histories || orderDetailData?.history || [];
  ok(
    '12.4-order-history-status-ui',
    Array.isArray(histories) && histories.length > 1 && /history|timeline|lich su don hang/i.test(norm(orderDetail.text)),
    'Order detail shows detailed status history/timeline',
    'Order detail API has history but store UI does not show an order-history/timeline section',
    { apiHistoryCount: histories.length, renderedTextSample: orderDetail.text.slice(0, 500) },
  );

  const modalButton = await page.locator('button').evaluateAll((buttons) => {
    const normalize = (value) => String(value || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/[đĐ]/g, 'd').toLowerCase();
    const item = buttons
      .map((button, index) => ({ index, text: button.innerText, disabled: button.disabled }))
      .find((button) => normalize(button.text).includes('danh gia san pham') && !button.disabled);
    return item || null;
  });
  if (modalButton) {
    await page.locator('button').nth(modalButton.index).click();
  }
  await page.waitForTimeout(500);
  const modalBeforeClose = await page.locator('.fixed.inset-0').count();
  if (modalBeforeClose) await page.locator('.fixed.inset-0 button').first().click().catch(() => {});
  await page.waitForTimeout(300);
  ok(
    '12.3-review-modal-close',
    modalBeforeClose > 0 ? (await page.locator('.fixed.inset-0').count()) === 0 : false,
    'Review modal can be opened and closed by X/first modal button',
    'Review modal not opened/closed in order detail UI',
    { modalButton, modalBeforeClose },
  );

  const otherOrder = await api('/orders/73', { token: other.token });
  ok(
    '12.4-multi-user-order-isolation',
    otherOrder.status === 404 || otherOrder.status === 403,
    'Other customer cannot fetch order 73',
    'Other customer can fetch another user order',
    { status: otherOrder.status, body: otherOrder.data },
  );

  const authTest = await browser.newContext({ viewport: { width: 1366, height: 768 } });
  const lp = await authTest.newPage();
  await goto(lp, '/login');
  await lp.locator('input[name=username]').fill('store_e2e_20260604174344@motosale.local');
  await lp.locator('input[name=password]').fill('Store@12345');
  await lp.locator('input[name=rememberMe]').check({ force: true });
  await lp.locator('form').first().locator('button[type=submit]').click();
  await lp.waitForURL(/127\.0\.0\.1:5174\/$/, { timeout: 15000 }).catch(() => {});
  const storageRemember = await lp.evaluate(() => ({ localToken: !!localStorage.getItem('token'), sessionToken: !!sessionStorage.getItem('token') }));
  ok(
    '12.4-auth-remember-storage',
    storageRemember.localToken && !storageRemember.sessionToken,
    'Remember me stores token in localStorage only',
    'Remember me storage location wrong',
    storageRemember,
  );
  await lp.evaluate(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
  });
  await goto(lp, '/account');
  ok('12.4-auth-token-missing-redirect', lp.url().includes('/login'), 'Missing token redirects protected page to login', 'Missing token did not redirect protected page', { url: lp.url() });
  await authTest.close();

  await clearCart(customer.token);
  await addCart(customer.token, 19, 1);
  for (const route of ['/account', '/cart', '/checkout', '/orders/73', '/products/10']) {
    const rc = await authContext(browser, customer, { width: 390, height: 844 });
    const rp = await rc.newPage();
    await goto(rp, route, { wait: 700 });
    const info = await bodyInfo(rp);
    ok(
      `12.5-responsive-overflow-${route.replace(/[^a-z0-9]/gi, '_')}`,
      info.scrollW <= info.clientW + 2,
      'No horizontal overflow on mobile',
      'Horizontal overflow detected',
      { route, scrollW: info.scrollW, clientW: info.clientW, url: rp.url() },
    );
    await rc.close();
  }

  const mineBefore = await api('/reviews/product/10/me', { token: customer.token });
  if (mineBefore.data?.myReview?.id) {
    await api(`/reviews/${mineBefore.data.myReview.id}`, { method: 'DELETE', token: admin.token });
    rec('12.5-review-precleanup', 'INFO', 'Deleted pre-existing review to allow create test', { id: mineBefore.data.myReview.id });
  }
  const createReview = await api('/products/10/reviews', { method: 'POST', token: customer.token, body: { rating: 5, title: 'section12 review', comment: 'section12 review content', orderId: 73 } });
  const reviewId = createReview.data?.review?.id || createReview.data?.review?.Id;
  const repeat = await api('/products/10/reviews', { method: 'POST', token: customer.token, body: { rating: 4, title: 'section12 repeat', comment: 'repeat', orderId: 73 } });
  const publicPending = await api('/products/10/reviews');
  const pendingVisible = JSON.stringify(publicPending.data).includes('section12 review content');
  const approve = reviewId ? await api(`/reviews/${reviewId}/status`, { method: 'PATCH', token: admin.token, body: { status: 'Approved' } }) : { ok: false, status: 0 };
  const publicApproved = await api('/products/10/reviews');
  const approvedVisible = JSON.stringify(publicApproved.data).includes('section12 review content');
  const hide = reviewId ? await api(`/reviews/${reviewId}/status`, { method: 'PATCH', token: admin.token, body: { status: 'Hidden' } }) : { ok: false, status: 0 };
  const publicHidden = await api('/products/10/reviews');
  const hiddenVisible = JSON.stringify(publicHidden.data).includes('section12 review content');
  ok(
    '12.4-review-repeat-moderation-api',
    createReview.ok && !repeat.ok && !pendingVisible && approve.ok && approvedVisible && hide.ok && !hiddenVisible,
    'Review create/repeat block/pending-approve-hide flow works at API level',
    'Review moderation or repeat logic failed',
    { createStatus: createReview.status, reviewId, repeatStatus: repeat.status, pendingVisible, approveStatus: approve.status, approvedVisible, hideStatus: hide.status, hiddenVisible },
  );
  if (reviewId) {
    const del = await api(`/reviews/${reviewId}`, { method: 'DELETE', token: admin.token });
    rec('12.5-review-cleanup', del.ok ? 'INFO' : 'WARN', 'Review cleanup attempted', { reviewId, status: del.status, ok: del.ok });
  }

  block(
    '12.4-product-hidden-out-of-stock-during-checkout',
    'Not executed destructively in this run because it requires changing live product/stock state; full UI race test needs disposable product/stock fixture with guaranteed restore.',
    { productId: 10, skuId: 19 },
  );
  warn('12.4-shipping-fee', 'Current store checkout hardcodes shippingFee=0; no non-zero shipping fee business path to test.', { shippingFeeHardcoded: true });
  warn('12.4-voucher-cart-change-after-apply', 'Checkout has no quantity controls and applied voucher is local checkout state; editing cart after applying voucher resets voucher rather than revalidating within checkout.', {});

  await ac.close();
} finally {
  await browser.close();
}

await clearCart(customer.token).catch(() => {});
for (const orderId of createdOrders) {
  // eslint-disable-next-line no-await-in-loop
  await safeCancel(customer.token, orderId, 'section12 final cleanup').catch(() => {});
}

const finalCart = await cartState(customer.token).catch((e) => ({ error: e.message }));
const finalReviews = await api('/products/10/reviews');
ok(
  '12.5-cleanup-smoke',
  (finalCart.totalItems || 0) === 0 && !JSON.stringify(finalReviews.data).includes('section12 review content'),
  'Cleanup smoke OK: cart empty and test review not public',
  'Cleanup smoke found leftover cart/review data',
  { finalCart, reviewTextFound: JSON.stringify(finalReviews.data).includes('section12 review content'), createdOrders },
);

const summary = results.reduce((acc, r) => {
  acc[r.status] = (acc[r.status] || 0) + 1;
  return acc;
}, {});
const payload = { generatedAt: new Date().toISOString(), artifactDir: ART, summary, results };
console.log(JSON.stringify(payload, null, 2));
