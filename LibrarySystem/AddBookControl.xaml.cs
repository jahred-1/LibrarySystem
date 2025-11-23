using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class AddBookControl : UserControl
    {
        public event EventHandler? BookAdded;

        public AddBookControl()
        {
            InitializeComponent();
            this.Loaded += AddBookControl_Loaded;
        }

        private void AddBookControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CategoryComboBox.Items.Clear();
                CategoryComboBox.Items.Add("General");
                CategoryComboBox.Items.Add("College of Computer Studies");
                CategoryComboBox.Items.Add("College of Engineering");
                CategoryComboBox.Items.Add("College of Education");
                CategoryComboBox.Items.Add("College of Business & Accountancy");
                CategoryComboBox.SelectedIndex = 0;

                if (string.IsNullOrWhiteSpace(CopiesTextBox.Text)) CopiesTextBox.Text = "1";
            }
            catch
            {
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var title = TitleTextBox.Text?.Trim();
                var author = AuthorTextBox.Text?.Trim();
                var category = CategoryComboBox.Text?.Trim();
                var isbn = ISBNTextBox.Text?.Trim();
                var desc = DescriptionTextBox.Text?.Trim();

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
                {
                    MessageBox.Show("Please fill in Title, Author and ISBN.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existing = DatabaseHelper.GetAllBooks().FirstOrDefault(b => string.Equals(b.ISBN?.Trim(), isbn, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    MessageBox.Show($"ISBN '{isbn}' already exists for '{existing.Title}'.", "Duplicate ISBN", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int copies = 1;
                if (!int.TryParse(CopiesTextBox.Text, out copies) || copies < 1) copies = 1;

                int? year = null;
                if (int.TryParse(YearTextBox.Text, out var y) && y > 0) year = y;

                var book = new Book
                {
                    Title = title ?? string.Empty,
                    Author = author ?? string.Empty,
                    Category = string.IsNullOrWhiteSpace(category) ? "General" : category,
                    ISBN = isbn ?? string.Empty,
                    TotalCopies = copies,
                    AvailableCopies = copies,
                    YearPublished = year,
                    Description = desc ?? string.Empty,
                    AddedDate = DateTime.Now
                };

                if (DatabaseHelper.AddBook(book))
                {
                    MessageBox.Show("Book added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    BookAdded?.Invoke(this, EventArgs.Empty);

                    // clear fields
                    TitleTextBox.Text = "";
                    AuthorTextBox.Text = "";
                    ISBNTextBox.Text = "";
                    CopiesTextBox.Text = "1";
                    YearTextBox.Text = "";
                    DescriptionTextBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Failed to add book.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (parentWin != null) parentWin.Close();
        }
    }
}
