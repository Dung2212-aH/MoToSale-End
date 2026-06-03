# V2 Frontend Admin Migration Plan

> Trang thai: In Progress
>
> Muc tieu: giu nguyen `FrontendAdmin` hien co lam nen UI, chuyen dan sang backend v2 da chuan hoa theo BaseCore. Khong viet lai frontend tu dau.

## 1. Nguyen tac bat buoc

- DB v2, backend v2 va API contract v2 la nguon su that moi.
- Giao dien admin tieng Viet, ten bang/cot/entity/API ky thuat dung tieng Anh.
- Khong xoa nghiep vu dang co tren FE neu chua co phuong an thay the.
- Chuyen tung module, build va test UI that sau moi lo.
- Khong duy tri adapter tam thoi vo thoi han. Adapter phai duoc ghi ro va xoa khi module da chuyen xong.
- Khong tao bang trong controller luc runtime. Moi thay doi schema di qua EF Core migration.
- FE giu layout, Tailwind CSS, responsive va cac sua loi UI da co.

## 2. Nen tang da chot

- Source FE dich: `v2/frontend-admin`, duoc copy tu `FrontendAdmin` hien co.
- Source BE dich: `v2/backend`.
- Gateway: `MoToSale.ApiGateway`, port `5100`.
- Auth service: `MoToSale.AuthService`, port `5101`.
- API service: `MoToSale.APIService`, port `5102`.
- DB: SQL Server `MoToSale`, code-first EF Core migrations.

## 3. Doi chieu contract FE va BE v2

### 3.1 Dung lai gan nhu truc tiep

| Module | FE hien co | BE v2 | Trang thai |
|---|---|---|---|
| Hang xe | `/brands` | `/brands` | Can map field |
| Dong xe | `/models` | `/models` | Can map field |
| Danh muc | `/categories` | `/categories` | Can map field |
| Don hang list/detail | `/orders` | `/orders` | Can map DTO |
| Voucher CRUD | `/vouchers` | `/vouchers` | Can map DTO |
| Ton kho list | `/inventory` | `/inventory` | Can map DTO theo store/SKU |
| Phieu kho | `/inventory/documents` | `/inventory/documents` | Can map DTO |
| Giu cho | `/inventory/holds` | `/inventory/holds` | Can map DTO |
| Dieu chinh ton | `/inventory/adjustments`, `/inventory/adjust` | Cung endpoint | Can map DTO |
| Bao hanh | `/warranties` | `/warranties` | Can map DTO |
| Danh gia | `/reviews` | `/reviews` | Can map DTO |
| Bai viet, FAQ, lien he, banner | `/content/*` | `/content/*` | Can map DTO |

### 3.2 Can sua service FE

| Module | FE hien co | BE v2 | Viec can lam |
|---|---|---|---|
| Dang nhap | Gui `{ Email, MatKhau }` | Nhan `{ Email, Password }` | Doi payload |
| San pham cap nhat | `PATCH /products/{id}` | `PUT /products/{id}` | Doi method va map DTO |
| Bien the | `/products/{id}/variants` | `/products/{id}/skus` | Doi route, ten field va UI label noi bo |
| Anh chinh | FE chua goi endpoint dat chinh rieng | `POST /products/{id}/images/{imageId}/primary` | Noi nut dat anh chinh |
| Bai viet upload anh | `/content/posts/{id}/image` | `/content/posts/image` | Doi flow upload |
| Huy don | `PUT /orders/{id}/cancel` | `POST /orders/{id}/cancel` | Doi method va payload |
| Huy thanh toan | `PATCH /payments/{id}/cancel` | `POST /payments/{id}/cancel` | Doi method |

### 3.3 BE v2 con thieu hoac chua du chieu sau

| Module | Thieu |
|---|---|
| Nguoi dung | Admin create, update, update status, delete; FE dang goi `/users/all` trong khi v2 list la `/users` |
| Khach hang | Can kiem tra DTO ghi chu cham soc va phan trang |
| Thanh toan | Can endpoint detail va confirm thu cong |
| Ton kho | Can xac nhan export, threshold, sync dung nghia ledger; bo hanh vi sua tong ton kieu cu |
| Don hang | Can map allocation UI, goi y phan bo, timeline trang thai |
| Audit log | Chua co API doc |
| Cau hinh van hanh | Chua co API kho/cau hinh |
| Bao cao | Chua co endpoint dashboard/report chuyen dung; FE dang tong hop client-side |

## 4. Lo trinh chuyen FE

### Phase 0 - Baseline

- [x] Xoa frontend v2 viet moi.
- [x] Copy `FrontendAdmin` hien co vao `v2/frontend-admin`.
- [x] Loai `node_modules`, `dist`, `test-artifacts`, `test-evidence`.
- [x] Build backend v2.
- [x] Cai dependency va build FE v2.
- [ ] Chup baseline UI cac trang dang co.

### Phase 1 - Adapter nen tang

- [x] Sua login payload theo DTO v2.
- [ ] Sua AuthContext theo DTO v2.
- [ ] Tao helper unwrap response, paging va mapper dung chung.
- [ ] Chuan hoa enum ky thuat -> nhan tieng Viet.
- [x] Doi Vite proxy sang gateway `5100`.
- [x] Sua adapter huy don, huy thanh toan va upload anh bai viet theo route v2.
- [x] Tach DB nghien cuu sang `MoToSaleV2`, sinh va ap migration `HardenSchemaRelations`.
- [x] Build BE va FE sau lo chinh sua nen tang.

### Phase 2 - Catalog

- [x] Chuyen route bien the cu `/variants` sang SKU v2 `/skus`.
- [x] Map DTO SKU v2 vao modal bien the va bo ton kho khoi form SKU.
- [x] Chuyen upload anh theo SKU va endpoint dat anh chinh sang contract v2.
- [x] Map bo loc san pham cu `loaiSanPham` sang `kind` v2 de tach xe may va phu tung.
- [x] Doc `parentId` v2 khi tao cay danh muc; dam bao `Dau nhot` nam duoi `Phu tung`.
- [ ] Chuyen hang xe, dong xe, danh muc.
- [ ] Chuyen san pham sang mo hinh `Product -> Sku`.
- [ ] Chuyen anh san pham/SKU, dat anh chinh.
- [ ] Chuyen tuong thich phu tung.

### Phase 3 - Inventory

- [ ] Chuyen ton theo `Store + Sku`.
- [ ] Chuyen ledger, giu cho, nguong canh bao.
- [ ] Chuyen phieu nhap/xuat/dieu chinh/kiem ke/chuyen kho.
- [ ] Bo toan bo logic sua ton tai form san pham.

### Phase 4 - Order And Payment

- [ ] Chuyen gio hang, don hang, timeline.
- [ ] Bo sung UI phan bo don theo cua hang.
- [ ] Bo sung goi y phan bo tu dong.
- [ ] Chuyen thanh toan thu cong va doi soat trang thai.

### Phase 5 - Operations

- [ ] Chuyen user, customer care, review, warranty.
- [ ] Chuyen post, FAQ, contact, home banner.
- [ ] Bo sung audit log, cau hinh van hanh.
- [ ] Bo sung dashboard, report va export XLSX.

### Phase 6 - Regression

- [ ] Build BE.
- [ ] Build FE admin.
- [ ] Test UI that desktop/tablet/mobile.
- [ ] Test tat ca nut, form, modal, filter, paging.
- [ ] Doi soat DB: ton kho, ledger, reservation, allocation, payment, timeline.

## 5. Acceptance criteria

- `v2/frontend-admin` giu du cac module van hanh cua admin hien tai.
- Khong con FE admin viet lai song song.
- Khong con contract FE cu tieng Viet trong request/response sau khi module da chuyen xong.
- Khong con `EnsureTablesAsync` hoac `CREATE TABLE` trong controller.
- Ton kho chi thay doi qua Inventory service va ledger.
- Moi migration, build va regression deu pass.
