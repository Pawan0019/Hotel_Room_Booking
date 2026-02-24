using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class Guest
{
    [Key]
    public int GuestId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    // Navigation property
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
