using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Sales Manager")]
public class SalesManagerController(TTMDbContext db) : Controller
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public IActionResult Dashboard() => View();

    public async Task<IActionResult> TurnoverReport(DateTime? startDate, DateTime? endDate)
    {
        var (start, end) = GetDateRange(startDate, endDate);
        var orders = await db.Orders
            .Where(order => order.OrderDate >= start && order.OrderDate < end)
            .ToListAsync();
        var daily = orders.GroupBy(order => order.OrderDate.Date)
            .Select(group => new DailyTurnover
            {
                Date = group.Key,
                Amount = group.Sum(order => order.TotalCost),
                OrderCount = group.Count()
            })
            .OrderBy(item => item.Date)
            .ToList();
        return View(new TurnoverReportViewModel
        {
            StartDate = start,
            EndDate = end.AddTicks(-1),
            TotalTurnover = daily.Sum(item => item.Amount),
            DailyTurnovers = daily
        });
    }

    public async Task<IActionResult> DownloadTurnoverReportExcel(DateTime? startDate, DateTime? endDate)
    {
        var (start, end) = GetDateRange(startDate, endDate);
        var orders = await db.Orders
            .Where(order => order.OrderDate >= start && order.OrderDate < end)
            .ToListAsync();
        var rows = orders.GroupBy(order => order.OrderDate.Date)
            .OrderBy(group => group.Key)
            .Select(group => new[]
            {
                group.Key.ToString("yyyy-MM-dd"),
                group.Count().ToString(),
                group.Sum(order => order.TotalCost).ToString("F2")
            });
        return ExcelFile("Turnover", ["Date", "Orders", "Turnover"], rows,
            $"TurnoverReport_{start:yyyyMMdd}_{end.AddTicks(-1):yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> DeliveryReport(DateTime? reportDate)
    {
        var date = (reportDate ?? DateTime.Today).Date;
        var deliveries = await db.Orders
            .Include(order => order.Customer)
                .ThenInclude(customer => customer.User)
            .Where(order => order.OrderDate >= date &&
                            order.OrderDate < date.AddDays(1) &&
                            order.Status == OrderStatus.Delivered)
            .ToListAsync();
        return View(new DeliveryReportViewModel
        {
            ReportDate = date,
            OutgoingDeliveries = deliveries
        });
    }

    public async Task<IActionResult> DownloadDeliveryReportExcel(DateTime? reportDate)
    {
        var date = (reportDate ?? DateTime.Today).Date;
        var deliveries = await db.Orders
            .Include(order => order.Customer)
                .ThenInclude(customer => customer.User)
            .Where(order => order.OrderDate >= date &&
                            order.OrderDate < date.AddDays(1) &&
                            order.Status == OrderStatus.Delivered)
            .OrderBy(order => order.OrderId)
            .ToListAsync();
        var rows = deliveries.Select(order => new[]
        {
            order.OrderId.ToString(),
            order.OrderDate.ToString("yyyy-MM-dd HH:mm"),
            $"{order.Customer.User.FirstName} {order.Customer.User.LastName}",
            order.TotalCost.ToString("F2")
        });
        return ExcelFile("Deliveries",
            ["Order ID", "Order Date", "Customer", "Total Cost"],
            rows, $"DeliveryReport_{date:yyyyMMdd}.xlsx");
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
        return View(new StockBySupplierViewModel
        {
            Groups = GroupProducts(products),
            SelectedSupplier = supplierId,
            GeneratedAt = DateTime.Now
        });
    }

    public async Task<IActionResult> DownloadStockBySupplierReportExcel(int? supplierId)
    {
        var products = await GetFilteredProductsAsync(supplierId);
        var rows = products.Select(product => new[]
        {
            product.ProductId,
            product.ProductName,
            product.Price.ToString("F2"),
            product.StockLevel.ToString()
        });
        return ExcelFile("Stock",
            ["Product ID", "Product Name", "Price", "Stock Level"],
            rows, $"StockBySupplierReport_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> InvoicesReport(DateTime? filterDate, string? filterUsername)
    {
        var orders = await GetFilteredOrdersAsync(filterDate, filterUsername);
        return View(new InvoicesViewModel
        {
            Orders = orders,
            GeneratedAt = DateTime.Now,
            FilterDate = filterDate,
            FilterUsername = filterUsername
        });
    }

    public async Task<IActionResult> DownloadInvoicesReportPdf(DateTime? filterDate, string? filterUsername)
    {
        var orders = await GetFilteredOrdersAsync(filterDate, filterUsername);
        var lines = new List<string> { "INVOICES REPORT", $"Generated: {DateTime.Now:g}", "" };
        lines.AddRange(orders.Select(order =>
            $"Order {order.OrderId} | {order.OrderDate:yyyy-MM-dd} | {order.Customer.User.UserName} | GBP {order.TotalCost:F2}"));
        return File(TextPdf.Create(lines), "application/pdf", "InvoicesReport.pdf");
    }

    public async Task<IActionResult> ProductSalesReport()
    {
        return View(new ProductSalesReportViewModel
        {
            ProductSales = await GetProductSalesAsync(),
            GeneratedAt = DateTime.Now
        });
    }

    public async Task<IActionResult> DownloadProductSalesReportPdf()
    {
        var sales = await GetProductSalesAsync();
        var lines = new List<string> { "PRODUCT SALES REPORT", $"Generated: {DateTime.Now:g}", "" };
        lines.AddRange(sales.Select(product =>
            $"{product.ProductId} | {product.ProductName} | Qty {product.TotalQuantitySold} | GBP {product.TotalRevenue:F2} | Stock {product.CurrentStockLevel}"));
        return File(TextPdf.Create(lines), "application/pdf", "ProductSalesReport.pdf");
    }

    private async Task<List<Order>> GetFilteredOrdersAsync(DateTime? filterDate, string? filterUsername)
    {
        var query = db.Orders
            .Include(order => order.Customer)
                .ThenInclude(customer => customer.User)
            .AsQueryable();
        if (filterDate.HasValue)
        {
            var start = filterDate.Value.Date;
            query = query.Where(order => order.OrderDate >= start && order.OrderDate < start.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(filterUsername))
        {
            query = query.Where(order => order.Customer.User.UserName != null &&
                                         order.Customer.User.UserName.Contains(filterUsername));
        }

        return await query.OrderByDescending(order => order.OrderDate).ToListAsync();
    }

    private async Task<List<Product>> GetFilteredProductsAsync(int? supplierId)
    {
        var products = await db.Products.OrderBy(product => product.ProductId).ToListAsync();
        return supplierId switch
        {
            1 => products.Where(product => EndsWithEvenDigit(product.ProductId)).ToList(),
            2 => products.Where(product => EndsWithOddDigit(product.ProductId)).ToList(),
            _ => products
        };
    }

    private async Task<List<ProductSalesSummary>> GetProductSalesAsync()
    {
        var products = await db.Products.ToListAsync();
        var items = await db.OrderItems
            .Include(item => item.Product)
            .ToListAsync();
        return products.Select(product =>
        {
            var productItems = items.Where(item => item.ProductId == product.ProductId).ToList();
            return new ProductSalesSummary
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CurrentStockLevel = product.StockLevel,
                TotalQuantitySold = productItems.Sum(item => item.Quantity),
                TotalRevenue = productItems.Sum(item => item.Quantity * item.Product.Price)
            };
        }).OrderByDescending(item => item.TotalRevenue).ToList();
    }

    private static List<ProductGroup> GroupProducts(IEnumerable<Product> products) =>
        products.GroupBy(product => product.ProductId.Length >= 2
                ? product.ProductId[..2]
                : product.ProductId)
            .Select(group => new ProductGroup
            {
                GroupName = $"Category {group.Key}",
                Products = group.ToList()
            })
            .ToList();

    private static bool EndsWithEvenDigit(string value) =>
        value.Length > 0 && char.IsDigit(value[^1]) && (value[^1] - '0') % 2 == 0;

    private static bool EndsWithOddDigit(string value) =>
        value.Length > 0 && char.IsDigit(value[^1]) && (value[^1] - '0') % 2 != 0;

    private static (DateTime Start, DateTime EndExclusive) GetDateRange(
        DateTime? startDate,
        DateTime? endDate)
    {
        var start = (startDate ?? DateTime.Today.AddDays(-7)).Date;
        var inclusiveEnd = (endDate ?? DateTime.Today).Date;
        if (inclusiveEnd < start)
        {
            throw new ArgumentException("End date must be on or after start date.", nameof(endDate));
        }

        return (start, inclusiveEnd.AddDays(1));
    }

    private IActionResult ExcelFile(
        string sheetName,
        string[] headers,
        IEnumerable<string[]> rows,
        string fileName)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);
        for (var column = 0; column < headers.Length; column++)
        {
            sheet.Cell(1, column + 1).Value = headers[column];
            sheet.Cell(1, column + 1).Style.Font.Bold = true;
        }

        var rowNumber = 2;
        foreach (var values in rows)
        {
            for (var column = 0; column < values.Length; column++)
            {
                sheet.Cell(rowNumber, column + 1).Value = values[column];
            }

            rowNumber++;
        }

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), ExcelContentType, fileName);
    }
}
