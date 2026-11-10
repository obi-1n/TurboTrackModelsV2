using Microsoft.AspNetCore.Identity;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents an application user in the system, leveraging ASP.NET Identity functionality.
    /// Inherits properties and methods provided by the IdentityUser base class.
    /// </summary>
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}