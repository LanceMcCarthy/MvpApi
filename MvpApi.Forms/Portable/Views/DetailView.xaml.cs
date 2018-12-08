using MvpApi.Forms.Portable.ViewModels;
using System;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable.Views
{
    public partial class DetailView : ContentView
	{
        public DetailView()
		{
			InitializeComponent();
        }
        
        private void DoneButton_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MainPageViewModel vm)
            {
                vm.SelectedContribution = null;
            }
        }
    }
}