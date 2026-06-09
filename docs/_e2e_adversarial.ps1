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
$rnd=Get-Random -Maximum 99999
$prodId=(Api GET '/products?Page=1&PageSize=1').data.items[0].id
$detail=(Api GET "/products/$prodId").data; $sku=$detail.skus[0].id; $price=[decimal]$detail.skus[0].listPrice
if($price -le 0){$price=300000}
Api POST '/inventory/adjust' @{skuId=$sku;transactionType='Import';qty=500;reason='adv-setup'} $admin | Out-Null
Chk ($sku -gt 0) "setup sku=$sku price=$price"

function NewOnlineOrder($qty,$method,$voucher){
  $cc=(Api GET '/cart' $cust).data; foreach($it in @($cc.items)){ Api DELETE "/cart/items/$($it.id)" $cust|Out-Null }
  Api POST '/cart/items' @{skuId=$sku;qty=$qty} $cust|Out-Null
  (Api POST '/orders' @{shippingRecipient='Adv';shippingPhone='0900000222';shippingEmail='a@a';shippingAddress='1';receivingMethod='Delivery';orderType='FullPayment';shippingFee=0;depositAmount=0;note='adv';paymentMethod=$method;voucherCode=$voucher} $cust).data.id
}

'#################### ADV-1: HUY DON DA XUAT KHO (Shipping) -> hoan ton? ####################'
$o1=NewOnlineOrder 2 'COD' $null
$g1=(Api GET "/orders/$o1" $admin).data.grandTotal
Api POST '/payments' @{orderId=$o1;paymentType='Full';amount=$g1;method='Cash'} $admin | Out-Null
$onBefore=OnHand $sku $admin
$lineId=[int](@((Api GET "/orders/$o1" $admin).data.lines)[0].id)
$alloc=(Api POST "/orders/$o1/allocate" @{allocations=@(@{orderLineId=$lineId;qty=2})} $admin)
$onAfterAlloc=OnHand $sku $admin
$o1state=(Api GET "/orders/$o1" $admin).data
Chk ($alloc.ok -and $onAfterAlloc -eq $onBefore-2 -and $o1state.orderStatus -eq 'Shipping') "ADV-1a allocate -> ton -2, Shipping ($onBefore -> $onAfterAlloc)" $alloc.raw
$cancel=(Api POST "/orders/$o1/cancel" @{reason='adv huy sau xuat kho'} $admin)
$onAfterCancel=OnHand $sku $admin
$o1after=(Api GET "/orders/$o1" $admin).data
Chk ($cancel.ok -and $o1after.orderStatus -eq 'Cancelled') "ADV-1b huy don Shipping duoc chap nhan"
Chk ($onAfterCancel -eq $onBefore) "ADV-1c HUY don da xuat kho PHAI hoan ton ve $onBefore (thuc te=$onAfterCancel)"

'#################### ADV-2: Chuyen khoan -> Cho xac nhan CK -> xac nhan -> Paid (2 truc doc lap) ####################'
$o2=NewOnlineOrder 1 'BankTransfer' $null
$claim=(Api POST "/orders/$o2/payment-claim" $cust)
$payId=$claim.data.id
$o2claim=(Api GET "/orders/$o2" $admin).data
Chk ($o2claim.paymentStatus -eq 'PendingConfirmation') "ADV-2a khach bao CK -> PaymentStatus=Cho xac nhan CK (thuc te=$($o2claim.paymentStatus))"
$conf=(Api POST "/payments/$payId/confirm" $admin)
$o2state=(Api GET "/orders/$o2" $admin).data
Chk ($conf.ok -and $o2state.paymentStatus -eq 'Paid') "ADV-2b xac nhan CK -> Paid" $conf.raw
Chk ($o2state.orderStatus -eq 'Pending') "ADV-2c thanh toan KHONG doi trang thai don (van Pending - 2 truc doc lap; thuc te=$($o2state.orderStatus))"

'#################### ADV-3: Huy don DA THANH TOAN -> tien xu ly the nao? ####################'
$o3=NewOnlineOrder 1 'COD' $null
$g3=(Api GET "/orders/$o3" $admin).data.grandTotal
Api POST '/payments' @{orderId=$o3;paymentType='Full';amount=$g3;method='Cash'} $admin | Out-Null
$cancel3=(Api POST "/orders/$o3/cancel" @{reason='adv huy don da tra tien'} $admin)
$o3after=(Api GET "/orders/$o3" $admin).data
Chk ($cancel3.ok) "ADV-3a huy don da thanh toan duoc chap nhan"
# Sau khi huy, dom van la 'Paid' nhung khong co phieu hoan tien => tien bi giu, mismatch
Chk ($o3after.paymentStatus -ne 'Paid') "ADV-3b huy don da tra tien: paymentStatus KHONG con 'Paid' (thuc te=$($o3after.paymentStatus); con tien chua hoan?)"

'#################### ADV-4: Voucher UsedCount hoan lai khi huy don? ####################'
$vc="ADV$rnd"
$vId=(Api POST '/vouchers' @{code=$vc;description='adv';discountType='Amount';discountValue=10000;maxDiscount=$null;minOrderValue=0;usageLimit=1;perUserLimit=$null;startAt=$null;endAt=$null;status=1} $admin).data.id
$o4=NewOnlineOrder 1 'COD' $vc
$vAfterUse=(Api GET "/vouchers/$vId" $admin).data.usedCount
Api POST "/orders/$o4/cancel" @{reason='adv huy don voucher'} $admin | Out-Null
$vAfterCancel=(Api GET "/vouchers/$vId" $admin).data.usedCount
Chk ($vAfterUse -eq 1) "ADV-4a dung voucher -> usedCount=1 (thuc te=$vAfterUse)"
Chk ($vAfterCancel -eq 0) "ADV-4b huy don -> voucher usedCount PHAI tra ve 0 (thuc te=$vAfterCancel)"

'#################### ADV-5: PerUserLimit co duoc enforce? ####################'
$vc2="ADP$rnd"
$vId2=(Api POST '/vouchers' @{code=$vc2;description='adv2';discountType='Amount';discountValue=5000;maxDiscount=$null;minOrderValue=0;usageLimit=$null;perUserLimit=1;startAt=$null;endAt=$null;status=1} $admin).data.id
$o5a=NewOnlineOrder 1 'COD' $vc2
$o5b=NewOnlineOrder 1 'COD' $vc2
# Neu perUserLimit duoc enforce, lan 2 cua cung 1 khach phai bi tu choi (o5b=null)
Chk ($o5a -gt 0 -and ($null -eq $o5b -or $o5b -eq 0)) "ADV-5 PerUserLimit=1: khach dung lan 2 PHAI bi chan (o5a=$o5a o5b=$o5b)"

'#################### ADV-6: Idempotency / bien ####################'
$o6=NewOnlineOrder 1 'COD' $null
$g6=(Api GET "/orders/$o6" $admin).data.grandTotal
Api POST '/payments' @{orderId=$o6;paymentType='Full';amount=$g6;method='Cash'} $admin | Out-Null
$ff1=(Api POST "/orders/$o6/fulfill" $admin)
$ff2=(Api POST "/orders/$o6/fulfill" $admin)
Chk ($ff1.ok -and $ff2.ok -eq $false) "ADV-6a double-fulfill PHAI bi chan lan 2"
$over=(Api POST '/payments' @{orderId=$o6;paymentType='Full';amount=$g6;method='Cash'} $admin)
Chk ($over.ok -eq $false) "ADV-6b thu vuot tong PHAI bi chan"

'#################### ADV-7: Huy don dang Cho xac nhan CK -> Failed ####################'
$o7=NewOnlineOrder 1 'BankTransfer' $null
Api POST "/orders/$o7/payment-claim" $cust | Out-Null
Api POST "/orders/$o7/cancel" @{reason='adv huy don cho xac nhan CK'} $admin | Out-Null
$o7after=(Api GET "/orders/$o7" $admin).data
Chk ($o7after.orderStatus -eq 'Cancelled' -and $o7after.paymentStatus -eq 'Failed') "ADV-7 huy don cho-xac-nhan-CK -> Cancelled + Failed (thuc te=$($o7after.orderStatus)/$($o7after.paymentStatus))"

''
"==================== KET QUA: PASS=$P FAIL=$F ===================="
if($fails.Count){ "FAILED:"; $fails | ForEach-Object { " - $_" } }
