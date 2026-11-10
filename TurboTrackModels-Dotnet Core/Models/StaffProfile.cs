using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents the profile information of a staff member.
    /// </summary>
    /// <remarks>
    /// This class is used to associate staff members with their user accounts
    /// and is a part of the data model in the Turbo Track application.
    /// </remarks>
    public class StaffProfile
    {
        //Primary key (auto-generated)
        //StaffID is not used for login, it is only used to identify the staff member in the database and link to orders
        [Key]
        public int StaffId { get; set; }

        //Navigational properties
        //Links the StaffProfile to the user account
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

    }
}