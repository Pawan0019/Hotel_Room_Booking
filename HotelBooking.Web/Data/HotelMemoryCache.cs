using HotelBooking.Web.Models;
using System.Collections.Concurrent;

namespace HotelBooking.Web.Data;

public class HotelMemoryCache
{
    // Thread-safe dictionary for fast lookups
    public ConcurrentDictionary<int, Room> RoomsDict { get; private set; } = new();

    // Thread-safe lists (using ConcurrentBag or just locking standard lists if complex ops needed)
    // Requirement says "List<Room>", "List<Guest>", "List<Booking>"
    // For simplicity and requirement compliance, we'll use List but manage access safely if needed.
    // However, for a singleton service in a web app, thread safety is crucial. 
    // I will use properties that return the underlying collections or copies.
    
    // Backing fields
    private List<Room> _rooms = new();
    private List<Guest> _guests = new();
    private List<Booking> _bookings = new();
    
    // Lock objects
    private readonly object _roomLock = new();
    private readonly object _guestLock = new();
    private readonly object _bookingLock = new();

    public List<Room> Rooms 
    { 
        get { lock(_roomLock) { return new List<Room>(_rooms); } }
        set { lock(_roomLock) { _rooms = value; RebuildRoomDict(); } }
    }

    public List<Guest> Guests
    {
        get { lock(_guestLock) { return new List<Guest>(_guests); } }
        set { lock(_guestLock) { _guests = value; } }
    }

    public List<Booking> Bookings
    {
        get { lock(_bookingLock) { return new List<Booking>(_bookings); } }
        set { lock(_bookingLock) { _bookings = value; } }
    }

    public void AddRoom(Room room)
    {
        lock(_roomLock) 
        { 
            _rooms.Add(room); 
            RoomsDict.TryAdd(room.RoomId, room);
        }
    }

    public void UpdateRoom(Room room)
    {
        lock(_roomLock)
        {
            var existing = _rooms.FirstOrDefault(r => r.RoomId == room.RoomId);
            if (existing != null)
            {
                _rooms.Remove(existing);
                _rooms.Add(room);
                RoomsDict[room.RoomId] = room;
            }
        }
    }

    public void DeleteRoom(int id)
    {
        lock(_roomLock)
        {
            var existing = _rooms.FirstOrDefault(r => r.RoomId == id);
            if (existing != null)
            {
                _rooms.Remove(existing);
                RoomsDict.TryRemove(id, out _);
            }
        }
    }

    public void AddGuest(Guest guest)
    {
        lock(_guestLock) { _guests.Add(guest); }
    }

    public void UpdateGuest(Guest guest)
    {
        lock(_guestLock)
        {
            var existing = _guests.FirstOrDefault(g => g.GuestId == guest.GuestId);
            if (existing != null)
            {
                _guests.Remove(existing);
                _guests.Add(guest);
            }
        }
    }

    public void DeleteGuest(int id)
    {
        lock(_guestLock)
        {
            var existing = _guests.FirstOrDefault(g => g.GuestId == id);
            if (existing != null) _guests.Remove(existing);
        }
    }

    public void AddBooking(Booking booking)
    {
        lock(_bookingLock) { _bookings.Add(booking); }
    }

    public void UpdateBooking(Booking booking)
    {
        lock(_bookingLock)
        {
            var existing = _bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
            if (existing != null)
            {
                _bookings.Remove(existing);
                _bookings.Add(booking);
            }
        }
    }

    public void DeleteBooking(int id)
    {
        lock(_bookingLock)
        {
            var existing = _bookings.FirstOrDefault(b => b.BookingId == id);
            if (existing != null) _bookings.Remove(existing);
        }
    }

    private void RebuildRoomDict()
    {
        RoomsDict.Clear();
        foreach (var room in _rooms)
        {
            RoomsDict.TryAdd(room.RoomId, room);
        }
    }
}
