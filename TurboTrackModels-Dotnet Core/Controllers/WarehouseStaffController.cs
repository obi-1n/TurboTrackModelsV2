using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Warehouse Assistant")]
public class WarehouseStaffController(TTMDbContext db) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var orders = await db.Orders
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .Include(order => order.Customer)
                .ThenInclude(customer => customer.User)
            .Where(order => order.Status == OrderStatus.Pending)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsShipped(int orderId)
    {
        var order = await db.Orders.FindAsync(orderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Shipped;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
