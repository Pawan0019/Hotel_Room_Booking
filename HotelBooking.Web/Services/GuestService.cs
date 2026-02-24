using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Services;

public class GuestService : IGuestService
{
    private readonly HotelDbContext _context;
    private readonly HotelMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;

    public GuestService(HotelDbContext context, HotelMemoryCache cache, IServiceProvider serviceProvider)
    {
        _context = context;
        _cache = cache;
        _serviceProvider = serviceProvider;
    }

    private async Task EnsureCacheLoadedAsync()
    {
        if (_cache.Guests.Count == 0)
        {
            if (await _context.Guests.AnyAsync())
            {
                _cache.Guests = await _context.Guests.ToListAsync();
            }
        }
    }

    public async Task AddGuestAsync(Guest guest)
    {
        if (!await IsEmailUniqueAsync(guest.Email))
        {
            throw new DuplicateGuestException($"Guest with email {guest.Email} already exists.");
        }
        if (!await IsPhoneUniqueAsync(guest.Phone))
        {
            throw new DuplicateGuestException($"Guest with phone {guest.Phone} already exists.");
        }
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
        _cache.AddGuest(guest);
    }

    public async Task UpdateGuestAsync(Guest guest)
    {
        if (!await IsEmailUniqueAsync(guest.Email, guest.GuestId))
        {
            throw new DuplicateGuestException($"Guest with email {guest.Email} already exists.");
        }
        if (!await IsPhoneUniqueAsync(guest.Phone, guest.GuestId))
        {
            throw new DuplicateGuestException($"Guest with phone {guest.Phone} already exists.");
        }

        var existingGuest = await _context.Guests.FindAsync(guest.GuestId);
        if (existingGuest != null)
        {
            // Check for deactivation
            if (existingGuest.IsActive && !guest.IsActive)
            {
                // Cascade cancel bookings using IBookingService
                using (var scope = _serviceProvider.CreateScope())
                {
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.CancelBookingsByGuestAsync(guest.GuestId);
                }
            }

            _context.Entry(existingGuest).CurrentValues.SetValues(guest);
            await _context.SaveChangesAsync();
            _cache.UpdateGuest(guest);
        }
    }

    public async Task DeleteGuestAsync(int id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest != null)
        {
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            _cache.DeleteGuest(id);
        }
    }

    public async Task<IEnumerable<Guest>> GetAllGuestsAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Guests;
    }

    public async Task<IEnumerable<Guest>> GetActiveGuestsAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Guests.Where(g => g.IsActive);
    }

    public async Task<Guest?> GetGuestByIdAsync(int id)
    {
        await EnsureCacheLoadedAsync();
        // Cache lookup from list
        var cached = _cache.Guests.FirstOrDefault(g => g.GuestId == id);
        if (cached != null) return cached;
        return await _context.Guests.FindAsync(id);
    }

    public async Task ActivateGuestAsync(int id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest != null)
        {
            guest.IsActive = true;
            _context.Update(guest);
            await _context.SaveChangesAsync();
            _cache.UpdateGuest(guest);
        }
    }

    public async Task DeactivateGuestAsync(int id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest != null)
        {
            guest.IsActive = false;
            _context.Update(guest);
            
            // Cascade cancel bookings using IBookingService
            using (var scope = _serviceProvider.CreateScope())
            {
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                await bookingService.CancelBookingsByGuestAsync(id);
            }

            await _context.SaveChangesAsync();
            _cache.UpdateGuest(guest);
        }
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeGuestId = null)
    {
        if (excludeGuestId.HasValue)
        {
            return !await _context.Guests.AnyAsync(g => g.Email == email && g.GuestId != excludeGuestId.Value);
        }
        return !await _context.Guests.AnyAsync(g => g.Email == email);
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludeGuestId = null)
    {
        if (excludeGuestId.HasValue)
        {
            return !await _context.Guests.AnyAsync(g => g.Phone == phone && g.GuestId != excludeGuestId.Value);
        }
        return !await _context.Guests.AnyAsync(g => g.Phone == phone);
    }

    public async Task<bool> HasActiveBookingsAsync(int guestId)
    {
        return await _context.Bookings.AnyAsync(b => b.GuestId == guestId && b.CheckOutDate >= DateTime.Today);
    }
}
