using System.Configuration;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Helper class to retrieve Stripe configuration settings from Web.config
    /// </summary>
    public static class StripeSettings
    {
        // Secret Key used for server-side API calls
        public static string SecretKey => ConfigurationManager.AppSettings["StripeSecretKey"];

        // Publishable Key used for client-side Stripe Elements
        public static string PublishableKey => ConfigurationManager.AppSettings["StripePublishableKey"];
    }
}
