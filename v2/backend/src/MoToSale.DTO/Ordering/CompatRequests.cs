namespace MoToSale.DTO.Ordering;

// ===== Compatibility request classes =====
// Lớp request "khoan dung" (tolerant) phục vụ frontend khách hàng (UNCHANGED) đang dùng model
// sản phẩm/biến thể CŨ và đặt tên field tiếng Việt. Mỗi lớp chấp nhận CẢ tên tiếng Anh (admin /
// CheckoutRequest hiện hữu) LẪN tên tiếng Việt (frontend). Tất cả nullable để controller tự map
// sang request gốc của service. Đặt ở đây để không đụng vào DTO do agent khác sở hữu.

/// <summary>
/// Thêm sản phẩm vào giỏ. Frontend gửi (maSanPham, maBienSanPham, soLuong) theo model cũ.
/// Trong v2, một Sku CHÍNH LÀ biến thể nên maBienSanPham == Sku.Id. Nếu thiếu maBienSanPham,
/// controller sẽ resolve SKU theo maSanPham. Cũng nhận tên tiếng Anh (SkuId, ProductId, Qty).
/// </summary>
public class AddCartItemCompatRequest
{
    // English (admin/native)
    public int? SkuId { get; set; }
    public int? ProductId { get; set; }
    public int? Qty { get; set; }

    // Vietnamese (frontend cũ)
    public int? MaBienSanPham { get; set; }   // = Sku.Id (biến thể)
    public int? MaSanPham { get; set; }        // = Product.Id
    public int? SoLuong { get; set; }

    public int ResolvedQty => Qty ?? SoLuong ?? 0;
    public int? ResolvedSkuId => SkuId ?? MaBienSanPham;
    public int? ResolvedProductId => ProductId ?? MaSanPham;
}

/// <summary>Cập nhật số lượng dòng giỏ. Frontend gửi { soLuong }; admin gửi { qty }.</summary>
public class UpdateCartItemCompatRequest
{
    public int? Qty { get; set; }
    public int? SoLuong { get; set; }

    public int ResolvedQty => Qty ?? SoLuong ?? 0;
}

/// <summary>
/// Tạo đơn từ giỏ. Chấp nhận cả payload tiếng Anh (giống CheckoutRequest hiện hữu) lẫn payload
/// tiếng Việt của frontend. Controller map sang CheckoutRequest gốc; phí ship tính server-side.
/// </summary>
public class CheckoutCompatRequest
{
    // ===== English (admin / CheckoutRequest tương thích ngược) =====
    public string? ShippingRecipient { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingEmail { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ReceivingMethod { get; set; }
    public string? OrderType { get; set; }
    public decimal? ShippingFee { get; set; }
    public decimal? DepositAmount { get; set; }
    public string? Note { get; set; }
    public string? VoucherCode { get; set; }

    // ===== Vietnamese (frontend cũ) =====
    public int? MaDiaChiNhanHang { get; set; }          // AddressId
    public string? HoTenNhanHang { get; set; }          // ShippingRecipient
    public string? SoDienThoaiNhanHang { get; set; }    // ShippingPhone
    public string? EmailNhanHang { get; set; }          // ShippingEmail
    public string? DiaChiNhanHang { get; set; }         // ShippingAddress
    public string? ShippingProvince { get; set; }       // ShippingProvince
    public string? MaVoucherCode { get; set; }          // VoucherCode
    public string? GhiChu { get; set; }                 // Note
    public string? PhuongThucNhanHang { get; set; }     // ReceivingMethod
    public string? LoaiDonHang { get; set; }            // OrderType
    public string? PhuongThucThanhToan { get; set; }    // PaymentMethod
    public int? SoKyTraGop { get; set; }                // số kỳ trả góp (chưa dùng tới service)
    public string? HoSoTraGop { get; set; }             // hồ sơ trả góp (chưa dùng tới service)
    public decimal? TienDatCoc { get; set; }            // DepositAmount
    public DateTime? NgayHenNhanXe { get; set; }        // PickupAppointmentAt
    public string? GhiChuGiaoNhan { get; set; }         // DeliveryNote
    public int? SoPhutGiuCho { get; set; }              // số phút giữ chỗ (chưa dùng tới service)

    // ===== Resolved (English ưu tiên, fallback tiếng Việt) =====
    public string ResolvedRecipient => ShippingRecipient ?? HoTenNhanHang ?? string.Empty;
    public string ResolvedPhone => ShippingPhone ?? SoDienThoaiNhanHang ?? string.Empty;
    public string? ResolvedEmail => ShippingEmail ?? EmailNhanHang;
    public string? ResolvedAddress => ShippingAddress ?? DiaChiNhanHang;
    public string ResolvedReceivingMethod => ReceivingMethod ?? PhuongThucNhanHang ?? "Delivery";
    public string? ResolvedOrderType => OrderType ?? LoaiDonHang;
    public string? ResolvedVoucherCode => VoucherCode ?? MaVoucherCode;
    public string? ResolvedNote => Note ?? GhiChu;
    public decimal ResolvedDepositAmount => DepositAmount ?? TienDatCoc ?? 0m;

    public string? ResolvedProvince => ShippingProvince;
    public string? ResolvedPaymentMethod => PhuongThucThanhToan;
    public DateTime? ResolvedPickupAppointmentAt => NgayHenNhanXe;
    public string? ResolvedDeliveryNote => GhiChuGiaoNhan;
    public int? ResolvedAddressId => MaDiaChiNhanHang;
}

/// <summary>Hủy đơn. Frontend gửi { lyDoHuyDon }; admin gửi { reason }.</summary>
public class CancelOrderCompatRequest
{
    public string? Reason { get; set; }
    public string? LyDoHuyDon { get; set; }

    public string? ResolvedReason => Reason ?? LyDoHuyDon;
}
