using HotelBooking.Web.Models;
using HotelBooking.Web.ViewModels;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.Controllers;

public class GuestsController : Controller
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    // GET: Guests
    public async Task<IActionResult> Index()
    {
        var guests = await _guestService.GetAllGuestsAsync();
        var model = guests.Select(g => new GuestListViewModel
        {
            GuestId = g.GuestId,
            Name = g.Name,
            Email = g.Email,
            Phone = g.Phone,
            IsActive = g.IsActive
        });
        return View(model);
    }

    // GET: Guests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null)
        {
            return NotFound();
        }

        var model = new GuestListViewModel
        {
            GuestId = guest.GuestId,
            Name = guest.Name,
            Email = guest.Email,
            Phone = guest.Phone,
            IsActive = guest.IsActive
        };

        return View(model);
    }

    // GET: Guests/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Guests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GuestCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var guest = new Guest
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = true // Default to true
            };

            try
            {
                await _guestService.AddGuestAsync(guest);
                return RedirectToAction(nameof(Index));
            }
            catch (DuplicateGuestException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(model);
    }

    // GET: Guests/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null)
        {
            return NotFound();
        }

        var model = new GuestEditViewModel
        {
            GuestId = guest.GuestId,
            Name = guest.Name,
            Email = guest.Email,
            Phone = guest.Phone,
            IsActive = guest.IsActive
        };

        return View(model);
    }

    // POST: Guests/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GuestEditViewModel model)
    {
        if (id != model.GuestId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (!await _guestService.IsEmailUniqueAsync(model.Email, id))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            if (!await _guestService.IsPhoneUniqueAsync(model.Phone, id))
            {
                ModelState.AddModelError("Phone", "Phone number already exists.");
                return View(model);
            }

            try
            {
                var guest = new Guest
                {
                    GuestId = model.GuestId,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    IsActive = model.IsActive
                };
                await _guestService.UpdateGuestAsync(guest);
                return RedirectToAction(nameof(Index));
            }
            catch (DuplicateGuestException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(model);
    }

    // GET: Guests/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null)
        {
            return NotFound();
        }

        var model = new GuestListViewModel
        {
            GuestId = guest.GuestId,
            Name = guest.Name,
            Email = guest.Email,
            Phone = guest.Phone,
            IsActive = guest.IsActive
        };

        return View(model);
    }

    // POST: Guests/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _guestService.HasActiveBookingsAsync(id))
        {
            var guest = await _guestService.GetGuestByIdAsync(id);
             if (guest == null) return NotFound();

            var model = new GuestListViewModel
            {
                GuestId = guest.GuestId,
                Name = guest.Name,
                Email = guest.Email,
                Phone = guest.Phone,
                IsActive = guest.IsActive
            };
            ViewData["ErrorMessage"] = "Cannot delete guest with active bookings. Please cancel bookings first.";
            return View(model);
        }

        await _guestService.DeleteGuestAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // POST: Guests/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _guestService.DeactivateGuestAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // POST: Guests/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        await _guestService.ActivateGuestAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
