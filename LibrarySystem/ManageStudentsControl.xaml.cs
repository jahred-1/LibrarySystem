using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class ManageStudentsControl : UserControl
    {
        public ManageStudentsControl()
        {
            InitializeComponent();
            this.Loaded += ManageStudentsControl_Loaded;
        }

        private void ManageStudentsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid != null)
            {
                LoadStudents();
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

        private void LoadStudents()
        {
            try
            {
                if (StudentsDataGrid == null) return;

                var students = DatabaseHelper.GetAllStudents();
                StudentsDataGrid.ItemsSource = students;
            }
            catch
            {
                // Silently fail if control is not initialized
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox != null)
            {
                SearchTextBox.Text = "Search by Student ID or Name...";
                SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999"));
            }
           
            LoadStudents();
        }

        private void ApplyFilters()
        {
            try
            {
                if (StudentsDataGrid == null) return;

                var all = DatabaseHelper.GetAllStudents().AsEnumerable();

                // Text search filter
                var searchText = SearchTextBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText) && searchText != "Search by Student ID or Name...")
                {
                    searchText = searchText.ToLower();
                    all = all.Where(s => 
                        s.StudentId.ToLower().Contains(searchText) || 
                        s.FullName.ToLower().Contains(searchText) ||
                        s.Email.ToLower().Contains(searchText));
                }

              

                StudentsDataGrid.ItemsSource = all.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
