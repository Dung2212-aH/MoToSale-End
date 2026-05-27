# Ke hoach sua nghiep vu quan ly don hang Admin

## 1. Muc tieu

Chuan hoa phan quan ly don hang trong trang Admin de trang thai don hang, thanh toan va van chuyen tach bach, de hieu va dung nghiep vu.

Luong trang thai don hang chinh:

```text
AwaitingPayment -> Confirmed -> Processing -> Shipping -> Delivered -> Completed
```

Nhanh huy:

```text
AwaitingPayment / Confirmed / Processing -> Cancelled
```

Khong nen gom trang thai don hang, thanh toan va van chuyen vao chung mot dropdown.

## 2. Chuan hoa trang thai

### 2.1. Trang thai don hang

Dung trong `TrangThaiDonHang`.

| Gia tri | Nhan hien thi | Ghi chu |
| --- | --- | --- |
| `AwaitingPayment` | Cho thanh toan / cho xac nhan | Trang thai don moi sau checkout |
| `Confirmed` | Da xac nhan | Admin/staff da xac nhan don |
| `Processing` | Dang chuan bi hang | Dang soan/gom hang |
| `Shipping` | Dang giao | Don dang tren duong giao |
| `Delivered` | Da giao | Khach da nhan hang |
| `Completed` | Hoan tat | Don da ket thuc nghiep vu |
| `Cancelled` | Da huy | Don bi huy |

An khoi UI admin:

| Gia tri | Ly do |
| --- | --- |
| `Pending` | Trang thai legacy/ky thuat |
| `Checkout` | Trang thai tam trong qua trinh checkout |

### 2.2. Trang thai thanh toan

Dung trong `TrangThaiThanhToan`.

| Gia tri | Nhan hien thi |
| --- | --- |
| `Unpaid` | Chua thanh toan |
| `Pending` | Cho xac nhan thanh toan |
| `PartiallyPaid` | Thanh toan mot phan / da dat coc |
| `Paid` | Da thanh toan |
| `Failed` | Thanh toan that bai |
| `Refunded` | Da hoan tien |
| `Cancelled` | Da huy thanh toan |

Can bo sung `Refunded` vao backend neu admin can quan ly hoan tien.

### 2.3. Trang thai van chuyen

Dung trong `TrangThaiVanChuyen`.

| Gia tri | Nhan hien thi | Ap dung |
| --- | --- | --- |
| `NotShipped` | Chua giao | Mac dinh |
| `Preparing` | Dang chuan bi giao | Giao hang |
| `Shipping` | Dang giao | Giao hang |
| `Delivered` | Da giao | Giao hang |
| `PickupReady` | San sang nhan tai showroom | Nhan tai showroom |
| `PickedUp` | Da nhan tai showroom | Nhan tai showroom |
| `Cancelled` | Da huy giao hang | Don bi huy |

## 3. Sua backend

File chinh:

- `D:\MotorTeam\MoToSale-End\Backend\OrderService\Controllers\OrdersController.cs`
- `D:\MotorTeam\MoToSale-End\Backend\OrderService\DTOs\Orders\OrderSearchDto.cs`
- `D:\MotorTeam\MoToSale-End\Backend\OrderService\Repositories\OrderRepository.cs`

### 3.1. Guard chuyen trang thai don

Them rule chuyen trang thai hop le:

| Trang thai hien tai | Duoc chuyen sang |
| --- | --- |
| `AwaitingPayment` | `Confirmed`, `Cancelled` |
| `Confirmed` | `Processing`, `Cancelled` |
| `Processing` | `Shipping`, `Cancelled` |
| `Shipping` | `Delivered` |
| `Delivered` | `Completed` |
| `Completed` | Khong cho doi tiep |
| `Cancelled` | Khong cho doi tiep |

Neu DB con don legacy `Pending` hoac `Checkout`, co the cho chuyen sang `AwaitingPayment`, `Confirmed` hoac `Cancelled`.

### 3.2. Rule huy don

Khi chuyen don sang `Cancelled`:

- Bat buoc co `LyDoHuyDon`.
- Set `NgayHuyDon` neu chua co.
- Set `TrangThaiThanhToan = Cancelled` neu chua thanh toan thanh cong.
- Set `TrangThaiVanChuyen = Cancelled`.
- Giai phong ton kho giu cho neu co.

Khong cho huy don khi:

- `Delivered`
- `Completed`

Neu can xu ly sau giao hang, nen dung nghiep vu hoan tien/doi tra rieng, khong doi don ve `Cancelled`.

### 3.3. Rule thanh toan

Backend `AllowedAdminPaymentStatuses` nen co:

```text
Unpaid, Pending, PartiallyPaid, Paid, Failed, Refunded, Cancelled
```

Khi set `Paid`:

- Set `NgayThanhToanThanhCong` neu chua co.

Khi set `Refunded`:

- Yeu cau ghi chu hoac ly do hoan tien.
- Khong tu dong doi `TrangThaiDonHang` neu don da `Completed`.

### 3.4. Rule van chuyen

Them whitelist cho `TrangThaiVanChuyen`:

```text
NotShipped, Preparing, Shipping, Delivered, PickupReady, PickedUp, Cancelled
```

Khong nhan raw string tuy y tu client.

Goi y dong bo:

- Don sang `Processing` thi shipping co the la `Preparing`.
- Don sang `Shipping` thi shipping phai la `Shipping`.
- Don sang `Delivered` thi shipping phai la `Delivered`.
- Don sang `Cancelled` thi shipping phai la `Cancelled`.

### 3.5. Search/filter don hang

Bo sung vao `OrderSearchDto`:

```csharp
public string? TrangThaiVanChuyen { get; set; }
public string? Keyword { get; set; }
```

Repository can filter theo:

- `TrangThaiDonHang`
- `TrangThaiThanhToan`
- `TrangThaiVanChuyen`
- Ma don
- Ten nguoi nhan
- So dien thoai
- Email

## 4. Sua FrontendAdmin

File chinh:

- `D:\MotorTeam\MoToSale-End\FrontendAdmin\src\utils\constants.js`
- `D:\MotorTeam\MoToSale-End\FrontendAdmin\src\pages\orders\OrderList.jsx`
- `D:\MotorTeam\MoToSale-End\FrontendAdmin\src\pages\orders\OrderDetail.jsx`
- `D:\MotorTeam\MoToSale-End\FrontendAdmin\src\services\orderService.js`

### 4.1. Constants

Sua `ORDER_STATUS_LABELS` de khong gop nhieu trang thai ve `Pending`.

`ORDER_STATUS_OPTIONS` chi nen hien cac status nghiep vu:

```text
AwaitingPayment
Confirmed
Processing
Shipping
Delivered
Completed
Cancelled
```

Them:

```text
PAYMENT_STATUS_OPTIONS
SHIPPING_STATUS_OPTIONS
```

### 4.2. Trang danh sach don hang

Bang nen co cot:

```text
Ma don
Khach hang
Tong tien
Trang thai don
Thanh toan
Van chuyen
Ngay tao
Thao tac
```

Filter nen co:

- Tu khoa: ma don, ten, so dien thoai, email.
- Trang thai don.
- Trang thai thanh toan.
- Trang thai van chuyen.
- Khoang ngay neu can.

Can map params dung backend:

```js
{
  keyword,
  trangThaiDonHang,
  trangThaiThanhToan,
  trangThaiVanChuyen,
  page,
  pageSize
}
```

Khong nen gui `status` chung chung neu backend khong doc field nay.

### 4.3. Trang chi tiet don hang

Tach thanh 3 khu vuc:

1. Trang thai don hang.
2. Trang thai thanh toan.
3. Trang thai van chuyen.

Nut hanh dong:

- `Cap nhat trang thai don`
- `Cap nhat thanh toan`
- `Cap nhat van chuyen`
- `Huy don`

Modal cap nhat trang thai don:

- Hien trang thai hien tai.
- Dropdown chi hien trang thai ke tiep hop le.
- Neu chon `Cancelled`, bat buoc nhap ly do.
- Neu chon `Completed`, hien canh bao thao tac ket thuc don.

Modal thanh toan:

- Dropdown trang thai thanh toan.
- Neu `Refunded`, bat buoc co ghi chu/ly do.

Modal van chuyen:

- Neu `PhuongThucNhanHang = Delivery`, hien `Preparing`, `Shipping`, `Delivered`.
- Neu `PhuongThucNhanHang = Pickup`, hien `PickupReady`, `PickedUp`.

### 4.4. Disable hanh dong dung nghiep vu

Khong cho cap nhat don neu:

- Don `Cancelled`.
- Don `Completed`.

Khong cho huy neu:

- Don `Delivered`.
- Don `Completed`.
- Don da `Cancelled`.

## 5. Dong bo Frontend user

File can ra soat:

- `D:\MotorTeam\MoToSale-End\Frontend\src\utils\statusMappings.js`
- `D:\MotorTeam\MoToSale-End\Frontend\src\pages\OrdersPage.jsx`
- `D:\MotorTeam\MoToSale-End\Frontend\src\pages\OrderDetailPage.jsx`

Can dam bao:

- User chi duoc huy don khi `TrangThaiDonHang = AwaitingPayment`.
- Timeline van chuyen dung `TrangThaiVanChuyen`.
- Label hien thi giong Admin.
- Don `Completed` hien la hoan tat, khong gop voi `Delivered` neu can phan biet.

## 6. Test nghiep vu

### 6.1. Test backend

Chay build:

```powershell
dotnet build D:\MotorTeam\MoToSale-End\Backend\OrderService\OrderService.csproj
```

Test API qua gateway:

1. Tao don moi tu cart.
2. Cap nhat `AwaitingPayment -> Confirmed`.
3. Cap nhat `Confirmed -> Processing`.
4. Cap nhat `Processing -> Shipping`.
5. Cap nhat `Shipping -> Delivered`.
6. Cap nhat `Delivered -> Completed`.
7. Thu `AwaitingPayment -> Delivered`, phai bi chan.
8. Thu `Cancelled -> Confirmed`, phai bi chan.
9. Thu huy khong co ly do, phai bi chan.
10. Thu set payment `Refunded`, phai hop le neu da bo sung.

### 6.2. Test FrontendAdmin

Chay build:

```powershell
npm run build
```

Test UI:

1. Mo danh sach don hang.
2. Filter theo trang thai don.
3. Filter theo trang thai thanh toan.
4. Filter theo trang thai van chuyen.
5. Vao chi tiet don.
6. Cap nhat status theo dung flow.
7. Thu chon status khong hop le va dam bao UI khong hien option do.
8. Huy don co ly do.
9. Kiem tra badge tren list sau reload.
10. Quay lai list van giu filter/page neu co.

## 7. Thu tu thuc hien de it rui ro

1. Sua constants/status labels trong FrontendAdmin.
2. Sua backend guard trong `OrdersController`.
3. Sua DTO/repository filter.
4. Sua `OrderDetail.jsx`.
5. Sua `OrderList.jsx`.
6. Dong bo status mapping ben Frontend user.
7. Build backend.
8. Build FrontendAdmin.
9. Test API nhanh.
10. Test UI admin.

## 8. Ghi chu nghiep vu

- `Pending` va `Checkout` la trang thai legacy/ky thuat, khong nen de admin chon moi.
- `Delivered` la da giao hang, nhung co the chua dong nghiep vu ke toan/bao hanh.
- `Completed` la don da hoan tat, nen khoa cac thao tac sua/huy thong thuong.
- Hoan tien nen nam o `TrangThaiThanhToan = Refunded`, khong nen doi don ve `Cancelled` neu hang da giao.
- Neu sau nay lam doi tra/bao hanh, nen tao module rieng thay vi nhai them status don hang.
