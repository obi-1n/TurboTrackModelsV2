using Microsoft.AspNetCore.Mvc.Rendering;

namespace TurboTrackModels_Dotnet_Core.ViewModels;

public class FactorViewModel
{
    public string Purpose { get; set; } = string.Empty;
}

public class ConfigureTwoFactorViewModel
{
    public string SelectedProvider { get; set; } = string.Empty;
    public ICollection<SelectListItem> Providers { get; set; } = [];
}