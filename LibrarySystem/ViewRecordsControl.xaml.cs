using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibrarySystem.Models;
using Microsoft.Win32;

namespace LibrarySystem
{
    public partial class ViewRecordsControl : UserControl
    {
        public ViewRecordsControl()
        {
            InitializeComponent();
            this.Loaded += ViewRecordsControl_Loaded;
            DatabaseHelper.DataChanged += OnDataChanged;
        }

        private void ViewRecordsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (RecordsDataGrid != null)
            {
                LoadRecords();
            }
        }

        private void OnDataChanged()
        {
            Dispatcher.Invoke(() => LoadRecords());
        }

        private void LoadRecords()
        {
            try
            {
                if (RecordsDataGrid == null) return;

                var records = DatabaseHelper.GetAllBorrowRecords();
                var recordViews = records.Select(record => new RecordView
                {
                    Id = record.Id,
                    StudentId = DatabaseHelper.GetStudentById(record.StudentId)?.StudentId ?? record.StudentId.ToString(),
                    BookTitle = DatabaseHelper.GetBookById(record.BookId)?.Title ?? "Unknown",
                    IssueDate = record.IssueDate,
                    DueDate = record.DueDate,
                    ReturnDate = record.ReturnDate,
                    Status = record.Status
                }).ToList();

                RecordsDataGrid.ItemsSource = recordViews;
            }
            catch
            {
                // Silently fail if control is not initialized
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            try
            {
                if (RecordsDataGrid == null) return;

                var all = DatabaseHelper.GetAllBorrowRecords().AsEnumerable();

                // Type filter
                var filterType = (FilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Records";
                if (filterType == "Issue Records") all = all.Where(r => r.Status == "Issued");
                else if (filterType == "Return Records") all = all.Where(r => r.Status == "Returned");

                // Date filter
                if (FilterDatePicker?.SelectedDate != null)
                {
                    var d = FilterDatePicker.SelectedDate.Value.Date;
                    all = all.Where(r => r.IssueDate.Date == d);
                }

                var recordViews = all.Select(record => new RecordView
                {
                    Id = record.Id,
                    StudentId = DatabaseHelper.GetStudentById(record.StudentId)?.StudentId ?? record.StudentId.ToString(),
                    BookTitle = DatabaseHelper.GetBookById(record.BookId)?.Title ?? "Unknown",
                    IssueDate = record.IssueDate,
                    DueDate = record.DueDate,
                    ReturnDate = record.ReturnDate,
                    Status = record.Status
                }).ToList();

                RecordsDataGrid.ItemsSource = recordViews;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RecordsDataGrid == null || RecordsDataGrid.ItemsSource == null) return;

                var items = RecordsDataGrid.ItemsSource.Cast<RecordView>().ToList();
                if (items.Count == 0)
                {
                    MessageBox.Show("No records to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = "records_export.csv" };
                if (dlg.ShowDialog() != true) return;

                using (var sw = new StreamWriter(dlg.FileName))
                {
                    sw.WriteLine("Id,StudentId,BookTitle,IssueDate,DueDate,ReturnDate,Status");
                    foreach (var row in items)
                    {
                        var returnDate = row.ReturnDate.HasValue ? row.ReturnDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                        var line = $"{row.Id},\"{row.StudentId}\",\"{row.BookTitle}\",{row.IssueDate:yyyy-MM-dd},{row.DueDate:yyyy-MM-dd},{returnDate},\"{row.Status}\"";
                        sw.WriteLine(line);
                    }
                }

                MessageBox.Show("Export successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FilterComboBox != null) FilterComboBox.SelectedIndex = 0;
                if (FilterDatePicker != null) FilterDatePicker.SelectedDate = null;

                LoadRecords();
            }
            catch { }
        }
    }

    public class RecordView
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
