using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class MyReservationsControl : UserControl
    {
        public MyReservationsControl()
        {
            InitializeComponent();
            this.Loaded += MyReservationsControl_Loaded;
        }

        private void MyReservationsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ActiveReservationsText != null)
            {
                LoadReservations();
            }
        }

        private void LoadReservations()
        {
            try
            {
                if (ActiveReservationsText == null || ReservationsItemsControl == null) return;

                if (MainWindow.CurrentStudent == null)
                {
                    ActiveReservationsText.Text = "0";
                    ReservationsItemsControl.ItemsSource = new List<ReservationView>();
                    return;
                }

                var reservations = DatabaseHelper.GetReservationsByStudent(MainWindow.CurrentStudent.Id);
                var activeCount = reservations.Count(r => r.Status == "Pending" || r.Status == "Ready");

                ActiveReservationsText.Text = activeCount.ToString();

                // Create reservation views
                var reservationViews = new List<ReservationView>();
                foreach (var reservation in reservations.Where(r => r.Status != "Cancelled" && r.Status != "Completed"))
                {
                    var book = DatabaseHelper.GetBookById(reservation.BookId);
                    if (book != null)
                    {
                        var view = new ReservationView
                        {
                            ReservationId = reservation.Id,
                            BookTitle = book.Title,
                            BookAuthor = book.Author,
                            BookISBN = $"ISBN: {book.ISBN}",
                            ReservedDate = $"Reserved: {reservation.ReservationDate:MMM d, yyyy}",
                            HoldUntil = reservation.HoldUntilDate.HasValue
                                ? $"Hold Until: {reservation.HoldUntilDate.Value:MMM d, yyyy}"
                                : "Hold Until: N/A",
                            Status = reservation.Status,
                            CanPickUp = reservation.Status == "Ready"
                        };

                        if (reservation.Status == "Ready")
                        {
                            view.StatusText = "✓ READY FOR PICKUP";
                            view.StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                        }
                        else if (reservation.Status == "Pending")
                        {
                            view.StatusText = "⏳ PENDING";
                            view.StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                        }
                        else
                        {
                            view.StatusText = reservation.Status.ToUpper();
                            view.StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                        }

                        reservationViews.Add(view);
                    }
                }

                ReservationsItemsControl.ItemsSource = reservationViews;
            }
            catch
            {
                if (ActiveReservationsText != null)
                {
                    ActiveReservationsText.Text = "0";
                }
                if (ReservationsItemsControl != null)
                {
                    ReservationsItemsControl.ItemsSource = new List<ReservationView>();
                }
            }
        }

        private void PickUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null || button.Tag == null)
                {
                    MessageBox.Show("Invalid reservation.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(button.Tag.ToString(), out var reservationId))
                {
                    MessageBox.Show("Invalid reservation ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MainWindow.CurrentStudent == null)
                {
                    MessageBox.Show("Please log in to pick up reservations.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Fetch reservation to verify status and student
                var reservation = DatabaseHelper.GetReservationsByStudent(MainWindow.CurrentStudent.Id).FirstOrDefault(r => r.Id == reservationId);
                if (reservation == null)
                {
                    MessageBox.Show("Reservation not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (reservation.Status != "Ready")
                {
                    MessageBox.Show("This reservation is not ready for pickup yet.", "Not Available",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (DatabaseHelper.PickUpReservation(reservationId))
                {
                    MessageBox.Show("Reservation picked up successfully! The book has been issued to you.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    LoadReservations(); // Refresh the list
                }
                else
                {
                    MessageBox.Show("Failed to pick up reservation. Please try again.", "Error",
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

        private void CancelReservationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null || button.Tag == null)
                {
                    MessageBox.Show("Invalid reservation.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MainWindow.CurrentStudent == null)
                {
                    MessageBox.Show("Please log in to cancel reservations.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to cancel this reservation?",
                    "Cancel Reservation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int reservationId = Convert.ToInt32(button.Tag);
                    if (DatabaseHelper.CancelReservation(reservationId))
                    {
                        MessageBox.Show("Reservation cancelled successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        LoadReservations(); // Refresh the list
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel reservation. Please try again.", "Error",
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

    public class ReservationView
    {
        public int ReservationId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public string ReservedDate { get; set; } = string.Empty;
        public string HoldUntil { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public SolidColorBrush StatusColor { get; set; } = new SolidColorBrush(Colors.Gray);
        public bool CanPickUp { get; set; }
    }
}
