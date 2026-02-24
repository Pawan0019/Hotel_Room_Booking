using HotelBooking.Web.Models;

namespace HotelBooking.Web.Services;

public interface IBookingService
{
    Task BookRoomAsync(Booking booking);
    Task CancelBookingAsync(int bookingId);
    decimal CalculateTotalAmount(decimal pricePerNight, DateTime checkIn, DateTime checkOut);
    Task MarkRoomUnavailableAsync(int roomId);
    Task MakeRoomAvailableAsync(int roomId);
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<IEnumerable<IGrouping<int, Booking>>> GroupBookingsByGuestAsync();
    Task<string> GetMostBookedRoomTypeAsync();
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
    Task CancelBookingsByGuestAsync(int guestId);
}
