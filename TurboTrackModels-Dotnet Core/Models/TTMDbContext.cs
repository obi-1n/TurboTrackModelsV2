using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

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

        // DbSets
        public DbSet<CustomerProfile> Customers { get; set; }
        public DbSet<StaffProfile> Staff { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fluent API configurations go here if required
        }
    }
}