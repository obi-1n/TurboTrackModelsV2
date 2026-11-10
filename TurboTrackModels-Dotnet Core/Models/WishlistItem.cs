namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents an item in a user's wishlist.
    /// Contains information about the associated product and the date it was added to the wishlist.
    /// </summary>
    public class WishlistItem
    {
        public string UserId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;

        public DateTime DateAdded { get; set; }
    }
}
