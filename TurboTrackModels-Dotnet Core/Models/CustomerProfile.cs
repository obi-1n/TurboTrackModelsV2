using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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
        public string Street { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
        public string PhoneNumber { get; set; }

        //Navigational properties
        //A customer can have many orders, but an order can only have one customer
        public List<Order> Orders { get; set; }

        //Foreign key for ApplicationUser
        //Links the CustomerProfile to the user account
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

    }
}