# V2 Frontend Store - Section 12 Retest Report

Thoi gian retest: 2026-06-06, Asia/Saigon

## Ket Qua Tong Hop

| Trang thai | So luong |
|---|---:|
| PASS | 47 |
| INFO | 4 |
| WARN | 2 |
| BLOCK | 1 |
| FAIL | 0 |

## Loi Da Sua Va Xac Nhan

- Checkout pickup da luu rieng `fulfillmentNote` va `pickupAppointmentAt` xuong backend, reload order detail van con du lieu.
- Order detail storefront da hien thi `Lich su don hang` theo timeline tu `histories` backend.
- Gio hien thi tren order detail da chuan hon: lich su don dung mui gio Viet Nam, lich hen pickup giu dung gio khach chon.
- Mobile checkout het tran ngang: `/checkout` viewport 390 co `scrollW = 390`, `clientW = 390`.
- Header mega-menu san pham khong con lam tang scroll ngang khi dang an.
- Footer newsletter co validate required va feedback tai cho sau khi submit.

## Kiem Tra Chinh

- `npm run build` tai `v2/frontend-store`: PASS.
- `dotnet build v2/backend/src/MoToSale.APIService/MoToSale.APIService.csproj`: PASS.
- `dotnet ef database update`: da apply migration `20260605174021_AddOrderFulfillmentPickupFields`.
- Script retest: `node docs/test-scripts/store-section12-full-test.mjs`.

## WARN Con Lai

- `12.4-shipping-fee`: Store checkout hien chua co nghiep vu tinh phi ship khac 0, dang mac dinh `shippingFee = 0`.
- `12.4-voucher-cart-change-after-apply`: Checkout khong co sua so luong tai cho; neu quay ve gio hang de sua thi voucher local se reset. Day la hanh vi an toan, nhung neu muon UX tot hon co the them quantity controls ngay trong checkout.

## BLOCK Con Lai

- `12.4-product-hidden-out-of-stock-during-checkout`: Chua chay destructive test doi trang thai/ton kho live product vi can disposable fixture co restore dam bao.
