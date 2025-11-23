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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibrarySystem
{
    /// <summary>
    /// Interaction logic for StudentPortal.xaml
    /// </summary>
    public partial class StudentPortal : Window
    {
        private bool isSidebarCollapsed = false;
        public StudentPortal()
        {
            InitializeComponent();

            // Update student info
            if (MainWindow.CurrentStudent != null)
            {
                StudentNameText.Text = MainWindow.CurrentStudent.FullName;
                StudentIdText.Text = $"Student ID: {MainWindow.CurrentStudent.StudentId}";
            }

            // Load Dashboard by default
            ContentArea1.Content = new StudentDashboardControl();
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
                                tb.Text != "📖" &&
                                tb.Text != "📋" &&
                                tb.Text != "🔖" &&
                                tb.Text != "👤" &&
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
        private void StudentDashboard_Click(object sender, RoutedEventArgs e)
        {
            ContentArea1.Content = new StudentDashboardControl();
        }

        private void BrowseBooks_Click(object sender, RoutedEventArgs e)
        {
            ContentArea1.Content = new BrowseBooksControl();
        }

        private void MyBorrowedBooks_Click(object sender, RoutedEventArgs e)
        {
            ContentArea1.Content = new MyBorrowedBooksControl();
        }

        private void BorrowHistory_Click(object sender, RoutedEventArgs e)
        {
            ContentArea1.Content = new BorrowHistoryControl();
        }

        private void MyReservations_Click(object sender, RoutedEventArgs e)
        {
            ContentArea1.Content = new MyReservationsControl();
        }

        private void MyProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentArea1.Content = new MyProfileControl();
        }


        private void Logout2_Click(object sender, RoutedEventArgs e)
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
    }
    }
    
    
