using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents an individual item within an order.
    /// This class links a product to an order and manages the quantity of the product within that order.
    /// </summary>
    public class OrderItem
    {
        //Composite primary key of OrderId
        //Column order is used to specify the order of the columns in the database
        [Key, Column(Order = 0), ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        //Composite primary key of ProductId
        [Key, Column(Order = 1), ForeignKey("Product")]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        //Order total cost
        public int Quantity { get; set; }
    }
}