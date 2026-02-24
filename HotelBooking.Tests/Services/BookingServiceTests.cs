using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using HotelBooking.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace HotelBooking.Tests.Services;

[TestFixture]
public class BookingServiceTests
{
    private HotelDbContext _context;
    private HotelMemoryCache _cache;
    private BookingService _service;
    private RoomService _roomService;
    private GuestService _guestService;
    private IServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new HotelDbContext(options);
        _cache = new HotelMemoryCache();
        
        // Create a service provider for dependency injection
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddSingleton(_cache);
        _serviceProvider = services.BuildServiceProvider();
        
        // We need Room/Guest services to populate data correctly or just add to DB directly
        // BookingService uses _context and _cache.
        _service = new BookingService(_context, _cache);
        _roomService = new RoomService(_context, _cache);
        _guestService = new GuestService(_context, _cache, _serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }


    [Test]
    public async Task AddRoom_ShouldAddRoom()
    {
        // Arrange
        var room = new Room 
        { 
            RoomNumber = "101", 
            IsAvailable = true, 
            RoomType = "Single", 
            PricePerNight = 100 
        };

        // Act
        await _roomService.AddRoomAsync(room);

        // Assert
        var addedRoom = await _roomService.GetRoomByIdAsync(room.RoomId);
        Assert.That(addedRoom, Is.Not.Null);
        Assert.That(addedRoom!.RoomNumber, Is.EqualTo("101"));
        Assert.That(addedRoom.RoomType, Is.EqualTo("Single"));
        Assert.That(addedRoom.PricePerNight, Is.EqualTo(100));
        Assert.That(addedRoom.IsAvailable, Is.True);
    }

    [Test]
    public async Task BookRoom_ShouldMarkRoomUnavailable()
    {
        // Arrange
        var room = new Room { RoomNumber = "101", IsAvailable = true, RoomType = "Single", PricePerNight = 100 };
        var guest = new Guest { Name = "John Doe", IsActive = true, Email = "john@example.com", Phone = "1234567890" };
        
        await _roomService.AddRoomAsync(room);
        await _guestService.AddGuestAsync(guest);

        var booking = new Booking
        {
            RoomId = room.RoomId,
            GuestId = guest.GuestId,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1)
        };

        // Act
        await _service.BookRoomAsync(booking);

        // Assert
        var createdBooking = await _service.GetBookingByIdAsync(booking.BookingId);
        Assert.That(createdBooking, Is.Not.Null);
        Assert.That(createdBooking!.RoomId, Is.EqualTo(room.RoomId));
        Assert.That(createdBooking.GuestId, Is.EqualTo(guest.GuestId));
        Assert.That(createdBooking.IsCancelled, Is.False);
    }

    [Test]
    public async Task CancelBooking_ShouldMakeRoomAvailable()
    {
        // Arrange
        var room = new Room { RoomNumber = "101", IsAvailable = true, RoomType = "Single", PricePerNight = 100 };
        var guest = new Guest { Name = "John Doe", IsActive = true, Email = "john@example.com", Phone = "1234567890" };
        
        await _roomService.AddRoomAsync(room);
        await _guestService.AddGuestAsync(guest);

        var booking = new Booking
        {
            RoomId = room.RoomId,
            GuestId = guest.GuestId,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1)
        };

        await _service.BookRoomAsync(booking);
        var createdBooking = await _service.GetBookingByIdAsync(booking.BookingId);
        Assert.That(createdBooking!.IsCancelled, Is.False);

        // Act
        await _service.CancelBookingAsync(booking.BookingId);

        // Assert
        var cancelledBooking = await _service.GetBookingByIdAsync(booking.BookingId);
        Assert.That(cancelledBooking, Is.Not.Null);
        Assert.That(cancelledBooking!.IsCancelled, Is.True);
    }


    [Test]
    public async Task BookRoom_WhenRoomUnavailable_ShouldFail()
    {
        // Arrange
        var room = new Room { RoomNumber = "101", IsAvailable = false, RoomType = "S", PricePerNight = 100 };
        var guest = new Guest { Name = "John", IsActive = true, Email = "j@d.com", Phone = "123" };
        
        await _roomService.AddRoomAsync(room);
        await _guestService.AddGuestAsync(guest);

        var booking = new Booking
        {
            RoomId = room.RoomId,
            GuestId = guest.GuestId,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1)
        };

        // Act & Assert
        Assert.ThrowsAsync<RoomUnavailableException>(() => _service.BookRoomAsync(booking));
    }


    [Test]
    public void CalculateTotalAmount_ShouldReturnCorrectValue()
    {
        // Arrange
        var room = new Room { PricePerNight = 150 };
        var booking = new Booking
        {
            Room = room,
            CheckInDate = new DateTime(2025, 1, 1),
            CheckOutDate = new DateTime(2025, 1, 4) // 3 nights
        };

        // Act
        var totalAmount = booking.CheckOutDate.Subtract(booking.CheckInDate).Days * room.PricePerNight;

        // Assert
        Assert.That(totalAmount, Is.EqualTo(450));
    }
}
