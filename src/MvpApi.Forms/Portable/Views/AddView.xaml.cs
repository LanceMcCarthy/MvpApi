using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MvpApi.Forms.Portable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddView : ContentView
    {
        public AddView()
        {
            InitializeComponent();
        }
    }
}