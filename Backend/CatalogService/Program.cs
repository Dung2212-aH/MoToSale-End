using System.Text;
using CatalogService.Data;
using CatalogService.Repositories.Brands;
using CatalogService.Repositories.Categories;
using CatalogService.Repositories.ProductImages;
using CatalogService.Repositories.Products;
using CatalogService.Repositories.ProductVariants;
using CatalogService.Repositories.VehicleModels;
using CatalogService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey chua duoc cau hinh.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService.Services.CatalogService>();
builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Idempotent schema upgrade for homepage-content features (featured flags + banners).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.ExecuteSqlRawAsync(
        """
        IF COL_LENGTH(N'dbo.SANPHAM', N'NoiBat') IS NULL
            ALTER TABLE dbo.SANPHAM ADD NoiBat BIT NOT NULL CONSTRAINT DF_SANPHAM_NoiBat DEFAULT(0);
        IF COL_LENGTH(N'dbo.SANPHAM', N'HotDeal') IS NULL
            ALTER TABLE dbo.SANPHAM ADD HotDeal BIT NOT NULL CONSTRAINT DF_SANPHAM_HotDeal DEFAULT(0);

        IF OBJECT_ID(N'dbo.TRANGCHU_BANNER', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.TRANGCHU_BANNER (
                MaBanner INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                ViTri VARCHAR(30) NOT NULL,
                TieuDe NVARCHAR(255) NULL,
                UrlAnh NVARCHAR(500) NOT NULL,
                LienKet NVARCHAR(500) NULL,
                ThuTu INT NOT NULL CONSTRAINT DF_TRANGCHU_BANNER_ThuTu DEFAULT(0),
                DangHoatDong BIT NOT NULL CONSTRAINT DF_TRANGCHU_BANNER_DangHoatDong DEFAULT(1),
                NgayCapNhat DATETIME2(0) NOT NULL CONSTRAINT DF_TRANGCHU_BANNER_NgayCapNhat DEFAULT(SYSDATETIME())
            );

            INSERT INTO dbo.TRANGCHU_BANNER (ViTri, TieuDe, UrlAnh, LienKet, ThuTu, DangHoatDong, NgayCapNhat)
            VALUES
                ('Slider', N'Banner chính', N'https://bizweb.dktcdn.net/100/519/812/themes/954445/assets/slider_1.jpg?1758009468922', N'/products', 0, 1, SYSDATETIME()),
                ('BannerLeft', N'Ưu đãi xe máy', N'https://bizweb.dktcdn.net/100/519/812/themes/954445/assets/banner_three_1.jpg?1758009468922', NULL, 0, 1, SYSDATETIME()),
                ('BannerRight', N'Ưu đãi phụ tùng', N'https://bizweb.dktcdn.net/100/519/812/themes/954445/assets/banner_three_2.jpg?1758009468922', NULL, 0, 1, SYSDATETIME()),
                ('ProductBanner', N'Sản phẩm nổi bật', N'https://bizweb.dktcdn.net/100/519/812/themes/954445/assets/image_product_3.png?1758009468922', NULL, 0, 1, SYSDATETIME());
        END;
        """);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled: services run on HTTP in dev mode

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "CatalogService" }));

app.Map("/error", () => Results.Problem());

app.Run();
