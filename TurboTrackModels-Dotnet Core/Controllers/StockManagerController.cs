using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Stock Control Manager,Sales Manager")]
public class StockManagerController(TTMDbContext db) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = userId is null ? null : await db.Users.FindAsync(userId);
        ViewBag.FirstName = user?.FirstName;
        return View();
    }

    public async Task<IActionResult> StockReport()
    {
        return View(new SalesStockReportViewModel
        {
            Products = await db.Products.OrderBy(product => product.ProductName).ToListAsync(),
            GeneratedAt = DateTime.Now
        });
    }

    public async Task<IActionResult> DownloadStockReportExcel()
    {
        var products = await db.Products.OrderBy(product => product.ProductId).ToListAsync();
        var rows = products.Select(product => new object?[]
        {
            product.ProductId,
            product.ProductName,
            product.Price,
            product.StockLevel
        });
        return File(
            ReportExcel.Create("Stock", ["Product ID", "Product Name", "Price", "Stock Level"], rows),
            ReportExcel.ContentType,
            "StockReport.xlsx");
    }

    public async Task<IActionResult> StockBySupplierReport(int? supplierId)
    {
        var products = await GetFilteredProductsAsync(supplierId);
        var groups = products.GroupBy(product => product.ProductId.Length >= 2
                ? product.ProductId[..2]
                : product.ProductId)
            .Select(group => new ProductGroup
            {
                GroupName = $"Category {group.Key}",
                Products = group.ToList()
            })
            .ToList();
        return View(new StockBySupplierViewModel
        {
            Groups = groups,
            SelectedSupplier = supplierId,
            GeneratedAt = DateTime.Now
        });
    }

    public async Task<IActionResult> DownloadStockBySupplierReportExcel(int? supplierId)
    {
        var products = await GetFilteredProductsAsync(supplierId);
        var rows = products.Select(product => new object?[]
        {
            product.ProductId,
            product.ProductName,
            product.Price,
            product.StockLevel
        });
        return File(ReportExcel.Create("Stock by Supplier",
                ["Product ID", "Product Name", "Price", "Stock Level"], rows),
            ReportExcel.ContentType,
            $"StockBySupplierReport_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private async Task<List<Product>> GetFilteredProductsAsync(int? supplierId)
    {
        var products = await db.Products.OrderBy(product => product.ProductId).ToListAsync();
        return supplierId switch
        {
            1 => products.Where(product => HasDigitParity(product.ProductId, 0)).ToList(),
            2 => products.Where(product => HasDigitParity(product.ProductId, 1)).ToList(),
            _ => products
        };
    }

    private static bool HasDigitParity(string productId, int parity) =>
        productId.Length > 0 &&
        char.IsDigit(productId[^1]) &&
        (productId[^1] - '0') % 2 == parity;
}
