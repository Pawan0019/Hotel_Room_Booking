using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBooking.Web.ViewModels;

public class BookingCreateViewModel
{
    [Required]
    [Display(Name = "Room")]
    public int? RoomId { get; set; }

    [Required]
    [Display(Name = "Guest")]
    public int? GuestId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DateInFuture]
    [Display(Name = "Check-In Date")]
    public DateTime? CheckInDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DateGreaterThan("CheckInDate")]
    [Display(Name = "Check-Out Date")]
    public DateTime? CheckOutDate { get; set; }

    public SelectList? RoomDropdownList { get; set; }
    public SelectList? GuestDropdownList { get; set; }
}

public class BookingListViewModel
{
    public int BookingId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }
    
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BookingCancelViewModel
{
    public int BookingId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }
    
    public decimal TotalAmount { get; set; }
}

public class BookingStatsViewModel
{
    public string GuestName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public IEnumerable<BookingListViewModel> Bookings { get; set; } = new List<BookingListViewModel>();
}
