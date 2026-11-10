using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController(TTMDbContext db) : Controller
{
    public async Task<IActionResult> RequestReturns()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var threshold = DateTime.UtcNow.AddDays(-28);
        var orders = await db.Orders
            .Where(order => order.CustomerID == customer.CustomerID &&
                            order.OrderDate >= threshold &&
                            order.Status == OrderStatus.Delivered)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> SubmitReturnRequest(int orderId)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var order = await db.Orders
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.OrderId == orderId &&
                                         item.CustomerID == customer.CustomerID);
        if (order is null)
        {
            return NotFound("Order not found.");
        }

        if (order.OrderDate < DateTime.UtcNow.AddDays(-28) ||
            order.Status != OrderStatus.Delivered)
        {
            return BadRequest("This order is not eligible for a return.");
        }

        return View(new ReturnRequestViewModel
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            TotalCost = order.TotalCost,
            OrderItems = order.OrderItems
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReturnRequest(ReturnRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var order = await db.Orders.FirstOrDefaultAsync(item =>
            item.OrderId == model.OrderId && item.CustomerID == customer.CustomerID);
        if (order is null)
        {
            return NotFound("Order not found.");
        }

        if (order.Status != OrderStatus.Delivered ||
            order.OrderDate < DateTime.UtcNow.AddDays(-28))
        {
            return BadRequest("This order is not eligible for a return.");
        }

        order.ReturnReason = model.ReturnReason;
        order.Status = OrderStatus.ReturnRequested;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(OrderHistory));
    }

    public async Task<IActionResult> OrderHistory()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var orders = await db.Orders
            .Where(order => order.CustomerID == customer.CustomerID)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var order = await db.Orders.FirstOrDefaultAsync(item =>
            item.OrderId == orderId && item.CustomerID == customer.CustomerID);
        if (order is null)
        {
            return NotFound("Order not found.");
        }

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Cancelled;
            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Order #{orderId} has been cancelled successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Order #{orderId} cannot be cancelled as it is already being processed.";
        }

        return RedirectToAction(nameof(OrderHistory));
    }

    public async Task<IActionResult> DownloadInvoice(int orderId)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var order = await db.Orders
            .Include(item => item.Customer)
                .ThenInclude(profile => profile.User)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.OrderId == orderId &&
                                         item.CustomerID == customer.CustomerID);
        if (order is null)
        {
            return NotFound("Order not found.");
        }

        var lines = new List<string>
        {
            "TURBO TRACK MODELS - INVOICE",
            $"Order ID: {order.OrderId}",
            $"Date: {order.OrderDate:dd MMMM yyyy}",
            $"Customer: {order.Customer.User.FirstName} {order.Customer.User.LastName}",
            $"Email: {order.Customer.User.Email}",
            $"Address: {order.Customer.Street}, {order.Customer.Town}, {order.Customer.PostCode}",
            "",
            "Items:"
        };
        lines.AddRange(order.OrderItems.Select(item =>
            $"{item.Product.ProductName} | Qty {item.Quantity} | GBP {item.Product.Price:F2} | GBP {item.Quantity * item.Product.Price:F2}"));
        lines.Add($"Order total: GBP {order.TotalCost:F2}");

        return File(TextPdf.Create(lines), "application/pdf", $"Invoice_{order.OrderId}.pdf");
    }

    private Task<CustomerProfile?> GetCurrentCustomerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return db.Customers.FirstOrDefaultAsync(customer => customer.UserId == userId);
    }
}
