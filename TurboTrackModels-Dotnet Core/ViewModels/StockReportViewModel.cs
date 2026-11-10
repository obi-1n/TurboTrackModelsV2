namespace TurboTrackModels_Dotnet_Core.ViewModels;

public class StockReportViewModel
{
    public List<StockMovement> Movements { get; set; } = [];
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class StockMovement
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int InitialStock { get; set; }
    public int QuantitySold { get; set; }
    public int CurrentStock { get; set; }
    public int NetChange => -QuantitySold;
}
