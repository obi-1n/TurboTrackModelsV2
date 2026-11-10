using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Invoice Clerk")]
public class InvoiceClerkController(TTMDbContext db) : Controller
{
    public async Task<IActionResult> Dashboard(DateTime? filterDate, string? filterUsername)
    {
        var query = db.Orders
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .Include(order => order.Customer)
                .ThenInclude(customer => customer.User)
            .AsQueryable();

        if (filterDate.HasValue)
        {
            var start = filterDate.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(order => order.OrderDate >= start && order.OrderDate < end);
        }

        if (!string.IsNullOrWhiteSpace(filterUsername))
        {
            query = query.Where(order => order.Customer.User.UserName != null &&
                                         order.Customer.User.UserName.Contains(filterUsername));
        }

        ViewBag.FilterDate = filterDate?.ToString("yyyy-MM-dd");
        ViewBag.FilterUsername = filterUsername;
        return View(await query.OrderByDescending(order => order.OrderId).ToListAsync());
    }

    public async Task<IActionResult> DownloadInvoice(int orderId)
    {
        var order = await db.Orders
            .Include(item => item.Customer)
                .ThenInclude(customer => customer.User)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.OrderId == orderId);
        if (order is null)
        {
            return NotFound("Order not found.");
        }

        return File(InvoicePdf.Create(order), "application/pdf", $"Invoice_{order.OrderId}.pdf");
    }
}
