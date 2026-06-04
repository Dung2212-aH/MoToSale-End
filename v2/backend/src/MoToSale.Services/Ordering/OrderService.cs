using MoToSale.Common;
using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;
using MoToSale.Entities.Catalog;
using MoToSale.Entities.Identity;
using MoToSale.Entities.Inventory;
using MoToSale.Entities.Operations;
using MoToSale.Entities.Ordering;
using MoToSale.Entities.Payments;
using MoToSale.Repository;
using MoToSale.Repository.EFCore;
using MoToSale.Repository.Inventory;
using MoToSale.Repository.Ordering;
using MoToSale.Repository.Payments;

namespace MoToSale.Services.Ordering;

public class OrderService : IOrderService
{
    private const int HoldMinutes = 30;

    private readonly ICartRepository _cart;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orders;
    private readonly IReservationRepository _reservations;
    private readonly IInventoryRepository _inventory;
    private readonly IRepository<Sku> _skus;
    private readonly IRepository<Product> _products;
    private readonly IVoucherService _vouchers;
    private readonly IVoucherRepository _voucherRepo;
    private readonly IRepository<User> _users;
    private readonly IPaymentRepository _payments;
    private readonly AppDbContext _db;

    public OrderService(
        ICartRepository cart, IUnitOfWork unitOfWork, IOrderRepository orders, IReservationRepository reservations,
        IInventoryRepository inventory, IRepository<Sku> skus, IRepository<Product> products,
        IVoucherService vouchers, IVoucherRepository voucherRepo, IRepository<User> users, IPaymentRepository payments,
        AppDbContext db)
    {
        _cart = cart;
        _unitOfWork = unitOfWork;
        _orders = orders;
        _reservations = reservations;
        _inventory = inventory;
        _skus = skus;
        _products = products;
        _vouchers = vouchers;
        _voucherRepo = voucherRepo;
        _users = users;
        _payments = payments;
        _db = db;
    }

    private async Task<Dictionary<int, string>> UserNameMapAsync(IEnumerable<int> ids)
    {
        var idset = ids.Distinct().ToList();
        if (idset.Count == 0) return new();
        var users = await _users.FindAsync(u => idset.Contains(u.Id));
        return users.ToDictionary(u => u.Id, u => u.FullName);
    }

    // ===== Availability =====
    private async Task<int> AvailableForSaleAsync(int skuId) =>
        await _inventory.GetOnHandTotalAsync(skuId) - await _reservations.GetActiveQtyAsync(skuId);

    // ===== Cart =====
    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _cart.GetWithItemsAsync(userId);
        return await MapCartAsync(cart);
    }

    public async Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request)
    {
        if (request.Qty <= 0) throw new OrderException("Số lượng phải lớn hơn 0.");
        var sku = await _skus.GetByIdAsync(request.SkuId) ?? throw new OrderException("Không tìm thấy SKU.");

        var cart = await _cart.GetOrCreateAsync(userId);
        await _cart.SaveChangesAsync(); // đảm bảo có CartId

        var existing = await _cart.GetItemAsync(cart.Id, request.SkuId);
        var newQty = (existing?.Qty ?? 0) + request.Qty;
        if (newQty > await AvailableForSaleAsync(request.SkuId))
            throw new OrderException("Số lượng tồn khả dụng không đủ.");

        var unitPrice = sku.SalePrice ?? sku.ListPrice;
        if (existing is null)
        {
            _cart.AddItem(new CartItem { CartId = cart.Id, SkuId = request.SkuId, Qty = request.Qty, UnitPriceSnapshot = unitPrice, CreatedDate = DateTime.UtcNow });
        }
        else
        {
            existing.Qty = newQty;
            existing.UnitPriceSnapshot = unitPrice;
        }
        await _cart.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateItemAsync(int userId, int itemId, UpdateCartItemRequest request)
    {
        var cart = await _cart.GetWithItemsAsync(userId) ?? throw new OrderException("Giỏ hàng trống.");
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId) ?? throw new OrderException("Không tìm thấy dòng giỏ hàng.");
        if (request.Qty <= 0) throw new OrderException("Số lượng phải lớn hơn 0.");
        if (request.Qty > await AvailableForSaleAsync(item.SkuId)) throw new OrderException("Số lượng tồn khả dụng không đủ.");
        item.Qty = request.Qty;
        await _cart.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int itemId)
    {
        var cart = await _cart.GetWithItemsAsync(userId) ?? throw new OrderException("Giỏ hàng trống.");
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is not null)
        {
            _cart.RemoveItem(item);
            await _cart.SaveChangesAsync();
        }
        return await GetCartAsync(userId);
    }

    // ===== Checkout =====
    public async Task<int> CheckoutAsync(int userId, CheckoutRequest req)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(() => CheckoutCoreAsync(userId, req));
    }

    private async Task<int> CheckoutCoreAsync(int userId, CheckoutRequest req)
    {
        var cart = await _cart.GetWithItemsAsync(userId);
        if (cart is null || cart.Items.Count == 0) throw new OrderException("Giỏ hàng trống.");

        foreach (var item in cart.Items)
        {
            if (item.Qty > await AvailableForSaleAsync(item.SkuId))
                throw new OrderException("Một số sản phẩm không đủ tồn khả dụng.");
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Code = $"DH{now:yyyyMMddHHmmssfff}",
            UserId = userId,
            Channel = "Online",
            OrderType = req.OrderType is OrderType.Deposit or OrderType.Installment ? req.OrderType : OrderType.FullPayment,
            OrderStatus = OrderStatus.AwaitingPayment,
            PaymentStatus = PaymentStatus.Unpaid,
            FulfillmentStatus = FulfillmentStatus.Unallocated,
            ShippingRecipient = req.ShippingRecipient,
            ShippingPhone = req.ShippingPhone,
            ShippingEmail = req.ShippingEmail,
            ShippingAddress = req.ShippingAddress,
            ReceivingMethod = req.ReceivingMethod,
            ShippingFee = req.ShippingFee,
            Note = req.Note,
            PlacedAt = now,
            CreatedDate = now,
        };

        foreach (var item in cart.Items)
        {
            var sku = await _skus.GetByIdAsync(item.SkuId)!;
            var product = sku is null ? null : await _products.GetByIdAsync(sku.ProductId);
            order.Lines.Add(new OrderLine
            {
                SkuId = item.SkuId,
                ProductNameSnapshot = product?.Name ?? "",
                SkuCodeSnapshot = sku?.SkuCode ?? "",
                UnitPrice = item.UnitPriceSnapshot,
                Qty = item.Qty,
                LineTotal = item.UnitPriceSnapshot * item.Qty,
                CreatedDate = now,
            });
        }

        order.Subtotal = order.Lines.Sum(l => l.LineTotal);
        order.DiscountTotal = 0;

        if (!string.IsNullOrWhiteSpace(req.VoucherCode))
        {
            var vr = await _vouchers.ValidateAsync(req.VoucherCode, order.Subtotal);
            if (!vr.Valid) throw new OrderException(vr.Message ?? "Voucher không hợp lệ.");
            order.DiscountTotal = vr.DiscountAmount;
            var voucher = await _voucherRepo.GetByCodeAsync(req.VoucherCode.Trim().ToUpperInvariant());
            if (voucher is not null) { voucher.UsedCount++; voucher.UpdatedDate = now; _voucherRepo.Update(voucher); }
        }

        order.GrandTotal = order.Subtotal - order.DiscountTotal + order.ShippingFee;

        if (order.OrderType == OrderType.Deposit)
        {
            if (req.DepositAmount <= 0 || req.DepositAmount >= order.GrandTotal)
                throw new OrderException("Tiền đặt cọc không hợp lệ.");
            order.DepositAmount = req.DepositAmount;
            order.RemainingAmount = order.GrandTotal - req.DepositAmount;
        }
        else
        {
            order.RemainingAmount = order.GrandTotal;
        }

        _orders.Add(order);
        await _orders.SaveChangesAsync(); // sinh OrderId + OrderLine.Id

        foreach (var line in order.Lines)
        {
            _reservations.Add(new Reservation
            {
                OrderId = order.Id,
                OrderLineId = line.Id,
                SkuId = line.SkuId,
                Qty = line.Qty,
                ReservationStatus = ReservationStatus.Active,
                ExpiresAt = now.AddMinutes(HoldMinutes),
                CreatedDate = now,
            });
            var resItem = await _inventory.GetOrCreateItemAsync(line.SkuId);
            resItem.Reserved += line.Qty;
            resItem.UpdatedDate = now;
        }

        _cart.ClearItems(cart.Items);
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = order.Id, ToStatus = OrderStatus.AwaitingPayment, Note = "Tạo đơn", ChangedBy = userId, CreatedDate = now });
        await _orders.SaveChangesAsync();

        return order.Id;
    }

    // ===== Bán tại quầy (POS) =====
    public async Task<int> CreatePosOrderAsync(PosOrderRequest req, int? staffUserId)
        => await _unitOfWork.ExecuteInTransactionAsync(() => CreatePosOrderCoreAsync(req, staffUserId));

    private async Task<int> CreatePosOrderCoreAsync(PosOrderRequest req, int? staffUserId)
    {
        if (req.Lines is null || req.Lines.Count == 0) throw new OrderException("Đơn tại quầy phải có ít nhất một sản phẩm.");

        var now = DateTime.UtcNow;
        var isDeposit = req.OrderType == OrderType.Deposit;

        // Khách hàng: dùng khách đã chọn, hoặc khách lẻ (tự tạo nếu chưa có).
        var customerId = req.CustomerId is > 0 ? req.CustomerId.Value : await GetOrCreateWalkInCustomerAsync(now);

        var order = new Order
        {
            Code = $"POS{now:yyyyMMddHHmmssfff}",
            UserId = customerId,
            Channel = "InStore",
            OrderType = isDeposit ? OrderType.Deposit : OrderType.FullPayment,
            OrderStatus = OrderStatus.Confirmed,
            PaymentStatus = PaymentStatus.Unpaid,
            FulfillmentStatus = FulfillmentStatus.Unallocated,
            ShippingRecipient = string.IsNullOrWhiteSpace(req.CustomerName) ? "Khách lẻ" : req.CustomerName.Trim(),
            ShippingPhone = req.CustomerPhone ?? "",
            ReceivingMethod = "Pickup",
            Note = req.Note,
            PlacedAt = now,
            CreatedDate = now,
        };

        foreach (var l in req.Lines)
        {
            if (l.Qty <= 0) throw new OrderException("Số lượng sản phẩm phải lớn hơn 0.");
            var sku = await _skus.GetByIdAsync(l.SkuId) ?? throw new OrderException($"Không tìm thấy SKU #{l.SkuId}.");
            var product = await _products.GetByIdAsync(sku.ProductId);
            var unitPrice = l.UnitPrice is > 0 ? l.UnitPrice.Value : (sku.SalePrice ?? sku.ListPrice);
            order.Lines.Add(new OrderLine
            {
                SkuId = l.SkuId,
                ProductNameSnapshot = product?.Name ?? "",
                SkuCodeSnapshot = sku.SkuCode ?? "",
                UnitPrice = unitPrice,
                Qty = l.Qty,
                LineTotal = unitPrice * l.Qty,
                CreatedDate = now,
            });
        }

        order.Subtotal = order.Lines.Sum(x => x.LineTotal);
        order.DiscountTotal = 0;
        if (!string.IsNullOrWhiteSpace(req.VoucherCode))
        {
            var vr = await _vouchers.ValidateAsync(req.VoucherCode, order.Subtotal);
            if (!vr.Valid) throw new OrderException(vr.Message ?? "Voucher không hợp lệ.");
            order.DiscountTotal = vr.DiscountAmount;
            var voucher = await _voucherRepo.GetByCodeAsync(req.VoucherCode.Trim().ToUpperInvariant());
            if (voucher is not null) { voucher.UsedCount++; voucher.UpdatedDate = now; _voucherRepo.Update(voucher); }
        }
        order.GrandTotal = order.Subtotal - order.DiscountTotal; // bán tại quầy không tính phí ship

        if (isDeposit)
        {
            if (req.DepositAmount <= 0 || req.DepositAmount >= order.GrandTotal)
                throw new OrderException("Tiền đặt cọc không hợp lệ (0 < cọc < tổng tiền).");
            order.DepositAmount = req.DepositAmount;
            order.RemainingAmount = order.GrandTotal - req.DepositAmount;
        }
        else
        {
            order.RemainingAmount = order.GrandTotal;
        }

        _orders.Add(order);
        await _orders.SaveChangesAsync(); // sinh OrderId + OrderLine.Id

        if (!isDeposit)
        {
            // Bán đứt tại quầy → xuất kho ngay (tôn trọng hàng đang giữ chỗ cho đơn cọc).
            foreach (var line in order.Lines)
            {
                if (line.Qty > await AvailableForSaleAsync(line.SkuId))
                    throw new OrderException($"Tồn khả dụng không đủ cho {line.ProductNameSnapshot}.");
                var item = await _inventory.GetOrCreateItemAsync(line.SkuId);
                item.OnHand -= line.Qty;
                item.UpdatedDate = now;
                _inventory.AddMovement(new StockMovement
                {
                    SkuId = line.SkuId, Type = (int)StockMovementType.Issue,
                    QtyDelta = -line.Qty, BalanceAfter = item.OnHand, RefType = "Order", RefId = order.Id,
                    Reason = $"Bán tại quầy {order.Code}", PerformedBy = staffUserId, OccurredAt = now, CreatedDate = now,
                });
                line.Allocations.Add(new Allocation { OrderLineId = line.Id, Qty = line.Qty, AllocationStatus = AllocationStatus.Fulfilled, CreatedDate = now });
            }
            order.FulfillmentStatus = FulfillmentStatus.Fulfilled;
        }
        else
        {
            // Đặt cọc → giữ chỗ tồn, xuất kho khi tất toán & giao xe.
            foreach (var line in order.Lines)
            {
                if (line.Qty > await AvailableForSaleAsync(line.SkuId))
                    throw new OrderException($"Tồn khả dụng không đủ cho {line.ProductNameSnapshot}.");
                _reservations.Add(new Reservation
                {
                    OrderId = order.Id, OrderLineId = line.Id, SkuId = line.SkuId, Qty = line.Qty,
                    ReservationStatus = ReservationStatus.Confirmed, ExpiresAt = now.AddDays(7), CreatedDate = now,
                });
                var resItem = await _inventory.GetOrCreateItemAsync(line.SkuId);
                resItem.Reserved += line.Qty;
                resItem.UpdatedDate = now;
            }
        }

        // Ghi nhận tiền đã thu tại quầy + thu quỹ.
        var paid = Math.Max(0, req.PaidAmount);
        if (paid > order.GrandTotal) throw new OrderException("Tiền thu vượt quá giá trị đơn.");
        if (isDeposit && paid < order.DepositAmount) throw new OrderException("Tiền thu phải tối thiểu bằng tiền cọc.");
        if (paid > 0)
        {
            var method = string.IsNullOrWhiteSpace(req.PaymentMethod) ? PaymentMethod.Cash : req.PaymentMethod;
            _payments.Add(new Payment
            {
                Code = $"TT{now:yyyyMMddHHmmssfff}", OrderId = order.Id,
                PaymentType = isDeposit ? PaymentRecordType.Deposit : PaymentRecordType.Full,
                Amount = paid, Method = method, PaymentRecordStatus = PaymentRecordStatus.Paid,
                Note = "Thu tại quầy", RecordedBy = staffUserId, PaidAt = now, CreatedDate = now,
            });
            _db.CashTransactions.Add(new CashTransaction
            {
                Code = $"CT{now:yyyyMMddHHmmssfff}", TransactionType = "Receipt", Category = "CustomerPayment",
                Amount = paid, Method = method, ReferenceType = "Payment", ReferenceId = order.Id,
                Note = $"Thu tại quầy {order.Code}", RecordedBy = staffUserId, OccurredAt = now, CreatedDate = now,
            });
            order.RemainingAmount = Math.Max(0, order.GrandTotal - paid);
            order.PaymentStatus = paid >= order.GrandTotal
                ? PaymentStatus.Paid
                : isDeposit ? PaymentStatus.DepositPaid : PaymentStatus.PartiallyPaid;
        }

        // Đơn bán đứt đã thu đủ → hoàn tất luôn.
        if (!isDeposit && order.PaymentStatus == PaymentStatus.Paid)
            order.OrderStatus = OrderStatus.Completed;

        order.UpdatedDate = now;
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = order.Id, ToStatus = order.OrderStatus, Note = "Tạo đơn tại quầy (POS)", ChangedBy = staffUserId, CreatedDate = now });
        await _orders.SaveChangesAsync();
        return order.Id;
    }

    private async Task<int> GetOrCreateWalkInCustomerAsync(DateTime now)
    {
        const string walkInEmail = "khachle@motosale.local";
        var existing = await _users.FindAsync(u => u.Email == walkInEmail);
        if (existing.Count > 0) return existing[0].Id;
        var user = new User
        {
            FullName = "Khách lẻ",
            Email = walkInEmail,
            PasswordHash = "-", // tài khoản hệ thống, không đăng nhập
            Status = (int)EntityStatus.Active,
            CreatedDate = now,
        };
        _users.Add(user);
        await _users.SaveChangesAsync();
        return user.Id;
    }

    public async Task<List<OrderListItem>> GetMyOrdersAsync(int userId)
    {
        var orders = await _orders.GetByUserAsync(userId);
        return orders.Select(o => new OrderListItem(o.Id, o.Code, o.OrderStatus, o.PaymentStatus, o.FulfillmentStatus, o.GrandTotal, o.PlacedAt, o.UserId, null, MapLineSummaries(o))).ToList();
    }

    public async Task<OrderDetail?> GetOrderAsync(int id)
    {
        var o = await _orders.GetDetailAsync(id);
        return o is null ? null : await MapDetailAsync(o);
    }

    public async Task<PagingResponse<OrderListItem>> SearchOrdersAsync(OrderSearchRequest request)
    {
        var page = await _orders.SearchAsync(request);
        var names = await UserNameMapAsync(page.Items.Select(o => o.UserId));
        return new PagingResponse<OrderListItem>
        {
            Items = page.Items.Select(o => new OrderListItem(o.Id, o.Code, o.OrderStatus, o.PaymentStatus, o.FulfillmentStatus, o.GrandTotal, o.PlacedAt, o.UserId, names.GetValueOrDefault(o.UserId), MapLineSummaries(o))).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalItems = page.TotalItems,
        };
    }

    // ===== Allocation =====
    public async Task<List<AllocationSuggestionItem>> GetAllocationSuggestionAsync(int orderId)
    {
        var order = await _orders.GetDetailAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        var result = new List<AllocationSuggestionItem>();
        foreach (var line in order.Lines)
        {
            var available = await _inventory.GetTotalAvailableAsync(line.SkuId);
            result.Add(new AllocationSuggestionItem(line.Id, line.SkuId, line.ProductNameSnapshot, line.Qty, available));
        }
        return result;
    }

    public async Task AllocateAsync(int orderId, AllocateRequest req, int? userId)
    {
        var order = await _orders.GetDetailAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        if (order.OrderStatus == OrderStatus.Cancelled) throw new OrderException("Đơn đã hủy.");
        if (req.Allocations is null || req.Allocations.Count == 0) throw new OrderException("Chưa có dòng xuất kho.");

        // Mỗi dòng phải được xuất kho đủ số lượng.
        foreach (var line in order.Lines)
        {
            var allocated = req.Allocations.Where(a => a.OrderLineId == line.Id).Sum(a => a.Qty);
            if (allocated != line.Qty)
                throw new OrderException($"Dòng #{line.Id} cần xuất đủ {line.Qty} (đang {allocated}).");
        }

        var now = DateTime.UtcNow;
        foreach (var a in req.Allocations)
        {
            if (a.Qty <= 0) continue;
            var line = order.Lines.First(l => l.Id == a.OrderLineId);
            var item = await _inventory.GetOrCreateItemAsync(line.SkuId);
            if (item.OnHand < a.Qty)
                throw new OrderException($"Tồn kho không đủ cho SKU #{line.SkuId}.");

            item.OnHand -= a.Qty;
            item.UpdatedDate = now;
            _inventory.AddMovement(new StockMovement
            {
                SkuId = line.SkuId, Type = (int)StockMovementType.Issue,
                QtyDelta = -a.Qty, BalanceAfter = item.OnHand, RefType = "Order", RefId = orderId,
                Reason = $"Xuất kho đơn {order.Code}", PerformedBy = userId, OccurredAt = now, CreatedDate = now,
            });

            line.Allocations.Add(new Allocation { OrderLineId = line.Id, Qty = a.Qty, AllocationStatus = AllocationStatus.Planned, CreatedDate = now });
        }

        // Nhả giữ chỗ (đã chuyển thành xuất kho thực tế).
        foreach (var r in await _reservations.GetByOrderAsync(orderId))
        {
            if (r.ReservationStatus is ReservationStatus.Active or ReservationStatus.Confirmed)
            {
                r.ReservationStatus = ReservationStatus.Released;
                r.UpdatedDate = now;
                var relItem = await _inventory.GetItemAsync(r.SkuId);
                if (relItem is not null) { relItem.Reserved = Math.Max(0, relItem.Reserved - r.Qty); relItem.UpdatedDate = now; }
            }
        }

        var from = order.OrderStatus;
        order.FulfillmentStatus = FulfillmentStatus.Allocated;
        order.OrderStatus = OrderStatus.Allocated;
        order.UpdatedDate = now;
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = orderId, FromStatus = from, ToStatus = OrderStatus.Allocated, Note = "Soạn hàng & xuất kho", ChangedBy = userId, CreatedDate = now });
        await _orders.SaveChangesAsync();
    }

    // ===== Giao hàng & xuất kho (chốt đơn giữ chỗ / đơn cọc) =====
    public async Task FulfillAsync(int orderId, int? userId)
        => await _unitOfWork.ExecuteInTransactionAsync(() => FulfillCoreAsync(orderId, userId));

    private async Task FulfillCoreAsync(int orderId, int? userId)
    {
        var order = await _orders.GetDetailAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        if (order.OrderStatus == OrderStatus.Cancelled) throw new OrderException("Đơn đã hủy.");
        if (order.FulfillmentStatus == FulfillmentStatus.Fulfilled) throw new OrderException("Đơn đã giao/xuất kho.");

        var now = DateTime.UtcNow;
        var alreadyIssued = order.Lines.Any(l => l.Allocations.Any(a => a.AllocationStatus != AllocationStatus.Cancelled));

        // Nếu hàng chưa xuất (đơn cọc/giữ chỗ) → trừ tồn thật bây giờ.
        if (!alreadyIssued)
        {
            foreach (var line in order.Lines)
            {
                var item = await _inventory.GetOrCreateItemAsync(line.SkuId);
                if (item.OnHand < line.Qty) throw new OrderException($"Tồn kho không đủ cho {line.ProductNameSnapshot}.");
                item.OnHand -= line.Qty;
                item.UpdatedDate = now;
                _inventory.AddMovement(new StockMovement
                {
                    SkuId = line.SkuId, Type = (int)StockMovementType.Issue,
                    QtyDelta = -line.Qty, BalanceAfter = item.OnHand, RefType = "Order", RefId = orderId,
                    Reason = $"Giao hàng đơn {order.Code}", PerformedBy = userId, OccurredAt = now, CreatedDate = now,
                });
                line.Allocations.Add(new Allocation { OrderLineId = line.Id, Qty = line.Qty, AllocationStatus = AllocationStatus.Fulfilled, CreatedDate = now });
            }
        }

        // Nhả giữ chỗ (đã giao thực tế).
        foreach (var r in await _reservations.GetByOrderAsync(orderId))
        {
            if (r.ReservationStatus is ReservationStatus.Active or ReservationStatus.Confirmed)
            {
                r.ReservationStatus = ReservationStatus.Released;
                r.UpdatedDate = now;
                var relItem = await _inventory.GetItemAsync(r.SkuId);
                if (relItem is not null) { relItem.Reserved = Math.Max(0, relItem.Reserved - r.Qty); relItem.UpdatedDate = now; }
            }
        }

        var from = order.OrderStatus;
        order.FulfillmentStatus = FulfillmentStatus.Fulfilled;
        order.OrderStatus = order.PaymentStatus == PaymentStatus.Paid ? OrderStatus.Completed : OrderStatus.Allocated;
        order.UpdatedDate = now;
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = orderId, FromStatus = from, ToStatus = order.OrderStatus, Note = "Giao hàng & xuất kho", ChangedBy = userId, CreatedDate = now });
        await _orders.SaveChangesAsync();
    }

    // ===== Sửa đơn =====
    public async Task UpdateOrderAsync(int orderId, UpdateOrderRequest req, int? userId)
        => await _unitOfWork.ExecuteInTransactionAsync(() => UpdateOrderCoreAsync(orderId, req, userId));

    private async Task UpdateOrderCoreAsync(int orderId, UpdateOrderRequest req, int? userId)
    {
        var order = await _orders.GetDetailAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        if (order.OrderStatus is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new OrderException("Đơn đã hoàn tất hoặc đã hủy, không thể sửa.");

        var now = DateTime.UtcNow;

        // Thông tin giao/khách + ghi chú — luôn cho sửa (không ảnh hưởng tiền/tồn).
        if (req.ShippingRecipient is not null) order.ShippingRecipient = req.ShippingRecipient.Trim();
        if (req.ShippingPhone is not null) order.ShippingPhone = req.ShippingPhone.Trim();
        order.ShippingEmail = req.ShippingEmail;
        order.ShippingAddress = req.ShippingAddress;
        order.Note = req.Note;

        // Sửa sản phẩm chỉ khi đơn còn Chờ thanh toán (chưa thu tiền, chưa xuất kho).
        if (req.Lines is { Count: > 0 })
        {
            if (order.OrderStatus != OrderStatus.AwaitingPayment)
                throw new OrderException("Chỉ sửa được sản phẩm khi đơn đang Chờ thanh toán (chưa xác nhận/thu tiền).");

            // Gỡ giữ chỗ cũ + dòng cũ.
            foreach (var r in await _reservations.GetByOrderAsync(orderId))
            {
                if (r.ReservationStatus is ReservationStatus.Active or ReservationStatus.Confirmed)
                {
                    var it = await _inventory.GetItemAsync(r.SkuId);
                    if (it is not null) { it.Reserved = Math.Max(0, it.Reserved - r.Qty); it.UpdatedDate = now; }
                }
                _reservations.Delete(r);
            }
            _db.OrderLines.RemoveRange(order.Lines);
            await _orders.SaveChangesAsync();

            var newLines = new List<OrderLine>();
            foreach (var l in req.Lines)
            {
                if (l.Qty <= 0) throw new OrderException("Số lượng sản phẩm phải lớn hơn 0.");
                var sku = await _skus.GetByIdAsync(l.SkuId) ?? throw new OrderException($"Không tìm thấy SKU #{l.SkuId}.");
                if (l.Qty > await AvailableForSaleAsync(l.SkuId)) throw new OrderException($"Tồn khả dụng không đủ cho SKU #{l.SkuId}.");
                var product = await _products.GetByIdAsync(sku.ProductId);
                var unitPrice = l.UnitPrice is > 0 ? l.UnitPrice.Value : (sku.SalePrice ?? sku.ListPrice);
                newLines.Add(new OrderLine
                {
                    OrderId = orderId, SkuId = l.SkuId, ProductNameSnapshot = product?.Name ?? "", SkuCodeSnapshot = sku.SkuCode ?? "",
                    UnitPrice = unitPrice, Qty = l.Qty, LineTotal = unitPrice * l.Qty, CreatedDate = now,
                });
            }
            _db.OrderLines.AddRange(newLines);
            await _orders.SaveChangesAsync(); // sinh OrderLine.Id

            foreach (var line in newLines)
            {
                _reservations.Add(new Reservation
                {
                    OrderId = orderId, OrderLineId = line.Id, SkuId = line.SkuId, Qty = line.Qty,
                    ReservationStatus = ReservationStatus.Active, ExpiresAt = now.AddMinutes(HoldMinutes), CreatedDate = now,
                });
                var it = await _inventory.GetOrCreateItemAsync(line.SkuId);
                it.Reserved += line.Qty;
                it.UpdatedDate = now;
            }

            order.Subtotal = newLines.Sum(x => x.LineTotal);
            if (order.DiscountTotal > order.Subtotal) order.DiscountTotal = order.Subtotal;
            order.GrandTotal = order.Subtotal - order.DiscountTotal + order.ShippingFee;
            order.RemainingAmount = order.GrandTotal; // đơn Chờ thanh toán = chưa thu
        }

        order.UpdatedDate = now;
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = orderId, ToStatus = order.OrderStatus, Note = "Sửa thông tin đơn", ChangedBy = userId, CreatedDate = now });
        await _orders.SaveChangesAsync();
    }

    // ===== Status =====
    public async Task CancelOrderAsync(int orderId, string? reason, int? userId)
    {
        var order = await _orders.GetDetailAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        if (order.OrderStatus == OrderStatus.Cancelled) throw new OrderException("Đơn đã hủy.");

        var now = DateTime.UtcNow;

        // Đã phân phối → trả tồn về kho.
        if (order.FulfillmentStatus == FulfillmentStatus.Allocated)
        {
            foreach (var line in order.Lines)
            {
                foreach (var alloc in line.Allocations.Where(x => x.AllocationStatus != AllocationStatus.Cancelled))
                {
                    var item = await _inventory.GetOrCreateItemAsync(line.SkuId);
                    item.OnHand += alloc.Qty;
                    item.UpdatedDate = now;
                    _inventory.AddMovement(new StockMovement
                    {
                        SkuId = line.SkuId, Type = (int)StockMovementType.Receipt,
                        QtyDelta = +alloc.Qty, BalanceAfter = item.OnHand, RefType = "Order", RefId = orderId,
                        Reason = $"Hủy đơn {order.Code}", PerformedBy = userId, OccurredAt = now, CreatedDate = now,
                    });
                    alloc.AllocationStatus = AllocationStatus.Cancelled;
                }
            }
        }

        foreach (var r in await _reservations.GetByOrderAsync(orderId))
        {
            if (r.ReservationStatus is ReservationStatus.Active or ReservationStatus.Confirmed)
            {
                r.ReservationStatus = ReservationStatus.Released;
                r.UpdatedDate = now;
                var relItem = await _inventory.GetItemAsync(r.SkuId);
                if (relItem is not null) { relItem.Reserved = Math.Max(0, relItem.Reserved - r.Qty); relItem.UpdatedDate = now; }
            }
        }

        var from = order.OrderStatus;
        order.OrderStatus = OrderStatus.Cancelled;
        order.UpdatedDate = now;
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = orderId, FromStatus = from, ToStatus = OrderStatus.Cancelled, Note = reason ?? "Hủy đơn", ChangedBy = userId, CreatedDate = now });
        await _orders.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int orderId, UpdateOrderStatusRequest req, int? userId)
    {
        var order = await _orders.GetByIdAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        var allowed = new HashSet<string> { OrderStatus.AwaitingPayment, OrderStatus.Confirmed, OrderStatus.Allocated, OrderStatus.Shipping, OrderStatus.Delivered, OrderStatus.Completed };
        if (!allowed.Contains(req.ToStatus)) throw new OrderException("Invalid order status.");
        var from = order.OrderStatus;
        var fulfillmentFrom = order.FulfillmentStatus;
        var now = DateTime.UtcNow;
        order.OrderStatus = req.ToStatus;
        if (req.ToStatus == OrderStatus.Allocated) order.FulfillmentStatus = FulfillmentStatus.Allocated;
        if (req.ToStatus == OrderStatus.Shipping) order.FulfillmentStatus = FulfillmentStatus.Shipped;
        if (req.ToStatus is OrderStatus.Delivered or OrderStatus.Completed) order.FulfillmentStatus = FulfillmentStatus.Fulfilled;
        order.UpdatedDate = now;
        _orders.Update(order);
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = orderId, FromStatus = from, ToStatus = req.ToStatus, Note = req.Note, ChangedBy = userId, CreatedDate = now });
        if (order.FulfillmentStatus != fulfillmentFrom)
        {
            _orders.AddStatusHistory(new OrderStatusHistory
            {
                OrderId = orderId, FromStatus = fulfillmentFrom, ToStatus = order.FulfillmentStatus,
                Note = "FulfillmentStatus: Synced from order status", ChangedBy = userId, CreatedDate = now,
            });
        }
        await _orders.SaveChangesAsync();
    }

    public async Task UpdateFulfillmentStatusAsync(int orderId, UpdateFulfillmentStatusRequest req, int? userId)
    {
        var order = await _orders.GetByIdAsync(orderId) ?? throw new OrderException("Order not found.");
        var allowed = new HashSet<string> { FulfillmentStatus.Unallocated, FulfillmentStatus.Allocated, FulfillmentStatus.Shipped, FulfillmentStatus.Fulfilled };
        if (!allowed.Contains(req.ToStatus)) throw new OrderException("Invalid fulfillment status.");
        var from = order.FulfillmentStatus;
        var now = DateTime.UtcNow;
        order.FulfillmentStatus = req.ToStatus;
        if (req.ToStatus == FulfillmentStatus.Shipped) order.OrderStatus = OrderStatus.Shipping;
        if (req.ToStatus == FulfillmentStatus.Fulfilled) order.OrderStatus = OrderStatus.Delivered;
        order.UpdatedDate = now;
        _orders.Update(order);
        _orders.AddStatusHistory(new OrderStatusHistory
        {
            OrderId = orderId,
            FromStatus = from,
            ToStatus = req.ToStatus,
            Note = string.IsNullOrWhiteSpace(req.Note) ? "FulfillmentStatus" : $"FulfillmentStatus: {req.Note}",
            ChangedBy = userId,
            CreatedDate = now,
        });
        await _orders.SaveChangesAsync();
    }

    // ===== Mapping =====
    private async Task<CartDto> MapCartAsync(Cart? cart)
    {
        if (cart is null || cart.Items.Count == 0)
            return new CartDto(cart?.Id ?? 0, Array.Empty<CartItemDto>(), 0, 0);

        var items = new List<CartItemDto>();
        foreach (var i in cart.Items.OrderBy(x => x.Id))
        {
            var sku = await _skus.GetByIdAsync(i.SkuId);
            var product = sku is null ? null : await _products.GetByIdAsync(sku.ProductId);
            items.Add(new CartItemDto(i.Id, i.SkuId, sku?.SkuCode ?? "", product?.Name ?? "", i.Qty, i.UnitPriceSnapshot, i.UnitPriceSnapshot * i.Qty, null));
        }
        return new CartDto(cart.Id, items, items.Sum(x => x.Qty), items.Sum(x => x.LineTotal));
    }

    private async Task<OrderDetail> MapDetailAsync(Order o)
    {
        var lines = new List<OrderLineDto>();
        foreach (var l in o.Lines)
        {
            var allocs = new List<AllocationDto>();
            foreach (var a in l.Allocations)
                allocs.Add(new AllocationDto(a.Id, a.Qty, a.AllocationStatus));
            lines.Add(new OrderLineDto(l.Id, l.SkuId, l.ProductNameSnapshot, l.SkuCodeSnapshot, l.UnitPrice, l.Qty, l.LineTotal, allocs));
        }

        var names = await UserNameMapAsync(new[] { o.UserId });
        var histories = await _orders.GetHistoriesAsync(o.Id);
        var payments = await _payments.GetByOrderAsync(o.Id);
        return new OrderDetail(
            o.Id, o.Code, o.UserId, o.OrderType, o.OrderStatus, o.PaymentStatus, o.FulfillmentStatus,
            o.Subtotal, o.DiscountTotal, o.ShippingFee, o.GrandTotal, o.DepositAmount, o.RemainingAmount,
            o.ShippingRecipient, o.ShippingPhone, o.ShippingEmail, o.ShippingAddress, o.ReceivingMethod,
            o.Note, o.PlacedAt, names.GetValueOrDefault(o.UserId), lines,
            histories.Select(x => new OrderHistoryDto(
                x.Id,
                x.Note?.StartsWith("FulfillmentStatus", StringComparison.Ordinal) == true ? "ShippingStatus"
                    : x.Note?.StartsWith("PaymentStatus", StringComparison.Ordinal) == true ? "PaymentStatus"
                    : "OrderStatus",
                x.FromStatus, x.ToStatus, x.Note, x.ChangedBy, x.CreatedDate)),
            payments.Select(x => new OrderPaymentDto(x.Id, x.Code, x.PaymentType, x.Amount, x.Method, x.PaymentRecordStatus, x.TransactionRef, x.PaidAt)));
    }

    private static IEnumerable<OrderLineSummaryDto> MapLineSummaries(Order order) =>
        order.Lines.Select(x => new OrderLineSummaryDto(x.SkuId, x.ProductNameSnapshot, x.SkuCodeSnapshot, x.UnitPrice, x.Qty, x.LineTotal));
}
