using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class ShellView : UserControl
    {
        public static ShellView Instance { get; set; }

        public ShellView()
        {
            Instance = this;
            this.InitializeComponent();
        }

        public async Task SignInAsync()
        {

        }
    }
}
