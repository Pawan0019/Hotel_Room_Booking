using HotelBooking.Web.Models;
using HotelBooking.Web.ViewModels;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace HotelBooking.Web.Controllers;

public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;

    public BookingsController(IBookingService bookingService, IRoomService roomService, IGuestService guestService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _guestService = guestService;
    }

    // GET: Bookings
    public async Task<IActionResult> Index()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        var model = bookings.Select(b => new BookingListViewModel
        {
            BookingId = b.BookingId,
            RoomNumber = b.Room?.RoomNumber ?? "N/A",
            GuestName = b.Guest?.Name ?? "N/A",
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            TotalAmount = b.TotalAmount,
            IsCancelled = b.IsCancelled,
            Status = b.IsCancelled ? "Cancelled" : (b.CheckOutDate < DateTime.Today ? "Completed" : "Active")
        });
        return View(model);
    }

    // GET: Bookings/Stats (For LINQ reqs)
    public async Task<IActionResult> Stats()
    {
        var grouped = await _bookingService.GroupBookingsByGuestAsync();
        
        var model = grouped.Select(g => new BookingStatsViewModel
        {
            GuestName = g.First().Guest?.Name ?? "Unknown",
            BookingCount = g.Count(),
            Bookings = g.Select(b => new BookingListViewModel
            {
                BookingId = b.BookingId,
                RoomNumber = b.Room?.RoomNumber ?? "N/A",
                GuestName = b.Guest?.Name ?? "N/A",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalAmount = b.TotalAmount,
                IsCancelled = b.IsCancelled,
                Status = b.IsCancelled ? "Cancelled" : (b.CheckOutDate < DateTime.Today ? "Completed" : "Active")
            })
        });

        ViewBag.MostBookedRoomType = await _bookingService.GetMostBookedRoomTypeAsync();
        return View(model);
    }

    // GET: Bookings/Create
    public async Task<IActionResult> Create()
    {
        var rooms = await _roomService.GetAvailableRoomsAsync();
        if (!rooms.Any())
        {
            TempData["Error"] = "No rooms are currently available for booking.";
            return RedirectToAction(nameof(Index));
        }

        var guests = await _guestService.GetActiveGuestsAsync();

        // Pass room prices for client-side calculation
        ViewBag.RoomPrices = rooms.ToDictionary(r => r.RoomId, r => r.PricePerNight);

        var model = new BookingCreateViewModel
        {
            RoomDropdownList = new SelectList(rooms, "RoomId", "RoomNumber"),
            GuestDropdownList = new SelectList(guests, "GuestId", "Name")
        };
        
        return View(model);
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Map ViewModel to Entity
                var booking = new Booking
                {
                    RoomId = model.RoomId.GetValueOrDefault(),
                    GuestId = model.GuestId.GetValueOrDefault(),
                    CheckInDate = model.CheckInDate.GetValueOrDefault(),
                    CheckOutDate = model.CheckOutDate.GetValueOrDefault(),
                    // TotalAmount is calculated in service
                };

                await _bookingService.BookRoomAsync(booking);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidBookingDateException ex)
            {
                ModelState.AddModelError("CheckInDate", ex.Message);
            }
            catch (BookingConflictException ex)
            {
                ModelState.AddModelError("", ex.Message); // Global error for conflict
            }
            catch (RoomUnavailableException ex)
            {
                ModelState.AddModelError("RoomId", ex.Message);
            }
            catch (GuestInactiveException ex)
            {
                ModelState.AddModelError("GuestId", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        // Repopulate lists if invalid
        var rooms = await _roomService.GetAvailableRoomsAsync();
        var guests = await _guestService.GetActiveGuestsAsync();
        ViewBag.RoomPrices = rooms.ToDictionary(r => r.RoomId, r => r.PricePerNight);
        
        model.RoomDropdownList = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);
        model.GuestDropdownList = new SelectList(guests, "GuestId", "Name", model.GuestId);

        return View(model);
    }

    // GET: Bookings/Cancel/5
    public async Task<IActionResult> Cancel(int id)
    {
         var booking = await _bookingService.GetBookingByIdAsync(id);
         if (booking == null) return NotFound();

         var model = new BookingCancelViewModel
         {
             BookingId = booking.BookingId,
             RoomNumber = booking.Room?.RoomNumber ?? "N/A",
             GuestName = booking.Guest?.Name ?? "N/A",
             CheckInDate = booking.CheckInDate,
             CheckOutDate = booking.CheckOutDate,
             TotalAmount = booking.TotalAmount
         };

         return View(model);
    }

    // POST: Bookings/Cancel/5
    [HttpPost, ActionName("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(int id)
    {
        await _bookingService.CancelBookingAsync(id);
        return RedirectToAction(nameof(Index));
    }

}
