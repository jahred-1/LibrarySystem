using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class BorrowHistoryControl : UserControl
    {
        public BorrowHistoryControl()
        {
            InitializeComponent();
            this.Loaded += BorrowHistoryControl_Loaded;
        }

        private void BorrowHistoryControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Only load if the items control exists
            if (HistoryItemsControl != null)
            {
                LoadHistory();
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (HistoryItemsControl == null) return;

                if (MainWindow.CurrentStudent == null)
                {
                    HistoryItemsControl.ItemsSource = new List<HistoryItemView>();
                    return;
                }

                var records = DatabaseHelper.GetBorrowRecordsByStudent(MainWindow.CurrentStudent.Id)
                    .OrderByDescending(r => r.IssueDate)
                    .ToList();

                // Create history views
                var historyViews = new List<HistoryItemView>();
                foreach (var record in records)
                {
                    var book = DatabaseHelper.GetBookById(record.BookId);
                    if (book != null)
                    {
                        var view = new HistoryItemView
                        {
                            BookTitle = book.Title,
                            BookAuthor = $"by {book.Author}",
                            BorrowedDate = record.IssueDate.ToString("MMM d, yyyy"),
                            ReturnedDate = record.ReturnDate?.ToString("MMM d, yyyy") ?? "Not returned"
                        };

                        if (record.Status == "Returned" && record.ReturnDate.HasValue)
                        {
                            var daysLate = (record.ReturnDate.Value.Date - record.DueDate.Date).Days;
                            if (daysLate > 0)
                            {
                                view.LateText = $"({daysLate} day{(daysLate != 1 ? "s" : "") } late)";
                                view.StatusText = "⚠ Late Return";
                                view.StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F97316")); // amber
                            }
                            else
                            {
                                view.StatusText = "✓ Returned";
                                view.StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // green
                            }
                        }
                        else if (record.Status == "Issued")
                        {
                            view.StatusText = "Issued";
                            // Use amber instead of blue for issued status per UI change
                            view.StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                        }
                        else
                        {
                            view.StatusText = record.Status;
                            view.StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
                        }

                        historyViews.Add(view);
                    }
                }

                HistoryItemsControl.ItemsSource = historyViews;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadHistory error: {ex.Message}");
                if (HistoryItemsControl != null)
                {
                    HistoryItemsControl.ItemsSource = new List<HistoryItemView>();
                }
            }
        }
    }

    public class HistoryItemView
    {
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BorrowedDate { get; set; } = string.Empty;
        public string ReturnedDate { get; set; } = string.Empty;
        public string LateText { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public SolidColorBrush StatusBackground { get; set; } = new SolidColorBrush(Colors.Gray);
    }
}
