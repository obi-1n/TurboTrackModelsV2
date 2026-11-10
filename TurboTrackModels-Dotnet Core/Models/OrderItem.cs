namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents an individual item within an order.
    /// This class links a product to an order and manages the quantity of the product within that order.
    /// </summary>
    public class OrderItem
    {
        // EF Core composite keys are configured in TTMDbContext.
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}