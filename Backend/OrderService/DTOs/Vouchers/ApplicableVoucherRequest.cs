namespace OrderService.DTOs.Vouchers;

public class ApplicableVoucherRequest
{
    public string? OrderType { get; set; }
    public decimal Subtotal { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
    public List<int> BrandIds { get; set; } = new();
}