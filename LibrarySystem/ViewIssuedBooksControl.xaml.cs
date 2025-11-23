using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class ViewIssuedBooksControl : UserControl
    {
        public ViewIssuedBooksControl()
        {
            InitializeComponent();
            this.Loaded += ViewIssuedBooksControl_Loaded;
        }

        private void ViewIssuedBooksControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IssuedBooksDataGrid != null)
            {
                LoadIssuedBooks();
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null && SearchTextBox.Text == "Search by Student ID or Book ID...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null && string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search by Student ID or Book ID...";
                SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999"));
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadIssuedBooks();
        }

        private void LoadIssuedBooks()
        {
            try
            {
                if (IssuedBooksDataGrid == null) return;

                var records = DatabaseHelper.GetAllBorrowRecords().Where(r => r.Status == "Issued").ToList();
                var issuedViews = new List<IssuedBookView>();

                foreach (var record in records)
                {
                    var student = DatabaseHelper.GetStudentById(record.StudentId);
                    var book = DatabaseHelper.GetBookById(record.BookId);
                    
                    issuedViews.Add(new IssuedBookView
                    {
                        BookId = record.BookId,
                        BookTitle = book?.Title ?? "Unknown",
                        StudentId = student?.StudentId ?? record.StudentId.ToString(),
                        StudentName = student?.FullName ?? "Unknown",
                        IssueDate = record.IssueDate,
                        DueDate = record.DueDate
                    });
                }

                IssuedBooksDataGrid.ItemsSource = issuedViews;

                // Update stats
                if (CurrentlyIssuedText != null) CurrentlyIssuedText.Text = records.Count.ToString();
                if (DueTodayText != null) DueTodayText.Text = records.Count(r => r.DueDate.Date == DateTime.Now.Date).ToString();
                if (OverdueText != null) OverdueText.Text = records.Count(r => r.DueDate < DateTime.Now).ToString();
            }
            catch
            {
                // Silently fail if control is not initialized
            }
        }
    }

    public class IssuedBookView
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
