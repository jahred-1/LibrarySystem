using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class MyProfileControl : UserControl
    {
        public MyProfileControl()
        {
            InitializeComponent();
            this.Loaded += MyProfileControl_Loaded;
        }

        private void MyProfileControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (StudentNameText != null)
            {
                LoadProfileData();
            }
        }

        private void LoadProfileData()
        {
            try
            {
                if (MainWindow.CurrentStudent == null)
                {
                    return;
                }

                var student = MainWindow.CurrentStudent;
                var records = DatabaseHelper.GetBorrowRecordsByStudent(student.Id);
                var totalBorrowed = records.Count;
                var currentlyBorrowed = records.Count(r => r.Status == "Issued");
                var onTimeReturns = records.Count(r => r.Status == "Returned" && r.ReturnDate.HasValue && r.ReturnDate <= r.DueDate);
                var onTimePercentage = totalBorrowed > 0 ? (onTimeReturns * 100.0 / totalBorrowed) : 0;

                // Calculate this month and year activity
                var now = DateTime.Now;
                var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                var thisYearStart = new DateTime(now.Year, 1, 1);

                var thisMonthCount = records.Count(r => r.IssueDate >= thisMonthStart);
                var thisYearCount = records.Count(r => r.IssueDate >= thisYearStart);

                // Calculate favorite genre
                var books = records.Select(r => DatabaseHelper.GetBookById(r.BookId))
                    .Where(b => b != null && !string.IsNullOrEmpty(b.Category))
                    .GroupBy(b => b.Category)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                var favoriteGenre = books?.Key ?? "N/A";

                // Update profile information
                if (StudentNameText != null) StudentNameText.Text = student.FullName;
                if (StudentIdText != null) StudentIdText.Text = $"Student ID: {student.StudentId}";
                if (EmailText != null) EmailText.Text = student.Email ?? "N/A";
                if (CourseText != null) CourseText.Text = student.Course ?? "N/A";
                if (SectionText != null) SectionText.Text = student.Section ?? "N/A";
                if (TotalBorrowedText != null) TotalBorrowedText.Text = totalBorrowed.ToString();
                if (CurrentlyBorrowedProfileText != null) CurrentlyBorrowedProfileText.Text = currentlyBorrowed.ToString();
                if (OnTimeReturnsText != null) OnTimeReturnsText.Text = $"{onTimePercentage:F0}%";

                // Update membership details
                if (MemberSinceText != null)
                {
                    MemberSinceText.Text = student.CreatedAt.ToString("MMMM d, yyyy");
                }

                if (MembershipStatusText != null)
                {
                    var status = student.IsActive ? "Active - Good Standing" : "Inactive";
                    MembershipStatusText.Text = status;
                    MembershipStatusText.Foreground = student.IsActive
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                }

                if (BorrowingLimitText != null)
                {
                    BorrowingLimitText.Text = $"{currentlyBorrowed} of 10 books";
                }

                // Update reading activity
                if (ThisMonthText != null)
                {
                    ThisMonthText.Text = $"{thisMonthCount} book{(thisMonthCount != 1 ? "s" : "")}";
                }

                if (ThisMonthProgress != null)
                {
                    // Progress based on a goal of 10 books per month
                    ThisMonthProgress.Value = Math.Min((thisMonthCount / 10.0) * 100, 100);
                }

                if (ThisYearText != null)
                {
                    ThisYearText.Text = $"{thisYearCount} book{(thisYearCount != 1 ? "s" : "")}";
                }

                if (ThisYearProgress != null)
                {
                    // Progress based on a goal of 50 books per year
                    ThisYearProgress.Value = Math.Min((thisYearCount / 50.0) * 100, 100);
                }

                if (FavoriteGenreText != null)
                {
                    FavoriteGenreText.Text = favoriteGenre;
                }

                // Update initials
                if (InitialsText != null)
                {
                    var nameParts = student.FullName?.Split(' ') ?? new string[0];
                    if (nameParts.Length >= 2)
                    {
                        InitialsText.Text = $"{nameParts[0][0]}{nameParts[nameParts.Length - 1][0]}".ToUpper();
                    }
                    else if (nameParts.Length == 1 && nameParts[0].Length >= 2)
                    {
                        InitialsText.Text = nameParts[0].Substring(0, 2).ToUpper();
                    }
                    else
                    {
                        InitialsText.Text = "??";
                    }
                }
            }
            catch (Exception ex)
            {
                // Error loading profile data - log but don't crash
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.CurrentStudent == null)
                {
                    MessageBox.Show("Please log in to edit your profile.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var student = MainWindow.CurrentStudent;
                var parentWindow = Window.GetWindow(this);

                var editWindow = new Window
                {
                    Title = "Edit Profile",
                    Width = 500,
                    Height = 400,
                    WindowStartupLocation = parentWindow != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen,
                    Owner = parentWindow,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                var fullNameLabel = new TextBlock { Text = "Full Name:", Margin = new Thickness(0, 0, 0, 5) };
                var fullNameTextBox = new TextBox
                {
                    Text = student.FullName,
                    Margin = new Thickness(0, 0, 0, 15),
                    Height = 30
                };

                var emailLabel = new TextBlock { Text = "Email:", Margin = new Thickness(0, 0, 0, 5) };
                var emailTextBox = new TextBox
                {
                    Text = student.Email,
                    Margin = new Thickness(0, 0, 0, 15),
                    Height = 30
                };

                var courseLabel = new TextBlock { Text = "Course:", Margin = new Thickness(0, 0, 0, 5) };
                var courseTextBox = new TextBox
                {
                    Text = student.Course,
                    Margin = new Thickness(0, 0, 0, 15),
                    Height = 30
                };

                var sectionLabel = new TextBlock { Text = "Section:", Margin = new Thickness(0, 0, 0, 5) };
                var sectionTextBox = new TextBox
                {
                    Text = student.Section,
                    Margin = new Thickness(0, 0, 0, 20),
                    Height = 30
                };

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var saveButton = new Button
                {
                    Content = "Save",
                    Width = 80,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = Brushes.DodgerBlue,
                    Foreground = Brushes.White
                };
                saveButton.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(fullNameTextBox.Text))
                    {
                        MessageBox.Show("Please enter your full name.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(emailTextBox.Text))
                    {
                        MessageBox.Show("Please enter your email.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    student.FullName = fullNameTextBox.Text.Trim();
                    student.Email = emailTextBox.Text.Trim();
                    student.Course = courseTextBox.Text.Trim();
                    student.Section = sectionTextBox.Text.Trim();

                    if (DatabaseHelper.UpdateStudent(student))
                    {
                        MessageBox.Show("Profile updated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadProfileData(); // Refresh profile data
                        editWindow.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update profile. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Height = 35,
                    Background = Brushes.LightGray
                };
                cancelButton.Click += (s, args) => editWindow.Close();

                buttonPanel.Children.Add(saveButton);
                buttonPanel.Children.Add(cancelButton);

                stackPanel.Children.Add(fullNameLabel);
                stackPanel.Children.Add(fullNameTextBox);
                stackPanel.Children.Add(emailLabel);
                stackPanel.Children.Add(emailTextBox);
                stackPanel.Children.Add(courseLabel);
                stackPanel.Children.Add(courseTextBox);
                stackPanel.Children.Add(sectionLabel);
                stackPanel.Children.Add(sectionTextBox);
                stackPanel.Children.Add(buttonPanel);

                editWindow.Content = stackPanel;
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parentWindow = Window.GetWindow(this);

                // Create a password change dialog with show/hide toggle
                var passwordWindow = new Window
                {
                    Title = "Change Password",
                    Width = 420,
                    Height = 340,
                    WindowStartupLocation = parentWindow != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen,
                    Owner = parentWindow,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                var currentPasswordLabel = new TextBlock
                {
                    Text = "Current Password:",
                    Margin = new Thickness(0, 0, 0, 5)
                };
                var currentPasswordBox = new PasswordBox
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    Height = 30
                };

                var newPasswordLabel = new TextBlock
                {
                    Text = "New Password:",
                    Margin = new Thickness(0, 0, 0, 5)
                };
                var newPasswordBox = new PasswordBox
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    Height = 30
                };

                var confirmPasswordLabel = new TextBlock
                {
                    Text = "Confirm New Password:",
                    Margin = new Thickness(0, 0, 0, 5)
                };
                var confirmPasswordBox = new PasswordBox
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    Height = 30
                };

                // Show/Hide checkbox
                var showPasswordCheck = new CheckBox { Content = "Show passwords", Margin = new Thickness(0, 0, 0, 15) };

                // Create textboxes to mirror password content when shown
                var currentPasswordText = new TextBox { Visibility = Visibility.Collapsed, Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                var newPasswordText = new TextBox { Visibility = Visibility.Collapsed, Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                var confirmPasswordText = new TextBox { Visibility = Visibility.Collapsed, Height = 30, Margin = new Thickness(0, 0, 0, 10) };

                // Toggle show/hide behavior
                showPasswordCheck.Checked += (s, ev) =>
                {
                    currentPasswordText.Text = currentPasswordBox.Password;
                    newPasswordText.Text = newPasswordBox.Password;
                    confirmPasswordText.Text = confirmPasswordBox.Password;

                    currentPasswordText.Visibility = Visibility.Visible;
                    newPasswordText.Visibility = Visibility.Visible;
                    confirmPasswordText.Visibility = Visibility.Visible;

                    currentPasswordBox.Visibility = Visibility.Collapsed;
                    newPasswordBox.Visibility = Visibility.Collapsed;
                    confirmPasswordBox.Visibility = Visibility.Collapsed;
                };
                showPasswordCheck.Unchecked += (s, ev) =>
                {
                    currentPasswordBox.Password = currentPasswordText.Text;
                    newPasswordBox.Password = newPasswordText.Text;
                    confirmPasswordBox.Password = confirmPasswordText.Text;

                    currentPasswordText.Visibility = Visibility.Collapsed;
                    newPasswordText.Visibility = Visibility.Collapsed;
                    confirmPasswordText.Visibility = Visibility.Collapsed;

                    currentPasswordBox.Visibility = Visibility.Visible;
                    newPasswordBox.Visibility = Visibility.Visible;
                    confirmPasswordBox.Visibility = Visibility.Visible;
                };

                // Keep mirrored content in sync
                currentPasswordBox.PasswordChanged += (s, ev) => { if (currentPasswordText.Visibility == Visibility.Visible) currentPasswordText.Text = currentPasswordBox.Password; };
                newPasswordBox.PasswordChanged += (s, ev) => { if (newPasswordText.Visibility == Visibility.Visible) newPasswordText.Text = newPasswordBox.Password; };
                confirmPasswordBox.PasswordChanged += (s, ev) => { if (confirmPasswordText.Visibility == Visibility.Visible) confirmPasswordText.Text = confirmPasswordBox.Password; };

                currentPasswordText.TextChanged += (s, ev) => { if (currentPasswordBox.Visibility == Visibility.Visible) currentPasswordBox.Password = currentPasswordText.Text; };
                newPasswordText.TextChanged += (s, ev) => { if (newPasswordBox.Visibility == Visibility.Visible) newPasswordBox.Password = newPasswordText.Text; };
                confirmPasswordText.TextChanged += (s, ev) => { if (confirmPasswordBox.Visibility == Visibility.Visible) confirmPasswordBox.Password = confirmPasswordText.Text; };

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var saveButton = new Button
                {
                    Content = "Change Password",
                    Width = 140,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = Brushes.DodgerBlue,
                    Foreground = Brushes.White
                };
                saveButton.Click += (s, args) =>
                {
                    var current = currentPasswordBox.Visibility == Visibility.Visible ? currentPasswordBox.Password : currentPasswordText.Text;
                    var np = newPasswordBox.Visibility == Visibility.Visible ? newPasswordBox.Password : newPasswordText.Text;
                    var cp = confirmPasswordBox.Visibility == Visibility.Visible ? confirmPasswordBox.Password : confirmPasswordText.Text;

                    if (string.IsNullOrWhiteSpace(current))
                    {
                        MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(np) || np.Length < 6)
                    {
                        MessageBox.Show("Please enter a new password with at least 6 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (np != cp)
                    {
                        MessageBox.Show("New passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (MainWindow.CurrentStudent != null)
                    {
                        if (MainWindow.CurrentStudent.Password != current)
                        {
                            MessageBox.Show("Current password is incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (DatabaseHelper.UpdateStudentPassword(MainWindow.CurrentStudent.Id, np))
                        {
                            MainWindow.CurrentStudent.Password = np;
                            MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            passwordWindow.Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Height = 35,
                    Background = Brushes.LightGray
                };
                cancelButton.Click += (s, args) => passwordWindow.Close();

                buttonPanel.Children.Add(saveButton);
                buttonPanel.Children.Add(cancelButton);

                stackPanel.Children.Add(currentPasswordLabel);
                stackPanel.Children.Add(currentPasswordBox);
                stackPanel.Children.Add(currentPasswordText);
                stackPanel.Children.Add(newPasswordLabel);
                stackPanel.Children.Add(newPasswordBox);
                stackPanel.Children.Add(newPasswordText);
                stackPanel.Children.Add(confirmPasswordLabel);
                stackPanel.Children.Add(confirmPasswordBox);
                stackPanel.Children.Add(confirmPasswordText);
                stackPanel.Children.Add(showPasswordCheck);
                stackPanel.Children.Add(buttonPanel);

                passwordWindow.Content = stackPanel;
                passwordWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NotificationPreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Notification Preferences:\n\n- Email notifications for due dates\n- Email notifications for reservation ready\n- SMS notifications (not implemented)", "Notification Preferences", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrivacySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Privacy Settings:\n\n- Make profile private\n- Share activity with library\n\n(These settings are informational in the demo.)", "Privacy Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
