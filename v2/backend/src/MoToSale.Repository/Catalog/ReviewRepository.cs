using Microsoft.EntityFrameworkCore;
using MoToSale.Common;
using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;
using MoToSale.Entities.Catalog;
using MoToSale.Repository.EFCore;

namespace MoToSale.Repository.Catalog;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    private const string Approved = "Approved";

    // Đơn được coi là đủ điều kiện đánh giá khi đã giao/hoàn tất.
    private static readonly string[] EligibleOrderStatuses = { OrderStatus.Delivered, OrderStatus.Completed };

    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<PagingResponse<ReviewDto>> SearchAsync(PagingRequest r, string? status)
    {
        var query =
            from rv in Set.AsNoTracking()
            join p in Context.Products.AsNoTracking() on rv.ProductId equals p.Id into ps
            from p in ps.DefaultIfEmpty()
            join u in Context.Users.AsNoTracking() on rv.UserId equals u.Id into us
            from u in us.DefaultIfEmpty()
            select new { rv, ProductName = p != null ? p.Name : "", UserName = u != null ? u.FullName : "" };

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.rv.ReviewStatus == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.rv.Id)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new ReviewDto(x.rv.Id, x.rv.ProductId, x.ProductName, x.rv.UserId, x.UserName, x.rv.Rating, x.rv.Title, x.rv.Comment, x.rv.ImageUrl, x.rv.ReviewStatus, x.rv.CreatedDate))
            .ToListAsync();
        return new PagingResponse<ReviewDto> { Items = items, Page = r.Page, PageSize = r.PageSize, TotalItems = total };
    }

    public async Task<List<CustomerReviewDto>> GetApprovedByProductAsync(int productId)
    {
        return await (
            from rv in Set.AsNoTracking()
            where rv.ProductId == productId && rv.ReviewStatus == Approved
            join u in Context.Users.AsNoTracking() on rv.UserId equals u.Id into us
            from u in us.DefaultIfEmpty()
            orderby rv.Id descending
            select new CustomerReviewDto(
                rv.Id, rv.ProductId, rv.UserId, u != null ? u.FullName : "",
                rv.Rating, rv.Title, rv.Comment, rv.ImageUrl, rv.ReviewStatus, rv.CreatedDate))
            .ToListAsync();
    }

    public async Task<ReviewSummaryDto> GetSummaryAsync(int productId)
    {
        var ratings = await Set.AsNoTracking()
            .Where(rv => rv.ProductId == productId && rv.ReviewStatus == Approved)
            .Select(rv => rv.Rating)
            .ToListAsync();

        var count = ratings.Count;
        var average = count == 0 ? 0d : Math.Round(ratings.Average(), 1);
        var breakdown = new ReviewBreakdownDto(
            Five: ratings.Count(r => r == 5),
            Four: ratings.Count(r => r == 4),
            Three: ratings.Count(r => r == 3),
            Two: ratings.Count(r => r == 2),
            One: ratings.Count(r => r == 1));

        return new ReviewSummaryDto(productId, average, count, breakdown);
    }

    public async Task<CustomerReviewDto?> GetUserReviewAsync(int productId, int userId)
    {
        return await (
            from rv in Set.AsNoTracking()
            where rv.ProductId == productId && rv.UserId == userId
            join u in Context.Users.AsNoTracking() on rv.UserId equals u.Id into us
            from u in us.DefaultIfEmpty()
            orderby rv.Id descending
            select new CustomerReviewDto(
                rv.Id, rv.ProductId, rv.UserId, u != null ? u.FullName : "",
                rv.Rating, rv.Title, rv.Comment, rv.ImageUrl, rv.ReviewStatus, rv.CreatedDate))
            .FirstOrDefaultAsync();
    }

    public async Task<Review?> GetUserReviewEntityAsync(int productId, int userId)
    {
        return await Set.FirstOrDefaultAsync(rv => rv.ProductId == productId && rv.UserId == userId);
    }

    public async Task<int?> GetEligibleOrderIdAsync(int productId, int userId)
    {
        // Đơn của người dùng, có dòng hàng thuộc sản phẩm này (so khớp qua ProductId hoặc SKU),
        // và trạng thái đơn/giao hàng cho thấy đã giao/hoàn tất.
        var query =
            from o in Context.Orders.AsNoTracking()
            where o.UserId == userId
                && (EligibleOrderStatuses.Contains(o.OrderStatus) || o.ShippingStatus == ShippingStatus.Delivered)
                && o.Lines.Any(l =>
                    l.ProductId == productId
                    || Context.Skus.Any(s => s.Id == l.SkuId && s.ProductId == productId))
            orderby o.Id descending
            select (int?)o.Id;

        return await query.FirstOrDefaultAsync();
    }

    public Task<bool> ProductExistsAsync(int productId) =>
        Context.Products.AsNoTracking().AnyAsync(p => p.Id == productId);
}
