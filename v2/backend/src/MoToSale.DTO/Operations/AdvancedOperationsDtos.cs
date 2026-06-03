namespace MoToSale.DTO.Operations;

public record CreateSalesReturnLineRequest(int OrderLineId, int Qty, string ItemCondition);
public record CreateSalesReturnRequest(int OrderId, int StoreId, string Reason, string? Note, List<CreateSalesReturnLineRequest> Lines);
public record ApproveSalesReturnRequest(decimal RefundAmount, string RefundMethod, string? TransactionRef, string? Note);
public record RejectSalesReturnRequest(string? Note);
public record SalesReturnLineDto(int Id, int OrderLineId, int SkuId, string ProductName, string SkuCode, int Qty, decimal UnitPrice, decimal LineTotal, string ItemCondition);
public record SalesReturnDto(int Id, string Code, int OrderId, string OrderCode, int StoreId, string ReturnStatus, string Reason, string? Note, decimal RefundAmount, decimal MaxRefundAmount, DateTime CreatedDate, DateTime? ApprovedAt, IEnumerable<SalesReturnLineDto> Lines);

public record RefundDto(int Id, string Code, int OrderId, string OrderCode, int? SalesReturnId, decimal Amount, string Method, string RefundStatus, string? Reason, string? TransactionRef, DateTime RefundedAt);
public record OrderReceivableDto(int OrderId, string OrderCode, string? CustomerName, decimal GrandTotal, decimal AdjustedTotal, decimal DepositRequired, decimal TotalPaid, decimal TotalRefunded, decimal NetPaid, decimal Outstanding, string PaymentStatus);

public record CreateStaffShiftRequest(int StaffUserId, int StoreId, DateTime StartsAt, DateTime EndsAt, string? Note);
public record UpdateStaffShiftRequest(int StoreId, DateTime StartsAt, DateTime EndsAt, string ShiftStatus, string? Note);
public record StaffShiftDto(int Id, int StaffUserId, string StaffName, int StoreId, string StoreName, DateTime StartsAt, DateTime EndsAt, string ShiftStatus, string? Note);
