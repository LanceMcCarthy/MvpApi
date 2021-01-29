using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CommonHelpers.Common;
using MvpApi.Common.Models;
using Template10.Utils;

namespace MvpCompanion.UI.Dialogs
{
    public sealed partial class AwardQuestionsDialog : ContentDialog
    {
        private ObservableCollection<QuestionnaireItem> Items { get; set; } = new ObservableCollection<QuestionnaireItem>();

        public AwardQuestionsDialog()
        {
            this.InitializeComponent();
            
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Items = MvpCompanion.UI.Helpers.DesignTimeHelpers.GenerateQuestionnaireItems();
            }

            ItemsListView.ItemsSource = Items;

            Loaded += AwardQuestionsDialog_Loaded;
        }

        private async void AwardQuestionsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ShowProgress("getting questions...");

            // Go through the questions and populate the ListView.

            var questions = new List<AwardConsiderationQuestionModel>(await App.ApiService.GetAwardConsiderationQuestionsAsync());

            foreach (var question in questions)
            {
                Items.Add(new QuestionnaireItem { QuestionItem = question });
            }

            // Check for saved answers an update the matching questions.

            ShowProgress("getting saved answers...");

            var savedAnswers = await App.ApiService.GetAwardConsiderationAnswersAsync();

            var answers = new List<AwardConsiderationAnswerModel>();
            
            // If there is a 400 error, this means the MVP has already submitted their answers.
            if (savedAnswers == null)
            {
                ShowProgress("You've likely already submitted your answers. If not, try again later.");

                Items.ForEach(item=>item.AnswerItem = new AwardConsiderationAnswerModel());

                SubmitButton.IsEnabled = false;
                ConfirmationCheckBox.IsEnabled = false;
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                // Go through the questions and see if there is already an Answer for it.
                foreach (var item in Items)
                {
                    var matchingAnswer = answers.FirstOrDefault(a => a.AwardQuestionId == item.QuestionItem.AwardQuestionId);

                    if (matchingAnswer != null)
                    {
                        // If there is a preexisting answer, set it
                        item.AnswerItem = matchingAnswer;
                    }
                }
            }
            
            HideProgress();
        }

        private async void SaveAnswersButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                var answers = new List<AwardConsiderationAnswerModel>();

                if (answers.Count == 0)
                    return;

                foreach (var item in Items)
                {
                    answers.Add(item.AnswerItem);
                }

                await App.ApiService.SaveAwardConsiderationAnswerAsync(answers);
            }
            finally
            {
                deferral.Complete();

            }

            this.Hide();
        }

        private void CancelButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void SubmitButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Need to get double confirmation from user, this is not undoable!!!
            if (ConfirmationCheckBox.IsChecked == true)
            {
                ShowProgress("submitting answers...");

                var result = true; //await App.ApiService.SubmitAwardConsiderationAnswerAsync();
                
                if (result)
                {
                    SubmitButton.Content = "success";
                    SubmitButton.IsEnabled = false;
                    ConfirmationCheckBox.IsEnabled = false;
                }

                HideProgress();
            }
        }

        private void ShowProgress(string text)
        {
            if(LoadingGrid.Visibility != Visibility.Visible)
            {
                LoadingGrid.Visibility = Visibility.Visible;
            }

            if (!StatusProgressBar.IsIndeterminate)
            {
                StatusProgressBar.IsIndeterminate = true;
            }

            StatusTextBlock.Text = text;
        }

        private void HideProgress()
        {
            if (LoadingGrid.Visibility != Visibility.Collapsed)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
            }

            if (StatusProgressBar.IsIndeterminate)
            {
                StatusProgressBar.IsIndeterminate = false;
            }

            StatusTextBlock.Text = string.Empty;
        }

        private void ConfirmationCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = true;
        }

        private void ConfirmationCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;
        }
    }
}
