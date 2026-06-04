# Sổ tay vận hành & bảo trì — MoToSale v2

Phiên bản: 1.0 · Ngày: 04/06/2026 · Đối tượng: người quản trị hệ thống / kỹ thuật vận hành.
Đi kèm: `V2_DEPLOYMENT_GUIDE.md` (cài đặt/triển khai), `V2_USER_MANUAL.md` (sử dụng).

---

## 1. Tổng quan vận hành

| Thành phần | Cổng | Vai trò | Khởi động |
|---|---|---|---|
| ApiGateway | 5100 | Điểm vào duy nhất | thứ 3 |
| AuthService | 5101 | Đăng nhập/JWT/tài khoản | thứ 1 |
| APIService | 5102 | Nghiệp vụ + **tự migrate & seed** | thứ 2 |
| Frontend admin | 5176 (dev) / web tĩnh (prod) | Giao diện | sau cùng |
| SQL Server LocalDB | — | DB `MoToSaleV2` | nền tảng |

Thứ tự khởi động: **Auth → API → Gateway → FE**. Thứ tự dừng: ngược lại.

---

## 2. Khởi động / Dừng dịch vụ

### 2.1 Khởi động (dev — PowerShell)
```powershell
cd v2\backend
Start-Process dotnet "run --project src\MoToSale.AuthService"
Start-Process dotnet "run --project src\MoToSale.APIService"   # chờ ~30–60s migrate+seed
Start-Process dotnet "run --project src\MoToSale.ApiGateway"
cd ..\frontend-admin ; npm run dev
```

### 2.2 Kiểm tra "sống" (health)
```powershell
Invoke-RestMethod http://localhost:5100/health/auth   # 200
Invoke-RestMethod http://localhost:5100/health/api    # 200
```

### 2.3 Dừng
- Đóng cửa sổ tiến trình, hoặc tìm & dừng theo cổng:
```powershell
Get-NetTCPConnection -LocalPort 5100,5101,5102 -State Listen |
  Select-Object -Expand OwningProcess -Unique | ForEach-Object { Stop-Process -Id $_ -Force }
```

### 2.4 Khởi động lại nhanh sau khi cập nhật code
Dừng theo 2.3 → `dotnet build` → khởi động lại theo 2.1. Nếu đổi entity → chạy migration (mục 4) trước.

---

## 3. Quản lý tài khoản & phân quyền

- **Vai trò**: `Admin` (toàn quyền) · `Staff` (tác nghiệp) · `Customer` (mua online).
- Tạo/sửa/khóa nhân viên: đăng nhập Admin → **Hệ thống → Tài khoản & vai trò**.
- **Ràng buộc an toàn** (hệ thống tự enforce):
  - Không tự **khóa/xóa** chính tài khoản đang đăng nhập; không xóa **Admin hoạt động cuối cùng**.
  - **Không xóa** tài khoản đã phát sinh đơn (chỉ khóa).
- **Đổi mật khẩu**: menu tài khoản góc phải, hoặc Admin đặt lại cho Staff.
- **Khuyến nghị**: mỗi nhân viên một tài khoản riêng (để nhật ký truy đúng người); đổi toàn bộ mật khẩu seed trước khi go-live.
- Token JWT hết hạn sau **480 phút** → người dùng cần đăng nhập lại.

---

## 4. Cơ sở dữ liệu — migrate, sao lưu, phục hồi

DB: **`MoToSaleV2`** trên `(localdb)\MSSQLLocalDB`.

### 4.1 Migration
- APIService **tự áp migration khi khởi động**. Khi đổi entity, tạo migration mới:
```powershell
cd v2\backend
dotnet ef migrations add <Ten> --project src\MoToSale.Repository --startup-project src\MoToSale.APIService
dotnet ef database update --project src\MoToSale.Repository --startup-project src\MoToSale.APIService
```

### 4.2 Sao lưu (backup)
```powershell
sqllocaldb start MSSQLLocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "BACKUP DATABASE [MoToSaleV2] TO DISK='C:\Backups\MoToSaleV2_2026-06-04.bak' WITH INIT, COMPRESSION"
```
Khuyến nghị backup **định kỳ** (hằng ngày) + giữ nhiều bản; có thể đặt lịch bằng Task Scheduler gọi lệnh trên.

### 4.3 Phục hồi (restore)
```powershell
# Ngắt kết nối ứng dụng trước (dừng Auth/API). Sau đó:
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "ALTER DATABASE [MoToSaleV2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [MoToSaleV2] FROM DISK='C:\Backups\MoToSaleV2_2026-06-04.bak' WITH REPLACE; ALTER DATABASE [MoToSaleV2] SET MULTI_USER"
```

### 4.4 Làm mới sạch (chỉ dev — MẤT dữ liệu)
```powershell
cd v2\backend
dotnet ef database drop -f --project src\MoToSale.Repository --startup-project src\MoToSale.APIService
# khởi động lại APIService để migrate + seed lại
```

---

## 5. Dọn dữ liệu test trước demo / go-live

Dữ liệu thử nghiệm mang tiền tố mã: **`SMOKE` · `SMK` · `BTL-E2E` · `DEMO`** (ở Order.Code, Voucher.Code, Supplier.Code…).

- **An toàn nhất**: phục hồi từ backup "sạch" trước khi tạo dữ liệu test, hoặc làm mới + seed lại (mục 4.4) rồi nhập dữ liệu thật.
- Nếu xóa thủ công: lưu ý ràng buộc nghiệp vụ — đơn/giao dịch tài chính **không xóa cứng**; nên xóa theo đúng thứ tự phụ thuộc hoặc dùng bản backup sạch. **Backup trước khi dọn.**

---

## 6. Theo dõi & nhật ký

- **Nhật ký ứng dụng**: log Kestrel hiện ở cửa sổ tiến trình mỗi service (mức `Information`/`Warning` theo `appsettings`). Khi chạy nền, chuyển hướng ra file để lưu.
- **Nhật ký kiểm toán nghiệp vụ**: trong app → **Hệ thống → Nhật ký kiểm toán** (ai/lúc nào/đối tượng/hành động) — dùng để truy vết thao tác bán/sửa/xóa/duyệt.
- **Kiểm tra định kỳ**: health endpoint (2.2), dung lượng DB, log lỗi 5xx, cảnh báo tồn dưới ngưỡng (trang Tồn kho/Báo cáo).

---

## 7. Cấu hình quan trọng (khi vận hành)

| Mục | Vị trí | Lưu ý |
|---|---|---|
| Chuỗi kết nối DB | `appsettings.json` hoặc env `ConnectionStrings__DefaultConnection` | Trỏ SQL Server thật khi prod |
| Khóa ký JWT | `Jwt:SecretKey` (Auth **và** API phải giống) hoặc env `Jwt__SecretKey` | Đổi khỏi giá trị mặc định, ≥32 ký tự |
| Hạn token | `Jwt:ExpiryMinutes` (mặc định 480) | |
| Định tuyến Gateway | `MoToSale.ApiGateway/ocelot.json` | Sửa khi đổi cổng downstream |
| Proxy FE (prod) | nginx/IIS | `/api`,`/uploads` → Gateway; SPA fallback |
| Cấu hình cửa hàng (tên, MST, VAT, ngưỡng) | trong app → Cấu hình | Ảnh hưởng hóa đơn & cảnh báo tồn |

---

## 8. Sự cố thường gặp & cách xử lý

| Triệu chứng | Xử lý |
|---|---|
| FE báo *"Backend không khả dụng… 5100"* | Khởi động đủ Auth/API/Gateway (mục 2.1). |
| `502 Bad Gateway` ngay sau start | APIService đang migrate+seed (~30–60s); đợi rồi thử lại. |
| Đăng nhập `401/400` | Sai tài khoản, hoặc `Jwt:SecretKey` Auth ≠ API → đồng bộ rồi khởi động lại. |
| Lỗi kết nối SQL | `sqllocaldb start MSSQLLocalDB`; kiểm tra connection string. |
| Token hết hạn liên tục | Tăng `Jwt:ExpiryMinutes`; kiểm tra giờ hệ thống. |
| Cổng bị chiếm | Đổi `Urls` + `ocelot.json` cho khớp; hoặc dừng tiến trình chiếm cổng (2.3). |
| Dữ liệu sai sau thao tác hỏng | Phục hồi từ backup gần nhất (4.3); kiểm nhật ký kiểm toán để xác định phạm vi. |
| `dotnet ef` không nhận | `dotnet tool install --global dotnet-ef`, mở lại terminal. |

---

## 9. Quy trình bảo trì định kỳ (đề xuất)

- **Hằng ngày**: backup DB; xem nhanh log lỗi & cảnh báo tồn.
- **Hằng tuần**: kiểm tra dung lượng DB/backup; rà nhật ký kiểm toán bất thường.
- **Khi cập nhật phiên bản**: backup → dừng dịch vụ → `dotnet build`/migration → khởi động lại → health-check → kiểm thử nhanh luồng bán/đơn/báo cáo.
- **Định kỳ bảo mật**: đổi mật khẩu quản trị, rà tài khoản còn hiệu lực, xác nhận secret/connection string không lộ.
