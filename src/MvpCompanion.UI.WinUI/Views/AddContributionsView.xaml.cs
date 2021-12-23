using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class AddContributionsView : UserControl
{
    public AddContributionsView()
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

        Loaded += AddContributionsView_Loaded;
        Unloaded += AddContributionsView_Unloaded;
    }

    private void AddContributionsPage_SizeChanged(object sender, SizeChangedEventArgs e)
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


    private void AddContributionsView_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnLoaded();
    }

    private void AddContributionsView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }
}