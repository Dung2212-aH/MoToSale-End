using MoToSale.Common;
using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;
using MoToSale.Entities.Catalog;
using MoToSale.Entities.Identity;
using MoToSale.Entities.Inventory;
using MoToSale.Entities.Ordering;
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

    public OrderService(
        ICartRepository cart, IUnitOfWork unitOfWork, IOrderRepository orders, IReservationRepository reservations,
        IInventoryRepository inventory, IRepository<Sku> skus, IRepository<Product> products,
        IVoucherService vouchers, IVoucherRepository voucherRepo, IRepository<User> users, IPaymentRepository payments)
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
            Code = $"DH{now:yyyyMMddHHmmss}",
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
                StoreId = null,
                Qty = line.Qty,
                ReservationStatus = ReservationStatus.Active,
                ExpiresAt = now.AddMinutes(HoldMinutes),
                CreatedDate = now,
            });
        }

        _cart.ClearItems(cart.Items);
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = order.Id, ToStatus = OrderStatus.AwaitingPayment, Note = "Tạo đơn", ChangedBy = userId, CreatedDate = now });
        await _orders.SaveChangesAsync();

        return order.Id;
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
            var stores = await _inventory.GetStoreStockAsync(line.SkuId);
            result.Add(new AllocationSuggestionItem(line.Id, line.SkuId, line.ProductNameSnapshot, line.Qty, stores));
        }
        return result;
    }

    public async Task AllocateAsync(int orderId, AllocateRequest req, int? userId)
    {
        var order = await _orders.GetDetailAsync(orderId) ?? throw new OrderException("Không tìm thấy đơn hàng.");
        if (order.OrderStatus == OrderStatus.Cancelled) throw new OrderException("Đơn đã hủy.");
        if (req.Allocations is null || req.Allocations.Count == 0) throw new OrderException("Chưa có phân phối.");

        // Mỗi dòng phải được phân phối đủ số lượng.
        foreach (var line in order.Lines)
        {
            var allocated = req.Allocations.Where(a => a.OrderLineId == line.Id).Sum(a => a.Qty);
            if (allocated != line.Qty)
                throw new OrderException($"Dòng #{line.Id} cần phân phối đủ {line.Qty} (đang {allocated}).");
        }

        var now = DateTime.UtcNow;
        foreach (var a in req.Allocations)
        {
            if (a.Qty <= 0) continue;
            var line = order.Lines.First(l => l.Id == a.OrderLineId);
            var item = await _inventory.GetOrCreateItemAsync(a.StoreId, line.SkuId);
            if (item.OnHand < a.Qty)
                throw new OrderException($"Cửa hàng #{a.StoreId} không đủ tồn cho SKU #{line.SkuId}.");

            item.OnHand -= a.Qty;
            item.UpdatedDate = now;
            _inventory.AddMovement(new StockMovement
            {
                StoreId = a.StoreId, SkuId = line.SkuId, Type = (int)StockMovementType.Issue,
                QtyDelta = -a.Qty, BalanceAfter = item.OnHand, RefType = "Order", RefId = orderId,
                Reason = $"Phân phối đơn {order.Code}", PerformedBy = userId, OccurredAt = now, CreatedDate = now,
            });

            line.Allocations.Add(new Allocation { OrderLineId = line.Id, StoreId = a.StoreId, Qty = a.Qty, AllocationStatus = AllocationStatus.Planned, CreatedDate = now });
        }

        // Nhả giữ chỗ (đã chuyển thành xuất kho thực tế).
        foreach (var r in await _reservations.GetByOrderAsync(orderId))
        {
            if (r.ReservationStatus is ReservationStatus.Active or ReservationStatus.Confirmed)
            {
                r.ReservationStatus = ReservationStatus.Released;
                r.UpdatedDate = now;
            }
        }

        var from = order.OrderStatus;
        order.FulfillmentStatus = FulfillmentStatus.Allocated;
        order.OrderStatus = OrderStatus.Allocated;
        order.UpdatedDate = now;
        _orders.AddStatusHistory(new OrderStatusHistory { OrderId = orderId, FromStatus = from, ToStatus = OrderStatus.Allocated, Note = "Phân phối cửa hàng", ChangedBy = userId, CreatedDate = now });
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
                    var item = await _inventory.GetOrCreateItemAsync(alloc.StoreId, line.SkuId);
                    item.OnHand += alloc.Qty;
                    item.UpdatedDate = now;
                    _inventory.AddMovement(new StockMovement
                    {
                        StoreId = alloc.StoreId, SkuId = line.SkuId, Type = (int)StockMovementType.Receipt,
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
                allocs.Add(new AllocationDto(a.Id, a.StoreId, "", a.Qty, a.AllocationStatus));
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
