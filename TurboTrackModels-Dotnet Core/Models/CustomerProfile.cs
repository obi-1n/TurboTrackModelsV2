using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Represents a customer profile entity in the system. This stores customer-specific
    /// details such as address, phone number, and associated user account.
    /// </summary>
    /// <remarks>
    /// This entity links to other entities in the system, such as user accounts and orders.
    /// It is used to store and manage customer-related data, including the address and
    /// contact information.
    /// </remarks>
    public class CustomerProfile
    {
        //Primary key (auto-generated)
        //CustomerID is not used for login, it is only used to identify the customer in the database and link to orders
        [Key]
        public int CustomerID { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        //Navigational properties
        //A customer can have many orders, but an order can only have one customer
        public List<Order> Orders { get; set; } = new();

        //Foreign key for ApplicationUser
        //Links the CustomerProfile to the user account
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

    }
}