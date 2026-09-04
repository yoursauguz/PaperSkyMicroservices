using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public class DiscountContext : DbContext
{
    public DbSet<Coupon> Coupons { get; set; } = default!;

    public DiscountContext(DbContextOptions<DiscountContext> options) : base (options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().HasData(
            new Coupon { Id = 1, ProductName = "Harry Potter", Description = "Harry Potter And The Sorcerer's Stone", Amount = 20 },
            new Coupon { Id = 2, ProductName = "The Lord of the Rings", Description = "The Lord of the Rings - The fellowship of the ring", Amount = 30 }
            );
    }
}
