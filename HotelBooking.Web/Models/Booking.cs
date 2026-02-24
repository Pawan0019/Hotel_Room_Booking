using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.Web.Models;

public class Booking
{
    [Key]
    public int BookingId { get; set; }

    [ForeignKey("Room")]
    public int RoomId { get; set; }

    [ForeignKey("Guest")]
    public int GuestId { get; set; }

    [Required]
    [Display(Name = "Check-In Date")]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }

    [Required]
    [Display(Name = "Check-Out Date")]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000)]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    public bool IsCancelled { get; set; }

    // Navigation properties
    public virtual Room? Room { get; set; }
    public virtual Guest? Guest { get; set; }
}
