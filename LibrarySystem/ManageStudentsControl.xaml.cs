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
            LoadStudents();
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, students can sign up through the main window
            MessageBox.Show("Students can register through the sign-up page on the login screen.", 
                "Add Student", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
