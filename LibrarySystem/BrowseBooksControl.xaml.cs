using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    /// <summary>
    /// Interaction logic for BrowseBooksControl.xaml
    /// </summary>
    public partial class BrowseBooksControl : UserControl
    {
        public BrowseBooksControl()
        {
            InitializeComponent();
            this.Loaded += BrowseBooksControl_Loaded;
        }

        private void BrowseBooksControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure controls initialized
            if (BooksItemsControl != null)
            {
                LoadBooks();
                PopulateCategories();
            }
        }

        private void PopulateCategories()
        {
            try
            {
                if (CategoryComboBox == null) return;

                var books = DatabaseHelper.GetAllBooks();
                var cats = books.Select(b => b.Category)
                                .Where(c => !string.IsNullOrWhiteSpace(c))
                                .Distinct()
                                .OrderBy(c => c)
                                .ToList();

                CategoryComboBox.Items.Clear();
                CategoryComboBox.Items.Add(new ComboBoxItem { Content = "All Categories", IsSelected = true });
                foreach (var c in cats)
                {
                    CategoryComboBox.Items.Add(new ComboBoxItem { Content = c });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PopulateCategories error: {ex.Message}");
            }
        }

        private void LoadBooks()
        {
            try
            {
                if (BooksItemsControl == null) return;
                var books = DatabaseHelper.GetAllBooks();
                BooksItemsControl.ItemsSource = books;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBooks error: {ex.Message}");
                if (BooksItemsControl != null) BooksItemsControl.ItemsSource = new List<Book>();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBooks();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterBooks();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = BooksItemsControl.ItemsSource as IEnumerable<Book> ?? DatabaseHelper.GetAllBooks();
                var sorted = items.OrderBy(b => b.Title).ToList();
                BooksItemsControl.ItemsSource = sorted;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SortButton_Click error: {ex.Message}");
            }
        }

        private void FilterBooks()
        {
            try
            {
                if (BooksItemsControl == null) return;

                var all = DatabaseHelper.GetAllBooks();
                var search = (SearchTextBox?.Text ?? string.Empty).Trim().ToLower();

                string category = null;
                if (CategoryComboBox?.SelectedItem is ComboBoxItem cbi)
                {
                    category = cbi.Content?.ToString();
                    if (string.Equals(category, "All Categories", StringComparison.OrdinalIgnoreCase)) category = null;
                }

                var filtered = all.Where(b =>
                    (string.IsNullOrWhiteSpace(search) ||
                        (b.Title ?? string.Empty).ToLower().Contains(search) ||
                        (b.Author ?? string.Empty).ToLower().Contains(search) ||
                        (b.ISBN ?? string.Empty).ToLower().Contains(search))
                    && (string.IsNullOrWhiteSpace(category) || ((b.Category ?? string.Empty).Equals(category, StringComparison.OrdinalIgnoreCase)))
                ).ToList();

                BooksItemsControl.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FilterBooks error: {ex.Message}");
            }
        }

        private void ManageBooksButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Window
                {
                    Title = "Manage Books",
                    Width = 900,
                    Height = 700,
                    Owner = Window.GetWindow(this),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                var ctrl = new ManageBooksControl();
                win.Content = ctrl;
                win.ShowDialog();

                // After closing, refresh
                LoadBooks();
                PopulateCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Manage Books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Window
                {
                    Title = "Add New Book",
                    Width = 700,
                    Height = 520,
                    Owner = Window.GetWindow(this),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    Content = new AddBookControl()
                };

                if (win.Content is AddBookControl ctrl)
                {
                    ctrl.BookAdded += (s, ev) =>
                    {
                        LoadBooks();
                        PopulateCategories();
                    };
                }

                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Book window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || btn.Tag == null) return;
                if (!int.TryParse(btn.Tag.ToString(), out var bookId)) return;

                var book = DatabaseHelper.GetBookById(bookId);
                if (book == null)
                {
                    MessageBox.Show("Book not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var detailsWin = new Window
                {
                    Title = book.Title,
                    Width = 520,
                    Height = 380,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this),
                    ResizeMode = ResizeMode.NoResize
                };

                var sp = new StackPanel { Margin = new Thickness(16) };
                sp.Children.Add(new TextBlock { Text = book.Title, FontSize = 18, FontWeight = FontWeights.Bold });
                sp.Children.Add(new TextBlock { Text = $"Author: {book.Author}", Margin = new Thickness(0,8,0,0) });
                sp.Children.Add(new TextBlock { Text = $"Category: {book.Category}", Margin = new Thickness(0,4,0,0) });
                sp.Children.Add(new TextBlock { Text = $"Year Published: {book.YearPublished}", Margin = new Thickness(0,4,0,0) });
                sp.Children.Add(new TextBlock { Text = $"ISBN: {book.ISBN}", Margin = new Thickness(0,4,0,0) });
                sp.Children.Add(new TextBlock { Text = $"Available: {book.AvailableCopies}" , Margin = new Thickness(0,4,0,0)});
                sp.Children.Add(new TextBlock { Text = "", Margin = new Thickness(0,8,0,0) });
                sp.Children.Add(new TextBlock { Text = book.Description, TextWrapping = TextWrapping.Wrap });

                detailsWin.Content = sp;
                detailsWin.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to show details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || btn.Tag == null) return;
                if (!int.TryParse(btn.Tag.ToString(), out var bookId)) return;

                if (MainWindow.CurrentStudent == null)
                {
                    MessageBox.Show("Please log in to reserve books.", "Not Logged In", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var book = DatabaseHelper.GetBookById(bookId);
                if (book == null)
                {
                    MessageBox.Show("Book not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Prevent duplicate active reservation
                if (DatabaseHelper.ReservationExists(MainWindow.CurrentStudent.Id, bookId))
                {
                    MessageBox.Show("You already have an active reservation for this book.", "Duplicate Reservation", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (DatabaseHelper.CreateReservation(MainWindow.CurrentStudent.Id, bookId))
                {
                    MessageBox.Show("Reservation created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // navigate to MyReservations
                    var parentWin = Window.GetWindow(this);
                    if (parentWin is StudentPortal studentPortal)
                    {
                        studentPortal.ContentArea1.Content = new MyReservationsControl();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to create reservation. It may already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reserve book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
