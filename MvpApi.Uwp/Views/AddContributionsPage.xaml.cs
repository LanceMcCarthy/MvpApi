using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Views
{
    public sealed partial class AddContributionsPage : Page
    {
        public AddContributionsPage()
        {
            InitializeComponent();

            // Workaround to VisualStates not working properly
            SizeChanged += AddContributionsPage_SizeChanged;

            AnnualReachNumericBox.Minimum = 0;
            SecondAnnualQuantityNumericBox.Minimum = 0;
            AnnualReachNumericBox.Minimum = 0;
            AnnualQuantityNumericBox.Maximum = int.MaxValue;
            SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
            AnnualReachNumericBox.Maximum = int.MaxValue;
        }

        private void AddContributionsPage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 641)
            {
                BottomRowDefinition.Height = new GridLength(1, GridUnitType.Star);
                Grid.SetColumnSpan(FormGrid, 3);

                Grid.SetRow(QueueGrid, 2);
                Grid.SetColumn(QueueGrid, 0);
                Grid.SetColumnSpan(QueueGrid, 3);

                SplitterRectangle.Visibility = Visibility.Collapsed;
            }
            else
            {
                BottomRowDefinition.Height = new GridLength(1, GridUnitType.Auto);

                Grid.SetColumnSpan(FormGrid, 1);

                Grid.SetRow(QueueGrid, 1);
                Grid.SetColumn(QueueGrid, 2);
                Grid.SetColumnSpan(QueueGrid, 1);

                SplitterRectangle.Visibility = Visibility.Visible;
            }
        }
    }
}
