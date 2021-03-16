using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class KudosPage : BasePage
    {
        public KudosPage()
        {
            InitializeComponent();
            PageViewModel = this.ViewModel;
        }
    }
}
