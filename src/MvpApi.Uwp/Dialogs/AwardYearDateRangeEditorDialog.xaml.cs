using MvpApi.Uwp.ViewModels;
using MvpApi.Uwp.Views;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class AwardYearDateRangeEditorDialog : ContentDialog
    {
        public AwardYearDateRangeEditorDialog()
        {
            InitializeComponent();

            StartDatePicker.Value = ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionStartDate;
            EndDatePicker.Value = ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionDeadline;
        }

        private void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(StartDatePicker.Value !=null)
            {
                ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionStartDate = StartDatePicker.Value.Value;
            }

            if (EndDatePicker.Value != null)
            {
                ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionDeadline = EndDatePicker.Value.Value;
            }

            this.Hide();
        }

        private void CancelButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
