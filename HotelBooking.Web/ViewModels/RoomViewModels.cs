using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.ViewModels;

public class RoomCreateViewModel
{
    [Required]
    [Display(Name = "Room Number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Room Type")]
    [RegularExpression("^(Single|Double|Suite|Deluxe)$", ErrorMessage = "Room Type must be Single, Double, Suite, or Deluxe.")]
    public string RoomType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0.")]
    [Display(Name = "Price Per Night")]
    public decimal PricePerNight { get; set; }

    [Display(Name = "Is Available")]
    public bool IsAvailable { get; set; } = true;
}

public class RoomEditViewModel
{
    public int RoomId { get; set; }

    [Required]
    [Display(Name = "Room Number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Room Type")]
    [RegularExpression("^(Single|Double|Suite|Deluxe)$", ErrorMessage = "Room Type must be Single, Double, Suite, or Deluxe.")]
    public string RoomType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0.")]
    [Display(Name = "Price Per Night")]
    public decimal PricePerNight { get; set; }

    [Display(Name = "Is Available")]
    public bool IsAvailable { get; set; }
}

public class RoomListViewModel
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
}

public class RoomSearchViewModel
{
    public string? RoomType { get; set; }
    public decimal? MaxPrice { get; set; }
}

public class RoomFilterViewModel
{
    public string? RoomType { get; set; }
    public bool? IsAvailable { get; set; }
}
