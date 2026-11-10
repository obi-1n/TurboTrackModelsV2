using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents an application user in the system, leveraging ASP.NET Identity functionality.
    /// Inherits properties and methods provided by the IdentityUser base class.
    /// </summary>
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        //// needing the user manager to get the role of the user
        //private ApplicationUserManager userManager;

        //// The current property is not mapped to the database 
        //// property to get the current role of the user logged in
        //[NotMapped]
        //public string CurrentRole
        //{
        //    get
        //    {
        //        if (userManager == null)
        //        {
        //            userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //        }

        //        return userManager.GetRoles(this.Id).Single();
        //    }
        //}
    }
}