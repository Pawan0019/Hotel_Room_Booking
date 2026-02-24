using HotelBooking.Web.Models;

namespace HotelBooking.Web.Services;

public interface IGuestService
{
    Task AddGuestAsync(Guest guest);
    Task UpdateGuestAsync(Guest guest);
    Task DeleteGuestAsync(int id);
    Task<IEnumerable<Guest>> GetAllGuestsAsync();
    Task<IEnumerable<Guest>> GetActiveGuestsAsync();
    Task<Guest?> GetGuestByIdAsync(int id);
    Task ActivateGuestAsync(int id);
    Task DeactivateGuestAsync(int id);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeGuestId = null);
    Task<bool> IsPhoneUniqueAsync(string phone, int? excludeGuestId = null);
    Task<bool> HasActiveBookingsAsync(int guestId);
}
