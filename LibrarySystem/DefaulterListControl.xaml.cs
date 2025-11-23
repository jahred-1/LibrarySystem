using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class DefaulterListControl : UserControl
    {
        public DefaulterListControl()
        {
            InitializeComponent();
            this.Loaded += DefaulterListControl_Loaded;
        }

        private void DefaulterListControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DefaultersDataGrid != null)
            {
                LoadDefaulters();
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null && SearchTextBox.Text == "Search by Student ID or Name...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null && string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search by Student ID or Name...";
                SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999"));
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDefaulters();
        }

        private void LoadDefaulters()
        {
            try
            {
                if (DefaultersDataGrid == null) return;

                var overdueRecords = DatabaseHelper.GetOverdueBooks();
                var defaulterViews = new List<DefaulterView>();
                decimal totalFines = 0;

                foreach (var record in overdueRecords)
                {
                    var student = DatabaseHelper.GetStudentById(record.StudentId);
                    var book = DatabaseHelper.GetBookById(record.BookId);
                    var daysOverdue = (DateTime.Now - record.DueDate).Days;
                    var fine = daysOverdue * 10; // ₱10 per day
                    totalFines += fine;

                    defaulterViews.Add(new DefaulterView
                    {
                        StudentId = student?.StudentId ?? record.StudentId.ToString(),
                        StudentName = student?.FullName ?? "Unknown",
                        BookId = record.BookId,
                        BookTitle = book?.Title ?? "Unknown",
                        DaysOverdue = daysOverdue,
                        FineAmount = fine
                    });
                }

                DefaultersDataGrid.ItemsSource = defaulterViews;
                if (TotalDefaultersText != null) TotalDefaultersText.Text = $"{defaulterViews.Count} Students";
                if (TotalFinesText != null) TotalFinesText.Text = $"₱{totalFines:F2}";
            }
            catch
            {
                // Silently fail if control is not initialized
            }
        }
    }

    public class DefaulterView
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public decimal FineAmount { get; set; }
    }
}
