namespace OrderService.DTOs.Orders;

public class ShippingQuoteResponse
{
    public decimal ShippingFee { get; set; }
    public decimal OriginalShippingFee { get; set; }
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool IsFreeShipping { get; set; }
    public string? FreeReason { get; set; }
}
