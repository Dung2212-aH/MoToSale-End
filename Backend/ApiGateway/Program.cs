using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

Console.WriteLine("""
BaseCore API Gateway
Gateway:        http://localhost:5000
AuthService:    http://localhost:5001  -> /api/auth, /api/users/me
CatalogService: http://localhost:5002  -> /api/products, /api/categories, /api/stores, /api/content, /api/favorites
OrderService:   http://localhost:5003  -> /api/cart, /api/orders, /api/vouchers
PaymentService: http://localhost:5004  -> /api/payments
Health:         /health/auth, /health/catalog, /health/orders, /health/payments
""");

await app.UseOcelot();

app.Run();
