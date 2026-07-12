using ECommerce_Application.Models;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ECommerceApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<PasswordResetOTP> PasswordResetOTPs { get; set; }

        // Fluent API configurations Relationships and constraints
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //  customer → cart (One-to-One)
            builder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.CustomerId);

            // Category self-referencing relationship (Parent-Child)
            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → Vendor (Many-to-One)
            builder.Entity<Product>()
                .HasOne(p => p.Vendor)
                .WithMany(v => v.Products)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            //Review → Customer (Many-to-One)
            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // One review per customer per product only
            builder.Entity<Review>()
                .HasIndex(r => new { r.CustomerId, r.ProductId })
                .IsUnique();

            // WishlistItem → Customer (Many-to-One)
            builder.Entity<WishlistItem>()
                .HasOne(w => w.Customer)
                .WithMany(c => c.WishlistItems)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // One wishlist item per customer per product only
            builder.Entity<WishlistItem>()
                .HasIndex(w => new { w.CustomerId, w.ProductId })
                .IsUnique();

            // OrderItem → Vendor (Many-to-One)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Vendor)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem → Product (Many-to-One)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → Customer (Many-to-One)
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique promotion code 
            builder.Entity<Promotion>()
                .HasIndex(p => p.Code)
                .IsUnique();
        }
    }
}
