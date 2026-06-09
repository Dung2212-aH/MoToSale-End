$ErrorActionPreference = 'Continue'
$base = 'http://localhost:5100/api'
$ct = 'application/json; charset=utf-8'
function J($o){ ,([Text.Encoding]::UTF8.GetBytes((ConvertTo-Json -InputObject $o -Depth 8))) }
$script:pass=0; $script:fail=0; $script:fails=@()
function Chk($cond,$name,$detail=''){ if($cond){ $script:pass++; "[PASS] $name" } else { $script:fail++; $script:fails+=$name; "[FAIL] $name :: $detail" } }
function Api($method,$path,$a3=$null,$a4=$null){
  # Linh hoat: string = token, hashtable = body (bat ke vi tri)
  $body=$null; $tok=$null
  foreach($x in @($a3,$a4)){ if($null -eq $x){ continue }; if($x -is [string]){ $tok=$x } else { $body=$x } }
  $h=@{}; if($tok){ $h.Authorization = "Bearer $tok" }
  $a=@{ Uri="$base$path"; Method=$method; Headers=$h; TimeoutSec=15; UseBasicParsing=$true }
  if($null -ne $body){ $a.Body=(J $body); $a.ContentType=$ct }
  try { $r=Invoke-WebRequest @a; $d=$null; if($r.Content){ try{$d=$r.Content|ConvertFrom-Json}catch{} }; return @{ ok=$true; status=[int]$r.StatusCode; data=$d } }
  catch {
    $resp=$_.Exception.Response
    if($resp){ $code=[int]$resp.StatusCode; $txt=''; try{ $sr=New-Object IO.StreamReader($resp.GetResponseStream()); $txt=$sr.ReadToEnd() }catch{}; $d=$null; try{$d=$txt|ConvertFrom-Json}catch{}; return @{ ok=$false; status=$code; data=$d; raw=$txt } }
    return @{ ok=$false; status=0; err=$_.Exception.Message }
  }
}

'#################### SETUP ####################'
$ok=$false; for($i=1;$i -le 20;$i++){ try{ Invoke-RestMethod "http://localhost:5100/health/api" -TimeoutSec 3 | Out-Null; $ok=$true; break }catch{ Start-Sleep 3 } }
Chk $ok 'SETUP API healthy'
$admin = (Api POST '/auth/login' @{ email='admin@motosale.local'; password='Admin@123' }).data.token
$staff = (Api POST '/auth/login' @{ email='staff@motosale.local'; password='Staff@123' }).data.token
Chk ($admin -and $staff) 'SETUP admin+staff login'
$rnd = Get-Random -Maximum 999999
$emailA = "e2eA$rnd@motosale.local"; $emailB = "e2eB$rnd@motosale.local"
Api POST '/auth/register' @{ fullName='E2E KhachA'; email=$emailA; phoneNumber='0900000111'; password='E2e@123' } | Out-Null
Api POST '/auth/register' @{ fullName='E2E KhachB'; email=$emailB; phoneNumber='0900000222'; password='E2e@123' } | Out-Null
$custA = (Api POST '/auth/login' @{ email=$emailA; password='E2e@123' }).data.token
$custB = (Api POST '/auth/login' @{ email=$emailB; password='E2e@123' }).data.token
Chk ($custA -and $custB) 'SETUP 2 customers'
$prod = (Api GET '/products?Page=1&PageSize=5').data.items[0]
$prodId = $prod.id
$detail = (Api GET "/products/$prodId").data
$skuId = $detail.skus[0].id
$price = $detail.skus[0].listPrice
Api POST '/inventory/adjust' @{ skuId=$skuId; transactionType='Import'; qty=200; reason='E2E setup' } $admin | Out-Null
Chk ($skuId -gt 0) "SETUP product #$prodId sku #$skuId + stock"

'#################### A. STOREFRONT ####################'
Chk ((Api POST '/auth/login' @{ email=$emailA; password='WRONG' }).ok -eq $false) 'A12 Login sai mat khau bi tu choi'
Chk ((Api POST '/auth/register' @{ fullName='x'; email=$emailA; phoneNumber='0900000111'; password='E2e@123' }).ok -eq $false) 'A13 Dang ky trung email bi chan'
Chk (@((Api GET '/products?Page=1&PageSize=3').data.items).Count -ge 1) 'A2 Danh sach san pham + phan trang'
$cat = (Api GET '/categories').data; $catList = if($cat.items){$cat.items}else{$cat}
Chk (@($catList).Count -ge 1 -and ((Api GET "/products?CategoryId=$($catList[0].id)&Page=1&PageSize=3").ok)) 'A2 Loc theo danh muc'
Chk ((Api GET "/products/$prodId").data.skus) 'A3 Chi tiet san pham + SKU'
Chk ((Api GET "/products/$prodId/reviews").ok -and (Api GET "/products/$prodId/reviews/summary").ok) 'A3 Review cong khai + summary (an danh)'
$c=(Api GET '/cart' $custA).data; foreach($it in @($c.items)){ if($it){ Api DELETE "/cart/items/$($it.id)" $null $custA | Out-Null } }
$addR = Api POST '/cart/items' @{ skuId=$skuId; qty=3 } $custA
$cart=(Api GET '/cart' $custA).data; $items=@($cart.items)
if($items.Count -gt 0){
  Api PUT "/cart/items/$($items[0].id)" @{ qty=2 } $custA | Out-Null
  $cart2=(Api GET '/cart' $custA).data
  Chk (@($cart2.items)[0].qty -eq 2) 'A4 Gio: them + sua so luong'
} else {
  Chk $false 'A4 Gio: them + sua so luong' ("add ok=$($addR.ok) status=$($addR.status) raw=$($addR.raw)")
}
Chk ((Api POST '/cart/items' @{ skuId=$skuId; qty=999999 } $custA).ok -eq $false) 'A4/X-EDGE Them vuot ton bi chan'
$cE=(Api GET '/cart' $custB).data; foreach($it in @($cE.items)){ Api DELETE "/cart/items/$($it.id)" $null $custB | Out-Null }
Chk ((Api POST '/orders' @{ shippingRecipient='B'; shippingPhone='0900000222'; shippingEmail=$emailB; shippingAddress='x'; receivingMethod='Delivery'; orderType='FullPayment'; shippingFee=0; depositAmount=0; note='empty'; voucherCode=$null } $custB).ok -eq $false) 'A5 Checkout gio rong bi chan'
$o1=(Api POST '/orders' @{ shippingRecipient='E2E KhachA'; shippingPhone='0900000111'; shippingEmail=$emailA; shippingAddress='123 Test HN'; receivingMethod='Delivery'; orderType='FullPayment'; shippingFee=0; depositAmount=0; note='E2E O1'; voucherCode=$null } $custA).data
Chk ($o1.id -gt 0) 'A5 Dat hang hop le (COD)'
Chk (@((Api GET '/orders/mine' $custA).data.items | Where-Object { $_.id -eq $o1.id }).Count -ge 1) 'A7 Don cua toi chua don vua tao'
Chk ((Api GET "/orders/$($o1.id)" $custA).data.orderStatus) 'A8 Xem chi tiet don'
Api POST "/favorites/$prodId" $null $custA | Out-Null
$fav=(Api GET '/favorites' $custA).data
$favItem=$fav.items | Where-Object { $_.productId -eq $prodId } | Select-Object -First 1
Chk ($favItem -and $favItem.product) 'A9 Yeu thich: them (kem product)'
Api POST "/favorites/$prodId" $null $custA | Out-Null
Chk (@((Api GET '/favorites' $custA).data.items|Where-Object{$_.productId -eq $prodId}).Count -eq 1) 'A9 Yeu thich idempotent'
Api DELETE "/favorites/$prodId" $null $custA | Out-Null
Chk (@((Api GET '/favorites' $custA).data.items|Where-Object{$_.productId -eq $prodId}).Count -eq 0) 'A9 Bo yeu thich'
Api PUT '/users/me' @{ fullName='E2E KhachA Sua'; phoneNumber='0900000999' } $custA | Out-Null
Chk ((Api GET '/users/me' $custA).data.fullName -eq 'E2E KhachA Sua') 'A14 Cap nhat ho so'
Api POST '/users/me/addresses' @{ recipientName='E2E A'; phone='0900000111'; line='1 Test'; ward='P1'; district='Q1'; province='HN'; isDefault=$true } $custA | Out-Null
$ad=(Api GET '/users/me/addresses' $custA).data; $adl=if($ad.items){$ad.items}else{$ad}
Chk (@($adl).Count -ge 1) 'A14 Them + doc dia chi'
$longName = ('TenRatDai ' * 20)
$tn=(Api PUT '/users/me' @{ fullName=$longName; phoneNumber='0900000999' } $custA)
Chk ($tn.ok -or $tn.status -eq 400) 'A14/X-EDGE Ten qua dai xu ly graceful (khong 500)' "status=$($tn.status)"
Api PUT '/users/me/password' @{ currentPassword='E2e@123'; newPassword='E2e@789' } $custA | Out-Null
Chk ((Api POST '/auth/login' @{ email=$emailA; password='E2e@789' }).data.token) 'A14 Doi mat khau + dang nhap lai'
$custA = (Api POST '/auth/login' @{ email=$emailA; password='E2e@789' }).data.token
Chk ((Api GET '/showrooms').data[0].name) 'A11 He thong cua hang (/showrooms)'
Chk ((Api GET '/content/home-banners').ok) 'A1 Banner trang chu'

'#################### B. ADMIN ####################'
Chk ((Api POST '/business-operations/suppliers' @{ code="SUP$rnd"; name='x' } $staff).status -eq 403) 'B/X-SEC Staff bi chan tao NCC'
$dash=(Api GET '/reports/dashboard' $admin).data
Chk ($null -ne $dash.stats.cogs -and $null -ne $dash.stats.grossProfit) 'B4 Dashboard co COGS + lai gop'
Chk ((Api GET '/reports?from=2026-01-01&to=2026-12-31' $admin).ok) 'B4 Bao cao theo ky'
Chk ((Api GET '/inventory?Page=1&PageSize=3' $admin).ok) 'B2 Ton kho (admin)'
Chk ((Api GET '/vouchers?Page=1&PageSize=3' $admin).ok) 'B1 Voucher list (admin)'
Chk (@((Api GET '/audit-logs?Page=1&PageSize=3' $admin).data.items).Count -ge 0) 'B5 Nhat ky kiem toan'
$pos=(Api POST '/orders/pos' @{ customerName='Khach le'; orderType='FullPayment'; depositAmount=0; paymentMethod='Cash'; paidAmount=$price; lines=@(@{ skuId=$skuId; qty=1; unitPrice=$price }) } $admin)
Chk ($pos.ok -and $pos.data.id) 'B1 POS ban dut tao don'

'#################### X. CROSS 2-WAY ####################'
Chk (@((Api GET '/orders?Page=1&PageSize=50' $admin).data.items | Where-Object { $_.id -eq $o1.id }).Count -ge 1) 'X1 Admin thay don online cua khach'
$o1d=(Api GET "/orders/$($o1.id)" $admin).data
Api POST '/payments' @{ orderId=$o1.id; paymentType='Full'; amount=$o1d.grandTotal; method='Cash'; transactionRef=$null; note='E2E full pay' } $admin | Out-Null
Api POST "/orders/$($o1.id)/fulfill" $null $admin | Out-Null
$o1after=(Api GET "/orders/$($o1.id)" $custA).data
Chk ($o1after.paymentStatus -eq 'Paid' -and $o1after.orderStatus -eq 'Delivered') 'X1 Thu du + giao -> khach thay Da giao/Da thanh toan'
$rstate=(Api GET "/reviews/product/$prodId/me" $custA).data
Chk ($rstate.canReview -eq $true) 'X3 Khach du dieu kien danh gia'
Api POST "/products/$prodId/reviews" @{ rating=5; title='Tot'; comment='E2E review 2 chieu'; orderId=$o1.id } $custA | Out-Null
$pend=(Api GET '/reviews?status=Pending&Page=1&PageSize=50' $admin).data
$rv=$pend.items | Where-Object { $_.comment -like '*E2E review 2 chieu*' } | Select-Object -First 1
Chk ($rv) 'X3 Admin thay review cho duyet'
if($rv){ Api PATCH "/reviews/$($rv.id)/status" @{ status='Approved' } $admin | Out-Null }
Chk (@((Api GET "/products/$prodId/reviews").data.items).Count -ge 1) 'X3 Review duyet -> hien cong khai'
$contact=(Api POST '/content/contacts' @{ fullName="LienHe$rnd"; phone='0900000333'; email='lh@x.com'; subject='Tu van'; body='E2E lien he'; type='Consultation'; productId=$null })
Chk ($contact.data.id -gt 0) 'X4 Khach gui lien he (cong khai)'
$ctList=(Api GET '/content/contacts?Page=1&PageSize=50' $admin).data
$myCt=$ctList.items | Where-Object { $_.fullName -eq "LienHe$rnd" } | Select-Object -First 1
Chk ($myCt) 'X4 Admin thay lien he moi'
if($myCt){ Chk ((Api PATCH "/content/contacts/$($myCt.id)/process" $null $admin).ok) 'X4 Admin danh dau da xu ly' }
Api PUT '/operations/settings' @{ items=@(@{ key='StoreName'; value="E2E Shop $rnd" }) } $admin | Out-Null
Chk ((Api GET '/showrooms').data[0].name -eq "E2E Shop $rnd") 'X5 Cau hinh cua hang dong bo sang storefront'
Api POST '/content/posts' @{ title="Bai E2E $rnd"; slug="bai-e2e-$rnd"; summary='s'; body='Noi dung'; category='Tin'; postStatus='Published' } $admin | Out-Null
Api POST '/content/posts' @{ title="Draft E2E $rnd"; slug="draft-e2e-$rnd"; summary='s'; body='Noi dung'; category='Tin'; postStatus='Draft' } $admin | Out-Null
$pubList=(Api GET '/content/posts/public').data; $pubItems = if($pubList.items){$pubList.items}else{$pubList}
Chk (@($pubItems|Where-Object{$_.title -eq "Bai E2E $rnd"}).Count -ge 1 -and @($pubItems|Where-Object{$_.title -eq "Draft E2E $rnd"}).Count -eq 0) 'X6 Published hien cong khai, Draft thi khong'
$custList=(Api GET '/users/customers?Page=1&PageSize=200' $admin).data; $custItems = if($custList.items){$custList.items}else{$custList}
Chk (@($custItems|Where-Object{$_.email -eq $emailA}).Count -ge 1) 'X7 Khach dang ky tu storefront hien o admin'
$posDep=(Api POST '/orders/pos' @{ customerName='Khach Coc'; customerPhone='0900000444'; note='E2E coc'; orderType='Deposit'; depositAmount=50000; paymentMethod='Cash'; paidAmount=50000; lines=@(@{ skuId=$skuId; qty=1; unitPrice=300000 }) } $admin)
if($posDep.ok){ $dep=(Api GET "/orders/$($posDep.data.id)" $admin).data; Chk ($dep.paymentStatus -eq 'Unpaid' -and $dep.remainingAmount -gt 0 -and $dep.depositAmount -gt 0) 'X2 Don coc (Cho thanh toan + da cot coc + con no)' } else { Chk $false 'X2 Don coc' $posDep.raw }

'#################### X-SEC ####################'
Chk ((Api GET "/orders/$($o1.id)" $custB).ok -eq $false) 'X-SEC-1 Khach B KHONG xem duoc don khach A'
Chk ((Api GET '/inventory?Page=1&PageSize=1' $custA).status -eq 403) 'X-SEC-2 Khach bi chan /inventory'
Chk ((Api GET '/reports/dashboard' $custA).status -eq 403) 'X-SEC-2 Khach bi chan /reports'
Chk ((Api GET '/vouchers?Page=1&PageSize=1' $custA).status -eq 403) 'X-SEC-2 Khach bi chan /vouchers'
Chk ((Api GET '/audit-logs?Page=1&PageSize=1' $custA).status -eq 403) 'X-SEC-2 Khach bi chan /audit-logs'
Chk ((Api GET '/cart').status -eq 401) 'X-SEC-3 /cart yeu cau dang nhap (401)'

'#################### X-EDGE / X-RACE ####################'
$codes=@(); for($k=0;$k -lt 3;$k++){ $r=(Api POST '/orders/pos' @{ customerName='Lien tiep'; orderType='FullPayment'; depositAmount=0; paymentMethod='Cash'; paidAmount=$price; lines=@(@{ skuId=$skuId; qty=1; unitPrice=$price }) } $admin); if($r.ok){ $codes+=(Api GET "/orders/$($r.data.id)" $admin).data.code } }
Chk (($codes.Count -eq 3) -and (($codes | Select-Object -Unique).Count -eq 3)) 'X-RACE-3 Nhieu don/giay -> ma khong trung (regression BUG-01)'
$vc="EDGE$rnd"
Api POST '/vouchers' @{ code=$vc; description='E2E'; discountType='Percent'; discountValue=10; maxDiscount=20000; minOrderValue=500000; usageLimit=$null; perUserLimit=$null; status=1 } $admin | Out-Null
$vLow=(Api POST '/vouchers/validate' @{ code=$vc; subtotal=100000 } $custA).data
$vHigh=(Api POST '/vouchers/validate' @{ code=$vc; subtotal=1000000 } $custA).data
Chk ($vLow.valid -eq $false) 'X-EDGE-4 Voucher duoi don toi thieu -> khong hop le'
Chk ($vHigh.valid -eq $true -and $vHigh.discountAmount -le 20000) 'X-EDGE-4 Voucher hop le + giam bi chan maxDiscount'

"`n#################### SUMMARY ####################"
"PASS = $script:pass    FAIL = $script:fail"
if($script:fail -gt 0){ "FAILED: " + ($script:fails -join ' | ') }
