using Windows.UI.Xaml.Controls;
using Template10.Controls;
using Template10.Services.NavigationService;

namespace MvpApi.Uwp.Views
{
    public sealed partial class ShellPage : Page
    {
        public static ShellPage Instance { get; set; }

        public static HamburgerMenu HamburgerMenu => Instance.Menu;

        public ShellPage()
        {
            Instance = this;
            InitializeComponent();
        }

        public ShellPage(INavigationService navigationService) : this()
        {
            this.InitializeComponent();
            Menu.NavigationService = navigationService;
        }
    }
}
