using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents a product in the system, including details about its identity, attributes, and stock levels.
    /// </summary>
    public class Product
    {
        //Primary key
        //ProductID is a string because it is used to identify the product in the database and link to orders
        [Key]
        [Required]
        public string ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        //Stock level is an integer between 0 and 100
        [Range(0, 100)]
        public int StockLevel { get; set; }

        // Image path or filename
        public string ImageUrl { get; set; }


        //Navigational properties
        //A product can be in many orders, and an order can have many products
        public List<Order> Orders { get; set; }


    }
}