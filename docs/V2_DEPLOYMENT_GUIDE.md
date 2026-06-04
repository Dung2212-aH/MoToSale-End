# Hướng dẫn kỹ thuật & triển khai — MoToSale v2

Phiên bản: 1.0 · Ngày: 04/06/2026 · Đi kèm: `V2_SRS_REQUIREMENTS.md`, `V2_DESIGN.md`, `../v2/README.md`

Mục lục:
1. Yêu cầu môi trường
2. Cài đặt & chạy (development)
3. Cơ sở dữ liệu & EF Migrations
4. Cấu hình (appsettings / biến môi trường)
5. Build & kiểm thử
6. Triển khai production
7. Xử lý sự cố thường gặp
8. Nhật ký thay đổi (CHANGELOG)

---

## 1. Yêu cầu môi trường

| Thành phần | Phiên bản | Ghi chú |
|---|---|---|
| .NET SDK | **8.0+** | `dotnet --version` |
| Node.js | **≥ 20.19** | Vite 8 yêu cầu; kèm npm |
| SQL Server LocalDB | `(localdb)\MSSQLLocalDB` | Cài kèm Visual Studio hoặc gói "SQL Server Express LocalDB" |
| (tùy chọn) dotnet-ef | mới nhất | Cho thao tác migration thủ công |
| OS | Windows 10/11 | Lệnh trong tài liệu dùng PowerShell |

Kiểm tra LocalDB: `sqllocaldb info` → phải thấy `MSSQLLocalDB` (nếu chưa có: `sqllocaldb create MSSQLLocalDB`).

---

## 2. Cài đặt & chạy (development)

### 2.1 Backend (3 microservice)
Thứ tự khuyến nghị: **AuthService → APIService → ApiGateway**. APIService **tự động migrate + seed** DB khi khởi động lần đầu (chờ ~30–60s).

```powershell
cd v2\backend
dotnet restore
dotnet build
# Mỗi service một cửa sổ PowerShell:
dotnet run --project src\MoToSale.AuthService    # http://localhost:5101
dotnet run --project src\MoToSale.APIService     # http://localhost:5102
dotnet run --project src\MoToSale.ApiGateway     # http://localhost:5100
```

Kiểm tra "sống":
- `GET http://localhost:5100/health/auth` → 200
- `GET http://localhost:5100/health/api` → 200
- Swagger: `http://localhost:5101/swagger`, `http://localhost:5102/swagger`

### 2.2 Frontend admin
```powershell
cd v2\frontend-admin
npm install
npm run dev        # http://localhost:5176 (proxy /api, /uploads → :5100)
```

### 2.3 Đăng nhập (tài khoản seed)
| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | `admin@motosale.local` | `Admin@123` |
| Staff | `staff@motosale.local` | `Staff@123` |
| Customer (demo) | `customer@motosale.local` | — |

---

## 3. Cơ sở dữ liệu & EF Migrations

- **DB**: `MoToSaleV2` trên `(localdb)\MSSQLLocalDB` (code-first, EF Core).
- **Tự động**: APIService gọi `db.Database.MigrateAsync()` + `SeedConfiguration.RunAsync()` khi khởi động → không cần thao tác tay cho lần chạy đầu.
- Project chứa migration/seed: `MoToSale.Repository` (startup: `MoToSale.APIService`).

Thao tác thủ công (khi đổi entity):
```powershell
dotnet tool install --global dotnet-ef        # nếu chưa có
cd v2\backend
# Tạo migration mới
dotnet ef migrations add <TenMigration> --project src\MoToSale.Repository --startup-project src\MoToSale.APIService
# Áp dụng vào DB
dotnet ef database update --project src\MoToSale.Repository --startup-project src\MoToSale.APIService
```

Làm mới sạch DB (mất dữ liệu — chỉ dev):
```powershell
dotnet ef database drop -f --project src\MoToSale.Repository --startup-project src\MoToSale.APIService
# chạy lại APIService để migrate + seed lại
```

---

## 4. Cấu hình

Cấu hình chính ở `appsettings.json` của **APIService** và **AuthService** (giá trị phải **giống nhau** ở phần `Jwt`).

```jsonc
{
  "Urls": "http://localhost:5102",                  // 5101 cho AuthService
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MoToSaleV2;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "SecretKey": "MoToSaleV2_SharedJwtSecretKey_change_me_0123456789",  // ⚠ đổi khi production
    "Issuer": "MoToSale.Auth",
    "Audience": "MoToSale.Client",
    "ExpiryMinutes": 480
  }
}
```

| Khóa | Ý nghĩa |
|---|---|
| `Urls` | Cổng Kestrel của service |
| `ConnectionStrings:DefaultConnection` | Chuỗi kết nối SQL Server |
| `Jwt:SecretKey` | **Khóa ký JWT dùng chung** giữa Auth & API — phải trùng nhau, dài ≥ 32 ký tự |
| `Jwt:ExpiryMinutes` | Hạn token (mặc định 480 phút) |

- **Định tuyến Gateway**: `src/MoToSale.ApiGateway/ocelot.json` (`/api/auth/*`, `/api/users/*` → 5101; `/api/*`, `/uploads/*` → 5102; `BaseUrl` = 5100).
- **Frontend**: gọi `'/api'` (file `src/services/api.js`); dev proxy `/api`,`/uploads` → `:5100` cấu hình trong `vite.config.js`.

**Khuyến nghị bảo mật (production):** đưa `SecretKey` và connection string ra **biến môi trường** / user-secrets, không commit:
```powershell
$env:Jwt__SecretKey = "<chuoi-bi-mat-dai>"
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=MoToSaleV2;User Id=...;Password=...;TrustServerCertificate=True"
```
(ASP.NET Core tự nạp biến môi trường `Jwt__SecretKey`, `ConnectionStrings__DefaultConnection` đè lên appsettings.)

---

## 5. Build & kiểm thử

```powershell
# Backend
cd v2\backend
dotnet build            # 0 warning / 0 error
dotnet test             # 20/20 PASS

# Frontend
cd v2\frontend-admin
npm run build           # xuất thư mục dist/
npm run preview         # xem thử bản build
```

---

## 6. Triển khai production

### 6.1 Backend (publish)
```powershell
cd v2\backend
dotnet publish src\MoToSale.AuthService -c Release -o publish\auth
dotnet publish src\MoToSale.APIService -c Release -o publish\api
dotnet publish src\MoToSale.ApiGateway -c Release -o publish\gateway
```
Chạy mỗi service bằng `dotnet <Service>.dll` (hoặc đăng ký **Windows Service** / IIS / Docker). Đặt `ASPNETCORE_ENVIRONMENT=Production`, cấu hình `Urls`, connection string SQL Server thật, `Jwt:SecretKey` qua biến môi trường.

### 6.2 Frontend
```powershell
cd v2\frontend-admin
npm run build           # -> dist/
```
Phục vụ `dist/` bằng web server tĩnh (nginx/IIS). Cấu hình reverse-proxy để **`/api` và `/uploads` trỏ về Gateway** (vì bản production không còn Vite dev-proxy). Ví dụ nginx:
```nginx
location /api      { proxy_pass http://127.0.0.1:5100; }
location /uploads  { proxy_pass http://127.0.0.1:5100; }
location /         { try_files $uri /index.html; }   # SPA fallback
```

### 6.3 Checklist trước khi go-live
- [ ] Đổi `Jwt:SecretKey` (đưa ra env/secret), connection string SQL Server thật.
- [ ] Bật **HTTPS** (chứng chỉ) ở Gateway / reverse-proxy.
- [ ] Đổi mật khẩu tài khoản seed; tạo tài khoản thật.
- [ ] **Dọn dữ liệu test** (mã có tiền tố `SMOKE/SMK/BTL-E2E/DEMO`).
- [ ] Cấu hình sao lưu (backup) DB định kỳ.
- [ ] Rà CORS/host cho phép.

---

## 7. Xử lý sự cố thường gặp

| Triệu chứng | Nguyên nhân & cách xử lý |
|---|---|
| FE báo *"Backend không khả dụng… port 5100"* | Chưa chạy Gateway, hoặc Auth/API chưa lên. Khởi động đủ 3 service. |
| `502 Bad Gateway` ngay sau khi start | APIService đang migrate+seed (~30–60s). Đợi rồi thử lại. |
| Đăng nhập trả `401/400` | Sai tài khoản; hoặc `Jwt:SecretKey` ở Auth ≠ API → token không verify được. Đồng bộ SecretKey. |
| Lỗi kết nối SQL | LocalDB chưa chạy: `sqllocaldb start MSSQLLocalDB`; kiểm tra connection string. |
| `dotnet ef` không nhận lệnh | `dotnet tool install --global dotnet-ef` rồi mở lại terminal. |
| Port bị chiếm | Đổi `Urls` trong appsettings (và cập nhật `ocelot.json` tương ứng). |
| FE trắng trang sau `build` | Thiếu reverse-proxy `/api` ở production; cấu hình nginx/IIS như mục 6.2. |

---

## 8. Nhật ký thay đổi (CHANGELOG)

### v2.0 — 04/06/2026
**Mô hình & kho**
- Chuyển sang **1 cửa hàng / 1 kho** duy nhất (bỏ StoreId); migration `SingleInventoryLocation`.
- Đồng bộ trường **Reserved** (giữ chỗ) theo từng nghiệp vụ (checkout/POS-cọc/soạn hàng/hủy).

**Bán hàng**
- Thêm **Bán tại quầy (POS)**: bán đứt / **đặt cọc** / bán chịu, **khách quen** (tra SĐT), áp voucher, tự tạo "Khách lẻ".
- **Giao hàng & xuất kho** (`POST /orders/{id}/fulfill`): chốt đơn cọc → trừ tồn thật, nhả giữ chỗ, tự **Hoàn tất** khi thu đủ.
- **Sửa đơn** (`PUT /orders/{id}`): sửa thông tin luôn; sửa dòng hàng khi *Chờ thanh toán* (tính lại tiền).
- Hình thức thanh toán **theo trạng thái đơn** (không cho thu vượt nợ).
- **Hóa đơn GTGT (VAT)** in trình duyệt (tách thuế, số tiền bằng chữ) — POS & chi tiết đơn.

**Tài chính**
- **Đổi trả → tự hoàn tồn + sinh phiếu hoàn tiền + ghi chi quỹ** (chuỗi E).
- Thu tiền khách / thanh toán NCC / hoàn tiền **tự ghi sổ quỹ**; hủy phiếu = đảo phiếu.
- Báo cáo **lãi gộp / giá vốn (COGS)** theo giá vốn bình quân từ phiếu nhập (chuỗi H).
- `/reports` ưu tiên tính ở backend, giảm fallback phía FE (chuỗi D).

**Hậu mãi**
- **Sửa thông tin gốc bảo hành/sửa chữa** khi còn mới tiếp nhận; chặn sửa sau khi xử lý.

**Ràng buộc dữ liệu (hợp lý hóa xóa/sửa)**
- Chặn xóa: **Voucher đã dùng**, **SKU đã phát sinh đơn/tồn/giữ chỗ**, **tài khoản đã có đơn**, **danh mục/hãng còn tham chiếu**. Sản phẩm/ca = xóa mềm. Giao dịch tài chính/kho = bất biến.

**Giao diện**
- Tái cấu trúc menu thành **5 nhóm** theo domain; tách trang "Đổi trả / Công nợ / Phân ca"; ẩn trang phụ.
- **Manufacturer (Hãng SX phụ tùng)** đầy đủ FE (trước đó thiếu).
- Cải thiện dòng thời gian đơn, dropdown khách hàng.

**Sửa lỗi**
- **BUG-01**: trùng mã đơn khi tạo nhiều đơn trong cùng 1 giây → thêm mili-giây vào mã (`…HHmmssfff`) cho POS & đơn online.

**Kiểm thử**
- E2E **59/59 PASS**, BE test **20/20**, build FE/BE sạch (báo cáo `V2_BTL_FULL_SYSTEM_TEST_REPORT_20260604.md`).
