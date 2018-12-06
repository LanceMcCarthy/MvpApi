using MvpApi.Common.Models;
using MvpApi.Forms.Portable.Models;
using MvpApi.Forms.Portable.ViewModels;
using System;
using Telerik.XamarinForms.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MvpApi.Forms.Portable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetailView : ContentView
	{
        public DetailView()
		{
			InitializeComponent();
        }
        
        private void DoneButton_Clicked(object sender, EventArgs e)
        {
            (BindingContext as MainPageViewModel).LoadView(ViewType.Home);
        }
    }
}