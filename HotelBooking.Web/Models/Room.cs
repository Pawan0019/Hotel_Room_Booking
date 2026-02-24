using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.Web.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Room Number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Room Type")]
    public string RoomType { get; set; } = string.Empty; // Single, Double, Suite

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000)]
    [Display(Name = "Price Per Night")]
    public decimal PricePerNight { get; set; }

    [Display(Name = "Is Available")]
    public bool IsAvailable { get; set; } = true;

    // Navigation property
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
