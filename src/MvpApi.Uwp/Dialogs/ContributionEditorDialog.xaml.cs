using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Uwp.Views;
using MvpCompanion.UI.Common.Extensions;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class ContributionEditorDialog : ContentDialog
    {
        public static readonly DependencyProperty ContributionResultProperty = DependencyProperty.Register(
            "ContributionResult",
            typeof(ContributionsModel),
            typeof(ContributionEditorDialog),
            new PropertyMetadata(default(ContributionsModel)));
        
        public ContributionsModel ContributionResult
        {
            get => (ContributionsModel)GetValue(ContributionResultProperty);
            set => SetValue(ContributionResultProperty, value);
        }
        
        public ContributionEditorDialog()
        {
            InitializeComponent();

            AnnualReachNumericBox.Minimum = 0;
            SecondAnnualQuantityNumericBox.Minimum = 0;
            AnnualReachNumericBox.Minimum = 0;
            AnnualQuantityNumericBox.Maximum = int.MaxValue;
            SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
            AnnualReachNumericBox.Maximum = int.MaxValue;

            Loaded += ContributionEditorDialog_Loaded;
            Unloaded += ContributionEditorDialog_Unloaded;
        }

        public ContributionEditorDialog(ContributionsModel originalContribution, bool cloneContribution)
        {
            InitializeComponent();
            
            ViewModel.SelectedContribution = originalContribution;
            
            if (cloneContribution)
            {
                // Strip the ID to prevent accidentally overwriting an existing contribution
                ViewModel.SelectedContribution.ContributionId = null;

                ViewModel.EditingExistingContribution = false;
            }
            else
            {
                ViewModel.EditingExistingContribution = true;
            }
            
            AnnualReachNumericBox.Minimum = 0;
            SecondAnnualQuantityNumericBox.Minimum = 0;
            AnnualReachNumericBox.Minimum = 0;
            AnnualQuantityNumericBox.Maximum = int.MaxValue;
            SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
            AnnualReachNumericBox.Maximum = int.MaxValue;

            Loaded += ContributionEditorDialog_Loaded;
            Unloaded += ContributionEditorDialog_Unloaded;
        }

        private void ContributionEditorDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnDialogClosingAsync();
        }

        private async void ContributionEditorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await ViewModel.OnDialogLoadedAsync();
            }
            else
            {
                ViewModel.WarningMessage = "No Internet Available";
            }
        }
        
        private async void SaveButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            var saveSucceeded = false;

            try
            {
                ContributionResult = ViewModel.SelectedContribution;

                var isValid = await ContributionResult.Validate(true);

                if (isValid)
                {
                    ViewModel.SelectedContribution.UploadStatus = UploadStatus.InProgress;

                    if (ViewModel.EditingExistingContribution)
                    {
                        // Update an existing contribution
                        var contributionUpdated = await App.ApiService.UpdateContributionAsync(ViewModel.SelectedContribution);

                        if (contributionUpdated == true)
                        {
                            saveSucceeded = true;
                            ContributionResult = ViewModel.SelectedContribution;
                        }
                    }
                    else
                    {
                        // Submit a new contribution
                        var newSubmissionResult = await App.ApiService.SubmitContributionAsync(ViewModel.SelectedContribution);

                        if (newSubmissionResult != null)
                        {
                            // IMPORTANT = Uploading a new contribution returns the saved submission, which comes with an ID
                            ContributionResult = newSubmissionResult;

                            // copying back the ID which was created on the server once the item was added to the database
                            ViewModel.SelectedContribution.ContributionId = newSubmissionResult.ContributionId;

                            saveSucceeded = true;
                        }
                    }

                    // Show the result in the UI-bound object
                    ViewModel.SelectedContribution.UploadStatus = saveSucceeded ? UploadStatus.Success : UploadStatus.Failed;

                    if (saveSucceeded)
                    {
                        Hide();
                    }
                    else
                    {
                        args.Cancel = true;
                    }
                }
                else
                {
                    // prevent the closing of the dialog
                    args.Cancel = true;

                    await new MessageDialog("Check for errors or missing data and try again.", "Invalid Contribution").ShowAsync();
                }
            }
            catch(Exception ex)
            {
                // prevent the closing of the dialog
                args.Cancel = true;
                
                await new MessageDialog("Something went wrong saving the contribution.", "Error").ShowAsync();
                
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void CancelButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ContributionResult = null;

            Hide();

            //var match = ViewModel.SelectedContribution.Compare(ContributionResult);

            //if (match)
            //{
            //    ContributionResult = null;
            //    Hide();
            //}
            //else
            //{
            //    //var md = new MessageDialog("Navigating away now will lose your pending uploads, continue?", "Warning: Pending Uploads");
            //    //md.Commands.Add(new UICommand("yes"));
            //    //md.Commands.Add(new UICommand("no"));
            //    //md.CancelCommandIndex = 1;
            //    //md.DefaultCommandIndex = 1;

            //    //var result = await md.ShowAsync();

            //    //if (result.Label == "yes")
            //    //{
            //    //    CurrentContribution = null;
            //    //    Hide();
            //    //}
            //    //else
            //    //{
            //    //    args.Cancel = true;
            //    //}
            //}
        }
        
    }
}
