using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructures.Models;

namespace OrderManagementApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .HasOne<Order>()
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<Order>()
                .HasOne<User>()
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Description = "Gaming Laptop", Price = 12000, StockQuantity = 10 },
                new Product { Id = 2, Name = "Akıllı Telefon", Description = "Son model akıllı telefon", Price = 8000, StockQuantity = 20 },
                new Product { Id = 3, Name = "Tablet", Description = "10 inç tablet", Price = 5000, StockQuantity = 15 }
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "user1", Email = "user1@example.com" },
                new User { Id = 2, Username = "user2", Email = "user2@example.com" }
            );
        }
    }
}