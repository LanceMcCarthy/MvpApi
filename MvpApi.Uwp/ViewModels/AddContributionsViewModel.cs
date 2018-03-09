﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MvpApi.Common.Models;
using MvpApi.Uwp.Dialogs;
using MvpApi.Uwp.Extensions;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;
using Template10.Common;
using Template10.Mvvm;
using Template10.Utils;

namespace MvpApi.Uwp.ViewModels
{
    public class AddContributionsViewModel : PageViewModelBase
    {
        #region Fields
        
        private ContributionsModel selectedContribution;
        private string urlHeader = "Url";
        private string annualQuantityHeader = "Annual Quantity";
        private string secondAnnualQuantityHeader = "Second Annual Quantity";
        private string annualReachHeader = "Annual Reach";
        private bool isUrlRequired;
        private bool isAnnualQuantityRequired;
        private bool isSecondAnnualQuantityRequired;
        private bool canUpload = true;

        #endregion

        public AddContributionsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Types = DesignTimeHelpers.GenerateContributionTypes();
                //CategoryAreas = DesignTimeHelpers.GenerateTechnologyAreas(); //Causing designer layout error
                Visibilies = DesignTimeHelpers.GenerateVisibilities();

                UploadQueue = DesignTimeHelpers.GenerateContributions();
                SelectedContribution = UploadQueue.FirstOrDefault();

                return;
            }

            EditCommand = new DelegateCommand<ContributionsModel>(async cont => await EditContribution(cont));
            RemoveCommand = new DelegateCommand<ContributionsModel>(async cont => await RemoveContribution(cont));

            UploadQueue.CollectionChanged += UploadQueue_CollectionChanged;
        }
        
        // *** Properties *** //

        // Collections and SelectedItems

        public ObservableCollection<ContributionsModel> UploadQueue { get; } = new ObservableCollection<ContributionsModel>();

        public ObservableCollection<ContributionTypeModel> Types { get; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<VisibilityViewModel> Visibilies { get; } = new ObservableCollection<VisibilityViewModel>();
        
        public ObservableCollection<ContributionAreaContributionModel> CategoryAreas { get; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ContributionsModel SelectedContribution
        {
            get => selectedContribution;
            set => Set(ref selectedContribution, value);
        }
        
        // Form Headers
        
        public string AnnualQuantityHeader
        {
            get => annualQuantityHeader;
            set => Set(ref annualQuantityHeader, value);
        }

        public string SecondAnnualQuantityHeader
        {
            get => secondAnnualQuantityHeader;
            set => Set(ref secondAnnualQuantityHeader, value);
        }

        public string AnnualReachHeader
        {
            get => annualReachHeader;
            set => Set(ref annualReachHeader, value);
        }

        public string UrlHeader
        {
            get => urlHeader;
            set => Set(ref urlHeader, value);
        }

        // Validation properties

        public bool IsUrlRequired
        {
            get => isUrlRequired;
            set => Set(ref isUrlRequired, value);
        }

        public bool IsAnnualQuantityRequired
        {
            get => isAnnualQuantityRequired;
            set => Set(ref isAnnualQuantityRequired, value);
        }

        public bool IsSecondAnnualQuantityRequired
        {
            get => isSecondAnnualQuantityRequired;
            set => Set(ref isSecondAnnualQuantityRequired, value);
        }
        
        public bool CanUpload
        {
            get => canUpload;
            set => Set(ref canUpload, value);
        }
        
        // Commands

        public DelegateCommand<ContributionsModel> EditCommand { get; set; }

        public DelegateCommand<ContributionsModel> RemoveCommand { get; set; }
        
        #region Event handlers

        // Collection and Selection Changed handlers

        private void UploadQueue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CanUpload = UploadQueue.Any();
        }
        
        public async void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate < new DateTime(2016, 10, 1) || e.NewDate > new DateTime(2018, 3, 31))
            {
                await new MessageDialog("The contribution date must be after the start of your current award period and before March 31, 2018 in order for it to count towards your evaluation", "Notice: Out of range").ShowAsync();
            }
        }
        
        public void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is ContributionTypeModel type)
            {
                // There are complex rules around the names of the properties, this method determines the requirements and updates the UI accordingly
                DetermineContributionTypeRequirements(type);

                // Also need set the type name
                SelectedContribution.ContributionTypeName = type.EnglishName;
            }
        }
        
        // Button click handlers

        public async void AddCurrentItemButton_Click(object sender, RoutedEventArgs e)
        {
            var isValid = await SelectedContribution.Validate(true);

            if (!isValid)
                return;

            // Validate all required fields
            var isValidated = await ValidateRequiredFields();

            if (isValidated)
            {
                SelectedContribution.ContributionId = null;

                if(!UploadQueue.Contains(SelectedContribution))
                    UploadQueue.Add(SelectedContribution);

                SetupNextEntry();
            }
            else
            {
                await new MessageDialog("You need to fill in every field marked as 'Required' before starting a new contribution.").ShowAsync();
            }
        }

        public void ClearCurrentItemButton_Click(object sender, RoutedEventArgs e)
        {
            SetupNextEntry();
        }

        public async void ClearQueueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var md = new MessageDialog("You are about to clear all of the contributions in the upload queue, are you sure?", "CLEAR");
                md.Commands.Add(new UICommand("YES"));
                md.Commands.Add(new UICommand("whoa, no!"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label != "YES")
                    return;

                UploadQueue.Clear();

                SetupNextEntry();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong clearing the queue, please try again. Error: {ex.Message}").ShowAsync();
            }
        }

        public async void UploadQueue_Click(object sender, RoutedEventArgs e)
        {
            foreach (var contribution in UploadQueue)
            {
                contribution.UploadStatus = UploadStatus.InProgress;

                var success = await UploadContributionAsync(contribution);
            
                contribution.UploadStatus = success
                    ? UploadStatus.Success
                    : UploadStatus.Failed;
            }

            UploadQueue.Remove(c => c.UploadStatus == UploadStatus.Success);
            
            if (UploadQueue.Any())
            {
                await new MessageDialog("Not all contributions were saved, view the queue for remaining items and try again", "Incomplete Upload").ShowAsync();

                SelectedContribution = UploadQueue.LastOrDefault();
            }
            else
            {
                await new MessageDialog("All contributions have been saved!").ShowAsync();

                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();
            }
        }
        
        #endregion

        #region Methods

        private void SetupNextEntry()
        {
            // Set up first contribution, ID is 0 so that we dont accidentally overwrite the data in the form if another contribution is selected for editing
            // this ContributionId will be nulled out before uploading
            SelectedContribution = new ContributionsModel
            {
                ContributionId = 0,
                StartDate = DateTime.Now,
                Visibility = Visibilies.FirstOrDefault(),
                ContributionType = Types.FirstOrDefault(),
                ContributionTypeName = SelectedContribution?.ContributionType?.Name,
                ContributionTechnology = CategoryAreas?.FirstOrDefault()?.ContributionAreas.FirstOrDefault()
            };
        }

        public async Task EditContribution(ContributionsModel contribution)
        {
            if (contribution == null)
                return;
            
            if (contribution.ContributionId == 0)
            {
                var md = new MessageDialog("Editing this contribution will replace the unsaved information you have in the form. If you do not want to lose that information, click 'cancel' and add it to the queue before editing this one.", "Warning");
                md.Commands.Add(new UICommand("edit"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label == "cancel")
                    return;
            }

            SelectedContribution = contribution;
        }

        public async Task RemoveContribution(ContributionsModel contribution)
        {
            if (contribution == null)
                return;

            try
            {
                var md = new MessageDialog("Are you sure you want to remove this contribution from the queue?", "Remove Contribution?");
                md.Commands.Add(new UICommand("yes"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label == "yes")
                {
                    if (SelectedContribution == contribution)
                    {
                        SelectedContribution = null;
                    }

                    UploadQueue.Remove(contribution);
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong deleting this item, please try again. Error: {ex.Message}").ShowAsync();
            }
        }
        
        public async Task<bool> UploadContributionAsync(ContributionsModel contribution)
        {
            try
            {
                var submissionResult = await App.ApiService.SubmitContributionAsync(contribution);

                // copying back the ID which was created on the server once the item was added to the database
                contribution.ContributionId = submissionResult.ContributionId;

                return true;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong saving the item, please try again. Error: {ex.Message}").ShowAsync();
                return false;
            }
        }
        
        private async Task<bool> ValidateRequiredFields()
        {
            if (SelectedContribution.ContributionType == null)
            {
                await new MessageDialog("The Contribution Type is a required field").ShowAsync();
                return false;
            }

            if (SelectedContribution.Visibility == null)
            {
                await new MessageDialog("The Visibility is a required field").ShowAsync();
                return false;
            }

            if (SelectedContribution.ContributionTechnology == null)
            {
                await new MessageDialog("The Category Technology is a required field").ShowAsync();
                return false;
            }

            if (IsUrlRequired && string.IsNullOrEmpty(SelectedContribution.ReferenceUrl))
            {
                await new MessageDialog($"The {UrlHeader} field is required when entering a {SelectedContribution.ContributionType.EnglishName} Activity Type",
                    $"Missing {UrlHeader}!").ShowAsync();

                return false;
            }

            if (IsAnnualQuantityRequired && SelectedContribution.AnnualQuantity == null)
            {
                await new MessageDialog($"The {AnnualQuantityHeader} field is required when entering a {SelectedContribution.ContributionType.EnglishName} Activity Type",
                    $"Missing {AnnualQuantityHeader}!").ShowAsync();

                return false;
            }

            if (IsSecondAnnualQuantityRequired && SelectedContribution.SecondAnnualQuantity == null)
            {
                await new MessageDialog($"The {SecondAnnualQuantityHeader} field is required when entering a {SelectedContribution.ContributionType.EnglishName} Activity Type",
                    $"Missing {SecondAnnualQuantityHeader}!").ShowAsync();

                return false;
            }

            return true;
        }
        
        private async Task LoadSupportingDataAsync()
        {
            IsBusyMessage = "loading types...";

            var types = await App.ApiService.GetContributionTypesAsync();

            types.ForEach(type =>
            {
                Types.Add(type);
            });
            
            IsBusyMessage = "loading technologies...";

            var areaRoots = await App.ApiService.GetContributionAreasAsync();

            // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
            var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);

            areas.ForEach(area =>
            {
                CategoryAreas.Add(area);
            });


            IsBusyMessage = "loading visibility options...";

            var visibilities = await App.ApiService.GetVisibilitiesAsync();

            visibilities.ForEach(visibility =>
            {
                Visibilies.Add(visibility);
            });
        }

        #endregion
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {
                try
                {
                    IsBusy = true;

                    // ** Get the neccessary associated data from the API **

                    await LoadSupportingDataAsync();
                    
                    SetupNextEntry();

                    if (!(ApplicationData.Current.LocalSettings.Values["AddContributionPageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                    {
                        var td = new TutorialDialog
                        {
                            SettingsKey = "AddContributionPageTutorialShown",
                            MessageTitle = "Add Contribution Page",
                            Message = "This page allows you to add contributions to your MVP profile.\r\n\n" +
                                      "- Complete the form and the click the 'Add' button to add the completed contribution to the upload queue.\r\n" +
                                      "- You can edit or remove items that are already in the queue using the item's 'Edit' or 'Remove' buttons.\r\n" +
                                      "- Click 'Upload' to save the queue to your profile.\r\n" +
                                      "- You can clear the form, or the entire queue, using the 'Clear' buttons.\r\n\n" +
                                      "Note: Watch the queue items color change as the items are uploaded and save is confirmed."
                        };

                        await td.ShowAsync();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LoadDataAsync Exception {ex}");
                }
                finally
                {
                    IsBusyMessage = "";
                    IsBusy = false;
                }
            }
            else
            {
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion

        public void DetermineContributionTypeRequirements(ContributionTypeModel contributionType)
        {
            switch (contributionType.EnglishName)
            {
                case "Article":
                    AnnualQuantityHeader = "Number of Articles";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Views";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Blog Site Posts":
                    AnnualQuantityHeader = "Number of Posts";
                    SecondAnnualQuantityHeader = "Number of Subscribers";
                    AnnualReachHeader = "Annual Unique Visitors";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Book (Author)":
                    AnnualQuantityHeader = "Number of Books";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Copies Sold";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Book (Co-Author)":
                    AnnualQuantityHeader = "Number of Books";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Copies Sold";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Code Project/Tools":
                    AnnualQuantityHeader = "Number of Projects";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Downloads";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Code Samples":
                    AnnualQuantityHeader = "Number of Samples";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Downloads";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Conference (booth presenter)":
                    AnnualQuantityHeader = "Number of Conferences";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Visitors";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Conference (organizer)":
                    AnnualQuantityHeader = "Number of Conferences";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Visitors";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Forum Moderator":
                    AnnualQuantityHeader = "Number of Threads Moderated";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Forum Participation (3rd Party Forums)":
                    AnnualQuantityHeader = "Number of Answers";
                    SecondAnnualQuantityHeader = "Number of Posts";
                    AnnualReachHeader = "Views of Answers";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = false;
                    IsSecondAnnualQuantityRequired = true;
                    break;
                case "Forum Participation (Microsoft Forums)":
                    AnnualQuantityHeader = "Number of Answers";
                    SecondAnnualQuantityHeader = "Number of Posts";
                    AnnualReachHeader = "Views of Answers";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Mentorship":
                    AnnualQuantityHeader = "Number of Mentees";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Open Source Project(s)":
                    AnnualQuantityHeader = "Project(s)";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Commits";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Other":
                    AnnualQuantityHeader = "Annual Quantity";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Product Group Feedback":
                    AnnualQuantityHeader = "Number of Events provided";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Feedbacks provided";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Site Owner":
                    AnnualQuantityHeader = "Posts";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Visitors";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (Conference)":
                    AnnualQuantityHeader = "Talks";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Attendees of talks";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (Local)":
                    AnnualQuantityHeader = "Talks";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Attendees of talks";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (User group)":
                    AnnualQuantityHeader = "Talks";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Attendees of talks";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
                    AnnualQuantityHeader = "Number of Posts";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Followers";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Translation Review, Feedback and Editing":
                    AnnualQuantityHeader = "Annual Quantity";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "User Group Owner":
                    AnnualQuantityHeader = "Meetings";
                    SecondAnnualQuantityHeader = "Members";
                    AnnualReachHeader = "";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Video":
                    AnnualQuantityHeader = "Number of Videos";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Views";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Webcast":
                    AnnualQuantityHeader = "Number of Videos";
                    SecondAnnualQuantityHeader = "Number of Views";
                    AnnualReachHeader = "";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Website Posts":
                    AnnualQuantityHeader = "Number of Posts";
                    SecondAnnualQuantityHeader = "Number of Subscribers";
                    AnnualReachHeader = "Annual Unique Visitors";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                default: // Fall back on 'other'
                    AnnualQuantityHeader = "Annual Quantity";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
            }
        }

        // Not done yet
        #region Excel document logic (to be added)

        public async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fop = new FileOpenPicker();
            fop.FileTypeFilter.Add(".xlsx");
            fop.FileTypeFilter.Add(".xls");

            var selectedFile = await fop.PickSingleFileAsync();

            await ReadDocumentAsync(selectedFile);
        }

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

                        UploadQueue.Add(contribution);
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

        #endregion
    }
}