using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Services;

public class RoomService : IRoomService
{
    private readonly HotelDbContext _context;
    private readonly HotelMemoryCache _cache;

    public RoomService(HotelDbContext context, HotelMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private async Task EnsureCacheLoadedAsync()
    {
        if (_cache.Rooms.Count == 0)
        {
            if (await _context.Rooms.AnyAsync())
            {
                _cache.Rooms = await _context.Rooms.ToListAsync();
            }
        }
    }

    public async Task AddRoomAsync(Room room)
    {
        if (!await IsRoomNumberUniqueAsync(room.RoomNumber))
        {
            throw new DuplicateRoomException($"Room number {room.RoomNumber} already exists.");
        }
        _context.Add(room);
        await _context.SaveChangesAsync();
        _cache.AddRoom(room);
    }

    public async Task UpdateRoomAsync(Room room)
    {
        if (!await IsRoomNumberUniqueAsync(room.RoomNumber, room.RoomId))
        {
            throw new DuplicateRoomException($"Room number {room.RoomNumber} already exists.");
        }
        _context.Update(room);
        await _context.SaveChangesAsync();
        _cache.UpdateRoom(room);
    }

    public async Task DeleteRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            _cache.DeleteRoom(id);
        }
    }

    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Rooms;
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        await EnsureCacheLoadedAsync();
        if (_cache.RoomsDict.ContainsKey(id))
            return _cache.RoomsDict[id];
            
        return await _context.Rooms.FindAsync(id);
    }

    public async Task<IEnumerable<Room>> SearchRoomByNumberAsync(string roomNumber)
    {
        await EnsureCacheLoadedAsync();
        return _cache.Rooms.Where(r => r.RoomNumber.Contains(roomNumber));
    }

    public async Task<IEnumerable<Room>> FilterRoomsByTypeAsync(string roomType)
    {
        await EnsureCacheLoadedAsync();
        return _cache.Rooms.Where(r => r.RoomType == roomType);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Rooms.Where(r => r.IsAvailable);
    }

    public async Task<IEnumerable<Room>> SortRoomsByPriceAscendingAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Rooms.OrderBy(r => r.PricePerNight);
    }

    public async Task<IEnumerable<Room>> SortRoomsByPriceDescendingAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Rooms.OrderByDescending(r => r.PricePerNight);
    }

    public async Task<bool> IsRoomNumberUniqueAsync(string roomNumber, int? excludeRoomId = null)
    {
        if (excludeRoomId.HasValue)
        {
            return !await _context.Rooms.AnyAsync(r => r.RoomNumber == roomNumber && r.RoomId != excludeRoomId.Value);
        }
        return !await _context.Rooms.AnyAsync(r => r.RoomNumber == roomNumber);
    }

    public async Task<bool> HasActiveBookingsAsync(int roomId)
    {
        // Check for current or future bookings
        return await _context.Bookings.AnyAsync(b => b.RoomId == roomId && b.CheckOutDate >= DateTime.Today);
    }
}
