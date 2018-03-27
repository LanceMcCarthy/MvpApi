using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Views
{
    public sealed partial class ContributionDetailPage : Page
    {
        public ContributionDetailPage()
        {
            InitializeComponent();

            AnnualReachNumericBox.Minimum = 0;
            SecondAnnualQuantityNumericBox.Minimum = 0;
            AnnualReachNumericBox.Minimum = 0;
            AnnualQuantityNumericBox.Maximum = int.MaxValue;
            SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
            AnnualReachNumericBox.Maximum = int.MaxValue;
        }
    }
}