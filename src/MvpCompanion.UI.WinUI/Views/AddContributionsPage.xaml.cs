using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class AddContributionsPage : BasePage
    {
        public AddContributionsPage()
        {
            InitializeComponent();
            this.PageViewModel = this.ViewModel;

            // Workaround to VisualStates not working properly
            SizeChanged += AddContributionsPage_SizeChanged;

            AnnualReachNumericBox.Minimum = 0;
            SecondAnnualQuantityNumericBox.Minimum = 0;
            AnnualReachNumericBox.Minimum = 0;
            AnnualQuantityNumericBox.Maximum = int.MaxValue;
            SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
            AnnualReachNumericBox.Maximum = int.MaxValue;
        }

        private void AddContributionsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 641)
            {
                BottomRowDefinition.Height = new GridLength() { GridUnitType = GridUnitType.Star };
                Grid.SetColumnSpan(FormGrid, 3);

                Grid.SetRow(QueueGrid, 2);
                Grid.SetColumn(QueueGrid, 0);
                Grid.SetColumnSpan(QueueGrid, 3);

                SplitterRectangle.Visibility = Visibility.Collapsed;
            }
            else
            {
                BottomRowDefinition.Height = new GridLength(){GridUnitType = GridUnitType.Auto};

                Grid.SetColumnSpan(FormGrid, 1);

                Grid.SetRow(QueueGrid, 1);
                Grid.SetColumn(QueueGrid, 2);
                Grid.SetColumnSpan(QueueGrid, 1);

                SplitterRectangle.Visibility = Visibility.Visible;
            }
        }
    }
}
