using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class TutorialDialog : ContentDialog
    {
        public TutorialDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty MessageTitleProperty = DependencyProperty.Register(
            "MessageTitle", typeof(string), typeof(TutorialDialog), new PropertyMetadata("Message Title"));

        public string MessageTitle
        {
            get => (string) GetValue(MessageTitleProperty);
            set => SetValue(MessageTitleProperty, value);
        }
        
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(TutorialDialog), new PropertyMetadata("Message"));

        public string Message
        {
            get => (string) GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string SettingsKey { get; set; } = "DefaultSetting";

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ApplicationData.Current.LocalSettings.Values[SettingsKey] = DontShowAgainCheckBox.IsChecked;
            Hide();
        }
    }
}
