using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class ProfilePage : BasePage
    {
        public ProfilePage()
        {
            InitializeComponent();
            PageViewModel = this.ViewModel;
        }
    }
}
