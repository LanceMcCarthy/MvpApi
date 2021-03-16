using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class ContributionDetailPage : BasePage
    {
        public ContributionDetailPage()
        {
            InitializeComponent();
            PageViewModel = this.ViewModel;

            AnnualReachNumericBox.Minimum = 0;
            SecondAnnualQuantityNumericBox.Minimum = 0;
            AnnualReachNumericBox.Minimum = 0;
            AnnualQuantityNumericBox.Maximum = int.MaxValue;
            SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
            AnnualReachNumericBox.Maximum = int.MaxValue;
        }
    }
}