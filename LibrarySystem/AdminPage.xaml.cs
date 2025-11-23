using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibrarySystem
{
    /// <summary>
    /// Interaction logic for AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Window
    {
        private bool isSidebarCollapsed = false;

        public AdminPage()
        {
            InitializeComponent();

            // Load Dashboard by default
            ContentArea.Content = new DashboardControl();
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSidebarCollapsed)
            {
                // COLLAPSE 
                SidebarColumn.Width = new GridLength(50);

                // Hide text elements
                SidebarTitle.Visibility = Visibility.Collapsed;
                LogoSection.Visibility = Visibility.Collapsed; // Hide QCU Logo Section

                // Hide text in menu buttons (keep only icons visible)
                foreach (var child in SidebarStackPanel.Children)
                {
                    if (child is Button btn && btn.Content is StackPanel sp)
                    {
                        foreach (var item in sp.Children)
                        {
                            if (item is TextBlock tb &&
                                tb.Text != "📊" &&
                                tb.Text != "📚" &&
                                tb.Text != "👤" &&
                                tb.Text != "📖" &&
                                tb.Text != "🔄" &&
                                tb.Text != "📋" &&
                                tb.Text != "📕" &&
                                tb.Text != "👥" &&
                                tb.Text != "▐➜")
                            {
                                tb.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }

                isSidebarCollapsed = true;
            }
            else
            {
                // EXPAND - Make sidebar wide (250px width)
                SidebarColumn.Width = new GridLength(250);

                // Show text elements
                SidebarTitle.Visibility = Visibility.Visible;
                LogoSection.Visibility = Visibility.Visible; // Show QCU Logo Section

                // Show text in menu buttons
                foreach (var child in SidebarStackPanel.Children)
                {
                    if (child is Button btn && btn.Content is StackPanel sp)
                    {
                        foreach (var item in sp.Children)
                        {
                            if (item is TextBlock tb)
                            {
                                tb.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }

                isSidebarCollapsed = false;
            }
        }

        // Navigation Methods
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new DashboardControl();
        }

        private void ManageBooks_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ManageBooksControl();
        }

        private void ManageStudents_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ManageStudentsControl();
        }

        private void ResetStudentsPassword_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ResetStudentPasswordControl();
        }

        private void IssueBook_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new IssueBookControl();
        }

        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ReturnBookControl();
        }

        private void ViewRecords_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ViewRecordsControl();
        }

        private void ViewIssuedBooks_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ViewIssuedBooksControl();
        }

        private void DefaulterList_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new DefaulterListControl();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click
            MessageBox.Show("Button clicked!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}