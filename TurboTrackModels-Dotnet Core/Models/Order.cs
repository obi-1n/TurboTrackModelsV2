using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents a customer order in the system.
    /// Tracks order date, total cost, status, and associated customer/staff.
    /// </summary>
    public class Order
    {
        //Primary key
        // Primary key (auto-generated)
        [Key]
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal ShippingCost { get; set; }
        public OrderStatus Status { get; set; }
        public string? ReturnReason { get; set; }

        //Navigational properties
        //An order can have many products, and a product can be in many orders
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }
        public CustomerProfile Customer { get; set; } = null!;

        [ForeignKey("Staff")]
        //StaffID is nullable beacuase staff may not be involved if the order is taken online
        public int? StaffID { get; set; }
        public StaffProfile? Staff { get; set; }

        //An order will consist of one or more items
        public List<OrderItem> OrderItems { get; set; } = new();
    }

    /// <summary>
    /// Represents the various statuses an order can have.
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Cancelled,
        ReturnRequested,
        Returned
    }
}