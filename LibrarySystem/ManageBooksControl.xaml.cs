using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class ManageBooksControl : UserControl
    {
        private string placeholderText = "Search books by title, author, or ISBN...";

        public ManageBooksControl()
        {
            InitializeComponent();
            this.Loaded += ManageBooksControl_Loaded;
        }

        private void ManageBooksControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid != null)
            {
                LoadBooks();
                BooksDataGrid.MouseDoubleClick += BooksDataGrid_MouseDoubleClick;
            }
        }

        private void BooksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book book)
            {
                ShowBookDetails(book);
            }
        }

        private void ShowBookDetails(Book book)
        {
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
            sp.Children.Add(new TextBlock { Text = $"Author: {book.Author}", Margin = new Thickness(0, 8, 0, 0) });
            sp.Children.Add(new TextBlock { Text = $"Category: {book.Category}", Margin = new Thickness(0, 4, 0, 0) });
            sp.Children.Add(new TextBlock { Text = $"Year Published: {book.YearPublished}", Margin = new Thickness(0, 4, 0, 0) });
            sp.Children.Add(new TextBlock { Text = $"ISBN: {book.ISBN}", Margin = new Thickness(0, 4, 0, 0) });
            sp.Children.Add(new TextBlock { Text = $"Available: {book.AvailableCopies}", Margin = new Thickness(0, 4, 0, 0) });
            sp.Children.Add(new TextBlock { Text = "", Margin = new Thickness(0, 8, 0, 0) });
            sp.Children.Add(new TextBlock { Text = book.Description, TextWrapping = TextWrapping.Wrap });

            detailsWin.Content = sp;
            detailsWin.ShowDialog();
        }

        private void LoadBooks()
        {
            try
            {
                if (BooksDataGrid == null) return;

                var books = DatabaseHelper.GetAllBooks();
                BooksDataGrid.ItemsSource = books;
            }
            catch
            {
                // Silently fail if control is not initialized
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null && SearchTextBox.Text == placeholderText)
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null && string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = placeholderText;
                SearchTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"));
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchTextBox == null || BooksDataGrid == null) return;

                var searchText = SearchTextBox.Text?.ToLower() ?? "";

                if (string.IsNullOrWhiteSpace(searchText) || searchText == placeholderText)
                {
                    LoadBooks();
                    return;
                }

                var allBooks = DatabaseHelper.GetAllBooks();
                var filteredBooks = allBooks.Where(book =>
                    (book.Title ?? string.Empty).ToLower().Contains(searchText) ||
                    (book.Author ?? string.Empty).ToLower().Contains(searchText) ||
                    (book.ISBN ?? string.Empty).ToLower().Contains(searchText)
                ).ToList();

                BooksDataGrid.ItemsSource = filteredBooks;
            }
            catch
            {
                // Silently fail
            }
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
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
                    ctrl.BookAdded += (s, ev) => LoadBooks();
                }

                win.ShowDialog();

                // After dialog, refresh list
                LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}