using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turbo_Track_Models_Application.Models
{
    /// <summary>
    /// Represents an item in a user's wishlist.
    /// Contains information about the associated product and the date it was added to the wishlist.
    /// </summary>
    public class WishlistItem
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }

        [Key, Column(Order = 1)]
        public string ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public DateTime DateAdded { get; set; }
    }
}
