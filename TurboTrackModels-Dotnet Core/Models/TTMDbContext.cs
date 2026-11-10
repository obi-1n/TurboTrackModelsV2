using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the
    /// Turbo Track Models application.
    /// </summary>
    public class TTMDbContext : IdentityDbContext<User>
    {
        public TTMDbContext(DbContextOptions<TTMDbContext> options)
            : base(options)
        {
        }

        public DbSet<CustomerProfile> Customers => Set<CustomerProfile>();
        public DbSet<StaffProfile> Staff => Set<StaffProfile>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OrderItem>()
                .HasKey(item => new { item.OrderId, item.ProductId });

            builder.Entity<WishlistItem>()
                .HasKey(item => new { item.UserId, item.ProductId });

            builder.Entity<WishlistItem>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerProfile>()
                .HasOne(profile => profile.User)
                .WithOne()
                .HasForeignKey<CustomerProfile>(profile => profile.UserId);

            builder.Entity<StaffProfile>()
                .HasOne(profile => profile.User)
                .WithOne()
                .HasForeignKey<StaffProfile>(profile => profile.UserId);
        }
    }
}