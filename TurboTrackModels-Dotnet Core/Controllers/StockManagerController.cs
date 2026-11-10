using ClosedXML.Excel;
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

    public async Task<IActionResult> DownloadStockReportPdf()
    {
        var products = await db.Products.OrderBy(product => product.ProductId).ToListAsync();
        var lines = new List<string> { "STOCK REPORT", $"Generated: {DateTime.Now:g}", "" };
        lines.AddRange(products.Select(product =>
            $"{product.ProductId} | {product.ProductName} | Stock {product.StockLevel}"));
        return File(TextPdf.Create(lines), "application/pdf", "StockReport.pdf");
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
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Stock by Supplier");
        var headers = new[] { "Product ID", "Product Name", "Price", "Stock Level" };
        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
            sheet.Cell(1, index + 1).Style.Font.Bold = true;
        }

        for (var index = 0; index < products.Count; index++)
        {
            var row = index + 2;
            sheet.Cell(row, 1).Value = products[index].ProductId;
            sheet.Cell(row, 2).Value = products[index].ProductName;
            sheet.Cell(row, 3).Value = products[index].Price;
            sheet.Cell(row, 4).Value = products[index].StockLevel;
        }

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
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
