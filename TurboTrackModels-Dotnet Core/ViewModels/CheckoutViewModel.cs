using System.ComponentModel.DataAnnotations;

namespace TurboTrackModels_Dotnet_Core.ViewModels;

public class CheckoutViewModel
{
    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string Town { get; set; } = string.Empty;

    [Required]
    public string Postcode { get; set; } = string.Empty;
}