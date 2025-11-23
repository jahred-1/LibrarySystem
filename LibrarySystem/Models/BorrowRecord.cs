using System;

namespace LibrarySystem.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int BookId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Issued"; // Issued, Returned, Overdue
        public decimal? FineAmount { get; set; }
    }
}





