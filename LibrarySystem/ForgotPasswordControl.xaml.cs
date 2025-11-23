using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using LibrarySystem.Models;

namespace LibrarySystem
{
    public partial class ForgotPasswordControl : UserControl
    {
        public ForgotPasswordControl()
        {
            InitializeComponent();
        }

        private void RequestCodeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var email = ResetEmailTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please enter your registered email.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var student = DatabaseHelper.GetStudentByEmail(email);
                if (student == null)
                {
                    MessageBox.Show("No account found with the provided email.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Generate simple 6-digit code
                var code = new Random().Next(100000, 999999).ToString();
                var expires = DateTime.Now.AddMinutes(15);

                if (DatabaseHelper.CreatePasswordReset(student.Id, code, expires))
                {
                    // Attempt to send email
                    var subject = "Library System Password Reset Code";
                    var body = $"Your password reset code is: {code}\nThis code expires at {expires}.";
                    var sent = EmailHelper.TrySendEmail(student.Email, subject, body, out var error);

                    if (sent)
                    {
                        MessageBox.Show("A reset code has been sent to your email address.", "Reset Code Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Email not sent — fallback show and copy
                        try { Clipboard.SetText(code); } catch { }
                        try
                        {
                            var mailto = $"mailto:{student.Email}?subject=Password%20Reset%20Code&body=Your%20reset%20code%20is%20{code}%20(Expires%20at%20{Uri.EscapeDataString(expires.ToString())})";
                            Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });
                        }
                        catch { }

                        MessageBox.Show($"Unable to send email automatically: {error}\n\nFor demo the code has been copied to clipboard and a mail client was opened (if available).\nCode: {code}\nExpires at: {expires}", "Reset Code", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to create reset code. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Confirm that an email is entered and show a confirmation
            var email = ResetEmailTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your registered email before confirming.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"A reset code will be sent to: {email}. If you don't receive it, check your spam folder.", "Confirm Email", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var email = ResetEmailTextBox.Text?.Trim();
                var code = ResetCodeTextBox.Text?.Trim();
                var newPass = NewPasswordBox.Password;
                var confirm = ConfirmPasswordBox.Password;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Please enter your registered email and the reset code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 6)
                {
                    MessageBox.Show("Please enter a new password with at least 6 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPass != confirm)
                {
                    MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var student = DatabaseHelper.GetStudentByEmail(email);
                if (student == null)
                {
                    MessageBox.Show("No account found with the provided email.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var reset = DatabaseHelper.GetPasswordResetByStudentAndCode(student.Id, code);
                if (reset == null)
                {
                    MessageBox.Show("Invalid or expired reset code.", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (reset.Value.ExpiresAt < DateTime.Now)
                {
                    MessageBox.Show("Reset code has expired.", "Expired", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DatabaseHelper.DeletePasswordReset(reset.Value.Id);
                    return;
                }

                if (DatabaseHelper.UpdateStudentPassword(student.Id, newPass))
                {
                    MessageBox.Show("Password reset successful. You can now log in with your new password.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DatabaseHelper.DeletePasswordReset(reset.Value.Id);
                }
                else
                {
                    MessageBox.Show("Failed to reset password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
