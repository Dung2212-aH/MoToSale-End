# V2 Frontend Store - Section 12 Test Report

Thoi gian test: 2026-06-06 00:16, Asia/Saigon  
Pham vi: muc 12 trong `docs/V2_FRONTEND_STORE_FULL_USER_JOURNEY_TEST_SCENARIOS.md`  
Moi truong: Store `http://127.0.0.1:5174`, Gateway `http://localhost:5100/api`

## Tom tat

Da chay Playwright UI + doi chieu API cho muc 12.

Ket qua full-run: 42 PASS, 4 FAIL, 3 WARN, 1 BLOCK, 4 INFO.

Sau khi re-check isolated de loai false negative:

- Product detail `Them vao gio hang`: PASS.
- Order detail review modal open/close: PASS.
- Loi app con lai: 3 FAIL, 4 WARN, 1 BLOCK.

Artifact screenshot:

- `docs/test-artifacts/store-section12-20260605/12-1-vouchers-guest.png`
- `docs/test-artifacts/store-section12-20260605/12-2-checkout-prefill.png`
- `docs/test-artifacts/store-section12-20260605/12-2-valid-pickup-submit.png`
- `docs/test-artifacts/store-section12-20260605/12-3-product-detail.png`
- `docs/test-artifacts/store-section12-20260605/12-3-store-system.png`
- `docs/test-artifacts/store-section12-20260605/12-4-bank-transfer-qr.png`
- `docs/test-artifacts/store-section12-20260605/12-4-order-detail.png`
- `docs/test-artifacts/store-section12-20260605/isolated-product-addcart.png`

## Loi con lai

| ID | Muc do | Ket qua |
| --- | --- | --- |
| S12-F01 | FAIL | Checkout pickup co UI `fulfillmentNote` va `pickupAppointmentAt`, nhung don hop le chi luu `note`; API order #88 chi co keys `fulfillmentStatus`, `note`. |
| S12-F02 | FAIL | Store order detail khong render lich su/timeline don hang, trong khi API `/orders/73` tra `histories` co 5 dong. |
| S12-F03 | FAIL | Mobile `/checkout` bi horizontal overflow: viewport 390px, `scrollWidth = 399`. |
| S12-W01 | WARN | Footer newsletter chi `preventDefault`, khong co API/action/thong bao thanh cong-that bai. |
| S12-W02 | WARN | Checkout `shippingFee` dang hardcode `0`, chua co nghiep vu phi ship khac 0 de test. |
| S12-W03 | WARN | Sau khi apply voucher, checkout khong co luong revalidate neu cart thay doi trong cung man; hien luong cart change se reset voucher khi quay lai. |
| S12-W04 | WARN | Don test dung voucher bi huy da lam `usedCount` voucher tang trong qua trinh mutation; can xac nhan nghiep vu co phai hoan usage khi huy don hay khong. |
| S12-B01 | BLOCK | Race case product hidden/out-of-stock during checkout chua chay destructive tren product that; can disposable product/stock fixture de restore an toan. |

## Cac nhom da pass

- Route public/protected: `/vouchers` public, protected routes redirect login, `/login` va `/register` redirect ve home khi da dang nhap.
- 404 route: khong crash, dung layout ngoai `MainLayout`.
- Product filters: danh muc phu tung hien `compatibleCarModelId`, danh muc xe may hien `vehicleTypeCategoryId`.
- Account: email readOnly.
- Checkout: prefill profile/address pass; invalid deposit `0`, am, chu, bang tong tien, lon hon tong tien deu bi chan.
- Voucher: guest xem duoc voucher, guest bam nhan bi yeu cau dang nhap; amount/percent voucher tinh dung tren subtotal hien tai.
- Bank transfer: mo QR, so tien QR 760.000d, noi dung chuyen khoan co ma don; order financial cross-check pass.
- Cart empty: checkout bi disable, CTA mua sam co.
- Product detail: gallery next/thumb pass, add cart pass trong isolated re-check, review form visible.
- Store system: card/map/tel link pass; forced API 500 co retry state.
- Orders/order detail: forced API 500 co retry state.
- Multi-user isolation: user khac xem `/orders/73` bi 404.
- Auth storage: remember me luu token localStorage; xoa token redirect login.
- Responsive: `/account`, `/cart`, `/orders/73`, `/products/10` khong overflow ngang.
- Review moderation API: create review, chan review trung, pending khong public, approve public, hide khong public, cleanup delete pass.

## Cleanup

- Don pickup test #88: da huy thanh cong.
- Don bank transfer test #89: da huy thanh cong.
- Don isolated pickup #83: da huy thanh cong.
- Review test #10: da hide/delete, khong public.
- Cart customer test: da clear, `totalItems = 0`.

## Ghi chu ky thuat

Test script: `docs/test-scripts/store-section12-full-test.mjs`.

Co 2 false negative trong full-run da re-check isolated:

- `Them vao gio hang` tren product detail pass, screenshot `isolated-product-addcart.png`.
- Review modal order detail co nut va co the dong/mo; loi full-run ban dau do helper normalize tieng Viet chua xu ly ky tu `D/d`.
