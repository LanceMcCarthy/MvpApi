using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace MvpApi.Uwp.Views
{
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            Loaded += AboutPage_Loaded            ;
        }

        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            GearsStory.RepeatBehavior = RepeatBehavior.Forever;
            GearsStory.Begin();
        }
    }
}
