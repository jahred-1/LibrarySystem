using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibrarySystem.Models;

namespace LibrarySystem
{
    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            InitializeComponent();
            this.Loaded += DashboardControl_Loaded;
        }

        private void DashboardControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (TotalBooksText != null)
            {
                LoadDashboardData();
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                if (TotalBooksText == null || TotalStudentsText == null || BooksIssuedText == null || OverdueBooksText == null)
                    return;

                var totalBooks = DatabaseHelper.GetTotalBooks();
                var totalStudents = DatabaseHelper.GetTotalStudents();
                var issuedBooks = DatabaseHelper.GetIssuedBooksCount();
                var overdueBooks = DatabaseHelper.GetOverdueBooksCount();

                TotalBooksText.Text = totalBooks.ToString("N0");
                TotalStudentsText.Text = totalStudents.ToString("N0");
                BooksIssuedText.Text = issuedBooks.ToString("N0");
                OverdueBooksText.Text = overdueBooks.ToString("N0");

                // Load recent activity
                LoadRecentActivity();
            }
            catch
            {
                // If database not initialized or error, show 0
                if (TotalBooksText != null) TotalBooksText.Text = "0";
                if (TotalStudentsText != null) TotalStudentsText.Text = "0";
                if (BooksIssuedText != null) BooksIssuedText.Text = "0";
                if (OverdueBooksText != null) OverdueBooksText.Text = "0";
            }
        }

        private void LoadRecentActivity()
        {
            try
            {
                if (RecentActivityItemsControl == null) return;

                var activities = new List<ActivityItem>();
                var now = DateTime.Now;

                // Get recent students (last 10, ordered by CreatedAt DESC)
                var students = DatabaseHelper.GetAllStudents()
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(5)
                    .ToList();

                foreach (var student in students)
                {
                    var timeDiff = now - student.CreatedAt;
                    string timeAgo;
                    if (timeDiff.TotalMinutes < 1)
                        timeAgo = "Just now";
                    else if (timeDiff.TotalMinutes < 60)
                        timeAgo = $"{(int)timeDiff.TotalMinutes} minute{((int)timeDiff.TotalMinutes != 1 ? "s" : "")} ago";
                    else if (timeDiff.TotalHours < 24)
                        timeAgo = $"{(int)timeDiff.TotalHours} hour{((int)timeDiff.TotalHours != 1 ? "s" : "")} ago";
                    else
                        timeAgo = $"{(int)timeDiff.TotalDays} day{((int)timeDiff.TotalDays != 1 ? "s" : "")} ago";

                    activities.Add(new ActivityItem
                    {
                        Icon = "👤",
                        IconBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3E5F5")),
                        Description = $"New student registered: {student.FullName}",
                        TimeAgo = timeAgo,
                        DateLabel = student.CreatedAt.Date == now.Date ? "Today" : student.CreatedAt.ToString("MMM d"),
                        DateColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7B1FA2")),
                        ActivityDate = student.CreatedAt
                    });
                }

                // Get recent books (last 5, ordered by AddedDate DESC)
                var books = DatabaseHelper.GetAllBooks()
                    .OrderByDescending(b => b.AddedDate)
                    .Take(5)
                    .ToList();

                foreach (var book in books)
                {
                    var timeDiff = now - book.AddedDate;
                    string timeAgo;
                    if (timeDiff.TotalMinutes < 1)
                        timeAgo = "Just now";
                    else if (timeDiff.TotalMinutes < 60)
                        timeAgo = $"{(int)timeDiff.TotalMinutes} minute{((int)timeDiff.TotalMinutes != 1 ? "s" : "")} ago";
                    else if (timeDiff.TotalHours < 24)
                        timeAgo = $"{(int)timeDiff.TotalHours} hour{((int)timeDiff.TotalHours != 1 ? "s" : "")} ago";
                    else
                        timeAgo = $"{(int)timeDiff.TotalDays} day{((int)timeDiff.TotalDays != 1 ? "s" : "")} ago";

                    activities.Add(new ActivityItem
                    {
                        Icon = "📚",
                        IconBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")),
                        Description = $"New book added: '{book.Title}'",
                        TimeAgo = timeAgo,
                        DateLabel = book.AddedDate.Date == now.Date ? "Today" : book.AddedDate.ToString("MMM d"),
                        DateColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2")),
                        ActivityDate = book.AddedDate
                    });
                }

                // Sort by date (most recent first) and take top 10
                activities = activities.OrderByDescending(a => a.ActivityDate).Take(10).ToList();

                RecentActivityItemsControl.ItemsSource = activities;
            }
            catch
            {
                if (RecentActivityItemsControl != null)
                {
                    RecentActivityItemsControl.ItemsSource = new List<ActivityItem>();
                }
            }
        }
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = string.Empty;
        public SolidColorBrush IconBackground { get; set; } = new SolidColorBrush(Colors.Gray);
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string DateLabel { get; set; } = string.Empty;
        public SolidColorBrush DateColor { get; set; } = new SolidColorBrush(Colors.Black);
        public DateTime ActivityDate { get; set; } = DateTime.Now;
    }
}
