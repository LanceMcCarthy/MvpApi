using System;
using MvpApi.Common.Models;
using MvpApi.Services.Apis;
using MvpCompanion.UI.Common.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Utils;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class AwardQuestionsDialog : ContentDialog
    {
        private readonly MvpApiService apiService;

        private ObservableCollection<QuestionnaireItem> QuestionnaireItems { get; } = new ObservableCollection<QuestionnaireItem>();

        public AwardQuestionsDialog(MvpApiService service)
        {
            this.apiService = service;

            this.InitializeComponent();
            
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                QuestionnaireItems = DesignTimeHelpers.GenerateQuestionnaireItems();
            }

            ItemsListView.ItemsSource = QuestionnaireItems;

            Loaded += AwardQuestionsDialog_Loaded;
        }

        private async void AwardQuestionsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ShowProgress("getting questions...");

            // Go through the questions and populate the ListView.
            var readOnlyQuestions = await apiService.GetAwardConsiderationQuestionsAsync();
            
            foreach (var question in readOnlyQuestions)
            {
                QuestionnaireItems.Add(new QuestionnaireItem { QuestionItem = question });
            }

            // Check for saved answers an update the matching questions.

            ShowProgress("getting saved answers...");

            var savedAnswers = await apiService.GetAwardConsiderationAnswersAsync();
            
            // If there is a 400 error, this means the MVP has already submitted their answers.
            if (savedAnswers == null)
            {
                ShowProgress("You've likely already submitted your answers. If not, try again later.");

                QuestionnaireItems.ForEach(item => item.AnswerItem = new AwardAnswerViewModel());

                SubmitButton.IsEnabled = false;
                ConfirmationCheckBox.IsEnabled = false;
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                // Go through the questions and see if there is already an Answer for it.
                foreach (var questionnaireItem in QuestionnaireItems)
                {
                    var matchingAnswer = savedAnswers.FirstOrDefault(a => a.AwardQuestionId == questionnaireItem.QuestionItem.AwardQuestionId);

                    if (matchingAnswer != null)
                    {
                        // If there is a preexisting answer, set it
                        questionnaireItem.AnswerItem = matchingAnswer;
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
                var answers = new List<AwardAnswerViewModel>();

                if (answers.Count == 0)
                {
                    return;
                }

                foreach (var questionnaireItem in QuestionnaireItems)
                {
                    answers.Add(questionnaireItem.AnswerItem);
                }

                await apiService.SaveAwardConsiderationAnswerAsync(answers);
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
            if (ConfirmationCheckBox.IsChecked == true)
            {
                // Get final, explicit, permission to submit the answers because this is not undoable!!!

                var md = new MessageDialog("Once you submit the answers, you cannot change them. Are you Sure you want to submit your saved answers?", "Final Confirmation!");
                md.Commands.Add(new UICommand("SUBMIT (final)"));
                md.Commands.Add(new UICommand("CANCEL"));

                var mdResult = await md.ShowAsync();

                if (mdResult.Label == "CANCEL")
                {
                    return;
                }

                ShowProgress("permanently submitting answers...");

                var result = await apiService.SubmitAwardConsiderationAnswerAsync();
                
                if (result)
                {
                    SubmitButton.Content = "DONE";
                    SubmitButton.IsEnabled = false;
                    ConfirmationCheckBox.IsEnabled = false;

                    await new MessageDialog("You have successfully submitted your answers for the current MVP renewal year.", "Success!").ShowAsync();
                }
                else
                {
                    await new MessageDialog("The answers were not sent for final submission. If this happens again, you may need to use the MVP portal to do final submission (your answers are saved in the portal).").ShowAsync();
                }

                HideProgress();
            }
        }

        private async void ConfirmationCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            // Make sure the user has saved answers for all the questions.

            ShowProgress("Double checking all answers have been saved...");

            var questions = await apiService.GetAwardConsiderationQuestionsAsync();
            var savedAnswers = await apiService.GetAwardConsiderationAnswersAsync();

            HideProgress();

            if (savedAnswers.Count == questions.Count)
            {
                SubmitButton.IsEnabled = true;
            }
            else
            {
                await new MessageDialog("You need to save all your answers before you can submit them.").ShowAsync();
            }
        }

        private void ConfirmationCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;
        }

        private void ShowProgress(string text)
        {
            if (LoadingGrid.Visibility != Visibility.Visible)
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
    }
}
