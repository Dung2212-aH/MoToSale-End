namespace OrderService.DTOs.Vouchers;

public class ValidateVoucherRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal ShippingFee { get; set; }
}