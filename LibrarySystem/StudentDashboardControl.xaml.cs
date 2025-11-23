using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibrarySystem.Models;

namespace LibrarySystem
{
    /// <summary>
    /// Interaction logic for StudentDashboardControl.xaml
    /// </summary>
    public partial class StudentDashboardControl : UserControl
    {
        public StudentDashboardControl()
        {
            InitializeComponent();
            this.Loaded += StudentDashboardControl_Loaded;
            this.Unloaded += StudentDashboardControl_Unloaded;

            // Subscribe to DB changes for near real-time updates
            DatabaseHelper.DataChanged += OnDatabaseChanged;
        }

        private void StudentDashboardControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (BorrowedText != null)
            {
                LoadDashboardData();
                LoadRecentActivity();
            }
        }

        private void StudentDashboardControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseHelper.DataChanged -= OnDatabaseChanged;
            }
            catch { }
        }

        private void OnDatabaseChanged()
        {
            // Ensure UI thread
            Dispatcher.Invoke(() =>
            {
                LoadDashboardData();
                LoadRecentActivity();
            });
        }

        private void LoadDashboardData()
        {
            try
            {
                if (BorrowedText == null || ReturnedText == null || ReservationsText == null || DueSoonText == null)
                    return;

                if (MainWindow.CurrentStudent == null)
                {
                    BorrowedText.Text = "0";
                    ReturnedText.Text = "0";
                    ReservationsText.Text = "0";
                    DueSoonText.Text = "0";
                    return;
                }

                var records = DatabaseHelper.GetBorrowRecordsByStudent(MainWindow.CurrentStudent.Id);
                var borrowed = records.Count(r => r.Status == "Issued");
                var returned = records.Count(r => r.Status == "Returned");
                var reservations = DatabaseHelper.GetReservationsByStudent(MainWindow.CurrentStudent.Id).Count;
                var dueSoon = records.Count(r => r.Status == "Issued" && r.DueDate.Date <= DateTime.Now.AddDays(3).Date && r.DueDate.Date >= DateTime.Now.Date);

                BorrowedText.Text = borrowed.ToString();
                ReturnedText.Text = returned.ToString();
                ReservationsText.Text = reservations.ToString();
                DueSoonText.Text = dueSoon.ToString();
            }
            catch
            {
                if (BorrowedText != null) BorrowedText.Text = "0";
                if (ReturnedText != null) ReturnedText.Text = "0";
                if (ReservationsText != null) ReservationsText.Text = "0";
                if (DueSoonText != null) DueSoonText.Text = "0";
            }
        }

        private void LoadRecentActivity()
        {
            try
            {
                if (RecentActivityItemsControl == null) return;

                var activities = new List<ActivityItem>();
                var now = DateTime.Now;

                if (MainWindow.CurrentStudent == null)
                {
                    RecentActivityItemsControl.ItemsSource = activities;
                    return;
                }

                // Student's recent borrow/return activities
                var records = DatabaseHelper.GetBorrowRecordsByStudent(MainWindow.CurrentStudent.Id)
                    .OrderByDescending(r => r.IssueDate)
                    .Take(10)
                    .ToList();

                foreach (var r in records)
                {
                    var book = DatabaseHelper.GetBookById(r.BookId);
                    var desc = book != null ? $"{(r.Status == "Issued" ? "Borrowed" : r.Status)}: {book.Title}" : "Book activity";
                    var timeDiff = now - r.IssueDate;
                    string timeAgo;
                    if (timeDiff.TotalMinutes < 1) timeAgo = "Just now";
                    else if (timeDiff.TotalMinutes < 60) timeAgo = $"{(int)timeDiff.TotalMinutes} minute{((int)timeDiff.TotalMinutes != 1 ? "s" : "")} ago";
                    else if (timeDiff.TotalHours < 24) timeAgo = $"{(int)timeDiff.TotalHours} hour{((int)timeDiff.TotalHours != 1 ? "s" : "")} ago";
                    else timeAgo = $"{(int)timeDiff.TotalDays} day{((int)timeDiff.TotalDays != 1 ? "s" : "")} ago";

                    activities.Add(new ActivityItem
                    {
                        Description = desc,
                        TimeAgo = timeAgo,
                        DateLabel = r.IssueDate.Date == now.Date ? "Today" : r.IssueDate.ToString("MMM d"),
                        DateColor = UiHelpers.BrushFromHex("#1976D2"),
                        ActivityDate = r.IssueDate
                    });
                }

                // Student's recent reservations
                var res = DatabaseHelper.GetReservationsByStudent(MainWindow.CurrentStudent.Id)
                    .OrderByDescending(x => x.ReservationDate)
                    .Take(10)
                    .ToList();

                foreach (var reservation in res)
                {
                    var book = DatabaseHelper.GetBookById(reservation.BookId);
                    var timeDiff = now - reservation.ReservationDate;
                    string timeAgo;
                    if (timeDiff.TotalMinutes < 1) timeAgo = "Just now";
                    else if (timeDiff.TotalMinutes < 60) timeAgo = $"{(int)timeDiff.TotalMinutes} minute{((int)timeDiff.TotalMinutes != 1 ? "s" : "")} ago";
                    else if (timeDiff.TotalHours < 24) timeAgo = $"{(int)timeDiff.TotalHours} hour{((int)timeDiff.TotalHours != 1 ? "s" : "")} ago";
                    else timeAgo = $"{(int)timeDiff.TotalDays} day{((int)timeDiff.TotalDays != 1 ? "s" : "")} ago";

                    activities.Add(new ActivityItem
                    {
                        Description = $"Reservation: {book?.Title ?? "Unknown"}",
                        TimeAgo = timeAgo,
                        DateLabel = reservation.ReservationDate.Date == now.Date ? "Today" : reservation.ReservationDate.ToString("MMM d"),
                        DateColor = UiHelpers.BrushFromHex("#F59E0B"),
                        ActivityDate = reservation.ReservationDate
                    });
                }

                var sorted = activities.OrderByDescending(a => a.ActivityDate).Take(10).ToList();
                RecentActivityItemsControl.ItemsSource = sorted;
            }
            catch
            {
                if (RecentActivityItemsControl != null)
                    RecentActivityItemsControl.ItemsSource = new List<ActivityItem>();
            }
        }

        private void BrowseBooks_Click(object sender, RoutedEventArgs e)
        {
            var parent = Window.GetWindow(this);
            if (parent is StudentPortal sp)
            {
                sp.ContentArea1.Content = new BrowseBooksControl();
            }
            else
            {
                // fallback: open in a new window
                var win = new Window { Title = "Browse Books", Width = 900, Height = 700, Content = new BrowseBooksControl(), Owner = parent };
                win.ShowDialog();
            }
        }

        private void MyBooks_Click(object sender, RoutedEventArgs e)
        {
            var parent = Window.GetWindow(this);
            if (parent is StudentPortal sp)
            {
                sp.ContentArea1.Content = new MyBorrowedBooksControl();
            }
            else
            {
                var win = new Window { Title = "My Books", Width = 900, Height = 700, Content = new MyBorrowedBooksControl(), Owner = parent };
                win.ShowDialog();
            }
        }
    }
}
