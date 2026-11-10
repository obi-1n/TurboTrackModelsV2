using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Customer")]
public class WishlistController(TTMDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var wishlist = await db.WishlistItems
            .Include(item => item.Product)
            .Where(item => item.UserId == userId)
            .ToListAsync();
        return View(wishlist);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(productId))
        {
            return BadRequest();
        }

        if (!await db.WishlistItems.AnyAsync(item =>
                item.UserId == userId && item.ProductId == productId) &&
            await db.Products.AnyAsync(product => product.ProductId == productId))
        {
            db.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                DateAdded = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(productId))
        {
            return BadRequest();
        }

        var item = await db.WishlistItems.FindAsync(userId, productId);
        if (item is not null)
        {
            db.WishlistItems.Remove(item);
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
