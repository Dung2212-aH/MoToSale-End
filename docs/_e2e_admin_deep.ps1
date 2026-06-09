$ErrorActionPreference='Continue'
$base='http://localhost:5100/api'; $ct='application/json; charset=utf-8'
function J($o){ ,([Text.Encoding]::UTF8.GetBytes((ConvertTo-Json -InputObject $o -Depth 10))) }
function Api($method,$path,$a3=$null,$a4=$null){
  $body=$null;$tok=$null
  foreach($x in @($a3,$a4)){ if($null -eq $x){continue}; if($x -is [string]){$tok=$x}else{$body=$x} }
  $h=@{}; if($tok){$h.Authorization="Bearer $tok"}
  $a=@{Uri="$base$path";Method=$method;Headers=$h;TimeoutSec=20;UseBasicParsing=$true}
  if($null -ne $body){$a.Body=(J $body);$a.ContentType=$ct}
  try{$r=Invoke-WebRequest @a;$d=$null;if($r.Content){try{$d=$r.Content|ConvertFrom-Json}catch{}};@{ok=$true;status=[int]$r.StatusCode;data=$d}}
  catch{$resp=$_.Exception.Response;$txt='';if($resp){try{$sr=New-Object IO.StreamReader($resp.GetResponseStream());$txt=$sr.ReadToEnd()}catch{}};@{ok=$false;status=if($resp){[int]$resp.StatusCode}else{0};raw=$txt}}
}
$P=0;$F=0;$fails=@()
function Chk($c,$n,$d=''){ if($c){$script:P++;"[PASS] $n"}else{$script:F++;$script:fails+=$n;"[FAIL] $n :: $d"} }

'#################### SETUP ####################'
$admin=(Api POST '/auth/login' @{email='admin@motosale.local';password='Admin@123'}).data.token
$staff=(Api POST '/auth/login' @{email='staff@motosale.local';password='Staff@123'}).data.token
Chk ($admin -and $staff) 'login admin+staff'
$rnd=Get-Random -Maximum 99999
$prodId=(Api GET '/products?Page=1&PageSize=1').data.items[0].id
$detail=(Api GET "/products/$prodId").data; $sku=$detail.skus[0].id; $price=[decimal]$detail.skus[0].listPrice
Api POST '/inventory/adjust' @{skuId=$sku;transactionType='Import';qty=500;reason='setup'} $admin | Out-Null
$lookups=(Api GET '/business-operations/lookups' $admin).data
$custId=($lookups.customers | Select-Object -First 1).id
$staffId=($lookups.staff | Select-Object -First 1).id
Chk ($sku -and $custId -and $staffId) "setup product/sku + lookups (cust=$custId staff=$staffId)"

'#################### 1. BAN HANG (POS / DON / VOUCHER) ####################'
# POS ban dut
$st0=(Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object { $_.skuId -eq $sku } | Select-Object -First 1
$on0=[int]$st0.onHand
$pos=(Api POST '/orders/pos' @{customerName='Khach le';orderType='FullPayment';depositAmount=0;paymentMethod='Cash';paidAmount=$price;lines=@(@{skuId=$sku;qty=1;unitPrice=$price})} $admin)
$posOrd=if($pos.ok){(Api GET "/orders/$($pos.data.id)" $admin).data}else{$null}
Chk ($pos.ok -and $posOrd.orderStatus -eq 'Delivered' -and $posOrd.paymentStatus -eq 'Paid' -and $posOrd.fulfillmentStatus -eq 'Fulfilled') '1.1 POS ban dut -> Delivered/Paid/Fulfilled' $pos.raw
$st1=(Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object { $_.skuId -eq $sku } | Select-Object -First 1
Chk ([int]$st1.onHand -eq $on0-1) "1.2 POS ban dut -> ton giam 1 ($on0 -> $($st1.onHand))"
# POS dat coc
$posD=(Api POST '/orders/pos' @{customerName='Khach coc';customerPhone='0900012345';orderType='Deposit';depositAmount=100000;paymentMethod='Cash';paidAmount=100000;lines=@(@{skuId=$sku;qty=2;unitPrice=300000})} $admin)
$depOrd=if($posD.ok){(Api GET "/orders/$($posD.data.id)" $admin).data}else{$null}
Chk ($posD.ok -and $depOrd.paymentStatus -eq 'Unpaid' -and $depOrd.depositAmount -gt 0 -and $depOrd.remainingAmount -gt 0) '1.3 POS dat coc -> Cho thanh toan + da cot coc + con no' $posD.raw
# tat toan coc + giao -> Delivered
Api POST '/payments' @{orderId=$posD.data.id;paymentType='Remaining';amount=$depOrd.remainingAmount;method='Cash'} $admin | Out-Null
Api POST "/orders/$($posD.data.id)/fulfill" $admin | Out-Null
$depAfter=(Api GET "/orders/$($posD.data.id)" $admin).data
Chk ($depAfter.paymentStatus -eq 'Paid' -and $depAfter.orderStatus -eq 'Delivered') '1.4 Coc: thu not + giao -> Delivered/Paid'
# POS loi
Chk ((Api POST '/orders/pos' @{customerName='x';orderType='FullPayment';depositAmount=0;paymentMethod='Cash';paidAmount=0;lines=@()} $admin).ok -eq $false) '1.5 POS gio rong -> chan'
Chk ((Api POST '/orders/pos' @{customerName='x';orderType='Deposit';depositAmount=99999999;paymentMethod='Cash';paidAmount=99999999;lines=@(@{skuId=$sku;qty=1;unitPrice=$price})} $admin).ok -eq $false) '1.6 POS coc >= tong -> chan'
# Voucher CRUD + chan xoa khi da dung
$vc="ADM$rnd"
$vId=(Api POST '/vouchers' @{code=$vc;description='t';discountType='Percent';discountValue=10;maxDiscount=50000;minOrderValue=0;usageLimit=$null;perUserLimit=$null;startAt=$null;endAt=$null;status=1} $admin).data.id
Chk ($vId -gt 0) '1.7 Voucher: tao'
Chk ((Api PUT "/vouchers/$vId" @{code=$vc;description='t2';discountType='Percent';discountValue=15;maxDiscount=50000;minOrderValue=0;usageLimit=$null;perUserLimit=$null;startAt=$null;endAt=$null;status=1} $admin).ok) '1.8 Voucher: sua'
Chk ((Api DELETE "/vouchers/$vId" $admin).ok) '1.9 Voucher: xoa (chua dung) OK'
# Sua don + chan sua sau xac nhan: dung don online
$cust=(Api POST '/auth/login' @{email='store.smoke@motosale.local';password='Smoke@123'}).data.token
$cc=(Api GET '/cart' $cust).data; foreach($it in @($cc.items)){ Api DELETE "/cart/items/$($it.id)" $cust|Out-Null }
Api POST '/cart/items' @{skuId=$sku;qty=1} $cust|Out-Null
$onlineId=(Api POST '/orders' @{shippingRecipient='On';shippingPhone='0900000111';shippingEmail='x@x';shippingAddress='1';receivingMethod='Delivery';orderType='FullPayment';shippingFee=0;depositAmount=0;note='on';voucherCode=$null} $cust).data.id
$upd=(Api PUT "/orders/$onlineId" @{shippingRecipient='On Sua';shippingPhone='0900000111';shippingEmail='x@x';shippingAddress='2';note='sua';lines=@(@{skuId=$sku;qty=3;unitPrice=$null})} $admin)
$updOrd=(Api GET "/orders/$onlineId" $admin).data
Chk ($upd.ok -and $updOrd.shippingRecipient -eq 'On Sua') '1.10 Sua don (Cho thanh toan) OK'
# thu tien -> xac nhan -> chan sua dong hang
Api POST '/payments' @{orderId=$onlineId;paymentType='Full';amount=$updOrd.grandTotal;method='Cash'} $admin | Out-Null
$qtyBefore=[int](@((Api GET "/orders/$onlineId" $admin).data.lines)[0].qty)
Api PUT "/orders/$onlineId" @{shippingRecipient='On';shippingPhone='0900000111';shippingEmail='x@x';shippingAddress='1';note='x';lines=@(@{skuId=$sku;qty=5;unitPrice=$null})} $admin | Out-Null
$qtyAfter=[int](@((Api GET "/orders/$onlineId" $admin).data.lines)[0].qty)
Chk ($qtyAfter -eq $qtyBefore) "1.11 Dong hang KHONG doi sau khi da thu tien ($qtyBefore giu nguyen)"
# Huy don da Delivered -> chan
Chk ((Api POST "/orders/$($pos.data.id)/cancel" @{reason='x'} $admin).ok -eq $false) '1.12 Huy don da giao (Delivered) -> chan'

'#################### 2. SAN PHAM & KHO ####################'
$cat=(Api GET '/categories').data.items[0]
$newP=(Api POST '/products' @{code="ADMP$rnd";name="SP admin $rnd";categoryId=$cat.id;kind=2;listPrice=20000} $admin)
Chk ($newP.ok -and $newP.data.id) '2.1 Tao san pham'
Chk ((Api PUT "/products/$($newP.data.id)" @{code="ADMP$rnd";name="SP admin sua $rnd";categoryId=$cat.id;kind=2;listPrice=25000} $admin).ok) '2.2 Sua san pham'
Api DELETE "/products/$($newP.data.id)" $admin | Out-Null
$pdDel=(Api GET "/products/$($newP.data.id)" $admin)
Chk ($pdDel.ok -and $pdDel.data.status -ne 1) '2.3 Xoa mem san pham (Inactive)'
# SKU chan xoa khi da phat sinh (sku chinh da co movement/order)
Chk ((Api DELETE "/products/$prodId/skus/$sku" $admin).ok -eq $false) '2.4 Xoa SKU da co don/ton -> chan'
# Ton: dieu chinh + nguong + sync + movements
$onA=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
Api POST '/inventory/adjust' @{skuId=$sku;transactionType='Import';qty=10;reason='t'} $admin | Out-Null
$onB=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
Chk ($onB -eq $onA+10) "2.5 Dieu chinh ton +10 ($onA -> $onB)"
Chk ((Api PUT '/inventory/threshold' @{skuId=$sku;reorderPoint=7} $admin).ok) '2.6 Dat nguong ton'
Chk ((Api GET '/inventory/movements?Page=1&PageSize=5' $admin).ok) '2.7 Lich su movements'
# Chung tu kho: tao Adjustment + duyet -> ton doi
$onC=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
$doc=(Api POST '/inventory/documents' @{type=1;note='nhap';reason='Supplement';lines=@(@{skuId=$sku;qty=5;note=$null})} $admin)  # 1=Receipt, reason hop le
Chk ($doc.ok -and $doc.data.id) '2.8 Tao chung tu kho (nhap)'
$appr=(Api POST "/inventory/documents/$($doc.data.id)/approve" $admin)
$onD=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
Chk ($appr.ok -and $onD -eq $onC+5) "2.9 Duyet chung tu nhap -> ton +5 ($onC -> $onD)"
Chk ((Api POST "/inventory/documents/$($doc.data.id)/cancel" $admin).ok -eq $false) '2.10 Huy chung tu da duyet -> chan'

'#################### 3. CUNG UNG ####################'
$supId=(Api POST '/business-operations/suppliers' @{code="SUP$rnd";name="NCC $rnd"} $admin).data.id
Chk ($supId -gt 0) '3.1 Tao nha cung cap'
$poId=(Api POST '/business-operations/purchases' @{supplierId=$supId;note='po';lines=@(@{skuId=$sku;qty=10;unitCost=5000})} $admin).data.id
Chk ($poId -gt 0) '3.2 Tao don mua'
Chk ((Api POST "/business-operations/purchases/$poId/approve" $admin).ok) '3.3 Duyet don mua'
$poList=(Api GET '/business-operations/purchases' $admin).data; $poArr=if($poList.items){$poList.items}else{$poList}
$poDetail=$poArr | Where-Object { $_.id -eq $poId } | Select-Object -First 1
$onE=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
$poLineId=@($poDetail.lines)[0].id
$recv=(Api POST "/business-operations/purchases/$poId/receive" @{note='nhan';lines=@(@{purchaseOrderLineId=$poLineId;qty=10})} $admin)
$onF=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
Chk ($recv.ok -and $onF -eq $onE+10) "3.4 Nhan hang -> ton +10 ($onE -> $onF)" $recv.raw
Chk ((Api POST "/business-operations/purchases/$poId/pay" @{amount=50000;method='Cash';note='tt'} $admin).ok) '3.5 Thanh toan NCC (chi quy)'

'#################### 4. DICH VU & HAU MAI ####################'
# Doi tra tu don POS ban dut (da giao)
$posLineId=($posOrd.lines | Select-Object -First 1).id
$onG=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
$ret=(Api POST '/advanced-operations/returns' @{orderId=$pos.data.id;reason='Loi';note=$null;lines=@(@{orderLineId=$posLineId;qty=1;itemCondition='Resellable'})} $admin)
Chk ($ret.ok -and $ret.data.id) '4.1 Tao phieu tra hang' $ret.raw
$appR=(Api POST "/advanced-operations/returns/$($ret.data.id)/approve" @{refundAmount=$price;refundMethod='Cash';transactionRef=$null;note='ok'} $admin)
$onH=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
Chk ($appR.ok -and $onH -eq $onG+1) "4.2 Duyet tra -> hoan ton +1 ($onG -> $onH)" $appR.raw
$refunds=@((Api GET '/advanced-operations/refunds' $admin).data.items)
Chk ((@($refunds | Where-Object {$_.orderId -eq $pos.data.id}).Count) -ge 1) '4.3 Sinh phieu hoan tien'
Chk ((Api PUT "/advanced-operations/returns/$($ret.data.id)" @{orderId=$pos.data.id;reason='x';note='x';lines=@(@{orderLineId=$posLineId;qty=1;itemCondition='Resellable'})} $admin).ok -eq $false) '4.4 Sua phieu tra da duyet -> chan'
# Bao hanh
$wId=(Api POST '/warranties' @{skuId=$sku;productSnapshot='SP';customerName='KH';customerPhone='0900';frameNumber='F1';engineNumber='E1';reportedIssue='loi';months=12;startAt=$null} $admin).data.id
Chk ($wId -gt 0) '4.5 Tao bao hanh'
Chk ((Api PUT "/warranties/$wId" @{skuId=$sku;productSnapshot='SP2';months=24;reportedIssue='loi2'} $admin).ok) '4.6 Sua bao hanh khi moi tiep nhan'
Chk ((Api PATCH "/warranties/$wId/status" @{status='Completed';note='xong';actualCost=50000} $admin).ok) '4.7 Chuyen trang thai bao hanh'
# Sua chua
$rep=(Api POST '/business-operations/repairs' @{customerId=$custId;assignedStaffId=$staffId;warrantyId=$null;vehicleDescription='Xe';reportedIssue='Hong';laborCost=100000;note=$null;lines=@(@{skuId=$sku;description='Phu tung';qty=1;unitPrice=30000})} $admin)
Chk ($rep.ok -and $rep.data.id) '4.8 Tao phieu sua chua (kem phu tung)' $rep.raw
if($rep.ok){
  $rid=$rep.data.id
  Api PUT "/business-operations/repairs/$rid/status" @{status='Inspecting';note='kiem tra'} $admin | Out-Null
  Api PUT "/business-operations/repairs/$rid/status" @{status='Quoted';note='bao gia'} $admin | Out-Null
  $onI=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
  $r1=(Api PUT "/business-operations/repairs/$rid/status" @{status='Repairing';note='sua'} $admin)
  $onJ=[int]((Api GET '/inventory?Page=1&PageSize=200' $admin).data.items | Where-Object {$_.skuId -eq $sku} | Select-Object -First 1).onHand
  Chk ($r1.ok -and $onJ -eq $onI-1) "4.9 Sua chua Received->Inspecting->Quoted->Repairing + xuat kho phu tung ($onI->$onJ)" $r1.raw
}
# CSKH
$crm=(Api POST '/business-operations/interactions' @{customerId=$custId;assignedStaffId=$staffId;interactionType='Call';subject='Tu van';note=$null;followUpAt=$null} $admin)
Chk ($crm.ok -and $crm.data.id) '4.10 Tao tuong tac CSKH' $crm.raw
if($crm.ok){ Chk ((Api POST "/business-operations/interactions/$($crm.data.id)/complete" $admin).ok) '4.11 Hoan thanh CSKH' }
# Reviews list
Chk ((Api GET '/reviews?Page=1&PageSize=5' $admin).ok) '4.12 Danh sach danh gia (admin)'

'#################### 5. TAI CHINH & BAO CAO ####################'
$cashId=(Api POST '/business-operations/cash' @{transactionType='Receipt';category='Other';amount=200000;method='Cash';referenceType=$null;referenceId=$null;note='thu khac';occurredAt=$null} $admin).data.id
Chk ($cashId -gt 0) '5.1 Lap phieu thu quy'
Chk ((Api POST "/business-operations/cash/$cashId/reverse" $admin).ok) '5.2 Dao phieu quy'
Chk ((Api GET '/advanced-operations/receivables' $admin).ok) '5.3 Cong no phai thu'
$dash=(Api GET '/reports/dashboard' $admin).data
Chk ($null -ne $dash.stats.cogs -and $null -ne $dash.stats.grossProfit) '5.4 Dashboard COGS + lai gop'
$rep2=(Api GET '/reports?from=2026-01-01&to=2026-12-31' $admin)
Chk ($rep2.ok) '5.5 Bao cao theo ky'
Chk ((Api GET '/inventory/export' $admin).ok) '5.6 Xuat Excel ton kho'

'#################### 6. HE THONG ####################'
$stEmail="adminstaff$rnd@x.com"
$nu=(Api POST '/users' @{fullName='NV moi';email=$stEmail;phoneNumber='0900999';password='Nv@12345';role='Staff'} $admin)
Chk ($nu.ok) '6.1 Tao tai khoan Staff' $nu.raw
# khoa mem 1 khach
$em2="lk$rnd@x.com"; Api POST '/auth/register' @{fullName='L';email=$em2;phoneNumber='0900777';password='Lk@12345'} | Out-Null
$uid2=(@((Api GET '/users/customers?Page=1&PageSize=200' $admin).data.items)|Where-Object{$_.email -eq $em2}|Select-Object -First 1).id
Api DELETE "/users/$uid2" $admin | Out-Null
$u2=(@((Api GET '/users/customers?Page=1&PageSize=200' $admin).data.items)|Where-Object{$_.email -eq $em2}|Select-Object -First 1)
Chk ($u2.status -eq 0 -and (Api POST '/auth/login' @{email=$em2;password='Lk@12345'}).ok -eq $false) '6.2 Khoa mem tai khoan'
# Ca lam + chong trung
$s1=(Api POST '/advanced-operations/shifts' @{staffUserId=$staffId;startsAt='2026-07-01T08:00:00';endsAt='2026-07-01T12:00:00';note=$null} $admin)
Chk ($s1.ok) '6.3 Tao ca lam' $s1.raw
$s2=(Api POST '/advanced-operations/shifts' @{staffUserId=$staffId;startsAt='2026-07-01T10:00:00';endsAt='2026-07-01T14:00:00';note=$null} $admin)
Chk ($s2.ok -eq $false) '6.4 Ca trung gio cung NV -> chan'
if($s1.ok){ Chk ((Api DELETE "/advanced-operations/shifts/$($s1.data.id)" $admin).ok) '6.5 Huy ca lam' }
# Cau hinh + audit
Chk ((Api PUT '/operations/settings' @{items=@(@{key='DepositPolicy';value="Coc 30% ($rnd)"})} $admin).ok) '6.6 Luu cau hinh'
Chk (@((Api GET '/audit-logs?Page=1&PageSize=5' $admin).data.items).Count -ge 0) '6.7 Nhat ky kiem toan'

'#################### 7. PHAN QUYEN (Staff bi chan Admin-only) ####################'
Chk ((Api POST '/business-operations/suppliers' @{code="x$rnd";name='x'} $staff).status -eq 403) '7.1 Staff chan tao NCC'
Chk ((Api GET '/reports/dashboard' $staff).ok) '7.2 Staff XEM duoc /reports (Admin+Staff theo thiet ke)'
Chk ((Api GET '/users?Page=1&PageSize=1' $staff).status -eq 403) '7.3 Staff chan /users (list)'
Chk ((Api POST '/business-operations/cash' @{transactionType='Receipt';category='x';amount=1;method='Cash'} $staff).status -eq 403) '7.4 Staff chan so quy'
Chk ((Api POST '/orders/pos' @{customerName='x';orderType='FullPayment';depositAmount=0;paymentMethod='Cash';paidAmount=$price;lines=@(@{skuId=$sku;qty=1;unitPrice=$price})} $staff).ok) '7.5 Staff DUOC dung POS'

"`n#################### SUMMARY ####################"
"PASS = $P    FAIL = $F"
if($F -gt 0){ "FAILED: " + ($fails -join ' | ') }
