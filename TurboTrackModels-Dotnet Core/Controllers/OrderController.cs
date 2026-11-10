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
    StripePaymentService stripePaymentService,
    ILogger<OrderController> logger,
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
            status.GetString() != "succeeded" ||
            !paymentResponse.RootElement.TryGetProperty("id", out var chargeIdElement) ||
            string.IsNullOrWhiteSpace(chargeIdElement.GetString()))
        {
            ModelState.AddModelError(string.Empty,
                "The payment response could not be verified. Contact support before trying again.");
            return CheckoutView(cart);
        }

        var chargeId = chargeIdElement.GetString()!;
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var staff = currentUserId is null
            ? null
            : await db.Staff.FirstOrDefaultAsync(profile => profile.UserId == currentUserId);
        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            TotalCost = totalAmount,
            ShippingCost = ShippingCost,
            StripeChargeId = chargeId,
            Status = OrderStatus.Pending,
            CustomerID = customer.CustomerID,
            StaffID = staff?.StaffId,
            OrderItems = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = decimal.Round(item.Product.Price, 2, MidpointRounding.AwayFromZero)
            }).ToList()
        };

        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);
        var currentProducts = new Dictionary<string, Product>();
        var stockChanged = false;
        var productUnavailable = false;
        foreach (var item in cart.Items)
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product is null)
            {
                productUnavailable = true;
                stockChanged = true;
                continue;
            }

            currentProducts.Add(item.ProductId, product);
            stockChanged |= product.StockLevel < item.Quantity;
        }

        if (stockChanged)
        {
            order.Status = OrderStatus.RefundPending;
            if (productUnavailable)
            {
                order.OrderItems = [];
            }

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync();
                logger.LogError(exception,
                    "Could not record order after successful Stripe charge {ChargeId}.", chargeId);
                var failedSaveRefund = await stripePaymentService.RefundAsync(
                    chargeId, $"refund-charge-{chargeId}");
                ModelState.AddModelError(string.Empty, failedSaveRefund.Succeeded
                    ? "Stock changed and the order could not be recorded, so your payment has been refunded."
                    : $"Stock changed after payment and the order could not be recorded. Contact support with payment reference {chargeId}. Refund error: {failedSaveRefund.ErrorMessage}");
                return CheckoutView(cart);
            }

            var refund = await stripePaymentService.RefundAsync(
                chargeId, $"refund-order-{order.OrderId}");
            if (refund.Succeeded)
            {
                order.Status = OrderStatus.Cancelled;
                order.StripeRefundId = refund.RefundId;
                await db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty,
                    "Stock changed while payment was processing. Your payment has been refunded.");
            }
            else
            {
                logger.LogError(
                    "Automatic refund failed for order {OrderId}, Stripe charge {ChargeId}: {Error}",
                    order.OrderId, chargeId, refund.ErrorMessage);
                ModelState.AddModelError(string.Empty,
                    $"Stock changed after payment. The order is recorded as awaiting a refund. Contact support with order #{order.OrderId} and payment reference {chargeId}. Refund error: {refund.ErrorMessage}");
            }

            return CheckoutView(cart);
        }

        foreach (var item in cart.Items)
        {
            currentProducts[item.ProductId].StockLevel -= item.Quantity;
        }

        order.TotalCost = order.OrderItems.Sum(item => item.UnitPrice * item.Quantity) + order.ShippingCost;
        db.Orders.Add(order);
        try
        {
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync();
            logger.LogError(exception,
                "Could not save order after successful Stripe charge {ChargeId}.", chargeId);
            var refund = await stripePaymentService.RefundAsync(
                chargeId, $"refund-order-save-{chargeId}");
            ModelState.AddModelError(string.Empty, refund.Succeeded
                ? "The order could not be saved, so your payment has been refunded."
                : $"The payment succeeded but the order could not be saved and an automatic refund failed. Contact support with payment reference {chargeId}. Refund error: {refund.ErrorMessage}");
            return CheckoutView(cart);
        }

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
            if (product is null)
            {
                result.Add(new Product
                {
                    ProductId = item.ProductId,
                    ProductName = "Unavailable product",
                    StockLevel = 0
                });
                continue;
            }

            if (product.StockLevel < item.Quantity)
            {
                result.Add(product);
            }

            item.Product = product;
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
