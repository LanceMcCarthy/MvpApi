using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class AboutPage : BasePage
    {
        public AboutPage()
        {
            InitializeComponent();
            PageViewModel = this.ViewModel;
            Loaded += AboutPage_Loaded;
        }

        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            GearsStory.RepeatBehavior = new RepeatBehavior { Type = RepeatBehaviorType.Forever};
            GearsStory.Begin();
        }
    }
}
