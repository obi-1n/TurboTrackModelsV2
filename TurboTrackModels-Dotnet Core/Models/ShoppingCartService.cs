using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Provides functionality to manage a shopping cart within a user session, including
    /// tasks such as adding, removing, and updating products in the cart, as well as clearing the cart.
    /// </summary>
    public class ShoppingCartService
    {
        // Service to manage the shopping cart using session state
        private readonly HttpSessionStateBase _session;
        // Key to store the shopping cart in session
        private const string CartKey = "Cart";

        // Constructor to initialize the shopping cart service with the session
        public ShoppingCartService(HttpSessionStateBase session)
        {
            // Store the session for later use
            _session = session;
        }

        // Method to retrieve the shopping cart from session, or create a new one if it doesn't exist
        public ShoppingCart GetCart()
        {
            // Check if the cart exists in session, if not create a new one
            if (_session[CartKey] == null)
                _session[CartKey] = new ShoppingCart();

            return (ShoppingCart)_session[CartKey];
        }

        // Method to add a product to the shopping cart
        public void AddToCart(Product product)
        {
            // Retrieve the current cart from session
            var cart = GetCart();

            // Check if the product is already in the cart
            var item = cart.Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }
        }

        // Method to remove a product from the shopping cart by product ID
        public void RemoveFromCart(string productId)
        {
            // Retrieve the current cart from session
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            // If the item is found, remove it from the cart
            if (item != null)
                cart.Items.Remove(item);
        }

        // Method to update product quantity
        public void UpdateQuantity(string productId, int change)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity += change;
                if (item.Quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
            }
        }

        // Method to clear the shopping cart from session
        public void Clear()
        {
            _session.Remove(CartKey);
        }
    }
}