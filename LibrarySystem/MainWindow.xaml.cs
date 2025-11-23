using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class MainWindow : Window
    {
        private bool isSignUp = false;
        public static Student? CurrentStudent { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (LoginPasswordBox.Visibility == Visibility.Visible)
            {
                LoginPasswordText.Text = LoginPasswordBox.Password;
                LoginPasswordBox.Visibility = Visibility.Collapsed;
                LoginPasswordText.Visibility = Visibility.Visible;
            }
            else
            {
                LoginPasswordBox.Password = LoginPasswordText.Text;
                LoginPasswordBox.Visibility = Visibility.Visible;
                LoginPasswordText.Visibility = Visibility.Collapsed;
            }
        }

        private void SignUpTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (SignUpPasswordBox.Visibility == Visibility.Visible)
            {
                SignUpPasswordText.Text = SignUpPasswordBox.Password;
                SignUpPasswordBox.Visibility = Visibility.Collapsed;
                SignUpPasswordText.Visibility = Visibility.Visible;
            }
            else
            {
                SignUpPasswordBox.Password = SignUpPasswordText.Text;
                SignUpPasswordBox.Visibility = Visibility.Visible;
                SignUpPasswordText.Visibility = Visibility.Collapsed;
            }
        }

        private void SignUp_Click(object sender, MouseButtonEventArgs e)
        {
            if (!isSignUp)
            {
                Storyboard sb = (Storyboard)this.Resources["SwapToSignUp"];
                sb.Begin();
                LoginForm.Visibility = Visibility.Collapsed;
                SignUpForm.Visibility = Visibility.Visible;
                isSignUp = true;
            }
        }

        private void Login_Click(object sender, MouseButtonEventArgs e)
        {
            if (isSignUp)
            {
                Storyboard sb = (Storyboard)this.Resources["SwapToLogin"];
                sb.Begin();
                SignUpForm.Visibility = Visibility.Collapsed;
                LoginForm.Visibility = Visibility.Visible;
                isSignUp = false;
            }

        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            // Per request, show message instructing user to contact librarian
            MessageBox.Show("Please contact the library staff or librarian to reset your password.", "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoginButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string emailOrId = LoginEmailTextBox.Text.Trim();
                string password = LoginPasswordBox.Visibility == Visibility.Visible
                    ? LoginPasswordBox.Password
                    : LoginPasswordText.Text;

                if (string.IsNullOrWhiteSpace(emailOrId) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both email/ID and password.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Try student login first
                var student = DatabaseHelper.GetStudentByEmailOrId(emailOrId);
                if (student != null && student.Password == password && student.IsActive)
                {
                    CurrentStudent = student;
                    StudentPortal studentportal = new StudentPortal();
                    studentportal.WindowState = WindowState.Maximized;
                    studentportal.WindowStyle = WindowStyle.SingleBorderWindow;
                    studentportal.ResizeMode = ResizeMode.CanMinimize;
                    studentportal.Show();
                    this.Close();
                    return;
                }

                // Try admin login
                var admin = DatabaseHelper.GetAdminByEmail(emailOrId);
                if (admin != null && admin.Password == password)
                {
                    AdminPage adminPage = new AdminPage();
                    adminPage.WindowState = WindowState.Maximized;
                    adminPage.WindowStyle = WindowStyle.SingleBorderWindow;
                    adminPage.ResizeMode = ResizeMode.CanMinimize;
                    adminPage.Show();
                    this.Close();
                    return;
                }

                // If neither student nor admin login succeeded
                MessageBox.Show("Invalid email/ID or password. Please try again.", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            string fullName = SignUpFullNameTextBox.Text.Trim();
            string studentId = SignUpStudentIdTextBox.Text.Trim();
            string email = SignUpEmailTextBox.Text.Trim();
            string course = SignUpCourseTextBox.Text.Trim();
            string section = SignUpSectionTextBox.Text.Trim();
            string password = SignUpPasswordBox.Visibility == Visibility.Visible
                ? SignUpPasswordBox.Password
                : SignUpPasswordText.Text;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(studentId) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(course) ||
                string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if student already exists
            var existingStudent = DatabaseHelper.GetStudentByEmailOrId(email);
            if (existingStudent != null)
            {
                MessageBox.Show("A student with this email or ID already exists.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create new student
            var newStudent = new Student
            {
                StudentId = studentId,
                FullName = fullName,
                Email = email,
                Password = password,
                Course = course,
                Section = section,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            if (DatabaseHelper.AddStudent(newStudent))
            {
                // Get the newly created student to set as current
                CurrentStudent = DatabaseHelper.GetStudentByEmailOrId(email);

                MessageBox.Show("Account created successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                StudentPortal studentportal = new StudentPortal();
                studentportal.WindowState = WindowState.Maximized;
                studentportal.WindowStyle = WindowStyle.SingleBorderWindow;
                studentportal.ResizeMode = ResizeMode.CanMinimize;
                studentportal.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to create account. Please try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
