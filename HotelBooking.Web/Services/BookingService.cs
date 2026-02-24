using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Services;

public class BookingService : IBookingService
{
    private readonly HotelDbContext _context;
    private readonly HotelMemoryCache _cache;

    public BookingService(HotelDbContext context, HotelMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private async Task EnsureCacheLoadedAsync()
    {
        if (_cache.Bookings.Count == 0)
        {
            if (await _context.Bookings.AnyAsync())
            {
                _cache.Bookings = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Guest)
                    .ToListAsync();
            }
        }
    }

    public async Task BookRoomAsync(Booking booking)
    {
        // Create execution strategy to work with EnableRetryOnFailure
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Use a Serializable transaction to prevent race conditions (Double Booking)
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // 1. Date Validation
                if (booking.CheckInDate < DateTime.Today)
                {
                    throw new InvalidBookingDateException("Check-in date cannot be in the past.");
                }

                if (booking.CheckInDate >= booking.CheckOutDate)
                {
                    throw new InvalidBookingDateException("Check-out date must be after check-in date.");
                }

                // 2. Validate Guest
                var guest = await _context.Guests.FindAsync(booking.GuestId);
                if (guest == null)
                {
                    throw new InvalidOperationException("Guest not found.");
                }
                if (!guest.IsActive)
                {
                    throw new GuestInactiveException("Guest is not active and cannot book a room.");
                }

                // 3. Validate Room
                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room == null)
                {
                    throw new InvalidOperationException("Room not found.");
                }
                if (!room.IsAvailable)
                {
                    throw new RoomUnavailableException("Room is not available (Marked Unavailable).");
                }

                // Critical Check within Transaction
                if (!await IsRoomAvailableAsync(booking.RoomId, booking.CheckInDate, booking.CheckOutDate))
                {
                    throw new BookingConflictException("Room is already booked for the selected dates.");
                }

                // 4. Calculate/Validate Amount
                // Always recalculate to ensure integrity
                booking.TotalAmount = CalculateTotalAmount(room.PricePerNight, booking.CheckInDate, booking.CheckOutDate);

                if (booking.TotalAmount <= 0)
                {
                    throw new InvalidOperationException("Total amount must be positive.");
                }

                // 5. Update State & Save
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Commit Transaction
                await transaction.CommitAsync();

                // Update Cache (After Commit)
                _cache.AddBooking(booking);
            }
            catch (Exception)
            {
                // Rollback is automatic on dispose, but explicit rollback can be good for clarity
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking != null && !booking.IsCancelled)
        {
            // We don't need to touch Room.IsAvailable
            booking.IsCancelled = true;
            await _context.SaveChangesAsync();
            _cache.UpdateBooking(booking);
        }
    }

    public decimal CalculateTotalAmount(decimal pricePerNight, DateTime checkIn, DateTime checkOut)
    {
        var days = (checkOut - checkIn).Days;
        if (days <= 0) return 0;
        return pricePerNight * days;
    }

    public async Task MarkRoomUnavailableAsync(int roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room != null)
        {
            room.IsAvailable = false;
            _context.Update(room);
            await _context.SaveChangesAsync();
            _cache.UpdateRoom(room);
        }
    }

    public async Task MakeRoomAvailableAsync(int roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room != null)
        {
            room.IsAvailable = true;
            _context.Update(room);
            await _context.SaveChangesAsync();
            _cache.UpdateRoom(room);
        }
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Bookings;
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        await EnsureCacheLoadedAsync();
        var cached = _cache.Bookings.FirstOrDefault(b => b.BookingId == id);
        if (cached != null) return cached;

        return await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.BookingId == id);
    }

    public async Task<IEnumerable<IGrouping<int, Booking>>> GroupBookingsByGuestAsync()
    {
        await EnsureCacheLoadedAsync();
        // Use cache for LINQ performance
        return await Task.FromResult(_cache.Bookings.GroupBy(b => b.GuestId));
    }

    public async Task<string> GetMostBookedRoomTypeAsync()
    {
        await EnsureCacheLoadedAsync();
        // Use cache for LINQ performance
        var mostBooked = _cache.Bookings
            .Where(b => b.Room != null)
            .GroupBy(b => b.Room!.RoomType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return await Task.FromResult(mostBooked ?? "No Bookings Found");
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        // Check for overlapping bookings, ignoring cancelled ones
        return !await _context.Bookings.AnyAsync(b => 
            b.RoomId == roomId && 
            !b.IsCancelled &&
            (checkIn < b.CheckOutDate && checkOut > b.CheckInDate));
    }

    public async Task CancelBookingsByGuestAsync(int guestId)
    {
        var activeBookings = await _context.Bookings
            .Where(b => b.GuestId == guestId && b.CheckOutDate >= DateTime.Today && !b.IsCancelled)
            .ToListAsync();

        foreach (var booking in activeBookings)
        {
            booking.IsCancelled = true;
            
            // Cache update: Remove from cache as it's modified significantly or just update
            // For simplicity and consistency with current cache design which stores entities:
            _cache.UpdateBooking(booking); 
        }
        
        await _context.SaveChangesAsync();
    }
}
