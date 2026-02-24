using System;

namespace HotelBooking.Web.Models;

public class RoomUnavailableException : InvalidOperationException
{
    public RoomUnavailableException() : base("The selected room is currently unavailable for maintenance or other reasons.") { }
    public RoomUnavailableException(string message) : base(message) { }
}

public class GuestInactiveException : InvalidOperationException
{
    public GuestInactiveException() : base("The selected guest is inactive and cannot make bookings.") { }
    public GuestInactiveException(string message) : base(message) { }
}

public class InvalidBookingDateException : InvalidOperationException
{
    public InvalidBookingDateException() : base("The selected dates are invalid.") { }
    public InvalidBookingDateException(string message) : base(message) { }
}

public class BookingConflictException : InvalidOperationException
{
    public BookingConflictException() : base("The room is already booked for the selected dates.") { }
    public BookingConflictException(string message) : base(message) { }
}

public class DuplicateRoomException : InvalidOperationException
{
    public DuplicateRoomException() : base("A room with this number already exists.") { }
    public DuplicateRoomException(string message) : base(message) { }
}

public class DuplicateGuestException : InvalidOperationException
{
    public DuplicateGuestException() : base("A guest with this email or phone number already exists.") { }
    public DuplicateGuestException(string message) : base(message) { }
}
