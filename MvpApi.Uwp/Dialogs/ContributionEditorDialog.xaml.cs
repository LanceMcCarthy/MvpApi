using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Uwp.Extensions;
using MvpApi.Uwp.Helpers;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class ContributionEditorDialog : ContentDialog
    {
        public static readonly DependencyProperty CurrentContributionProperty = DependencyProperty.Register(
            "CurrentContribution",
            typeof(ContributionsModel),
            typeof(ContributionEditorDialog),
            new PropertyMetadata(default(ContributionsModel), OnCurrentContributionChanged));

        private static void OnCurrentContributionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContributionEditorDialog self)
            {
                self.ViewModel.SelectedContribution = e.NewValue as ContributionsModel;
            }
        }

        public ContributionsModel CurrentContribution
        {
            get => (ContributionsModel)GetValue(CurrentContributionProperty);
            set => SetValue(CurrentContributionProperty, value);
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

            try
            {
                CurrentContribution = ViewModel.SelectedContribution;

                var isValid = await CurrentContribution.Validate(true);

                if (isValid)
                {
                    Hide();
                }
                else
                {
                    args.Cancel = true;
                }

            }
            catch
            {
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void CancelButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var match = ViewModel.SelectedContribution.Compare(CurrentContribution);

            if (match)
            {
                CurrentContribution = null;
                Hide();
            }
            else
            {
                var md = new MessageDialog("Navigating away now will lose your pending uploads, continue?", "Warning: Pending Uploads");
                md.Commands.Add(new UICommand("yes"));
                md.Commands.Add(new UICommand("no"));
                md.CancelCommandIndex = 1;
                md.DefaultCommandIndex = 1;

                var result = await md.ShowAsync();

                if (result.Label == "yes")
                {
                    CurrentContribution = null;
                    Hide();
                }
                else
                {
                    args.Cancel = true;
                }
            }
        }
    }
}
