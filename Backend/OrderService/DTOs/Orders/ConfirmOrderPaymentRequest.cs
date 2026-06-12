namespace OrderService.DTOs.Orders;

public class ConfirmOrderPaymentRequest
{
    /// <summary>Bank/gateway transaction reference, optional.</summary>
    public string? MaGiaoDich { get; set; }

    /// <summary>Optional note recorded on the order.</summary>
    public string? GhiChu { get; set; }

}
