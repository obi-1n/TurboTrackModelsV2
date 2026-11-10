using System.Collections.Generic;
using System.Linq;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents a shopping cart containing items a user intends to purchase.
    /// Provides functionality to calculate total cost.
    /// </summary>
    public class ShoppingCart
    {
        //List of items in the shopping cart
        public List<CartItem> Items { get; set; } = new();
        //Calculate the total cost of the items in the cart
        public decimal TotalCost =>
            Items.Sum(i => i.Product.Price * i.Quantity);
    }

    /// <summary>
    /// Represents an individual item within the shopping cart.
    /// Includes reference to the product and the desired quantity.
    /// </summary>
    public class CartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}