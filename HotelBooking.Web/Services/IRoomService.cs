using HotelBooking.Web.Models;

namespace HotelBooking.Web.Services;

public interface IRoomService
{
    Task AddRoomAsync(Room room);
    Task UpdateRoomAsync(Room room);
    Task DeleteRoomAsync(int id);
    Task<IEnumerable<Room>> GetAllRoomsAsync();
    Task<Room?> GetRoomByIdAsync(int id);
    Task<IEnumerable<Room>> SearchRoomByNumberAsync(string roomNumber);
    Task<IEnumerable<Room>> FilterRoomsByTypeAsync(string roomType);
    Task<IEnumerable<Room>> GetAvailableRoomsAsync();
    Task<IEnumerable<Room>> SortRoomsByPriceAscendingAsync();
    Task<IEnumerable<Room>> SortRoomsByPriceDescendingAsync();
    Task<bool> IsRoomNumberUniqueAsync(string roomNumber, int? excludeRoomId = null);
    Task<bool> HasActiveBookingsAsync(int roomId);
}
