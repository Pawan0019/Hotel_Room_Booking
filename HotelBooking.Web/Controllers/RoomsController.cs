using HotelBooking.Web.Models;
using HotelBooking.Web.ViewModels;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // GET: Rooms
    public async Task<IActionResult> Index(string searchString, string roomType, string sortOrder)
    {
        ViewData["CurrentFilter"] = searchString;
        ViewData["TypeFilter"] = roomType;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["AvailableFilter"] = "available";

        IEnumerable<Room> rooms;

        if (!string.IsNullOrEmpty(searchString))
        {
            rooms = await _roomService.SearchRoomByNumberAsync(searchString);
        }
        else if (!string.IsNullOrEmpty(roomType))
        {
            rooms = await _roomService.FilterRoomsByTypeAsync(roomType);
        }
        else
        {
            rooms = await _roomService.GetAllRoomsAsync();
        }

        if (sortOrder == "price_desc")
        {
            rooms = await _roomService.SortRoomsByPriceDescendingAsync();
        }
        else if (sortOrder == "price_asc")
        {
             rooms = await _roomService.SortRoomsByPriceAscendingAsync();
        }

        // Map to ViewModel
        var model = rooms.Select(r => new RoomListViewModel
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            RoomType = r.RoomType,
            PricePerNight = r.PricePerNight,
            IsAvailable = r.IsAvailable
        });

        return View(model);
    }
    
    public async Task<IActionResult> Available()
    {
        var rooms = await _roomService.GetAvailableRoomsAsync();
        var model = rooms.Select(r => new RoomListViewModel
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            RoomType = r.RoomType,
            PricePerNight = r.PricePerNight,
            IsAvailable = r.IsAvailable
        });
        return View("Index", model);
    }

    // GET: Rooms/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        var model = new RoomListViewModel
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            IsAvailable = room.IsAvailable
        };

        return View(model);
    }

    // GET: Rooms/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Rooms/Create
    // POST: Rooms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var room = new Room
            {
                RoomNumber = model.RoomNumber,
                RoomType = model.RoomType,
                PricePerNight = model.PricePerNight,
                IsAvailable = model.IsAvailable
            };

            try
            {
                await _roomService.AddRoomAsync(room);
                return RedirectToAction(nameof(Index));
            }
            catch (DuplicateRoomException ex)
            {
                ModelState.AddModelError("RoomNumber", ex.Message);
            }
        }
        return View(model);
    }

    // GET: Rooms/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        var model = new RoomEditViewModel
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            IsAvailable = room.IsAvailable
        };

        return View(model);
    }

    // POST: Rooms/Edit/5
    // POST: Rooms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoomEditViewModel model)
    {
        if (id != model.RoomId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (!await _roomService.IsRoomNumberUniqueAsync(model.RoomNumber, id))
            {
                ModelState.AddModelError("RoomNumber", "Room Number already exists.");
                return View(model);
            }

            // Check if room has active bookings
            if (await _roomService.HasActiveBookingsAsync(id))
            {
                var existingRoom = await _roomService.GetRoomByIdAsync(id);
                // Cannot change RoomType if occupied/booked
                if (existingRoom != null && existingRoom.RoomType != model.RoomType)
                {
                    ModelState.AddModelError("RoomType", "Cannot change Room Type for a room with active bookings.");
                    return View(model);
                }
                // Cannot change IsAvailable manually if occupied/booked
                // Note: logic might be complex depending on if we are freeing it or blocking it. 
                // Requirement: "Cannot change availability manually"
                if (existingRoom != null && existingRoom.IsAvailable != model.IsAvailable)
                {
                    ModelState.AddModelError("IsAvailable", "Cannot change Availability manually for a room with active bookings.");
                    return View(model);
                }
            }

            try
            {
                var room = new Room
                {
                    RoomId = model.RoomId,
                    RoomNumber = model.RoomNumber,
                    RoomType = model.RoomType,
                    PricePerNight = model.PricePerNight,
                    IsAvailable = model.IsAvailable
                };
                await _roomService.UpdateRoomAsync(room);
            }
            catch (Exception)
            {
                if ((await _roomService.GetRoomByIdAsync(model.RoomId)) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: Rooms/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        var model = new RoomListViewModel
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            IsAvailable = room.IsAvailable
        };

        return View(model);
    }

    // POST: Rooms/Delete/5
    // POST: Rooms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _roomService.HasActiveBookingsAsync(id))
        {
            // Since we can't easily return a View with an error message in this pattern (redirect),
            // we will fetch the model and return the View with error.
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null) return NotFound();

            var model = new RoomListViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable
            };
            
            ViewData["ErrorMessage"] = "Cannot delete room with active bookings. Please cancel bookings first.";
            return View(model);
        }

        await _roomService.DeleteRoomAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
