using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Assistant Manager,Stock Control Manager")]
public class AssistantManagerController(
    TTMDbContext db,
    StripePaymentService stripePaymentService,
    IWebHostEnvironment environment) : Controller
{
    public IActionResult Dashboard() => View();

    public async Task<IActionResult> ManageReturns()
    {
        var orders = await db.Orders
            .Include(order => order.Customer)
                .ThenInclude(customer => customer.User)
            .Where(order => order.Status == OrderStatus.ReturnRequested ||
                            order.Status == OrderStatus.RefundPending)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryRefund(int orderId)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(order =>
            order.OrderId == orderId && order.Status == OrderStatus.RefundPending);
        if (order is null)
        {
            TempData["Error"] = $"Order #{orderId} is not awaiting a refund.";
            return RedirectToAction(nameof(ManageReturns));
        }

        if (string.IsNullOrWhiteSpace(order.StripeChargeId))
        {
            TempData["Error"] = $"Order #{orderId} has no recorded Stripe charge and cannot be refunded.";
            return RedirectToAction(nameof(ManageReturns));
        }

        var refund = order.StripeRefundId is null
            ? await stripePaymentService.RefundAsync(
                order.StripeChargeId, $"refund-order-{order.OrderId}")
            : new StripeRefundResult(true, order.StripeRefundId, null);
        if (!refund.Succeeded)
        {
            TempData["Error"] = $"Refund for order #{orderId} failed: {refund.ErrorMessage}";
            return RedirectToAction(nameof(ManageReturns));
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);
        var currentOrder = await db.Orders.FirstOrDefaultAsync(order =>
            order.OrderId == orderId && order.Status == OrderStatus.RefundPending);
        if (currentOrder is not null)
        {
            currentOrder.Status = OrderStatus.Cancelled;
            currentOrder.StripeRefundId = refund.RefundId;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = $"Refund for order #{orderId} completed.";
        }
        else
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"Order #{orderId} is no longer awaiting a refund.";
        }

        return RedirectToAction(nameof(ManageReturns));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsReturned(int orderId)
    {
        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.OrderId == orderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status == OrderStatus.ReturnRequested)
        {
            if (string.IsNullOrWhiteSpace(order.StripeChargeId))
            {
                TempData["Error"] =
                    $"Order #{orderId} has no recorded payment reference and cannot be refunded. Contact support.";
                return RedirectToAction(nameof(ManageReturns));
            }

            var refund = order.StripeRefundId is null
                ? await stripePaymentService.RefundAsync(
                    order.StripeChargeId, $"refund-order-{order.OrderId}")
                : new StripeRefundResult(true, order.StripeRefundId, null);
            if (!refund.Succeeded)
            {
                TempData["Error"] =
                    $"Order #{orderId} could not be completed because the payment refund failed: {refund.ErrorMessage}";
                return RedirectToAction(nameof(ManageReturns));
            }

            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            var currentOrder = await db.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(order => order.OrderId == orderId);
            if (currentOrder?.Status == OrderStatus.ReturnRequested)
            {
                currentOrder.Status = OrderStatus.Returned;
                currentOrder.StripeRefundId = refund.RefundId;
                foreach (var item in currentOrder.OrderItems)
                {
                    item.Product.StockLevel += item.Quantity;
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] =
                    $"Order #{orderId} was marked returned, its payment refunded, and items restored to stock.";
            }
            else
            {
                await transaction.RollbackAsync();
                TempData["Error"] =
                    $"Order #{orderId} is no longer awaiting a return. The refund was already requested.";
            }
        }

        return RedirectToAction(nameof(ManageReturns));
    }

    public async Task<IActionResult> ProductList() =>
        View(await db.Products.OrderBy(product => product.ProductName).ToListAsync());

    public IActionResult AddProduct() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(Product product, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        if (await db.Products.AnyAsync(existing => existing.ProductId == product.ProductId))
        {
            ModelState.AddModelError(nameof(product.ProductId), "Product ID already exists.");
            return View(product);
        }

        product.ImageUrl = await SaveImageAsync(imageFile);
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        db.Products.Add(product);
        await db.SaveChangesAsync();
        return RedirectAfterProductChange();
    }

    public async Task<IActionResult> EditProduct(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(string id, Product product, IFormFile? imageFile)
    {
        if (id != product.ProductId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var existing = await db.Products.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.ProductName = product.ProductName;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockLevel = product.StockLevel;
        if (imageFile is { Length: > 0 })
        {
            existing.ImageUrl = await SaveImageAsync(imageFile);
            if (!ModelState.IsValid)
            {
                return View(product);
            }
        }

        await db.SaveChangesAsync();
        return RedirectAfterProductChange();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        db.Products.Remove(product);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Products referenced by orders cannot be deleted.");
            return View("ProductList", await db.Products.OrderBy(item => item.ProductName).ToListAsync());
        }

        return RedirectAfterProductChange();
    }

    private IActionResult RedirectAfterProductChange() =>
        User.IsInRole("Stock Control Manager")
            ? RedirectToAction("Dashboard", "StockManager")
            : RedirectToAction(nameof(ProductList));

    private async Task<string?> SaveImageAsync(IFormFile? imageFile)
    {
        if (imageFile is null || imageFile.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            ModelState.AddModelError(nameof(imageFile), "Upload a JPG, PNG, or WebP image.");
            return null;
        }

        const long maxImageSize = 5 * 1024 * 1024;
        if (imageFile.Length > maxImageSize)
        {
            ModelState.AddModelError(nameof(imageFile), "Image files must be 5 MB or smaller.");
            return null;
        }

        var folder = Path.Combine(environment.WebRootPath ?? "wwwroot", "Scalextric Car Images");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
        await imageFile.CopyToAsync(stream);
        return $"/Scalextric Car Images/{fileName}";
    }
}
