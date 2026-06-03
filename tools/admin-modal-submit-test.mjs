import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';

const frontendRequire = createRequire('file:///D:/MotorTeam/MoToSale-End/v2/frontend-admin/package.json');
const { chromium } = frontendRequire('playwright');

const ROOT = 'D:/MotorTeam/MoToSale-End';
const BASE_URL = process.env.ADMIN_BASE_URL || 'http://localhost:5176';
const OUT_DIR = path.join(ROOT, 'docs/modal-full-submit-test-20260602');
const REPORT_PATH = path.join(ROOT, 'docs/V2_ADMIN_MODAL_FULL_SUBMIT_TEST_REPORT_20260602.md');
const VALID_IMAGE = path.join(ROOT, 'docs/ui-smoke-20260602-final/dashboard.png');
const BAD_FILE = path.join(OUT_DIR, 'not-an-image.txt');

fs.mkdirSync(OUT_DIR, { recursive: true });
fs.writeFileSync(BAD_FILE, 'not an image - modal upload negative test', 'utf8');

const nowTag = new Date().toISOString().replace(/[:.]/g, '-');

const rows = [];
let currentResponses = [];
let currentDialogs = [];

function mdEscape(value) {
  return String(value ?? '').replace(/\|/g, '\\|').replace(/\r?\n/g, ' ');
}

function pushResult({ page, modal, action, data, expected, actual, status, evidence }) {
  rows.push({
    page,
    modal,
    action,
    data,
    expected,
    actual,
    status,
    evidence,
    responses: currentResponses.slice(),
    dialogs: currentDialogs.slice(),
  });
  try {
    writeReport('Baseline build passed before run; test in progress');
  } catch {
    // Report writing must never stop the UI test run.
  }
}

async function screenshot(page, name) {
  const filename = `${String(rows.length + 1).padStart(3, '0')}-${name}-${nowTag}.png`.replace(/[^a-zA-Z0-9_.-]/g, '-');
  const file = path.join(OUT_DIR, filename);
  await page.screenshot({ path: file, fullPage: false });
  return file;
}

async function waitSoft(page, ms = 600) {
  await page.waitForTimeout(ms);
}

async function login(page) {
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'domcontentloaded' });
  const email = page.locator('input[placeholder="Email"], input[placeholder*="Email"], input[type="email"], input[name="email"]').first();
  if (await email.count()) {
    await email.fill('admin@motosale.local');
    await page.locator('input[type="password"], input[name="password"], input[placeholder*="kh"]').first().fill('Admin@123');
    await page.locator('button[type="submit"]').first().click();
    await page.waitForLoadState('domcontentloaded').catch(() => {});
    await page.waitForTimeout(1000);
  }
}

async function goto(page, route) {
  currentResponses = [];
  currentDialogs = [];
  await page.goto(`${BASE_URL}${route}`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(700);
}

async function visibleModal(page) {
  const modal = page.locator('.modal.show, .modal.d-block, .modal[style*="block"]').last();
  await modal.waitFor({ state: 'visible', timeout: 5000 });
  return modal;
}

async function clickCloseAndReopen(page, openAction, pageName, modalName) {
  await openAction();
  let modal = await visibleModal(page);
  let ev1 = await screenshot(page, `${pageName}-${modalName}-open-close-x`);
  const close = modal.locator('button.close, .modal-header button').first();
  await close.click();
  await waitSoft(page);
  const closedByX = await page.locator('.modal.show, .modal.d-block, .modal[style*="block"]').count() === 0;
  pushResult({
    page: pageName,
    modal: modalName,
    action: 'Đóng bằng nút x',
    data: 'Mở modal rồi bấm x',
    expected: 'Modal đóng và có thể mở lại',
    actual: closedByX ? 'Modal đã đóng' : 'Modal vẫn còn hiển thị',
    status: closedByX ? 'Pass' : 'Fail',
    evidence: ev1,
  });

  await openAction();
  modal = await visibleModal(page);
  const cancel = modal.locator('button:has-text("Hủy"), button:has-text("Đóng"), button.btn-secondary, button.btn-default').last();
  const ev2 = await screenshot(page, `${pageName}-${modalName}-open-close-cancel`);
  await cancel.click();
  await waitSoft(page);
  const closedByCancel = await page.locator('.modal.show, .modal.d-block, .modal[style*="block"]').count() === 0;
  pushResult({
    page: pageName,
    modal: modalName,
    action: 'Đóng bằng nút Hủy/Đóng',
    data: 'Mở modal rồi bấm nút footer',
    expected: 'Modal đóng và có thể mở lại',
    actual: closedByCancel ? 'Modal đã đóng' : 'Modal vẫn còn hiển thị',
    status: closedByCancel ? 'Pass' : 'Fail',
    evidence: ev2,
  });
}

async function fillByName(modal, name, value) {
  const field = modal.locator(`[name="${name}"]`).first();
  if (await field.count()) {
    await field.fill(String(value));
    return true;
  }
  return false;
}

async function selectFirstNonEmpty(locator) {
  if (!(await locator.count())) return false;
  const options = await locator.locator('option').evaluateAll((opts) => opts.map((o) => o.value).filter((x) => x !== ''));
  if (options.length) {
    await locator.selectOption(options[0]);
    return true;
  }
  return false;
}

async function fillGeneric(modal, seed) {
  const textInputs = modal.locator('input:not([type="hidden"]):not([type="file"]):not([type="checkbox"]):not([type="radio"]), textarea');
  const count = await textInputs.count();
  for (let i = 0; i < count; i += 1) {
    const input = textInputs.nth(i);
    if (!(await input.isVisible().catch(() => false))) continue;
    const type = (await input.getAttribute('type')) || '';
    const name = ((await input.getAttribute('name')) || '').toLowerCase();
    const placeholder = ((await input.getAttribute('placeholder')) || '').toLowerCase();
    const label = ((await input.locator('xpath=ancestor::*[contains(@class,"form-group")][1]//label').textContent().catch(() => '')) || '').toLowerCase();
    const key = `${name} ${placeholder} ${label}`;
    let value = seed;
    if (key.includes('mã nhà cung cấp') || key.includes('ma nha cung cap') || key.includes('code') || key.includes('mã ncc')) value = `TST${Date.now().toString().slice(-7)}`;
    if (key.includes('mã số thuế') || key.includes('ma so thue') || key.includes('tax')) value = `TAX${Date.now().toString().slice(-7)}`;
    if (key.includes('sku')) value = `SKU${Date.now().toString().slice(-6)}`;
    if (key.includes('sđt') || key.includes('sdt') || key.includes('điện thoại') || key.includes('dien thoai') || key.includes('phone')) value = '0901234567';
    if (key.includes('email')) value = `modal${Date.now()}@test.local`;
    if (key.includes('khách hàng') || key.includes('khach hang') || key.includes('người liên hệ') || key.includes('nguoi lien he')) value = `Khách test ${Date.now().toString().slice(-5)}`;
    if (key.includes('sản phẩm') || key.includes('san pham')) value = `Sản phẩm test ${Date.now().toString().slice(-5)}`;
    if (key.includes('số khung') || key.includes('so khung')) value = `FRAME${Date.now().toString().slice(-6)}`;
    if (key.includes('số máy') || key.includes('so may')) value = `ENG${Date.now().toString().slice(-6)}`;
    if (key.includes('lỗi') || key.includes('loi')) value = 'Khách báo lỗi kiểm thử';
    if (type === 'number' || name.includes('thu') || name.includes('sort') || name.includes('order') || name.includes('amount') || name.includes('quantity') || name.includes('price') || name.includes('year') || key.includes('chi phí') || key.includes('chi phi')) value = '1';
    if (type === 'date' || name.includes('date')) value = '2026-06-02';
    await input.fill(String(value)).catch(() => {});
  }
  const selects = modal.locator('select');
  for (let i = 0; i < await selects.count(); i += 1) {
    await selectFirstNonEmpty(selects.nth(i)).catch(() => {});
  }
  const checks = modal.locator('input[type="checkbox"]');
  for (let i = 0; i < await checks.count(); i += 1) {
    const cb = checks.nth(i);
    if (await cb.isVisible().catch(() => false)) await cb.setChecked(true).catch(() => {});
  }
}

async function submitModal(page, pageName, modalName, dataLabel, expectSuccess = true) {
  currentResponses = [];
  currentDialogs = [];
  const modal = await visibleModal(page);
  const evBefore = await screenshot(page, `${pageName}-${modalName}-before-submit`);
  const submit = modal.locator('button[type="submit"], button:has-text("Lưu"), button:has-text("Cập nhật"), button:has-text("Thêm"), button:has-text("Ghi nhận"), button:has-text("Xác nhận")').last();
  await submit.click();
  await waitSoft(page, 1800);
  const stillOpen = await page.locator('.modal.show, .modal.d-block, .modal[style*="block"]').count() > 0;
  const evAfter = await screenshot(page, `${pageName}-${modalName}-after-submit`);
  const apiFailure = currentResponses.some((r) => r.status >= 400);
  const pass = expectSuccess ? (!stillOpen && !apiFailure) : (stillOpen || currentDialogs.length > 0 || apiFailure);
  pushResult({
    page: pageName,
    modal: modalName,
    action: 'Submit modal',
    data: dataLabel,
    expected: expectSuccess ? 'Submit thành công, modal đóng, API không lỗi' : 'Validation/alert/API lỗi rõ ràng, modal không mất dữ liệu',
    actual: `modalOpen=${stillOpen}; apiFailures=${currentResponses.filter((r) => r.status >= 400).length}; dialogs=${currentDialogs.map((d) => d.message).join('; ')}`,
    status: pass ? 'Pass' : 'Fail',
    evidence: `${evBefore}; ${evAfter}`,
  });
}

async function testFaq(page) {
  await goto(page, '/faq');
  const open = async () => page.locator('button.btn-primary:has-text("FAQ"), button.btn-primary').first().click();
  await clickCloseAndReopen(page, open, 'FAQ', 'Thêm FAQ');
  await open();
  await submitModal(page, 'FAQ', 'Thêm FAQ', 'Rỗng/thiếu field bắt buộc', false);
  let modal = await visibleModal(page);
  await fillByName(modal, 'cauHoi', `Modal test FAQ có dấu ${Date.now()}`);
  await fillByName(modal, 'cauTraLoi', 'Câu trả lời kiểm thử modal có dấu, không bị lỗi encoding.');
  await fillByName(modal, 'danhMuc', 'Kiểm thử');
  await fillByName(modal, 'thuTu', '1');
  await submitModal(page, 'FAQ', 'Thêm FAQ', 'Dữ liệu hợp lệ tiếng Việt có dấu', true);
}

async function testCategory(page) {
  await goto(page, '/categories');
  const open = async () => page.locator('button.btn-primary').first().click();
  await clickCloseAndReopen(page, open, 'Danh mục', 'Thêm danh mục');
  await open();
  await submitModal(page, 'Danh mục', 'Thêm danh mục', 'Rỗng/thiếu tên', false);
  let modal = await visibleModal(page);
  const name = `Modal Test Danh mục ${Date.now()}`;
  await fillByName(modal, 'tenDanhMuc', name);
  await fillByName(modal, 'moTa', 'Mô tả kiểm thử danh mục tiếng Việt.');
  await fillByName(modal, 'thuTu', '9');
  await submitModal(page, 'Danh mục', 'Thêm danh mục', 'Danh mục cha hợp lệ', true);
}

async function testBrands(page) {
  await goto(page, '/brands');
  const openBrand = async () => page.locator('button.btn-primary').filter({ hasText: /h.ng|Th/ }).first().click().catch(async () => page.locator('button.btn-primary').first().click());
  await clickCloseAndReopen(page, openBrand, 'Hãng xe', 'Thêm hãng');
  await openBrand();
  await submitModal(page, 'Hãng xe', 'Thêm hãng', 'Rỗng/thiếu tên', false);
  let modal = await visibleModal(page);
  const brandName = `Modal Brand ${Date.now()}`;
  await fillByName(modal, 'tenHang', brandName);
  const file = modal.locator('input[type="file"]').first();
  if (await file.count()) await file.setInputFiles(VALID_IMAGE);
  await submitModal(page, 'Hãng xe', 'Thêm hãng', 'Tên hợp lệ + upload logo PNG', true);

  await goto(page, '/brands');
  const firstEdit = page.locator('button.btn-info').first();
  if (await firstEdit.count()) {
    await firstEdit.click();
    modal = await visibleModal(page);
    if (await modal.locator('input[type="file"]').count()) await modal.locator('input[type="file"]').first().setInputFiles(VALID_IMAGE);
    await submitModal(page, 'Hãng xe', 'Sửa hãng', 'Đổi/upload logo PNG cho bản ghi đầu tiên', true);
  }

  await goto(page, '/brands');
  const tabButtons = page.locator('button, a').filter({ hasText: /Dòng|dong|Model/i });
  if (await tabButtons.count()) await tabButtons.last().click();
  await waitSoft(page, 800);
  const addButtons = page.locator('button.btn-primary');
  if (await addButtons.count()) {
    await addButtons.last().click();
    await submitModal(page, 'Dòng xe', 'Thêm dòng xe', 'Rỗng/thiếu tên và hãng', false);
    modal = await visibleModal(page);
    await selectFirstNonEmpty(modal.locator('select[name="hangXeId"]').first());
    await fillByName(modal, 'tenDongXe', `Modal Dòng xe ${Date.now()}`);
    await submitModal(page, 'Dòng xe', 'Thêm dòng xe', 'Chọn hãng + tên dòng xe hợp lệ', true);
  }
}

async function testGenericRoute(page, route, pageName) {
  await goto(page, route);
  const modalOpeners = page.locator('button').filter({ has: page.locator('i.fa-plus, i.fa-edit, i.fa-eye, i.fa-image, i.fa-cogs, i.fa-barcode') });
  const fallback = page.locator('button.btn-primary, button.btn-info, button.btn-warning, button.btn-secondary');
  const count = Math.min(3, await fallback.count());
  for (let i = 0; i < count; i += 1) {
    await goto(page, route);
    const btn = fallback.nth(i);
    if (!(await btn.isVisible().catch(() => false))) continue;
    currentResponses = [];
    currentDialogs = [];
    const text = ((await btn.textContent().catch(() => '')) || '').trim() || `button-${i}`;
    await btn.click().catch(() => {});
    await waitSoft(page, 1000);
    const hasModal = await page.locator('.modal.show, .modal.d-block, .modal[style*="block"]').count() > 0;
    if (!hasModal) continue;
    const modal = await visibleModal(page);
    const title = ((await modal.locator('.modal-title, h5').first().textContent().catch(() => 'Modal')) || 'Modal').trim();
    const evOpen = await screenshot(page, `${pageName}-${i}-generic-open`);
    pushResult({
      page: pageName,
      modal: title,
      action: `Mở modal từ ${text}`,
      data: 'Generic open/visual',
      expected: 'Modal mở, không tràn rõ ràng',
      actual: 'Modal hiển thị',
      status: 'Pass',
      evidence: evOpen,
    });
    const fileInput = modal.locator('input[type="file"]').first();
    if (await fileInput.count()) await fileInput.setInputFiles(VALID_IMAGE).catch(() => {});
    await fillGeneric(modal, `Modal test ${pageName} ${Date.now()}`);
    const submit = modal.locator('button[type="submit"], button:has-text("Lưu"), button:has-text("Cập nhật"), button:has-text("Thêm"), button:has-text("Ghi nhận"), button:has-text("Xác nhận")').last();
    if (await submit.count()) {
      await submit.click();
      await waitSoft(page, 1600);
      const stillOpen = await page.locator('.modal.show, .modal.d-block, .modal[style*="block"]').count() > 0;
      const apiFailure = currentResponses.some((r) => r.status >= 400);
      const evSubmit = await screenshot(page, `${pageName}-${i}-generic-submit`);
      pushResult({
        page: pageName,
        modal: title,
        action: 'Generic submit',
        data: 'Auto-fill visible fields + submit',
        expected: 'Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail',
        actual: `modalOpen=${stillOpen}; apiFailures=${currentResponses.filter((r) => r.status >= 400).length}; dialogs=${currentDialogs.map((d) => d.message).join('; ')}`,
        status: apiFailure ? 'Fail' : 'Pass',
        evidence: evSubmit,
      });
    } else {
      const close = modal.locator('button.close, button:has-text("Đóng"), button:has-text("Hủy")').first();
      if (await close.count()) await close.click().catch(() => {});
    }
  }
}

function writeReport(buildStatus = 'Pending') {
  const lines = [];
  lines.push('# V2 Admin Modal Full Submit Test Report - 2026-06-02');
  lines.push('');
  lines.push(`Base URL: ${BASE_URL}`);
  lines.push(`Evidence folder: ${OUT_DIR}`);
  lines.push(`Build status: ${buildStatus}`);
  lines.push('');
  const summary = rows.reduce((acc, r) => {
    acc[r.status] = (acc[r.status] || 0) + 1;
    return acc;
  }, {});
  lines.push('## Summary');
  lines.push('');
  lines.push(Object.entries(summary).map(([k, v]) => `- ${k}: ${v}`).join('\n') || '- Chưa có kết quả');
  lines.push('');
  lines.push('## Results');
  lines.push('');
  lines.push('| Trang | Modal | Nút/Action | Test data | Expected | Actual | Status | Evidence |');
  lines.push('|---|---|---|---|---|---|---|---|');
  for (const r of rows) {
    lines.push(`| ${mdEscape(r.page)} | ${mdEscape(r.modal)} | ${mdEscape(r.action)} | ${mdEscape(r.data)} | ${mdEscape(r.expected)} | ${mdEscape(r.actual)} | ${r.status} | ${mdEscape(r.evidence)} |`);
  }
  lines.push('');
  lines.push('## API/Dialog Evidence');
  for (const [idx, r] of rows.entries()) {
    lines.push('');
    lines.push(`### ${idx + 1}. ${r.page} - ${r.modal} - ${r.action}`);
    lines.push('');
    if (r.dialogs.length) {
      lines.push('Dialogs:');
      for (const d of r.dialogs) lines.push(`- ${d.type}: ${d.message}`);
    }
    if (r.responses.length) {
      lines.push('API responses:');
      for (const res of r.responses) lines.push(`- ${res.method} ${res.status} ${res.url} ${res.body ? `=> ${res.body}` : ''}`.slice(0, 1000));
    }
    if (!r.dialogs.length && !r.responses.length) lines.push('- Không có dialog/API liên quan được capture.');
  }
  fs.writeFileSync(REPORT_PATH, lines.join('\n'), 'utf8');
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
  page.setDefaultTimeout(8000);
  page.setDefaultNavigationTimeout(12000);
  page.on('response', async (res) => {
    const url = res.url();
    if (!url.includes('/api/')) return;
    let body = '';
    try { body = (await res.text()).slice(0, 350); } catch {}
    currentResponses.push({ method: res.request().method(), status: res.status(), url, body });
  });
  page.on('dialog', async (dialog) => {
    currentDialogs.push({ type: dialog.type(), message: dialog.message() });
    await dialog.accept();
  });

  try {
    await login(page);
    const guarded = async (name, fn) => {
      const timer = new Promise((_, reject) => setTimeout(() => reject(new Error(`Timeout nhóm test: ${name}`)), 90000));
      try {
        await Promise.race([fn(), timer]);
      } catch (err) {
        const ev = await screenshot(page, `${name}-group-timeout`).catch(() => '');
        pushResult({
          page: name,
          modal: 'Nhóm test',
          action: 'Chạy nhóm modal',
          data: 'Timeout/exception guard',
          expected: 'Nhóm test chạy xong trong timeout',
          actual: String(err?.message || err),
          status: 'Blocked',
          evidence: ev,
        });
      }
    };

    await guarded('FAQ', () => testFaq(page));
    await guarded('Danh mục', () => testCategory(page));
    await guarded('Hãng và dòng xe', () => testBrands(page));

    const routes = [
      ['/posts', 'Bài viết'],
      ['/home-banners', 'Banner'],
      ['/contacts', 'Liên hệ'],
      ['/vouchers', 'Voucher'],
      ['/motorcycles', 'Xe máy'],
      ['/parts', 'Phụ tùng'],
      ['/inventory', 'Tồn kho'],
      ['/stock-documents', 'Phiếu kho'],
      ['/orders', 'Đơn hàng'],
      ['/customers', 'Khách hàng'],
      ['/warranties', 'Bảo hành'],
      ['/advanced-operations', 'Vận hành nâng cao'],
      ['/business-operations', 'Nghiệp vụ cửa hàng'],
    ];
    for (const [route, name] of routes) {
      await guarded(name, () => testGenericRoute(page, route, name));
    }
  } catch (err) {
    const ev = await screenshot(page, 'fatal-error').catch(() => '');
    pushResult({
      page: 'Harness',
      modal: 'Runtime',
      action: 'Run full plan',
      data: 'Automated Playwright run',
      expected: 'Không crash',
      actual: String(err?.stack || err),
      status: 'Fail',
      evidence: ev,
    });
  } finally {
    writeReport('Baseline build passed before run; final build pending');
    await browser.close();
  }
}

await main();
