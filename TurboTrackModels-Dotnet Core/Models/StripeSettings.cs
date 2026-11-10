namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Stripe configuration values bound from application configuration.
    /// </summary>
    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
    }
}
