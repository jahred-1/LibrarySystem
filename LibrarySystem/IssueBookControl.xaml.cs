using System;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class IssueBookControl : UserControl
    {
        public IssueBookControl()
        {
            InitializeComponent();
        }

        private void IssueBookButton_Click(object sender, RoutedEventArgs e)
        {
            string studentIdText = StudentIdTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentIdText))
            {
                MessageBox.Show("Please enter a Student ID.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(BookIdTextBox.Text, out int bookId))
            {
                MessageBox.Show("Please enter a valid Book ID.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int days = 14;
            if (!string.IsNullOrWhiteSpace(DaysTextBox.Text) && !int.TryParse(DaysTextBox.Text, out days))
            {
                MessageBox.Show("Please enter a valid number of days.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verify student exists by StudentId string (e.g., "24-1861")
            var student = DatabaseHelper.GetStudentByEmailOrId(studentIdText);
            if (student == null)
            {
                MessageBox.Show("Student not found. Please check the Student ID.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Verify book exists
            var book = DatabaseHelper.GetBookById(bookId);
            if (book == null)
            {
                MessageBox.Show("Book not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (book.AvailableCopies <= 0)
            {
                MessageBox.Show("This book is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Use the student's database ID (not the StudentId string) for IssueBook
            if (DatabaseHelper.IssueBook(student.Id, bookId, days))
            {
                MessageBox.Show($"Book '{book.Title}' issued to {student.FullName} successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ClearButton_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Failed to issue book. Please try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            StudentIdTextBox.Clear();
            BookIdTextBox.Clear();
            DaysTextBox.Text = "14";
        }
    }
}
