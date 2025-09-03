using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VerisFlow.VenusDeckParser.Core;

namespace VerisFlow.VenusDeckParser.Desktop
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// A collection to hold the processed labware data for binding to the DataGrid.
        /// </summary>
        public ObservableCollection<ProcessedLabwareInfo> ProcessedLabwareData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ProcessedLabwareData = new ObservableCollection<ProcessedLabwareInfo>();
            // Set the DataContext for data binding
            this.DataContext = this;
        }

        private void SelectDeckLayout_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Deck Layout File",
                Filter = "Deck Layout Files (*.lay)|*.lay|All files (*.*)|*.*",
                DefaultExt = ".lay"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProcessDeckLayoutFile(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Asynchronously processes the deck layout file to prevent UI freezing.
        /// </summary>
        /// <param name="deckLayoutFile">The full path to the .lay file.</param>
        private async void ProcessDeckLayoutFile(string deckLayoutFile)
        {
            StatusTextBlock.Text = $"Processing: {Path.GetFileName(deckLayoutFile)}...";
            SelectLayoutButton.IsEnabled = false; // Disable button during processing

            try
            {
                string markdownFilePath = Path.ChangeExtension(deckLayoutFile, ".md");

                // Run all file/CPU-intensive operations on a background thread.
                var processedData = await Task.Run(() =>
                {
                    var raw = DeckLayoutParser.GetLabwareInfo(deckLayoutFile);
                    var processed = LabwareDataProcessor.Process(raw);
                    var markdown = GenerateMarkdown(deckLayoutFile, processed);

                    // Use the synchronous method INSIDE the background task.
                    File.WriteAllText(markdownFilePath, markdown);

                    return processed; // Only return the data needed by the UI thread.
                });

                // --- Update UI on the UI thread ---
                ProcessedLabwareData.Clear();
                foreach (var item in processedData)
                {
                    ProcessedLabwareData.Add(item);
                }

                StatusTextBlock.Text = $"Displayed {processedData.Count} items and saved report to {markdownFilePath}.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"An error occurred: {ex.Message}";
                MessageBox.Show($"Error processing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable the button once processing is complete.
                SelectLayoutButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Generates the detailed Markdown report with the final calculated labware information.
        /// </summary>
        private string GenerateMarkdown(string deckLayoutFile, List<ProcessedLabwareInfo> processedData)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# Deck Layout Report for {deckLayoutFile}");
            sb.AppendLine();
            sb.AppendLine($"**Generated on:** {DateTime.Now}");
            sb.AppendLine();
            sb.AppendLine("## Processed Labware Information");
            sb.AppendLine();

            if (processedData.Count > 0)
            {
                sb.AppendLine($"**Total Labware Instances:** {processedData.Count}");
                sb.AppendLine();

                // Markdown Table Header
                sb.AppendLine("| # | ID | Type | Template | X | Y | Dx | Dy | Column | Row | TipRack | AlphaIndex |");
                sb.AppendLine("|---|----|------|----------|---|---|----|----|--------|-----|---------|------------|");

                // Table Rows
                foreach (var labware in processedData)
                {
                    sb.AppendLine($"| {labware.Index} " +
                                  $"| `{labware.Id}` " +
                                  $"| {labware.LabwareType} " +
                                  $"| {labware.Template} " +
                                  $"| {labware.FinalX:F3} " +
                                  $"| {labware.FinalY:F3} " +
                                  $"| {labware.Dx:F3} " +
                                  $"| {labware.Dy:F3} " +
                                  $"| {labware.Column} " +
                                  $"| {labware.Row} " +
                                  $"| {labware.TipRack} " +
                                  $"| {labware.AlphaIndex} |");
                }
            }
            else
            {
                sb.AppendLine("No labware information could be processed from the file.");
            }

            return sb.ToString();
        }

        #region Drag and Drop Handlers

        private void MainContent_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Ensure only a single .lay file is being dragged
                if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".lay", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Copy;
                    DragDropOverlay.Visibility = Visibility.Visible;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            e.Handled = true;
        }

        private void MainContent_DragLeave(object sender, DragEventArgs e)
        {
            DragDropOverlay.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void MainContent_Drop(object sender, DragEventArgs e)
        {
            DragDropOverlay.Visibility = Visibility.Collapsed;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".lay", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessDeckLayoutFile(files[0]);
                }
            }
            e.Handled = true;
        }

        #endregion

        #region Custom Title Bar Event Handlers
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }



        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Restore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox
            {
                // Set the owner to center the dialog over the main window
                Owner = this
            };
            aboutBox.ShowDialog();
        }
        #endregion
    }
}