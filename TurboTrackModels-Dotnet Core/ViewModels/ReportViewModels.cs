using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.ViewModels;

public class DailyTurnover
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}

public class TurnoverReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalTurnover { get; set; }
    public List<DailyTurnover> DailyTurnovers { get; set; } = [];
}

public class DeliveryReportViewModel
{
    public DateTime ReportDate { get; set; }
    public List<Order> OutgoingDeliveries { get; set; } = [];
}

public class SalesStockReportViewModel
{
    public List<Product> Products { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public class ProductGroup
{
    public string GroupName { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = [];
}

public class StockBySupplierViewModel
{
    public List<ProductGroup> Groups { get; set; } = [];
    public int? SelectedSupplier { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class InvoicesViewModel
{
    public List<Order> Orders { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
    public DateTime? FilterDate { get; set; }
    public string? FilterUsername { get; set; }
    public decimal TotalAmount => Orders.Sum(order => order.TotalCost);
    public int TotalInvoices => Orders.Count;
}

public class ProductSalesSummary
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStockLevel { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ProductSalesReportViewModel
{
    public List<ProductSalesSummary> ProductSales { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}
