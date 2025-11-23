using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class ReturnBookControl : UserControl
    {
        private BorrowRecord? currentRecord;

        public ReturnBookControl()
        {
            InitializeComponent();
        }

        private void RecordIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(RecordIdTextBox.Text, out int recordId))
            {
                var records = DatabaseHelper.GetAllBorrowRecords();
                currentRecord = records.FirstOrDefault(r => r.Id == recordId && r.Status == "Issued");
                
                if (currentRecord != null)
                {
                    var daysOverdue = (DateTime.Now - currentRecord.DueDate).Days;
                    if (daysOverdue > 0)
                    {
                        decimal fine = daysOverdue * 10; // ₱10 per day
                        LateFeeTextBlock.Text = $"₱{fine:F2}";
                    }
                    else
                    {
                        LateFeeTextBlock.Text = "₱0.00";
                    }
                }
                else
                {
                    LateFeeTextBlock.Text = "₱0.00";
                }
            }
            else
            {
                LateFeeTextBlock.Text = "₱0.00";
            }
        }

        private void ReturnBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RecordIdTextBox.Text, out int recordId))
            {
                MessageBox.Show("Please enter a valid Borrow Record ID.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (currentRecord == null)
            {
                MessageBox.Show("Record not found or already returned.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DatabaseHelper.ReturnBook(recordId))
            {
                var book = DatabaseHelper.GetBookById(currentRecord.BookId);
                MessageBox.Show($"Book '{book?.Title}' returned successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ClearButton_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Failed to return book. Please try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            RecordIdTextBox.Clear();
            LateFeeTextBlock.Text = "₱0.00";
            currentRecord = null;
        }
    }
}
