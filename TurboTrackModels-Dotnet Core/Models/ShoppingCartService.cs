using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Manages a shopping cart stored in ASP.NET Core session state.
    /// </summary>
    public class ShoppingCartService
    {
        private const string CartKey = "Cart";
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShoppingCartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ShoppingCart GetCart()
        {
            var serializedCart = GetSession().GetString(CartKey);
            if (serializedCart is null)
            {
                return new ShoppingCart();
            }

            return JsonSerializer.Deserialize<ShoppingCart>(serializedCart, SerializerOptions)
                ?? throw new InvalidOperationException("The shopping cart in session is invalid.");
        }

        public void AddToCart(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (item is null)
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
                item.Product = product;
                item.Quantity++;
            }

            SaveCart(cart);
        }

        public void RemoveFromCart(string productId)
        {
            ArgumentNullException.ThrowIfNull(productId);

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is not null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void UpdateQuantity(string productId, int change)
        {
            ArgumentNullException.ThrowIfNull(productId);

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is null)
            {
                return;
            }

            item.Quantity += change;
            if (item.Quantity <= 0)
            {
                cart.Items.Remove(item);
            }

            SaveCart(cart);
        }

        public void Clear()
        {
            GetSession().Remove(CartKey);
        }

        private ISession GetSession()
        {
            return _httpContextAccessor.HttpContext?.Session
                ?? throw new InvalidOperationException("An active HTTP session is required.");
        }

        private void SaveCart(ShoppingCart cart)
        {
            GetSession().SetString(CartKey, JsonSerializer.Serialize(cart, SerializerOptions));
        }
    }
}