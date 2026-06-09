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
function OnHand($sku,$tok){ [int](((Api GET '/inventory?Page=1&PageSize=300' $tok).data.items | Where-Object { $_.skuId -eq $sku } | Select-Object -First 1).onHand) }

'#################### SETUP ####################'
$admin=(Api POST '/auth/login' @{email='admin@motosale.local';password='Admin@123'}).data.token
$cust=(Api POST '/auth/login' @{email='store.smoke@motosale.local';password='Smoke@123'}).data.token
Chk ($admin -and $cust) 'login admin + customer'
$prodId=(Api GET '/products?Page=1&PageSize=1').data.items[0].id
$detail=(Api GET "/products/$prodId").data; $sku=$detail.skus[0].id; $price=[decimal]$detail.skus[0].listPrice
if($price -le 0){$price=300000}
Api POST '/inventory/adjust' @{skuId=$sku;transactionType='Import';qty=500;reason='ops-setup'} $admin | Out-Null
Chk ($sku -gt 0) "setup sku=$sku price=$price onHand=$(OnHand $sku $admin)"

function NewBankOrderDelivered($qty){
  $cc=(Api GET '/cart' $cust).data; foreach($it in @($cc.items)){ Api DELETE "/cart/items/$($it.id)" $cust|Out-Null }
  Api POST '/cart/items' @{skuId=$sku;qty=$qty} $cust|Out-Null
  $oid=(Api POST '/orders' @{shippingRecipient='Ops';shippingPhone='0900000555';shippingEmail='o@o';shippingAddress='1';receivingMethod='Delivery';orderType='FullPayment';shippingFee=0;depositAmount=0;note='ops';paymentMethod='BankTransfer';voucherCode=$null} $cust).data.id
  Api PUT "/orders/$oid/status" @{toStatus='Delivered';note='giao truoc khi thu (CK)'} $admin | Out-Null
  return $oid
}

'#################### OP-1: Hoan tien KHONG duoc vuot qua tien da thu ####################'
$o1=NewBankOrderDelivered 1
$o1d=(Api GET "/orders/$o1" $admin).data
$line1=[int](@($o1d.lines)[0].id)
Chk ($o1d.orderStatus -eq 'Delivered' -and $o1d.paymentStatus -eq 'Unpaid') "OP-1a don CK giao truoc khi thu = Delivered + Unpaid (thuc te=$($o1d.orderStatus)/$($o1d.paymentStatus))"
$ret1=(Api POST '/advanced-operations/returns' @{orderId=$o1;reason='ops test';note='x';lines=@(@{orderLineId=$line1;qty=1;itemCondition='Resellable'})} $admin)
$retId=[int]$ret1.data.id
Chk ($ret1.ok -and $retId -gt 0) "OP-1b tao phieu tra hang OK (id=$retId)" $ret1.raw
$appr=(Api POST "/advanced-operations/returns/$retId/approve" @{refundAmount=$o1d.grandTotal;refundMethod='Cash';transactionRef=$null;note='hoan'} $admin)
Chk ($appr.ok -eq $false -and $appr.status -eq 400) "OP-1c hoan tien ($($o1d.grandTotal)) > da thu (0) PHAI bi chan 400 (status=$($appr.status))" $appr.raw
$appr0=(Api POST "/advanced-operations/returns/$retId/approve" @{refundAmount=0;refundMethod='Cash';transactionRef=$null;note='thu hoi hang khong hoan tien'} $admin)
Chk ($appr0.ok) "OP-1d duyet tra hang voi hoan=0 (thu hoi hang) VAN OK" $appr0.raw

'#################### OP-2: Tra hang vuot so luong da mua ####################'
$o2=NewBankOrderDelivered 1
$line2=[int](@((Api GET "/orders/$o2" $admin).data.lines)[0].id)
$ret2=(Api POST '/advanced-operations/returns' @{orderId=$o2;reason='ops';note='x';lines=@(@{orderLineId=$line2;qty=5;itemCondition='Resellable'})} $admin)
Chk ($ret2.ok -eq $false) "OP-2 tra 5 khi chi mua 1 PHAI bi chan" $ret2.raw

'#################### OP-3..: Cung ung (don mua) ####################'
$sup=(Api GET '/business-operations/suppliers' $admin).data
$supItems = if($sup.items){$sup.items}else{$sup}
$supId = if(@($supItems).Count -gt 0){ @($supItems)[0].id } else { (Api POST '/business-operations/suppliers' @{name='NCC Ops';phone='0900';email='n@n';address='x'} $admin).data }
$po=(Api POST '/business-operations/purchases' @{supplierId=$supId;note='ops po';lines=@(@{skuId=$sku;qty=10;unitCost=100000})} $admin)
$poId=$po.data
Api POST "/business-operations/purchases/$poId/approve" $admin | Out-Null
# Over-receive
$poLineId=[int](@((Api GET '/business-operations/purchases' $admin).data.items | Where-Object {$_.id -eq $poId}).lines[0].id)
$recvOver=(Api POST "/business-operations/purchases/$poId/receive" @{note='x';lines=@(@{purchaseOrderLineId=$poLineId;qty=99})} $admin)
Chk ($recvOver.ok -eq $false) "OP-3 nhan 99 khi dat 10 PHAI bi chan" $recvOver.raw
# Receive ok 10
Api POST "/business-operations/purchases/$poId/receive" @{note='ok';lines=@(@{purchaseOrderLineId=$poLineId;qty=10})} $admin | Out-Null
# Overpay supplier (total=1,000,000)
$payOver=(Api POST "/business-operations/purchases/$poId/pay" @{amount=9999999;method='Cash';note='x'} $admin)
Chk ($payOver.ok -eq $false) "OP-4 tra NCC vuot tong don PHAI bi chan" $payOver.raw

'#################### OP-5: So quy - dao phieu 2 lan ####################'
$ct1=(Api POST '/business-operations/cash' @{transactionType='Receipt';category='Other';amount=50000;method='Cash';note='ops cash'} $admin)
$ctId=$ct1.data
Api POST "/business-operations/cash/$ctId/reverse" $admin | Out-Null
$rev2=(Api POST "/business-operations/cash/$ctId/reverse" $admin)
Chk ($rev2.ok -eq $false) "OP-5 dao phieu thu chi lan 2 PHAI bi chan" $rev2.raw

'#################### OP-6: Kho - xuat vuot ton (am) ####################'
$on=OnHand $sku $admin
$expOver=(Api POST '/inventory/adjust' @{skuId=$sku;transactionType='Export';qty=($on+1000);reason='ops am'} $admin)
Chk ($expOver.ok -eq $false) "OP-6 xuat kho vuot ton (am) PHAI bi chan" $expOver.raw

'#################### OP-7: Sua chua - nhay trang thai sai ####################'
$lk=(Api GET '/business-operations/lookups' $admin).data
$custId=($lk.customers | Select-Object -First 1).id
$ro=(Api POST '/business-operations/repairs' @{customerId=$custId;vehicleDescription='Wave';reportedIssue='Khong no';laborCost=100000;lines=@()} $admin)
$roId=$ro.data
$jump=(Api PUT "/business-operations/repairs/$roId/status" @{status='Repairing';note='nhay'} $admin)
Chk ($jump.ok -eq $false) "OP-7 sua chua nhay Received->Repairing PHAI bi chan" $jump.raw

'#################### OP-8: Bao cao NET doanh thu (tru tien hoan) ####################'
$rev0=[decimal]((Api GET '/reports' $admin).data.stats.monthRevenue)
$pos8=(Api POST '/orders/pos' @{customerName='Net';orderType='FullPayment';depositAmount=0;paymentMethod='Cash';paidAmount=$price;lines=@(@{skuId=$sku;qty=1;unitPrice=$price})} $admin)
$rev1=[decimal]((Api GET '/reports' $admin).data.stats.monthRevenue)
$pid8=[int]$pos8.data.id
$line8=[int](@((Api GET "/orders/$pid8" $admin).data.lines)[0].id)
$r8=(Api POST '/advanced-operations/returns' @{orderId=$pid8;reason='net test';note='x';lines=@(@{orderLineId=$line8;qty=1;itemCondition='Resellable'})} $admin)
$r8id=[int]$r8.data.id
$ap8=(Api POST "/advanced-operations/returns/$r8id/approve" @{refundAmount=$price;refundMethod='Cash';transactionRef=$null;note='hoan net'} $admin)
$rev2=[decimal]((Api GET '/reports' $admin).data.stats.monthRevenue)
Chk ($rev1 -gt $rev0) "OP-8a ban hang -> doanh thu tang (rev0=$rev0 -> rev1=$rev1)"
Chk ($ap8.ok -and ([math]::Abs(($rev1-$rev2)-$price) -lt 1)) "OP-8b duyet hoan tien -> doanh thu giam dung bang refund (rev1=$rev1 rev2=$rev2 refund=$price)" $ap8.raw

'#################### OP-9: Ghi giam ton < giu cho -> chan 400 sach (khong 500 lo stack) ####################'
# dam bao co giu cho > 0
$inv9=(Api GET '/inventory?Page=1&PageSize=300' $admin).data.items | Where-Object { $_.skuId -eq $sku } | Select-Object -First 1
if([int]$inv9.reserved -le 0){
  $cc9=(Api GET '/cart' $cust).data; foreach($it in @($cc9.items)){ Api DELETE "/cart/items/$($it.id)" $cust|Out-Null }
  Api POST '/cart/items' @{skuId=$sku;qty=2} $cust|Out-Null
  Api POST '/orders' @{shippingRecipient='Hold';shippingPhone='0900000666';shippingEmail='h@h';shippingAddress='1';receivingMethod='Delivery';orderType='FullPayment';shippingFee=0;depositAmount=0;note='hold';paymentMethod='COD';voucherCode=$null} $cust | Out-Null
  $inv9=(Api GET '/inventory?Page=1&PageSize=300' $admin).data.items | Where-Object { $_.skuId -eq $sku } | Select-Object -First 1
}
$H=[int]$inv9.onHand; $R=[int]$inv9.reserved
$exportQty = $H - $R + 1   # day onHand xuong R-1 (< giu cho)
$adj9=(Api POST '/inventory/adjust' @{skuId=$sku;transactionType='Export';qty=$exportQty;reason='ghi giam duoi giu cho'} $admin)
Chk ($adj9.ok -eq $false -and $adj9.status -eq 400) "OP-9 ghi giam ton < giu cho ($H-$exportQty < $R) PHAI bi chan 400 sach (status=$($adj9.status))" $adj9.raw

''
"==================== KET QUA: PASS=$P FAIL=$F ===================="
if($fails.Count){ "FAILED:"; $fails | ForEach-Object { " - $_" } }
