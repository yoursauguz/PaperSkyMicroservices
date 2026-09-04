using Discount.Grpc;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddDbContext<DiscountContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

app.UseMigration();

app.MapGrpcService<DiscountService>();

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

app.Run();
