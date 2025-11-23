using System;

namespace LibrarySystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int BookId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? HoldUntilDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Ready, Cancelled, Completed
    }
}





