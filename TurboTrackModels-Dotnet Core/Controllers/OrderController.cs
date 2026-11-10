using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Customer,Sales Assistant,Assistant Manager")]
public class OrderController(
    TTMDbContext db,
    ShoppingCartService shoppingCart,
    IHttpClientFactory httpClientFactory,
    IOptions<StripeSettings> stripeOptions) : Controller
{
    private const decimal ShippingCost = 4m;
    private StripeSettings StripeSettings => stripeOptions.Value;

    public async Task<IActionResult> SelectCustomer()
    {
        if (!IsStaffOrder())
        {
            return RedirectToAction(nameof(Address));
        }

        return View(await db.Customers
            .Include(customer => customer.User)
            .OrderBy(customer => customer.User.LastName)
            .ThenBy(customer => customer.User.FirstName)
            .ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectCustomer(int customerId)
    {
        if (!IsStaffOrder())
        {
            return RedirectToAction(nameof(Address));
        }

        if (!await db.Customers.AnyAsync(customer => customer.CustomerID == customerId))
        {
            ModelState.AddModelError(string.Empty, "Customer not found.");
            return View(await db.Customers.Include(customer => customer.User).ToListAsync());
        }

        HttpContext.Session.SetInt32("SelectedCustomerId", customerId);
        return RedirectToAction(nameof(Address));
    }

    [HttpGet]
    public async Task<IActionResult> Address()
    {
        if (!shoppingCart.GetCart().Items.Any())
        {
            return RedirectToAction("ShoppingCart", "ShoppingCart");
        }

        var customer = await GetOrderCustomerAsync();
        if (customer is null && IsStaffOrder())
        {
            return RedirectToAction(nameof(SelectCustomer));
        }

        return View(customer is null
            ? new AddressViewModel()
            : new AddressViewModel
            {
                Street = customer.Street,
                Town = customer.Town,
                PostCode = customer.PostCode,
                PhoneNumber = customer.PhoneNumber
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Address(AddressViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await GetOrderCustomerAsync();
        if (customer is null)
        {
            if (IsStaffOrder())
            {
                return RedirectToAction(nameof(SelectCustomer));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Challenge();
            }

            customer = new CustomerProfile { UserId = userId };
            db.Customers.Add(customer);
        }

        customer.Street = model.Street;
        customer.Town = model.Town;
        customer.PostCode = model.PostCode;
        customer.PhoneNumber = model.PhoneNumber;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Checkout));
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = shoppingCart.GetCart();
        if (cart.Items.Count == 0)
        {
            return RedirectToAction("ShoppingCart", "ShoppingCart");
        }

        var outOfStock = await GetOutOfStockProductsAsync(cart);
        if (outOfStock.Count > 0)
        {
            return View("OutOfStock", outOfStock);
        }

        var customer = await GetOrderCustomerAsync();
        if (customer is null || string.IsNullOrWhiteSpace(customer.Street))
        {
            return RedirectToAction(IsStaffOrder() ? nameof(SelectCustomer) : nameof(Address));
        }

        ViewBag.PublishableKey = StripeSettings.PublishableKey;
        ViewBag.ShippingCost = ShippingCost;
        ViewBag.TotalAmount = cart.TotalCost + ShippingCost;
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(string? stripeToken)
    {
        var cart = shoppingCart.GetCart();
        if (cart.Items.Count == 0)
        {
            return RedirectToAction("ShoppingCart", "ShoppingCart");
        }

        var outOfStock = await GetOutOfStockProductsAsync(cart);
        if (outOfStock.Count > 0)
        {
            return View("OutOfStock", outOfStock);
        }

        var customer = await GetOrderCustomerAsync();
        if (customer is null || string.IsNullOrWhiteSpace(customer.Street))
        {
            return RedirectToAction(IsStaffOrder() ? nameof(SelectCustomer) : nameof(Address));
        }

        if (string.IsNullOrWhiteSpace(stripeToken) || string.IsNullOrWhiteSpace(StripeSettings.SecretKey))
        {
            ModelState.AddModelError(string.Empty, "Payment could not be started. Check the payment details and try again.");
            return CheckoutView(cart);
        }

        var totalAmount = cart.TotalCost + ShippingCost;
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/charges");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", StripeSettings.SecretKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = decimal.ToInt64(decimal.Round(totalAmount * 100, 0)).ToString(),
            ["currency"] = "gbp",
            ["description"] = "Turbo Track Models Order",
            ["source"] = stripeToken
        });

        using var response = await httpClientFactory.CreateClient().SendAsync(request);
        using var paymentResponse = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        if (!response.IsSuccessStatusCode)
        {
            var message = paymentResponse.RootElement.TryGetProperty("error", out var error) &&
                          error.TryGetProperty("message", out var errorMessage)
                ? errorMessage.GetString()
                : null;
            ModelState.AddModelError(string.Empty, message ?? "The payment provider declined the payment.");
            return CheckoutView(cart);
        }

        if (!paymentResponse.RootElement.TryGetProperty("status", out var status) ||
            status.GetString() != "succeeded")
        {
            ModelState.AddModelError(string.Empty, "The payment was not completed.");
            return CheckoutView(cart);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var staff = currentUserId is null
            ? null
            : await db.Staff.FirstOrDefaultAsync(profile => profile.UserId == currentUserId);
        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            TotalCost = totalAmount,
            Status = OrderStatus.Pending,
            CustomerID = customer.CustomerID,
            StaffID = staff?.StaffId,
            OrderItems = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList()
        };

        foreach (var item in cart.Items)
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product is null || product.StockLevel < item.Quantity)
            {
                ModelState.AddModelError(string.Empty, "Stock changed while payment was processing. Contact support with your payment reference.");
                return CheckoutView(cart);
            }

            product.StockLevel -= item.Quantity;
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        shoppingCart.Clear();
        HttpContext.Session.Remove("SelectedCustomerId");
        return RedirectToAction(nameof(Success), new { id = order.OrderId });
    }

    public async Task<IActionResult> Success(int id)
    {
        var order = await db.Orders
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.OrderId == id);
        return order is null ? NotFound() : View(order);
    }

    private bool IsStaffOrder() =>
        User.IsInRole("Sales Assistant") || User.IsInRole("Assistant Manager");

    private async Task<CustomerProfile?> GetOrderCustomerAsync()
    {
        if (IsStaffOrder())
        {
            var selectedId = HttpContext.Session.GetInt32("SelectedCustomerId");
            return selectedId.HasValue
                ? await db.Customers.FindAsync(selectedId.Value)
                : null;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? null
            : await db.Customers.FirstOrDefaultAsync(customer => customer.UserId == userId);
    }

    private async Task<List<Product>> GetOutOfStockProductsAsync(ShoppingCart cart)
    {
        var result = new List<Product>();
        foreach (var item in cart.Items)
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product is null || product.StockLevel < item.Quantity)
            {
                result.Add(product ?? new Product
                {
                    ProductId = item.ProductId,
                    ProductName = "Unavailable product",
                    StockLevel = 0
                });
            }
        }

        return result;
    }

    private IActionResult CheckoutView(ShoppingCart cart)
    {
        ViewBag.PublishableKey = StripeSettings.PublishableKey;
        ViewBag.ShippingCost = ShippingCost;
        ViewBag.TotalAmount = cart.TotalCost + ShippingCost;
        return View(nameof(Checkout), cart);
    }
}
