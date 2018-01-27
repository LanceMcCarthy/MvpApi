using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MvpApi.Common.Models;
using MvpApi.Uwp.Views;

namespace MvpApi.Uwp.ViewModels
{
    public class AddSubmissionViewModel : PageViewModelBase
    {
        private StorageFile selectedFile;
        private bool isReadEnabled;
        private bool isLoadFileEnabled;
        private bool isUploadEnabled;

        public AddSubmissionViewModel()
        {
        }

        #region Properties

        public ObservableCollection<ContributionsModel> Contributions { get; set; } = new ObservableCollection<ContributionsModel>();

        public StorageFile SelectedFile
        {
            get => selectedFile;
            set => Set(ref selectedFile, value);
        }

        public bool IsReadEnabled
        {
            get => isReadEnabled;
            set => Set(ref isReadEnabled, value);
        }

        public bool IsLoadFileEnabled
        {
            get => isLoadFileEnabled;
            set => Set(ref isLoadFileEnabled, value);
        }

        public bool IsUploadEnabled
        {
            get => isUploadEnabled;
            set => Set(ref isUploadEnabled, value);
        }

        #endregion
        
        #region Event handlers

        public async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fop = new FileOpenPicker();
            fop.FileTypeFilter.Add(".xlsx");
            fop.FileTypeFilter.Add(".xls");

            SelectedFile = await fop.PickSingleFileAsync();

            IsReadEnabled = SelectedFile != null;
        }

        public async void ReadFileButton_Click(object sender, RoutedEventArgs e)
        {
            await ReadDocumentAsync(SelectedFile);
        }

        public async void UploadContributionsButton_Click(object sender, RoutedEventArgs e)
        {
            
            await UploadContributionsAsync();
        }

        #endregion
        
        #region methods

        private async Task ReadDocumentAsync(StorageFile file)
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "reading file...";

                using (var fileStream = await file.OpenReadAsync())
                using (var doc = SpreadsheetDocument.Open(fileStream.AsStream(), false))
                {
                    var workbookPart = doc.WorkbookPart;
                    var worksheetPart = workbookPart.WorksheetParts.First();
                    var sheet = worksheetPart.Worksheet;

                    var rows = sheet.Descendants<Row>();

                    Debug.WriteLine("Row count = {0}", rows.LongCount());

                    foreach (Row row in rows)
                    {
                        // TODO Investigate if I can build a datatable instead
                        string[] columnValues = new string[4];

                        foreach (Cell cell in row.Elements<Cell>())
                        {
                            string cellValue = string.Empty;

                            // Skip if null or header cell
                            if (cell.DataType == null ||
                                cell.CellReference == "A1" ||
                                cell.CellReference == "B1" ||
                                cell.CellReference == "C1")
                                continue;

                            if (cell.DataType == CellValues.SharedString || cell.DataType == CellValues.Date || cell.DataType == CellValues.Number)
                            {
                                if (int.TryParse(cell.InnerText, out var id))
                                {
                                    var item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);

                                    if (item.Text != null)
                                    {
                                        cellValue = item.Text.Text;
                                    }
                                    else if (item.InnerText != null)
                                    {
                                        cellValue = item.InnerText;
                                    }
                                    else if (item.InnerXml != null)
                                    {
                                        cellValue = item.InnerXml;
                                    }
                                }

                                IsBusyMessage = $"reading file, parsed cell {cell.CellReference}";

                                Debug.WriteLine($"Cell {cell.CellReference}, Value = {cellValue}");

                                
                                int columnIndex = 0;

                                var cr = cell.CellReference.ToString().ToUpper();

                                for (int i = 0; i < cr.Length && cr[i] >= 'A'; i++)
                                    columnIndex = columnIndex * 26 + (cr[i] - 64);
                                
                                // Finally, add the cell value to the array
                                columnValues[columnIndex - 1] = cellValue;
                            }
                        }

                        // TODO need a safer way to do this
                        var contribution = new ContributionsModel();

                        contribution.Title = columnValues[0];

                        if (DateTime.TryParse(columnValues[1], out DateTime date))
                        {
                            contribution.StartDate = date;
                        }

                        contribution.ReferenceUrl = columnValues[2];

                        Contributions.Add(contribution);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReadDocumentAsync Exception: {ex}");
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        private async Task UploadContributionsAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "uploading...";

                // TODO add upload method to MvpApiService.cs
                var success = await Task.Run(async () =>
                {
                    // TODO add upload method to MvpApiService.cs
                    await Task.Delay(2000);

                    // task would return true or false depending on if the upload was successful
                    return true;
                });

                if (success)
                {
                    SelectedFile = null;
                    IsLoadFileEnabled = true;
                    IsReadEnabled = false;
                    IsUploadEnabled = false;

                    Contributions.Clear();
                }
                else
                {
                    // show error dialog
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UploadContributionsAsync Exception: {ex}");
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }
        
        #endregion
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {
                IsLoadFileEnabled = true;
            }
            else
            {
                await NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
