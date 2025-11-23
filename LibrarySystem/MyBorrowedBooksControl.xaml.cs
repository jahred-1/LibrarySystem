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
    /// Interaction logic for MyBorrowedBooksControl.xaml
    /// </summary>
    public partial class MyBorrowedBooksControl : UserControl
    {
        public MyBorrowedBooksControl()
        {
            InitializeComponent();
            this.Loaded += MyBorrowedBooksControl_Loaded;
        }

        private void MyBorrowedBooksControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentlyBorrowedText != null)
            {
                LoadBorrowedBooks();
            }
        }

        private void LoadBorrowedBooks()
        {
            try
            {
                if (CurrentlyBorrowedText == null || DueThisWeekText == null || OverdueText == null || BorrowedBooksItemsControl == null) return;

                if (MainWindow.CurrentStudent == null)
                {
                    CurrentlyBorrowedText.Text = "0";
                    DueThisWeekText.Text = "0";
                    OverdueText.Text = "0";
                    BorrowedBooksItemsControl.ItemsSource = new List<BorrowedBookView>();
                    return;
                }

                var records = DatabaseHelper.GetBorrowRecordsByStudent(MainWindow.CurrentStudent.Id)
                    .Where(r => r.Status == "Issued").ToList();
                
                var currentlyBorrowed = records.Count;
                var dueThisWeek = records.Count(r => r.DueDate.Date >= DateTime.Now.Date && r.DueDate.Date <= DateTime.Now.AddDays(7).Date);
                var overdue = records.Count(r => r.DueDate < DateTime.Now);

                CurrentlyBorrowedText.Text = currentlyBorrowed.ToString();
                DueThisWeekText.Text = dueThisWeek.ToString();
                OverdueText.Text = overdue.ToString();

                // Create book views
                var bookViews = new List<BorrowedBookView>();
                var now = DateTime.Now;

                foreach (var record in records)
                {
                    var book = DatabaseHelper.GetBookById(record.BookId);
                    if (book != null)
                    {
                        var daysRemaining = (record.DueDate.Date - now.Date).Days;
                        var totalDays = (record.DueDate.Date - record.IssueDate.Date).Days;
                        var elapsedDays = (now.Date - record.IssueDate.Date).Days;
                        var progressValue = totalDays > 0 ? (elapsedDays / (double)totalDays) * 100 : 0;
                        progressValue = Math.Min(Math.Max(progressValue, 0), 100);

                        var view = new BorrowedBookView
                        {
                            RecordId = record.Id,
                            BookTitle = book.Title,
                            BookAuthor = $"by {book.Author}",
                            BookISBN = $"ISBN: {book.ISBN}",
                            BorrowedDate = record.IssueDate.ToString("MMM d, yyyy"),
                            DueDate = record.DueDate.ToString("MMM d, yyyy")
                        };

                        if (daysRemaining < 0)
                        {
                            view.DueDateColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                            var absDays = Math.Abs(daysRemaining);
                            view.DaysRemaining = $"{absDays} day{(absDays != 1 ? "s" : "")} overdue";
                            view.DaysRemainingColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                            view.DaysRemainingWeight = FontWeights.SemiBold;
                            view.ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                        }
                        else if (daysRemaining == 0)
                        {
                            view.DueDateColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                            view.DaysRemaining = "Due today!";
                            view.DaysRemainingColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                            view.DaysRemainingWeight = FontWeights.SemiBold;
                            view.ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                        }
                        else if (daysRemaining <= 3)
                        {
                            view.DueDateColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                            var days = daysRemaining;
                            view.DaysRemaining = $"{days} day{(days != 1 ? "s" : "")} remaining";
                            view.DaysRemainingColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                            view.DaysRemainingWeight = FontWeights.Normal;
                            view.ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                        }
                        else
                        {
                            view.DueDateColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                            var days = daysRemaining;
                            view.DaysRemaining = $"{days} day{(days != 1 ? "s" : "")} remaining";
                            view.DaysRemainingColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                            view.DaysRemainingWeight = FontWeights.Normal;
                            view.ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
                        }

                        view.ProgressValue = progressValue;
                        bookViews.Add(view);
                    }
                }

                BorrowedBooksItemsControl.ItemsSource = bookViews;
            }
            catch
            {
                if (CurrentlyBorrowedText != null) CurrentlyBorrowedText.Text = "0";
                if (DueThisWeekText != null) DueThisWeekText.Text = "0";
                if (OverdueText != null) OverdueText.Text = "0";
                if (BorrowedBooksItemsControl != null) BorrowedBooksItemsControl.ItemsSource = new List<BorrowedBookView>();
            }
        }

        private void RenewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null || button.Tag == null)
                {
                    MessageBox.Show("Invalid book record.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MainWindow.CurrentStudent == null)
                {
                    MessageBox.Show("Please log in to renew books.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int recordId = Convert.ToInt32(button.Tag);
                var record = DatabaseHelper.GetBorrowRecordById(recordId);
                
                if (record == null || record.Status != "Issued")
                {
                    MessageBox.Show("This book record is not valid for renewal.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DatabaseHelper.RenewBook(recordId, 14))
                {
                    var book = DatabaseHelper.GetBookById(record.BookId);
                    MessageBox.Show($"Book '{book?.Title}' renewed successfully for 14 additional days!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBorrowedBooks(); // Refresh the list
                }
                else
                {
                    MessageBox.Show("Failed to renew book. Please try again.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null || button.Tag == null)
                {
                    MessageBox.Show("Invalid book record.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MainWindow.CurrentStudent == null)
                {
                    MessageBox.Show("Please log in to return books.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to return this book?", 
                    "Return Book", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int recordId = Convert.ToInt32(button.Tag);
                    var record = DatabaseHelper.GetBorrowRecordById(recordId);
                    
                    if (record == null || record.Status != "Issued")
                    {
                        MessageBox.Show("This book record is not valid for return.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (DatabaseHelper.ReturnBook(recordId))
                    {
                        var book = DatabaseHelper.GetBookById(record.BookId);
                        MessageBox.Show($"Book '{book?.Title}' returned successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBorrowedBooks(); // Refresh the list
                    }
                    else
                    {
                        MessageBox.Show("Failed to return book. Please try again.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }

    public class BorrowedBookView
    {
        public int RecordId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public string BorrowedDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public SolidColorBrush DueDateColor { get; set; } = new SolidColorBrush(Colors.Black);
        public double ProgressValue { get; set; }
        public SolidColorBrush ProgressColor { get; set; } = new SolidColorBrush(Colors.Blue);
        public string DaysRemaining { get; set; } = string.Empty;
        public SolidColorBrush DaysRemainingColor { get; set; } = new SolidColorBrush(Colors.Gray);
        public FontWeight DaysRemainingWeight { get; set; } = FontWeights.Normal;
    }
}
