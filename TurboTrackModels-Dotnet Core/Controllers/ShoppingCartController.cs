using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

public class ShoppingCartController(
    TTMDbContext db,
    ShoppingCartService shoppingCart) : Controller
{
    public async Task<IActionResult> ShoppingCart()
    {
        var cart = shoppingCart.GetCart();
        foreach (var item in cart.Items)
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product is not null)
            {
                item.Product = product;
            }
        }

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        shoppingCart.AddToCart(product);
        return RedirectToAction(nameof(ShoppingCart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(string id)
    {
        shoppingCart.RemoveFromCart(id);
        return RedirectToAction(nameof(ShoppingCart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(string id, int change)
    {
        shoppingCart.UpdateQuantity(id, change);
        return RedirectToAction(nameof(ShoppingCart));
    }
}
