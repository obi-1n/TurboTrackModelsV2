using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController(TTMDbContext db, StripePaymentService stripePaymentService) : Controller
{
    public async Task<IActionResult> RequestReturns()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null)
        {
            return NotFound("Customer profile not found.");
        }

        var orders = await db.Orders
            .Where(order => order.CustomerID == customer.CustomerID &&
                            order.Status == OrderStatus.Pending)
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

        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(item =>
                item.OrderId == orderId && item.CustomerID == customer.CustomerID);
        if (order is null)
        {
            return NotFound("Order not found.");
        }

        if (order.Status == OrderStatus.Pending)
        {
            if (string.IsNullOrWhiteSpace(order.StripeChargeId))
            {
                TempData["ErrorMessage"] =
                    $"Order #{orderId} has no recorded payment reference and could not be refunded. Contact support.";
                return RedirectToAction(nameof(OrderHistory));
            }

            var refund = order.StripeRefundId is null
                ? await stripePaymentService.RefundAsync(
                    order.StripeChargeId, $"refund-order-{order.OrderId}")
                : new StripeRefundResult(true, order.StripeRefundId, null);
            if (!refund.Succeeded)
            {
                TempData["ErrorMessage"] =
                    $"Order #{orderId} could not be cancelled because the payment refund failed: {refund.ErrorMessage}";
                return RedirectToAction(nameof(OrderHistory));
            }

            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            var currentOrder = await db.Orders
                .Include(item => item.OrderItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(item =>
                    item.OrderId == orderId && item.CustomerID == customer.CustomerID);
            if (currentOrder?.Status == OrderStatus.Pending)
            {
                foreach (var item in currentOrder.OrderItems)
                {
                    item.Product.StockLevel += item.Quantity;
                }

                currentOrder.Status = OrderStatus.Cancelled;
                currentOrder.StripeRefundId = refund.RefundId;
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["SuccessMessage"] =
                    $"Order #{orderId} has been cancelled and its payment refunded.";
            }
            else
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] =
                    $"Order #{orderId} is no longer pending. The refund was already requested; contact support if you still need help.";
            }

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

        return File(InvoicePdf.Create(order), "application/pdf", $"Invoice_{order.OrderId}.pdf");
    }

    private Task<CustomerProfile?> GetCurrentCustomerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return db.Customers.FirstOrDefaultAsync(customer => customer.UserId == userId);
    }
}
