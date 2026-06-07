namespace MoToSale.Common;

public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string AwaitingPayment = "AwaitingPayment";
    public const string Confirmed = "Confirmed";
    public const string Allocated = "Allocated";
    public const string Shipping = "Shipping";
    public const string Delivered = "Delivered";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class PaymentStatus
{
    public const string Unpaid = "Unpaid";
    public const string DepositPaid = "DepositPaid";
    public const string PartiallyPaid = "PartiallyPaid";
    public const string Paid = "Paid";
    public const string Refunded = "Refunded";
}

public static class FulfillmentStatus
{
    public const string Unallocated = "Unallocated";
    public const string Allocated = "Allocated";
    public const string Shipped = "Shipped";
    public const string Fulfilled = "Fulfilled";
}

/// <summary>Trạng thái vận chuyển hiển thị cho người dùng cuối.</summary>
public static class ShippingStatus
{
    public const string Preparing = "Preparing";
    public const string Shipping = "Shipping";
    public const string Delivered = "Delivered";
}

public static class OrderType
{
    public const string FullPayment = "FullPayment";
    public const string Deposit = "Deposit";
    public const string Installment = "Installment";
}

public static class AllocationStatus
{
    public const string Planned = "Planned";
    public const string Picked = "Picked";
    public const string Shipped = "Shipped";
    public const string Fulfilled = "Fulfilled";
    public const string Cancelled = "Cancelled";
}
