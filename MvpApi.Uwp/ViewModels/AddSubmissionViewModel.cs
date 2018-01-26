using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class AddSubmissionViewModel : ViewModelBase
    {
        private StorageFile selectedFile;
        private Visibility readFileButtonVisibility = Visibility.Collapsed;

        public AddSubmissionViewModel()
        {
        }

        public ObservableCollection<ContributionsModel> Contributions { get; set; } = new ObservableCollection<ContributionsModel>();

        public StorageFile SelectedFile
        {
            get => selectedFile;
            set => Set(ref selectedFile, value);
        }

        public Visibility ReadFileButtonVisibility
        {
            get => readFileButtonVisibility;
            set => Set(ref readFileButtonVisibility, value);
        }

        public async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fop = new FileOpenPicker();
            fop.FileTypeFilter.Add(".xlsx");
            fop.FileTypeFilter.Add(".xls");

            SelectedFile = await fop.PickSingleFileAsync();

            ReadFileButtonVisibility = SelectedFile != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public async void ReadFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ReadDocumentAsync(SelectedFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReadDocumentAsync Exception: {ex}");
            }
        }

        private async Task ReadDocumentAsync(StorageFile file)
        {
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
                    string[] columnValues = new string[4];

                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        string cellValue = string.Empty;

                        // Skip if null or header cell
                        if(cell.DataType == null ||
                           cell.CellReference == "A1" ||
                           cell.CellReference == "B1" ||
                           cell.CellReference == "C1" )
                            continue;

                        if (cell.DataType == CellValues.SharedString || cell.DataType == CellValues.Date || cell.DataType == CellValues.Number)
                        {
                            if (int.TryParse(cell.InnerText, out var id))
                            {
                                var item = GetSharedStringItemById(workbookPart, id);

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


                            Debug.WriteLine($"Cell {cell.CellReference}, Value = {cellValue}");
                            
                            // TODO Investigate if I can build a Datatable using index instead, it would be more reliable
                            columnValues[ColumnIndex(cell.CellReference)] = cellValue;
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

        private static int ColumnIndex(string cellReference)
        {
            int columnIndex = 0;

            var cr = cellReference.ToUpper();

            for (int i = 0; i < cr.Length && cr[i] >= 'A';i++ ) 
                columnIndex = columnIndex * 26 + (cr[i] - 64);

            return columnIndex - 1;
        }

        private static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, int id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {

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
