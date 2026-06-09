# Huong dan deploy lai sau khi sua code

Tai lieu nay dung cho quy trinh hien tai:

- Source lam viec chinh tren may local: `D:\MotorTeam\MoToSale-End\v2`
- Repo goc cua project day du: `https://github.com/Dung2212-aH/MoToSale-End`
- Repo deploy VPS, chi chua noi dung `v2` o root: `https://github.com/tongvandong/xemoto`
- VPS clone source tai: `/opt/xemoto`
- Domain store: `https://xemoto.xyz`
- Domain admin: `https://admin.xemoto.xyz`

## Nguyen tac quan trong

- Khong sua truc tiep file trong container Docker.
- Khong commit file `.env`.
- Khong chay `docker compose down -v` neu khong muon xoa DB va anh upload.
- Nen build truoc, sau do moi `up -d` de neu build loi thi app cu van con chay.

## 1. Sua code tren may local

Sua code trong:

```powershell
D:\MotorTeam\MoToSale-End\v2
```

Vi du cac thu muc hay sua:

```text
v2/backend
v2/frontend-admin
v2/frontend-store
v2/docker-compose.yml
```

Neu can test local:

```powershell
cd D:\MotorTeam\MoToSale-End\v2\frontend-admin
npm run build
```

```powershell
cd D:\MotorTeam\MoToSale-End\v2\frontend-store
npm run build
```

```powershell
cd D:\MotorTeam\MoToSale-End\v2\backend
dotnet test
```

## 2. Commit vao repo goc `MoToSale-End`

Tu thu muc repo goc:

```powershell
cd D:\MotorTeam\MoToSale-End
git status
git add .
git commit -m "fix: update demo"
git push origin push-ready
```

Neu branch hien tai khong phai `push-ready`, kiem tra:

```powershell
git branch --show-current
```

## 3. Day rieng noi dung `v2` sang repo deploy `xemoto`

Vi repo `tongvandong/xemoto` chi chua noi dung `v2` o root, can subtree split:

```powershell
cd D:\MotorTeam\MoToSale-End
$split = git subtree split --prefix=v2 HEAD
git push https://github.com/tongvandong/xemoto.git "$split`:refs/heads/main"
```

Neu Git bao remote `main` da co commit moi va can force do split history, chi force khi chac chan repo `xemoto` chi la repo deploy:

```powershell
git push --force-with-lease https://github.com/tongvandong/xemoto.git "$split`:refs/heads/main"
```

Thong thuong neu tat ca deu day tu mot nguon local nay thi khong can force.

## 4. SSH vao VPS

```bash
ssh root@160.187.229.220
```

Neu dung SSH key:

```bash
ssh -i /duong/dan/toi/key root@160.187.229.220
```

## 5. Pull code moi tren VPS

```bash
cd /opt/xemoto
git pull
```

Kiem tra commit moi nhat:

```bash
git log -1 --oneline
```

## 6. Build image moi

```bash
docker compose build
```

Neu build loi, dung lai va xem loi. App cu thuong van dang chay vi chua recreate container.

## 7. Chay lai container

```bash
docker compose up -d
docker compose ps
```

Tat ca service nen o trang thai `Up`:

```text
mssql
auth
api
gateway
admin
store
```

## 8. Kiem tra log sau deploy

```bash
docker compose logs --tail=100 api
docker compose logs --tail=100 auth
docker compose logs --tail=100 gateway
docker compose logs --tail=100 admin
docker compose logs --tail=100 store
```

Neu muon xem realtime:

```bash
docker compose logs -f api
```

## 9. Kiem tra web

Tren VPS:

```bash
curl -I https://xemoto.xyz
curl -I https://admin.xemoto.xyz
curl http://127.0.0.1:5100/health/api
curl http://127.0.0.1:5100/health/auth
```

Tren trinh duyet:

```text
https://xemoto.xyz
https://admin.xemoto.xyz
```

## 10. Neu chi sua frontend

Van co the dung lenh chung:

```bash
cd /opt/xemoto
git pull
docker compose build
docker compose up -d
```

Neu muon nhanh hon, build/chay rieng service:

```bash
docker compose build admin
docker compose up -d admin
```

Hoac voi store:

```bash
docker compose build store
docker compose up -d store
```

## 11. Neu chi sua backend

Build/chay rieng service lien quan:

```bash
docker compose build api
docker compose up -d api
```

Neu sua auth:

```bash
docker compose build auth
docker compose up -d auth
```

Neu sua gateway/route nginx noi bo:

```bash
docker compose build gateway
docker compose up -d gateway
```

Lenh chung van an toan hon neu khong chac sua service nao:

```bash
docker compose build
docker compose up -d
```

## 12. Neu co migration DB

APIService dang tu chay migration khi start. Sau khi pull code moi:

```bash
docker compose build api
docker compose up -d api
docker compose logs --tail=150 api
```

Can chu y:

- Migration loi co the lam `api` khong len.
- Nen backup DB truoc migration lon neu du lieu quan trong.
- Khong xoa volume DB.

## 13. Lenh khong nen dung

Khong dung lenh nay khi deploy lai binh thuong:

```bash
docker compose down -v
```

Vi `-v` se xoa volumes:

- `mssql-data`: du lieu SQL Server
- `api-uploads`: anh upload

Neu chi muon restart:

```bash
docker compose restart
```

Neu chi muon recreate container sau khi build:

```bash
docker compose up -d
```

## 14. Xu ly loi thuong gap

### Web van hien code cu

Chay:

```bash
cd /opt/xemoto
git log -1 --oneline
docker compose build --no-cache admin store
docker compose up -d admin store
```

Sau do hard refresh browser:

```text
Ctrl + F5
```

### API loi 500

Xem log:

```bash
docker compose logs --tail=200 api
```

### Gateway khong goi duoc API

Kiem tra container:

```bash
docker compose ps
docker compose logs --tail=100 gateway
```

Kiem tra health:

```bash
curl http://127.0.0.1:5100/health/api
curl http://127.0.0.1:5100/health/auth
```

### Nginx loi sau khi deploy

Thuong deploy code khong can sua nginx. Neu co sua nginx:

```bash
nginx -t
systemctl reload nginx
systemctl status nginx --no-pager
```

## 15. Quy trinh nhanh de nho

Local:

```powershell
cd D:\MotorTeam\MoToSale-End
git add .
git commit -m "fix: update demo"
git push origin push-ready
$split = git subtree split --prefix=v2 HEAD
git push https://github.com/tongvandong/xemoto.git "$split`:refs/heads/main"
```

VPS:

```bash
cd /opt/xemoto
git pull
docker compose build
docker compose up -d
docker compose ps
```
