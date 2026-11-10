using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents the Entity Framework database context for the Turbo Track Models Application.
    /// </summary>
    /// <remarks>
    /// Inherits from <see cref="Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext{TUser}"/>,
    /// where TUser is of type <see cref="User"/>. This context is responsible for managing
    /// database operations and providing DbSet properties for the application's data models.
    /// </remarks>
    /// <example>
    /// Use <c>TTMDbContext.Create()</c> to create a new instance of the database context.
    /// </example>
    public class TTMDbContext : IdentityDbContext<User>
    {
        //DbSet for each of the models in the application
        public DbSet<CustomerProfile> Customers { get; set; }
        public DbSet<StaffProfile> Staff { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public TTMDbContext()
            : base("TTMConnection", throwIfV1Schema: false)
        {
            //Set the database initializer to the custom one we created
            Database.SetInitializer<TTMDbContext>(new DatabaseInitialiser());
        }

        public static TTMDbContext Create()
        {
            return new TTMDbContext();
        }
    }
}