using System;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class ResetStudentPasswordControl : UserControl
    {
        private Student? selectedStudent;

        public ResetStudentPasswordControl()
        {
            InitializeComponent();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            var query = SearchBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter email or student ID.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selectedStudent = DatabaseHelper.GetStudentByEmailOrId(query);
            if (selectedStudent == null)
            {
                StudentInfoText.Text = "Student not found";
                MessageBox.Show("Student not found.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StudentInfoText.Text = $"{selectedStudent.FullName} ({selectedStudent.StudentId}) - {selectedStudent.Email}";
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStudent == null)
            {
                MessageBox.Show("Please find the student first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var np = NewPasswordBox.Password;
            var cp = ConfirmPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(np) || np.Length < 6)
            {
                MessageBox.Show("Please enter a new password of at least 6 characters.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (np != cp)
            {
                MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DatabaseHelper.UpdateStudentPassword(selectedStudent.Id, np))
            {
                MessageBox.Show("Password updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Clear fields
                SearchBox.Text = string.Empty;
                StudentInfoText.Text = "Not selected";
                NewPasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
                selectedStudent = null;
            }
            else
            {
                MessageBox.Show("Failed to update password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}