using MvpApi.Common.Models;
using MvpApi.Forms.Portable.Models;
using MvpApi.Forms.Portable.ViewModels;
using System.Linq;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable.Views
{
    public partial class HomeView : ContentView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void RadDataGrid_SelectionChanged(object sender, Telerik.XamarinForms.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || !e.AddedItems.Any())
            {
                return;
            }

            if (e.AddedItems.FirstOrDefault() is ContributionsModel contribution)
            {
                (BindingContext as MainPageViewModel).SelectedContribution = contribution;
                (BindingContext as MainPageViewModel).LoadView(ViewType.Home);
            }
        }
    }
}